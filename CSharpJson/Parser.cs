using System.Text;

namespace CSharpJson;

enum ParseState
{
  Value,
  Object,
  Array
}

public class JsonParseError : Exception
{
  public JsonParseError() { }

  public JsonParseError(string message) : base(message) { }

  public JsonParseError(string message, Exception innerException) : base(message, innerException) { }
}


internal class Parser
{
  ByteIterator iter;

  static readonly byte[] True = Encoding.UTF8.GetBytes("true");
  static readonly byte[] False = Encoding.UTF8.GetBytes("false");
  static readonly byte[] Null = Encoding.UTF8.GetBytes("null");

  public Parser(ByteIterator itr)
  {
    iter = itr;
  }

  public IJsonValue ParseNext(ParseState state)
  {
    iter.IgnoreWhitespace();
    byte? c = iter[0];
    if (c.HasValue)
    {
      byte d = c.Value;
      switch (state)
      {
        case ParseState.Value:
          if (IsString(d))
            return new JsonString(ReadString());
          if (IsNumber(d))
            return new JsonNumber(0);
          if (IsArray(d))
            return new JsonArray(new(Array.Empty<IJsonValue>()));
          if (IsObject(d))
            return new JsonObject(new());
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
        case ParseState.Object:
          break;
        case ParseState.Array:
          break;
      }
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

  string ReadString()
  {
    if (iter.Next() != (byte)'"')
      throw new JsonParseError("Expected string start");
    MemoryStream s = new();
    while (iter[0] != null)
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
    MemoryStream s = new();
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
}