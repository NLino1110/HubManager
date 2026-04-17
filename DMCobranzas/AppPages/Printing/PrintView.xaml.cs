using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using DMCobranzas.AppPages.Printing;
using DMCobranzas.Services;
using DMSA.Models.Odoo.Native;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DMCobranzas.AppPages;

public class TextProcessor
{
    public List<string> ProcessText(string texto)
    {
        var processedLines = new List<string>();

        // Dividir el texto original en líneas
        var lines = texto.Split('\n');

        foreach (var line in lines)
        {
            // Si la línea contiene "Cliente:" y es mayor a 25 caracteres, procesarla
            if (line.Contains("Cliente:") && line.Length > 35)
            {
                var words = line.Split(' ');

                if (words.Length > 4)
                {
                    processedLines.AddRange(SplitWordsIntoLines(words, 40));
                }
                else
                {
                    processedLines.Add(line);
                }
            }
            else
            {
                // Si la línea no necesita procesamiento especial, añadirla directamente
                processedLines.Add(line);
            }
        }

        return processedLines;
    }

    private List<string> SplitWordsIntoLines(string[] words, int maxLength)
    {
        var lines = new List<string>();
        var currentLine = string.Empty;

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
            var textWidth = testLine.Length;

            if (textWidth > maxLength)
            {
                if (!string.IsNullOrEmpty(currentLine))
                {
                    lines.Add(currentLine);
                }
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }

        return lines;
    }
}

public partial class PrintView : ContentPage
{
    public res_company res_Company { get; set; }
    string LocalTemplateContent { get; set; }
    byte[] printData { get; set; }

    private readonly IPrinterService _printer;

    public void setTemplatePreview(string newTemplate)
    {
        //editor.Text = newTemplate;
        previewWeb.Source = new HtmlWebViewSource
        {
            Html = newTemplate
        };
        LocalTemplateContent = newTemplate;
    }

    public void setTemplatePlain(string newTemplatePlain)
    {        
        LocalTemplateContent = newTemplatePlain;
    }

    public void setData(byte[] printDataParam)
    {
        printData = printDataParam;        
    }

    //private ObservableCollection<DeviceLocal> m_deviceList { get; set; }

    public PrintView()
	{
		InitializeComponent();
       
        //editor.IsReadOnly = true;

#if ANDROID
        _printer = new BluetoothPrinterService();
#elif WINDOWS
        _printer = new BluetoothPrinterServiceWindows();
#endif
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ScanAndSelect();

        //IDispatcherTimer timer;

        //timer = Dispatcher.CreateTimer();
        //timer.Interval = TimeSpan.FromMilliseconds(500);
        //timer.IsRepeating = false;
        //timer.Tick += async (s, e) =>
        //{
        //    await ScanAndSelect();
        //    timer.Stop();
        //};
        //timer.Start();
    }

    private async Task ScanAndSelect()
    {
        try
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            string text = "No se han encontrado dispositivos emparejados, reinicie la impresora y vuelva a intentar...";
            ToastDuration duration = ToastDuration.Long;
            double fontSize = 14;
#if ANDROID
            var listado = _printer.GetPairedDevices();

            if (listado.Count() == 0)
            {                
                var toast = Toast.Make(text, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
                return;
            }

            foreach (var printer_item in listado)
            {
                if (printer_item.Name.Contains("Printer"))
                {
                    text += "\n" + printer_item.Name + " - " + printer_item.Address;
                    var mac_add = printer_item.Address;
                    //var mac_add = "5A:4A:CA:BB:A1:2A";
                    await _printer.Connect(mac_add);
                    text = "Impresora encontrada [" + printer_item.Name + "]";
                    btnPrint.IsEnabled = true;
                    await Toast.Make(text, duration, fontSize).Show();
                    break;
                }
            }
#elif WINDOWS
            var listado = await _printer.GetPairedDevicesAsync();

            if (listado.Count() == 0)
            {                
                var toast = Toast.Make(text, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
                return;
            }

            foreach (var printer_item in listado)
            {
                if (printer_item.Name.Contains("Printer"))
                {
                    text += "\n" + printer_item.Name + " - " + printer_item.Address;
                    var mac_add = printer_item.Address;
                    //var mac_add = "5A:4A:CA:BB:A1:2A";
                    await _printer.Connect(mac_add);
                    text = "Impresora encontrada [" + printer_item.Name + "]";
                    btnPrint.IsEnabled = true;
                    await Toast.Make(text, duration, fontSize).Show();
                    break;
                }
            }
#endif
        }
        catch (Exception exPrinting)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            string text = "Error al seleccionar impresora:" + exPrinting.Message;
            ToastDuration duration = ToastDuration.Long;
            double fontSize = 14;
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
        }
        finally
        {

        }
    }

    void OnEditorTextChanged(object sender, TextChangedEventArgs e)
    {
        
    }

    void OnEditorCompleted(object sender, EventArgs e)
    {
       
    }

    public SKImage LoadEmbeddedImage()
    {   
        string resourceName = "CobranzasDMSA-Odoo.Resources.Images.demo2.bmp";
        var assembly = Assembly.GetExecutingAssembly();
        
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream != null)
            {        
                return SKImage.FromEncodedData(stream);
            }
        }
        return null;
    }

    public SKBitmap LoadEmbeddedBitmap()
    {        
        string resourceName = "DMCobranzas.Resources.Images.logo_macronegocios.bmp";
        var assembly = Assembly.GetExecutingAssembly();
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream != null)
            {
                return SKBitmap.Decode(stream);
            }
        }
        return null; 
    }

    public SKBitmap LoadEmbeddedBitmap(int company_id)
    {
        //    "data": [
        //    {
        //        "id": 1,
        //        "name": "MACRONEGOCIOS S.A."
        //    },
        //    {
        //        "id": 2,
        //        "name": "D'MUJERES S.A. D'MUJERSA"
        //    }
        //]

        string resourceName = "DMCobranzas.Resources.Images.logo_neutro.bmp";

        switch (company_id)
        {
            case 1:
                {
                    resourceName = "DMCobranzas.Resources.Images.logo_macronegocios.bmp";
                }
                break;
            case 2:
                {
                    resourceName = "DMCobranzas.Resources.Images.logo_dmujeres.bmp";
                }
                break;
        }
        
        var assembly = Assembly.GetExecutingAssembly();
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream != null)
            {
                return SKBitmap.Decode(stream);
            }
        }
        return null;
    }

    public SKBitmap MakeTransparent(SKBitmap bitmap, SKColor targetColor, SKColor transparentColor)
    {
        var imageInfo = new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                
        var transparentBitmap = new SKBitmap(imageInfo);
        
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                var color = bitmap.GetPixel(x, y);
                                
                if (color == targetColor)
                {
                    transparentBitmap.SetPixel(x, y, SKColors.Transparent);
                }
                else
                {
                    transparentBitmap.SetPixel(x, y, color);
                }
            }
        }

        return transparentBitmap;
    }


    public async Task<SKBitmap> BuildImageFromText(string texto)
    {
        var textProcessor = new TextProcessor();
        var lineas = textProcessor.ProcessText(texto).ToArray();

        //var lineas = texto.Split('\n');
        int multiploLinea = 40;
        int lineasCount = 1;
        int logoHeight = 70;
        if(lineas.Length > 0)
        {
            lineasCount = lineas.Length;
        }

        // Establecer un tamaño más grande para el lienzo según la longitud del texto
        //var textPaint = new SKPaint
        //{
        //    TextSize = 24,
        //    IsAntialias = true,
        //    Color = SKColors.Black
        //};

        var canvasWidth = 620; // Ajusta según sea necesario
        var canvasHeight = multiploLinea * lineasCount;//1000; // Ajusta según sea necesario

        // Crear un lienzo de SkiaSharp con fondo blanco
        using (var surface = SKSurface.Create(new SKImageInfo((int)canvasWidth, (int)canvasHeight, SKColorType.Bgra8888, SKAlphaType.Premul)))
        {
            var canvas = surface.Canvas;
            
            // Establecer color de fondo
            canvas.Clear(SKColors.White);

            using (var watermarkPaint = new SKPaint())
            {
                var watermarkText = "Macronegocios";
                var watermarkFontSize = 15;

                using (var watermarkTypeface = SKTypeface.FromFamilyName("Arial"))
                {
                    watermarkPaint.Typeface = watermarkTypeface;
                    watermarkPaint.TextSize = watermarkFontSize;
                    watermarkPaint.Color = SKColors.LightGray.WithAlpha(180);
                    watermarkPaint.IsAntialias = true;

                    var textBounds = new SKRect();
                    watermarkPaint.MeasureText(watermarkText, ref textBounds);

                    var textWidth = textBounds.Width;
                    var textHeight = textBounds.Height;
                    
                    canvas.Save();
                    float angle = -45;
                    canvas.RotateDegrees(angle, canvasWidth / 2, canvasHeight / 2);
                    
                    for (float y = -canvasHeight; y < canvasHeight * 2; y += textHeight + 20)
                    {
                        for (float x = -canvasWidth; x < canvasWidth * 2; x += textWidth + 20)
                        {
                            canvas.DrawText(watermarkText, x, y - textBounds.Top, watermarkPaint);
                        }
                    }
                    
                    canvas.Restore();
                }
            }

            if (res_Company != null)
            {
                //Se dibuja logo
                var logoSki = LoadEmbeddedBitmap(res_Company.id);
                //logoSki.Width = canvasWidth;
                float leftLogo = 10;
                leftLogo = (canvasWidth / 2) - (logoSki.Width / 2);

                //Transforma a transparente el logo
                var targetColor = SKColors.White;
                var transparentColor = SKColors.Transparent;
                var transparentBitmap = MakeTransparent(logoSki, targetColor, transparentColor);
                canvas.DrawBitmap(transparentBitmap, new SKPoint(leftLogo, 10));
            }

            //var fontPath = Path.Combine(FileSystem.Current.AppDataDirectory, "Consolas.ttf");
            using (Stream fontStream = await FileSystem.Current.OpenAppPackageFileAsync("Consolas.ttf"))
            using (var consolasTypeface = SKTypeface.FromStream(fontStream))
            {
                Console.WriteLine(consolasTypeface);

                //string fontPath = "Consolas";
                //SKTypeface consolasTypeface = SKTypeface.FromFamilyName("Consolas", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
                //SKTypeface consolasTypeface = SKTypeface.FromFamilyName(fontPath);

                // Configurar el tipo y tamaño de la fuente
                var paint = new SKPaint
                {
                    TextSize = 24,
                    IsAntialias = true,
                    Color = SKColors.Black,
                    //Typeface = consolasTypeface
                };

                var font = new SKFont(consolasTypeface, 24);

                // Dividir el texto en líneas y dibujar cada línea en el lienzo
                //var lineas = texto.Split('\n');
                var yPos = 50 + logoHeight; // Ajustar según sea necesario

                int spaceLines = 10;

                foreach (var linea in lineas)
                {
                    string lineaVal = linea;
                    if (lineaVal == "") lineaVal = " ";
                    if (lineaVal == "\n") lineaVal = " ";
                    if (lineaVal == "\r") lineaVal = " ";
                                        
                    if (lineaVal.Contains("\r") || lineaVal.Contains("\n"))
                        lineaVal = lineaVal.Replace("\r", "").Replace("\n", "");

                    // Crear un SKTextBlob para manejar saltos de línea y formato
                    using (var textBlob = SKTextBlob.Create(lineaVal, font))
                    {
                        // Dibujar el SKTextBlob en el lienzo
                        canvas.DrawText(textBlob, 10, yPos, paint);
                        yPos += (int)textBlob.Bounds.Height + spaceLines; // Alto de los caracteres + espacio extra
                    }
                }

                // Tomar la instantánea como SKImage
                var skImage = surface.Snapshot();

                // Convertir SKImage a SKBitmap
                var skBitmap = SKBitmap.FromImage(skImage);

                return skBitmap;
            }
        }
    }

    public async Task CompartirImagen(SKBitmap bitmap, string nombreArchivo)
    {
        // Guardar el bitmap como un archivo
        var filePath = Path.Combine(FileSystem.CacheDirectory, nombreArchivo + ".png");

        using (var stream = new SKFileWStream(filePath))
        {
            bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
        }

        // Compartir el archivo
        var shareFileRequest = new ShareFileRequest
        {
            Title = "Compartir Imagen",
            File = new ShareFile(filePath)
        };

        await Share.Default.RequestAsync(shareFileRequest);
    }

    private async void btnShare_Clicked(object sender, EventArgs e)
    {
        //await ShareText("Prueba de compartir!!");
        var bmpImg = await BuildImageFromText(LocalTemplateContent);
        await CompartirImagen(bmpImg, "tmp_img");
    }

    [Obsolete("This method is deprecated. Use the new CleanPrintString method.")]
    public static string CleanPrintString(string input)
    {        
        string pattern = @"[^a-zA-Z0-9\s]";
        string cleanedString = Regex.Replace(input, pattern, "");

        return cleanedString;
    }

    private async void btnPrint_Clicked(object sender, EventArgs e)
    {
        Debug.WriteLine("Imprimir...");

        btnPrint.IsEnabled = false;

        try
        {
            
            await _printer.Print(printData);
        }
        catch (Exception exPrinting)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            string text = "Error al imprimir:" + exPrinting.Message;
            ToastDuration duration = ToastDuration.Long;
            double fontSize = 14;
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
        }
        finally
        {
            //btm.Dispose();
            //btm = null;            
        }

        btnPrint.IsEnabled = true;
    }

    private void myListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        Debug.WriteLine("Seleccionado: " + e.SelectedItem);
    }   

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

#if ANDROID || WINDOWS
        try
        {
            _printer?.Disconnect();
            Debug.WriteLine("Printer disconnected");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error disconnecting printer: " + ex.Message);
        }
#endif
    }
}