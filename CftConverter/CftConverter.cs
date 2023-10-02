using System.Collections;
using System.Reflection;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;

namespace CgeTools.CftConverter;

// Converts a CGE Engine Font (.cft) to PNG and back
// Based on information found here: https://criezy.blogspot.com/2014/09/do-you-play-english-part-3.html
public static class CftConverter
{
    private const string LabelFont = "small_pixel.ttf";
    private const int ImgWidth = 220;
    private const int ImgHeight = 220;
    private const int NumChars = 256;

    public static void Convert(CmdLineOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.InputFile))
        {
            Console.Error.WriteLine("Input file must not be empty");
            Environment.Exit(-1);
        }

        if (string.IsNullOrWhiteSpace(options.OutputFile))
        {
            Console.Error.WriteLine("Output file must not be empty");
            Environment.Exit(-1);
        }

        if (!File.Exists(options.InputFile))
        {
            Console.Error.WriteLine($"Input file \"{options.InputFile}\" does not exist");
            Environment.Exit(-1);
        }

        var ext = Path.GetExtension(options.InputFile).ToLower();
        switch (ext)
        {
            case ".cft":
                ConvertToPng(options);
                break;
            case ".png":
                ConvertToCfg(options);
                break;
            default:
                Console.Error.WriteLine($"Unknown input file extension: {ext}");
                Environment.Exit(-1);
                break;
        }
    }

    private static void ConvertToPng(CmdLineOptions options)
    {
        using var image = new Image<Rgb24>(ImgWidth, ImgHeight, BackgroundColor);
        FontCollection collection = new();
        var family =
            collection.Add(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!,
                LabelFont));
        var font = family.CreateFont(8, FontStyle.Regular);

        var drawingOptions = new DrawingOptions
        {
            GraphicsOptions = new GraphicsOptions
            {
                Antialias = false
            }
        };

        for (var i = 0; i < 16; i++)
        {
            var pos = (i + 1) * Offset;
            var text = i.ToString("X");
            image.Mutate(c =>
            {
                // First, we draw 0-F on the x-axis
                c.DrawText(drawingOptions, text, font, LabelColor, new PointF(pos + 2, 3));
                // Second, we draw 0-F on the y-axis
                c.DrawText(drawingOptions, text, font, LabelColor, new PointF(5, pos + 1));
            });
        }

        using var fs = new FileStream(options.InputFile, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);

        var fontWidths = new byte[NumChars];

        for (var i = 0; i < fontWidths.Length; i++)
        {
            fontWidths[i] = br.ReadByte();
        }

        var fontBits = new BitArray[NumChars];

        for (var i = 0; i < fontWidths.Length; i++)
        {
            var fontWidth = fontWidths[i];
            var fontBytes = br.ReadBytes(fontWidth);
            fontBits[i] = new BitArray(fontBytes);
        }

        for (var i = 0; i < fontBits.Length; i++)
        {
            var row = i / 16;
            var col = i % 16;

            var x = (col + 1) * Offset;
            var y = (row + 1) * Offset;

            image.Mutate(c => c.DrawPolygon(drawingOptions, BorderColor, 1,
                new PointF(x, y),
                new PointF(x + 7, y),
                new PointF(x + 7, y + 9),
                new PointF(x, y + 9))
            );

            var bits = fontBits[i];

            for (var j = 0; j < bits.Count; j++)
            {
                image[x + 1 + j / 8, y + 1 + j % 8] = bits[j] ? Black : White;
            }
        }

        image.SaveAsPng(options.OutputFile);
    }

    private static void ConvertToCfg(CmdLineOptions options)
    {
        using var image = Image.Load<Rgb24>(options.InputFile);
        var i = 0;

        var bytes = new byte[NumChars][];

        for (var row = 0; row < 16; row++)
        {
            var startY = BaseStart + row * Offset;
            for (var col = 0; col < 16; col++)
            {
                var startX = BaseStart + col * Offset;
                var bitArray = new BitArray(HeightPerChar * WidthPerChar);
                var actualWidth = WidthPerChar;

                for (var dataX = 0; dataX < WidthPerChar; dataX++)
                {
                    var countNonBwColors = 0;
                    for (var dataY = 0; dataY < HeightPerChar; dataY++)
                    {
                        var curX = startX + dataX;
                        var curY = startY + dataY;
                        var idx = dataX * HeightPerChar + dataY;

                        var color = image[curX, curY];
                        if (color == Black)
                        {
                            bitArray[idx] = true;
                        }
                        else if (color == White)
                        {
                            bitArray[idx] = false;
                        }
                        else
                        {
                            countNonBwColors++;
                        }
                    }

                    switch (countNonBwColors)
                    {
                        case 0:
                            continue;
                        case HeightPerChar:
                            actualWidth--;
                            break;
                        default:
                            throw new Exception(
                                $"Colors invalid at row {row}, col {col}, character column index {startX + dataX}: Count of non B/W colors should either be 0 or {HeightPerChar} (actual: {countNonBwColors})");
                    }
                }

                var charBytes = new byte[WidthPerChar];
                bitArray.CopyTo(charBytes, 0);
                if (actualWidth != WidthPerChar)
                {
                    Array.Resize(ref charBytes, actualWidth);
                }

                bytes[i] = charBytes;
                i++;
            }
        }

        using var fs = new FileStream(options.OutputFile, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);

        foreach (var b in bytes)
        {
            bw.Write((byte)b.Length);
        }

        foreach (var b in bytes)
        {
            bw.Write(b);
        }
    }

    #region Colors

    private static readonly Rgb24 BackgroundColor = new(143, 212, 255);
    private static readonly Rgb24 Black = new(0, 0, 0);
    private static readonly Rgb24 White = new(255, 255, 255);
    private static readonly Rgb24 BorderColor = new(255, 174, 0);
    private static readonly Rgb24 LabelColor = new(0, 0, 255);

    #endregion

    #region ToCftConstants

    private const int BaseStart = 13;
    private const int WidthPerChar = 6;
    private const int HeightPerChar = 8;
    private const int Offset = 12;

    #endregion
}