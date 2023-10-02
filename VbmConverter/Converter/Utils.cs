using System.Runtime.InteropServices;

namespace CgeTools.VbmConverter.Converter;

public static class Utils
{
    public static Rgb24 ColorFromDac(byte r, byte g, byte b)
    {
        return new Rgb24((byte)(r << 2), (byte)(g << 2), (byte)(b << 2));
    }

    public static Rgb24 ColorToDac(byte r, byte g, byte b)
    {
        return new Rgb24((byte)(r >> 2), (byte)(g >> 2), (byte)(b >> 2));
    }

    public static void SavePaletteMap(IEnumerable<PaletteMap> paletteColors, string outputPath)
    {
        using var stream = File.OpenWrite(outputPath);
        using var writer = new BinaryWriter(stream);
        foreach (var b in GetBytes(paletteColors))
        {
            writer.Write(b);
        }
    }

    private static IEnumerable<byte> GetBytes<T>(IEnumerable<T> elements) where T : struct
    {
        var enumerable = elements as T[] ?? elements.ToArray();
        var len = enumerable.Length;
        var size = Marshal.SizeOf(default(T));
        var arr = new byte[size * len];

        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            for (var i = 0; i < len; ++i)
            {
                Marshal.StructureToPtr(enumerable[i], ptr, true);
                Marshal.Copy(ptr, arr, i * size, size);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return arr;
    }

    public static Dictionary<Rgb24, byte> ReadPalFromPalMap(string palMapFn)
    {
        var bytes = File.ReadAllBytes(palMapFn);
        var paletteMaps = MemoryMarshal.Cast<byte, PaletteMap>(bytes);
        return paletteMaps.ToArray().ToDictionary(m => new Rgb24(m.R, m.G, m.B), m => m.OriginalColorIndex);
    }
}