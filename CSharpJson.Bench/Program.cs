using CSharpJson;
using CommandLine;
using System.Text.Json.Nodes;

Parser.Default.ParseArguments<Options>(args)
.WithParsed<Options>(options =>
{
  var json = File.ReadAllBytes(options.File);

  if (options.StdLib)
  {
    System.Text.Json.JsonSerializer.Deserialize(json, typeof(JsonValue));
  }
  else
  {
    JsonDocument.DecodeRaw(json);
  }
});
public class Options
{
  [Option('s', "stdlib", HelpText = "Use stdlib JSON parser")]
  public bool StdLib { get; set; }
  [Value(0, MetaName = "file", HelpText = "file to parse", Required = true)]
  public string File { get; set; } = "";
}