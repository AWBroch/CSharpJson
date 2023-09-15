using System.Text;

namespace CSharpJson;

public interface IJsonValue
{
  internal string ToJson();
  internal virtual string ToJsonPretty()
  {
    return ToJson();
  }
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
  public string ToJson() => Val.ToString();
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
  public string ToJsonPretty()
  {
    return @$"[
    {string.Join(",\n    ", Val.Select(v => v.ToJsonPretty().Replace("\n", "\n    ")))}
]";
  }
  public List<IJsonValue> Val { get; set; }
  public JsonArray(List<IJsonValue> val)
  {
    Val = val;
  }
}

public struct JsonObject : IJsonValue
{
  public string ToJson() => $"{{{string.Join(',',
     Val.ToArray()
     .Select(kv => $"{JsonString.StringToJson(kv.Key)}:{kv.Value.ToJson()}"))}}}";
  public string ToJsonPretty()
  {
    return @$"{{
    {string.Join(",\n    ",
     Val.ToArray()
     .Select(kv => $"{JsonString.StringToJson(kv.Key)}: {kv.Value.ToJsonPretty().Replace("\n", "\n    ")}"))}
}}";
  }
  public Dictionary<string, IJsonValue> Val { get; set; }
  public JsonObject(Dictionary<string, IJsonValue> dict)
  {
    Val = dict;
  }

  public IJsonValue this[string idx]
  {
    get
    {
      return Val[idx];
    }
    set
    {
      Val[idx] = value;
    }
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
    var parser = new Parser(bytes);
    doc.Root = parser.ParseNext();
    return doc;
  }
  public static JsonDocument DecodeRaw(MemoryStream mem)
  {
    JsonDocument doc = new();
    var parser = new Parser(mem);
    doc.Root = parser.ParseNext();
    return doc;
  }
}