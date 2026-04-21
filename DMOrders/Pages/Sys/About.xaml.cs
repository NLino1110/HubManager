using System.Runtime.InteropServices;

namespace DMOrders.AppPages.Sys;

public partial class About : ContentPage
{
    public About()
	{
		InitializeComponent();        
        ReadDeviceInfo();
    }

    private void ReadDeviceInfo()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        lblAppName.Text = AppInfo.Name;
        var framework = RuntimeInformation.FrameworkDescription;

        sb.AppendLine($"Model: {DeviceInfo.Current.Model}");
        sb.AppendLine($"Manufacturer: {DeviceInfo.Current.Manufacturer}");
        sb.AppendLine($"Name: {DeviceInfo.Current.Name}");
        sb.AppendLine($"OS Version: {DeviceInfo.Current.VersionString}");
        sb.AppendLine($"Idiom: {DeviceInfo.Current.Idiom}");
        sb.AppendLine($"Platform: {DeviceInfo.Current.Platform}");
        sb.AppendLine($"Framework: {framework}");

        //string hostName = Dns.GetHostName();
        //Console.WriteLine(hostName);

        //System.Net.IPHostEntry iPHostEntry = new IPHostEntry();
        //iPHostEntry.HostName = hostName;

        //string IP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
        ////iPHostEntry.AddressList

        //var ip = CobranzasDMSA.Services.AndroidTools.OsCore.GetIpDevice();
        //Debug.WriteLine("Ip:");

        bool isVirtual = DeviceInfo.Current.DeviceType switch
        {
            DeviceType.Physical => false,
            DeviceType.Virtual => true,
            _ => false
        };

        sb.AppendLine($"Virtual device: {isVirtual}");
        sb.AppendLine($"Odoo Edition");
        lblUpdated.Text = sb.ToString();
        lblAppVersion.Text = "Versión " + App.Session.AppVersion;
    }

    private async void btnBack_Clicked(object sender, EventArgs e)
    {
        btnBack.IsEnabled = false;
        await Navigation.PopModalAsync();
    }
}