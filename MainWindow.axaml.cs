using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ppdproject.Models;
using ppdproject.Helpers;
using System;

namespace ppdproject;

public partial class MainWindow : Window
{
    private Image<Rgba32>? img1;
    private Image<Rgba32>? img2;
    private Bitmap? imgTransformada;

    public MainWindow() => InitializeComponent();

    private async void OpenImage1_Click(object? sender, RoutedEventArgs e)
    {
        var path = await OpenFile();
        if (path != null)
        {
            img1 = ImageUtils.LoadImage(path);
            Image1.Source = await ToBitmap(img1);
            imgTransformada = null;
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

        var selectedItem = OperationComboBox.SelectedItem as ComboBoxItem;
        if (selectedItem == null)
        {
            await MessageBox("Selecione uma operação.");
            return;
        }
        var op = selectedItem.Content?.ToString();

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

    private async void AddTransform_Click(object? sender, RoutedEventArgs e)
    {
        var selectedItem = GeoOperationComboBox.SelectedItem as ComboBoxItem;
        if (selectedItem == null)
        {
            await MessageBox("Selecione uma operação geométrica.");
            return;
        }
        var op = selectedItem.Content?.ToString();
        var p1 = Param1Box.Text;
        var p2 = Param2Box.Text;

        Bitmap current = imgTransformada ?? await ToBitmap(img1!);
        Bitmap? result = null;

        switch (op)
        {
            case "Rotacionar":
            case "Zoom In (Interp.)":
            case "Zoom In (Replicação)":
            case "Zoom Out (Valor-Médio)":
            case "Zoom Out (Exclusão)":
                if (string.IsNullOrWhiteSpace(p1))
                {
                    await MessageBox("Preencha o parâmetro 1.");
                    return;
                }
                break;
            case "Escalar":
            case "Cisalhamento":
            case "Transladar":
                if (string.IsNullOrWhiteSpace(p1) || string.IsNullOrWhiteSpace(p2))
                {
                    await MessageBox("Preencha ambos os parâmetros.");
                    return;
                }
                break;
        }

        switch (op)
        {
            case "Rotacionar":
                if (float.TryParse(p1.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float angle))
                    result = ImageSharpHelper.Rotate(current, angle);
                break;
            case "Escalar":
                if (float.TryParse(p1.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float sx) &&
                    float.TryParse(p2.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float sy))
                    result = ImageSharpHelper.Scale(current, sx, sy);
                break;
            case "Reflexão Horizontal":
                result = ImageSharpHelper.FlipHorizontal(current);
                break;
            case "Reflexão Vertical":
                result = ImageSharpHelper.FlipVertical(current);
                break;
            case "Cisalhamento":
                if (float.TryParse(p1.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float shx) &&
                    float.TryParse(p2.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float shy))
                    result = ImageSharpHelper.Shear(current, shx, shy);
                break;
            case "Transladar":
                if (int.TryParse(p1, out int dx) && int.TryParse(p2, out int dy))
                    result = ImageSharpHelper.Translate(current, dx, dy);
                break;
            case "Zoom In (Interp.)":
                if (float.TryParse(p1.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float zin))
                    result = ImageSharpHelper.ZoomIn(current, zin);
                break;
            case "Zoom In (Replicação)":
                if (float.TryParse(p1.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float zinr))
                    result = ImageSharpHelper.ZoomInReplicacao(current, zinr);
                break;
            case "Zoom Out (Valor-Médio)":
                if (float.TryParse(p1.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float zout))
                    result = ImageSharpHelper.ZoomOut(current, zout);
                break;
            case "Zoom Out (Exclusão)":
                if (float.TryParse(p1.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float zoutr))
                    result = ImageSharpHelper.ZoomOutExclusao(current, zoutr);
                break;
        }

        if (result != null)
        {
            imgTransformada = result;
            Image2.Source = imgTransformada;
        }
    }

    private async Task MessageBox(string msg)
    {
        var dlg = new Window
        {
            Width = 300,
            Height = 100,
            Content = new TextBlock { Text = msg, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center }
        };
        await dlg.ShowDialog(this);
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

    private async void ResetImage_Click(object? sender, RoutedEventArgs e)
    {
        if (img1 != null)
        {
            imgTransformada = null;
            Image2.Source = await ToBitmap(img1);
        }
    }
}
