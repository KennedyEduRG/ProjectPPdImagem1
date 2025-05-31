using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;

namespace ppdproject.Helpers
{
    public static class PseudoColorizer
    {
        public static Image<Rgba32> SliceAndColor(Image<L8> gray, List<(byte min, byte max, Rgba32 color)> slices)
        {
            var img = new Image<Rgba32>(gray.Width, gray.Height);
            for (int y = 0; y < gray.Height; y++)
            for (int x = 0; x < gray.Width; x++)
            {
                byte val = gray[x, y].PackedValue;
                Rgba32 color = new Rgba32(val, val, val);
                foreach (var (min, max, c) in slices)
                {
                    if (val >= min && val <= max)
                    {
                        color = c;
                        break;
                    }
                }
                img[x, y] = color;
            }
            return img;
        }

        public static Image<Rgba32> RedistributeColors(Image<Rgba32> img)
        {
            var outImg = new Image<Rgba32>(img.Width, img.Height);
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var px = img[x, y];
                byte brightness = (byte)((px.R + px.G + px.B) / 3);

                Rgba32 newColor;
                if (brightness < 85)
                    newColor = new Rgba32(0, 0, 160);
                else if (brightness < 170)
                    newColor = new Rgba32(0, 160, 0);
                else
                    newColor = new Rgba32(160, 57, 0);

                outImg[x, y] = newColor;
            }
            return outImg;
        }
    }
}