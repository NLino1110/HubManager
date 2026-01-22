//using CloudKit;
using ApiManager;
using BeebTech.Maui.Controls.Controls;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Sample.Models;
using CommunityToolkit.Maui.Sample.ViewModels.Views;
using DMCobranzas.Controls.Modals;
using DMCobranzas.Controls.Modals.TabbedPages;
using DMCobranzas.Models;
using DMCobranzas.Models.Specials;
using DMCobranzas.Services.ApiHub;
using DMCobranzas.Settings.helpers;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite.DebitCollection;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using DMSA.Sync.Core.Update.Pusher;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMCobranzas.AppPages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CobranzasPage : ContentPage
{
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged(nameof(IsLoading));
            Debug.WriteLine("_isLoading");
            Debug.WriteLine(_isLoading);
        }
    }


    //public bool IsRefreshing = true;

    private bool isFirtAppears = true;

    public bool isWindows { get; set; } = false;

    //CobReciboCab[] items { get; set; }

    //public List<ItemsGroup> _items { get; private set; } = new List<ItemsGroup>();

    public ObservableCollection<ItemsGroup> _items { get; set; } //= new ObservableCollection<ItemsGroup>();
    readonly PopupSizeConstants popupSizeConstants;
    readonly CsharpBindingPopupViewModel csharpBindingPopupViewModel;

    public CobranzasPage()
    {
        InitializeComponent();

        if (popupSizeConstants == null)
        {
            this.popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        }
        else
        {
            this.popupSizeConstants = popupSizeConstants;
        }

        this.csharpBindingPopupViewModel = csharpBindingPopupViewModel;

        _items = new ObservableCollection<ItemsGroup>();

        // Crear una lista de objetos

        DeleteCommand = new Command(DeleteItem);
        EditCommand = new Command(EditItem);
        TicketCommand = new Command(TicketItem);

        CerrarDiaCommand = new Command(CerrarDia);
        ReporteDiaCommand = new Command(ReporteDia);
        EnviarCobroCommand = new Command(EnviarCobro);

        ReversarCommand = new Command(Reversar);

        //var task = Task.Run(() =>
        //{
        //    //await LoadData();
        //    btnBuscar_Clicked(null, null);
        //});

        //Task.WaitAll(task);

        if (App.Session.CurrentUserFront.empresas != null)
        {
            Empresas = App.Session.CurrentUserFront.empresas;
        }

        SelectorCmp.ItemsSource = Empresas;
        SelectorCmp.SelectedIndex = 0;
        
        isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;

        dateIni.Date = DateTime.Today.AddMonths(-1);

        //scrollViewResult.SizeChanged += (sender, e) =>
        //{
        //    Debug.WriteLine("scrollViewResult SizeChanged");
        //    scrollViewResult.GetVisualElementWindow().Content.InvalidateArrange();
        //    scrollViewResult.GetVisualElementWindow().Content.InvalidateMeasure();
        //};

        //collectionView.SizeChanged += (sender, e) =>
        //{
        //Debug.WriteLine("SizeChanged");            
        //collectionView.GetVisualElementWindow().Content.InvalidateMeasure();
        //};

        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        //if(!UITools.LoadingNow())
        if (isFirtAppears)
        {
            isFirtAppears = false;
        }
        else
        {
            //LoadDataByDispatcher();
        }

        //var task = Task.Run(async () =>
        //{
        //    await Task.Delay(2000);
        //    if (isEmptyDb)
        //    {                
        //        await DisplayAlert("Alert", "You have been alerted", "OK");
        //    }
        //});
        //task.Wait();

        //IDispatcherTimer timer;

        //timer = Dispatcher.CreateTimer();
        //timer.Interval = TimeSpan.FromMilliseconds(500);
        //timer.IsRepeating = false;
        //timer.Tick += async (s, e) =>
        //{
        //    await LoadData();

        //    timer.Stop();
        //};
        //timer.Start();
    }

    private async void btnBuscar_Clicked(object sender, EventArgs e)
    {
        await LoadData();
    }

    private async Task ProcessItemsGroup(List<MultipleCobrosInvoice> registrosGrupo,
        ObservableCollection<ItemsGroup> _items,
        res_company se)
    {
        bool FoundCerrado = false;

        foreach (var item in registrosGrupo)
        {
            AccountPaymentDailyDb cobCierreDb = new AccountPaymentDailyDb(App.Session.odooConnection.DbNameSqlite);
            var cierres = await cobCierreDb.GetItemAsync(se.id, item.create_date.ToString("yyyy-MM-dd"));
            //YA HA SIDO CERRADO
            if (cierres != null)
            {
                item.CERRADO = "S";
                FoundCerrado = true;
            }
        }

        if (registrosGrupo.Count() > 0)
        {
            DateTime fecha = registrosGrupo[0].create_date;
            string GroupTitle = fecha.ToString("yyyy-MM-dd");

            var newGroup = new ItemsGroup(GroupTitle, registrosGrupo[0].create_date.ToString("yyyy-MM-dd"), registrosGrupo[0].create_date.ToString("yyyy-MM-dd"), registrosGrupo);
            newGroup.showButtonCierre = !FoundCerrado;
            _items.Add(newGroup);
        }
    }
    
    private async Task LoadData()
    {
        //collectionView: Contiene una referencia directa que en teoría debería bastar para que se 
        // actualice la visualizacion de forma directa, no lo logra, por lo cual se están realizando
        // 2 asignaciones. Considerar optimización para evitar dicho comportamiento.

        //if (UITools.LoadingNow())
        //    return;

        //await UITools.ShowLoading(_absoluteLayout);

        if (IsLoading)
            return;

        IsLoading = true;

        Debug.WriteLine("Load data.....");

        try
        {
            if(SelectorCmp.SelectedItem ==null)
            {
                //POSIBLE BUG
                return;
            }

            _items.Clear();

            //TODO: Revisar, no deberíamos tener que volver a reasignar la variable
            _items = new ObservableCollection<ItemsGroup>();
            //var task = Task.Run(async () =>
            //{
            DateTime dateEndField = dateEnd.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            var SelCompany = (res_company) SelectorCmp.SelectedItem;
            var database = new MultipleCobrosInvoiceDb(App.Session.odooConnection.DbNameSqlite);
            //var ls_items = await database.GetItemsAsync(se.empresa, dateIni.Date, dateEndField, App.Session.CurrentUser.codusuario, true);
            //var ls_items = await database.GetItemsAsync(SelCompany.id,
            //    dateIni.Date,
            //    dateEndField,
            //    txtSearch.Text.Trim(),
            //    App.Session.CurrentUser.uid,
            //    true);

            string text_search = txtSearch.Text;

            if (string.IsNullOrWhiteSpace(text_search))
            {
                text_search = "";
            }
            else
            {
                text_search = text_search.Trim();
            }

            var ls_items = await database.GetItemsAsync(x=> x.company_id == SelCompany.id && 
                 x.create_date >= dateIni.Date && 
                 x.create_date <= dateEndField &&                                   
                 x.partner_name.Contains(text_search) &&
                 x.create_uid == App.Session.CurrentUserFront.uid);

            //Se ordenan los registros por FECHA
            //ls_items.Sort((x, y) => x.FECHA.CompareTo(y.FECHA));

            //Se ordena desde la fecha mas actual
            ls_items = ls_items.OrderByDescending(c => c.create_date).ToList();

            // Variable para almacenar la fecha actual
            DateTime currentFecha = DateTime.MinValue;
            List<MultipleCobrosInvoice> registrosGrupo = new List<MultipleCobrosInvoice>();

            foreach (var _paymentHeaderItem in ls_items)
            {

                if(_paymentHeaderItem.payment_status == DMSA.Models.CobrosEstados.PROCESANDO)
                {
                    // Obtén la fecha y hora actual
                    DateTime fechaActual = DateTime.Now;
                    TimeSpan diferenciaDeTiempo = fechaActual - _paymentHeaderItem.create_date;

                    if (diferenciaDeTiempo.TotalMinutes > 5)
                    {
                        _paymentHeaderItem.payment_status = DMSA.Models.CobrosEstados.PENDIENTE;
                        await Toast.Make("Cobro " + _paymentHeaderItem.recipe_name + " se regreso a estado PENDIENTE por inactividad.").Show();
                    }
                }

                DateTime fecha = _paymentHeaderItem.create_date; // Convertir la cadena de fecha a DateTime

                if (fecha.Date != currentFecha.Date) // Si la fecha cambia
                {
                    // Ejecutar la función que recibe los registros de la fecha anterior
                    await ProcessItemsGroup(registrosGrupo, _items, SelCompany);

                    currentFecha = fecha.Date; // Actualizar la fecha actual
                    registrosGrupo = new List<MultipleCobrosInvoice>();
                }

                registrosGrupo.Add(_paymentHeaderItem);
            }

            await ProcessItemsGroup(registrosGrupo, _items, SelCompany);

            collectionView.ItemsSource = _items;
            
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error: " + ex.Message);
        }

        //await UITools.HideLoading(_absoluteLayout);

        IsLoading = false;
    }

    private bool PermitirCerrar(ItemsGroup group)
    {
        foreach (var itemgroup in group)
        {
            if (itemgroup.payment_status == DMSA.Models.CobrosEstados.PENDIENTE || itemgroup.payment_status == DMSA.Models.CobrosEstados.PROCESANDO)
                return false;
        }

        return true;
    }

    res_company[] Empresas { get; set; }

    public ICommand CerrarDiaCommand { get; set; }

    private async void CerrarDia(object obj)
    {
        var itemgroup = (ItemsGroup) obj;

        var resultCerrar = PermitirCerrar(itemgroup);

        if (!resultCerrar)
        {
            await DisplayAlert("Cierre no permitido", "Aún existen cobros sin procesar, por favor envíelos antes de cerrar el día.", "Cerrar");
            return;
        }

        Debug.WriteLine("CerrarDia");
        string idCierre = itemgroup.GroupData;

        //CobReciboCab cobCarteraDet = (CobReciboCab) obj;
        string numdeposito = await DisplayPromptAsync(itemgroup.GroupData, "# Depósito", "GUARDAR", "CANCELAR", "########", 10, Keyboard.Numeric); //, cobCarteraDet.VALORXAPLICAR);

        if (numdeposito == null || numdeposito == "" || numdeposito.Length <= 3)
        {
            await Toast.Make("Número de depósito para cierre no válido.").Show();
            return;
        }

        if (numdeposito != null && numdeposito != "")
        {
            //ID de Cierre es la fecha
            //Leer la base de datos
            AccountPaymentHeaderDb cobReciboCab = new AccountPaymentHeaderDb(App.Session.odooConnection.DbNameSqlite);
            var se = (res_company)SelectorCmp.SelectedItem;
            DateTime dateTime = DateTime.Parse(itemgroup.GroupData);
            //var itemsCobros = await cobReciboCab.GetItemsAsync(se.empresa, dateTime);
            var itemsCobros = await cobReciboCab.GetItemsDateCutAsync(se.id, dateTime);

            Debug.WriteLine(itemsCobros.Count());

            if (itemsCobros.Count() == 0)
            {
                await Toast.Make("No se encontraron registros para cierre").Show();
                return;
            }

            await UITools.ShowLoadingPopup(this);
            
            decimal monto_total = 0;

            AccountPaymentDb accountPaymentDb = new AccountPaymentDb(App.Session.odooConnection.DbNameSqlite);
            List<AccountPayment> wholeAccountPayments = new List<AccountPayment>();

            for (int i = 0; i < itemsCobros.Count(); i++)
            {
                // Validacion Estado del cobro
                if (itemsCobros[i].payment_status == DMSA.Models.CobrosEstados.PENDIENTE || itemsCobros[i].payment_status == DMSA.Models.CobrosEstados.PROCESANDO)
                {
                    // Cierra Espera
                    //loading.dismiss();
                    await UITools.HideLoadingPopup();

                    var mensajeError = $"Error ==> No se puede procesar el Dia: <b>{idCierre}</b>, existen Recibos no <b>ENVIADOS</b>.";
                    //var alert = this.alertCtrl.create(new { title = "Atención", subTitle = mensajeError, buttons = new[] { "Aceptar" } });
                    //alert.present();
                    throw new Exception(mensajeError); // Manejo de Error evita continuar
                }

                monto_total = itemsCobros[i].payment_amount;

                var wpi = await accountPaymentDb.GetByParent(itemsCobros[i].id);

                wholeAccountPayments.AddRange(wpi);

            }

            var registroCierre = new AccountPaymentDaily
            {
                company_id = se.id,
                closing_id = idCierre,
                bank_id = 1, //TODO: (numdeposito.Length > 0 ? "S" : "N"),
                payment_reference = numdeposito,
                closing_amount = monto_total,
                datetime_closing = DateTime.Now,
                closing_details = JsonConvert.SerializeObject(wholeAccountPayments),
                uid = App.Session.CurrentUserFront.uid,
            };

            AccountPaymentDailyDb cobcierre = new AccountPaymentDailyDb(App.Session.odooConnection.DbNameSqlite);

            var foundCierre = await cobcierre.GetItemAsync(se.id, itemgroup.GroupData);
            if (foundCierre != null)
            {
                //Actualizar
                await cobcierre.UpdateAsync(registroCierre);
            }
            else
            {
                //Ingresar nuevo registro
                await cobcierre.InsertAsync(registroCierre);
                await SendPaymentDaily(registroCierre);
            }

            await UITools.HideLoadingPopup();
            await Toast.Make($"Cierre {idCierre} realizado.").Show();
        }

        await LoadData();
    }


    public async Task<bool> SendPaymentDaily(AccountPaymentDaily registroCierre)
    {
        HubAccountPaymentDaily hubAccountPaymentDaily = new HubAccountPaymentDaily(App.Session);
        string jsonSerialized = JsonConvert.SerializeObject(registroCierre);
        AccountPaymentDaily objSend = JsonConvert.DeserializeObject<AccountPaymentDaily>(jsonSerialized);
        //objSend.payments = Array.Empty<AccountPaymentSend>();

        //paymentSend = paymentSendList.ToArray();
        //Se deben enviar los pagos a parte asi mismo las lineas de facturas
        // luego de ser almacenadas deben extraerse para que se sincronicen con la informacion de la tablet
        // en caso de que los datos no existan

        var headerResult = await hubAccountPaymentDaily.Send(objSend);
        return true;
    }
       
    public ICommand RefreshCommand => new Command(async () =>
    {
        //refreshView.IsRefreshing = true;
        // Simular una operación de actualización como obtener datos
        await Task.Delay(2000);

        // Actualiza los datos aquí
        // ...

        //refreshView.IsRefreshing = false;
    });

    public ICommand ReporteDiaCommand { get; set; }

    private void ReporteDia(object obj)
    {
        TicketItem(obj);
        Debug.WriteLine("ReporteDia");
    }

    public ICommand DeleteCommand { get; set; }

    private void DeleteItem(object obj)
    {
        Debug.WriteLine("DeleteItem");
    }

    public ICommand EditCommand { get; set; }

    private async void EditItem(object obj)
    {
        Debug.WriteLine("EditItem");

        AccountPaymentView objPage = new AccountPaymentView();
        
        objPage.Disappearing += NewPayment_Disappearing;
        objPage.Sel_MultipleCobrosInvoice = (MultipleCobrosInvoice)obj;
        objPage.editionMode = true;        

        //CobrosTabs objPage = new CobrosTabs();

        ////Se coloca en modo de edición
        //objPage.SetEditionMode();

        ////CobrosMain objPage = new CobrosMain();
        ////Se asigna la empresa seleccionada
        //await objPage.setCobReciboCab((AccountPaymentHeader) obj);



        //Se asigna título
        //obj.Title = "Cartera Clientes/" + se.nombre;
        //objPage.dataItem = (CobReciboCab)obj;
        //objPage.empresa = empresa;
        //objPage.SetTitle();
        await Navigation.PushAsync(objPage, false);
    }

    public ICommand TicketCommand { get; set; }

    private async void TicketItem(object obj)
    {
        Debug.WriteLine("EditItem");
        PrintView objPage = new PrintView();
        //CobrosMain objPage = new CobrosMain();
        //Se asigna la empresa seleccionada

        //((ItemsGroup)obj)[0]

        //objPage.setCobReciboCab((CobReciboCab)obj);
        string printTemplate = "";
        Services.Templates.Processor processor = new Services.Templates.Processor();
        switch (obj.GetType().Name)
        {
            case "ItemsGroup":
                {
                    //printTemplate = await processor.Template_ItemsGroup((ItemsGroup)obj);
                    printTemplate = await processor.Template_ItemsGroup_V2((ItemsGroup)obj);

                    var _itemsGroup = (ItemsGroup) obj;

                    foreach (var _itemGroup in _itemsGroup)
                    {
                        res_company[] Empresas = null;
                        Empresas = App.Session.CurrentUserFront.empresas;
                        var res_CompanyData = Empresas.ToList().Where(i => i.id == _itemGroup.company_id).FirstOrDefault();
                        objPage.res_Company = res_CompanyData;
                        break;
                    }   
                }
                break;
            case "MultipleCobrosInvoice":
                {
                    //printTemplate = await processor.Template_CobReciboCab((AccountPaymentHeader)obj);
                    printTemplate = await processor.Template_AccountPaymentHeader_v2((MultipleCobrosInvoice)obj);
                    var _itemGroup = (MultipleCobrosInvoice)obj;
                    
                    res_company[] Empresas = null;
                    Empresas = App.Session.CurrentUserFront.empresas;
                    var res_CompanyData = Empresas.ToList().Where(i => i.id == _itemGroup.company_id).FirstOrDefault();
                    objPage.res_Company = res_CompanyData;                        
                }
                break;
        }

        objPage.setTemplate(printTemplate);

        //Se asigna título
        //obj.Title = "Cartera Clientes/" + se.nombre;
        //objPage.dataItem = (CobReciboCab)obj;
        //objPage.empresa = empresa;
        //objPage.SetTitle();
        await Navigation.PushAsync(objPage, false);
    }

    public ICommand EnviarCobroCommand { get; set; }

    private async void EnviarCobro(object obj)
    {
        bool answer = await DisplayAlert("Envío de cobro", "Está seguro que desea enviar este cobro?", "Confirmar", "Cancelar");
        
        if (!answer)
        {
            return;
        }

        var _multipleCobrosInvoice = (MultipleCobrosInvoice)obj;
        
        await UITools.ShowLoadingPopup(this);
        var result = await DebitCollection.SendPayment(_multipleCobrosInvoice, false);
        await UITools.HideLoadingPopup();

        if (result.result.Count > 0 && result.error == null)
        {
            await Toast.Make("Envío de pagos correcto").Show();
        }
        else
        {
            string error_message = "";
            if(result.error != null && result.error.message != null)
            {
                error_message = result.error.message;
            }

            await Toast.Make("Envío de pagos erroneo:" + ParseTool.CleanServerMessage_v1(error_message, true)).Show();
        }

        await LoadData();
    }

    public ICommand ReversarCommand { get; set; }

    private async void Reversar(object obj)
    {
        AccountPaymentHeader accountPaymentHeader = (AccountPaymentHeader)obj;

        bool answer = await DisplayAlert("Reversar cobro", "Está seguro que desea reversar este cobro? " + accountPaymentHeader.recipe_name, "Reversar", "Cancelar");
        //Debug.WriteLine("Answer: " + answer);
        if (!answer)
        {
            return;
        }

        //var secuencia = await database.obtenerSecuenciaRecibo(dataItem.CODEMPRESA, App.Session.CurrentUser.codusuario, fechaActual);
        //string secuencia_final = GenerarCodigoRecibo(App.Session.CurrentUser.codusuario, dataItem.CODEMPRESA, fechaActual, secuencia.ToString());

        //CobReciboCab _cobReciboCab = (CobReciboCab)obj;
        //ApiProcessor apiProcessor = new ApiProcessor();
        //await apiProcessor.EnviarCobro(_cobReciboCab);
    }

    public ICommand ShowSwipeCommand { get; set; }

    private void ShowSwipe(object obj)
    {
        Debug.WriteLine("ShowSwipe");
        SwipeView swipeView = (SwipeView)obj;
    }

    private void SwipeItem_Invoked(object sender, EventArgs e)
    {
        Debug.WriteLine("Invoked");
        //var swipeItem = (SwipeItem)sender;
        //var sw = (SwipeView)swipeItem.Parent.Parent;        
        //sw.Open(OpenSwipeItem.RightItems, true);        
    }

    private void SwipeItem_Invoked_1(object sender, EventArgs e)
    {
        Debug.WriteLine("SwipeLeft");
        //var element = ((SwipeItem)sender);
    }

    //private ActivityIndicator _activityIndicator;

    //private async void SimulateLoading()
    //{
    //    // Simulación de un retraso para mostrar el indicador de actividad
    //    await Task.Delay(3000);

    //    // Detener el indicador de actividad y ocultarlo
    //    _activityIndicator.IsRunning = false;
    //    _activityIndicator.IsVisible = false;
    //}

    private void NewPayment_Disappearing(object sender, EventArgs e)
    {
        Debug.WriteLine("Busqueda cerrada");
        //throw new NotImplementedException();
        LoadDataByDispatcher();
    }

    void LoadDataByDispatcher()
    {
        IDispatcherTimer timer;

        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.IsRepeating = false;
        timer.Tick += async (s, e) =>
        {
            await LoadData();

            timer.Stop();
        };
        timer.Start();
    }

    void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;

        if (selectedIndex != -1)
        {
            LoadDataByDispatcher();
        }
    }

    private async void NewPayment(object sender, EventArgs e)    
    {
        //Valida envío pendientes de días anteriores antes de permitir ingresar nuevos cobros
        // es el mismo método para envíos automáticos

        var se = (res_company) SelectorCmp.SelectedItem;
        AccountPaymentDailyDb cobCierreDb = new AccountPaymentDailyDb(App.Session.odooConnection.DbNameSqlite);
        var cierres = await cobCierreDb.GetItemAsync(se.id, DateTime.Now.ToString("yyyy-MM-dd"));

        //YA HA SIDO CERRADO
        if (cierres != null)
        {
            await Toast.Make("Ya se ha cerrado el día, no podrá ingresar más cobros hasta iniciar un nuevo período.").Show();
            return;
        }

        AccountPaymentView obj = new AccountPaymentView();
        
        obj.Sel_Company_Id = new res_company()
        {
            id = se.id,
            name = se.name
        };

        obj.Disappearing += NewPayment_Disappearing;
        
        await Navigation.PushAsync(obj, false);
    }

    private void swipeView_SwipeEnded(object sender, SwipeEndedEventArgs e)
    {
        Debug.WriteLine("...");
    }

    private void swipeView_SwipeChanging(object sender, SwipeChangingEventArgs e)
    {
        //Debug.WriteLine("Cha...");
    }

    private void swipeView_SwipeStarted(object sender, SwipeStartedEventArgs e)
    {
        Debug.WriteLine("Sta..");
    }    
}