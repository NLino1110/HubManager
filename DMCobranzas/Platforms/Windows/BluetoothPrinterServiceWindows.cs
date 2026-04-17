using DMCobranzas.Services;
using global::Windows.Devices.Bluetooth;
using global::Windows.Devices.Bluetooth.Rfcomm;
using global::Windows.Networking.Sockets;
using global::Windows.Storage.Streams;
using Windows.Devices.Enumeration;

namespace DMCobranzas
{
    public class BluetoothPrinterServiceWindows : IPrinterService
    {
        private StreamSocket socket;
        private DataWriter writer;

        public async Task<bool> Connect(string deviceId)
        {
            var device = await BluetoothDevice.FromIdAsync(deviceId);

            if (device == null)
                return false;

            var services = await device.GetRfcommServicesAsync();

            var service = services.Services
                .FirstOrDefault(s => s.ServiceId.Uuid == RfcommServiceId.SerialPort.Uuid);

            if (service == null)
                return false;

            socket = new StreamSocket();
            await socket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName);

            writer = new DataWriter(socket.OutputStream);

            return true;
        }

        public async Task<bool> Print(byte[] data)
        {
            writer.WriteBytes(data);
            await writer.StoreAsync();
            return true;
        }

        public async Task Disconnect()
        {
            writer?.DetachStream();
            writer?.Dispose();
            socket?.Dispose();
        }

        public Task PrintBarCode(int which, string str)
        {
            throw new NotImplementedException();
        }

        public async Task<List<BluetoothDeviceInfo>> GetPairedDevicesAsync()
        {
            var list = new List<BluetoothDeviceInfo>();

            // Selector oficial de dispositivos Bluetooth
            var selector = BluetoothDevice.GetDeviceSelector();

            var devices = await DeviceInformation.FindAllAsync(selector);

            foreach (var device in devices)
            {
                // Solo dispositivos emparejados
                //if (device.Pairing?.IsPaired == true)                
                //{
                    list.Add(new BluetoothDeviceInfo
                    {
                        Name = device.Name,
                        Address = device.Id
                    });
                //}
            }

            return list;
        }

        List<BluetoothDeviceInfo> IPrinterService.GetPairedDevices()
        {
            throw new NotImplementedException();
        }
    }
}
