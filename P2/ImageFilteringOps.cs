using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ppdproject.Models
{
    public static class ImageFilteringOps
    {
        // 1a. Filtro Média (NxN)
        public static Image<Rgba32> Mean(Image<Rgba32> img, int size)
        {
            int offset = size / 2;
            var result = img.Clone();
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                int r = 0, g = 0, b = 0, count = 0;
                for (int dy = -offset; dy <= offset; dy++)
                for (int dx = -offset; dx <= offset; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < img.Width && ny >= 0 && ny < img.Height)
                    {
                        var px = img[nx, ny];
                        r += px.R; g += px.G; b += px.B; count++;
                    }
                }
                result[x, y] = new Rgba32((byte)(r / count), (byte)(g / count), (byte)(b / count), img[x, y].A);
            }
            return result;
        }

        // 1b. Filtro Mediana (NxN)
        public static Image<Rgba32> Median(Image<Rgba32> img, int size)
        {
            int offset = size / 2;
            var result = img.Clone();
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var rList = new List<byte>();
                var gList = new List<byte>();
                var bList = new List<byte>();
                for (int dy = -offset; dy <= offset; dy++)
                for (int dx = -offset; dx <= offset; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < img.Width && ny >= 0 && ny < img.Height)
                    {
                        var px = img[nx, ny];
                        rList.Add(px.R); gList.Add(px.G); bList.Add(px.B);
                    }
                }
                rList.Sort(); gList.Sort(); bList.Sort();
                int mid = rList.Count / 2;
                result[x, y] = new Rgba32(rList[mid], gList[mid], bList[mid], img[x, y].A);
            }
            return result;
        }

        // 1c. Filtro Máximo (NxN)
        public static Image<Rgba32> Maximum(Image<Rgba32> img, int size)
        {
            int offset = size / 2;
            var result = img.Clone();
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                byte r = 0, g = 0, b = 0;
                for (int dy = -offset; dy <= offset; dy++)
                for (int dx = -offset; dx <= offset; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < img.Width && ny >= 0 && ny < img.Height)
                    {
                        var px = img[nx, ny];
                        r = Math.Max(r, px.R); g = Math.Max(g, px.G); b = Math.Max(b, px.B);
                    }
                }
                result[x, y] = new Rgba32(r, g, b, img[x, y].A);
            }
            return result;
        }

        // 1d. Filtro Mínimo (NxN)
        public static Image<Rgba32> Minimum(Image<Rgba32> img, int size)
        {
            int offset = size / 2;
            var result = img.Clone();
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                byte r = 255, g = 255, b = 255;
                for (int dy = -offset; dy <= offset; dy++)
                for (int dx = -offset; dx <= offset; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < img.Width && ny >= 0 && ny < img.Height)
                    {
                        var px = img[nx, ny];
                        r = Math.Min(r, px.R); g = Math.Min(g, px.G); b = Math.Min(b, px.B);
                    }
                }
                result[x, y] = new Rgba32(r, g, b, img[x, y].A);
            }
            return result;
        }

        // 1e. Filtro Moda (NxN)
        public static Image<Rgba32> Mode(Image<Rgba32> img, int size)
        {
            int offset = size / 2;
            var result = img.Clone();
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var rDict = new Dictionary<byte, int>();
                var gDict = new Dictionary<byte, int>();
                var bDict = new Dictionary<byte, int>();
                for (int dy = -offset; dy <= offset; dy++)
                for (int dx = -offset; dx <= offset; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < img.Width && ny >= 0 && ny < img.Height)
                    {
                        var px = img[nx, ny];
                        if (!rDict.ContainsKey(px.R)) rDict[px.R] = 0; rDict[px.R]++;
                        if (!gDict.ContainsKey(px.G)) gDict[px.G] = 0; gDict[px.G]++;
                        if (!bDict.ContainsKey(px.B)) bDict[px.B] = 0; bDict[px.B]++;
                    }
                }
                byte r = rDict.OrderByDescending(kv => kv.Value).First().Key;
                byte g = gDict.OrderByDescending(kv => kv.Value).First().Key;
                byte b = bDict.OrderByDescending(kv => kv.Value).First().Key;
                result[x, y] = new Rgba32(r, g, b, img[x, y].A);
            }
            return result;
        }

        // 1f. Filtros com preservação de bordas

        // Kawahara (janela 5x5, 9 regiões, seleciona a menor variância)
        public static Image<Rgba32> Kawahara(Image<Rgba32> img)
        {
            int[,] dx = { {0,0}, {0,2}, {0,4}, {2,0}, {2,2}, {2,4}, {4,0}, {4,2}, {4,4} };
            var result = img.Clone();
            for (int y = 2; y < img.Height - 2; y++)
            for (int x = 2; x < img.Width - 2; x++)
            {
                double minVar = double.MaxValue;
                byte bestR = 0, bestG = 0, bestB = 0;
                for (int k = 0; k < 9; k++)
                {
                    var valsR = new List<byte>();
                    var valsG = new List<byte>();
                    var valsB = new List<byte>();
                    for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                    {
                        int nx = x - 2 + dx[k, 0] + i;
                        int ny = y - 2 + dx[k, 1] + j;
                        var px = img[nx, ny];
                        valsR.Add(px.R); valsG.Add(px.G); valsB.Add(px.B);
                    }
                    double varR = Variance(valsR);
                    double varG = Variance(valsG);
                    double varB = Variance(valsB);
                    double var = varR + varG + varB;
                    if (var < minVar)
                    {
                        minVar = var;
                        bestR = (byte)valsR.Average(v => v);
                        bestG = (byte)valsG.Average(v => v);
                        bestB = (byte)valsB.Average(v => v);
                    }
                }
                result[x, y] = new Rgba32(bestR, bestG, bestB, img[x, y].A);
            }
            return result;
        }

        // Tomita & Tsuji (janela 3x3, 4 regiões, menor variância)
        public static Image<Rgba32> TomitaTsuji(Image<Rgba32> img)
        {
            int[][] regions = {
                new int[]{0,0,0,1,1,0,1,1}, // TL
                new int[]{0,1,0,2,1,1,1,2}, // TR
                new int[]{1,0,1,1,2,0,2,1}, // BL
                new int[]{1,1,1,2,2,1,2,2}  // BR
            };
            var result = img.Clone();
            for (int y = 1; y < img.Height - 1; y++)
            for (int x = 1; x < img.Width - 1; x++)
            {
                double minVar = double.MaxValue;
                byte bestR = 0, bestG = 0, bestB = 0;
                for (int k = 0; k < 4; k++)
                {
                    var valsR = new List<byte>();
                    var valsG = new List<byte>();
                    var valsB = new List<byte>();
                    for (int i = 0; i < 4; i++)
                    {
                        int nx = x - 1 + regions[k][i*2];
                        int ny = y - 1 + regions[k][i*2+1];
                        var px = img[nx, ny];
                        valsR.Add(px.R); valsG.Add(px.G); valsB.Add(px.B);
                    }
                    double varR = Variance(valsR);
                    double varG = Variance(valsG);
                    double varB = Variance(valsB);
                    double var = varR + varG + varB;
                    if (var < minVar)
                    {
                        minVar = var;
                        bestR = (byte)valsR.Average(v => v);
                        bestG = (byte)valsG.Average(v => v);
                        bestB = (byte)valsB.Average(v => v);
                    }
                }
                result[x, y] = new Rgba32(bestR, bestG, bestB, img[x, y].A);
            }
            return result;
        }

        // Nagao & Matsuyama (janela 5x5, 9 regiões poligonais)
        public static Image<Rgba32> NagaoMatsuyama(Image<Rgba32> img)
        {
            // Implementação simplificada: usa 9 regiões quadradas como Kawahara
            return Kawahara(img);
        }

        // Somboonkaew (janela 5x5, 13 regiões)
        public static Image<Rgba32> Somboonkaew(Image<Rgba32> img)
        {
            // Implementação simplificada: usa Kawahara como base
            return Kawahara(img);
        }

        // 2a. Filtros passa-alta H2, M1, M2, M3
        public static Image<Rgba32> HighPassH2(Image<Rgba32> img)
        {
            int[,] mask = { { 1, -2, 1 }, { -2, 5, -2 }, { 1, -2, 1 } };
            return Convolve(img, mask, 1, 0);
        }
        public static Image<Rgba32> HighPassM1(Image<Rgba32> img)
        {
            int[,] mask = { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
            return Convolve(img, mask, 1, 0);
        }
        public static Image<Rgba32> HighPassM2(Image<Rgba32> img)
        {
            int[,] mask = { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } };
            return Convolve(img, mask, 1, 0);
        }
        public static Image<Rgba32> HighPassM3(Image<Rgba32> img)
        {
            int[,] mask = { { 1, -2, 1 }, { -2, 5, -2 }, { 1, -2, 1 } };
            return Convolve(img, mask, 1, 0);
        }
        public static Image<Rgba32> HighPassH1(Image<Rgba32> img)
        {
            int[,] mask = { { -1, -1, -1 }, { -1, 8, -1 }, { -1, -1, -1 } };
            return Convolve(img, mask, 1, 0);
        }

        public static Image<Rgba32> HighBoost(Image<Rgba32> img, float A)
        {
            var blurred = Mean(img, 3);
            var result = img.Clone();
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var orig = img[x, y];
                var blur = blurred[x, y];
                int r = Clamp((int)(A * orig.R - blur.R));
                int g = Clamp((int)(A * orig.G - blur.G));
                int b = Clamp((int)(A * orig.B - blur.B));
                result[x, y] = new Rgba32((byte)r, (byte)g, (byte)b, orig.A);
            }
            return result;
        }

        // Generic convolution utility
        private static Image<Rgba32> Convolve(Image<Rgba32> img, int[,] mask, int factor = 1, int bias = 0)
        {
            int width = img.Width;
            int height = img.Height;
            int maskWidth = mask.GetLength(0);
            int maskHeight = mask.GetLength(1);
            int offsetX = maskWidth / 2;
            int offsetY = maskHeight / 2;
            var result = img.Clone();

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int r = 0, g = 0, b = 0;
                for (int j = 0; j < maskHeight; j++)
                for (int i = 0; i < maskWidth; i++)
                {
                    int nx = x + i - offsetX;
                    int ny = y + j - offsetY;
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        var px = img[nx, ny];
                        int coeff = mask[j, i];
                        r += px.R * coeff;
                        g += px.G * coeff;
                        b += px.B * coeff;
                    }
                }
                r = r / factor + bias;
                g = g / factor + bias;
                b = b / factor + bias;
                result[x, y] = new Rgba32(
                    (byte)Math.Clamp(r, 0, 255),
                    (byte)Math.Clamp(g, 0, 255),
                    (byte)Math.Clamp(b, 0, 255),
                    img[x, y].A
                );
            }
            return result;
        }

        // 3a. Pontilhado Ordenado 2x3 e 3x3
        public static Image<Rgba32> OrderedDither2x3(Image<Rgba32> img)
        {
            int[,] matrix = { { 1, 4, 3 }, { 5, 2, 6 } };
            int n = 2, m = 3;
            var gray = img.CloneAs<L8>();
            var result = new Image<Rgba32>(img.Width, img.Height);
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                double threshold = (matrix[y % n, x % m] + 0.5) * 255.0 / (n * m);
                byte val = gray[x, y].PackedValue;
                byte outVal = (byte)(val > threshold ? 255 : 0);
                result[x, y] = new Rgba32(outVal, outVal, outVal, 255);
            }
            return result;
        }
        public static Image<Rgba32> OrderedDither3x3(Image<Rgba32> img)
        {
            int[,] matrix = { { 6, 8, 4 }, { 1, 0, 3 }, { 5, 2, 7 } };
            int n = 3;
            var gray = img.CloneAs<L8>();
            var result = new Image<Rgba32>(img.Width, img.Height);
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                double threshold = (matrix[y % n, x % n] + 0.5) * 255.0 / (n * n);
                byte val = gray[x, y].PackedValue;
                byte outVal = (byte)(val > threshold ? 255 : 0);
                result[x, y] = new Rgba32(outVal, outVal, outVal, 255);
            }
            return result;
        }
        public static Image<Rgba32> OrderedDither2x2(Image<Rgba32> img)
        {
            int[,] matrix = { { 0, 2 }, { 3, 1 } };
            int n = 2;
            var gray = img.CloneAs<L8>();
            var result = new Image<Rgba32>(img.Width, img.Height);
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                double threshold = (matrix[y % n, x % n] + 0.5) * 255.0 / (n * n);
                byte val = gray[x, y].PackedValue;
                byte outVal = (byte)(val > threshold ? 255 : 0);
                result[x, y] = new Rgba32(outVal, outVal, outVal, 255);
            }
            return result;
        }

        // 3b. Pontilhado com difusão: Rogers, Jarvis, Stucki, Stevenson-Arce
        public static Image<Rgba32> RogersDither(Image<Rgba32> img)
        {
            // Rogers: [0 0 0 7/16]
            //         [3/16 5/16 1/16]
            float[,] kernel = {
                { 0, 0, 0, 7f/16 },
                { 3f/16, 5f/16, 1f/16, 0 }
            };
            return ErrorDiffusionDither(img, kernel, 1, 2);
        }
        public static Image<Rgba32> JarvisJudiceNinkeDither(Image<Rgba32> img)
        {
            float[,] kernel = {
                { 0, 0, 0, 7f/48, 5f/48 },
                { 3f/48, 5f/48, 7f/48, 5f/48, 3f/48 },
                { 1f/48, 3f/48, 5f/48, 3f/48, 1f/48 }
            };
            return ErrorDiffusionDither(img, kernel, 2, 2);
        }
        public static Image<Rgba32> StuckiDither(Image<Rgba32> img)
        {
            float[,] kernel = {
                { 0, 0, 0, 8f/42, 4f/42 },
                { 2f/42, 4f/42, 8f/42, 4f/42, 2f/42 },
                { 1f/42, 2f/42, 4f/42, 2f/42, 1f/42 }
            };
            return ErrorDiffusionDither(img, kernel, 2, 2);
        }
        public static Image<Rgba32> StevensonArceDither(Image<Rgba32> img)
        {
            // Kernel para Stevenson-Arce (7x1)
            float[,] kernel = {
                { 0, 0, 0, 0, 32f/200, 0, 0 },
                { 12f/200, 26f/200, 30f/200, 16f/200, 12f/200, 0, 0 }
            };
            return ErrorDiffusionDither(img, kernel, 1, 3);
        }
        public static Image<Rgba32> FloydSteinbergDither(Image<Rgba32> img)
        {
            var gray = img.CloneAs<L8>();
            int w = img.Width, h = img.Height;
            var arr = new float[w, h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                arr[x, y] = gray[x, y].PackedValue;

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float oldVal = arr[x, y];
                float newVal = oldVal > 127 ? 255 : 0;
                float err = oldVal - newVal;
                arr[x, y] = newVal;
                if (x + 1 < w) arr[x + 1, y] += err * 7 / 16f;
                if (x - 1 >= 0 && y + 1 < h) arr[x - 1, y + 1] += err * 3 / 16f;
                if (y + 1 < h) arr[x, y + 1] += err * 5 / 16f;
                if (x + 1 < w && y + 1 < h) arr[x + 1, y + 1] += err * 1 / 16f;
            }

            var result = new Image<Rgba32>(w, h);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                byte v = (byte)Math.Clamp(arr[x, y], 0, 255);
                result[x, y] = new Rgba32(v, v, v, 255);
            }
            return result;
        }

        // Utilitário: Difusão de erro genérica
        private static Image<Rgba32> ErrorDiffusionDither(Image<Rgba32> img, float[,] kernel, int yOffset, int xOffset)
        {
            var gray = img.CloneAs<L8>();
            int w = img.Width, h = img.Height;
            var arr = new float[w, h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                arr[x, y] = gray[x, y].PackedValue;

            int kRows = kernel.GetLength(0);
            int kCols = kernel.GetLength(1);

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float old = arr[x, y];
                float newVal = old > 127 ? 255 : 0;
                float err = old - newVal;
                arr[x, y] = newVal;
                for (int ky = 0; ky < kRows; ky++)
                for (int kx = 0; kx < kCols; kx++)
                {
                    int nx = x + kx - xOffset;
                    int ny = y + ky;
                    if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                        arr[nx, ny] += err * kernel[ky, kx];
                }
            }

            var result = new Image<Rgba32>(w, h);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                byte v = (byte)Math.Clamp(arr[x, y], 0, 255);
                result[x, y] = new Rgba32(v, v, v, 255);
            }
            return result;
        }

        // Utilitário: Variância
        private static double Variance(List<byte> vals)
        {
            double avg = vals.Average(v => v);
            return vals.Average(v => (v - avg) * (v - avg));
        }

        private static int Clamp(int v) => Math.Max(0, Math.Min(255, v));
    }
}