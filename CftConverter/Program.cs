using CgeTools.CftConverter;
using CommandLine;

Parser.Default.ParseArguments<CmdLineOptions>(args)
    .WithParsed(CftConverter.Convert);