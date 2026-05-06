using System;
using System.Collections.Generic;
using System.Text;
using DMSA.Models.Odoo.Tools;
using Microsoft.Maui.Devices.Sensors;

namespace DMDataSafe.Utils
{
    public class Sensors
    {
        public async Task<Location?> GetLocationAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request);
                return location;
            }
            catch (Exception ex)
            {                
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<ConnectedDevice> GetDeviceDataAsync()
        {
            var data = new ConnectedDevice();
                        
            data.CurrentDateTime = DateTime.Now;
                        
            var location = await GetLocationAsync();
            data.CurrentLocation = location != null
                ? $"{location.Latitude}, {location.Longitude}"
                : null;
                        
            data.battery = (long)(Battery.Default.ChargeLevel * 100);
                        
            data.freeStorage = 0;
            data.storage = 0;
            data.freeRam = 0;

            return data;
        }

        //public async Task<ConnectedDevice> GetDeviceDataWithInfoAsync()
        //{   
        //    ConnectedDevice device = new ConnectedDevice();
        //    device.Id = "0";
        //    device.AppName = AppInfo.Current.Name;
        //    device.PackageName = AppInfo.Current.PackageName;
        //    device.VersionString = App.Session.AppVersion;
        //    device.BuildString = AppInfo.Current.BuildString;
        //    device.UserData = "";
        //    device.Idiom = DeviceInfo.Current.Idiom.ToString();
        //    device.Manufacturer = DeviceInfo.Current.Manufacturer;
        //    device.DeviceName = DeviceInfo.Current.Name;
        //    device.OsVersion = DeviceInfo.Current.VersionString;
        //    device.Platform = DeviceInfo.Current.Platform.ToString();
        //    device.SerialNumber = "";
        //    device.Model = DeviceInfo.Current.Model;
        //    return device;
        //}

    }
}
