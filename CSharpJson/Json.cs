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

public interface JsonValue
{
  public JsonType Type { get; }
}

public class JsonNull : JsonValue
{
  public JsonType Type => JsonType.Null;
  public JsonNull()
  { }
}

public class JsonBoolean : JsonValue
{
  public JsonType Type => JsonType.Boolean;
  public bool Val { get; set; }
  public JsonBoolean(bool val)
  {
    Val = val;
  }
}

public class JsonNumber : JsonValue
{
  public JsonType Type => JsonType.Number;
  public double Val { get; set; }
  public JsonNumber(double val)
  {
    Val = val;
  }
}

public class JsonString : JsonValue
{
  public JsonType Type => JsonType.String;
  public string Val { get; set; }
  public JsonString(string val)
  {
    Val = val;
  }
}

public class JsonArray : JsonValue
{
  public JsonType Type => JsonType.Array;
  public List<JsonValue> Val { get; set; }
  public JsonArray(List<JsonValue> val)
  {
    Val = val;
  }
}

public class JsonObject : JsonValue
{
  public JsonType Type => JsonType.Object;
  public Dictionary<string, JsonValue> Val { get; set; }
  public JsonObject(Dictionary<string, JsonValue> dict)
  {
    Val = dict;
  }
}

public class JsonDecode
{

}
