using System.Text;

namespace CSharpJson.Test;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void PrettyPrint()
    {
        Assert.AreEqual(@"{
    ""key"": ""value"",
    ""otherKey"": ""otherValue"",
    ""array"": [
        {
            ""key"": true,
            ""more"": null
        },
        [
            false,
            12.24,
            47
        ]
    ]
}",
        new JsonObject(new Dictionary<string, IJsonValue> {
            {"key", new JsonString("value")},
            {"otherKey", new JsonString("otherValue")},
            {"array", new JsonArray(new List<IJsonValue> {
                new JsonObject(new Dictionary<string, IJsonValue> {
                    {"key", new JsonBoolean(true)},
                    {"more", new JsonNull()}
                }),
                new JsonArray(new List<IJsonValue> {
                    new JsonBoolean(false),
                    new JsonNumber(12.24),
                    new JsonNumber(47)
                })
            })}
        }).ToJsonPretty());
    }
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

    [TestMethod]
    public void ParseArray()
    {
        var doc = JsonDocument.DecodeString("[12,13,14]");
        Assert.IsTrue(new List<IJsonValue> {
            new JsonNumber(12),
            new JsonNumber(13),
            new JsonNumber(14)
        }.SequenceEqual(((JsonArray)doc.Root).Val));
        var s = new StringBuilder(string.Concat(Enumerable.Repeat("[5   ,\n\n", 300)));
        s.Append("[\"algo muy interesante. Ay si, ya tu sabes. ¡Imagínate!\", 3.1415926535, 5.2e+50, \"\",null,true,false,[],[],[],[[[[[[[[[[[[[[]]]]]]]]]]]]]]]");
        s.Append(string.Concat(Enumerable.Repeat(']', 300)));
        doc = JsonDocument.DecodeString(s.ToString());
        Assert.IsTrue(doc.Root is JsonArray);
    }

    [TestMethod]
    public void ParseObject()
    {
        var doc = JsonDocument.DecodeString("{\r\n\t\"name\": \"Steve\",\r\n\t\"nickname\": \"Cementhead\"\r\n}");
        Assert.IsTrue(doc.Root is JsonObject);
        JsonObject obj = (JsonObject)doc.Root;
        Assert.AreEqual("Steve", ((JsonString)obj.Val["name"]).Val);
        Assert.AreEqual("Cementhead", ((JsonString)obj.Val["nickname"]).Val);
        doc = JsonDocument.DecodeString(@"{
            ""stuff"": [
                ""string"",
                true,
                false,
                null,
                {
                    ""moreStuff"": true,
                    ""really"": ""yes"",
                    ""theStuff"": [
                        3,
                        0.1,
                        0.04,
                        0.001,
                        0.0005,
                        0.00009
                    ]
                }
            ]
        }");
        Assert.IsTrue(doc.Root is JsonObject);
        obj = (JsonObject)doc.Root;
        Assert.IsTrue(obj["stuff"] is JsonArray);
        var arr = (JsonArray)obj["stuff"];
        Assert.IsTrue(arr.Val[4] is JsonObject);
        obj = (JsonObject)arr.Val[4];
        Assert.AreEqual("yes", ((JsonString)obj["really"]).Val);
        Assert.IsInstanceOfType(obj["theStuff"], typeof(JsonArray));
        Assert.AreEqual(6, ((JsonArray)obj["theStuff"]).Val.Count);
    }
}