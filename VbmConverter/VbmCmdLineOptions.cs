using CommandLine;
using CommandLine.Text;

namespace CgeTools.VbmConverter;

[Verb("vbm", HelpText = "Convert .png files to .vbm")]
public class VbmCmdLineOptions : BaseCmdLineOptions
{
    [Option("embed-palette", Required = false,
        HelpText = "If defined, the .act palette will be embedded within the .vbm file")]
    public string? EmbedPalette { get; set; }

    [Usage(ApplicationAlias = "vbm-converter")]
    // ReSharper disable once UnusedMember.Global
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example("Convert all files in a directory to vbm",
                new VbmCmdLineOptions { InputPath = @"C:\TheInputPath", OutputPath = @"C:\TheOutputPath" });
            yield return new Example("Convert a single file to vbm",
                new VbmCmdLineOptions { InputPath = @"C:\TheInputPath\file.png", OutputPath = @"C:\TheOutputPath" });
        }
    }
}