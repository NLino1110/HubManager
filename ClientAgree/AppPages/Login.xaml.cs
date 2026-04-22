//using CobranzasDMSA.AppPages;
//using CobranzasDMSA.Models;
//using CobranzasDMSA.Settings.Sqlite;
//using DMSA.Models.Security;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.ApplicationModel.Communication;
using RestSharp;
using System.Buffers;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using Models.DMSA.Shared.Security;
using Models.DMSA.Mbw.Security;

namespace DMDataSafe.AppPages;

public partial class Login : ContentPage
{
    //TodoItemDatabase database;

    public Login()
    {
        InitializeComponent();

        //TODO: Revisión del tema, esta linea funciona, pero se debe analizar si es la mejor opción
        // respecto al rendimiento
        Application.Current.UserAppTheme = AppTheme.Light;
    }

    public void TryLogin_nw(object sender, EventArgs e)
    {
        App.Current.MainPage = new MainPage();

        var task = Task.Run(async () => {
            SoapClient client = new SoapClient(App.Session);
            var result = await client.asyncPost("", "");
            Console.WriteLine(result);
        });
        task.Wait();
    }

    public async void TryLogin(object sender, EventArgs e)
    {        
        string codigoUsuario = EntryUserName.Text;
        string password = EntryPassword.Text;

        if (codigoUsuario.Length <= 3 || password.Length <= 3)
        {
            await Toast.Make("Al parecer los datos del usuario y password están incorrectos.").Show();
            return;
        }

        BtnTryLogin.IsEnabled = false;

        string Action = "validaAccesoUsuarioJSON";
        string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string strParameters = "<codigoUsuario>" + EntryUserName.Text + "</codigoUsuario><password>" + password + "</password>";

        //List<RestSharp.Parameter> parameters = new List<RestSharp.Parameter>();

        //parameters.Add(RestSharp.Parameter.CreateParameter("codigoUsuario", Action, ParameterType.QueryString));
        //parameters.Add(RestSharp.Parameter.CreateParameter("password", cadenaJson, ParameterType.QueryString));

        //App.Current.MainPage = new MainPage();
        SoapClient client = new SoapClient(App.Session);
        var result = await client.asyncPost(Action, strParameters);
        Console.WriteLine(result);

        BtnTryLogin.IsEnabled = true;

        if (result != "")
        {
            var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(result);
            if (resultUser.exito)
            {
                if(resultUser.habilitadoMovil.ToLower() == "s")
                {
                    resultUser.codigoUsuario = codigoUsuario;
                    //Se prepara para la sesión el scope de la aplicación
                    //AppSession ns = new AppSession();
                    //ns.CurrentUser = resultUser;
                    //App.Session = ns;
                    App.Session.CurrentUser = resultUser;

                    //Casos especiales de tiendas que no tienen red interna WIFI
                    if (resultUser.accesos.Length > 0)
                    {
                        if (resultUser.accesos[0].agencias.Length > 0)
                        {
                            Debug.WriteLine(resultUser.accesos[0].agencias[0].CodAgencia);

                            //36 PUYO
                            if (resultUser.accesos[0].agencias[0].CodAgencia == 36)
                            {
                                App.Session.EndPointServerNewApi = App.Session.EndPointServerNewApiExternal;
                            }
                            else
                            {
                                App.Session.EndPointServerNewApi = App.Session.EndPointServerNewApiInternal;
                            }
                        }
                    }

                    App.Current.MainPage = new MainPageForProcess();
                }
                else
                {
                    await Toast.Make("Error: No habilitado para móvil" ).Show();
                }
            }
            else
            {
                if(resultUser.mensaje!=null)
                {
                    await Toast.Make("Error:" + resultUser.mensaje).Show();
                }
                else
                {
                    await Toast.Make("Error: desconocido").Show();
                }                
            }
        }
        else
        {
            await Toast.Make("Error: Acceso incorrecto").Show();
        }
    }
}