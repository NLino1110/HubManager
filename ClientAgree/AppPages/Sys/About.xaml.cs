
namespace ClientAgree.AppPages.Sys;

public partial class About : ContentPage
{
    //TODO: Asignación provisional
    // ya que este valor cambiará dependiendo del estado de la sesión
    private string _rootUrl = "http://192.168.204.108:8081/MyBusinessWeb/tmp/android/sqlite/"; 

    public About()
	{
		InitializeComponent();
        lblUpdated.Text = "Ult. Actualización: ";// + App.Session.CurrentUser.fechasincronizado;
        //Asignación de URL de descarga según la configuración de la sesión
        //_rootUrl = App.Session.CacheFilesUrl;

        //TODO: Agregar alertas al iniciar este proceso
        //HACK
        //UNDONE
        //UnresolvedMergeConflict

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

    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(false);
    }
}