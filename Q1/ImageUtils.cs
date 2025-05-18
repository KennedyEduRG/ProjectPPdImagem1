using System;
using System.IO;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using NetVips;

namespace ppdproject.Models
{
    public class ImageUtils
    {
        public static SixLabors.ImageSharp.Image<Rgba32> LoadImage(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();

            if (ext == ".pgm")
            {
                var vipsImage = NetVips.Image.NewFromFile(path, access: Enums.Access.Sequential);
                int width = vipsImage.Width;
                int height = vipsImage.Height;

                if (vipsImage.Bands != 1)
                    throw new NotSupportedException("A imagem PGM deve ter apenas uma banda (grayscale).");

                byte[] buffer = vipsImage.WriteToMemory();

                var image = new SixLabors.ImageSharp.Image<Rgba32>(width, height);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte val = buffer[y * width + x];
                        image[x, y] = new Rgba32(val, val, val, 255);
                    }
                }

                return image;
            }

            using var stream = File.OpenRead(path);
            return SixLabors.ImageSharp.Image.Load<Rgba32>(stream);
        }

        public static SixLabors.ImageSharp.Image<Rgba32> Apply(SixLabors.ImageSharp.Image<Rgba32> a, SixLabors.ImageSharp.Image<Rgba32> b, Func<Rgba32, Rgba32, Rgba32> op)
        {
            int width = Math.Min(a.Width, b.Width);
            int height = Math.Min(a.Height, b.Height);

            var result = new Image<Rgba32>(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result[x, y] = op(a[x, y], b[x, y]);
                }
            }

            return result;
        }

        public static byte Clamp(int val) => (byte)Math.Clamp(val, 0, 255);
    }
}
