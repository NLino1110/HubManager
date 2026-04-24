using ApiManager;
using CommunityToolkit.Maui.Alerts;
using DMDataSafe.ViewModels;
using DMSA.Models.Odoo.Customers;
using DMSA.Models.Odoo.General.Responses;
using System.Diagnostics;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace DMDataSafe.Modals;

public partial class ConfirmClient : ContentPage
{
    //public static readonly BindableProperty CustomerDisclaimerProperty =
    //        BindableProperty.Create(nameof(CustomerDisclaimer), typeof(string), typeof(ConfirmClient));

    //public string CustomerDisclaimer
    //{
    //    get => (string)GetValue(CustomerDisclaimerProperty);
    //    set => SetValue(CustomerDisclaimerProperty, value);
    //}
    public CustomerDataConsentResponse selectedCustomer { get; set; }
    public MainViewModelCliAprob BindingContextObj { get; set; }

    private bool hasChanges = false;
    TipoIde[] tipoIdes = null;

    string CustomerDisclaimer = "";
    string CustomerAgree1 = "";
    string CustomerAgree2 = "";
    string CustomerAgree3 = "";
    public ConfirmClient()
    {
        InitializeComponent();

        CustomerDisclaimer = "En DMujeres S.A. tratamos tus datos con el fin de gestionar nuestra relación como cliente. Tienes derecho a acceder, rectificar y actualizar tus datos, así como a solicitar su supresión, limitación o eliminación. También tienes derecho a la portabilidad de tus datos cuando proceda y a consultar nuestro Registro Nacional de Protección de Datos Personales. Si quieres ejercer estos derechos, envía un correo a servicio@dmujeres.ec";
        CustomerAgree1 = @"Acepto el envío de comunicaciones comerciales personalizadas, basadas en sus gustos, preferencias y consumos, sobre los productos y servicios de DMujeres S.A. por cualquier medio.";
        CustomerAgree2 = "He leído y acepto las condiciones generales y política de privacidad.<a href=\"#\">política de privacidad</a>";
        CustomerAgree3 = @"He leído y estoy de acuerdo con los términos y condiciones de uso.";

        lblCustomerDisclaimer.Text = CustomerDisclaimer;

        tipoIdes = new TipoIde[2] {
            new TipoIde() { id="C", descripcion = "CÉDULA" },
            new TipoIde() { id="R", descripcion = "RUC/OTROS" }
            };

        pickerTipId.ItemsSource = tipoIdes;
        //lblCustomerAgree1.Text = CustomerAgree1;
        //lblCustomerAgree2.Text = CustomerAgree2;
        //lblCustomerAgree3.Text = CustomerAgree3;
        BindingContext = this;
    }

    //public ICommand TapCommand => new Command<string>(async (url) => await Launcher.OpenAsync(url));
    public ICommand TapCommand => new Command<String>(launch_browser);

    private async void launch_browser(String url)
    {
        Debug.WriteLine($"*** Tap: {url}");
        //await Browser.OpenAsync(url);
        //https://www.dmujeres.ec/terminos-condiciones

        PopupInfoWeb obj = new PopupInfoWeb();
        obj.UrlViewer = url;
        await Navigation.PushModalAsync(obj, false);
    }

    private async void Cancel(object sender, EventArgs e)
    {
        //bool answer = await DisplayAlert("Actualizar", "Está seguro que desea iniciar la actualización?", "Continuar", "Cancelar");
        ////Debug.WriteLine("Answer: " + answer);
        //if (!answer)
        //{
        //    return;
        //}
        await Navigation.PopModalAsync(false);
        if (BindingContextObj != null)
        {
            BindingContextObj.RefreshCommand.Execute(this);
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await Browser.OpenAsync("https://www.google.com");
    }

    private void checkBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (checkBox.IsChecked && checkBox1.IsChecked && checkBox2.IsChecked)
        {
            BtnAgree.IsEnabled = true;
        }
        else
        {
            BtnAgree.IsEnabled = false;
        }
    }

    protected override void OnAppearing()
    {
        if (selectedCustomer.Email == null) selectedCustomer.Email = "";

        txtTipId.Text = selectedCustomer.doc_type_identification_id.ToUpper() == "C" ? "CÉDULA" : "RUC/OTROS";
        txtCedula.Text = selectedCustomer.vat_doc;
        txtNombres.Text = selectedCustomer.FirstName;
        txtApellidos.Text = selectedCustomer.LastName;
        txtTelefono.Text = selectedCustomer.Phone;
        txtEmail.Text = selectedCustomer.Email;
        txtAddress.Text = selectedCustomer.Address;

        if (!IsValidEmailTool(txtEmail.Text))
        {
            BtnAgree.IsEnabled = false;
            BtnDecline.IsEnabled = false;
        }

        base.OnAppearing();
    }

    //private async void Agree(object sender, EventArgs e)
    //{
    //    //Proceso de almacenamiento del cambio de estado
    //    //Comunicación con el API
    //    string Action = "ACTUALIZAR_APROBACION_CLIENTE";
    //    string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    //    //selectedCustomer.CODESTADO 
    //    string NUEVO_CODIGOESTADO = "52";
    //    string cadenaJson = "{\"IDENTIFICACION\":\"" + selectedCustomer.IDENTIFICACION + "\",\"CODUSUARIO\":\"" + selectedCustomer.CODUSUARIO + "\",\"CODESTADO\":" + NUEVO_CODIGOESTADO + "}";

    //    //cadenaJson = selectedCustomer;

    //    List<RestSharp.Parameter> parameters = new List<RestSharp.Parameter>();

    //    parameters.Add(RestSharp.Parameter.CreateParameter("codusuario", "caja1", ParameterType.QueryString));
    //    parameters.Add(RestSharp.Parameter.CreateParameter("accion", Action, ParameterType.QueryString));
    //    parameters.Add(RestSharp.Parameter.CreateParameter("cadenaJson", cadenaJson, ParameterType.QueryString));

    //    var task = Task.Run(async () =>
    //    {
    //        SoapClient client = new SoapClient();
    //        var result = await client.asyncPostJson(parameters.ToArray());
    //        Console.WriteLine(result);
    //        //Se obtiene resultado
    //        var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse_v1>(result);
    //    });

    //    Task.WaitAll(task);


    //    //Popup de agradecimiento
    //    Congratulations obj = new Congratulations();
    //    await Navigation.PushModalAsync(obj, false);
    //    await Task.Delay(1000);
    //    //Se cierra popup de agradecimiento
    //    await Navigation.PopModalAsync(false);
    //    //Se cierra popup de confirmación y se regresa a la lista
    //    await Navigation.PopModalAsync(false);

    //    if (BindingContextObj != null)
    //    {
    //        BindingContextObj.RefreshCommand.Execute(this);
    //    }

    //    //await Task.Run(async () =>
    //    //{
    //    //    await Task.Delay(1000);
    //    //    //await Navigation.PopModalAsync(false);
    //    //});

    //    //await Navigation.PopModalAsync(false);
    //    //App.Current.MainPage = new Congratulations();
    //}


    private async void Agree(object sender, EventArgs e)
    {
        HubCustomerDataConsent hubClienteAprobacion = new HubCustomerDataConsent(App.Session);
        ApiResponseOdooRpcT<int> apiResponse_V1;
        //var clienteA = await hubClienteAprobacion.GetAll();

        selectedCustomer.WriteDate = DateTime.Now;

        selectedCustomer.ApplicationOrigin = App.Session.ApplicationName;
        selectedCustomer.device_app_version = App.Session.AppVersion;
        selectedCustomer.device_platform_mod = DeviceInfo.Current.Platform.ToString();

        if (DeviceInfo.Current.Platform == DevicePlatform.Android ||
                DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            selectedCustomer.device_platform_source = DeviceInfo.Current.Idiom.ToString();
            selectedCustomer.device_manufacturer = DeviceInfo.Current.Manufacturer;
            selectedCustomer.device_model = DeviceInfo.Current.Model;
        }

        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
        {
            selectedCustomer.device_platform_source = DeviceInfo.Current.Idiom.ToString();
            selectedCustomer.device_manufacturer = "";
            selectedCustomer.device_model = "";
        }


        //SE ENVÍA EL NUEVO ESTADO 52 APROBADO
        selectedCustomer.CODESTADO = 52;
        if (!hasChanges)
        {
            apiResponse_V1 = await hubClienteAprobacion.Add(selectedCustomer);
        }
        else
        {
            var tIde = (TipoIde)pickerTipId.SelectedItem;
            selectedCustomer.doc_type_identification_id = tIde.id;
            selectedCustomer.vat_doc = txtCedula.Text;
            selectedCustomer.FirstName = txtNombres.Text;
            selectedCustomer.LastName = txtApellidos.Text;
            selectedCustomer.Phone = txtTelefono.Text;
            selectedCustomer.Email = txtEmail.Text;
            selectedCustomer.Address = txtAddress.Text;
            apiResponse_V1 = await hubClienteAprobacion.AddWithFull(selectedCustomer);
        }

        //if(true)
        //{
        //    Debug.WriteLine("Ok!");
        //}

        //Popup de agradecimiento
        Congratulations obj = new Congratulations();
        await Navigation.PushModalAsync(obj, false);
        await Task.Delay(1000);
        //Se cierra popup de agradecimiento
        await Navigation.PopModalAsync(false);
        //Se cierra popup de confirmación y se regresa a la lista
        await Navigation.PopModalAsync(false);

        if (BindingContextObj != null)
        {
            BindingContextObj.RefreshCommand.Execute(this);
        }

        //await Task.Run(async () =>
        //{
        //    await Task.Delay(1000);
        //    //await Navigation.PopModalAsync(false);
        //});

        //await Navigation.PopModalAsync(false);
        //App.Current.MainPage = new Congratulations();
    }

    private async void Decline(object sender, EventArgs e)
    {
        bool answer = await DisplayAlertAsync("Rechazar", "¿Está seguro que desea rechazar la solicitud? " +
            "Si esta solicitud es rechazada, el cliente no podrá acceder a las promociones.",
            "Continuar",
            "Cancelar");

        //Debug.WriteLine("Answer: " + answer);
        if (!answer)
        {
            return;
        }

        HubCustomerDataConsent hubClienteAprobacion = new HubCustomerDataConsent(App.Session);
        //var clienteA = await hubClienteAprobacion.GetAll();

        selectedCustomer.WriteDate = DateTime.Now;

        selectedCustomer.ApplicationOrigin = App.Session.ApplicationName;
        selectedCustomer.device_app_version = App.Session.AppVersion;
        selectedCustomer.device_platform_mod = DeviceInfo.Current.Platform.ToString();

        if (DeviceInfo.Current.Platform == DevicePlatform.Android ||
                DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            selectedCustomer.device_platform_source = DeviceInfo.Current.Idiom.ToString();
            selectedCustomer.device_manufacturer = DeviceInfo.Current.Manufacturer;
            selectedCustomer.device_model = DeviceInfo.Current.Model;
        }

        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
        {
            selectedCustomer.device_platform_source = DeviceInfo.Current.Idiom.ToString();
            selectedCustomer.device_manufacturer = "";
            selectedCustomer.device_model = "";
        }

        //SE ENVÍA EL NUEVO ESTADO 53 NO APROBADO
        selectedCustomer.CODESTADO = 53;
        //var resultUpdate = await hubClienteAprobacion.Update(selectedCustomer);
        var resultUpdate = await hubClienteAprobacion.Add(selectedCustomer);

        //if (true)
        //{
        //    Debug.WriteLine("Ok!");
        //}

        //Popup de agradecimiento
        Congratulations obj = new Congratulations();
        await Navigation.PushModalAsync(obj, false);
        await Task.Delay(1000);
        //Se cierra popup de agradecimiento
        await Navigation.PopModalAsync(false);
        //Se cierra popup de confirmación y se regresa a la lista
        await Navigation.PopModalAsync(false);

        if (BindingContextObj != null)
        {
            BindingContextObj.RefreshCommand.Execute(this);
        }

        //await Task.Run(async () =>
        //{
        //    await Task.Delay(1000);
        //    //await Navigation.PopModalAsync(false);
        //});

        //await Navigation.PopModalAsync(false);
        //App.Current.MainPage = new Congratulations();
    }

    private void BtnEditioMode_Clicked(object sender, EventArgs e)
    {
        //txtTipId.IsEnabled = true;
        //txtCedula.IsEnabled = true;
        //txtNombres.IsEnabled = true;
        //txtApellidos.IsEnabled = true;
        //txtTelefono.IsEnabled = true;
        //txtEmail.IsEnabled = true;
        //txtAddress.IsEnabled = true;
        txtTipId.IsVisible = false;
        //txtTipId.IsReadOnly = false;
        //txtCedula.IsReadOnly = false;
        txtNombres.IsReadOnly = false;
        txtApellidos.IsReadOnly = false;
        txtTelefono.IsReadOnly = false;
        txtEmail.IsReadOnly = false;
        txtAddress.IsReadOnly = false;

        var tIde = tipoIdes.Where(ti => ti.descripcion == txtTipId.Text).FirstOrDefault();
        pickerTipId.SelectedItem = tIde;

        gridTipId.IsVisible = true;
        BtnEditioMode.IsVisible = false;
        BtnEditionDone.IsVisible = true;
        BtnDecline.IsVisible = false;
        BtnAgree.IsVisible = false;

        txtCedula.Focus();
    }

    public static bool IsValidEmailTool(string email)
    {
        if (email == null) return false;

        if (email.Length > 0)
        {
            if (!IsValidEmail(email) || !IsValidEmail_l1(email) || !IsValidEmail_l2(email))
            {
                //txtEmail.Focus();
                //await Toast.Make("El formato del email está incorrecto, por favor corríjalo antes de continuar.").Show();
                //return;
                return false;
            }
        }

        return true;
    }

    public static bool IsValidEmail_l1(string email)
    {
        // Patrón para validar emails

        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        //string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
        //               + @"([-a-z0-9!#$%&'*+/=?^_`{}|~]|(?<!\.)\.)*)(?!\.)"
        //               + @"@((?!-)[a-z0-9-]{1,63}(?<!-)\.)+[a-z]{2,63}$";

        // Verificar si el email coincide con el patrón
        return Regex.IsMatch(email, pattern);
    }

    public static bool IsValidEmail_l2(string email)
    {
        // Patrón para validar emails

        if(email.Contains(".@") || email.Contains("@.") || email.Contains(".."))
        {
            return false;
        }

        // Verificar si el email coincide con el patrón
        return true;
    }

    public static bool IsValidEmail(string email)
    {
        //// Patrón para validar emails
        //// Este patrón no cubre todos los casos posibles de emails válidos, pero es un buen punto de partida
        //string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        //// Verificar si el email coincide con el patrón
        //return Regex.IsMatch(email, pattern);

        try
        {
            // Intentamos crear una instancia de MailAddress con el email proporcionado
            MailAddress mailAddress = new MailAddress(email);
            return true;
        }
        catch (FormatException)
        {
            // Si ocurre una excepción FormatException, el email no tiene un formato válido
            return false;
        }
    }

    private async void BtnEditionDone_Clicked(object sender, EventArgs e)
    {
        if (txtEmail.Text == null) txtEmail.Text = "";

        if(txtEmail.Text.Length>0)
        {
            if(!IsValidEmail(txtEmail.Text) || !IsValidEmail_l1(txtEmail.Text) || !IsValidEmail_l2(txtEmail.Text))
            {
                txtEmail.Focus();
                await Toast.Make("El formato del email está incorrecto, por favor corríjalo antes de continuar.").Show();
                return;
            }
        }

        //Se procede a activar los botones para avanzar con el proceso de aprobación
        //if (!IsValidEmailTool(txtEmail.Text))
        //{
            BtnAgree.IsEnabled = true;
            BtnDecline.IsEnabled = true;
        //}

        var tIde = (TipoIde) pickerTipId.SelectedItem;
        txtTipId.Text = tIde.descripcion;
        //var tIde = tipoIdes.Where(ti => ti.descripcion == txtTipId.Text).FirstOrDefault();
        //pickerTipId.SelectedItem = tIde;

        txtCedula.Text = txtCedula.Text.ToUpper();
        txtNombres.Text = txtNombres.Text.ToUpper();
        txtApellidos.Text = txtApellidos.Text.ToUpper();
        txtTelefono.Text = txtTelefono.Text.ToUpper();
        txtEmail.Text = txtEmail.Text.ToUpper();
        txtAddress.Text = txtAddress.Text.ToUpper();

        //txtTipId.IsEnabled = false;
        //txtCedula.IsEnabled = false;
        //txtNombres.IsEnabled = false;
        //txtApellidos.IsEnabled = false;
        //txtTelefono.IsEnabled = false;
        //txtEmail.IsEnabled = false;
        //txtAddress.IsEnabled = false;
        //txtTipId.IsReadOnly = true;
        txtTipId.IsVisible = true;
        //txtCedula.IsReadOnly = true;
        txtNombres.IsReadOnly = true;
        txtApellidos.IsReadOnly = true;
        txtTelefono.IsReadOnly = true;
        txtEmail.IsReadOnly = true;
        txtAddress.IsReadOnly = true;

        if(tIde.id != selectedCustomer.doc_type_identification_id ||
            txtCedula.Text != selectedCustomer.vat_doc ||
            txtNombres.Text != selectedCustomer.FirstName ||
            txtApellidos.Text != selectedCustomer.LastName ||
            txtTelefono.Text != selectedCustomer.Phone ||
            txtEmail.Text != selectedCustomer.Email ||
            txtAddress.Text != selectedCustomer.Address)
        {
            hasChanges = true;
        }

        gridTipId.IsVisible = false;
        BtnEditioMode.IsVisible = true;
        BtnEditionDone.IsVisible = false;
        BtnDecline.IsVisible = true;
        BtnAgree.IsVisible = true;
    }
}