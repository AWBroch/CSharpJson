namespace CSharpJson.Test;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void JsonTypeTest()
    {
        JsonType type = JsonType.Null;
        Assert.AreEqual("Null", type.ToString());
        type = JsonType.Object;
        Assert.AreEqual("Object", type.ToString());
    }
    [TestMethod]
    public void IJsonValueTest()
    {
        JsonNull nullVal = new();
        Assert.AreEqual(JsonType.Null, nullVal.Type);
        JsonNumber numberVal = new(12.5);
        Assert.AreEqual(JsonType.Number, numberVal.Type);
        IJsonValue anyVal = numberVal;
        Assert.AreEqual(JsonType.Number, anyVal.Type);
        Assert.AreEqual(12.5, ((JsonNumber)anyVal).Val);
        JsonBoolean boolVal = new(true);
        Assert.AreEqual(JsonType.Boolean, boolVal.Type);
        anyVal = boolVal;
        Assert.AreEqual(JsonType.Boolean, anyVal.Type);
        Assert.AreEqual(true, ((JsonBoolean)anyVal).Val);
        JsonString stringVal = new("Hello, world!");
        Assert.AreEqual(JsonType.String, stringVal.Type);
        anyVal = stringVal;
        Assert.AreEqual(JsonType.String, anyVal.Type);
        Assert.AreEqual("Hello, world!", ((JsonString)anyVal).Val);
        List<IJsonValue> list = new(new IJsonValue[] {
            new JsonNull(),
            new JsonNumber(0.0),
            new JsonString("Hey, hey, hey"),
            new JsonBoolean(true),
            new JsonBoolean(false),
            new JsonNull(),
            new JsonString("Testing, one two three")
        });
        JsonArray arrayVal = new(list);
        Assert.AreEqual(JsonType.Array, arrayVal.Type);
        anyVal = arrayVal;
        Assert.AreEqual(JsonType.Array, anyVal.Type);
        Assert.AreEqual(list, ((JsonArray)anyVal).Val);
        Dictionary<string, IJsonValue> dict = new()
        {
            { "thisField", new JsonString("That") },
            {"otherField", new JsonNull()},
            {"artist", new JsonString("Coutning Crows")},
            {"song", new JsonString("Mr. Jones")},
            {"album", new JsonString("August and Everything After")},
            {"year", new JsonNumber(1993)},
            {"awesome", new JsonBoolean(true)}
        };
        JsonObject objectValue = new(dict);
        Assert.AreEqual(JsonType.Object, objectValue.Type);
        anyVal = objectValue;
        Assert.AreEqual(JsonType.Object, anyVal.Type);
        Assert.AreEqual(dict, ((JsonObject)anyVal).Val);
    }

    [TestMethod]
    public void ParseAtoms()
    {
        var doc = JsonDocument.DecodeString("true");
        Assert.AreEqual(new JsonBoolean(true), doc.Root);
        doc = JsonDocument.DecodeString("false");
        Assert.AreEqual(new JsonBoolean(false), doc.Root);
        doc = JsonDocument.DecodeString("null");
        Assert.AreEqual(new JsonNull(), doc.Root);
    }

    [TestMethod]
    public void ParseString()
    {
        var doc = JsonDocument.DecodeString("\"short string\"");
        Assert.AreEqual("short string", ((JsonString)doc.Root).Val);
        var s = "\"string, \\\"string\\\", string—🎸\\uD83E\\uDD95\\u3ED8\\u0003\\f\"";
        doc = JsonDocument.DecodeString(s);
        Assert.AreEqual("string, \"string\", string—🎸🦕㻘\x03\x0C", ((JsonString)doc.Root).Val);
    }
}