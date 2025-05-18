using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using SkiaSharp;

namespace ppdproject.Helpers
{
    public static class ImageSharpHelper
    {
        // Converte de Bitmap Avalonia para ImageSharp
        private static Image<Rgba32> ToImageSharp(Bitmap bitmap)
        {
            using var ms = new MemoryStream();
            bitmap.Save(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return Image.Load<Rgba32>(ms);
        }

        // Converte de ImageSharp para Bitmap Avalonia
        private static Bitmap ToAvaloniaBitmap(Image<Rgba32> image)
        {
            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return new Bitmap(ms);
        }

        public static Bitmap Rotate(Bitmap original, float angle)
        {
            using var img = ToImageSharp(original);
            img.Mutate(x => x.Rotate(angle));
            return ToAvaloniaBitmap(img);
        }

        public static Bitmap Scale(Bitmap original, float scaleX, float scaleY)
        {
            using var img = ToImageSharp(original);
            int newW = (int)(img.Width * scaleX);
            int newH = (int)(img.Height * scaleY);
            img.Mutate(x => x.Resize(newW, newH));
            return ToAvaloniaBitmap(img);
        }

        public static Bitmap FlipHorizontal(Bitmap original)
        {
            using var img = ToImageSharp(original);
            img.Mutate(x => x.Flip(FlipMode.Horizontal));
            return ToAvaloniaBitmap(img);
        }

        public static Bitmap FlipVertical(Bitmap original)
        {
            using var img = ToImageSharp(original);
            img.Mutate(x => x.Flip(FlipMode.Vertical));
            return ToAvaloniaBitmap(img);
        }

        public static Bitmap Translate(Bitmap original, int dx, int dy)
        {
            using var img = ToImageSharp(original);
            var clone = new Image<Rgba32>(img.Width + dx, img.Height + dy);
            clone.Mutate(ctx => ctx.DrawImage(
                img,
                new SixLabors.ImageSharp.Point(dx, dy),
                new GraphicsOptions
                {
                    AlphaCompositionMode = PixelAlphaCompositionMode.SrcOver,
                    ColorBlendingMode = PixelColorBlendingMode.Normal,
                    BlendPercentage = 1f
                }
            ));
            return ToAvaloniaBitmap(clone);
        }

        // Zoom IN com interpolação
        public static Bitmap ZoomIn(Bitmap original, float factor)
        {
            if (factor <= 0)
                throw new ArgumentException("O fator de zoom deve ser maior que zero.");
            using var img = ToImageSharp(original);
            int newW = Math.Max(1, (int)(img.Width * factor));
            int newH = Math.Max(1, (int)(img.Height * factor));
            img.Mutate(x => x.Resize(newW, newH, KnownResamplers.NearestNeighbor));
            return ToAvaloniaBitmap(img);
        }

        // Zoom OUT com valor médio
        public static Bitmap ZoomOut(Bitmap original, float factor)
        {
            if (factor <= 0)
                throw new ArgumentException("O fator de zoom deve ser maior que zero.");
            using var img = ToImageSharp(original);
            int newW = Math.Max(1, (int)(img.Width / factor));
            int newH = Math.Max(1, (int)(img.Height / factor));
            img.Mutate(x => x.Resize(newW, newH, KnownResamplers.NearestNeighbor));
            return ToAvaloniaBitmap(img);
        }

        public static Bitmap ResizeWithSkia(Bitmap original, int newW, int newH)
        {
            using var ms = new MemoryStream();
            original.Save(ms);
            ms.Seek(0, SeekOrigin.Begin);
            using var skiaBitmap = SKBitmap.Decode(ms);
            using var resized = skiaBitmap.Resize(new SKImageInfo(newW, newH), SKFilterQuality.Medium);
            using var image = SKImage.FromBitmap(resized);
            using var outStream = new MemoryStream();
            image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(outStream);
            outStream.Seek(0, SeekOrigin.Begin);
            return new Bitmap(outStream);
        }
    }
}
