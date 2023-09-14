using CSharpJson;

string jsonFile = args[0];

var json = await File.ReadAllBytesAsync(jsonFile);

JsonDocument.DecodeRaw(json);