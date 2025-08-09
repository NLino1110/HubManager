using DMCobranzas.Settings.Sqlite;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Newtonsoft.Json;
using RestSharp;
using System.IO.Compression;
using static System.Net.Mime.MediaTypeNames;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Markup;
using ApiManager;
using DMSA.Models.Security;
using CobranzasDMSA.Models;
using DMSA.Models.General;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using Newtonsoft.Json.Schema;
using DMSA.Models.General.Requests;
using DMSA.Models.General.Responses;
using DMCobranzas.Services.ApiHub;
using System.Net;
//using Microsoft.Maui.Graphics.Platform;

namespace DMCobranzas.AppPages.Sys;

public partial class About : ContentPage
{
    public About()
	{
		InitializeComponent();
        //lblUpdated.Text = "Ult. Actualización: " + App.Session.CurrentUser.fechasincronizado;
        //Asignación de URL de descarga según la configuración de la sesión
        //_rootUrl = App.Session.CacheFilesUrl;

        //TODO: Agregar alertas al iniciar este proceso
        //HACK
        //UNDONE
        //UnresolvedMergeConflict

        ReadDeviceInfo();
    }

    //public void Draw(ICanvas canvas, RectF dirtyRectangle)
    //{
    //    canvas.FillColor = Colors.Yellow;

    //    canvas.FillRectangle(dirtyRectangle);

    //    using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("logo_dmujeres.png");

    //    Microsoft.Maui.Graphics.IImage image = PlatformImage.FromStream(stream);

    //    ImagePaint imagePaint = new ImagePaint
    //    {
    //        Image = image.Downsize(100)
    //    };

    //    canvas.SetFillPaint(imagePaint, RectF.Zero);

    //    canvas.FillRectangle(0, 0, 350, 350);
    //}

    private void ReadDeviceInfo()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        lblAppName.Text = AppInfo.Name;

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

        sb.AppendLine($"Virtual device: {isVirtual}");

        sb.AppendLine($"Odoo Edition");

        lblUpdated.Text = sb.ToString();

        lblAppVersion.Text = "Versión " + App.Session.AppVersion;
    }
}