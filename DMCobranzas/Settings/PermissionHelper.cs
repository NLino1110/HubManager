using CobranzasDMSA_Odoo.Settings.helpers;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Settings;

public static class PermissionHelper
{
    public static async Task<bool> AskForBluetoothAdminPermission()
    {
        bool resultP = false;
        
        try
        {
            var permission = await Permissions.CheckStatusAsync<Permissions.NetworkState>();

            if (permission != PermissionStatus.Granted)
            {                
                permission = await Permissions.RequestAsync<Permissions.NetworkState>();
            }
            else
            {
                resultP = true;
            }

            if (permission != PermissionStatus.Granted)
            {
                resultP = false;
            }
        }
        catch (Exception ex)
        {

        }

        return resultP;
    }

    static public async Task CheckBluetoothAccess()
    {
        if (DeviceInfo.Platform != DevicePlatform.Android)
            return;

        var status = PermissionStatus.Unknown;

        if (DeviceInfo.Version.Major >= 12)
        {
            status = await Permissions.CheckStatusAsync<MyBluetoothPermission>();

            if (status == PermissionStatus.Granted)
                return;

            if (Permissions.ShouldShowRationale<MyBluetoothPermission>())
            {
                await Shell.Current.DisplayAlert("Needs permissions", "BECAUSE!!!", "OK");
            }

            status = await Permissions.RequestAsync<MyBluetoothPermission>();


        }
        else
        {
            status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
                return;

            if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            {
                await Shell.Current.DisplayAlert("Needs permissions", "BECAUSE!!!", "OK");
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();


        }


        if (status != PermissionStatus.Granted)
            await Shell.Current.DisplayAlert("Permission required",
                "Location permission is required for bluetooth scanning. " +
                "We do not store or use your location at all.", "OK");
    }
}