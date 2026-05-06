using DMSA.Models.Odoo.Tools;

namespace WebMobileManager.Web.Handlers
{
    public class ConnectionManager
    {
        private readonly object _lock = new object();
        private readonly List<string> _connectedUsers = new List<string>();

        List<ConnectedDevice> _connectedDevices = new List<ConnectedDevice>();

        public void AddDevice(ConnectedDevice connectedDevice)
        {
            lock (_lock)
            {
                _connectedDevices.Add(connectedDevice);
            }
        }

        public void UpdateDevice(ConnectedDevice connectedDevice)
        {
            lock (_lock)
            {
                var itemFound = _connectedDevices.Where(i => i.Id == connectedDevice.Id).FirstOrDefault();

                if (itemFound != null)
                {
                    itemFound.AppName = connectedDevice.AppName;
                    itemFound.VersionString = connectedDevice.VersionString;
                    itemFound.Platform = connectedDevice.Platform;
                    itemFound.OsVersion = connectedDevice.OsVersion;
                    itemFound.Status = connectedDevice.Status;
                    itemFound.BuildString = connectedDevice.BuildString;
                    itemFound.DeviceName = connectedDevice.DeviceName;
                    itemFound.Manufacturer = connectedDevice.Manufacturer;
                    itemFound.SerialNumber = connectedDevice.SerialNumber;
                    itemFound.PackageName = connectedDevice.PackageName;
                    itemFound.Model = connectedDevice.Model;

                    itemFound.battery = connectedDevice.battery;
                    itemFound.CurrentDateTime = connectedDevice.CurrentDateTime;
                    itemFound.CurrentLocation = connectedDevice.CurrentLocation;
                    itemFound.freeRam = connectedDevice.freeRam;
                    itemFound.freeStorage = connectedDevice.freeStorage;
                    itemFound.UserData = connectedDevice.UserData;
                }
            }
        }

        public void RemoveDevice(ConnectedDevice connectedDevice)
        {
            lock (_lock)
            {
                var itemFound = _connectedDevices.Where(i => i.Id == connectedDevice.Id).FirstOrDefault();
                if (itemFound != null)
                    _connectedDevices.Remove(itemFound);
            }
        }

        //public void AddConnection(string connectionId)
        //{
        //    lock (_lock)
        //    {
        //        _connectedUsers.Add(connectionId);
        //    }
        //}

        //public void RemoveConnection(string connectionId)
        //{
        //    lock (_lock)
        //    {
        //        _connectedUsers.Remove(connectionId);
        //    }
        //}

        //public List<string> GetConnectedUsers()
        //{
        //    lock (_lock)
        //    {
        //        return _connectedUsers.ToList();
        //    }
        //}

        public void ClearDevices()
        {
            lock (_lock)
            {
                _connectedDevices.Clear();
            }
        }

        public List<ConnectedDevice> GetDevices()
        {
            lock (_lock)
            {
                return _connectedDevices.ToList();
            }
        }
    }
}
