using DMCobranzas.Models;
using DMCobranzas.Services;
using DMCobranzas.AppPages.Printing;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using SkiaSharp;
using System.Xml.Linq;
using System.Reflection;
using DMSA.Models.Odoo.Native;

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


public partial class PrintView : ContentPage, IDisposable
{
    public res_company res_Company { get; set; }
    string LocalTemplateContent { get; set; }

    public void setTemplate( string newTemplate)
    {
        editor.Text = newTemplate;
        LocalTemplateContent = newTemplate;
    }

    BluetoothPrinterManager btPrinterManager { get; set; }

    private ObservableCollection<DeviceLocal> m_deviceList { get; set; }

    public PrintView()
	{
		InitializeComponent();
        m_deviceList = new ObservableCollection<DeviceLocal>();
        btPrinterManager = new BluetoothPrinterManager();
        
        //PrintCommand = new Command(PrintItem);

        editor.IsReadOnly = true;

        BindingContext = this;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        IDispatcherTimer timer;

        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.IsRepeating = false;
        timer.Tick += async (s, e) =>
        {
            await ScanAndSelect();
            timer.Stop();
        };
        timer.Start();
    }

    private async Task ScanAndSelect()
    {
        m_deviceList.Clear();
        
        try
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            string text = "No se han encontrado dispositivos emparejados, agregue la impresora y vuelva a intentar...";
            ToastDuration duration = ToastDuration.Short;
            double fontSize = 14;

            var foundDevices = btPrinterManager.GetPairedDevices();

            //Cuando no haya dispositivos emparejados
            if (foundDevices == null || (foundDevices != null && foundDevices.Count() == 0))
            {
                var toast = Toast.Make(text, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
                return;
            }

            bool foundPrinter = false;
            string printerNameFound = "";
            foreach (var device in foundDevices)
            {
                //Por ahora asumimos que el primer dispositivo que contenga en su nombre 
                // la palabra Printer, es un impresora compatible
                if (device.Name.Contains("Printer"))
                {
                    printerNameFound = device.Name;
                    foundPrinter = true;
                    DeviceLocal ndl = new DeviceLocal()
                    {
                        Name = device.Name
                    };

                    m_deviceList.Add(ndl);

                    btPrinterManager.setPrinterDevice(device);
                    //await btm.createRfcommSocketToServiceRecord("0000eee2-0000-1000-8000-00805f9b34fb", "0000eee3-0000-1000-8000-00805f9b34fb");
                    //bool statusSocketConnect = await btPrinterManager.createRfcommSocketToServiceRecord("0000eee2-0000-1000-8000-00805f9b34fb", "0000eee3-0000-1000-8000-00805f9b34fb");
                    //await btm.Print("Prueba", "00001101-0000-1000-8000-00805F9B34FB", "00001101-0000-1000-8000-00805F9B34FB");
                    //bool statusSocketConnect = await btPrinterManager.createRfcommSocketToServiceRecord("00001101-0000-1000-8000-00805F9B34FB", "00001101-0000-1000-8000-00805F9B34FB");
                    //Para la impresora vieja
                    bool statusSocketConnect = await btPrinterManager.createRfcommSocketToServiceRecord("000018f0-0000-1000-8000-00805f9b34fb", "00002af1-0000-1000-8000-00805f9b34fb");

                    if (statusSocketConnect)
                    {

                    }

                    //await btPrinterManager.createRfcommSocketToServiceRecord("e7810a71-73ae-499d-8c15-faa9aef0c3f2", "bef8d6c9-9c21-4c9e-b632-bd58c1009f9f");
                    break;
                }
            }

            //btm.setPrinter();

            text = "Impresora no encontrada, agregue la impresora y vuelva a intentar...";

            //Dispositivos emparejados pero la impresora no se encuentra en la lista
            if (!foundPrinter)
            {
                var toast = Toast.Make(text, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
                return;
            }
            else
            {
                text = "Impresora seleccionada correctamente. " + printerNameFound;
                var toast = Toast.Make(text, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
            }

            //myListView.ItemsSource = m_deviceList;
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
        //string oldText = e.OldTextValue;
        //string newText = e.NewTextValue;
        //string myText = editor.Text;
    }

    void OnEditorCompleted(object sender, EventArgs e)
    {
        //string text = ((Editor)sender).Text;
    }

    //////public async Task ShareFile()
    //////{
    //////    string fn = "Attachment.txt";
    //////    string file = Path.Combine(FileSystem.CacheDirectory, fn);

    //////    File.WriteAllText(file, "Hello World");

    //////    await Share.Default.RequestAsync(new ShareFileRequest
    //////    {
    //////        Title = "Share text file",
    //////        File = new ShareFile(file)
    //////    });
    //////}

    public async Task ShareText(string text)
    {
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Text = text,
            Title = "Share Text"
        });
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


    public async Task<SKBitmap> CrearImagenDesdeTexto(string texto)
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

        var canvasWidth = 600; // Ajusta según sea necesario
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
        var bmpImg = await CrearImagenDesdeTexto(LocalTemplateContent);
        await CompartirImagen(bmpImg, "tmp_img");
    }



    public static string CleanPrintString(string input)
    {
        // Usa una expresión regular para eliminar caracteres especiales
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
            const string ESC = "\x1B";
            const string NewLine = "\n";

            // Iniciar impresión
            string initializePrinter = ESC + "@";
            string setAlignmentLeft = ESC + "a" + "0";
            string cutPaper = ESC + "d" + "\x08";
            string lineFeed = NewLine;

            //TODO: Se realiza esta depuración ya
            // que no envía a imprimir correctamente
            //string PrintString = CleanPrintString(LocalTemplateContent);            
            string PrintString = LocalTemplateContent;
            PrintString = initializePrinter +
                         setAlignmentLeft +
                         PrintString +                         
                         lineFeed +
                         cutPaper;

            //TODO: Se imprimirá linea por línea hasta encontrar el bug
            //await btPrinterManager.Print(PrintString);
            string[] lines = PrintString.Split('\n');
            foreach (var line in lines)
            {
                await btPrinterManager.Print(line.Trim());
                await btPrinterManager.Print(NewLine);
            }
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

    public void Dispose()
    {
        btPrinterManager.Dispose();
        //throw new NotImplementedException();
    }
}