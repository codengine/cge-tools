namespace CgeTools.VbmConverter.Converter;

public class VbmFile
{
    private readonly byte[] _imageData;

    public VbmFile(ushort width, ushort height, byte[] imageData, Palette? palette)
    {
        Width = width;
        Height = height;
        _imageData = imageData;
        Palette = palette;
    }

    private ushort Width { get; }
    private ushort Height { get; }
    public Palette? Palette { get; set; }

    public Image<Rgb24> ToImage(ushort virtualWidth, ushort virtualHeight, Rgb24 trans,
        out Dictionary<byte, Rgb24> usedColors)
    {
        var image = new Image<Rgb24>(virtualWidth, virtualHeight, trans);

        var srcIndex = 0;
        usedColors = new Dictionary<byte, Rgb24>();

        for (var planeCtr = 0; planeCtr < 4; planeCtr++)
        {
            var destPos = planeCtr;

            while (true)
            {
                var v = BitConverter.ToUInt16(_imageData, srcIndex);
                var cmd = v >> 14;
                var count = v & 0x3FFF;

                srcIndex += 2;

                if (cmd == 0) // End of image
                {
                    break;
                }

                while (count-- > 0)
                {
                    byte colorIndex;
                    Rgb24? color;
                    switch (cmd)
                    {
                        case 1: //SKIP
                            break;
                        case 2: //REPEAT
                            colorIndex = _imageData[srcIndex];
                            color = GetColorFromPalette(colorIndex);
                            usedColors[colorIndex] = color.Value;
                            PutPixel(image, destPos, color.Value);
                            break;
                        case 3: //COPY
                            colorIndex = _imageData[srcIndex++];
                            color = GetColorFromPalette(colorIndex);
                            usedColors[colorIndex] = color.Value;
                            PutPixel(image, destPos, color.Value);
                            break;
                    }

                    destPos += 4; // Move to next position based on plane mapping
                }

                if (cmd == 2)
                {
                    srcIndex++;
                }
            }
        }

        if (Width != virtualWidth || Height != virtualHeight)
        {
            CropImage(image);
        }

        return image;
    }

    private void CropImage(Image<Rgb24> targetImage)
    {
        // Define the rectangle for cropping
        var cropRectangle = new Rectangle(0, 0, Width, Height);

        // Apply the cropping operation
        targetImage.Mutate(x => x.Crop(cropRectangle));
    }

    private static void PutPixel(Image<Rgb24> image, int destPos, Rgb24 color)
    {
        var x = destPos % image.Width;
        var y = destPos / image.Width;
        image[x, y] = color;
    }

    private Rgb24 GetColorFromPalette(byte colorIndex)
    {
        if (Palette == null)
        {
            throw new Exception("No palette set");
        }

        return Palette[colorIndex];
    }
}