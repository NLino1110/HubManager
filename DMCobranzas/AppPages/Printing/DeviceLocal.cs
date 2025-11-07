using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCobranzas.AppPages.Printing
{
    public class DeviceLocal : Plugin.BLE.Abstractions.Contracts.IDevice
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int Rssi { get; set; }

        public object NativeDevice { get; set; }

        public DeviceState State { get; set; }

        public IList<AdvertisementRecord> AdvertisementRecords { get; set; }

        public bool IsConnectable => throw new NotImplementedException();

        public bool SupportsIsConnectable => throw new NotImplementedException();

        public DeviceBondState BondState => throw new NotImplementedException();

        IReadOnlyList<AdvertisementRecord> IDevice.AdvertisementRecords => throw new NotImplementedException();

        public void ClearServices()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<IService> GetServiceAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IService>> GetServicesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> RequestMtuAsync(int requestValue)
        {
            throw new NotImplementedException();
        }

        public Task<int> RequestMtuAsync(int requestValue, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public bool UpdateConnectionInterval(ConnectionInterval interval)
        {
            throw new NotImplementedException();
        }

        public bool UpdateConnectionParameters(ConnectParameters connectParameters = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateRssiAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateRssiAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
