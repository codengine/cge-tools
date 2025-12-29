using CommandLine;

#pragma warning disable CS8618

namespace CgeTools.VbmConverter;

public abstract class BaseCmdLineOptions
{
    [Option('i', "input", Required = true,
        HelpText =
            "Input path. Can either be a directory (for batch processing) or a filename (for single processing)")]
    public string InputPath { get; set; }

    [Option('o', "output", Required = true, HelpText = "Output path. Has to be a directory!")]
    public string OutputPath { get; set; }

    [Option('v', "verbose", HelpText = "Extended console output")]
    public bool Verbose { get; set; }
}