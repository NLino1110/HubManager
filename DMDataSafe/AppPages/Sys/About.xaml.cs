
namespace DMDataSafe.AppPages.Sys;

public partial class About : ContentPage
{   
    public About()
	{
		InitializeComponent();
        lblUpdated.Text = "Ult. Actualización: ";
        ReadDeviceInfo();
    }

    private void ReadDeviceInfo()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine($"Model: {DeviceInfo.Current.Model}");
        sb.AppendLine($"Manufacturer: {DeviceInfo.Current.Manufacturer}");
        sb.AppendLine($"Name: {DeviceInfo.Current.Name}");
        sb.AppendLine($"OS Version: {DeviceInfo.Current.VersionString}");
        sb.AppendLine($"Idiom: {DeviceInfo.Current.Idiom}");
        sb.AppendLine($"Platform: {DeviceInfo.Current.Platform}");

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

        sb.AppendLine($"Virtual device? {isVirtual}");

        lblUpdated.Text = sb.ToString();
    }

    private async void BtnClose_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(false);
    }
}