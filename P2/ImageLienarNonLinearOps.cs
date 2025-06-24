using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace ppdproject.Models
{
    public static class ImageLinearNonLinearOps
    {
        // 1. Transformação Linear: ajuste de brilho e contraste (y = a*x + b)
        public static Image<Rgba32> LinearTransform(Image<Rgba32> img, float a, float b)
        {
            var result = img.Clone();
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var px = img[x, y];
                byte r = Clamp(a * px.R + b);
                byte g = Clamp(a * px.G + b);
                byte b2 = Clamp(a * px.B + b);
                result[x, y] = new Rgba32(r, g, b2, px.A);
            }
            return result;
        }

        // 1. Transformação Não-Linear: Negativo
        public static Image<Rgba32> Negative(Image<Rgba32> img)
        {
            var result = img.Clone();
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var px = img[x, y];
                result[x, y] = new Rgba32((byte)(255 - px.R), (byte)(255 - px.G), (byte)(255 - px.B), px.A);
            }
            return result;
        }

        // 2. Equalização de Histograma (apenas para tons de cinza)
        public static Image<Rgba32> HistogramEqualization(Image<Rgba32> img)
        {
            var gray = img.CloneAs<L8>();
            int[] hist = new int[256];
            int w = gray.Width, h = gray.Height;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                hist[gray[x, y].PackedValue]++;

            int total = w * h;
            float[] cdf = new float[256];
            int sum = 0;
            for (int i = 0; i < 256; i++)
            {
                sum += hist[i];
                cdf[i] = sum / (float)total;
            }

            var result = new Image<Rgba32>(w, h);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                byte oldVal = gray[x, y].PackedValue;
                byte newVal = (byte)(cdf[oldVal] * 255);
                result[x, y] = new Rgba32(newVal, newVal, newVal, 255);
            }
            return result;
        }

        // 3. Correção Gama
        public static Image<Rgba32> GammaCorrection(Image<Rgba32> img, float gamma)
        {
            var result = img.Clone();
            float invGamma = 1f / gamma;
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var px = img[x, y];
                byte r = Clamp(255 * MathF.Pow(px.R / 255f, invGamma));
                byte g = Clamp(255 * MathF.Pow(px.G / 255f, invGamma));
                byte b2 = Clamp(255 * MathF.Pow(px.B / 255f, invGamma));
                result[x, y] = new Rgba32(r, g, b2, px.A);
            }
            return result;
        }

        // 4. Fatiamento de bits (extrai bits de uma faixa)
        public static Image<Rgba32> BitSlicing(Image<Rgba32> img, int bitStart, int bitEnd)
        {
            // bitStart e bitEnd: 0 (menos significativo) até 7 (mais significativo)
            var result = img.Clone();
            int mask = 0;
            for (int i = bitStart; i <= bitEnd; i++)
                mask |= (1 << i);

            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var px = img[x, y];
                byte r = (byte)((px.R & mask) << (7 - bitEnd));
                byte g = (byte)((px.G & mask) << (7 - bitEnd));
                byte b2 = (byte)((px.B & mask) << (7 - bitEnd));
                result[x, y] = new Rgba32(r, g, b2, px.A);
            }
            return result;
        }

        private static byte Clamp(float val) => (byte)Math.Clamp((int)val, 0, 255);
    }
}