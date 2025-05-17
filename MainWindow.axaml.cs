using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ppdproject.Models;

namespace ppdproject;

public partial class MainWindow : Window
{
    private Image<Rgba32>? img1;
    private Image<Rgba32>? img2;

    public MainWindow() => InitializeComponent();

    private async void OpenImage1_Click(object? sender, RoutedEventArgs e)
    {
        var path = await OpenFile();
        if (path != null)
        {
            img1 = ImageUtils.LoadImage(path);
            Image1.Source = await ToBitmap(img1);
        }
    }

    private async void OpenImage2_Click(object? sender, RoutedEventArgs e)
    {
        var path = await OpenFile();
        if (path != null)
        {
            img2 = ImageUtils.LoadImage(path);
            Image2.Source = await ToBitmap(img2);
        }
    }

    private async void ApplyOperation_Click(object? sender, RoutedEventArgs e)
    {
        if (img1 == null || img2 == null) return;

        var op = ((ComboBoxItem)OperationComboBox.SelectedItem!)?.Content?.ToString();
        Image<Rgba32>? result = op switch
        {
            "Adição" => ImageOperations.Add(img1, img2),
            "Subtração" => ImageOperations.Subtract(img1, img2),
            "Multiplicação" => ImageOperations.Multiply(img1, img2),
            "Divisão" => ImageOperations.Divide(img1, img2),
            "AND" => ImageOperations.And(img1, img2),
            "OR" => ImageOperations.Or(img1, img2),
            "XOR" => ImageOperations.Xor(img1, img2),
            _ => null
        };

        if (result != null)
            Image2.Source = await ToBitmap(result);
    }

    private async Task<string?> OpenFile()
    {
        var dialog = new OpenFileDialog { Filters = { new FileDialogFilter { Name = "Imagens", Extensions = { "png", "jpg", "jpeg", "bmp", "gif", "tiff", "pgm" } } } };
        var result = await dialog.ShowAsync(this);
        return result?.FirstOrDefault();
    }

    private async Task<Bitmap> ToBitmap(Image<Rgba32> img)
    {
        using var ms = new MemoryStream();
        await img.SaveAsPngAsync(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return new Bitmap(ms);
    }
}
