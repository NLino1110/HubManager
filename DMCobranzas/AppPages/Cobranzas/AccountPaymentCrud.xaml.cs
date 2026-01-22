using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Sample.Models;
using DMCobranzas.Settings.helpers;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.DebitCollection;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using Parlot.Fluent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMCobranzas.Controls.Modals;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AccountPaymentCrud : ContentPage
{
    public ObservableCollection<ResBank> Banks { get; set; } = new();
    public res_partner _res_partner { get; set; }
    public res_partner_bank _res_partner_bank { get; set; }
    //Arreglo representantivo de las lineas de pago
    public MultipleCobrosInvoiceLineAi[] multipleCobrosInvoiceLineAi { get; set; }
    public res_company Sel_Company_Id { get; set; }
    //public res_company res_Company { get; set; }
    public MultipleCobrosInvoice multipleCobrosInvoice { get; set; }
    public MultipleCobrosInvoiceLine multipleCobrosInvoiceLine { get; set; }

    bool isEmptyDb = false;
    public bool saveData { get; set; } = false;
    public bool isNewData { get; set; } = false;
    public int itemIndex { get; set; } = -1;

    List<AppParameter> tipoEmision { get; set; }

    List<account_journal_type> account_Journal_Types { get; set; }
    List<account_journal> account_Journals { get; set; }
    List<TarjetasCredito> tarjetasItems { get; set; }
    List<TarjetasTipoPago> tarjetasTipoPagoItems { get; set; }
    List<TarjetasPlazosBanco> plazosBancosItems { get; set; }

    private bool LoadingEditionData { get; set; } = false;
    private bool InitializingForm { get; set; } = false;
    //private account_journal selDiarioAux { get; set; }
    //private inbound_payment_method selInboundAux { get; set; } 

    public AccountPaymentCrud(res_partner ResPartner)
	{        
        _res_partner = ResPartner;

        InitializeComponent();
        
        BindingContext = this;
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();

        //var task = Task.Run(async () =>
        //{
        //    await Task.Delay(2000);
        //    if (isEmptyDb)
        //    {                
        //        await DisplayAlert("Alert", "You have been alerted", "OK");
        //    }
        //});
        //task.Wait();        

        IDispatcherTimer timer;

        timer = Dispatcher.CreateTimer();
        timer.IsRepeating = false;
        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.Tick += async (s, e) =>
        {
            //UpdateParticles();
            //canvasView.InvalidateSurface();
            //OnTapGestureRecognizerTapped(this, null);

            //Task.Delay(2000);
            if (isEmptyDb)
            {
                DisplayAlert("Alerta", "Al parecer no se han descargado las actualizaciones de datos. Actualice antes de continuar.", "OK");
                btnClose_Clicked(null, null);
                //var task = Task.Run(async () =>
                //{
                //    //await Navigation.PopModalAsync();
                //    btnCancel_Clicked(null, null);
                //});
                //task.Wait();
            }

            await PrepareForm();

            //Nuevo ingreso
            if (multipleCobrosInvoiceLine==null)
            {
                multipleCobrosInvoiceLine = new MultipleCobrosInvoiceLine();
                isNewData = true;

            }
            else
            {
                await LoadDataForEdition();
                
            }

            //var task = Task.Run(async () =>
            //{
                
            //});
            //task.Wait();

            timer.Stop();
        };
        timer.Start();
    }

    private async void OnExpandedChanged(object sender, CommunityToolkit.Maui.Core.ExpandedChangedEventArgs e)
    {
        if (e.IsExpanded)
        {
            await ArrowIcon.RotateTo(0, 200, Easing.CubicIn);
        }
        else
        {
            await ArrowIcon.RotateTo(180, 200, Easing.CubicOut);            
        }
    }


    protected override bool OnBackButtonPressed()
    {
        var tcs = new TaskCompletionSource<bool>();

        Dispatcher.Dispatch(async () =>
        {
            var leave = await DisplayAlert("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");

            if (leave)
            {
                //await Navigation.PushAsync(new MainPage());
                //base.OnBackButtonPressed();
                await Navigation.PopAsync();                
            }
        });

        return true;

        //return base.OnBackButtonPressed();
    }

    public ICommand ClearValueCommand { get; set; }

    private async void ClearValue(object obj)
    {
        //((FacNotaCreditoDetAuxiliar)obj).reconcile_amount = 0;
        Debug.WriteLine(obj);

        //bool answer = await DisplayAlert("Envío de cobro", "Está seguro que desea enviar este cobro?", "Confirmar", "Cancelar");
        ////Debug.WriteLine("Answer: " + answer);
        //if (!answer)
        //{
        //    return;
        //}
    }

    private async Task PrepareForm()
    {
        InitializingForm = true;
        ClearValueCommand = new Command(ClearValue);

        if (multipleCobrosInvoiceLine != null)
        {
            //MODO EDICION -- SI YA ESTABA GUARDADO PREVIAMENTE
            if (multipleCobrosInvoice != null)
            {
                var dbCompany = new CompanyDb(App.Session.odooConnection.DbNameSqlite);
                Sel_Company_Id = await dbCompany.GetItem(multipleCobrosInvoice.company_id);
            }
            else
            {
                //Si se está editando antes de ser guardado
                var dbCompany = new CompanyDb(App.Session.odooConnection.DbNameSqlite);
                Sel_Company_Id = await dbCompany.GetItem(multipleCobrosInvoiceLine.CompanyId);
            }
            //Sel_Company_Id = 
        }
        else
        {
            //MODO NUEVO
        }

        //var task = Task.Run(async () =>
        //{
        //var database = new CobReciboCabDb();
        //var listRes = await database.GetItemsAsync("1", dateIni.Date, dateEnd.Date, "", true);
        //Debug.WriteLine(listRes);

        //var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse_v1>(result);

        //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        //string text = "No existen datos para procesar, por favor actualice la información...";
        //ToastDuration duration = ToastDuration.Short;
        //double fontSize = 14;
        //var toast = Toast.Make(text, duration, fontSize);            

        //Se asume que el resultado devuelto desde la base de datos está en la primera fila
        //account_Journal_Types = new List<account_journal_type>();
        //account_Journal_Types.Add(
        //    new account_journal_type()
        //    {
        //        id = 1,
        //        code = "bank",
        //        name = "Banco"
        //    }
        //);

        //account_Journal_Types.Add(
        //    new account_journal_type()
        //    {
        //        id = 2,
        //        code = "cash",
        //        name = "Efectivo"
        //    }
        //);

        //pickerTipoDiario.ItemsSource = account_Journal_Types;
        //pickerTipoDiario.ItemDisplayBinding = new Binding("name");
        //pickerTipoDiario.SelectedIndex = 0;

        //formaspagos = Newtonsoft.Json.JsonConvert.DeserializeObject<st_formaspago[]>(lstFp[0].VALOR);
        ////Se excluyen los que tienen código de Retención
        //formaspagos = formaspagos.Where(x => !x.codigo.Contains("RF")).ToArray();

        //pickerFPago.ItemsSource = formaspagos;
        //pickerFPago.SelectedIndex = 2;

        //Bancos
        //var lstBancos = lstParametros.Where(x => x.CODPARAMETRO == "BANCOS").ToArray();
        //bancos = Newtonsoft.Json.JsonConvert.DeserializeObject<st_bancos[]>(lstBancos[0].VALOR);
        //pickerBancos.ItemsSource = bancos;
        //pickerBancos.SelectedIndex = 0;

        //Cuentas
        //var lstCuentas = lstParametros.Where(x => x.CODPARAMETRO == "CUENTAS").ToArray();
        //cuentas = Newtonsoft.Json.JsonConvert.DeserializeObject<st_cuentas[]>(lstCuentas[0].VALOR);
        //pickerCuentas.ItemsSource = cuentas;
        //pickerCuentas.SelectedIndex = 0;

        //Tarjetas
        //var lstTarjetas = lstParametros.Where(x => x.CODPARAMETRO == "TARJETAS").ToArray();
        //tarjetas = Newtonsoft.Json.JsonConvert.DeserializeObject<st_tarjetas[]>(lstTarjetas[0].VALOR);
        //pickerTarjetas.ItemsSource = tarjetas;
        //pickerTarjetas.SelectedIndex = 0;

        //List<AppParameter> ls_tipoCheque = new List<AppParameter>();
        //ls_tipoCheque.Add(new AppParameter() { name = "D", value = "AL DIA" });
        //ls_tipoCheque.Add(new AppParameter() { name = "P", value = "POSFECHADO" });

        //List<AppParameter> ls_indicador = new List<AppParameter>();
        //ls_indicador.Add(new AppParameter() { name = "N", value = "PROPIO" });
        //ls_indicador.Add(new AppParameter() { name = "S", value = "TERCEROS" });

        await UITools.ShowLoading(_absoluteLayout);
        //Cargar documentos

        //Se define si se cargan los datos de un accountPayment ya existente (edición)
        //o se prepara el formulario para ingresar datos nuevos
        // Debemos tomar en cuenta de que es posible que se deba implementar funciones de agregar
        // facturas nuevas en el detalle

        if (multipleCobrosInvoiceLine != null)
        {
            if (_res_partner != null)
            {
                Title = "EDICIÓN - " + _res_partner.id + "-" + _res_partner.name;
            }

            //await LoadPaymentLinesForEdit();
            await LoadPaymentLines();

            //if (accountPayment.lines != null)
            //{
            //    //Se agregan las facturas del accountPayment seleccionado
            //    Debug.WriteLine(accountPayment.lines.Length);
            //}
        }
        else
        {
            if (_res_partner != null)
            {
                Title = "NUEVO - " + _res_partner.name;
            }

            //await LoadPaymentLinesForNew();
            await LoadPaymentLines();
        }

        SetDirectPayment();

        await UITools.HideLoading(_absoluteLayout);

        txtRef.Text = "PAGO A LA FECHA " + DateTime.Now.ToString("yyyy-MM-dd");

        //tipoCheque = ls_tipoCheque.ToArray();
        //indicador = ls_indicador.ToArray();

        //pickerTipCheque.ItemsSource = tipoCheque;
        //pickerTipCheque.SelectedIndex = 0;
        //pickerIndicador.ItemsSource = indicador;
        //pickerIndicador.SelectedIndex = 0;
        //});

        //Task.WaitAll(task);

        tipoEmision = new List<AppParameter>();

        tipoEmision.Add(new AppParameter() { name = "transfer", value = "Trasferencia" });
        tipoEmision.Add(new AppParameter() { name = "deposito", value = "Depósito" });
        tipoEmision.Add(new AppParameter() { name = "cash", value = "Efectivo" });
        tipoEmision.Add(new AppParameter() { name = "check_day", value = "Cheque Día" });
        tipoEmision.Add(new AppParameter() { name = "check", value = "Cheque PF" });
        tipoEmision.Add(new AppParameter() { name = "credit_card", value = "Tarjeta Crédito" });
        tipoEmision.Add(new AppParameter() { name = "otros", value = "Otros" });

        pickerPaymentMethod.ItemsSource = tipoEmision;
        pickerPaymentMethod.ItemDisplayBinding = new Binding("value");
        pickerPaymentMethod.SelectedIndex = 0;
        pickerPaymentMethod.SelectedIndexChanged += pickerPaymentMethod_SelectedIndexChanged;

        var tarjetasCreditoDb = new TarjetasCreditoDb(App.Session.odooConnection.DbNameSqlite);
        tarjetasItems = (await tarjetasCreditoDb.GetItemsAsync(x=> x.active)).ToList();
        pickerCardId.ItemsSource = tarjetasItems;
        pickerCardId.ItemDisplayBinding = new Binding("display_name");
        pickerCardId.SelectedIndex = 0;
        pickerCardId.SelectedIndexChanged += PickerCardId_SelectedIndexChanged;

        int[] bank_ids = { 1,2,3,4, 5, 6, 7, 8, 9, 10, 12, 13 };

        var bank = new BankDb(App.Session.odooConnection.DbNameSqlite);
        var bankItems = (await bank.GetItemsAsync(x => bank_ids.Contains( x.id ))).ToList();
        //ddBankTcId.ItemsSource = tarjetasItems;
        //ddBankTcId.ItemDisplayBinding = new Binding("name");
        //ddBankTcId.SelectedIndex = 0;
        //ddBankTcId.SelectedIndexChanged += PickerCardId_SelectedIndexChanged;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Banks =
            [
                new ResBank { id = 0, name = "No seleccionada" },
                new ResBank { id = -1, name = "🔍 Buscar..." }
            ];

            foreach (var partnerItem in bankItems)
            {                
                Banks.Add(partnerItem);
            }

            ddBankTcId.ItemsSource = Banks;
            ddBankTcId.ItemDisplayBinding = new Binding("name");
            ddBankTcId.SelectedItem = Banks[0];

            ddBank.ItemsSource = Banks;
            ddBank.ItemDisplayBinding = new Binding("name");
            ddBank.SelectedItem = Banks[0];
            ddBankTcId.SelectedItemChanged += DdBankTcId_SelectedItemChanged;
        });

        TarjetasTipoPagoDb tarjetasTipoPagoDb = new TarjetasTipoPagoDb(App.Session.odooConnection.DbNameSqlite);
        tarjetasTipoPagoItems = (await tarjetasTipoPagoDb.GetItemsAsync(x => x.active)).ToList();
        pickerPaymentTypeId.ItemsSource = tarjetasTipoPagoItems;
        pickerPaymentTypeId.ItemDisplayBinding = new Binding("display_name");
        pickerPaymentTypeId.SelectedIndex = 0;
        pickerPaymentTypeId.SelectedIndexChanged += PickerPaymentTypeId_SelectedIndexChanged;

        InitializingForm = false;
    }

    private async void DdBankTcId_SelectedItemChanged(object sender, object e)
    {
        if (LoadingEditionData)
            return;

        await RefreshPlan();
    }

    private async void PickerPaymentTypeId_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (LoadingEditionData)
            return;

        await RefreshPlan();
    }

    private async void PickerCardId_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (LoadingEditionData)
            return;

        await RefreshPlan();
    }

    private async Task RefreshPlan()
    {
        
        int selected_journal = 0;
        int selected_bank = 0;
        //int selected_card = 0;
        int _pos_tipo_pago = 0;

        if (pickerDiario.SelectedIndex != -1)
        {
            var journal = (account_journal)pickerDiario.SelectedItem;
            selected_journal = journal.id;
        }
        if (ddBankTcId.SelectedItem != null)
        {
            var bank = (ResBank)ddBankTcId.SelectedItem;
            selected_bank = bank.id;
        }
        //if (pickerCardId.SelectedIndex != -1)
        //{
        //    var card = (TarjetasCredito) pickerCardId.SelectedItem;
        //    selected_card = card.id;
        //}
        if (pickerPaymentTypeId.SelectedIndex != -1)
        {
            var payment_type = (TarjetasTipoPago)pickerPaymentTypeId.SelectedItem;
            _pos_tipo_pago = payment_type.id;
        }

        var tarjetasPlazosBancoDb = new TarjetasPlazosBancoDb(App.Session.odooConnection.DbNameSqlite);
        plazosBancosItems = (await tarjetasPlazosBancoDb.GetItemsAsync(
            x => x._account_journal_id == selected_journal &&
            x._bank_id == selected_bank &&
            x._pos_tipo_pago == _pos_tipo_pago)).ToList();

        pickerPlanId.ItemsSource = plazosBancosItems;
        pickerPlanId.ItemDisplayBinding = new Binding("display_name");
        pickerPlanId.SelectedIndex = 0;
    }

    private async void pickerPaymentMethod_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (LoadingEditionData)
            return;
        
        var picker = (Picker) sender;
        int selectedIndex = picker.SelectedIndex;

        if (selectedIndex != -1)
        {
            // type es el tipo de pago seleccionado (string)
            // journal es el diario (objeto con propiedades Type, AplicaCheque, AplicaTarjeta)

            string type_value = ((AppParameter) picker.SelectedItem).name;

            AccountJournalDb db = new AccountJournalDb(App.Session.odooConnection.DbNameSqlite);

            var q = await db.GetItemsAsync(x => x.type == "bank");

            switch (type_value)
            {
                case "transfer":
                case "deposito":                
                case "cash":
                    {
                        q = await db.GetItemsAsync(x => x.type == "bank");
                        SetDirectPayment();
                    }
                    break;
                
                case "check_day":
                {
                    q = await db.GetItemsAsync(x => x.type == "bank" && x.aplica_cheque == true);
                        SetCheckPayment();
                }
                    break;
                case "check":                
                    {
                        q = await db.GetItemsAsync(x => x.type == "bank" && x.aplica_cheque == true);
                        SetCheckPostPayment();
                    }
                    break;

                case "credit_card":
                    {
                        q = await db.GetItemsAsync(x => x.type == "credit" && x.aplica_tarjeta == true);
                        SetCreditCardPayment();
                        
                        if(string.IsNullOrEmpty(txtChequeTitular.Text))
                            txtChequeTitular.Text = _res_partner.name;
                    }
                    break;

                case "otros":
                    {
                        q = await db.GetItemsAsync(x => x.type == "credit" && x.aplica_tarjeta == false);
                    }
                    break;

                default:
                    
                    break;
            }

            //pickerDiario.SelectedIndexChanged += PickerDiario_SelectedIndexChanged;

            
            //account_Journals = (await db.GetItemsAsync()).Where(
            //    j => j._company_id == Sel_Company_Id.id && 
            //    j.use_mobile_app == true).ToList();

            account_Journals = q;

            if (account_Journals.Count == 0)
            {
                //await toast.Show(cancellationTokenSource.Token);
                //await DisplayAlert("Alert", "You have been alerted", "OK");
                isEmptyDb = true;
                return;
            }

            account_Journals = account_Journals.OrderBy(j => j.name).ToList();

            pickerDiario.ItemsSource = account_Journals;
            pickerDiario.ItemDisplayBinding = new Binding("name");
            pickerDiario.SelectedIndex = 0;

            //    var paymentMethodSelected = (inbound_payment_method) picker.SelectedItem;

            //    CompanyDb companyDb = new CompanyDb(App.Session.odooConnection.DbNameSqlite);

            //    var companyId = await companyDb.GetItem(Sel_Company_Id.id);


            //    if (paymentMethodSelected.code == "in_third_checks")
            //    {
            //        var cuentaDiario = (account_journal) pickerDiario.SelectedItem;

            //        ChequeGroup.IsVisible = true;
            //        CreditCardGroup.IsVisible = false;

            //    }
            //    else
            //    {
            //        ChequeGroup.IsVisible = false;

            //        if (paymentMethodSelected.code.Contains("tc_in"))
            //        {
            //            CreditCardGroup.IsVisible = true;
            //        }
            //    }
        }
    }

    private void SetDirectPayment()
    {
        txtDepositoConfirmar.IsVisible = true;
        pickerFechaPago.IsVisible = true;
        ddBank.IsVisible = true;
        txtBancoCuenta.IsVisible = true;

        txtBinTc.IsVisible = false;
        txtAuthTc.IsVisible = false;
        txtLoteTc.IsVisible = false;

        txtNCheque.IsVisible = false;
        txtChequeTitular.IsVisible = false;
        txtChequeCiudad.IsVisible = false;

        pickerFechaCheque.IsVisible = false;

        pickerCardId.IsVisible = false;
        ddBankTcId.IsVisible = false;
        pickerPaymentTypeId.IsVisible = false;
        pickerPlanId.IsVisible = false;
    }

    private void SetCheckPayment()
    {
        txtDepositoConfirmar.IsVisible = true;
        pickerFechaPago.IsVisible = false;
        ddBank.IsVisible = true;
        txtBancoCuenta.IsVisible = true;

        txtBinTc.IsVisible = false;
        txtAuthTc.IsVisible = false;
        txtLoteTc.IsVisible = false;

        txtNCheque.IsVisible = true;
        txtChequeTitular.IsVisible = true;
        txtChequeCiudad.IsVisible = true;

        pickerFechaCheque.IsVisible = true;

        pickerCardId.IsVisible = false;
        ddBankTcId.IsVisible = false;
        pickerPaymentTypeId.IsVisible = false;
        pickerPlanId.IsVisible = false;
    }

    private void SetCheckPostPayment()
    {
        txtDepositoConfirmar.IsVisible = false;
        pickerFechaPago.IsVisible = true;
        ddBank.IsVisible = true;
        txtBancoCuenta.IsVisible = true;

        txtBinTc.IsVisible = false;
        txtAuthTc.IsVisible = false;
        txtLoteTc.IsVisible = false;

        txtNCheque.IsVisible = true;
        txtChequeTitular.IsVisible = true;
        txtChequeCiudad.IsVisible = true;

        pickerFechaCheque.IsVisible = true;

        pickerCardId.IsVisible = false;
        ddBankTcId.IsVisible = false;
        pickerPaymentTypeId.IsVisible = false;
        pickerPlanId.IsVisible = false;
    }

    private void SetCreditCardPayment()
    {
        txtDepositoConfirmar.IsVisible = false;
        pickerFechaPago.IsVisible = true;
        ddBank.IsVisible = false;
        txtBancoCuenta.IsVisible = false;

        txtBinTc.IsVisible = true;
        txtAuthTc.IsVisible = true;
        txtLoteTc.IsVisible = true;

        txtNCheque.IsVisible = false;
        txtChequeTitular.IsVisible = true;
        txtChequeCiudad.IsVisible = false;

        pickerFechaCheque.IsVisible = false;

        pickerCardId.IsVisible = true;
        ddBankTcId.IsVisible = true;
        pickerPaymentTypeId.IsVisible = true;
        pickerPlanId.IsVisible = true;
    }

    private async void PickerDiario_SelectedIndexChanged(object sender, EventArgs e)
    {
        //if (LoadingEditionData)
        //    return;
        
        //var picker = (Picker) sender;
        //int selectedIndex = picker.SelectedIndex;

        //if (selectedIndex != -1)
        //{            
        //    var cuentaDiario = (account_journal) picker.SelectedItem;
            
        //    ObservableCollection<inbound_payment_method> l_inbound = new ObservableCollection<inbound_payment_method>();            
        //    InboundPaymentMethodDb inboundPaymentMethodDb = new InboundPaymentMethodDb(App.Session.odooConnection.DbNameSqlite);
                    
        //    l_inbound = new ObservableCollection<inbound_payment_method>(await inboundPaymentMethodDb.GetItemsByParentAsync(cuentaDiario.id));
            
        //    pickerPaymentMethod.ItemsSource = l_inbound;
        //    pickerPaymentMethod.ItemDisplayBinding = new Binding("name");
        //    pickerPaymentMethod.SelectedIndex = 0;                       
        //}
    }


    private async Task<string> GetSellerName(int company_id, int user_id)
    {
        UserDb  resUserDb  = new UserDb(App.Session.odooConnection.DbNameSqlite);
        var res_User = await resUserDb.GetItemsAsync(company_id, user_id);
        
        if(res_User != null)
            return res_User.complete_name.ToUpper();
        return "";
    }

    private async Task<string> GetSellerNameByInvoice(int invoice_id)
    {
        AccountMoveLineDb accountMoveLineDb = new AccountMoveLineDb(App.Session.odooConnection.DbNameSqlite);
        var accountMoveLine = await accountMoveLineDb.GetItem(invoice_id);

        if (accountMoveLine != null)
        {
            AccountMoveDb accountMoveDb = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
            var accountMove = await accountMoveDb.GetItemAsync(x=> x.id == accountMoveLine._move_id);

            if (accountMove != null)
                return await GetSellerName(accountMove._company_id, accountMove._invoice_user_id);
        }

        return "";
    }

    private async Task LoadPaymentLinesForNew()
    {
        AccountMoveDb accountMoveDb = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
        //var accMovesByCustomer = await accountMoveDb.GetItemsByPartnerForPaymentAsync(_res_partner);

        var accMovesByCustomer = await accountMoveDb.GetItemsByPartnerAndCompany(_res_partner, Sel_Company_Id);

        List<MultipleCobrosInvoiceLineAi> multipleCobrosInvoiceLinesAiAux = new List<MultipleCobrosInvoiceLineAi>();

        decimal totalResidualPayment = 0;
        decimal totalPayment = 0;
        decimal totalApplied = 0;
        //decimal valorSaldoDocumento = 0;
        decimal totalInvoicePayment = 0;

        decimal residualAmount = 0;

        totalPayment = (decimal) ParseTool.StringToDouble(txtMonto.Text);

        int lineCounter = 0;
        foreach (var accountMoveItem in accMovesByCustomer)
        {
            lineCounter++;
            MultipleCobrosInvoiceLineAi cobrosInvoiceLineAiAux = new MultipleCobrosInvoiceLineAi();
            cobrosInvoiceLineAiAux.multiple_cobros_invoice_line_id = accountMoveItem.id;

            cobrosInvoiceLineAiAux.invoice_line_id = 0;

            AccountMoveLineDb accountMoveLineDb = new AccountMoveLineDb(App.Session.odooConnection.DbNameSqlite);
            //var itemMoveLine = await accountMoveLineDb.GetItemsByParentAsync(accountMoveItem.id);
            //var itemPaymentTerm = itemMoveLine.Where(x => x.display_type == "payment_term").FirstOrDefault();
            var itemPaymentTerm = await accountMoveLineDb.GetItemAsync(x=> x._move_id == accountMoveItem.id && x.display_type == "payment_term");            

            if (itemPaymentTerm != null)
            {
                cobrosInvoiceLineAiAux.invoice_line_id = itemPaymentTerm.id;
            }
            else
            {
                //Mostrar mensaje de advertencia
                // no se encontró término de pago en factura y es requerido para continuar
                // no se agregará factura a la lista
                continue;
            }

            //accountPaymentInvoiceLineAuxiliar.invoice_line_id_name = accountMoveItem.name;
            cobrosInvoiceLineAiAux.invoice_id = accountMoveItem.id;
            cobrosInvoiceLineAiAux.invoice_name = accountMoveItem.name;
            cobrosInvoiceLineAiAux.docnum_mask = accountMoveItem.docnum_mask;
            cobrosInvoiceLineAiAux.invoice_date = accountMoveItem.invoice_date; 
            cobrosInvoiceLineAiAux.invoice_date_due = accountMoveItem.invoice_date_due;            
            cobrosInvoiceLineAiAux.amount_asigned = accountMoveItem.amount_total;
            cobrosInvoiceLineAiAux.amount_residual = accountMoveItem.amount_residual;

            residualAmount += cobrosInvoiceLineAiAux.amount_residual;

            totalResidualPayment = totalPayment - totalApplied;

            cobrosInvoiceLineAiAux.seller = await GetSellerName(accountMoveItem._company_id, accountMoveItem._invoice_user_id);
            
            if (totalResidualPayment > 0)
            {
                //valorSaldoDocumento = accountPaymentInvoiceLineAuxiliar.invoice_amount_residual; // (+parseFloat("" + data[i].VALORSALDO.replace(",", "")).toFixed(2) - +parseFloat("" + data[i].VALORCHEQUE.replace(",", "")).toFixed(2)) * 1;
                totalInvoicePayment = (totalResidualPayment < cobrosInvoiceLineAiAux.amount_residual) ? totalResidualPayment : cobrosInvoiceLineAiAux.amount_residual; 
                //((valorFaltaAplicar < valorSaldoDocumento) ? valorFaltaAplicar : valorSaldoDocumento);
                //docItem.VALORXAPLICAR = "" + valorAplicaDocumento;
                cobrosInvoiceLineAiAux.amount_asigned = totalInvoicePayment; 
                totalApplied += totalInvoicePayment;
            }
            else
            {
                cobrosInvoiceLineAiAux.amount_asigned = 0;
                //docItem.BLOQUEADO = "N";
            }
            multipleCobrosInvoiceLinesAiAux.Add(cobrosInvoiceLineAiAux);
        }

        multipleCobrosInvoiceLinesAiAux = multipleCobrosInvoiceLinesAiAux.OrderBy(x => x.invoice_date).ToList();
        multipleCobrosInvoiceLineAi = multipleCobrosInvoiceLinesAiAux.ToArray();

        lblCounter.Text = "Total de documentos " + multipleCobrosInvoiceLineAi.Length.ToString();
        lblMonto.Text = "($ " + residualAmount.ToString() + ")";
        collectionView.ItemsSource = multipleCobrosInvoiceLineAi;
    }

    private async Task LoadDataForEdition()
    {
        LoadingEditionData = true;

        var tipoEmisionSelected = tipoEmision.Where(x => x.name == multipleCobrosInvoiceLine.Type).FirstOrDefault();

        pickerPaymentMethod.SelectedItem = tipoEmisionSelected;

        AccountJournalDb accountJournalDb = new AccountJournalDb(App.Session.odooConnection.DbNameSqlite);
             
        var selDiario = await accountJournalDb.GetItemAsync(x=>x.id == multipleCobrosInvoiceLine.JournalId);

        switch(tipoEmisionSelected.name)
        {
            case "transfer":
                {
                    SetDirectPayment();
                }
                break;
            case "deposito":
                {
                    SetDirectPayment();
                }
                break;
            case "cash":
                {
                    SetDirectPayment();
                }
                break;
            case "check_today":
                {
                    SetCheckPayment();
                }
                break;
            case "check":
                {
                    SetCheckPostPayment();
                }
                break;
            case "credit_card":
                {
                    SetCreditCardPayment();
                }
                break;

        }

        if (selDiario != null)
        {   
            if (account_Journals == null)
            {
                account_Journals = (await accountJournalDb.GetItemsAsync()).Where(j =>
                    j._company_id == Sel_Company_Id.id).ToList();
                account_Journals = account_Journals.OrderBy(j => j.name).ToList();
                //Se asigna lista al picker
                pickerDiario.ItemsSource = account_Journals;
                pickerDiario.ItemDisplayBinding = new Binding("name");
            }
                
            //Ahora desde memoria
            var selDiarioMemory = account_Journals.Where(x => x.id == selDiario.id).FirstOrDefault();
            pickerDiario.SelectedItem = selDiarioMemory;
        }

        txtBinTc.Text = multipleCobrosInvoiceLine.CardBinText;
        txtAuthTc.Text = multipleCobrosInvoiceLine.CardVoucher;
        txtLoteTc.Text = multipleCobrosInvoiceLine.LoteVoucher;

        txtChequeTitular.Text = multipleCobrosInvoiceLine.AccHolderName;
        txtChequeCiudad.Text = multipleCobrosInvoiceLine.CityId.ToString();
        pickerFechaCheque.Date = multipleCobrosInvoiceLine.WithdrawalDate.Value;

        var tarjeta = tarjetasItems.Where(x => x.id == multipleCobrosInvoiceLine.CardId).FirstOrDefault();
        pickerCardId.SelectedItem = tarjeta;

        var bancoTarjeta = Banks.Where(x => x.id == multipleCobrosInvoiceLine.BankTcId).FirstOrDefault();
        ddBankTcId.SelectedItem = bancoTarjeta;

        var tarjetaTipoPago = tarjetasTipoPagoItems.Where(x => x.id == multipleCobrosInvoiceLine.PaymentTypeId).FirstOrDefault();
        pickerPaymentTypeId.SelectedItem = tarjetaTipoPago;

        await RefreshPlan();

        //var tarjetasPlazosBancoDb = new TarjetasPlazosBancoDb(App.Session.odooConnection.DbNameSqlite);
        var plazosBancosItem = plazosBancosItems.Where(x => x.id == multipleCobrosInvoiceLine.PlanId).FirstOrDefault();
        pickerPlanId.SelectedItem = plazosBancosItem;

        PartnerBankDb partnerBankDb = new PartnerBankDb(App.Session.odooConnection.DbNameSqlite);

        var partnerBankItem = await partnerBankDb.GetItemAsync(x => x.id == multipleCobrosInvoiceLine.BankId);
        if (partnerBankItem != null)
        {
            //Busqueda de banco
            BankDb bankDb = new BankDb(App.Session.odooConnection.DbNameSqlite);
            var bankItem = await bankDb.GetItemAsync(x => x.id == partnerBankItem._bank_id);

            txtBancoCuenta.Text = partnerBankItem.acc_number;
            lblAccountBank.Text = bankItem.name;
            lblAccountHolder.Text = partnerBankItem.acc_holder_name;
            lblAccountType.Text = partnerBankItem.type_account;
            stackAccountInfo.IsVisible = true;

            _res_partner_bank = partnerBankItem;
        }


        txtMonto.Text = multipleCobrosInvoiceLine.Amount.ToString(); //.ToString(App.Session.ApplicationCultureInfo);
        pickerFechaPago.Date = (DateTime) multipleCobrosInvoiceLine.PaymentDate;
        txtRef.Text = multipleCobrosInvoiceLine.Resumen;
        txtCircular.Text = multipleCobrosInvoiceLine.Circular;
        txtNCheque.Text = multipleCobrosInvoiceLine.NumberCheckText;

        LoadingEditionData = false;
    }
        
    private async Task LoadPaymentLines()
    {
        if(multipleCobrosInvoiceLine!=null)
        {
            if (multipleCobrosInvoiceLine.lines == null)
            {
                return;
            }
        }
        else
        {
            return;
        }

        List<MultipleCobrosInvoiceLineAi> accountPaymentLinesMem = new List<MultipleCobrosInvoiceLineAi>();

        foreach (var line in multipleCobrosInvoiceLine.lines)
        {
            //MultipleCobrosInvoiceLineAi itemN = new MultipleCobrosInvoiceLineAi();
            //itemN.id = line.id;
            //itemN.parent_payment_id = line.parent_payment_id;
            //itemN.invoice_name = line.invoice_line_id_name;
            //itemN.amount_asigned = line.reconcile_amount;
            //itemN.invoice_line_id = line.invoice_line_id;
            //itemN.invoice_line_id_name = line.invoice_line_id_name;
            //itemN.invoice_date = line.invoice_date;
            //itemN.amount_residual = line.amount_residual;
            //itemN.invoice_amount_residual = line.amount_residual;
            //itemN.seller = "----";
            //itemN.seller = await GetSellerNameByInvoice(line.invoice_line_id);
            //accountPaymentLinesMem.Add(itemN);
            accountPaymentLinesMem.Add(line);
        }

        accountPaymentLinesMem = accountPaymentLinesMem.OrderBy(x => x.invoice_date).ToList();

        multipleCobrosInvoiceLineAi = accountPaymentLinesMem.ToArray();

        //int repeat = 3;
        //accountPaymentLines = Enumerable
        //    .Repeat(accountPaymentLinesMem, repeat)
        //    .SelectMany(x => x)
        //    .ToArray();

        lblCounter.Text = "Total de documentos " + multipleCobrosInvoiceLineAi.Length.ToString();
        collectionView.ItemsSource = multipleCobrosInvoiceLineAi;
    }

    private async void btnLoadDocs_Clicked(object sender, EventArgs e)
    {
        await LoadPaymentLinesForNew();
    }

    private async void btnNewAccountBank_Clicked(object sender, EventArgs e)
    {
        PopupSizeConstants popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);

        var returnResultPopup = new PopupResPartnerBank(popupSizeConstants);
        returnResultPopup.partner = _res_partner;

        //Evita que se cierre cuando se haga clic (tap) fuera de la ventana
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        //if (!isWindows)
        //    returnResultPopup.Size = this.popupSizeConstants.Large;

        var result = await this.ShowPopupAsync <res_partner_bank> (returnResultPopup);

        if (result.Result != null)
        {
            PartnerBankDb partnerBankDb = new PartnerBankDb(App.Session.odooConnection.DbNameSqlite);
            var new_partnerBank = (res_partner_bank)result.Result;
            
            _res_partner_bank = new_partnerBank;

            new_partnerBank.id = await partnerBankDb.getNewId();

            await partnerBankDb.InsertAsync(new_partnerBank);

            txtBancoCuenta.Text = new_partnerBank.acc_number;
            lblAccountHolder.Text = new_partnerBank.acc_holder_name;
            lblAccountBank.Text = new_partnerBank.bank_name;
            string type_account = "-";
            switch (new_partnerBank.type_account)
            {
                case "savings":
                    {
                        type_account = "AHORROS";
                    }
                    break;
                case "current":
                    {
                        type_account = "CORRIENTE";
                    }
                    break;
            }

            lblAccountType.Text = type_account;            
            stackAccountInfo.IsVisible = true;

            //ObservableCollection<res_partner_bank> l_partnerBank = new ObservableCollection<res_partner_bank>();

            //l_partnerBank = new ObservableCollection<res_partner_bank>(await partnerBankDb.GetItemsAsync());

            ////var selectedItem = partnerBankDb.GetItem(new_partnerBank.id);

            //var selectedItem = l_partnerBank.Where(x => x.id == new_partnerBank.id).FirstOrDefault();

            //pickerAccountBank.ItemsSource = l_partnerBank;
            //pickerAccountBank.ItemDisplayBinding = new Binding("display");
            ////pickerAccountBank.SelectedIndex = l_partnerBank.Count() - 1;
            //pickerAccountBank.SelectedItem = selectedItem;
        }
        else
        {
            stackAccountInfo.IsVisible = false;
        }
    }

    private async void btnCuenta_Clicked(object sender, EventArgs e)
    {
        PopupSizeConstants popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        var returnResultPopup = new PopupSelectPartnerBank(popupSizeConstants);
        returnResultPopup.partner = _res_partner;

        returnResultPopup.Company = new res_company()
        {
            id = Sel_Company_Id.id,
            name = "COMPAÑIA",
        };

        //Evita que se cierre cuando se haga clic (tap) fuera de la ventana
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        //if (!isWindows)
        //    returnResultPopup.Size = popupSizeConstants.Large;

        var result = await PopupExtensions.ShowPopupAsync<res_partner_bank>(this, returnResultPopup);
        
        if (result.Result != null)
        {
            res_partner_bank Sel_Res_PartnerBank = result.Result;
            _res_partner_bank = Sel_Res_PartnerBank;

            txtBancoCuenta.Text = Sel_Res_PartnerBank.acc_number;
            lblAccountHolder.Text = Sel_Res_PartnerBank.acc_holder_name;
            lblAccountBank.Text = Sel_Res_PartnerBank.bank_name;
            string type_account = "-";
            switch(Sel_Res_PartnerBank.type_account)
            {
                case "savings":
                    {
                        type_account = "AHORROS";
                    }
                    break;
                case "current":
                    {
                        type_account = "CORRIENTE";
                    }
                    break;
            }
            lblAccountType.Text = type_account;
            //txtCuenta.Text = Sel_Res_PartnerBank.acc_number.ToString() + " - " + Sel_Res_PartnerBank.acc_holder_name;
            //_res_partnerItem = resPartner;
            stackAccountInfo.IsVisible = true;
        }
        else
        {
            stackAccountInfo.IsVisible = false;
        }
    }

    private async void btnSave_Clicked(object sender, EventArgs e)
    {
        double totalPagadoDbl = 0;

        if (!double.TryParse(txtMonto.Text, out totalPagadoDbl) || totalPagadoDbl == 0)
        {
            await Toast.Make("No se han ingresado valores correctos, no se puede guardar.").Show();
            return;
        }

        if (multipleCobrosInvoiceLine == null)
        {
            multipleCobrosInvoiceLine = new MultipleCobrosInvoiceLine();
            multipleCobrosInvoiceLine.MultipleCobrosInvoiceId = 0;
        }

        string journalCode = "";
        account_journal accountJournal = null;

        if (((account_journal)pickerDiario.SelectedItem) != null)
        {
            accountJournal = ((account_journal)pickerDiario.SelectedItem);
            journalCode = accountJournal.code;
        }

        multipleCobrosInvoiceLine.CompanyId = Sel_Company_Id.id;

        multipleCobrosInvoiceLine.Resumen = txtRef.Text;
        multipleCobrosInvoiceLine.Circular = txtCircular.Text;
        multipleCobrosInvoiceLine.PartnerId = _res_partner.id;

        multipleCobrosInvoiceLine.Type = ((AppParameter)pickerPaymentMethod.SelectedItem).name;

        multipleCobrosInvoiceLine.JournalId = ((account_journal)pickerDiario.SelectedItem).id;
        multipleCobrosInvoiceLine.journal_name = ((account_journal)pickerDiario.SelectedItem).name;
        multipleCobrosInvoiceLine.PartnerType = "customer";
        multipleCobrosInvoiceLine.PaymentType = "inbound";
        multipleCobrosInvoiceLine.PaymentDate = pickerFechaPago.Date;
        
        AppParameter selPaymentM = null;
        //accountPayment.payment_method_line_id = 0;
        multipleCobrosInvoiceLine.NumberCheckText = txtNCheque.Text;
        multipleCobrosInvoiceLine.WithdrawalDate = pickerFechaCheque.Date;

        if (pickerPaymentMethod.SelectedItem != null)
        {
            selPaymentM = (AppParameter)pickerPaymentMethod.SelectedItem;
            //accountPayment.payment_method_line_id = 1; // selPaymentM.value;

            //"transfer":
            //"deposito":
            //"cash":
            //"check_day":
            //"check":
            //credit_card
            //otros

            //accountPayment.DepositosConfirmarId = txtDepositoConfirmar.Text;
            //txtMonto
            //txtFDeposito
            multipleCobrosInvoiceLine.WithdrawalDate = pickerFechaCheque.Date;            
            //accountPayment.BankId = txtBancoDeposito.Text;
            //txtBancoCuenta

            if (selPaymentM.name == "credit_card") //&& aplica_tarjeta
            {
                if (txtBinTc.Text != null && txtBinTc.Text.Trim() != "" &&
                    txtAuthTc.Text != null && txtAuthTc.Text.Trim() != "" &&
                    txtLoteTc.Text != null && txtLoteTc.Text.Trim() != "")
                {
                    multipleCobrosInvoiceLine.CardBinText = txtBinTc.Text;
                    multipleCobrosInvoiceLine.CardVoucher = txtAuthTc.Text;
                    multipleCobrosInvoiceLine.LoteVoucher = txtLoteTc.Text;
                }
                else
                {
                    await Toast.Make("Los pagos con tarjeta de crédito requieren Bin, Voucher y Lote.").Show();
                    return;
                }

                if(txtBinTc.Text.Trim().Length < 6)
                {
                    await Toast.Make("Ingrese al menos 6 dígitos para el BIN.").Show();
                    return;
                }

                var CardId = (TarjetasCredito) pickerCardId.SelectedItem;
                var BankTc = (ResBank) ddBankTcId.SelectedItem;
                var paymentType = (TarjetasTipoPago) pickerPaymentTypeId.SelectedItem;
                var planTarjetasCredito = (TarjetasPlazosBanco)pickerPlanId.SelectedItem;

                multipleCobrosInvoiceLine.CardId = CardId.id;
                multipleCobrosInvoiceLine.BankTcId = BankTc.id;
                multipleCobrosInvoiceLine.PaymentTypeId = paymentType.id;
                multipleCobrosInvoiceLine.PlanId = planTarjetasCredito.id;
            }

            if (selPaymentM.name == "check_day" || selPaymentM.name == "check") // && aplica_cheque
            {
                //accountPayment.AccHolderName = 
                //accountPayment.CityId = 
                //accountPayment.PaymentDate =                 
                //accountPayment.NumberCheckText = txtNCheque.Text;

                if (_res_partner_bank != null)
                {
                    multipleCobrosInvoiceLine.PartnerBankId = _res_partner_bank.id;
                    //accountPayment.bank_account_id = _res_partner_bank.id;
                }
                else
                {
                    await Toast.Make("Debe seleccionar una cuenta bancaria antes de continuar.").Show();
                    return;
                }
            }
        }

        //accountPayment.numero_retencion = "0";
        txtMonto.Text = ParseTool.StringValueFix(txtMonto.Text);

        multipleCobrosInvoiceLine.Amount = (decimal)ParseTool.StringToDouble(txtMonto.Text); //.ToString(App.Session.ApplicationCultureInfo);
        multipleCobrosInvoiceLine.Diferencia = 0;

        //multipleCobrosInvoiceLine.Amount = 0;
        //multipleCobrosInvoiceLine.Diferencia = 0;

        List<MultipleCobrosInvoiceLineAi> linesL = new List<MultipleCobrosInvoiceLineAi>();
        if (multipleCobrosInvoiceLineAi != null)
        {
            foreach (var item in multipleCobrosInvoiceLineAi)
            {
                if (item.amount_asigned > 0)
                {
                    //MultipleCobrosInvoiceLineAi accountPaymentInvoiceLine = new MultipleCobrosInvoiceLineAi();
                    //accountPaymentInvoiceLine.reconcile_amount = item.reconcile_amount;
                    //accountPaymentInvoiceLine.invoice_line_id = item.invoice_line_id;
                    //accountPaymentInvoiceLine.invoice_line_id_name = item.invoice_line_id_name;
                    //accountPaymentInvoiceLine.invoice_date = item.invoice_date;
                    //accountPaymentInvoiceLine.payment_state = "draft";
                    //linesL.Add(accountPaymentInvoiceLine);
                    linesL.Add(item);
                }
            }
        }

        multipleCobrosInvoiceLine.lines = linesL.ToArray();

        Debug.WriteLine("Guardar Datos");
        saveData = true;
        await Navigation.PopAsync();
    }
    
    private async void btnCancel_Clicked(object sender, EventArgs e)
    {
        //bool answer = await DisplayAlert("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");
        ////Debug.WriteLine("Answer: " + answer);
        //if (!answer)
        //{
        //    return;
        //}

        //await Navigation.PopModalAsync();
        SendBackButtonPressed();
    }

    //Cierre automático en caso de que se requiera, sin lanzar el popup de confirmacion
    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void BtnTopTools_OnClicked_Clicked(object sender, EventArgs e)
    {
        // Cambiar el texto del botón
        //"&#xf0c9;"; //menu 61641
        //"&#xf00d;"; //cerrar

        var fntSrc = (FontImageSource)btnTopTools.ImageSource;

        int unicodevalue = char.ConvertToUtf32(fntSrc.Glyph, 0);
        
        if (unicodevalue == 61641)
        {
            // Animación de rotación
            await btnTopTools.RotateTo(90, 100); // Rotar 360 grados en 1000 milisegundos
            btnTopTools.Rotation = 0; // Restablecer la rotación después de la animación
            fntSrc.Glyph = "\uf00d";
            TopTools.IsVisible = true;
        }
        else
        {
            // Animación de rotación
            await btnTopTools.RotateTo(-90, 100); // Rotar 360 grados en 1000 milisegundos
            btnTopTools.Rotation = 0; // Restablecer la rotación después de la animación
            fntSrc.Glyph = "\uf0c9";
            TopTools.IsVisible = false;
        }

        //btnTopTools.ImageSource = new FontImageSource
        //{
        //    FontFamily = "FontAwesome5Solid",
        //    Color = Colors.White,
        //    Size = 20,
        //    FontAutoScalingEnabled = true,
        //    Glyph = "\uf0c7"
        //};
    }
}