using System.Diagnostics;
using System.Text;

namespace CSharpJson;


public class JsonParseError : Exception
{
  public JsonParseError() { }

  public JsonParseError(string message) : base(message) { }

  public JsonParseError(string message, Exception innerException) : base(message, innerException) { }
}


internal class Parser
{
  ByteIterator iter;
  int depth = 0;

  static readonly byte[] True = Encoding.UTF8.GetBytes("true");
  static readonly byte[] False = Encoding.UTF8.GetBytes("false");
  static readonly byte[] Null = Encoding.UTF8.GetBytes("null");

  public Parser(ByteIterator itr)
  {
    iter = itr;
  }

  public IJsonValue ParseNext()
  {

    depth++;
    if (depth >= 350)
      throw new JsonParseError("Too much recursion");
    var ret = ParseNextInner();
    depth--;
    return ret;
  }

  IJsonValue ParseNextInner()
  {
    IgnoreWhitespace();
    byte? c = iter[0];
    if (c.HasValue)
    {
      byte d = c.Value;
      if (IsString(d))
        return new JsonString(ReadString());
      if (IsNumber(d))
        return new JsonNumber(ReadNumber());
      if (IsArray(d))
      {
        List<IJsonValue> l = new();
        iter.Index++;
        while (iter[0] != (byte)']')
        {
          IgnoreWhitespace();
          l.Add(ParseNext());
          IgnoreWhitespace();
          switch (iter[0])
          {
            case (byte)',':
              iter.Index++;
              continue;
            case (byte)']':
              break;
            default:
              throw new JsonParseError($"Unexpected character `{(char?)iter[0]}`. Expected `,` or end of array `]`");
          }
        }
        iter.Index++; // To skip closing bracket
        return new JsonArray(l);
      }
      if (IsObject(d))
      {
        var dict = new Dictionary<string, IJsonValue>();
        iter.Index++;
        while (iter[0] != (byte)'}')
        {
          IgnoreWhitespace();
          string k = ReadString();
          IgnoreWhitespace();
          if (iter.Next() != (byte)':')
            throw new JsonParseError("Expected colon after object key");
          // IgnoreWhitespace called at beginning of ParseNext
          var v = ParseNext();
          IgnoreWhitespace();
          dict.Add(k, v);
          switch (iter[0])
          {
            case (byte)',':
              iter.Index++;
              continue;
            case (byte)'}':
              break;
            default:
              throw new JsonParseError($"Unexpected character `{(char?)iter[0]}`. Expected `,` or end of object `}}`");
          }
        }
        iter.Index++; // To skip closing brace
        return new JsonObject(dict);
      }
      var next4 = iter[0..4] ?? throw new JsonParseError("Unexpected end of input");
      if (next4.SequenceEqual(True))
      {
        iter.Index += 4;
        return new JsonBoolean(true);
      }
      if (next4.SequenceEqual(Null))
      {
        iter.Index += 4;
        return new JsonNull();
      }
      var next5 = iter[0..5] ?? throw new JsonParseError("Unexpected end of input");
      if (next5.SequenceEqual(False))
      {
        iter.Index += 5;
        return new JsonBoolean(false);
      }
      throw new JsonParseError($"Expected next value, found `{Encoding.UTF8.GetString(iter[0..]!)}`");

    }
    return new JsonNull();
  }

  static bool IsString(byte c)
  {
    return c == '"';
  }
  static bool IsObject(byte c)
  {
    return c == '{';
  }

  static bool IsArray(byte c)
  {
    return c == '[';
  }

  static bool IsNumber(byte c)
  {
    return (c >= '0' && c <= '9') || c == '-';
  }

  static bool IsWhitespace(byte b)
  {
    return b == 0x0020 || b == 0x000A || b == 0x000D || b == 0x0009;
  }

  public void IgnoreWhitespace()
  {
    while (iter[0].HasValue && IsWhitespace(iter[0]!.Value))
    {
      iter.Index++;
    }
  }

  string ReadString()
  {
    if (iter.Next() != (byte)'"')
      throw new JsonParseError("Expected string start");
    MemoryStream s = new();
    while (iter[0] is not null)
    {
      switch (iter[0])
      {
        case (byte)'"':
          iter.Index++;
          return Encoding.UTF8.GetString(s.ToArray());
        case (byte)'\\':
          iter.Index++;
          s.Write(Escape());
          break;
        default:
          s.WriteByte(iter.Next()!.Value);
          break;
      }
    }
    throw new JsonParseError("Unexpected end of input. Expected end of string.");
  }

  byte[] Escape()
  {
    MemoryStream s = new(4);
    var escape = iter.Next();
    if (!escape.HasValue)
      throw new JsonParseError("Unexpected end of input. Expected escape code");
    switch (escape.Value)
    {
      case (byte)'"':
      case (byte)'\\':
      case (byte)'\'':
        s.WriteByte(escape.Value); break;
      case (byte)'b': s.WriteByte(0x08); break;
      case (byte)'f': s.WriteByte(0x0C); break;
      case (byte)'n': s.WriteByte(0x0A); break;
      case (byte)'r': s.WriteByte(0x0D); break;
      case (byte)'t': s.WriteByte(0x09); break;
      case (byte)'u':
        var code = iter.NextRange(4) ?? throw new JsonParseError("Unexpected end of input. Expected valid unicode escape");
        char[] cs = new char[] { (char)Convert.ToUInt32(Encoding.UTF8.GetString(code), 16) };
        if (iter[0] == (byte)'\\' && iter[1] == (byte)'u')
        {
          iter.Index += 2;
          var secondCode = iter.NextRange(4) ?? throw new JsonParseError("Unexpected end of input. Expected valid unicode escape");
          cs = new char[] { cs[0], (char)Convert.ToUInt32(Encoding.UTF8.GetString(secondCode), 16) };
        }
        s.Write(Encoding.UTF8.GetBytes(cs));
        break;
      default:
        throw new JsonParseError($"Expected valid escape character, found `{(char)escape.Value}`");
    }
    return s.ToArray();
  }

  double ReadNumber()
  {
    var s = new MemoryStream(8);
    try
    {
      byte? i = iter[0];
      while (i is not null and
      ((>= (byte)'0' and <= (byte)'9')
        or (byte)'-'
        or (byte)'+'
        or (byte)'.'
        or (byte)'E'
        or (byte)'e'))
      {
        s.WriteByte(i.Value);
        iter.Index++;
        i = iter[0];
      }
      return Convert.ToDouble(Encoding.UTF8.GetString(s.ToArray()));
    }
    catch
    {
      throw new JsonParseError($"Expected number. Found `{Encoding.UTF8.GetString(s.ToArray())}`");
    }
  }
}