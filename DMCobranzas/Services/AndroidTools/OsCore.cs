using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if ANDROID
//using Android.App;
//using Android.Net.Wifi;
#endif

namespace CobranzasDMSA_Odoo.Services.AndroidTools
{
    public class OsCore
    {
        static public string GetIpDevice()
        {
#if ANDROID
                //WifiManager wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Service.WifiService);
                //int ipaddress = wifiManager.ConnectionInfo.IpAddress;
                //IPAddress ipAddr = new IPAddress(ipaddress);
                ////  System.out.println(host);  
                //return ipAddr.ToString();
#endif
            return "0.0.0.0";
        }
    }
}
