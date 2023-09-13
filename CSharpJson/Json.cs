using System.Text;

namespace CSharpJson;

public enum JsonType
{
  Object,
  Array,
  String,
  Number,
  Boolean,
  Null
};

public interface IJsonValue
{
  public JsonType Type { get; }
}

public struct JsonNull : IJsonValue
{
  public readonly JsonType Type => JsonType.Null;
  public JsonNull()
  { }
}

public struct JsonBoolean : IJsonValue
{
  public readonly JsonType Type => JsonType.Boolean;
  public bool Val { get; set; }
  public JsonBoolean(bool val)
  {
    Val = val;
  }
}

public struct JsonNumber : IJsonValue
{
  public readonly JsonType Type => JsonType.Number;
  public double Val { get; set; }
  public JsonNumber(double val)
  {
    Val = val;
  }
}

public struct JsonString : IJsonValue
{
  public readonly JsonType Type => JsonType.String;
  public string Val { get; set; }
  public JsonString(string val)
  {
    Val = val;
  }
}

public struct JsonArray : IJsonValue
{
  public readonly JsonType Type => JsonType.Array;
  public List<IJsonValue> Val { get; set; }
  public JsonArray(List<IJsonValue> val)
  {
    Val = val;
  }
}

public struct JsonObject : IJsonValue
{
  public readonly JsonType Type => JsonType.Object;
  public Dictionary<string, IJsonValue> Val { get; set; }
  public JsonObject(Dictionary<string, IJsonValue> dict)
  {
    Val = dict;
  }
}

public class JsonDocument
{
  public IJsonValue Root { get; set; }
  public JsonDocument()
  {
    Root = new JsonNull();
  }
  public static JsonDocument DecodeString(string str)
  {
    return DecodeRaw(Encoding.UTF8.GetBytes(str));
  }
  public static JsonDocument DecodeRaw(byte[] bytes)
  {
    JsonDocument doc = new();
    ByteIterator iter = new(bytes);
    var parser = new Parser(iter);
    return doc;
  }
}