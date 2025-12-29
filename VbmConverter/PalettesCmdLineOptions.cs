using CommandLine;
using CommandLine.Text;

namespace CgeTools.VbmConverter;

[Verb("palettes", aliases: ["pal", "p"], HelpText = "Extract palettes out of .vbm files")]
public class PalettesCmdLineOptions : BaseCmdLineOptions
{
    [Usage(ApplicationAlias = "vbm-converter")]
    // ReSharper disable once UnusedMember.Global
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example("Extract palettes of all files in a directory",
                new PalettesCmdLineOptions { InputPath = @"C:\TheInputPath", OutputPath = @"C:\TheOutputPath" });
        }
    }
}