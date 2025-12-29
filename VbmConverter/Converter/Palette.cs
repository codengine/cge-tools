using SixLabors.ImageSharp.PixelFormats;

namespace CgeTools.VbmConverter.Converter;

public class Palette
{
    public const int CountColorsPerPalette = 256;

    private readonly Rgb24[] _colors;

    public Palette(Rgb24[] colors)
    {
        _colors = ValidateColors(colors);
    }

    public Rgb24 this[int index]
    {
        get => _colors[index];
        set => _colors[index] = value;
    }

    private static Rgb24[] ValidateColors(Rgb24[] colors)
    {
        return colors.Length != CountColorsPerPalette
            ? throw new ArgumentException($"Expected {CountColorsPerPalette} colors, but received {colors.Length}")
            : colors;
    }

    public void WriteToAct(string outp)
    {
        using var fs = new FileStream(outp, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);
        for (var i = 0; i < 256; i++)
        {
            if (i < _colors.Length)
            {
                bw.Write(_colors[i].R);
                bw.Write(_colors[i].G);
                bw.Write(_colors[i].B);
            }
            else
            {
                // Write zero bytes if no more colors in the palette
                bw.Write((byte)0);
                bw.Write((byte)0);
                bw.Write((byte)0);
            }
        }
    }

    public static Palette ReadFromAct(string inp)
    {
        var result = new Rgb24[CountColorsPerPalette];

        var bytes = File.ReadAllBytes(inp);
        var memoryStream = new MemoryStream(bytes);
        var reader = new BinaryReader(memoryStream);

        var i = 0;

        while (memoryStream.Position < memoryStream.Length)
        {
            var read = reader.ReadBytes(3);
            result[i++] = new Rgb24(read[0], read[1], read[2]);
        }

        return new Palette(result);
    }
}