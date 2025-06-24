using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ppdproject.Models
{
    public static class ImageSegmentationOps
    {
        // 1) Detecção de pontos (Threshold T)
        public static Image<Rgba32> PointDetection(Image<Rgba32> img, byte T)
        {
            var gray = img.CloneAs<L8>();
            var result = new Image<Rgba32>(img.Width, img.Height);
            for (int y = 1; y < img.Height - 1; y++)
            for (int x = 1; x < img.Width - 1; x++)
            {
                int sum = 0;
                for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    sum += gray[x + dx, y + dy].PackedValue;
                int val = 9 * gray[x, y].PackedValue - sum;
                result[x, y] = (Math.Abs(val) > T) ? new Rgba32(255,255,255) : new Rgba32(0,0,0);
            }
            return result;
        }

        // 2) Detecção de retas (máscaras para cada direção)
        public static Image<Rgba32> LineDetection(Image<Rgba32> img, string direction)
        {
            int[,] mask = direction switch
            {
                "Horizontal" => new int[,] { { -1, -1, -1 }, { 2, 2, 2 }, { -1, -1, -1 } },
                "Vertical" => new int[,] { { -1, 2, -1 }, { -1, 2, -1 }, { -1, 2, -1 } },
                "45" => new int[,] { { 2, -1, -1 }, { -1, 2, -1 }, { -1, -1, 2 } },
                "135" => new int[,] { { -1, -1, 2 }, { -1, 2, -1 }, { 2, -1, -1 } },
                _ => throw new ArgumentException("Direção inválida")
            };
            return ConvolveGray(img, mask);
        }

        // 3) Detecção de bordas
        public static Image<Rgba32> Roberts(Image<Rgba32> img)
        {
            int[,] gx = { { 1, 0 }, { 0, -1 } };
            int[,] gy = { { 0, 1 }, { -1, 0 } };
            return EdgeMagnitude(img, gx, gy);
        }
        public static Image<Rgba32> RobertsCross(Image<Rgba32> img)
        {
            int[,] gx = { { 1, 0 }, { 0, -1 } };
            int[,] gy = { { 0, -1 }, { 1, 0 } };
            return EdgeMagnitude(img, gx, gy);
        }
        public static Image<Rgba32> PrewittGx(Image<Rgba32> img)
        {
            int[,] gx = { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
            return ConvolveGray(img, gx);
        }
        public static Image<Rgba32> PrewittGy(Image<Rgba32> img)
        {
            int[,] gy = { { 1, 1, 1 }, { 0, 0, 0 }, { -1, -1, -1 } };
            return ConvolveGray(img, gy);
        }
        public static Image<Rgba32> PrewittMag(Image<Rgba32> img)
        {
            int[,] gx = { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
            int[,] gy = { { 1, 1, 1 }, { 0, 0, 0 }, { -1, -1, -1 } };
            return EdgeMagnitude(img, gx, gy);
        }
        public static Image<Rgba32> SobelGx(Image<Rgba32> img)
        {
            int[,] gx = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            return ConvolveGray(img, gx);
        }
        public static Image<Rgba32> SobelGy(Image<Rgba32> img)
        {
            int[,] gy = { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
            return ConvolveGray(img, gy);
        }
        public static Image<Rgba32> SobelMag(Image<Rgba32> img)
        {
            int[,] gx = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] gy = { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
            return EdgeMagnitude(img, gx, gy);
        }
        // Krish, Robison, Frey-Chen: use máscaras específicas (exemplo para Krish)
        public static Image<Rgba32> Kirsch(Image<Rgba32> img)
        {
            int[][,] masks = {
                new int[,] { { 5, 5, 5 }, { -3, 0, -3 }, { -3, -3, -3 } },
                new int[,] { { 5, 5, -3 }, { 5, 0, -3 }, { -3, -3, -3 } },
                new int[,] { { 5, -3, -3 }, { 5, 0, -3 }, { 5, -3, -3 } },
                new int[,] { { -3, -3, -3 }, { 5, 0, -3 }, { 5, 5, -3 } },
                new int[,] { { -3, -3, -3 }, { -3, 0, -3 }, { 5, 5, 5 } },
                new int[,] { { -3, -3, -3 }, { -3, 0, 5 }, { -3, 5, 5 } },
                new int[,] { { -3, -3, 5 }, { -3, 0, 5 }, { -3, 5, 5 } },
                new int[,] { { -3, 5, 5 }, { -3, 0, 5 }, { -3, -3, 5 } }
            };
            var gray = img.CloneAs<L8>();
            var result = new Image<Rgba32>(img.Width, img.Height);
            for (int y = 1; y < img.Height - 1; y++)
            for (int x = 1; x < img.Width - 1; x++)
            {
                int maxVal = 0;
                foreach (var mask in masks)
                {
                    int sum = 0;
                    for (int j = 0; j < 3; j++)
                    for (int i = 0; i < 3; i++)
                        sum += gray[x + i - 1, y + j - 1].PackedValue * mask[j, i];
                    maxVal = Math.Max(maxVal, Math.Abs(sum));
                }
                byte v = (byte)Math.Clamp(maxVal / 15, 0, 255);
                result[x, y] = new Rgba32(v, v, v);
            }
            return result;
        }
        // Robison, Frey-Chen: similar, defina as máscaras e use o maior valor

        // Laplaciano H1 e H2
        public static Image<Rgba32> LaplacianH1(Image<Rgba32> img)
        {
            int[,] mask = { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };
            return ConvolveGray(img, mask);
        }
        public static Image<Rgba32> LaplacianH2(Image<Rgba32> img)
        {
            int[,] mask = { { 1, 1, 1 }, { 1, -8, 1 }, { 1, 1, 1 } };
            return ConvolveGray(img, mask);
        }

        // 4) Limiarização
        public static Image<Rgba32> ThresholdGlobal(Image<Rgba32> img, byte T)
        {
            var gray = img.CloneAs<L8>();
            var result = new Image<Rgba32>(img.Width, img.Height);
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                byte v = gray[x, y].PackedValue;
                result[x, y] = v > T ? new Rgba32(255,255,255) : new Rgba32(0,0,0);
            }
            return result;
        }
        public static Image<Rgba32> ThresholdLocalMean(Image<Rgba32> img, int size)
        {
            var gray = img.CloneAs<L8>();
            int offset = size / 2;
            var result = new Image<Rgba32>(img.Width, img.Height);
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                int sum = 0, count = 0;
                for (int dy = -offset; dy <= offset; dy++)
                for (int dx = -offset; dx <= offset; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < img.Width && ny >= 0 && ny < img.Height)
                    {
                        sum += gray[nx, ny].PackedValue;
                        count++;
                    }
                }
                byte mean = (byte)(sum / count);
                result[x, y] = gray[x, y].PackedValue > mean ? new Rgba32(255,255,255) : new Rgba32(0,0,0);
            }
            return result;
        }
        public static Image<Rgba32> ThresholdLocalMax(Image<Rgba32> img, int size)
        {
            var gray = img.CloneAs<L8>();
            int offset = size / 2;
            var result = new Image<Rgba32>(img.Width, img.Height);
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                byte max = 0;
                for (int dy = -offset; dy <= offset; dy++)
                for (int dx = -offset; dx <= offset; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < img.Width && ny >= 0 && ny < img.Height)
                        max = Math.Max(max, gray[nx, ny].PackedValue);
                }
                result[x, y] = gray[x, y].PackedValue > max ? new Rgba32(255,255,255) : new Rgba32(0,0,0);
            }
            return result;
        }
        public static Image<Rgba32> ThresholdLocalMin(Image<Rgba32> img, int size)
        {
            var gray = img.CloneAs<L8>();
            int offset = size / 2;
            var result = new Image<Rgba32>(img.Width, img.Height);
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                byte min = 255;
                for (int dy = -offset; dy <= offset; dy++)
                for (int dx = -offset; dx <= offset; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < img.Width && ny >= 0 && ny < img.Height)
                        min = Math.Min(min, gray[nx, ny].PackedValue);
                }
                result[x, y] = gray[x, y].PackedValue > min ? new Rgba32(255,255,255) : new Rgba32(0,0,0);
            }
            return result;
        }
        public static Image<Rgba32> ThresholdNiblack(Image<Rgba32> img, int size, double k)
        {
            var gray = img.CloneAs<L8>();
            int offset = size / 2;
            var result = new Image<Rgba32>(img.Width, img.Height);
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var vals = new List<byte>();
                for (int dy = -offset; dy <= offset; dy++)
                for (int dx = -offset; dx <= offset; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < img.Width && ny >= 0 && ny < img.Height)
                        vals.Add(gray[nx, ny].PackedValue);
                }
                double mean = vals.Average(v => v);
                double std = Math.Sqrt(vals.Average(v => (v - mean) * (v - mean)));
                double T = mean + k * std;
                result[x, y] = gray[x, y].PackedValue > T ? new Rgba32(255,255,255) : new Rgba32(0,0,0);
            }
            return result;
        }

        // 5) Segmentação de regiões: Crescimento de região (simples, 4-vizinhos)
        public static Image<Rgba32> RegionGrowing(Image<Rgba32> img, int seedX, int seedY, byte threshold)
        {
            var gray = img.CloneAs<L8>();
            var result = new Image<Rgba32>(img.Width, img.Height);
            bool[,] visited = new bool[img.Width, img.Height];
            Queue<(int x, int y)> queue = new();
            byte seedVal = gray[seedX, seedY].PackedValue;
            queue.Enqueue((seedX, seedY));
            visited[seedX, seedY] = true;
            var region = new List<(int x, int y)>();

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                region.Add((x, y));
                foreach (var (dx, dy) in new[] { (0,1), (1,0), (0,-1), (-1,0) })
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < img.Width && ny >= 0 && ny < img.Height && !visited[nx, ny])
                    {
                        byte val = gray[nx, ny].PackedValue;
                        if (Math.Abs(val - seedVal) <= threshold)
                        {
                            queue.Enqueue((nx, ny));
                            visited[nx, ny] = true;
                        }
                    }
                }
            }
            // Pseudocolorir a região
            foreach (var (x, y) in region)
                result[x, y] = new Rgba32(255, 0, 0); // vermelho para a região
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
                if (result[x, y].R == 0 && result[x, y].G == 0 && result[x, y].B == 0)
                    result[x, y] = new Rgba32(gray[x, y].PackedValue, gray[x, y].PackedValue, gray[x, y].PackedValue);
            return result;
        }

        /// <summary>
        /// Algoritmo Watershed didático: segmenta regiões e destaca linhas de contenção.
        /// Cada região recebe uma cor diferente, linhas de watershed ficam pretas.
        /// </summary>
        public static Image<Rgba32> WatershedLines(Image<Rgba32> img)
        {
            // 1. Converter para tons de cinza
            var gray = img.CloneAs<L8>();
            int w = img.Width, h = img.Height;
            var gradient = new float[w, h];

            // 2. Calcular gradiente (Sobel)
            int[,] gx = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] gy = { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
            for (int y = 1; y < h - 1; y++)
            for (int x = 1; x < w - 1; x++)
            {
                int sumx = 0, sumy = 0;
                for (int j = 0; j < 3; j++)
                for (int i = 0; i < 3; i++)
                {
                    int val = gray[x + i - 1, y + j - 1].PackedValue;
                    sumx += val * gx[j, i];
                    sumy += val * gy[j, i];
                }
                gradient[x, y] = (float)Math.Sqrt(sumx * sumx + sumy * sumy);
            }

            // 3. Encontrar mínimos locais (sementes)
            int[,] labels = new int[w, h];
            int currentLabel = 1;
            for (int y = 1; y < h - 1; y++)
            for (int x = 1; x < w - 1; x++)
            {
                bool isMin = true;
                float val = gradient[x, y];
                for (int dy = -1; dy <= 1 && isMin; dy++)
                for (int dx = -1; dx <= 1 && isMin; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    if (gradient[x + dx, y + dy] < val)
                        isMin = false;
                }
                if (isMin)
                    labels[x, y] = currentLabel++;
            }

            // 4. Watershed por inundação (fila de prioridade)
            var queue = new SortedSet<(float grad, int x, int y)>();
            for (int y = 1; y < h - 1; y++)
            for (int x = 1; x < w - 1; x++)
                if (labels[x, y] > 0)
                    queue.Add((gradient[x, y], x, y));

            int[] dxs = { -1, 0, 1, 0, -1, -1, 1, 1 };
            int[] dys = { 0, -1, 0, 1, -1, 1, -1, 1 };
            int WSHED = -1;

            while (queue.Count > 0)
            {
                var (g, x, y) = queue.Min;
                queue.Remove(queue.Min);

                for (int d = 0; d < 8; d++)
                {
                    int nx = x + dxs[d], ny = y + dys[d];
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                    if (labels[nx, ny] == 0)
                    {
                        labels[nx, ny] = labels[x, y];
                        queue.Add((gradient[nx, ny], nx, ny));
                    }
                    else if (labels[nx, ny] != labels[x, y] && labels[nx, ny] != WSHED)
                    {
                        labels[x, y] = WSHED; // Watershed line
                    }
                }
            }

            // 5. Colorir resultado: cada região com cor, watershed em preto
            var result = new Image<Rgba32>(w, h);
            var colors = new Dictionary<int, Rgba32>();
            var rand = new Random(0);
            colors[0] = new Rgba32(0, 0, 0); // fundo
            colors[WSHED] = new Rgba32(0, 0, 0); // watershed lines em preto
            for (int i = 1; i < currentLabel; i++)
                colors[i] = new Rgba32((byte)rand.Next(50, 255), (byte)rand.Next(50, 255), (byte)rand.Next(50, 255));

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                result[x, y] = colors.ContainsKey(labels[x, y]) ? colors[labels[x, y]] : new Rgba32(0, 0, 0);

            return result;
        }

        // Utilitários
        private static Image<Rgba32> ConvolveGray(Image<Rgba32> img, int[,] mask)
        {
            var gray = img.CloneAs<L8>();
            int w = img.Width, h = img.Height;
            int mw = mask.GetLength(0), mh = mask.GetLength(1);
            int ox = mw / 2, oy = mh / 2;
            var result = new Image<Rgba32>(w, h);
            for (int y = oy; y < h - oy; y++)
            for (int x = ox; x < w - ox; x++)
            {
                int sum = 0;
                for (int j = 0; j < mh; j++)
                for (int i = 0; i < mw; i++)
                    sum += gray[x + i - ox, y + j - oy].PackedValue * mask[j, i];
                byte v = (byte)Math.Clamp(Math.Abs(sum) / (mw * mh), 0, 255);
                result[x, y] = new Rgba32(v, v, v);
            }
            return result;
        }
        private static Image<Rgba32> EdgeMagnitude(Image<Rgba32> img, int[,] gx, int[,] gy)
        {
            var gray = img.CloneAs<L8>();
            int w = img.Width, h = img.Height;
            int mw = gx.GetLength(0), mh = gx.GetLength(1);
            int ox = mw / 2, oy = mh / 2;
            var result = new Image<Rgba32>(w, h);
            for (int y = oy; y < h - oy; y++)
            for (int x = ox; x < w - ox; x++)
            {
                int sumx = 0, sumy = 0;
                for (int j = 0; j < mh; j++)
                for (int i = 0; i < mw; i++)
                {
                    sumx += gray[x + i - ox, y + j - oy].PackedValue * gx[i, j];
                    sumy += gray[x + i - ox, y + j - oy].PackedValue * gy[i, j];
                }
                byte v = (byte)Math.Clamp(Math.Sqrt(sumx * sumx + sumy * sumy) / (mw * mh), 0, 255);
                result[x, y] = new Rgba32(v, v, v);
            }
            return result;
        }
        public static Image<Rgba32> Robinson(Image<Rgba32> img)
        {
            int[][,] masks = {
                // North
                new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } },
                // North-East
                new int[,] { { 2, 1, 0 }, { 1, 0, -1 }, { 0, -1, -2 } },
                // East
                new int[,] { { 1, 0, -1 }, { 2, 0, -2 }, { 1, 0, -1 } },
                // South-East
                new int[,] { { 0, -1, -2 }, { 1, 0, -1 }, { 2, 1, 0 } },
                // South
                new int[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } },
                // South-West
                new int[,] { { -2, -1, 0 }, { -1, 0, 1 }, { 0, 1, 2 } },
                // West
                new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } },
                // North-West
                new int[,] { { 0, 1, 2 }, { -1, 0, 1 }, { -2, -1, 0 } }
            };

            var gray = img.CloneAs<L8>();
            var result = new Image<Rgba32>(img.Width, img.Height);

            for (int y = 1; y < img.Height - 1; y++)
            for (int x = 1; x < img.Width - 1; x++)
            {
                int maxVal = 0;
                foreach (var mask in masks)
                {
                    int sum = 0;
                    for (int j = 0; j < 3; j++)
                    for (int i = 0; i < 3; i++)
                        sum += gray[x + i - 1, y + j - 1].PackedValue * mask[j, i];
                    maxVal = Math.Max(maxVal, Math.Abs(sum));
                }
                byte v = (byte)Math.Clamp(maxVal / 8, 0, 255);
                result[x, y] = new Rgba32(v, v, v);
            }
            return result;
        }
        public static Image<Rgba32> FreyChen(Image<Rgba32> img)
        {
            int[][,] masks = {
                new int[,] { { -1, -1, 2 }, { -1, 2, -1 }, { 2, -1, -1 } }, // 0°
                new int[,] { { -1, 2, -1 }, { 2, -1, -1 }, { -1, -1, 2 } }, // 45°
                new int[,] { { 2, -1, -1 }, { -1, -1, 2 }, { -1, 2, -1 } }, // 90°
                new int[,] { { -1, -1, 2 }, { -1, 2, -1 }, { 2, -1, -1 } }, // 135°
                new int[,] { { -1, 2, -1 }, { 2, -1, -1 }, { -1, -1, 2 } }, // 180°
                new int[,] { { 2, -1, -1 }, { -1, -1, 2 }, { -1, 2, -1 } }, // 225°
                new int[,] { { -1, -1, 2 }, { -1, 2, -1 }, { 2, -1, -1 } }, // 270°
                new int[,] { { -1, 2, -1 }, { 2, -1, -1 }, { -1, -1, 2 } }  // 315°
            };

            var gray = img.CloneAs<L8>();
            var result = new Image<Rgba32>(img.Width, img.Height);

            for (int y = 1; y < img.Height - 1; y++)
            for (int x = 1; x < img.Width - 1; x++)
            {
                int maxVal = 0;
                foreach (var mask in masks)
                {
                    int sum = 0;
                    for (int j = 0; j < 3; j++)
                    for (int i = 0; i < 3; i++)
                        sum += gray[x + i - 1, y + j - 1].PackedValue * mask[j, i];
                    maxVal = Math.Max(maxVal, Math.Abs(sum));
                }
                byte v = (byte)Math.Clamp(maxVal / 8, 0, 255);
                result[x, y] = new Rgba32(v, v, v);
            }
            return result;
        }
    }
}