using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace ppdproject.Helpers
{
    public static class ColorDecomposer
    {
        public static (Image<L8> R, Image<L8> G, Image<L8> B) DecomposeRGB(Image<Rgba32> img)
        {
            var r = new Image<L8>(img.Width, img.Height);
            var g = new Image<L8>(img.Width, img.Height);
            var b = new Image<L8>(img.Width, img.Height);

            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var px = img[x, y];
                r[x, y] = new L8(px.R);
                g[x, y] = new L8(px.G);
                b[x, y] = new L8(px.B);
            }
            return (r, g, b);
        }

        public static Image<Rgba32> ComposeRGB(Image<L8> r, Image<L8> g, Image<L8> b)
        {
            int w = r.Width, h = r.Height;
            var img = new Image<Rgba32>(w, h);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                img[x, y] = new Rgba32(r[x, y].PackedValue, g[x, y].PackedValue, b[x, y].PackedValue, 255);
            return img;
        }

        public static (Image<L8> C, Image<L8> M, Image<L8> Y) DecomposeCMY(Image<Rgba32> img)
        {
            var c = new Image<L8>(img.Width, img.Height);
            var m = new Image<L8>(img.Width, img.Height);
            var y = new Image<L8>(img.Width, img.Height);

            for (int y0 = 0; y0 < img.Height; y0++)
            for (int x = 0; x < img.Width; x++)
            {
                var px = img[x, y0];
                c[x, y0] = new L8((byte)(255 - px.R));
                m[x, y0] = new L8((byte)(255 - px.G));
                y[x, y0] = new L8((byte)(255 - px.B));
            }
            return (c, m, y);
        }

        public static (Image<L8> H, Image<L8> S, Image<L8> V) DecomposeHSV(Image<Rgba32> img)
        {
            var h = new Image<L8>(img.Width, img.Height);
            var s = new Image<L8>(img.Width, img.Height);
            var v = new Image<L8>(img.Width, img.Height);

            for (int y0 = 0; y0 < img.Height; y0++)
            for (int x = 0; x < img.Width; x++)
            {
                var px = img[x, y0];
                float r = px.R / 255f, g = px.G / 255f, b = px.B / 255f;
                float max = MathF.Max(r, MathF.Max(g, b));
                float min = MathF.Min(r, MathF.Min(g, b));
                float delta = max - min;

                float hue = 0;
                if (delta != 0)
                {
                    if (max == r) hue = 60 * (((g - b) / delta) % 6);
                    else if (max == g) hue = 60 * (((b - r) / delta) + 2);
                    else hue = 60 * (((r - g) / delta) + 4);
                }
                if (hue < 0) hue += 360;
                float sat = max == 0 ? 0 : delta / max;
                float val = max;

                h[x, y0] = new L8((byte)(hue / 360 * 255));
                s[x, y0] = new L8((byte)(sat * 255));
                v[x, y0] = new L8((byte)(val * 255));
            }
            return (h, s, v);
        }

        public static (Image<L8> Y, Image<L8> U, Image<L8> V) DecomposeYUV(Image<Rgba32> img)
        {
            var yimg = new Image<L8>(img.Width, img.Height);
            var uimg = new Image<L8>(img.Width, img.Height);
            var vimg = new Image<L8>(img.Width, img.Height);

            for (int y0 = 0; y0 < img.Height; y0++)
            for (int x = 0; x < img.Width; x++)
            {
                var px = img[x, y0];
                float r = px.R, g = px.G, b = px.B;
                float yval = 0.299f * r + 0.587f * g + 0.114f * b;
                float uval = -0.14713f * r - 0.28886f * g + 0.436f * b + 128;
                float vval = 0.615f * r - 0.51499f * g - 0.10001f * b + 128;
                yimg[x, y0] = new L8((byte)Math.Clamp(yval, 0, 255));
                uimg[x, y0] = new L8((byte)Math.Clamp(uval, 0, 255));
                vimg[x, y0] = new L8((byte)Math.Clamp(vval, 0, 255));
            }
            return (yimg, uimg, vimg);
        }

        public static (Image<L8> C, Image<L8> M, Image<L8> Y, Image<L8> K) DecomposeCMYK(Image<Rgba32> img)
        {
            var c = new Image<L8>(img.Width, img.Height);
            var m = new Image<L8>(img.Width, img.Height);
            var y = new Image<L8>(img.Width, img.Height);
            var k = new Image<L8>(img.Width, img.Height);

            for (int i = 0; i < img.Height; i++)
            for (int j = 0; j < img.Width; j++)
            {
                var px = img[j, i];
                float r = px.R / 255f, g = px.G / 255f, b = px.B / 255f;
                float kVal = 1 - MathF.Max(r, MathF.Max(g, b));
                float cVal = (1 - r - kVal) / (1 - kVal + 1e-6f);
                float mVal = (1 - g - kVal) / (1 - kVal + 1e-6f);
                float yVal = (1 - b - kVal) / (1 - kVal + 1e-6f);

                c[j, i] = new L8((byte)(cVal * 255));
                m[j, i] = new L8((byte)(mVal * 255));
                y[j, i] = new L8((byte)(yVal * 255));
                k[j, i] = new L8((byte)(kVal * 255));
            }
            return (c, m, y, k);
        }

        public static (Image<L8> X, Image<L8> Y, Image<L8> Z) DecomposeXYZ(Image<Rgba32> img)
        {
            var xImg = new Image<L8>(img.Width, img.Height);
            var yImg = new Image<L8>(img.Width, img.Height);
            var zImg = new Image<L8>(img.Width, img.Height);

            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var px = img[x, y];
                double r = px.R / 255.0, g = px.G / 255.0, b = px.B / 255.0;
                r = r > 0.04045 ? Math.Pow((r + 0.055) / 1.055, 2.4) : r / 12.92;
                g = g > 0.04045 ? Math.Pow((g + 0.055) / 1.055, 2.4) : g / 12.92;
                b = b > 0.04045 ? Math.Pow((b + 0.055) / 1.055, 2.4) : b / 12.92;
                double X = r * 0.4124 + g * 0.3576 + b * 0.1805;
                double Y = r * 0.2126 + g * 0.7152 + b * 0.0722;
                double Z = r * 0.0193 + g * 0.1192 + b * 0.9505;
                xImg[x, y] = new L8((byte)Math.Clamp(X * 255, 0, 255));
                yImg[x, y] = new L8((byte)Math.Clamp(Y * 255, 0, 255));
                zImg[x, y] = new L8((byte)Math.Clamp(Z * 255, 0, 255));
            }
            return (xImg, yImg, zImg);
        }

        public static (Image<L8> H, Image<L8> S, Image<L8> L) DecomposeHSL(Image<Rgba32> img)
        {
            var hImg = new Image<L8>(img.Width, img.Height);
            var sImg = new Image<L8>(img.Width, img.Height);
            var lImg = new Image<L8>(img.Width, img.Height);

            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var px = img[x, y];
                double r = px.R / 255.0, g = px.G / 255.0, b = px.B / 255.0;
                double max = Math.Max(r, Math.Max(g, b));
                double min = Math.Min(r, Math.Min(g, b));
                double h = 0, s, l = (max + min) / 2.0;

                if (max == min)
                {
                    h = s = 0; 
                }
                else
                {
                    double d = max - min;
                    s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);
                    if (max == r)
                        h = (g - b) / d + (g < b ? 6 : 0);
                    else if (max == g)
                        h = (b - r) / d + 2;
                    else
                        h = (r - g) / d + 4;
                    h /= 6;
                }
                hImg[x, y] = new L8((byte)(h * 255));
                sImg[x, y] = new L8((byte)(s * 255));
                lImg[x, y] = new L8((byte)(l * 255));
            }
            return (hImg, sImg, lImg);
        }

        public static (Image<L8> Y, Image<L8> Cb, Image<L8> Cr) DecomposeYCbCr(Image<Rgba32> img)
        {
            var yImg = new Image<L8>(img.Width, img.Height);
            var cbImg = new Image<L8>(img.Width, img.Height);
            var crImg = new Image<L8>(img.Width, img.Height);

            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var px = img[x, y];
                double r = px.R, g = px.G, b = px.B;
                double Y =  0.299 * r + 0.587 * g + 0.114 * b;
                double Cb = 128 - 0.168736 * r - 0.331264 * g + 0.5 * b;
                double Cr = 128 + 0.5 * r - 0.418688 * g - 0.081312 * b;
                yImg[x, y] = new L8((byte)Math.Clamp(Y, 0, 255));
                cbImg[x, y] = new L8((byte)Math.Clamp(Cb, 0, 255));
                crImg[x, y] = new L8((byte)Math.Clamp(Cr, 0, 255));
            }
            return (yImg, cbImg, crImg);
        }

        public static (Image<L8> Y, Image<L8> I, Image<L8> Q) DecomposeYIQ(Image<Rgba32> img)
        {
            var yImg = new Image<L8>(img.Width, img.Height);
            var iImg = new Image<L8>(img.Width, img.Height);
            var qImg = new Image<L8>(img.Width, img.Height);

            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var px = img[x, y];
                double r = px.R / 255.0, g = px.G / 255.0, b = px.B / 255.0;
                double Y = 0.299 * r + 0.587 * g + 0.114 * b;
                double I = 0.596 * r - 0.274 * g - 0.322 * b;
                double Q = 0.211 * r - 0.523 * g + 0.312 * b;
                yImg[x, y] = new L8((byte)Math.Clamp(Y * 255, 0, 255));
                iImg[x, y] = new L8((byte)Math.Clamp((I + 0.5957) / (2 * 0.5957) * 255, 0, 255)); // Normaliza para [0,255]
                qImg[x, y] = new L8((byte)Math.Clamp((Q + 0.5226) / (2 * 0.5226) * 255, 0, 255)); // Normaliza para [0,255]
            }
            return (yImg, iImg, qImg);
        }
    }
}