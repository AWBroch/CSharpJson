using System.Text;

namespace CSharpJson;

public interface IJsonValue
{
  string ToJson();
}

public struct JsonNull : IJsonValue
{
  public JsonNull()
  { }
  public string ToJson() => "null";
}

public struct JsonBoolean : IJsonValue
{
  public string ToJson() => $"{(Val ? "true" : "false")}";
  public bool Val { get; set; }
  public JsonBoolean(bool val)
  {
    Val = val;
  }
}

public struct JsonNumber : IJsonValue
{
  public string ToJson() => $"{Val}";
  public double Val { get; set; }
  public JsonNumber(double val)
  {
    Val = val;
  }
}

public struct JsonString : IJsonValue
{
  public string ToJson() => StringToJson(Val);
  internal static string StringToJson(string v) => $"\"{v
  .Replace("\"", "\\\"")
  .Replace("\n", "\\n")
  .Replace("\t", "\\t")
  .Replace("\r", "\\r")
  .Replace("\x08", "\\b")
  .Replace("\x0C", "\\f")}\"";
  public string Val { get; set; }
  public JsonString(string val)
  {
    Val = val;
  }
}

public struct JsonArray : IJsonValue
{
  public string ToJson() => $"[{string.Join(',', Val.Select(v => v.ToJson()))}]";
  public List<IJsonValue> Val { get; set; }
  public JsonArray(List<IJsonValue> val)
  {
    Val = val;
  }
}

public struct JsonObject : IJsonValue
{
  public string ToJson() => $"{{{string.Join(',', Val.ToArray().Select(kv => $"{JsonString.StringToJson(kv.Key)}:{kv.Value.ToJson()}"))}}}";
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
    doc.Root = parser.ParseNext(ParseState.Value);
    return doc;
  }
}