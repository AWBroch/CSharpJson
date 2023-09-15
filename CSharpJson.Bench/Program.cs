using CSharpJson;
using CommandLine;
using System.Text.Json.Nodes;
using System.Text;

Parser.Default.ParseArguments<Options>(args)
.WithParsed<Options>(options =>
{
  var json = File.ReadAllBytes(options.File);

  if (options.StdLib)
  {
    System.Text.Json.JsonSerializer.Deserialize(json, typeof(JsonValue));
  }
  else if (options.Newtonsoft)
  {
    Newtonsoft.Json.JsonConvert.DeserializeObject(Encoding.UTF8.GetString(json));
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
  [Option('n', "newtonsoft", HelpText = "Use Newtonsoft JSON parser")]
  public bool Newtonsoft { get; set; }
  [Value(0, MetaName = "file", HelpText = "file to parse", Required = true)]
  public string File { get; set; } = "";
}