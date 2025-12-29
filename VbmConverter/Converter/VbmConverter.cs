using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CgeTools.VbmConverter.Converter;

public static class VbmConverter
{
    private const int VirtualWidth = 320;
    private const int VirtualHeight = 200;
    private const string MappedColorTableFnSuffix = "mct";
    private static readonly Rgb24 Transparency = new(255, 0, 255);

    public static void ExtractPalettes(PalettesCmdLineOptions options)
    {
        Validate(options);
        var files = GetFiles(options, "*.vbm");
        foreach (var file in files)
        {
            var vbmFile = VbmReader.Read(file);
            if (vbmFile.Palette is not { } palette)
            {
                continue;
            }

            var outPath = Path.Combine(options.OutputPath, $"{Path.GetFileNameWithoutExtension(file)}.act");
            if (options.Verbose)
            {
                Console.WriteLine($"Writing palette to {outPath}...");
            }

            palette.WriteToAct(outPath);
        }
    }

    public static void ToPng(PngCmdLineOptions options)
    {
        Validate(options);
        var forcedPalette = !string.IsNullOrWhiteSpace(options.ForcePalette)
            ? Palette.ReadFromAct(options.ForcePalette)
            : null;
        var fallbackPalette = !string.IsNullOrWhiteSpace(options.FallbackPalette)
            ? Palette.ReadFromAct(options.FallbackPalette)
            : null;
        var paletteFiles = !string.IsNullOrWhiteSpace(options.PalettePath)
            ? ReadPalettes(options.PalettePath)
            : null;

        if (options.GameType == GameType.Soltys)
        {
            SoltysSystemColors.Modify(forcedPalette);
            SoltysSystemColors.Modify(fallbackPalette);
            foreach (var pal in paletteFiles?.Values ?? Enumerable.Empty<Palette>())
            {
                SoltysSystemColors.Modify(pal);
            }
        }

        var files = GetFiles(options, "*.vbm");
        foreach (var file in files)
        {
            var vbmFile = VbmReader.Read(file);
            var fn = Path.GetFileNameWithoutExtension(file);

            if (forcedPalette != null)
            {
                vbmFile.Palette = forcedPalette;
            }
            else if (vbmFile.Palette == null)
            {
                vbmFile.Palette = FindPalette(fn, paletteFiles) ?? fallbackPalette;
            }

            if (vbmFile.Palette == null)
            {
                Console.WriteLine($"No palette found for \"{file}\", skipping...");
                continue;
            }

            using var image = vbmFile.ToImage(VirtualWidth, VirtualHeight, Transparency, out var usedColors);

            var usedColorsGrouped = usedColors.GroupBy(kvp => kvp.Value, kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.ToList());

            var nonUniqueColors = usedColorsGrouped.Where(kvp => kvp.Value.Count > 1)
                .ToList();

            if (nonUniqueColors.Count > 1)
            {
                Console.WriteLine(
                    $"Palette used for {file} contains non-unique colors: {string.Join(", ", nonUniqueColors.Select(kvp => kvp.Key))}");
            }

            var imageOutputPath = Path.Combine(options.OutputPath, $"{fn}.png");

            if (options.Verbose)
            {
                Console.WriteLine($"Writing {imageOutputPath}...");
            }

            image.SaveAsPng(imageOutputPath);
            SaveUsedPalette(usedColors, Path.Combine(options.OutputPath, $"{fn}.{MappedColorTableFnSuffix}"));
        }
    }

    private static void SaveUsedPalette(Dictionary<byte, Rgb24> usedColors, string outputPath)
    {
        var paletteColors = usedColors.OrderBy(kvp => kvp.Key).Select(kvp => new PaletteMap
        {
            OriginalColorIndex = kvp.Key,
            R = kvp.Value.R,
            G = kvp.Value.G,
            B = kvp.Value.B
        }).ToList();

        Utils.SavePaletteMap(paletteColors, outputPath);
    }

    private static Palette? FindPalette(string filename, Dictionary<string, Palette>? paletteFiles)
    {
        var fnLower = filename.ToLower();

        if (paletteFiles?.TryGetValue(fnLower, out var foundPal) ?? false)
        {
            return foundPal;
        }

        return paletteFiles?.Where(kvp => fnLower.StartsWith(kvp.Key))
            .Select(kvp => kvp.Value)
            .FirstOrDefault();
    }

    private static Dictionary<string, Palette> ReadPalettes(string inputPath)
    {
        var palettes = Directory.GetFiles(inputPath, "*.act");
        return palettes.ToDictionary(p => Path.GetFileNameWithoutExtension(p).ToLower(), Palette.ReadFromAct);
    }

    public static void ToVbm(VbmCmdLineOptions options)
    {
        Validate(options);
        var files = GetFiles(options, "*.png");
        var embedPalette = !string.IsNullOrWhiteSpace(options.EmbedPalette)
            ? Palette.ReadFromAct(options.EmbedPalette)
            : null;

        foreach (var file in files)
        {
            var mappedColorTable = ReadUsedPalette(file);
            if (mappedColorTable == null)
            {
                continue;
            }

            var converter = new ToVbmConverter(mappedColorTable, VirtualWidth, VirtualHeight, Transparency);

            var fn = Path.GetFileNameWithoutExtension(file);
            var outputPath = Path.Combine(options.OutputPath, $"{fn}.vbm");

            if (options.Verbose)
            {
                Console.WriteLine($"Writing {outputPath}...");
            }

            converter.Convert(file, outputPath, embedPalette);
        }
    }

    private static Dictionary<Rgb24, byte>? ReadUsedPalette(string file)
    {
        var directoryName = Path.GetDirectoryName(file)!;
        var fn = $"{Path.GetFileNameWithoutExtension(file)}.{MappedColorTableFnSuffix}";
        var path = Path.Combine(directoryName, fn);
        if (File.Exists(path))
        {
            try
            {
                return Utils.ReadPalFromPalMap(path);
            }
            catch (Exception)
            {
                Console.Error.WriteLine($"Unable to read palette map from {path}, skipping...");
                return null;
            }
        }

        Console.Error.WriteLine($"No mapped color table found (expected at {path}), skipping...");
        return null;
    }

    private static void Validate(PngCmdLineOptions options)
    {
        Validate((BaseCmdLineOptions)options);
        if (!string.IsNullOrWhiteSpace(options.ForcePalette) && !File.Exists(options.ForcePalette))
        {
            throw new ValidationException("Palette file does not exist");
        }

        if (!string.IsNullOrWhiteSpace(options.PalettePath) && !Directory.Exists(options.PalettePath))
        {
            throw new ValidationException("Palette path does not exist");
        }

        if (!string.IsNullOrWhiteSpace(options.FallbackPalette) && !File.Exists(options.FallbackPalette))
        {
            throw new ValidationException("Fallback palette file does not exist");
        }
    }

    private static void Validate(VbmCmdLineOptions options)
    {
        Validate((BaseCmdLineOptions)options);
        if (!string.IsNullOrWhiteSpace(options.EmbedPalette) && !File.Exists(options.EmbedPalette))
        {
            throw new ValidationException("Palette file does not exist");
        }
    }

    private static void Validate(BaseCmdLineOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.InputPath))
        {
            throw new ValidationException("Input path must not be empty");
        }

        if (string.IsNullOrWhiteSpace(options.OutputPath))
        {
            throw new ValidationException("Output path must not be empty");
        }

        if (!Directory.Exists(options.OutputPath))
        {
            throw new ValidationException("Output path does not exist");
        }
    }

    private static string[] GetFiles(BaseCmdLineOptions options, string fnPattern)
    {
        if (Directory.Exists(options.InputPath))
        {
            var enumerationOptions = new EnumerationOptions
            {
                MatchCasing = MatchCasing.CaseInsensitive
            };
            return Directory.GetFiles(options.InputPath, fnPattern, enumerationOptions);
        }

        return File.Exists(options.InputPath)
            ? [options.InputPath]
            : throw new ValidationException($"Given input path \"{options.InputPath}\" does not exist");
    }
}