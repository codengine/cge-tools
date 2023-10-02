using CgeTools.VbmConverter;
using CgeTools.VbmConverter.Converter;
using CommandLine;
using CommandLine.Text;

try
{
    var parser = new Parser(settings => { settings.HelpWriter = null; });

    var parserResult = parser.ParseArguments<PalettesCmdLineOptions, PngCmdLineOptions, VbmCmdLineOptions>(args);

    parserResult.WithParsed<PalettesCmdLineOptions>(VbmConverter.ExtractPalettes)
        .WithParsed<PngCmdLineOptions>(VbmConverter.ToPng)
        .WithParsed<VbmCmdLineOptions>(VbmConverter.ToVbm)
        .WithNotParsed(_ =>
        {
            var helpText = HelpText.AutoBuild(parserResult, h =>
            {
                h.AddEnumValuesToHelpText = true;
                return h;
            }, parser.Settings.MaximumDisplayWidth);
            Console.Error.Write(helpText);
        });
}
catch (ValidationException ex)
{
    Console.Error.WriteLine(ex.Message);
    Environment.Exit(-1);
}