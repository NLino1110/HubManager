using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using System.Diagnostics;
using System.Text;
using System.Threading;


namespace CobranzasDMSA_Odoo.Services
{
    public class BluetoothPrinterManager: IDisposable
    {
        private IBluetoothLE bluetoothLE;
        private IAdapter adapter;
        private IDevice printerDevice;
        private IService service;
        private ICharacteristic characteristic;

        string[] codebar = { "UPC_A", "UPC_E", "JAN13(EAN13)", "JAN8(EAN8)", 
							   "CODE39", "ITF", "CODABAR", "CODE93", "CODE128", "QR Code" };

        byte[][] byteCodebar = new byte[][] {
            new byte[] { 0x1B, 0x40 }, // Restablecer impresora
            new byte[] { 0x1B, 0x40 }, // Restablecer impresora
            new byte[] { 0x1B, 0x40 }, // Restablecer impresora
            new byte[] { 0x1B, 0x40 }, // Restablecer impresora
            new byte[] { 0x1B, 0x40 }, // Restablecer impresora
            new byte[] { 0x1B, 0x40 }, // Restablecer impresora
            new byte[] { 0x1B, 0x40 }, // Restablecer impresora
            new byte[] { 0x1B, 0x40 }, // Restablecer impresora
            new byte[] { 0x1B, 0x40 }, // Restablecer impresora
            new byte[] { 0x1B, 0x40 }  // Restablecer impresora
        };

        public async Task<bool> createRfcommSocketToServiceRecord(string ServiceUuidS, string CharacteristicUuidS)
        {
            //if(adapter.ConnectedDevices.Count > 0)
            //{
            //    //await adapter.DisconnectDeviceAsync(printerDevice);
            //    return false;
            //}

            //if(service!=null)
            //{
            //    return false;
            //}

            Guid ServiceUuid = new Guid(ServiceUuidS);
            Guid CharacteristicUuid = new Guid(CharacteristicUuidS);

            if (printerDevice == null)
            {
                // No se encontró la impresora Bluetooth
                return false;
            }

            var parameters = new ConnectParameters(forceBleTransport: true);
            
            await adapter.ConnectToDeviceAsync(printerDevice, parameters);

            // Aquí es donde enviarías los datos de impresión a través de BLE utilizando el valor del parámetro 'data'
            // Por ejemplo, puedes convertir 'data' en bytes y enviarlos a la impresora usando el servicio y la característica adecuados.

            service = await printerDevice.GetServiceAsync(ServiceUuid);
            characteristic = await service.GetCharacteristicAsync(CharacteristicUuid);

            //characteristic.WriteType = CharacteristicWriteType.WithResponse;
            return true;
        }

        public void setPrinterDevice(IDevice newPrinterDevice)
        {
            printerDevice = newPrinterDevice;
        }

        public BluetoothPrinterManager()
        {
            //////#if ANDROID30_0_OR_GREATER || IOS10_2_OR_GREATER
            //////            bluetoothLE = CrossBluetoothLE.Current;
            //////            adapter = bluetoothLE.Adapter; // CrossBluetoothLE.Current.Adapter;            
            //////            adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
            //////#else
            //////            //TODO: Buscar solución para WINDOWS, por ahora no se implementará
            //////            // el feature de impresión bluetooth
            //////#endif

            bluetoothLE = CrossBluetoothLE.Current;
            adapter = bluetoothLE.Adapter; // CrossBluetoothLE.Current.Adapter;            
            adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
        }

        public async Task<IEnumerable<IDevice>> ScanForPrinters()
        {
            await adapter.StartScanningForDevicesAsync();
            await Task.Delay(5000); // Escanea durante 5 segundos
            await adapter.StopScanningForDevicesAsync();
            
            return adapter.GetSystemConnectedOrPairedDevices();
        }

        public async Task<IEnumerable<IDevice>> GetPairedDevicesAsync()
        {
            await Task.Delay(500);
            return adapter.GetSystemConnectedOrPairedDevices();
        }

        public IEnumerable<IDevice> GetPairedDevices()
        {
            return adapter.GetSystemConnectedOrPairedDevices();
        }

        private void Adapter_DeviceDiscovered(object sender, DeviceEventArgs e)
        {
            // Verifica si el dispositivo descubierto es tu impresora Bluetooth
            if (e.Device.Name == "BlueTooth Printer")
            {
                printerDevice = e.Device;
                adapter.DeviceDiscovered -= Adapter_DeviceDiscovered; // Deja de escuchar para evitar duplicados
            }
        }

        public async Task<bool> Print(string data)
        {
            if(characteristic == null)
            {
                return false;
            }

            //var bytes = Encoding.ASCII.GetBytes("ASCII " + data + "-" + DateTime.Now.ToString() + "\n");
            //await characteristic.WriteAsync(bytes);

            //var bytes = Encoding.UTF8.GetBytes(data);
            var bytes = Encoding.ASCII.GetBytes(data);

            CancellationToken cancellationPrint = new CancellationToken();
            await characteristic.WriteAsync(bytes, cancellationPrint);

            //await Task.Delay(2000);
            //await characteristic.StartUpdatesAsync();
            //await characteristic.StopUpdatesAsync();
            //await adapter.DisconnectDeviceAsync(printerDevice);

            //service.Dispose();
            //characteristic = null;
            
            return true;
        }

        public async Task<bool> PrintBytes(byte[] data)
        {
            CancellationToken cancellationPrint = new CancellationToken();
            //characteristic.WriteType = CharacteristicWriteType.Default;
            await characteristic.WriteAsync(data, cancellationPrint);
            return true;
        }

        /// <summary>
        /// Se utiliza este método para simular la forma de Java-Android, ya que al tratar de imprimir
        /// imagenes se comporta de modo extraño.
        /// Al enviarlo de este modo si logra imprimir las imágenes pero aún se comporta extraño, imprime
        /// lento y en menor calidad.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> PrintBytesChunk(byte[] data)
        {
            CancellationToken cancellationPrint = new CancellationToken();
            int len = data.Length;
            const int chunkSize = 256;
            // Escribir los bytes en el flujo
            for (int i = 0; i < data.Length; i += chunkSize)
            {   
                int remainingBytes = Math.Min(chunkSize, data.Length - i);
                byte[] chunk = new byte[remainingBytes];
                Array.Copy(data, i, chunk, 0, remainingBytes);

                string texto = Encoding.Default.GetString(chunk);
                await characteristic.WriteAsync(chunk, cancellationPrint);
                //Debug.WriteLine(texto);
            }

            return true;
        }

        public async Task ExploreDevice()// IDevice device)
        {
            IDevice device = printerDevice;
            var parameters = new ConnectParameters(forceBleTransport: true);
            // Conéctate al dispositivo
            //await adapter.ConnectToDeviceAsync(device, parameters);

            // Obtén los servicios disponibles en el dispositivo
            var services = await device.GetServicesAsync();

            foreach (var service in services)
            {
                //Console.WriteLine($"Servicio: {service.Id}, UUID: {service.Uuid}");
                Console.WriteLine($"--------------------------------------------------------");
                Console.WriteLine($"Servicio: {service.Name} {service.Id}");
                
                // Obtén las características del servicio
                var characteristics = await service.GetCharacteristicsAsync();

                foreach (var characteristic in characteristics)
                {
                    Console.WriteLine($"--------------------------------------------------------");
                    Console.WriteLine($"Característica: {characteristic.Name} {characteristic.Id}, UUID: {characteristic.Uuid}");
                    Console.WriteLine($"Can write: {characteristic.CanWrite}");
                    Console.WriteLine("Props:" + characteristic.Properties.ToString());

                    //Unknown characteristic
                    //WriteWithoutResponse, Write

                    if (characteristic.CanWrite && characteristic.Name.Equals("Unknown characteristic") &&
                        characteristic.Properties.ToString().Equals("WriteWithoutResponse, Write"))
                    {
                        Console.WriteLine($"SERVICIO DE IMPRESION: {service.Id}");
                        Console.WriteLine($"CARACTERISTICA DE IMPRESION: {characteristic.Id}");                        

                        //foreach(var props in characteristic.Properties)
                        //{

                        //}
                    }
                }
            }

            // Desconéctate del dispositivo
            //await adapter.DisconnectDeviceAsync(device);
        }


        public async Task PrintBarCode(int which, string str)
        {
            await PrintBytes(byteCodebar[which]);

            //string str = editText.getText().toString();
            if (which == 0)
            {
                if (str.Length == 11 || str.Length == 12)
                {
                    byte[] code = PrinterCommand.GetCodeBarCommand(str, 65, 3, 168, 0, 2);
                    await PrintBytes(new byte[] { 0x1b, 0x61, 0x00 });
                    await Print("UPC_A\n");
                    await PrintBytes(code);
                }
                else
                {
                    //Toast.makeText(Main_Activity.this, getText(R.string.msg_error), Toast.LENGTH_SHORT).show();
                    return;
                }
            }
            else if (which == 1)
            {
                if (str.Length == 6 || str.Length == 7)
                {
                    byte[] code = PrinterCommand.GetCodeBarCommand(str, 66, 3, 168, 0, 2);
                    await PrintBytes(new byte[] { 0x1b, 0x61, 0x00 });
                    await Print("UPC_E\n");
                    await PrintBytes(code);
                }
                else
                {
                    //Toast.makeText(Main_Activity.this, getText(R.string.msg_error), Toast.LENGTH_SHORT).show();
                    return;
                }
            }
            else if (which == 2)
            {
                if (str.Length == 12 || str.Length == 13)
                {
                    byte[] code = PrinterCommand.GetCodeBarCommand(str, 67, 3, 168, 0, 2);
                    await PrintBytes(new byte[] { 0x1b, 0x61, 0x00 });
                    await Print("JAN13(EAN13)\n");
                    await PrintBytes(code);
                }
                else
                {
                    //Toast.makeText(Main_Activity.this, getText(R.string.msg_error), Toast.LENGTH_SHORT).show();
                    return;
                }
            }
            else if (which == 3)
            {
                if (str.Length > 0)
                {
                    byte[] code = PrinterCommand.GetCodeBarCommand(str, 68, 3, 168, 0, 2);
                    await PrintBytes(new byte[] { 0x1b, 0x61, 0x00 });
                    await Print("JAN8(EAN8)\n");
                    await PrintBytes(code);
                }
                else
                {
                    //Toast.makeText(Main_Activity.this, getText(R.string.msg_error), Toast.LENGTH_SHORT).show();
                    return;
                }
            }
            else if (which == 4)
            {
                if (str.Length == 0)
                {
                    //Toast.makeText(Main_Activity.this, getText(R.string.msg_error), Toast.LENGTH_SHORT).show();
                    return;
                }
                else
                {
                    byte[] code = PrinterCommand.GetCodeBarCommand(str, 69, 3, 168, 1, 2);
                    await Print("CODE39\n");
                    await PrintBytes(new byte[] { 0x1b, 0x61, 0x00 });
                    await PrintBytes(code);
                }
            }
            else if (which == 5)
            {
                if (str.Length == 0)
                {
                    //Toast.makeText(Main_Activity.this, getText(R.string.msg_error), Toast.LENGTH_SHORT).show();
                    return;
                }
                else
                {
                    byte[] code = PrinterCommand.GetCodeBarCommand(str, 70, 3, 168, 1, 2);
                    await Print("ITF\n");
                    await PrintBytes(new byte[] { 0x1b, 0x61, 0x00 });
                    await PrintBytes(code);
                }
            }
            else if (which == 6)
            {
                if (str.Length == 0)
                {
                    //Toast.makeText(Main_Activity.this, getText(R.string.msg_error), Toast.LENGTH_SHORT).show();
                    return;
                }
                else
                {
                    byte[] code = PrinterCommand.GetCodeBarCommand(str, 71, 3, 168, 1, 2);
                    await Print("CODABAR\n");
                    await PrintBytes(new byte[] { 0x1b, 0x61, 0x00 });
                    await PrintBytes(code);
                }
            }
            else if (which == 7)
            {
                if (str.Length == 0)
                {
                    //Toast.makeText(Main_Activity.this, getText(R.string.msg_error), Toast.LENGTH_SHORT).show();
                    return;
                }
                else
                {
                    byte[] code = PrinterCommand.GetCodeBarCommand(str, 72, 3, 168, 1, 2);
                    await Print("CODE93\n");
                    await PrintBytes(new byte[] { 0x1b, 0x61, 0x00 });
                    await PrintBytes(code);
                }
            }
            else if (which == 8)
            {
                if (str.Length == 0)
                {
                    //Toast.makeText(Main_Activity.this, getText(R.string.msg_error), Toast.LENGTH_SHORT).show();
                    return;
                }
                else
                {
                    byte[] code = PrinterCommand.GetCodeBarCommand(str, 73, 3, 168, 1, 2);
                    await Print("CODE128\n");
                    await PrintBytes(new byte[] { 0x1b, 0x61, 0x00 });
                    await PrintBytes(code);
                }
            }
            else if (which == 9)
            {
                if (str.Length == 0)
                {
                    //Toast.makeText(Main_Activity.this, getText(R.string.empty1), Toast.LENGTH_SHORT).show();
                    return;
                }
                else
                {
                    byte[] code = PrinterCommand.GetBarCommand(str, 1, 3, 8);
                    await Print("QR Code\n");
                    await PrintBytes(new byte[] { 0x1b, 0x61, 0x00 });
                    await PrintBytes(code);
                }
            }
        }


        public void Dispose()
        {            
            if (printerDevice != null)
            {
                adapter.DisconnectDeviceAsync(printerDevice).Wait();
                printerDevice.Dispose();
                printerDevice = null;
            }

            bluetoothLE = null;
            adapter = null;

            if (service != null)
            {
                service.Dispose();
                service = null;
            }

            if(characteristic != null)
            {
                characteristic = null;
            }
            //GC.Collect();            
            //throw new NotImplementedException();
        }
    }
}
