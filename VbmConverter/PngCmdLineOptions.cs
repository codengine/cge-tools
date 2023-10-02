using CommandLine;
using CommandLine.Text;

namespace CgeTools.VbmConverter;

[Verb("png", HelpText = "Convert .vbm files to .png")]
public class PngCmdLineOptions : BaseCmdLineOptions
{
    [Option("force-palette", SetName = "forceP", Required = false,
        HelpText = "If defined, the given palette will be enforced, even if the .vbm file has it's own")]
    public string? ForcePalette { get; set; }

    [Option("palette-path", SetName = "paletteP", Required = false,
        HelpText =
            "If defined, .act files in this directory will be used as palettes. Filenames are matched using string matching (e.g. 24.act matches 24don01.vbm)")]
    public string? PalettePath { get; set; }

    [Option("fallback-palette", SetName = "paletteP", Required = false,
        HelpText =
            "If defined, the given fallback palette if it is neither supplied on its own nor matched using the palette path")]
    public string? FallbackPalette { get; set; }

    [Option('g', "game", HelpText = "The game to be processed.", Required = true, Default = null)]
    public GameType GameType { get; set; }

    [Usage(ApplicationAlias = "vbm-converter")]
    // ReSharper disable once UnusedMember.Global
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example(
                "Convert all files in a directory to png, using previously extracted .act files and a fallback palette",
                new PngCmdLineOptions
                {
                    InputPath = @"C:\InputPath", OutputPath = @"C:\OutputPath", PalettePath = @"C:\PalettePath",
                    FallbackPalette = @"C:\PalettePath\welcome.act"
                });
            yield return new Example("Convert all files in a directory to png, enforcing the given palette",
                new PngCmdLineOptions
                {
                    InputPath = @"C:\InputPath", OutputPath = @"C:\OutputPath",
                    ForcePalette = @"C:\PalettePath\welcome.act"
                });
            yield return new Example("Convert a single file to png, enforcing the given palette",
                new PngCmdLineOptions
                {
                    InputPath = @"C:\InputPath\24don01.vbm", OutputPath = @"C:\OutputPath",
                    ForcePalette = @"C:\PalettePath\24.act"
                });
        }
    }
}