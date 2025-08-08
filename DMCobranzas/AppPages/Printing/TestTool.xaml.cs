
using CobranzasDMSA_Odoo.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using SkiaSharp;
using SkiaSharp.QrCode;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CobranzasDMSA_Odoo.AppPages.Printing;

public partial class TestTool : ContentPage, IDisposable //, INotifyPropertyChanged
{
    BluetoothPrinterManager btPrinterManager { get; set; }

    ObservableCollection<DeviceLocal> m_deviceList { get; set; }
    //public List<DeviceLocal> deviceList
    //{
    //    get { return m_deviceList; }
    //    set
    //    {
    //        m_deviceList = value;
    //        OnPropertyChanged(nameof(deviceList));
    //    }
    //}

    //public event PropertyChangedEventHandler PropertyChanged;

    //protected override void OnPropertyChanged(string propertyName)
    //{
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //}    

    public TestTool()
	{
        //Scan(null, null);
        InitializeComponent();
        m_deviceList = new ObservableCollection<DeviceLocal>();
        
        const string ESC = "\x1B";
        const string NewLine = "\n";

        // Iniciar impresión
        string initializePrinter = ESC + "@";
        string setAlignmentCenter = ESC + "a" + "1";

        //string bodyPrint = "MACRONEGOCIOS S.A.\r\nRECIBO \r\nCLIENTE 11 JORGE CEVALLOS ORTEGA\r\nEstado PENDIENTE\r\n\r\n============F.PAGO===========\r\nCAJA COBRANZA         $ 11\r\n    ===========DOCUMENTOS=========\r\n    Fact 200-201-000000005         $1\r\n    Fact 200-201-000000026         $3\r\n    Fact 200-201-000000046         $3\r\n    NDCM3/2023/00001         $3\r\n________________\r\nTOTAL F.P.:     $ 11\r\nTOTAL CANC:     $ 10\r\n\r\n================================\r\nTOTAL FACT. Pendientes: $ -10\r\n================================\r\n\r\nEmail: jcevallos@ccs-ep.com\r\nVnd: Administrator 125\r\nFecha: 2023-11-20 21:57:04Z\r\n\r\n\r\n________________________________\r\n             -Firma Cliente-\r\n\r\n";
        string bodyPrint = "MACRONEGOCIOS S.A.\r\nRECIBO \r\n";

        // Título del recibo
        string title = "BIENVENIDO!"; // "!E¡BIENVENIDO!";
        string separator = "------"; // "! E -------------------------";

        // Contenido del recibo
        string item1 = "Producto 1";
        string item2 = "Producto 2";
        string item3 = "Producto 3";
        
        string price1 = "$10.00";
        string price2 = "$15.00";
        string price3 = "$20.00";

        // Pie del recibo
        string totalLabel = "TOTAL:";
        string totalAmount = "$45.00";

        // Comandos de corte y avance de línea
        string cutPaper = ESC + "d" + "\x08";
        string lineFeed = NewLine + NewLine;

        // Combinar todos los elementos en un solo recibo
        string receipt = initializePrinter +
                         setAlignmentCenter +
                         title +
                         NewLine +
                         separator +
                         NewLine +
                         item1 + "\t" + price1 + NewLine +
                         item2 + "\t" + price2 + NewLine + 
                         "---------------------" + "\xA" +
                         item3 + "\t" + price3 + NewLine +
                         separator +
                         NewLine +
                         totalLabel + "\t" + totalAmount +
                         lineFeed +
                         cutPaper;


        //editor.Text = receipt;
        //editor.Text = "Prueba";

        editor.Text = bodyPrint;

        btPrinterManager = new BluetoothPrinterManager();

        SetImage();

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

    void SetImage()
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string[] namesass = assembly.GetManifestResourceNames();

        Stream stream = assembly.GetManifestResourceStream("CobranzasDMSA.Resources.Images.demo.bmp");        
        
        imgPrint.Source = ImageSource.FromStream(() => stream);

            //// Convierte la imagen a un objeto SKBitmap
            //SKBitmap skBitmap;
            ////using (var stream = imageSource.GetStream())
            ////{
            //using (var skData = SKData.Create(stream))
            //{
            //    skBitmap = SKBitmap.Decode(skData);
            //}
            ////}

            //// Convierte el SKBitmap a bytes de bitmap
            //byte[] bitmapBytes;
            //using (var skImage = SKImage.FromBitmap(skBitmap))
            //{
                
                

            //    //using (var skData = skImage.Encode(SKEncodedImageFormat.Bmp, 50))
            //    using (var skData = skImage.Encode())
            //    {
            //        bitmapBytes = skData.ToArray();
            //    }
            //}
        
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
        m_deviceList.Clear();

        try
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            string text = "No se han encontrado dispositivos emparejados, agregue la impresora y vuelva a intentar...";
            ToastDuration duration = ToastDuration.Long;
            double fontSize = 14;

            //var foundDevices = await btm.ScanForPrinters();
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
                    //await btm.Print("Prueba", "00001101-0000-1000-8000-00805F9B34FB", "00001101-0000-1000-8000-00805F9B34FB");
                    //Para la impresora vieja
                    await btPrinterManager.createRfcommSocketToServiceRecord("000018f0-0000-1000-8000-00805f9b34fb", "00002af1-0000-1000-8000-00805f9b34fb");

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

            myListView.ItemsSource = m_deviceList;
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
            await btPrinterManager.ExploreDevice();            
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
            await btPrinterManager.Print(editor.Text);
        }
        catch(Exception exPrinting)
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
            btnScan.IsEnabled = true;
        }

        //deviceList = m_deviceList;

        //var ble = CrossBluetoothLE.Current;
        //var adapter = CrossBluetoothLE.Current.Adapter;
        //Console.WriteLine(ble);
        //Console.WriteLine(adapter);
        //ble.StateChanged += (s, e) =>
        //{
        //    Debug.WriteLine($"The bluetooth state changed to {e.NewState}");
        //};

        //deviceList.Clear();
        //adapter.DeviceDiscovered += (s, a) =>
        //{
        //    Console.WriteLine("Id:" + a.Device.Id);
        //    Console.WriteLine("Nd:" + a.Device.NativeDevice);
        //    Console.WriteLine("State:" + a.Device.State.ToString());
        //    Console.WriteLine("Nombre:" + a.Device.Name);

        //    //deviceList.Add((DeviceLocal) a.Device);
        //    deviceList.Add(new DeviceLocal()
        //    {
        //        Name = a.Device.Name,
        //    });
        //    m_deviceList = deviceList;
        //};

        //await adapter.StartScanningForDevicesAsync();

        //var systemDevices = adapter.GetSystemConnectedOrPairedDevices();
        //foreach (var device in systemDevices)
        //{
        //    await adapter.ConnectToDeviceAsync(device);            
        //    //adapter.ConnectedDevices[0].
        //}

        ////var scanFilterOptions = new ScanFilterOptions();
        ////scanFilterOptions.ServiceUuids = new[] { guid1, guid2, etc }; // cross platform filter
        ////scanFilterOptions.ManufacturerDataFilters = new[] { new ManufacturerDataFilter(1), new ManufacturerDataFilter(2) }; // android only filter
        ////scanFilterOptions.DeviceAddresses = new[] { "80:6F:B0:43:8D:3B", "80:6F:B0:25:C3:15", etc }; // android only filter
        ////scanFilterOptions.DeviceNames = { "printer" };
        ////await adapter.StartScanningForDevicesAsync(scanFilterOptions);
    }

    private async void PrintBarCode(object sender, EventArgs e)
    {
        btnPrintBarCode.IsEnabled = false;

        try
        {
            //Ejemplo Code-39
            await btPrinterManager.PrintBarCode(4, "RCH-1381");            
        }
        catch (Exception exPrinting)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            string text = "Error al imprimir codigo de barra:" + exPrinting.Message;
            ToastDuration duration = ToastDuration.Long;
            double fontSize = 14;
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
        }
        finally
        {
            //btm.Dispose();
            //btm = null;
            btnPrintBarCode.IsEnabled = true;
        }
    }

    private async void PrintQr(object sender, EventArgs e)
    {
        btnPrintQr.IsEnabled = false;

        try
        {
            //var content = "https://www.dmujeres.ec/";
            //using var generator = new QRCodeGenerator();

            //// Generate QrCode
            //var qr = generator.CreateQrCode(content, ECCLevel.L);

            //// Render to canvas
            //var info = new SKImageInfo(50, 50);
            //using var surface = SKSurface.Create(info);
            //var canvas = surface.Canvas;
            //canvas.Render(qr, info.Width, info.Height);

            //// Output to Stream -> File
            //using var image = surface.Snapshot();
            //using var data = image.Encode(SKEncodedImageFormat.Png, 60);
            ////using var stream = File.OpenWrite(@"output/hoge.png");
            ////data.SaveTo(stream);
            //using var stream = new MemoryStream();
            //data.SaveTo(stream);
            //byte[] qrData = stream.ToArray();
            //await btPrinterManager.PrintBytes(qrData);

            byte[] qrcode = PrinterCommand.GetBarCommand("https://www.dmujeres.ec/", 0, 3, 6);//
            PrintCommandBytes.ESC_Align[2] = 0x01;
            await btPrinterManager.PrintBytes(PrintCommandBytes.ESC_Align);
            await btPrinterManager.PrintBytes(qrcode);

            await btPrinterManager.PrintBytes(PrintCommandBytes.ESC_Align);
            PrintCommandBytes.GS_ExclamationMark[2] = 0x11;
            await btPrinterManager.PrintBytes(PrintCommandBytes.GS_ExclamationMark);
            await btPrinterManager.PrintBytes(Encoding.UTF8.GetBytes("DMujeres S.A.\n"));
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
            await btPrinterManager.PrintBytes(PrinterCommand.POS_Set_PrtAndFeedPaper(48));
            await btPrinterManager.PrintBytes(PrintCommandBytes.GS_V_m_n);            
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

    private async void PrintGraphical(object sender, EventArgs e)
    {
        btnPrintQr.IsEnabled = false;

        try
        {           
            byte[] qrcode = PrinterCommand.GetBarCommand("https://www.dmujeres.ec/", 0, 3, 6);//
            PrintCommandBytes.ESC_Align[2] = 0x01;
            await btPrinterManager.PrintBytes(PrintCommandBytes.ESC_Align);
            await btPrinterManager.PrintBytes(qrcode);

            await btPrinterManager.PrintBytes(PrintCommandBytes.ESC_Align);
            PrintCommandBytes.GS_ExclamationMark[2] = 0x11;
            await btPrinterManager.PrintBytes(PrintCommandBytes.GS_ExclamationMark);
            await btPrinterManager.PrintBytes(Encoding.UTF8.GetBytes("DMujeres S.A.\n"));
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
            await btPrinterManager.PrintBytes(PrinterCommand.POS_Set_PrtAndFeedPaper(48));
            await btPrinterManager.PrintBytes(PrintCommandBytes.GS_V_m_n);
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

    public SKImage LoadEmbeddedImage()
    {
        // Reemplaza "NombreProyecto.NombreCarpetaImagenes.nombre_imagen.png" con la ruta completa del recurso incrustado.
        string resourceName = "CobranzasDMSA.Resources.Images.demo2.png";

        // Obtén el ensamblado actual.
        var assembly = Assembly.GetExecutingAssembly();

        // Lee el recurso incrustado como una secuencia de bytes.
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream != null)
            {
                // Crea un SKImage a partir de la secuencia de bytes.
                return SKImage.FromEncodedData(stream);
            }
        }

        return null; // En caso de que la carga falle, devolver null.
    }

    public SKBitmap LoadEmbeddedBitmap()
    {
        // Reemplaza "NombreProyecto.NombreCarpetaImagenes.nombre_imagen.png" con la ruta completa del recurso incrustado.
        string resourceName = "CobranzasDMSA.Resources.Images.demo2.bmp";

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

    private async void PrintImg(object sender, EventArgs e)
    {
        //btnPrintImg.IsEnabled = false;

        try
        {

            Assembly assembly = GetType().GetTypeInfo().Assembly;
            //SKBitmap skBitmapForUpdate;
            int nMode = 0;
            int nPaperWidth = 384;

            //SKBitmap skBitmap = LoadEmbeddedImage();
            //var streamT = assembly.GetManifestResourceStream("CobranzasDMSA.Resources.Images.demo.bmp");
            //////SetImage(streamT);            
            //var memoryStream = new MemoryStream();
            //streamT.CopyTo(memoryStream);
            //byte[] data = memoryStream.ToArray();

            // Crea un SKImage a partir del SKBitmap.
            SKImage skImage = LoadEmbeddedImage();
            var skImageGray = _ConvertToBlackPixels(skImage);

            //SKBitmap sKBitmap = LoadEmbeddedBitmap();
            //SKBitmap sKBitmapGray = ConvertToBlackPixels(sKBitmap, 40);

            // Asigna la imagen al control Image.

            imgPrintSk.Source = ImageSource.FromStream(() => skImageGray.Encode().AsStream());
            //imgPrintSk.Source = ImageSource.FromStream(() => sKBitmapGray.Encode(SKEncodedImageFormat.Png,50).AsStream());


            int width = ((nPaperWidth + 7) / 8) * 8;
            //var data = POS_PrintBMP(skBitmap, nPaperWidth, nMode);

            Debug.WriteLine("Cargado...");

            //data = Zj.Com.Customize.Sdk.Other.ThresholdToBWPic(data);
            //data = Zj.Com.Customize.Sdk.Other.EachLinePixToCmd(data, width, nMode);

            //byte[] data = POS_PrintBMP(skBitmap, nPaperWidth, nMode);
            //byte[] data = bitmapBytes;


            ////SKBitmap skBitmapT;
            ////using (var skData = SKData.Create(streamT))
            ////{
            ////    skBitmapT = SKBitmap.Decode(skData);                
            ////}

            ////SKBitmap sknew = PrepareBMP(skBitmapT, nPaperWidth, nMode);
            ////var data = sknew.Encode(SKEncodedImageFormat.Png, 50);
            ////var memoryStream = new MemoryStream();
            ////data.SaveTo(memoryStream);
            //memoryStream.Write(sknew.Bytes, 0, sknew.Bytes.Length);


            //return memoryStream;
            //SetImage2();




            //await btPrinterManager.PrintBytes(PrintCommandBytes.ESC_Init);
            ////Esta linea no eliminar, colocarla cuando ya funcione
            //await btPrinterManager.PrintBytes(PrintCommandBytes.LF);

            ////await btPrinterManager.PrintBytes(data);
            ////await btPrinterManager.PrintBytesChunk(data);

            //await btPrinterManager.PrintBytes(PrinterCommand.POS_Set_PrtAndFeedPaper(30));
            //await btPrinterManager.PrintBytes(PrinterCommand.POS_Set_Cut(1));
            //await btPrinterManager.PrintBytes(PrinterCommand.POS_Set_PrtInit());            


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


    public void Dispose()
    {
        btPrinterManager.Dispose();
        //throw new NotImplementedException();
    }

    private async void btnTurnOnCamera_Clicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.CapturePhotoAsync();

            if (photo != null)
            {
                // Obtener la ruta de la foto tomada
                var photoPath = photo.FullPath;

                // Cargar la imagen en el contenedor Image
                imgPrint.Source = ImageSource.FromFile(photoPath);
            }
        }
        catch (Exception ex)
        {
            // Manejo de errores
        }
    }
}