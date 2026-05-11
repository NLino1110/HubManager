using DMSA.Models.Odoo.Tools;
using DMSA.Sync.Core.Database.Sqlite;
using Newtonsoft.Json;

namespace DMSA.Sync.Core.Services
{    
    public class Sensors
    {
        public async Task<string> GetUserData()
        {
            var userAccessDb = new UserAccessDb(Constants.Session.odooConnection.DbNameSqlite);
            var items = await userAccessDb.GetItemsAsync(x => x.uid > 0);

            var lastItem = items
                .OrderByDescending(x => x.log_fec_acceso)
                .FirstOrDefault();

            if (lastItem == null)
                return string.Empty;
            var stringResult = JsonConvert.SerializeObject(lastItem);
            return stringResult;
        }

        public async Task<Location?> GetLocationAsync()
        {
            try
            {
                //var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                var request = new GeolocationRequest(
                        GeolocationAccuracy.Best,
                        TimeSpan.FromSeconds(10)
                    );
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
            var stringLocation = string.Empty;

            if (location != null)
                stringLocation = JsonConvert.SerializeObject(location);

            data.CurrentLocation = stringLocation;
                        
            data.battery = (long)(Battery.Default.ChargeLevel * 100);
            
            data.freeStorage = 0;
            data.storage = 0;
            data.freeRam = 0;
            data.UserData = await GetUserData();

            return data;
        }
    }
}
