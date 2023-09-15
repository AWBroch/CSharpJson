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
  MemoryStream mem;
  int depth = 0;

  static readonly byte[] True = Encoding.UTF8.GetBytes("true");
  static readonly byte[] False = Encoding.UTF8.GetBytes("false");
  static readonly byte[] Null = Encoding.UTF8.GetBytes("null");

  public Parser(byte[] bytes)
  {
    mem = new MemoryStream(bytes);
  }
  public Parser(MemoryStream memStream)
  {
    mem = memStream;
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
    byte? c = PeekByte();
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
        Ignore();
        while (PeekByte() != (byte)']')
        {
          IgnoreWhitespace();
          l.Add(ParseNext());
          IgnoreWhitespace();
          byte? next = PeekByte();
          switch (next)
          {
            case (byte)',':
              Ignore();
              continue;
            case (byte)']':
              break;
            default:
              throw new JsonParseError($"Unexpected character `{(char?)next}`. Expected `,` or end of array `]`");
          }
        }
        Ignore(); // To skip closing bracket
        return new JsonArray(l);
      }
      if (IsObject(d))
      {
        var dict = new Dictionary<string, IJsonValue>();
        Ignore();
        while (PeekByte() != (byte)'}')
        {
          IgnoreWhitespace();
          string k = ReadString();
          IgnoreWhitespace();
          byte? n;
          if ((n = NextByte()) != (byte)':')
            throw new JsonParseError($"Expected colon after object key, found `{(char?)n}`");
          // IgnoreWhitespace called at beginning of ParseNext
          var v = ParseNext();
          IgnoreWhitespace();
          dict.Add(k, v);
          switch (n = PeekByte())
          {
            case (byte)',':
              Ignore();
              continue;
            case (byte)'}':
              break;
            default:
              throw new JsonParseError($"Unexpected character `{(char?)n}`. Expected `,` or end of object `}}`");
          }
        }
        Ignore(); // To skip closing brace
        return new JsonObject(dict);
      }
      var next5 = new byte[5];
      mem.ReadExactly(next5, 0, 4);
      if (next5[0..4].SequenceEqual(True))
      {
        return new JsonBoolean(true);
      }
      if (next5[0..4].SequenceEqual(Null))
      {
        return new JsonNull();
      }
      next5[4] = NextByte() ?? throw new JsonParseError("Unexpected end of input. Expected `false`");
      if (next5.SequenceEqual(False))
      {
        return new JsonBoolean(false);
      }
      throw new JsonParseError($"Expected next value, found `{Encoding.UTF8.GetString(mem.ToArray())}`");
    }

    return new JsonNull();
  }

  byte? NextByte()
  {
    int b = mem.ReadByte();
    return b == -1 ? null : (byte)b;
  }

  byte? PeekByte()
  {
    int b = mem.ReadByte();
    if (b == -1)
    {
      return null;
    }
    else
    {
      mem.Seek(-1, SeekOrigin.Current);
      return (byte)b;
    }
  }

  void Ignore(int bytes = 1)
  {
    mem.Seek(bytes, SeekOrigin.Current);
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
    byte? c;
    while ((c = PeekByte()) is not null && IsWhitespace(c!.Value))
    {
      Ignore();
    }
  }

  string ReadString()
  {
    if (NextByte() != (byte)'"')
      throw new JsonParseError("Expected string start");
    MemoryStream s = new();
    byte? c;
    while ((c = NextByte()) is not null)
    {
      switch (c)
      {
        case (byte)'"':
          return Encoding.UTF8.GetString(s.ToArray());
        case (byte)'\\':
          s.Write(Escape());
          break;
        default:
          s.WriteByte(c.Value);
          break;
      }
    }
    throw new JsonParseError("Unexpected end of input. Expected end of string.");
  }

  byte[] Escape()
  {
    MemoryStream s = new(4);
    var escape = NextByte();
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
        var code = new byte[4];
        mem.ReadExactly(code);
        char[] cs = new char[] { (char)Convert.ToUInt32(Encoding.UTF8.GetString(code), 16) };
        if (NextByte() == (byte)'\\' && PeekByte() == (byte)'u')
        {
          Ignore();
          var secondCode = new byte[4];
          mem.ReadExactly(secondCode);
          cs = new char[] { cs[0], (char)Convert.ToUInt32(Encoding.UTF8.GetString(secondCode), 16) };
        }
        else
        {
          Ignore(-1);
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
      byte? i;
      while ((i = PeekByte()) is not null and
      ((>= (byte)'0' and <= (byte)'9')
        or (byte)'-'
        or (byte)'+'
        or (byte)'.'
        or (byte)'E'
        or (byte)'e'))
      {
        s.WriteByte(NextByte()!.Value);
      }
      return Convert.ToDouble(Encoding.UTF8.GetString(s.ToArray()));
    }
    catch
    {
      throw new JsonParseError($"Expected number. Found `{Encoding.UTF8.GetString(s.ToArray())}`");
    }
  }
}