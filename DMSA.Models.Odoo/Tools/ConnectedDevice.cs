using DMSA.Models.Odoo.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Tools
{   
    public class ConnectedDevice
    {
        public string Id { get; set; }
        public string DeviceId { get; set; }
        public string PackageName { get; set; }
        public string BuildString { get; set; }
        public string VersionString { get; set; }
        public string Idiom { get; set; }
        public string Platform { get; set; }
        public string AppName { get; set; }
        public string DeviceName { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public string OsVersion { get; set; }
        public DateTime? DateTimeInit { get; set; }
        public string Status { get; set; }        
        public string? UserData { get; set; }

        //================================================//

        //public string? DeviceId { get; set; }        
        public DateTime? CurrentDateTime { get; set; }
        public string? CurrentLocation { get; set; }
        public long battery { get; set; }
        public long freeStorage { get; set; }
        public long freeRam { get; set; }
        public long storage { get; set; }
        public string? BtMAddr { get; set; }

        //================================================//
        //===============SETTING==========================//

        public bool WithRememberMe { get; set; }
        public bool WithAutoLogin { get; set; }


        
        [JsonIgnore]
        public string? MapsUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CurrentLocation))
                    return null;

                try
                {
                    var location = JsonConvert.DeserializeObject<LocationDto>(CurrentLocation);

                    if (location == null)
                        return null;

                    return $"https://www.google.com/maps?q={location.Latitude},{location.Longitude}";
                }
                catch
                {
                    return null;
                }
            }
        }



        [JsonIgnore]
        public string? LastUserLogged
        {
            get
            {
                var data = GetUserData();
                return data?.username ?? data?.name;
            }
        }

        [JsonIgnore]
        public DateTime? LastDateLogged
        {
            get
            {
                var data = GetUserData();
                return data?.log_fec_acceso;
            }
        }
                
        private user_access? GetUserData()
        {
            if (string.IsNullOrWhiteSpace(UserData))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<user_access>(UserData);
            }
            catch
            {
                return null;
            }
        }
    }

    public class LocationDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
