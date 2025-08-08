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
        public string DateTimeInit { get; set; }
        public string Status { get; set; }
        public string UserData { get; set; }
    }
}
