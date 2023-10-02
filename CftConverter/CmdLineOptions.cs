using CommandLine;
using CommandLine.Text;

namespace CgeTools.CftConverter;

public class CmdLineOptions
{
    [Option('i', "input", Required = true,
        HelpText = "Set path to the input file (either .cft or .png for reverse conversion)")]
    public string InputFile { get; set; }

    [Option('o', "output", Required = true,
        HelpText = "Set path to the output file (either .png or .cft for reverse conversion)")]
    public string OutputFile { get; set; }

    [Usage(ApplicationAlias = "cft-converter")]
    // ReSharper disable once UnusedMember.Global
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example("Convert font to png for editing",
                new CmdLineOptions { InputFile = "cge.cft", OutputFile = "cge.png" });
            yield return new Example("Convert png to ingame font",
                new CmdLineOptions { InputFile = "cge.png", OutputFile = "cge.cft" });
        }
    }
}