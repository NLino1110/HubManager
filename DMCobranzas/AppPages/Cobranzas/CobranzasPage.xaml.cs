using ApiManager;
using CommunityToolkit.Maui.Alerts;
using DMCobranzas.Controls.Modals.TabbedPages;
using DMCobranzas.Models.Specials;
using DMCobranzas.Settings.helpers;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Controls.Popups;
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

    public bool isWindows { get; set; } = false;

    public ObservableCollection<MultipleCobrosInvoiceGroup> _items { get; set; }
    readonly PopupSizeConstants popupSizeConstants;

    public ICommand ReversarCommand { get; set; }

    res_company[] Empresas { get; set; }

    public ICommand CerrarDiaCommand { get; set; }

    public ICommand ReporteDiaCommand { get; set; }

    public ICommand TicketCommand { get; set; }

    public ICommand DeleteCommand { get; set; }

    public ICommand EditCommand { get; set; }

    public ICommand EnviarCobroCommand { get; set; }
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

        _items = new ObservableCollection<MultipleCobrosInvoiceGroup>();

        DeleteCommand = new Command(DeleteItem);
        EditCommand = new Command(EditItem);
        TicketCommand = new Command(TicketItem);

        CerrarDiaCommand = new Command(CerrarDia);
        ReporteDiaCommand = new Command(ReporteDia);
        EnviarCobroCommand = new Command(EnviarCobro);

        ReversarCommand = new Command(Reversar);


        if (App.Session.CurrentUserFront.empresas != null)
        {
            Empresas = App.Session.CurrentUserFront.empresas;
        }

        SelectorCmp.ItemsSource = Empresas;
        SelectorCmp.SelectedIndex = 0;
        
        isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;

        //dateIni.Date = DateTime.Today.AddMonths(-1);
        dateIni.Date = DateTime.Today;
        dateEnd.Date = DateTime.Today;
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadDataByDispatcher();
    }

    private async void btnBuscar_Clicked(object sender, EventArgs e)
    {
        await LoadData();
    }

    private async Task ProcessItemsGroup(List<MultipleCobrosInvoice> registrosGrupo,
        ObservableCollection<MultipleCobrosInvoiceGroup> _items,
        res_company se)
    {
        bool FoundCerrado = false;

        foreach (var item in registrosGrupo)
        {
            AccountPaymentDailyDb cobCierreDb = new AccountPaymentDailyDb(App.Session.odooConnection.DbNameSqlite);
            var cierres = await cobCierreDb.GetItemAsync(se.id, item.create_date.ToString("yyyy-MM-dd"));
            
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

            var newGroup = new MultipleCobrosInvoiceGroup(GroupTitle, registrosGrupo[0].create_date.ToString("yyyy-MM-dd"), registrosGrupo[0].create_date.ToString("yyyy-MM-dd"), registrosGrupo);
            newGroup.showButtonCierre = !FoundCerrado;
            _items.Add(newGroup);
        }
    }
    
    private async Task LoadData()
    {        
        if (IsLoading)
            return;

        IsLoading = true;

        Debug.WriteLine("Load data.....");

        try
        {
            if (SelectorCmp.SelectedItem == null)
                return;

            _items.Clear();

            DateTime dateEndField = dateEnd.Date.Value.AddHours(23).AddMinutes(59).AddSeconds(59);

            var SelCompany = (res_company) SelectorCmp.SelectedItem;
            var database = new MultipleCobrosInvoiceDb(App.Session.odooConnection.DbNameSqlite);
            
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

            ls_items = ls_items.OrderByDescending(c => c.create_date).ToList();

            DateTime currentFecha = DateTime.MinValue;
            List<MultipleCobrosInvoice> registrosGrupo = new List<MultipleCobrosInvoice>();

            foreach (var _paymentHeaderItem in ls_items)
            {

                if (CobrosEstados.IsEnProceso(_paymentHeaderItem.payment_status))
                {
                    DateTime fechaActual = DateTime.Now;
                    TimeSpan diferenciaDeTiempo = fechaActual - _paymentHeaderItem.write_date;

                    if (_paymentHeaderItem.write_date == default || diferenciaDeTiempo.TotalMinutes > 5)
                    {
                        _paymentHeaderItem.payment_status = CobrosEstados.PENDIENTE;
                        await database.UpdateAsync(_paymentHeaderItem);
                        await DisplayAlertAsync(
                            "Atención",
                            $"Cobro {_paymentHeaderItem.receipt_name} se regresó a estado PENDIENTE por inactividad.",
                            "Aceptar");
                    }
                }

                DateTime fecha = _paymentHeaderItem.create_date;

                if (fecha.Date != currentFecha.Date)
                {
                    await ProcessItemsGroup(registrosGrupo, _items, SelCompany);

                    currentFecha = fecha.Date;
                    registrosGrupo = new List<MultipleCobrosInvoice>();
                }

                registrosGrupo.Add(_paymentHeaderItem);
            }

            await ProcessItemsGroup(registrosGrupo, _items, SelCompany);

            OnPropertyChanged(nameof(_items));
            collectionView.ItemsSource = null;
            collectionView.ItemsSource = _items;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool PermitirCerrar(MultipleCobrosInvoiceGroup group)
    {
        foreach (var itemgroup in group)
        {
            if (itemgroup.payment_status == CobrosEstados.PENDIENTE || CobrosEstados.IsEnProceso(itemgroup.payment_status))
                return false;
        }

        return true;
    }

    private async void CerrarDia(object obj)
    {
        var itemgroup = (MultipleCobrosInvoiceGroup) obj;

        var resultCerrar = PermitirCerrar(itemgroup);

        if (!resultCerrar)
        {
            await DisplayAlertAsync("Cierre no permitido", "A\u00FAn existen cobros sin procesar, por favor env\u00EDelos antes de cerrar el d\u00EDa.", "Cerrar");
            return;
        }

        Debug.WriteLine("CerrarDia");
        string idCierre = itemgroup.GroupData;

        string numdeposito = await DisplayPromptAsync(itemgroup.GroupData, "# Dep\u00F3sito", "GUARDAR", "CANCELAR", "########", 10, Keyboard.Numeric); //, cobCarteraDet.VALORXAPLICAR);

        if (numdeposito == null || numdeposito == "" || numdeposito.Length <= 3)
        {
            await Toast.Make("N\u00FAmero de dep\u00F3sito para cierre no v\u00E1lido.").Show();
            return;
        }

        if (numdeposito != null && numdeposito != "")
        {           
            var cobReciboCab = new MultipleCobrosInvoiceDb(App.Session.odooConnection.DbNameSqlite);
            
            var se = App.Session.res_Company;
            DateTime dateTime = DateTime.Parse(itemgroup.GroupData);
            var fechaInicio = dateTime.Date;
            var fechaFin = fechaInicio.AddDays(1);

            var itemsCobros = await cobReciboCab.GetItemsAsync(x=>x.company_id == se.id 
            && x.create_date >= fechaInicio 
            && x.create_date < fechaFin);

            Debug.WriteLine(itemsCobros.Count());

            if (itemsCobros.Count() == 0)
            {
                await Toast.Make("No se encontraron registros para cierre").Show();
                return;
            }

            await UITools.ShowLoadingPopup(this);
            
            decimal monto_total = 0;

            var multipleCobrosInvoiceLineDb = new MultipleCobrosInvoiceLineDb(App.Session.odooConnection.DbNameSqlite);
            List<MultipleCobrosInvoiceLine> wholeAccountPayments = new List<MultipleCobrosInvoiceLine>();

            for (int i = 0; i < itemsCobros.Count(); i++)
            {                
                if (itemsCobros[i].payment_status == CobrosEstados.PENDIENTE || CobrosEstados.IsEnProceso(itemsCobros[i].payment_status))
                {                    
                    await UITools.HideLoadingPopup();

                    var mensajeError = $"Error ==> No se puede procesar el Dia: <b>{idCierre}</b>, existen Recibos no <b>ENVIADOS</b>.";                    
                    throw new Exception(mensajeError);
                }

                monto_total = (decimal) itemsCobros[i].amount;

                int parent_id = itemsCobros[i].id;

                var wpi = await multipleCobrosInvoiceLineDb.GetItemsAsync(x => x.MultipleCobrosInvoiceId  == parent_id);

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
        var headerResult = await hubAccountPaymentDaily.Send(objSend);
        return true;
    }

    private void ReporteDia(object obj)
    {
        TicketItem(obj);
        Debug.WriteLine("ReporteDia");
    }

    private void DeleteItem(object obj)
    {
        Debug.WriteLine("DeleteItem");
    }


    private async void EditItem(object obj)
    {
        Debug.WriteLine("EditItem");
        var invoice = (MultipleCobrosInvoice)obj;

        if (!CobrosEstados.CanEdit(invoice.payment_status))
        {
            await DisplayAlertAsync(
                "Atenci\u00F3n",
                $"Solo se puede editar cobros en estado PENDIENTE o ERROR. Estado actual: {invoice.payment_status}.",
                "Aceptar");
            return;
        }

        AccountPaymentView objPage = new AccountPaymentView();        
        objPage.Disappearing += NewPayment_Disappearing;
        objPage.Sel_MultipleCobrosInvoice = invoice;
        objPage.editionMode = true;
        await Navigation.PushAsync(objPage, false);
    }


    private async void TicketItem(object obj)
    {
        Debug.WriteLine("EditItem");
        PrintView objPage = new PrintView();
        
        string printTemplateHtml = "";
        string printTemplatePlain = "";
        byte[] printTemplateData = null;
        Services.Templates.Processor processor = new Services.Templates.Processor();
        switch (obj.GetType().Name)
        {
            case "MultipleCobrosInvoiceGroup":
                {
                    (printTemplateData, printTemplateHtml, printTemplatePlain) = await processor.Template_MultipleCobrosInvoiceGroup((MultipleCobrosInvoiceGroup)obj);

                    var _itemsGroup = (MultipleCobrosInvoiceGroup) obj;

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
                    (printTemplateData,printTemplateHtml, printTemplatePlain) = await processor.Template_MultipleCobrosInvoice((MultipleCobrosInvoice)obj);
                    var _itemGroup = (MultipleCobrosInvoice)obj;

                    res_company[] Empresas = null;
                    Empresas = App.Session.CurrentUserFront.empresas;
                    var res_CompanyData = Empresas.ToList().Where(i => i.id == _itemGroup.company_id).FirstOrDefault();
                    objPage.res_Company = res_CompanyData;
                }
                break;
        }
                
        objPage.setTemplatePreview(printTemplateHtml);
        objPage.setTemplatePlain(printTemplatePlain);
        objPage.setData(printTemplateData);

        await Navigation.PushAsync(objPage, false);
    }


    private async void EnviarCobro(object obj)
    {
        var _multipleCobrosInvoice = (MultipleCobrosInvoice)obj;

        if (!CobrosEstados.CanSync(_multipleCobrosInvoice.payment_status))
        {
            await DisplayAlertAsync(
                "Atención",
                $"Solo se puede sincronizar cobros en estado PENDIENTE o EN PROCESO. Estado actual: {CobrosEstados.GetDisplayStatus(_multipleCobrosInvoice.payment_status)}.",
                "Aceptar");
            return;
        }

        bool answer = await DisplayAlertAsync("Env\u00EDo de cobro", "Est\u00E1 seguro que desea enviar este cobro?", "Confirmar", "Cancelar");
        
        if (!answer)
        {
            return;
        }

        if (_multipleCobrosInvoice.center_id != App.Session.res_center.id)
        {
            _multipleCobrosInvoice.center_id = App.Session.res_center.id;
            Debug.WriteLine("Diferencia entre res_center, dato ser\u00E1 reemplazado");
        }

        await UITools.ShowLoadingPopup(this);
        var result = await DebitCollection.SendPayment(_multipleCobrosInvoice, false);
        await UITools.HideLoadingPopup();

        if (result.result != null && result.result.Count > 0 && result.error == null)
        {
            await Toast.Make("Env\u00EDo de cobro correcto").Show();
        }
        else
        {
            string error_message = "";
            if(result.error != null && result.error.message != null)
            {
                error_message = result.error.data.message;
                error_message = ParseTool.CleanServerMessage_v1(error_message, true);
            }

            await Toast.Make("Env\u00EDo de cobro erroneo:" + error_message).Show();
        }

        await LoadData();
    }


    private async void Reversar(object obj)
    {
        MultipleCobrosInvoice accountPaymentHeader = (MultipleCobrosInvoice)obj;

        bool answer = await DisplayAlertAsync("Reversar cobro", "Est\u00E1 seguro que desea reversar este cobro? " + accountPaymentHeader.receipt_name, "Reversar", "Cancelar");
        
        if (!answer)
        {
            return;
        }
    }

    private void NewPayment_Disappearing(object sender, EventArgs e)
    {
        Debug.WriteLine("Busqueda cerrada");        
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

    private async Task<bool> ExistsPendingDiaryClose()
    {
        AccountPaymentDailyDb accountPaymentDailyDb = new AccountPaymentDailyDb(App.Session.odooConnection.DbNameSqlite);
        MultipleCobrosInvoiceDb multipleCobrosInvoiceDb = new MultipleCobrosInvoiceDb(App.Session.odooConnection.DbNameSqlite);

        var resultItems = await multipleCobrosInvoiceDb.GetItemsAsync(x => x.date < DateTime.Now.Date 
            && x.create_uid == App.Session.CurrentUserFront.uid);

        var fechasUnicas = resultItems
            .Select(x => x.date.Date)
            .Distinct()
            .ToList();

        if (!fechasUnicas.Any())
            return false;

        var fechasSet = fechasUnicas.ToHashSet();

        var cierres_full = await accountPaymentDailyDb.GetItemsAsync(x=> x.was_odoo_synced != null && x.uid == App.Session.CurrentUserFront.uid);

        var fechasCierres = cierres_full
            .Select(x =>
            {
                DateTime.TryParse(x.closing_id, out var fecha);
                return fecha.Date;
            })
            .Distinct()
            .ToHashSet();

        var hayFechasSinCierre = fechasUnicas
            .Any(f => !fechasCierres.Contains(f));

        return hayFechasSinCierre;
    }

    private async void NewPayment(object sender, EventArgs e)
    {   
        if(await ExistsPendingDiaryClose())
        {
            //await Toast.Make("Existen cierres pendientes, por favor verifique sus datos antes de continuar.").Show();
            //return;
            await Toast.Make("Existen cierres pendientes").Show();
        }

        var se = (res_company) SelectorCmp.SelectedItem;
        AccountPaymentDailyDb cobCierreDb = new AccountPaymentDailyDb(App.Session.odooConnection.DbNameSqlite);
        var cierres = await cobCierreDb.GetItemAsync(se.id, DateTime.Now.ToString("yyyy-MM-dd"));

        if (cierres != null)
        {
            await DisplayAlertAsync(
                "Atenci\u00F3n",
                "Ya se ha cerrado el d\u00EDa, no podr\u00E1 ingresar m\u00E1s cobros hasta iniciar un nuevo per\u00EDodo.",
                "Aceptar");
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
}