namespace CSharpJson.Test;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void JsonTypeTest()
    {
        JsonType type = JsonType.Null;
        Assert.AreEqual(type.ToString(), "Null");
        type = JsonType.Object;
        Assert.AreEqual(type.ToString(), "Object");
    }
    [TestMethod]
    public void IJsonValueTest()
    {
        JsonNull nullVal = new();
        Assert.AreEqual(nullVal.Type, JsonType.Null);
        JsonNumber numberVal = new(12.5);
        Assert.AreEqual(numberVal.Type, JsonType.Number);
        IJsonValue anyVal = numberVal;
        Assert.AreEqual(anyVal.Type, JsonType.Number);
        Assert.AreEqual(((JsonNumber)anyVal).Val, 12.5);
        JsonBoolean boolVal = new(true);
        Assert.AreEqual(boolVal.Type, JsonType.Boolean);
        anyVal = boolVal;
        Assert.AreEqual(anyVal.Type, JsonType.Boolean);
        Assert.AreEqual(((JsonBoolean)anyVal).Val, true);
        JsonString stringVal = new("Hello, world!");
        Assert.AreEqual(stringVal.Type, JsonType.String);
        anyVal = stringVal;
        Assert.AreEqual(anyVal.Type, JsonType.String);
        Assert.AreEqual(((JsonString)anyVal).Val, "Hello, world!");
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
        Assert.AreEqual(arrayVal.Type, JsonType.Array);
        anyVal = arrayVal;
        Assert.AreEqual(anyVal.Type, JsonType.Array);
        Assert.AreEqual(((JsonArray)anyVal).Val, list);
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
        Assert.AreEqual(objectValue.Type, JsonType.Object);
        anyVal = objectValue;
        Assert.AreEqual(anyVal.Type, JsonType.Object);
        Assert.AreEqual(((JsonObject)anyVal).Val, dict);
    }

    [TestMethod]
    public void ParseTest()
    {
        var doc = JsonDocument.DecodeString("{\"key\":\"value\"}");
        Assert.AreEqual(doc.Root, new JsonObject(new Dictionary<string, IJsonValue> {
            {"key", new JsonString("value")}
        }));
    }
}