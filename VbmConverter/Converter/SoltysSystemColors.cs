namespace CgeTools.VbmConverter.Converter;

public static class SoltysSystemColors
{
    private static readonly Rgb24 Black = new(0, 0, 0);

    private static readonly Dac[] Gst =
    {
        new() { R = 0, G = 60, B = 0 }, // 198
        new() { R = 0, G = 104, B = 0 }, // 199
        new() { R = 20, G = 172, B = 0 }, // 200
        new() { R = 82, G = 82, B = 0 }, // 201
        new() { R = 0, G = 132, B = 82 }, // 202
        new() { R = 132, G = 173, B = 82 }, // 203
        new() { R = 82, G = 0, B = 0 }, // 204
        new() { R = 206, G = 0, B = 24 }, // 205
        new() { R = 255, G = 33, B = 33 }, // 206
        new() { R = 123, G = 41, B = 0 }, // 207
        new() { R = 0, G = 41, B = 0 }, // 208
        new() { R = 0, G = 0, B = 82 }, // 209
        new() { R = 132, G = 0, B = 0 }, // 210
        new() { R = 255, G = 0, B = 0 }, // 211
        new() { R = 255, G = 66, B = 66 }, // 212
        new() { R = 148, G = 66, B = 16 }, // 213
        new() { R = 0, G = 82, B = 0 }, // 214
        new() { R = 0, G = 0, B = 132 }, // 215
        new() { R = 173, G = 0, B = 0 }, // 216
        new() { R = 255, G = 49, B = 0 }, // 217
        new() { R = 255, G = 99, B = 99 }, // 218
        new() { R = 181, G = 107, B = 49 }, // 219
        new() { R = 0, G = 132, B = 0 }, // 220
        new() { R = 0, G = 0, B = 255 }, // 221
        new() { R = 173, G = 41, B = 0 }, // 222
        new() { R = 255, G = 82, B = 0 }, // 223
        new() { R = 255, G = 132, B = 132 }, // 224
        new() { R = 214, G = 148, B = 74 }, // 225
        new() { R = 41, G = 214, B = 0 }, // 226
        new() { R = 0, G = 82, B = 173 }, // 227
        new() { R = 255, G = 214, B = 0 }, // 228
        new() { R = 247, G = 132, B = 49 }, // 229
        new() { R = 255, G = 165, B = 165 }, // 230
        new() { R = 239, G = 198, B = 123 }, // 231
        new() { R = 173, G = 214, B = 0 }, // 232
        new() { R = 0, G = 132, B = 214 }, // 233
        new() { R = 57, G = 57, B = 57 }, // 234
        new() { R = 247, G = 189, B = 74 }, // 235
        new() { R = 255, G = 198, B = 198 }, // 236
        new() { R = 255, G = 239, B = 173 }, // 237
        new() { R = 214, G = 255, B = 173 }, // 238
        new() { R = 82, G = 173, B = 255 }, // 239
        new() { R = 107, G = 107, B = 107 }, // 240
        new() { R = 247, G = 222, B = 99 }, // 241
        new() { R = 255, G = 0, B = 255 }, // 242
        new() { R = 255, G = 132, B = 255 }, // 243
        new() { R = 132, G = 132, B = 173 }, // 244
        new() { R = 148, G = 247, B = 255 }, // 245
        new() { R = 148, G = 148, B = 148 }, // 246
        new() { R = 82, G = 0, B = 82 }, // 247
        new() { R = 112, G = 68, B = 112 }, // 248
        new() { R = 176, G = 88, B = 144 }, // 249
        new() { R = 214, G = 132, B = 173 }, // 250
        new() { R = 206, G = 247, B = 255 }, // 251
        new() { R = 198, G = 198, B = 198 }, // 252
        new() { R = 0, G = 214, B = 255 }, // 253
        new() { R = 96, G = 224, B = 96 }, // 254
        new() { R = 255, G = 255, B = 255 } // 255
    };

    public static void Modify(Palette? palette)
    {
        if (palette == null)
        {
            return;
        }

        var startIdx = 256 - Gst.Length;

        for (var i = 0; i < Gst.Length; i++)
        {
            var idx = startIdx + i;
            var paletteColor = palette[idx];
            if (paletteColor != Black)
            {
                continue;
            }

            var g = Gst[i];

            palette[idx] = new Rgb24((byte)(g.R << 2), (byte)(g.G << 2), (byte)(g.B << 2));
        }
    }

    private struct Dac
    {
        public ushort R;
        public ushort G;
        public ushort B;
    }
}