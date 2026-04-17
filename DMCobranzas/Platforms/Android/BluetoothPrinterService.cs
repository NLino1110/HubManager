using Android.Bluetooth;
using Java.Util;
using Android.Content;
using DMCobranzas.Platforms.Android;
using DMCobranzas.Services;
using Java.IO;
using System.Text;

namespace DMCobranzas
{
    public class BluetoothPrinterService : IPrinterService
    {
        BluetoothAdapter adapter;
        BroadcastReceiver receiver;
        BluetoothSocket socket;
        System.IO.Stream outputStream;

        private static readonly Encoding PrinterEncoding =  Encoding.GetEncoding(437);

        public async Task<bool> Connect(string macAddress)
        {
            var adapter = BluetoothAdapter.DefaultAdapter;

            if (adapter.IsDiscovering)
                adapter.CancelDiscovery();

            if (socket != null && socket.IsConnected)
                await Disconnect();

            var device = adapter.GetRemoteDevice(macAddress);

            UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"); // SPP

            socket = device.CreateRfcommSocketToServiceRecord(uuid);

            await socket.ConnectAsync();

            //Esto podría necesitarse a futuro
            //await Task.Run(() => socket.Connect());

            outputStream = socket.OutputStream;

            return true;
        }

        public async Task Disconnect()
        {
            try
            {
                StopDiscovery();

                if (outputStream != null)
                {
                    await outputStream.FlushAsync();
                    outputStream.Close();
                    outputStream.Dispose();
                    outputStream = null;
                }

                if (socket != null)
                {
                    socket.Close();
                    socket.Dispose();
                    socket = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BT Disconnect error: {ex.Message}");
            }
        }

        public async Task<bool> Print(byte[] data)
        {
            await outputStream.WriteAsync(data, 0, data.Length);
            await outputStream.FlushAsync();
            return true;
        }

        public List<BluetoothDeviceInfo> GetPairedDevices()
        {
            var list = new List<BluetoothDeviceInfo>();

            var adapter = BluetoothAdapter.DefaultAdapter;

            if (adapter == null || !adapter.IsEnabled)
                return list;

            var devices = adapter.BondedDevices;

            foreach (var device in devices)
            {
                list.Add(new BluetoothDeviceInfo
                {
                    Name = device.Name,
                    Address = device.Address
                });
            }

            return list;
        }

        //public async Task PrintText(string text)
        //{
        //    byte[] bytes = PrinterEncoding.GetBytes(text);
        //    await Print(bytes);
        //}

        [Obsolete("Metodo no confiable, requiere revisión")]
        private async Task PrintText(string text)
        {
            var encoding = Encoding.GetEncoding(437);

            List<byte> buffer = new List<byte>();

            // reset
            buffer.AddRange(new byte[] { 0x1B, 0x40 });

            // tamaño normal
            buffer.AddRange(new byte[] { 0x1D, 0x21, 0x00 });

            // alineación izquierda
            buffer.AddRange(new byte[] { 0x1B, 0x61, 0x00 });

            buffer.AddRange(encoding.GetBytes(text));

            await Print(buffer.ToArray());
        }

        private byte[] Txt(string text)
        {
            return PrinterEncoding.GetBytes(text);
        }

        public void StartDiscovery(Action<BluetoothDeviceInfo> onDeviceFound)
        {
            adapter = BluetoothAdapter.DefaultAdapter;

            if (adapter.IsDiscovering)
                adapter.CancelDiscovery();

            receiver = new DeviceFoundReceiver(onDeviceFound);

            Android.App.Application.Context.RegisterReceiver(
                receiver,
                new Android.Content.IntentFilter(BluetoothDevice.ActionFound));

            adapter.StartDiscovery();
        }

        public void StopDiscovery()
        {
            if (adapter?.IsDiscovering == true)
                adapter.CancelDiscovery();

            if (receiver != null)
                Android.App.Application.Context.UnregisterReceiver(receiver);
        }

        public Task<List<BluetoothDeviceInfo>> GetPairedDevicesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
