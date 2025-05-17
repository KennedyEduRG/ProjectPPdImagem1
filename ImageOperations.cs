using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
namespace ppdproject.Models{
public static class ImageOperations
{
    public static Image<Rgba32> Add(Image<Rgba32> a, Image<Rgba32> b) =>
        ImageUtils.Apply(a, b, (p1, p2) => new Rgba32(
            ImageUtils.Clamp(p1.R + p2.R),
            ImageUtils.Clamp(p1.G + p2.G),
            ImageUtils.Clamp(p1.B + p2.B)));

    public static Image<Rgba32> Subtract(Image<Rgba32> a, Image<Rgba32> b) =>
        ImageUtils.Apply(a, b, (p1, p2) => new Rgba32(
            ImageUtils.Clamp(p1.R - p2.R),
            ImageUtils.Clamp(p1.G - p2.G),
            ImageUtils.Clamp(p1.B - p2.B)));

    public static Image<Rgba32> Multiply(Image<Rgba32> a, Image<Rgba32> b) =>
        ImageUtils.Apply(a, b, (p1, p2) => new Rgba32(
            ImageUtils.Clamp(p1.R * p2.R / 255),
            ImageUtils.Clamp(p1.G * p2.G / 255),
            ImageUtils.Clamp(p1.B * p2.B / 255)));

    public static Image<Rgba32> Divide(Image<Rgba32> a, Image<Rgba32> b) =>
        ImageUtils.Apply(a, b, (p1, p2) => new Rgba32(
            p2.R == 0 ? p1.R : ImageUtils.Clamp(p1.R / p2.R),
            p2.G == 0 ? p1.G : ImageUtils.Clamp(p1.G / p2.G),
            p2.B == 0 ? p1.B : ImageUtils.Clamp(p1.B / p2.B)));

    public static Image<Rgba32> And(Image<Rgba32> a, Image<Rgba32> b) =>
        ImageUtils.Apply(a, b, (p1, p2) => new Rgba32(
            (byte)(p1.R & p2.R),
            (byte)(p1.G & p2.G),
            (byte)(p1.B & p2.B)));

    public static Image<Rgba32> Or(Image<Rgba32> a, Image<Rgba32> b) =>
        ImageUtils.Apply(a, b, (p1, p2) => new Rgba32(
            (byte)(p1.R | p2.R),
            (byte)(p1.G | p2.G),
            (byte)(p1.B | p2.B)));

    public static Image<Rgba32> Xor(Image<Rgba32> a, Image<Rgba32> b) =>
        ImageUtils.Apply(a, b, (p1, p2) => new Rgba32(
            (byte)(p1.R ^ p2.R),
            (byte)(p1.G ^ p2.G),
            (byte)(p1.B ^ p2.B)));
}
}