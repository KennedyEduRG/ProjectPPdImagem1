using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
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
    private Image<Rgba32>? imgQ3;

    public MainWindow() => InitializeComponent();

    private async void OpenImage1_Click(object? sender, RoutedEventArgs e)
    {
        var path = await OpenFile();
        if (path != null)
        {
            img1 = ImageUtils.LoadImage(path);
            Image1.Source = await ToBitmap(img1);
            imgTransformada = null;
            ImageOriginalQ3.Source = await ToBitmap(img1); // Atualiza Q3 também!
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
                    result = ImageSharpHelper.ZoomOutValorMedio(current, zout);
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

    private async void OpenImageQ3_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filters = { new FileDialogFilter { Name = "Imagens", Extensions = { "png", "jpg", "jpeg", "bmp", "gif", "tiff", "pgm" } } } };
        var result = await dialog.ShowAsync(this);
        var path = result?.FirstOrDefault();
        if (path != null)
        {
            imgQ3 = ImageUtils.LoadImage(path);
            ImageOriginalQ3.Source = await ToBitmap(imgQ3);
        }
    }



    private async void RunQ3Operation_Click(object? sender, RoutedEventArgs e)
    {
        if (img1 == null) return;
        var selected = Q3OperationComboBox.SelectedItem as ComboBoxItem;
        if (selected == null) return;
        var op = selected.Content?.ToString();

        switch (op)
        {
            case "Mostrar RGB":
                {
                    var (r, g, b) = ColorDecomposer.DecomposeRGB(img1);

                    var imgR = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgG = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgB = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);

                    for (int y = 0; y < imgQ3.Height; y++)
                    for (int x = 0; x < imgQ3.Width; x++)
                    {
                        imgR[x, y] = new Rgba32(r[x, y].PackedValue, 0, 0);
                        imgG[x, y] = new Rgba32(0, g[x, y].PackedValue, 0);
                        imgB[x, y] = new Rgba32(0, 0, b[x, y].PackedValue);
                    }

                    ImageA.Source = await ToBitmap(imgR);
                    ImageB.Source = await ToBitmap(imgG);
                    ImageC.Source = await ToBitmap(imgB);
                    ImageD.Source = null;
                    break;
                }
            case "Mostrar CMY":
                {
                    var (c, m, y) = ColorDecomposer.DecomposeCMY(img1);

                    var imgC = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgM = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgY = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);

                    for (int i = 0; i < imgQ3.Height; i++)
                    for (int j = 0; j < imgQ3.Width; j++)
                    {
                        imgC[j, i] = new Rgba32(0, c[j, i].PackedValue, c[j, i].PackedValue); // Ciano = G+B
                        imgM[j, i] = new Rgba32(m[j, i].PackedValue, 0, m[j, i].PackedValue); // Magenta = R+B
                        imgY[j, i] = new Rgba32(y[j, i].PackedValue, y[j, i].PackedValue, 0); // Amarelo = R+G
                    }

                    ImageA.Source = await ToBitmap(imgC);
                    ImageB.Source = await ToBitmap(imgM);
                    ImageC.Source = await ToBitmap(imgY);
                    ImageD.Source = null;
                    break;
                }
            case "Mostrar CMYK":
                {
                    var (c, m, y, k) = ColorDecomposer.DecomposeCMYK(img1);

                    var imgC = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgM = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgY = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgK = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);

                    for (int i = 0; i < imgQ3.Height; i++)
                    for (int j = 0; j < imgQ3.Width; j++)
                    {
                        imgC[j, i] = new Rgba32(0, c[j, i].PackedValue, c[j, i].PackedValue); // Ciano
                        imgM[j, i] = new Rgba32(m[j, i].PackedValue, 0, m[j, i].PackedValue); // Magenta
                        imgY[j, i] = new Rgba32(y[j, i].PackedValue, y[j, i].PackedValue, 0); // Amarelo
                        imgK[j, i] = new Rgba32(k[j, i].PackedValue, k[j, i].PackedValue, k[j, i].PackedValue); // Preto
                    }

                    ImageA.Source = await ToBitmap(imgC);
                    ImageB.Source = await ToBitmap(imgM);
                    ImageC.Source = await ToBitmap(imgY);
                    ImageD.Source = await ToBitmap(imgK);
                    break;
                }
            case "Mostrar HSV":
                {
                    var (h, s, v) = ColorDecomposer.DecomposeHSV(img1);

                    var imgH = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgS = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgV = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);

                    for (int y = 0; y < imgQ3.Height; y++)
                    for (int x = 0; x < imgQ3.Width; x++)
                    {
                        imgH[x, y] = ColorFromHSV(h[x, y].PackedValue * 360.0 / 255.0, 1, 1);
                        imgS[x, y] = new Rgba32(s[x, y].PackedValue, s[x, y].PackedValue, s[x, y].PackedValue);
                        imgV[x, y] = new Rgba32(v[x, y].PackedValue, v[x, y].PackedValue, v[x, y].PackedValue);
                    }

                    ImageA.Source = await ToBitmap(imgH);
                    ImageB.Source = await ToBitmap(imgS);
                    ImageC.Source = await ToBitmap(imgV);
                    ImageD.Source = null;
                    break;
                }
            case "Mostrar YUV":
                {
                    var (y, u, v) = ColorDecomposer.DecomposeYUV(img1);

                    var imgY = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgU = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgV = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);

                    for (int i = 0; i < imgQ3.Height; i++)
                    for (int j = 0; j < imgQ3.Width; j++)
                    {
                        imgY[j, i] = new Rgba32(y[j, i].PackedValue, y[j, i].PackedValue, y[j, i].PackedValue);
                        imgU[j, i] = new Rgba32(0, u[j, i].PackedValue, u[j, i].PackedValue); // Azul-verde
                        imgV[j, i] = new Rgba32(v[j, i].PackedValue, 0, v[j, i].PackedValue); // Vermelho-azul
                    }

                    ImageA.Source = await ToBitmap(imgY);
                    ImageB.Source = await ToBitmap(imgU);
                    ImageC.Source = await ToBitmap(imgV);
                    ImageD.Source = null;
                    break;
                }
            case "Mostrar HSL":
                {
                    var (h, s, l) = ColorDecomposer.DecomposeHSL(img1);

                    var imgH = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgS = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgL = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);

                    for (int y = 0; y < imgQ3.Height; y++)
                    for (int x = 0; x < imgQ3.Width; x++)
                    {
                        imgH[x, y] = ColorFromHSV(h[x, y].PackedValue * 360.0 / 255.0, 1, 1);
                        imgS[x, y] = new Rgba32(s[x, y].PackedValue, s[x, y].PackedValue, s[x, y].PackedValue);
                        imgL[x, y] = new Rgba32(l[x, y].PackedValue, l[x, y].PackedValue, l[x, y].PackedValue);
                    }

                    ImageA.Source = await ToBitmap(imgH);
                    ImageB.Source = await ToBitmap(imgS);
                    ImageC.Source = await ToBitmap(imgL);
                    ImageD.Source = null;
                    break;
                }
            case "Mostrar XYZ":
                {
                    var (xImg, yImg, zImg) = ColorDecomposer.DecomposeXYZ(img1);

                    var imgX = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgY = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgZ = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);

                    for (int y = 0; y < imgQ3.Height; y++)
                    for (int x = 0; x < imgQ3.Width; x++)
                    {
                        imgX[x, y] = new Rgba32(xImg[x, y].PackedValue, 0, 0);
                        imgY[x, y] = new Rgba32(0, yImg[x, y].PackedValue, 0);
                        imgZ[x, y] = new Rgba32(0, 0, zImg[x, y].PackedValue);
                    }

                    ImageA.Source = await ToBitmap(imgX);
                    ImageB.Source = await ToBitmap(imgY);
                    ImageC.Source = await ToBitmap(imgZ);
                    ImageD.Source = null;
                    break;
                }
            case "Mostrar YCbCr":
                {
                    var (yImg, cbImg, crImg) = ColorDecomposer.DecomposeYCbCr(img1);

                    var imgY = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgCb = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgCr = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);

                    for (int y = 0; y < imgQ3.Height; y++)
                    for (int x = 0; x < imgQ3.Width; x++)
                    {
                        imgY[x, y] = new Rgba32(yImg[x, y].PackedValue, yImg[x, y].PackedValue, yImg[x, y].PackedValue);
                        imgCb[x, y] = new Rgba32(0, cbImg[x, y].PackedValue, cbImg[x, y].PackedValue);
                        imgCr[x, y] = new Rgba32(crImg[x, y].PackedValue, 0, crImg[x, y].PackedValue);
                    }

                    ImageA.Source = await ToBitmap(imgY);
                    ImageB.Source = await ToBitmap(imgCb);
                    ImageC.Source = await ToBitmap(imgCr);
                    ImageD.Source = null;
                    break;
                }
            case "Mostrar YIQ":
                {
                    var (yImg, iImg, qImg) = ColorDecomposer.DecomposeYIQ(img1);

                    var imgY = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgI = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);
                    var imgQ = new Image<Rgba32>(imgQ3.Width, imgQ3.Height);

                    for (int y = 0; y < imgQ3.Height; y++)
                    for (int x = 0; x < imgQ3.Width; x++)
                    {
                        imgY[x, y] = new Rgba32(yImg[x, y].PackedValue, yImg[x, y].PackedValue, yImg[x, y].PackedValue);
                        imgI[x, y] = new Rgba32(iImg[x, y].PackedValue, 0, iImg[x, y].PackedValue);
                        imgQ[x, y] = new Rgba32(0, qImg[x, y].PackedValue, qImg[x, y].PackedValue);
                    }

                    ImageA.Source = await ToBitmap(imgY);
                    ImageB.Source = await ToBitmap(imgI);
                    ImageC.Source = await ToBitmap(imgQ);
                    ImageD.Source = null;
                    break;
                }
            case "Pseudocolorizar (Fatiamento)":
                {
                    var gray = imgQ3.CloneAs<L8>();
                    var slices = new List<(byte, byte, Rgba32)>
                    {
                        (0, 60, new Rgba32(160,57,0)),
                        (61,120, new Rgba32(0,160,0)),
                        (121,180, new Rgba32(0,0,160)),
                        (181,255, new Rgba32(255,255,0))
                    };
                    var pseudo = PseudoColorizer.SliceAndColor(gray, slices);
                    ImageA.Source = await ToBitmap(pseudo);
                    ImageB.Source = null;
                    ImageC.Source = null;
                    ImageD.Source = null;
                    break;
                }
            case "Pseudocolorizar (Redistribuição)":
                {
                    var pseudo = PseudoColorizer.RedistributeColors(imgQ3);
                    ImageA.Source = await ToBitmap(pseudo);
                    ImageB.Source = null;
                    ImageC.Source = null;
                    ImageD.Source = null;
                    break;
                }
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

    private async Task<Bitmap> ToBitmap(Image<L8> img)
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

    // Adicione também este método utilitário para HSV/HSL:
    private Rgba32 ColorFromHSV(double hue, double saturation, double value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);

        value = value * 255;
        byte v = (byte)value;
        byte p = (byte)(value * (1 - saturation));
        byte q = (byte)(value * (1 - f * saturation));
        byte t = (byte)(value * (1 - (1 - f) * saturation));

        return hi switch
        {
            0 => new Rgba32(v, t, p),
            1 => new Rgba32(q, v, p),
            2 => new Rgba32(p, v, t),
            3 => new Rgba32(p, q, v),
            4 => new Rgba32(t, p, v),
            _ => new Rgba32(v, p, q),
        };
    }
}
