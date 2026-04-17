namespace DMCobranzas.Services
{
    public class BluetoothDeviceInfo
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public interface IPrinterService
    {
        Task<bool> Connect(string macAddress);
        Task Disconnect();
        Task<bool> Print(byte[] data);
        List<BluetoothDeviceInfo> GetPairedDevices();
        Task<List<BluetoothDeviceInfo>> GetPairedDevicesAsync();
    }
}
