namespace CgeTools.VbmConverter.Converter;

// Convert images from and to .vbm files
// The majority of this code comes from ScummVM (https://github.com/scummvm/scummvm/tree/master/engines/cge)
public class ToVbmConverter
{
    private readonly Dictionary<Rgb24, byte> _mappedMappedColorTable;
    private readonly Rgb24 _transparency;
    private readonly int _virtualHeight;
    private readonly int _virtualWidth;

    public ToVbmConverter(Dictionary<Rgb24, byte> mappedColorTable, int virtualWidth, int virtualHeight,
        Rgb24 transparency)
    {
        _mappedMappedColorTable = mappedColorTable;
        _virtualWidth = virtualWidth;
        _virtualHeight = virtualHeight;
        _transparency = transparency;
    }

    public void Convert(string inp, string outp, Palette? embedPalette)
    {
        using var image = Image.Load<Rgb24>(inp);
        using var fs = new FileStream(outp, FileMode.Create);
        using var bw = new BinaryWriter(fs);

        bw.Write((ushort)(embedPalette != null ? 1 : 0));

        var posOfCommandCount = fs.Position;
        bw.Write((ushort)0);
        bw.Write((ushort)image.Width); // Actual width
        bw.Write((ushort)image.Height); // Actual height

        if (embedPalette != null)
        {
            WritePalette(bw, embedPalette); // Only write palette if necessary
        }

        var posBeforeWritingBlocks = fs.Position;
        EncodePixels(bw, image);
        WriteBlockDescription(bw, image);

        var posAfter = fs.Position;
        fs.Position = posOfCommandCount;
        bw.Write((ushort)(posAfter - posBeforeWritingBlocks));
        fs.Position = posAfter;
    }

    private void WriteBlockDescription(BinaryWriter bw, Image<Rgb24> image)
    {
        var b = new HideDesc[image.Height];
        for (ushort i = 0; i < image.Height; i++)
        {
            b[i] = new HideDesc
            {
                Skip = 0xFFFF,
                Hide = 0x0000
            };
        }

        ushort cnt;
        ushort bpl;
        for (bpl = 0; bpl < 4; bpl++)
        {
            var skip = GetPixel(image, bpl) == _transparency;
            cnt = 0;
            for (ushort i = 0; i < image.Height; i++)
            {
                ushort j;
                for (j = bpl; j < image.Width; j += 4)
                {
                    var pix = image[j, i];
                    if (pix != _transparency)
                    {
                        if (j < b[i].Skip)
                        {
                            b[i].Skip = j;
                        }

                        if (j >= b[i].Hide)
                        {
                            b[i].Hide = (ushort)(j + 1);
                        }
                    }

                    if (pix == _transparency != skip || cnt >= 0x3FF0)
                    {
                        // end of block
                        skip = pix == _transparency;
                        cnt = 0;
                    }

                    cnt++;
                }
            }
        }

        cnt = 0;

        for (ushort i = 0; i < image.Height; i++)
        {
            if (b[i].Skip == 0xFFFF)
            {
                // whole line is skipped
                b[i].Skip = (ushort)((cnt + _virtualWidth) >> 2);
                cnt = 0;
            }
            else
            {
                var s = (ushort)(b[i].Skip & ~3);
                var h = (ushort)((b[i].Hide + 3) & ~3);
                b[i].Skip = (ushort)((cnt + s) >> 2);
                b[i].Hide = (ushort)((h - s) >> 2);
                cnt = (ushort)(_virtualWidth - h);
            }
        }

        foreach (var hideDesc in b)
        {
            bw.Write(BitConverter.GetBytes(hideDesc.Skip));
            bw.Write(BitConverter.GetBytes(hideDesc.Hide));
        }
    }

    private void EncodePixels(BinaryWriter bw, Image<Rgb24> image)
    {
        var endPos = _virtualWidth * _virtualHeight;

        for (var planeCtr = 0; planeCtr < 4; planeCtr++)
        {
            var pos = planeCtr;

            // "Repeat" is omitted here. Not necessary in 2023 :)
            while (pos < endPos)
            {
                var pixel = GetPixel(image, pos);

                if (pixel == _transparency)
                {
                    var count = 0;
                    while (pos < endPos && GetPixel(image, pos) == _transparency && count < 0x3FFF)
                    {
                        count++;
                        pos += 4;
                    }

                    if (pos + 4 < endPos)
                    {
                        bw.Write(BitConverter.GetBytes(
                            (ushort)((1 << 14) | count))); // SKIP command                        
                    }
                }
                else
                {
                    var countPos = bw.BaseStream.Position;
                    bw.Write((ushort)0);
                    var count = 0;

                    while (true)
                    {
                        if (pos >= endPos || count >= 0x3FFF)
                        {
                            break;
                        }

                        var curPixel = GetPixel(image, pos);
                        if (curPixel == _transparency)
                        {
                            break;
                        }

                        bw.Write(GetPaletteIndex(curPixel));
                        count++;
                        pos += 4;
                    }

                    var posAfter = bw.BaseStream.Position;
                    bw.BaseStream.Position = countPos;
                    bw.Write(BitConverter.GetBytes((ushort)((3 << 14) | count))); // COPY COMMAND
                    bw.BaseStream.Position = posAfter;
                }
            }

            bw.Write(BitConverter.GetBytes((ushort)0)); // End of Plane
        }
    }

    private Rgb24 GetPixel(Image<Rgb24> image, int pos)
    {
        var x = pos % _virtualWidth;
        var y = pos / _virtualWidth;
        return x >= image.Width || y >= image.Height ? _transparency : image[x, y];
    }

    private static void WritePalette(BinaryWriter bw, Palette embedPalette)
    {
        for (var i = 0; i < 256; i++)
        {
            var color = embedPalette[i];
            var dacColor = Utils.ColorToDac(color.R, color.G, color.B);
            bw.Write(dacColor.R);
            bw.Write(dacColor.G);
            bw.Write(dacColor.B);
        }
    }

    private byte GetPaletteIndex(Rgb24 color)
    {
        return _mappedMappedColorTable[color];
    }
}