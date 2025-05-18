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
using System.Collections.Generic; 

namespace ppdproject;

public partial class MainWindow : Window
{
    private Image<Rgba32>? img1;
    private Image<Rgba32>? img2;
    private Bitmap? imgTransformada; // Nova variável para armazenar a imagem transformada

    private List<Func<Bitmap, Bitmap>> compositeTransforms = new();
    private List<string> compositeDescriptions = new();

    public MainWindow() => InitializeComponent();

    private async void OpenImage1_Click(object? sender, RoutedEventArgs e)
    {
        var path = await OpenFile();
        if (path != null)
        {
            img1 = ImageUtils.LoadImage(path);
            Image1.Source = await ToBitmap(img1);
            imgTransformada = null; // limpa o estado acumulado
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

    private async void ApplyGeoOperation_Click(object? sender, RoutedEventArgs e)
    {
        if (img1 == null) return;

        var selectedItem = GeoOperationComboBox.SelectedItem as ComboBoxItem;
        if (selectedItem == null)
        {
            await MessageBox("Selecione uma operação geométrica.");
            return;
        }
        var op = selectedItem.Content?.ToString();

        var geoOp = ((ComboBoxItem)GeoOperationComboBox.SelectedItem!)?.Content?.ToString();
        Bitmap? result = null;
        var bmp = await ToBitmap(img1);

        try
        {
            switch (geoOp)
            {
                case "Rotacionar":
                    if (float.TryParse(Param1Box.Text, out float angle))
                        result = ImageSharpHelper.Rotate(bmp, angle);
                    break;
                case "Escalar":
                    if (float.TryParse(Param1Box.Text, out float sx) && float.TryParse(Param2Box.Text, out float sy))
                        result = ImageSharpHelper.Scale(bmp, sx, sy);
                    break;
                case "Espelhar Horizontal":
                    result = ImageSharpHelper.FlipHorizontal(bmp);
                    break;
                case "Espelhar Vertical":
                    result = ImageSharpHelper.FlipVertical(bmp);
                    break;
                case "Transladar":
                    if (int.TryParse(Param1Box.Text, out int dx) && int.TryParse(Param2Box.Text, out int dy))
                        result = ImageSharpHelper.Translate(bmp, dx, dy);
                    break;
                case "Zoom In":
                    if (float.TryParse(Param1Box.Text, out float zin))
                        result = ImageSharpHelper.ZoomIn(bmp, zin);
                    break;
                case "Zoom Out":
                    if (float.TryParse(Param1Box.Text, out float zout))
                        result = ImageSharpHelper.ZoomOut(bmp, zout);
                    break;
            }
        }
        catch (Exception ex)
        {
            await MessageBox("Erro: " + ex.Message);
        }

        if (result != null)
            Image2.Source = result;
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

        // Checagem de parâmetros obrigatórios
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
                {
                    compositeTransforms.Add(bmp => ImageSharpHelper.Rotate(bmp, angle));
                    compositeDescriptions.Add($"Rotacionar {angle}°");
                }
                break;
            case "Escalar":
                if (float.TryParse(p1.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float sx) &&
                    float.TryParse(p2.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float sy))
                {
                    compositeTransforms.Add(bmp => ImageSharpHelper.Scale(bmp, sx, sy));
                    compositeDescriptions.Add($"Escalar sx={sx}, sy={sy}");
                }
                break;
            case "Reflexão Horizontal":
                compositeTransforms.Add(bmp => ImageSharpHelper.FlipHorizontal(bmp));
                compositeDescriptions.Add("Reflexão Horizontal");
                break;
            case "Reflexão Vertical":
                compositeTransforms.Add(bmp => ImageSharpHelper.FlipVertical(bmp));
                compositeDescriptions.Add("Reflexão Vertical");
                break;
            case "Cisalhamento":
                if (float.TryParse(p1.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float shx) &&
                    float.TryParse(p2.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float shy))
                {
                    compositeTransforms.Add(bmp => ImageSharpHelper.Shear(bmp, shx, shy));
                    compositeDescriptions.Add($"Cisalhamento x={shx}, y={shy}");
                }
                break;
            case "Transladar":
                if (int.TryParse(p1, out int dx) && int.TryParse(p2, out int dy))
                {
                    compositeTransforms.Add(bmp => ImageSharpHelper.Translate(bmp, dx, dy));
                    compositeDescriptions.Add($"Transladar dx={dx}, dy={dy}");
                }
                break;
            case "Zoom In (Interp.)":
                if (float.TryParse(p1.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float zin))
                {
                    compositeTransforms.Add(bmp => ImageSharpHelper.ZoomIn(bmp, zin));
                    compositeDescriptions.Add($"Zoom In (Interp.) x{zin}");
                }
                break;
            case "Zoom In (Replicação)":
                if (float.TryParse(p1.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float zinr))
                {
                    compositeTransforms.Add(bmp => ImageSharpHelper.ZoomInReplicacao(bmp, zinr));
                    compositeDescriptions.Add($"Zoom In (Replicação) x{zinr}");
                }
                break;
            case "Zoom Out (Valor-Médio)":
                if (float.TryParse(p1.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float zout))
                {
                    compositeTransforms.Add(bmp => ImageSharpHelper.ZoomOut(bmp, zout));
                    compositeDescriptions.Add($"Zoom Out (Valor-Médio) x{zout}");
                }
                break;
            case "Zoom Out (Exclusão)":
                if (float.TryParse(p1.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float zoutr))
                {
                    compositeTransforms.Add(bmp => ImageSharpHelper.ZoomOutExclusao(bmp, zoutr));
                    compositeDescriptions.Add($"Zoom Out (Exclusão) x{zoutr}");
                }
                break;
        }
        TransformListBox.ItemsSource = null;
        TransformListBox.ItemsSource = compositeDescriptions.ToArray();
    }

    private void RemoveTransform_Click(object? sender, RoutedEventArgs e)
    {
        if (TransformListBox.SelectedIndex >= 0)
        {
            int idx = TransformListBox.SelectedIndex;
            compositeTransforms.RemoveAt(idx);
            compositeDescriptions.RemoveAt(idx);
            TransformListBox.ItemsSource = null;
            TransformListBox.ItemsSource = compositeDescriptions.ToArray();
        }
    }

    private async void ApplyComposite_Click(object? sender, RoutedEventArgs e)
    {
        if ((imgTransformada == null && img1 == null) || compositeTransforms.Count == 0)
            return;

        // Use a última imagem transformada, ou a original se for a primeira vez
        Bitmap current = imgTransformada ?? await ToBitmap(img1!);

        foreach (var t in compositeTransforms)
        {
            var next = t(current);
            if (current != imgTransformada && imgTransformada != null)
                current.Dispose();
            current = next;
            Image2.Source = current;
            await Task.Delay(400); // efeito visual opcional
        }

        imgTransformada = current; // guarda o último estado
        compositeTransforms.Clear();
        compositeDescriptions.Clear();
        TransformListBox.ItemsSource = null;
    }

    // Método simples para mostrar mensagem
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
}
