namespace CgeTools.VbmConverter.Converter;

public static class VbmReader
{
    public static VbmFile Read(string path)
    {
        using var fs = new FileStream(path, FileMode.Open);
        using var br = new BinaryReader(fs);

        var hasPalette = br.ReadUInt16() == 1;
        var numberOfCommands = br.ReadUInt16();
        var width = br.ReadUInt16();
        var height = br.ReadUInt16();
        var palette = hasPalette ? ReadPalette(br) : null;
        var imageData = br.ReadBytes(numberOfCommands);

        return new VbmFile(width, height, imageData, palette);
    }

    private static Palette ReadPalette(BinaryReader br)
    {
        var colors = new Rgb24[Palette.CountColorsPerPalette];

        for (var i = 0; i < colors.Length; i++)
        {
            var rgb = br.ReadBytes(3);
            colors[i] = Utils.ColorFromDac(rgb[0], rgb[1], rgb[2]);
        }

        return new Palette(colors);
    }
}