using CSharpJson;

string jsonFile = args[0];

var json = File.ReadAllBytes(jsonFile);

JsonDocument.DecodeRaw(json);