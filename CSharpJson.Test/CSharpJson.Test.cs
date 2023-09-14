namespace CSharpJson.Test;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void IJsonValueTest()
    {
        JsonNull nullVal = new();
        Assert.AreEqual("null", nullVal.ToJson());
        JsonNumber numberVal = new(12.5);
        Assert.AreEqual("12.5", numberVal.ToJson());
        IJsonValue anyVal = numberVal;
        Assert.IsTrue(anyVal is JsonNumber);
        Assert.AreEqual(12.5, ((JsonNumber)anyVal).Val);
        JsonBoolean boolVal = new(true);
        Assert.AreEqual("true", boolVal.ToJson());
        anyVal = boolVal;
        Assert.IsTrue(anyVal is JsonBoolean);
        Assert.AreEqual(true, ((JsonBoolean)anyVal).Val);
        JsonString stringVal = new("Hello, world!");
        Assert.AreEqual("\"Hello, world!\"", stringVal.ToJson());
        anyVal = stringVal;
        Assert.IsTrue(anyVal is JsonString);
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
        Assert.AreEqual("[null,0,\"Hey, hey, hey\",true,false,null,\"Testing, one two three\"]", arrayVal.ToJson());
        anyVal = arrayVal;
        Assert.IsTrue(anyVal is JsonArray);
        Assert.AreEqual(list, ((JsonArray)anyVal).Val);
        Dictionary<string, IJsonValue> dict = new()
        {
            { "thisField", new JsonString("That") },
            {"otherField", new JsonNull()},
            {"artist", new JsonString("Counting Crows")},
            {"song", new JsonString("Mr. Jones")},
            {"album", new JsonString("August and Everything After")},
            {"year", new JsonNumber(1993)},
            {"awesome", new JsonBoolean(true)}
        };
        JsonObject objectValue = new(dict);
        Assert.AreEqual("{\"thisField\":\"That\",\"otherField\":null,\"artist\":\"Counting Crows\",\"song\":\"Mr. Jones\",\"album\":\"August and Everything After\",\"year\":1993,\"awesome\":true}", objectValue.ToJson());
        anyVal = objectValue;
        Assert.IsTrue(anyVal is JsonObject);
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

    [TestMethod]
    public void ParseNumber()
    {
        var doc = JsonDocument.DecodeString("12");
        Assert.AreEqual(12.0, ((JsonNumber)doc.Root).Val);
        doc = JsonDocument.DecodeString("3.1415926535");
        Assert.AreEqual(3.1415926535, ((JsonNumber)doc.Root).Val);
        doc = JsonDocument.DecodeString("5.2e+50");
        Assert.AreEqual(5.2e+50, ((JsonNumber)doc.Root).Val);
        doc = JsonDocument.DecodeString("-12.24");
        Assert.AreEqual(-12.24, ((JsonNumber)doc.Root).Val);
        doc = JsonDocument.DecodeString("4.2e-100");
        Assert.AreEqual(4.2e-100, ((JsonNumber)doc.Root).Val);
    }
}