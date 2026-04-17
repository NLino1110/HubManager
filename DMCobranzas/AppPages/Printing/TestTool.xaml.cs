
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using DMCobranzas.Services;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DMCobranzas.AppPages.Printing;

public partial class TestTool : ContentPage, IDisposable //, INotifyPropertyChanged
{    
    private readonly IPrinterService _printer;
    
    public TestTool()
	{        
        InitializeComponent();        
        editor.Text = "Prueba de impresion";

#if ANDROID
        _printer = new BluetoothPrinterService();
#elif WINDOWS
        _printer = new BluetoothPrinterServiceWindows();
#endif
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

    void SetImage()
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string[] namesass = assembly.GetManifestResourceNames();
        Stream stream = assembly.GetManifestResourceStream("CobranzasDMSA.Resources.Images.demo.bmp");        
        imgPrint.Source = ImageSource.FromStream(() => stream);
        
    }

    void SetImage2()
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string[] namesass = assembly.GetManifestResourceNames();
        Stream stream = assembly.GetManifestResourceStream("CobranzasDMSA.Resources.Images.ic_launcher.png");
        imgPrint.Source = ImageSource.FromStream(() => stream);
    }

    void SetImage(Stream stream)
    {
        imgPrint.Source = ImageSource.FromStream(() => stream);
    }

    void OnEditorTextChanged(object sender, TextChangedEventArgs e)
    {
        string oldText = e.OldTextValue;
        string newText = e.NewTextValue;
        string myText = editor.Text;
    }

    void OnEditorCompleted(object sender, EventArgs e)
    {
        string text = ((Editor)sender).Text;
    }

    private async Task ScanAndSelect()
    {
        try
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            string text = "No se han encontrado dispositivos emparejados, reinicie la impresora y vuelva a intentar...";
            ToastDuration duration = ToastDuration.Long;
            double fontSize = 14;
            List<BluetoothDeviceInfo> listado = new List<BluetoothDeviceInfo>();

#if ANDROID
            listado = _printer.GetPairedDevices();
#elif WINDOWS
            listado = await _printer.GetPairedDevicesAsync();            
#endif

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
                    //btnPrint.IsEnabled = true;
                    await Toast.Make(text, duration, fontSize).Show();
                    break;
                }
            }


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

    private async void Explore(object sender, EventArgs e)
    {
        btnPrintSetup.IsEnabled = false;

        try
        {
                     
        }
        catch (Exception exPrinting)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            string text = "Error al configurar:" + exPrinting.Message;
            ToastDuration duration = ToastDuration.Long;
            double fontSize = 14;
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
        }
        finally
        {
            //btm.Dispose();
            //btm = null;
            btnPrintSetup.IsEnabled = true;
        }
    }

    private async void PrintSetup(object sender, EventArgs e)
    {
        btnPrintSetup.IsEnabled = false;

        try
        {            
            await ScanAndSelect();
        }
        catch (Exception exPrinting)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            string text = "Error al configurar:" + exPrinting.Message;
            ToastDuration duration = ToastDuration.Long;
            double fontSize = 14;
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
        }
        finally
        {
            //btm.Dispose();
            //btm = null;
            btnPrintSetup.IsEnabled = true;
        }
    }

    private async void Print(object sender, EventArgs e)
    {
        btnScan.IsEnabled = false;
        
        try
        {
            var receipt = new ReceiptBuilder();

            receipt
            .Center()
            .Bold()
            .Line("DMUJERES S. A.")
            .Normal()
            .Line("RECIBO #-DMJC-20260303-1")
            .Left()
            .Line("Cliente: (56363) PUNTO BELLEZA")
            .Line("Estado: PROCESADO")
            .Separator()

            .Bold()
            .Line("============= F.PAGO ===========")
            .Normal()

            .Columns("TRANSFERENCIA", "$9.00")
            .Line(" Produbanco / Promerica")
            .Line(" Cta. 9889 Dp#987954")

            .Columns("CHEQUE DIA", "$6.00")
            .Line(" Banco Pichincha")

            .Separator()

            .Columns("TOTAL F/P:", "$25.00")

            .Separator()

            .Columns("TOTAL CANC:", "$25.00")
            .Columns("TOT. FACT. PEND:", "$207.18")

            .Separator()

            .Line("Email: puntobellezauio@gmail.com")
            .Line("Vnd: CHONILLO MALDONADO")
            .Line("Fecha: 03/03/2026")

            .Feed()
            .Line("____________________________")
            .Line("-Firma Cliente-")
            .Feed()
            .Cut();

            editor.Text = receipt.BuildPreview();
            //await _printer.Print(receipt.Build());
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
            btnScan.IsEnabled = true;
        }
    }

    private async void PrintBarCode(object sender, EventArgs e)
    {
        btnPrintBarCode.IsEnabled = false;

        try
        {
            var data = EscPosCommands.Join(
                EscPosCommands.Init(),
                EscPosCommands.AlignCenter(),
                EscPosCommands.BarcodeFull(4, "RCH-1381"), // CODE39
                EscPosCommands.LineFeed(),
                EscPosCommands.LineFeed(),
                EscPosCommands.Cut()
            );

            await _printer.Print(data);
        }
        catch (Exception exPrinting)
        {
            var toast = Toast.Make(
                "Error al imprimir codigo de barra:" + exPrinting.Message,
                ToastDuration.Long,
                14
            );

            await toast.Show(new CancellationTokenSource().Token);
        }
        finally
        {
            btnPrintBarCode.IsEnabled = true;
        }
    }

    private async void PrintQr(object sender, EventArgs e)
    {
        btnPrintQr.IsEnabled = false;

        try
        {
            var data = EscPosCommands.Join(
                EscPosCommands.Init(),
                EscPosCommands.AlignCenter(),

                EscPosCommands.QR("https://www.dmujeres.ec/"),

                EscPosCommands.LineFeed(),
                EscPosCommands.LineFeed(),

                EscPosCommands.FontSize(1, 1),
                EscPosCommands.Text("DMujeres S.A.\n"),

                EscPosCommands.Feed(48),
                EscPosCommands.Cut(),

                EscPosCommands.FontSize(0, 0)
            );

            await _printer.Print(data);
        }
        catch (Exception exPrinting)
        {
            var toast = Toast.Make(
                "Error al imprimir qr:" + exPrinting.Message,
                ToastDuration.Long,
                14
            );

            await toast.Show(new CancellationTokenSource().Token);
        }
        finally
        {
            btnPrintQr.IsEnabled = true;
        }
    }

    private async void PrintGraphical(object sender, EventArgs e)
    {
        btnPrintQr.IsEnabled = false;

        try
        {
            ////byte[] qrcode = PrinterCommand.GetBarCommand("https://www.dmujeres.ec/", 0, 3, 6);//
            ////PrintCommandBytes.ESC_Align[2] = 0x01;
            ////await btPrinterManager.PrintBytes(PrintCommandBytes.ESC_Align);
            ////await btPrinterManager.PrintBytes(qrcode);

            ////await btPrinterManager.PrintBytes(PrintCommandBytes.ESC_Align);
            ////PrintCommandBytes.GS_ExclamationMark[2] = 0x11;
            ////await btPrinterManager.PrintBytes(PrintCommandBytes.GS_ExclamationMark);
            ////await btPrinterManager.PrintBytes(Encoding.UTF8.GetBytes("DMujeres S.A.\n"));
            //PrintCommandBytes.ESC_Align[2] = 0x00;
            //await btPrinterManager.PrintBytes(PrintCommandBytes.ESC_Align);
            //PrintCommandBytes.GS_ExclamationMark[2] = 0x00;
            //await btPrinterManager.PrintBytes(PrintCommandBytes.GS_ExclamationMark);
            //await btPrinterManager.PrintBytes("XYZ: 888888\nXYZ  S00003333\nXYZ：1001\nXYZ：xxxx-xx-xx\nXYZ：xxxx-xx-xx  xx:xx:xx\n".getBytes("GBK"));
            //await btPrinterManager.PrintBytes("XYZ       XYZ    XYZ    XYZ\nNIKEXYZ   10.00   899     8990\nNIKEXYZ 10.00   1599    15990\n".getBytes("GBK"));
            //await btPrinterManager.PrintBytes("XYZ：                20.00\nXYZ：                16889.00\nXYZ：                17000.00\n找零：                111.00\n".getBytes("GBK"));
            //await btPrinterManager.PrintBytes("公司名称：NIKE\n公司网址：www.xxx.xxx\n地址：深圳市xx区xx号\n电话：0755-11111111\n服务专线：400-xxx-xxxx\n================================\n".getBytes("GBK"));
            //PrintCommandBytes.ESC_Align[2] = 0x01;
            //await btPrinterManager.PrintBytes(PrintCommandBytes.ESC_Align);
            //PrintCommandBytes.GS_ExclamationMark[2] = 0x11;
            //await btPrinterManager.PrintBytes(PrintCommandBytes.GS_ExclamationMark);
            //await btPrinterManager.PrintBytes("谢谢惠顾,欢迎再次光临!\n".getBytes("GBK"));
            //PrintCommandBytes.ESC_Align[2] = 0x00;
            //await btPrinterManager.PrintBytes(PrintCommandBytes.ESC_Align);
            //PrintCommandBytes.GS_ExclamationMark[2] = 0x00;
            //await btPrinterManager.PrintBytes(PrintCommandBytes.GS_ExclamationMark);

            //await btPrinterManager.PrintBytes("(以上信息为测试模板,如有苟同，纯属巧合!)\n".getBytes("GBK"));
            //PrintCommandBytes.ESC_Align[2] = 0x02;
            //await btPrinterManager.PrintBytes(PrintCommandBytes.ESC_Align);
            //await btPrinterManager.PrintBytes(date);
            //////await btPrinterManager.PrintBytes(PrinterCommand.POS_Set_PrtAndFeedPaper(48));
            //////await btPrinterManager.PrintBytes(PrintCommandBytes.GS_V_m_n);
        }
        catch (Exception exPrinting)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            string text = "Error al imprimir qr:" + exPrinting.Message;
            ToastDuration duration = ToastDuration.Long;
            double fontSize = 14;
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
        }
        finally
        {
            //btm.Dispose();
            //btm = null;
            btnPrintQr.IsEnabled = true;
        }
    }

    public static byte[] POS_PrintBMP(SKBitmap mBitmap, int nWidth, int nMode)
    {
        int width = ((nWidth + 7) / 8) * 8;
        int height = mBitmap.Height * width / mBitmap.Width;
        height = ((height + 7) / 8) * 8;

        SKBitmap rszBitmap = mBitmap;
        if (mBitmap.Width != width)
        {
            rszBitmap = Zj.Com.Customize.Sdk.Other.ResizeBitmap(mBitmap, width, height);
        }

        SKBitmap grayBitmap = Zj.Com.Customize.Sdk.Other.ConvertToGrayscale(rszBitmap);

        byte[] dithered = Zj.Com.Customize.Sdk.Other.ThresholdToBWPic(rszBitmap);

        byte[] data = Zj.Com.Customize.Sdk.Other.EachLinePixToCmd(dithered, width, nMode);

        return data;
    }

    public static SKBitmap PrepareBMP(SKBitmap mBitmap, int nWidth, int nMode)
    {
        int width = ((nWidth + 7) / 8) * 8;
        int height = mBitmap.Height * width / mBitmap.Width;
        height = ((height + 7) / 8) * 8;

        SKBitmap rszBitmap = mBitmap;
        if (mBitmap.Width != width)
        {
            rszBitmap = Zj.Com.Customize.Sdk.Other.ResizeBitmap(mBitmap, width, height);
        }

        SKBitmap grayBitmap = Zj.Com.Customize.Sdk.Other.ConvertToGrayscale(rszBitmap);

        //byte[] dithered = Zj.Com.Customize.Sdk.Other.ThresholdToBWPic(grayBitmap);

        //byte[] data = Zj.Com.Customize.Sdk.Other.EachLinePixToCmd(dithered, width, nMode);

        return mBitmap;
    }

    public static byte[] ConvertBMPToESCPOSCommands(byte[] bmpData, int width, int height)
    {
        // Comandos de inicio y configuración de la impresora
        byte[] initializePrinter = { 0x1B, 0x40 }; // Inicializar la impresora
        byte[] selectImageMode = { 0x1D, 0x76, 0x30, 0x00 }; // Seleccionar el modo de imagen
        byte[] setPrintDensity = { 0x1D, 0x7C, 0x00 }; // Establecer la densidad de impresión

        // Comandos de datos de imagen
        byte[] printImage = { 0x1B, 0x2A, 0x21, 0x00, 0x00 }; // Imprimir datos de imagen

        // Calcular el tamaño de la imagen
        int dataSize = width * height / 8;
        int paddedWidth = (width + 7) / 8;

        // Crear el arreglo de bytes para los comandos de impresión ESC/POS
        List<byte> commands = new List<byte>();

        // Agregar los comandos de inicio y configuración de la impresora
        commands.AddRange(initializePrinter);
        commands.AddRange(selectImageMode);
        commands.AddRange(setPrintDensity);

        // Agregar el comando de impresión de imagen
        printImage[3] = (byte)(paddedWidth % 256);
        printImage[4] = (byte)(paddedWidth / 256);
        commands.AddRange(printImage);

        // Agregar los datos de imagen convertidos
        commands.AddRange(bmpData);

        return commands.ToArray();
    }

    public byte[] ConvertToEscPosCommands(SKBitmap bitmap)
    {
        const byte ESC = 0x1B;
        const byte GS = 0x1D;

        // Obtener los datos de píxeles de la imagen
        var data = bitmap.Bytes;

        // Calcular el ancho y alto de la imagen en píxeles
        int width = bitmap.Width;
        int height = bitmap.Height;

        // Calcular el ancho en bytes de cada fila de píxeles
        int bytesPerRow = (width + 7) / 8;

        // Crear un MemoryStream para almacenar los comandos ESC/POS
        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream))
        {
            // Iniciar la secuencia de comandos ESC/POS
            writer.Write(ESC);
            writer.Write('@');

            // Establecer el modo de impresión de píxeles
            writer.Write(GS);
            writer.Write('v');
            writer.Write((byte)0);
            writer.Write((byte)48);

            // Enviar los datos de píxeles fila por fila
            for (int y = 0; y < height; y++)
            {
                // Obtener el puntero al inicio de la fila
                int rowOffset = y * bytesPerRow;

                // Enviar el comando de impresión de una fila de píxeles
                writer.Write(GS);
                writer.Write('*');
                writer.Write((byte)33);
                writer.Write((byte)bytesPerRow);
                writer.Write((byte)(width % 256));
                writer.Write((byte)(width / 256));

                // Enviar los datos de píxeles de la fila
                for (int x = 0; x < bytesPerRow; x++)
                {
                    byte pixelByte = 0;

                    // Construir el byte de datos de píxeles para la fila
                    for (int bit = 0; bit < 8; bit++)
                    {
                        int pixelIndex = x * 8 + bit;
                        if (pixelIndex < width)
                        {
                            int pixelOffset = rowOffset + pixelIndex;
                            byte pixelValue = (data[pixelOffset] == 0) ? (byte)1 : (byte)0;
                            pixelByte |= (byte)(pixelValue << (7 - bit));
                        }
                    }

                    // Escribir el byte de datos de píxeles en el flujo
                    writer.Write(pixelByte);
                }
            }

            // Finalizar la secuencia de comandos ESC/POS
            writer.Write(ESC);
            writer.Write('2');

            // Obtener los comandos ESC/POS como una matriz de bytes
            return stream.ToArray();
        }
    }

    public SKImage ConvertToGrayScale(SKImage originalImage)
    {
        SKBitmap grayBitmap = new SKBitmap(originalImage.Width, originalImage.Height);

        using (SKCanvas canvas = new SKCanvas(grayBitmap))
        using (SKPaint paint = new SKPaint())
        {
            // Establece el efecto de escala de grises.
            paint.ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
            {
            0.299f, 0.299f, 0.299f, 0, 0,
            0.587f, 0.587f, 0.587f, 0, 0,
            0.114f, 0.114f, 0.114f, 0, 0,
            0, 0, 0, 1, 0
            });

            // Dibuja la imagen original en el nuevo bitmap con el efecto de escala de grises aplicado.
            canvas.DrawImage(originalImage, 0, 0, paint);
        }

        // Crea y devuelve un nuevo SKImage a partir del bitmap con el efecto de escala de grises aplicado.
        return SKImage.FromBitmap(grayBitmap);
    }

    public SKImage _ConvertToBlackPixels(SKImage originalImage)
    {
        SKBitmap grayBitmap = new SKBitmap(originalImage.Width, originalImage.Height);

        using (SKCanvas canvas = new SKCanvas(grayBitmap))
        using (SKPaint paint = new SKPaint())
        {
            // Establece el efecto de escala de grises.
            paint.ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
            {
            0.299f, 0.299f, 0.299f, 0, 0,
            0.587f, 0.587f, 0.587f, 0, 0,
            0.114f, 0.114f, 0.114f, 0, 0,
            0, 0, 0, 1, 0
            });

            // Dibuja la imagen original en el nuevo bitmap con el efecto de escala de grises aplicado.
            canvas.DrawImage(originalImage, 0, 0, paint);
        }

        // Crea y devuelve un nuevo SKImage a partir del bitmap con el efecto de escala de grises aplicado.
        return SKImage.FromBitmap(grayBitmap);
    }

    public SKBitmap ConvertToBlackPixels(SKBitmap originalBitmap, byte threshold = 128)
    {
        // Crear un nuevo SKBitmap para los píxeles modificados.
        SKBitmap blackBitmap = originalBitmap.Copy();

        // Obtener los bytes de los píxeles.
        byte[] pixels = blackBitmap.Bytes;

        // Convertir los píxeles a blanco o negro según el umbral.
        for (int i = 0; i < pixels.Length; i += 4) // Cada píxel ocupa 4 bytes (RGBA).
        {
            byte r = pixels[i];
            byte g = pixels[i + 1];
            byte b = pixels[i + 2];

            // Calcular la intensidad promedio del píxel.
            byte intensity = (byte)((r + g + b) / 3);

            // Establecer el píxel en blanco (255) o negro (0) según el umbral.
            if (intensity < threshold)
            {
                pixels[i] = 0; // R
                pixels[i + 1] = 0; // G
                pixels[i + 2] = 0; // B
            }
            else
            {
                pixels[i] = 255; // R
                pixels[i + 1] = 255; // G
                pixels[i + 2] = 255; // B
            }
        }

        // Crea y devuelve un nuevo SKBitmap con los píxeles modificados.
        return blackBitmap;
    }

    //public SKImage LoadEmbeddedImage()
    //{
    //    // Reemplaza "NombreProyecto.NombreCarpetaImagenes.nombre_imagen.png" con la ruta completa del recurso incrustado.
    //    string resourceName = "DMCobranzas.Resources.Images.demo2.png";

    //    // Obtén el ensamblado actual.
    //    var assembly = Assembly.GetExecutingAssembly();

    //    // Lee el recurso incrustado como una secuencia de bytes.
    //    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
    //    {
    //        if (stream != null)
    //        {
    //            // Crea un SKImage a partir de la secuencia de bytes.
    //            return SKImage.FromEncodedData(stream);
    //        }
    //    }

    //    return null; // En caso de que la carga falle, devolver null.
    //}

    public SKBitmap LoadEmbeddedBitmap()
    {
        // Reemplaza "NombreProyecto.NombreCarpetaImagenes.nombre_imagen.png" con la ruta completa del recurso incrustado.
        string resourceName = "DMCobranzas.Resources.Images.demo2.bmp";

        // Obtén el ensamblado actual.
        var assembly = Assembly.GetExecutingAssembly();

        // Lee el recurso incrustado como una secuencia de bytes.
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream != null)
            {
                // Crea un SKBitmap a partir de la secuencia de bytes.
                return SKBitmap.Decode(stream);
            }
        }

        return null; // En caso de que la carga falle, devolver null.
    }

    ////public static byte[] CreateTestThresholdImage()
    ////{
    ////    // Tamaño imagen (debe ser múltiplo de 8 en ancho)
    ////    int width = 32;   // pixels
    ////    int height = 32;  // pixels

    ////    int bytesPerRow = width / 8;
    ////    byte[] imageData = new byte[bytesPerRow * height];

    ////    int index = 0;

    ////    // Crear patrón tipo threshold (ajedrezado)
    ////    for (int y = 0; y < height; y++)
    ////    {
    ////        for (int xByte = 0; xByte < bytesPerRow; xByte++)
    ////        {
    ////            byte b = 0;

    ////            for (int bit = 0; bit < 8; bit++)
    ////            {
    ////                int x = xByte * 8 + bit;

    ////                // patrón threshold simple
    ////                bool black = ((x / 4 + y / 4) % 2 == 0);

    ////                if (black)
    ////                    b |= (byte)(1 << (7 - bit));
    ////            }

    ////            imageData[index++] = b;
    ////        }
    ////    }

    ////    // ---------- ESC/POS Raster Image ----------
    ////    // GS v 0
    ////    List<byte> command = new List<byte>();

    ////    command.Add(0x1D); // GS
    ////    command.Add(0x76); // v
    ////    command.Add(0x30); // 0
    ////    command.Add(0x00); // normal mode

    ////    // width (bytes)
    ////    command.Add((byte)(bytesPerRow % 256));
    ////    command.Add((byte)(bytesPerRow / 256));

    ////    // height
    ////    command.Add((byte)(height % 256));
    ////    command.Add((byte)(height / 256));

    ////    command.AddRange(imageData);

    ////    return command.ToArray();
    ////}

    public static byte[] CreateTestThresholdImage()
    {
        // ===== Tamaño (ancho múltiplo de 8) =====
        int width = 128;
        int height = 64;

        int bytesPerRow = width / 8;
        byte[] imageData = new byte[bytesPerRow * height];

        int index = 0;

        // ---------- Silueta tipo mapache ----------
        bool IsBlack(int x, int y)
        {
            int cx = width / 2;
            int cy = height / 2 + 4;

            // Cabeza (ovalo)
            double head =
                Math.Pow((x - cx) / 28.0, 2) +
                Math.Pow((y - cy) / 20.0, 2);

            bool headShape = head <= 1.0;

            // Oreja izquierda
            double earL =
                Math.Pow((x - (cx - 22)) / 10.0, 2) +
                Math.Pow((y - (cy - 22)) / 10.0, 2);

            bool leftEar = earL <= 1.0;

            // Oreja derecha
            double earR =
                Math.Pow((x - (cx + 22)) / 10.0, 2) +
                Math.Pow((y - (cy - 22)) / 10.0, 2);

            bool rightEar = earR <= 1.0;

            // Máscara oscura (antifaz)
            bool mask =
                y > cy - 6 &&
                y < cy + 4 &&
                Math.Abs(x - cx) < 24;

            // Ojo izquierdo (hueco blanco)
            double eyeL =
                Math.Pow(x - (cx - 10), 2) +
                Math.Pow(y - (cy - 1), 2);

            bool leftEyeHole = eyeL < 4 * 4;

            // Ojo derecho
            double eyeR =
                Math.Pow(x - (cx + 10), 2) +
                Math.Pow(y - (cy - 1), 2);

            bool rightEyeHole = eyeR < 4 * 4;

            // Nariz
            double nose =
                Math.Pow(x - cx, 2) +
                Math.Pow(y - (cy + 6), 2);

            bool noseShape = nose < 3 * 3;

            bool silhouette = headShape || leftEar || rightEar;

            // aplicar máscara pero respetar ojos
            if (mask && !leftEyeHole && !rightEyeHole)
                silhouette = true;

            if (noseShape)
                silhouette = true;

            return silhouette;
        }

        // ---------- Generar bitmap monocromo ----------
        for (int y = 0; y < height; y++)
        {
            for (int xByte = 0; xByte < bytesPerRow; xByte++)
            {
                byte b = 0;

                for (int bit = 0; bit < 8; bit++)
                {
                    int x = xByte * 8 + bit;

                    if (IsBlack(x, y))
                        b |= (byte)(1 << (7 - bit));
                }

                imageData[index++] = b;
            }
        }

        // ---------- ESC/POS Raster (GS v 0) ----------
        List<byte> command = new List<byte>();

        command.Add(0x1D);
        command.Add(0x76);
        command.Add(0x30);
        command.Add(0x00);

        // width (bytes)
        command.Add((byte)(bytesPerRow % 256));
        command.Add((byte)(bytesPerRow / 256));

        // height
        command.Add((byte)(height % 256));
        command.Add((byte)(height / 256));

        command.AddRange(imageData);

        return command.ToArray();
    }

    private async void PrintImgWorking(object sender, EventArgs e)
    {
        try
        {
            var bytes = CreateTestThresholdImage();
            
        }
        catch (Exception exPrinting)
        {
            Debug.WriteLine(exPrinting.Message);
        }
    }

    private async void PrintImg(object sender, EventArgs e)
    {
        //btnPrintImg.IsEnabled = false;

        try
        {                        
            int nMode = 0;
            int nPaperWidth = 384;

            SKBitmap sKBitmap = LoadEmbeddedBitmap();
            SKBitmap sKBitmapGray = ConvertToBlackPixels(sKBitmap, 40);

            imgPrintSk.Source = ImageSource.FromStream(() => sKBitmapGray.Encode(SKEncodedImageFormat.Png, 50).AsStream());

            int width = ((nPaperWidth + 7) / 8) * 8;
            var data = POS_PrintBMP(sKBitmap, nPaperWidth, nMode);
                       
            await _printer.Print(data);
        }
        catch (Exception exPrinting)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            string text = "Error al imprimir qr:" + exPrinting.Message;
            ToastDuration duration = ToastDuration.Long;
            double fontSize = 14;
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
        }
        finally
        {
            //btm.Dispose();
            //btm = null;
            btnPrintImg.IsEnabled = true;
        }
    }

    //private async void PrintImgSlow(object sender, EventArgs e)
    //{
    //    //btnPrintImg.IsEnabled = false;

    //    try
    //    {
    //        Assembly assembly = GetType().GetTypeInfo().Assembly;
    //        //SKBitmap skBitmapForUpdate;
    //        int nMode = 0;
    //        int nPaperWidth = 384;


    //        SKBitmap sKBitmap = LoadEmbeddedBitmap();
    //        SKBitmap sKBitmapGray = ConvertToBlackPixels(sKBitmap, 40);

    //        imgPrintSk.Source = ImageSource.FromStream(() => sKBitmapGray.Encode(SKEncodedImageFormat.Png,50).AsStream());

    //        int width = ((nPaperWidth + 7) / 8) * 8;
    //        var data = POS_PrintBMP(sKBitmap, nPaperWidth, nMode);

    //        Debug.WriteLine("Cargado...");
            
    //    }
    //    catch (Exception exPrinting)
    //    {
    //        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    //        string text = "Error al imprimir qr:" + exPrinting.Message;
    //        ToastDuration duration = ToastDuration.Long;
    //        double fontSize = 14;
    //        var toast = Toast.Make(text, duration, fontSize);
    //        await toast.Show(cancellationTokenSource.Token);
    //    }
    //    finally
    //    {
    //        //btm.Dispose();
    //        //btm = null;
    //        btnPrintImg.IsEnabled = true;
    //    }
    //}

    public void Dispose()
    {
        _printer.Disconnect();        
    }

    private async void btnTurnOnCamera_Clicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.CapturePhotoAsync();

            if (photo != null)
            {                
                var photoPath = photo.FullPath;
                
                ImageSource imgData = ImageSource.FromFile(photoPath);
                imgPrint.Source = imgData;

                int nMode = 0;
                int nPaperWidth = 384;

                SKBitmap sKBitmap = null;

                using (Stream stream = File.OpenRead(photoPath))
                {
                    if (stream != null)
                    {
                        sKBitmap = SKBitmap.Decode(stream);

                        SKBitmap sKBitmapGray = ConvertToBlackPixels(sKBitmap, 40);

                        imgPrintSk.Source = ImageSource.FromStream(() =>
                            sKBitmapGray.Encode(SKEncodedImageFormat.Png, 50).AsStream()
                        );

                        int width = ((nPaperWidth + 7) / 8) * 8;
                        var data = POS_PrintBMP(sKBitmapGray, nPaperWidth, nMode);

                        await _printer.Print(data);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Manejo de errores
        }
    }
}