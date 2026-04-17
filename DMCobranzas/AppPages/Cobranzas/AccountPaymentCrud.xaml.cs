using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using DMCobranzas.Settings.helpers;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.StaticData;
using DMSA.Sync.Core.Controls.Popups;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.DebitCollection;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace DMCobranzas.Controls.Modals;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AccountPaymentCrud : ContentPage
{
    public ObservableCollection<ResBank> Banks { get; set; } = new();
    public ObservableCollection<res_city> Cities { get; set; } = new();
    public res_partner _res_partner { get; set; }
    public ObservableCollection<MultipleCobrosInvoiceLineAi> multipleCobrosInvoiceLineAi { get; set; }
    public res_company Sel_Company_Id { get; set; }    
    public MultipleCobrosInvoice multipleCobrosInvoice { get; set; }
    public MultipleCobrosInvoiceLine multipleCobrosInvoiceLine { get; set; }
    bool isEmptyDb = false;
    public bool saveData { get; set; } = false;
    public bool isNewData { get; set; } = false;
    public int itemIndex { get; set; } = -1;
    List<AppParameter> tipoEmision { get; set; }
    List<account_journal> account_Journals { get; set; }
    List<TarjetasCredito> tarjetasItems { get; set; }
    List<TarjetasTipoPago> tarjetasTipoPagoItems { get; set; }
    List<TarjetasPlazosBanco> plazosBancosItems { get; set; }
    ResBank selected_bank_tc { get; set; }
    private bool LoadingEditionData { get; set; } = false;
    private bool InitializingForm { get; set; } = false;
    private bool HasBeenLoaded { get; set; } = false;
    res_city selected_city { get; set; }

    private int _detailsCount;
    public int detailsCount
    {
        get
        {            
            if (multipleCobrosInvoiceLineAi != null)
            {
                _detailsCount = multipleCobrosInvoiceLineAi.Count();
            }
            else
            {
                _detailsCount = 0;
            }
            return _detailsCount;
        }
    }

    private decimal _totalAssigned;

    public decimal totalAssigned
    {
        get
        {
            if (multipleCobrosInvoiceLineAi != null)
            {
                _totalAssigned = multipleCobrosInvoiceLineAi.Sum(x => x.amount_asigned);
            }
            else
            {
                _totalAssigned = 0;
            }

            return _totalAssigned;
        }
    }

    private bool _isLoadingDoc;
    public bool IsLoadingDocs
    {
        get => _isLoadingDoc;
        set
        {
            _isLoadingDoc = value;
            OnPropertyChanged(nameof(IsLoadingDocs));
            Debug.WriteLine("_isLoadingDoc");
            Debug.WriteLine(_isLoadingDoc);
        }
    }

    decimal totalPayment { get; set; }

    public AccountPaymentCrud(res_partner ResPartner)
	{        
        _res_partner = ResPartner;
        multipleCobrosInvoiceLineAi = new ObservableCollection<MultipleCobrosInvoiceLineAi>();

        InitializeComponent();
        
        BindingContext = this;
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if(HasBeenLoaded)
        {
            return;
        }

        IDispatcherTimer timer;

        timer = Dispatcher.CreateTimer();
        timer.IsRepeating = false;
        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.Tick += async (s, e) =>
        {            
            if (isEmptyDb)
            {
                DisplayAlert("Alerta", "Al parecer no se han descargado las actualizaciones de datos. Actualice antes de continuar.", "OK");
                btnClose_Clicked(null, null);                
            }

            await PrepareForm();

            if (multipleCobrosInvoiceLine==null)
            {
                multipleCobrosInvoiceLine = new MultipleCobrosInvoiceLine();
                isNewData = true;

            }
            else
            {
                await LoadDataForEdition();
                
            }

            HasBeenLoaded = true;

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
                
                await Navigation.PopAsync();                
            }
        });

        return true;
    }

    public ICommand ClearValueCommand { get; set; }

    private async void ClearValue(object obj)
    {        
        Debug.WriteLine(obj);
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

        
        if (multipleCobrosInvoiceLine != null)
        {
            if (_res_partner != null)
            {
                Title = "EDICIÓN - " + _res_partner.id + "-" + _res_partner.name;
            }
            
            await LoadPaymentLines();
        }
        else
        {
            if (_res_partner != null)
            {
                Title = "NUEVO - " + _res_partner.name;
            }

            await LoadPaymentLines();
        }

        SetDepositPayment();

        txtRef.Text = "";

        tipoEmision = TipoEmision.data;
        
        pickerPaymentMethod.ItemsSource = tipoEmision;
        pickerPaymentMethod.ItemDisplayBinding = new Binding("name");
        pickerPaymentMethod.SelectedIndex = 0;
        pickerPaymentMethod.SelectedIndexChanged += pickerPaymentMethod_SelectedIndexChanged;

        await UpdateJournal("transfer");

        var tarjetasCreditoDb = new TarjetasCreditoDb(App.Session.odooConnection.DbNameSqlite);
        tarjetasItems = (await tarjetasCreditoDb.GetItemsAsync(x=> x.active)).ToList();
        pickerCardId.ItemsSource = tarjetasItems;
        pickerCardId.ItemDisplayBinding = new Binding("display_name");
        pickerCardId.SelectedIndex = 0;
        pickerCardId.SelectedIndexChanged += PickerCardId_SelectedIndexChanged;

        string[] bank_ids = { "10","30","17","36", "37", "32", "232", "42" };

        if(App.Session.odooConnection.DbName.Contains("macronegocios"))
        {
            bank_ids = new string[] { "2","1","6","17", "3", "5", "47", "7" };
        }

        var bank = new BankDb(App.Session.odooConnection.DbNameSqlite);
        var bankItems = (await bank.GetItemsAsync(x => bank_ids.Contains( x.bic ))).ToList();

        string[] cities_ids = { "EC09001", "EC17001", "EC01001", "EC24001", "EC24003", "EC13001", "EC13008", "EC09007", "EC09009", "EC23001" };

        var cityDb = new ResCityDb(App.Session.odooConnection.DbNameSqlite);
        var citiesItems = (await cityDb.GetItemsAsync(x => cities_ids.Contains(x.zip))).ToList();

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
            ddBankTcId.SelectedItemChanged += ddBankTcId_SelectedItemChanged;

            ddBank.ItemsSource = Banks;
            ddBank.ItemDisplayBinding = new Binding("name");
            ddBank.SelectedItem = Banks[0];
            ddBank.SelectedItemChanged += ddBank_SelectedItemChanged;

            Cities =
            [
                new res_city { id = 0, name = "No seleccionada" },
                new res_city { id = -1, name = "🔍 Buscar..." }
            ];

            foreach (var cityItem in citiesItems)
            {
                Cities.Add(cityItem);
            }

            ddResCity.ItemsSource = Cities;
            ddResCity.ItemDisplayBinding = new Binding("name");
            ddResCity.SelectedItem = Cities[0];
        });

        TarjetasTipoPagoDb tarjetasTipoPagoDb = new TarjetasTipoPagoDb(App.Session.odooConnection.DbNameSqlite);
        tarjetasTipoPagoItems = (await tarjetasTipoPagoDb.GetItemsAsync(x => x.active)).ToList();
        pickerPaymentTypeId.ItemsSource = tarjetasTipoPagoItems;
        pickerPaymentTypeId.ItemDisplayBinding = new Binding("display_name");
        pickerPaymentTypeId.SelectedIndex = 0;
        pickerPaymentTypeId.SelectedIndexChanged += PickerPaymentTypeId_SelectedIndexChanged;

        InitializingForm = false;
    }

    private async void ddBankTcId_SelectedItemChanged(object sender, object e)
    {
        if (LoadingEditionData)
            return;

        ResBank new_selected_bank = (ResBank)e;

        if (new_selected_bank != null && (new_selected_bank.id == -1 || new_selected_bank.id == 0))
        {
            if (new_selected_bank.id == -1)
            {
                ddBankTcId.SelectedItem = selected_bank_tc;
                var selectedBank = await PopupResBank(sender, null);

                if (selectedBank != null)
                {
                    ddBankTcId.IsEnabled = false;
                    ddBankTcId.ItemsSource = null;

                    var banksCopy = new ObservableCollection<ResBank>();

                    foreach (var b in Banks)
                    {
                        banksCopy.Add(new ResBank
                        {
                            id = b.id,
                            name = b.name,
                        });
                    }

                    banksCopy[0] = selectedBank;
                    Banks = banksCopy;

                    ddBankTcId.ItemsSource = Banks;
                    ddBankTcId.SelectedItem = Banks[0];
                    selected_bank_tc = selectedBank;
                    ddBankTcId.IsEnabled = true;
                    return;
                }
            }
        }

        if (new_selected_bank == null)
        {
            ddBankTcId.IsEnabled = false;
            Debug.WriteLine("Clear");
            ddBankTcId.ItemsSource = null;
            var nsBrand = new ResBank { id = 0, name = "No seleccionada" };
            Banks[0] = nsBrand;
            ddBankTcId.ItemsSource = Banks;
            ddBankTcId.SelectedItem = nsBrand;
            selected_bank_tc = nsBrand;
            ddBankTcId.IsEnabled = true;
        }
        else
        {
            Debug.WriteLine("Seleccion valida directa...");
            selected_bank_tc = new_selected_bank;
        }

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

        if (pickerPaymentTypeId.SelectedIndex != -1)
        {
            var payment_type = (TarjetasTipoPago)pickerPaymentTypeId.SelectedItem;
            _pos_tipo_pago = payment_type.id;
        }

        var tarjetasPlazosBancoDb = new TarjetasPlazosBancoDb(App.Session.odooConnection.DbNameSqlite);
        plazosBancosItems = (await tarjetasPlazosBancoDb.GetItemsAsync(
            x => //x._account_journal_id == selected_journal &&
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
            string type_value = ((AppParameter) picker.SelectedItem).code;

            AccountJournalDb db = new AccountJournalDb(App.Session.odooConnection.DbNameSqlite);

            var q = await db.GetItemsAsync(x => x.type == "bank");

            switch (type_value)
            {
                case "transfer":
                    {
                        q = await db.GetItemsAsync(x => x.type == "bank");
                        SetTransferPayment();
                    }
                    break;
                case "deposito":
                    {
                        q = await db.GetItemsAsync(x => x.type == "bank");
                        SetDepositPayment();
                    }
                    break;
                case "cash":
                    {
                        q = await db.GetItemsAsync(x => x.type == "bank");
                        SetDirectPayment();
                    }
                    break;                
                case "check_day":
                    {
                        q = await db.GetItemsAsync(x => x.type == "bank");
                        SetCheckPayment();

                        if (string.IsNullOrEmpty(txtChequeTitular.Text))
                            txtChequeTitular.Text = _res_partner.name;
                    }
                    break;
                case "check":                
                    {
                        q = await db.GetItemsAsync(x => x.type == "credit" && x.aplica_cheque == true);
                        SetCheckPostPayment();

                        if (string.IsNullOrEmpty(txtChequeTitular.Text))
                            txtChequeTitular.Text = _res_partner.name;
                    }
                    break;
                case "credit_card":
                    {
                        q = await db.GetItemsAsync(x => x.type == "credit" && x.aplica_tarjeta == true);
                        SetCreditCardPayment();
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

            account_Journals = q;

            if (account_Journals.Count == 0)
            {                
                isEmptyDb = true;
                return;
            }

            account_Journals = account_Journals.OrderBy(j => j.name).ToList();

            pickerDiario.ItemsSource = account_Journals;
            pickerDiario.ItemDisplayBinding = new Binding("name");
            pickerDiario.SelectedIndex = -1;
        }
    }

    private async Task UpdateJournal(string type_value)
    {
        AccountJournalDb db = new AccountJournalDb(App.Session.odooConnection.DbNameSqlite);

        var q = await db.GetItemsAsync(x => x.type == "bank");

        switch (type_value)
        {
            case "transfer":
                {
                    q = await db.GetItemsAsync(x => x.type == "bank");
                    SetTransferPayment();
                }
                break;
            case "deposito":
                {
                    q = await db.GetItemsAsync(x => x.type == "bank");
                    SetDepositPayment();
                }
                break;
            case "cash":
                {
                    q = await db.GetItemsAsync(x => x.type == "bank");
                    SetDirectPayment();
                }
                break;
            case "check_day":
                {
                    q = await db.GetItemsAsync(x => x.type == "bank");
                    SetCheckPayment();

                    if (string.IsNullOrEmpty(txtChequeTitular.Text))
                        txtChequeTitular.Text = _res_partner.name;
                }
                break;
            case "check":
                {
                    q = await db.GetItemsAsync(x => x.type == "credit" && x.aplica_cheque == true);
                    SetCheckPostPayment();

                    if (string.IsNullOrEmpty(txtChequeTitular.Text))
                        txtChequeTitular.Text = _res_partner.name;
                }
                break;
            case "credit_card":
                {
                    q = await db.GetItemsAsync(x => x.type == "credit" && x.aplica_tarjeta == true);
                    SetCreditCardPayment();

                    //if(string.IsNullOrEmpty(txtChequeTitular.Text))
                    //txtChequeTitular.Text = _res_partner.name;
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

        account_Journals = q;

        if (account_Journals.Count == 0)
        {            
            isEmptyDb = true;
            return;
        }

        account_Journals = account_Journals.OrderBy(j => j.name).ToList();
        pickerDiario.ItemsSource = account_Journals;
        pickerDiario.ItemDisplayBinding = new Binding("name");
        pickerDiario.SelectedIndex = -1;
    }

    private void SetDirectPayment()
    {
        GrouppickerDiario.IsVisible = false;
        GrouptxtCircular.IsVisible = false;
        GrouppickerFechaPago.IsVisible = false;
        lblFechaPago.Text = "Fecha Pago";
        ddBank.IsVisible = false;
        GrouptxtBancoCuenta.IsVisible = false;

        GrouptxtBinTc.IsVisible = false;
        GrouptxtAuthTc.IsVisible = false;
        GrouptxtLoteTc.IsVisible = false;

        GrouptxtNCheque.IsVisible = false;
        GrouptxtChequeTitular.IsVisible = false;
        ddResCity.IsVisible = false;

        GrouppickerFechaCheque.IsVisible = false;

        pickerCardId.IsVisible = false;
        ddBankTcId.IsVisible = false;
        pickerPaymentTypeId.IsVisible = false;
        pickerPlanId.IsVisible = false;

        txtCircular.Placeholder = "Número de comprobante";
        lblCircular.Text = "Número de comprobante";
    }

    private void SetTransferPayment()
    {
        GrouppickerDiario.IsVisible = true;
        GrouppickerFechaPago.IsVisible = true;
        lblFechaPago.Text = "Fecha Pago";
        pickerFechaPago.MinimumDate = DateTime.Now.AddDays(-365);
        ddBank.IsVisible = true;
        GrouptxtBancoCuenta.IsVisible = true;

        GrouptxtCircular.IsVisible = true;
        GrouptxtBinTc.IsVisible = false;
        GrouptxtAuthTc.IsVisible = false;
        GrouptxtLoteTc.IsVisible = false;

        GrouptxtNCheque.IsVisible = false;
        GrouptxtChequeTitular.IsVisible = false;
        ddResCity.IsVisible = false;

        GrouppickerFechaCheque.IsVisible = false;

        pickerCardId.IsVisible = false;
        ddBankTcId.IsVisible = false;
        pickerPaymentTypeId.IsVisible = false;
        pickerPlanId.IsVisible = false;

        txtCircular.Placeholder = "Número de comprobante";
        lblCircular.Text = "Número de comprobante";
    }

    private void SetDepositPayment()
    {
        GrouppickerDiario.IsVisible = true;
        GrouptxtCircular.IsVisible = true;
        GrouppickerFechaPago.IsVisible = true;
        lblFechaPago.Text = "Fecha Pago";
        pickerFechaPago.MinimumDate = DateTime.Now.AddDays(-365);
        ddBank.IsVisible = false;
        GrouptxtBancoCuenta.IsVisible = false;

        GrouptxtBinTc.IsVisible = false;
        GrouptxtAuthTc.IsVisible = false;
        GrouptxtLoteTc.IsVisible = false;

        GrouptxtNCheque.IsVisible = false;
        GrouptxtChequeTitular.IsVisible = false;
        ddResCity.IsVisible = false;

        GrouppickerFechaCheque.IsVisible = false;

        pickerCardId.IsVisible = false;
        ddBankTcId.IsVisible = false;
        pickerPaymentTypeId.IsVisible = false;
        pickerPlanId.IsVisible = false;

        txtCircular.Placeholder = "Número de comprobante";
        lblCircular.Text = "Número de comprobante";
    }

    private void SetCheckPayment()
    {
        GrouppickerDiario.IsVisible = false;
        GrouptxtCircular.IsVisible = false;
        GrouppickerFechaPago.IsVisible = false;
        lblFechaPago.Text = "Fecha Pago";

        ddBank.IsVisible = true;
        GrouptxtBancoCuenta.IsVisible = true;

        GrouptxtBinTc.IsVisible = false;
        GrouptxtAuthTc.IsVisible = false;
        GrouptxtLoteTc.IsVisible = false;

        GrouptxtNCheque.IsVisible = true;
        GrouptxtChequeTitular.IsVisible = true;
        ddResCity.IsVisible = true;

        GrouppickerFechaCheque.IsVisible = true;
        pickerFechaCheque.MinimumDate = DateTime.Today;
        pickerFechaCheque.IsEnabled = false;        

        pickerCardId.IsVisible = false;
        ddBankTcId.IsVisible = false;
        pickerPaymentTypeId.IsVisible = false;
        pickerPlanId.IsVisible = false;
                
        txtCircular.Placeholder = "Número de comprobante";
        lblCircular.Text = "Número de comprobante";
    }

    private void SetCheckPostPayment()
    {
        GrouppickerDiario.IsVisible = true;
        GrouptxtCircular.IsVisible = false;
        GrouppickerFechaPago.IsVisible = true;
        lblFechaPago.Text = "Fec. Recepción Cheque";

        ddBank.IsVisible = true;
        GrouptxtBancoCuenta.IsVisible = true;

        GrouptxtBinTc.IsVisible = false;
        GrouptxtAuthTc.IsVisible = false;
        GrouptxtLoteTc.IsVisible = false;

        GrouptxtNCheque.IsVisible = true;
        GrouptxtChequeTitular.IsVisible = true;
        ddResCity.IsVisible = true;

        GrouppickerFechaCheque.IsVisible = true;
        pickerFechaCheque.MinimumDate = DateTime.Today.AddDays(1);
        pickerFechaCheque.IsEnabled = true;        

        pickerCardId.IsVisible = false;
        ddBankTcId.IsVisible = false;
        pickerPaymentTypeId.IsVisible = false;
        pickerPlanId.IsVisible = false;

        txtCircular.Placeholder = "Glosa";
        lblCircular.Text = "Glosa";
    }

    private void SetCreditCardPayment()
    {
        GrouppickerDiario.IsVisible = true;
        GrouptxtCircular.IsVisible = false;
        GrouppickerFechaPago.IsVisible = true;
        lblFechaPago.Text = "Fecha Pago";

        ddBank.IsVisible = false;
        GrouptxtBancoCuenta.IsVisible = false;

        GrouptxtBinTc.IsVisible = true;
        GrouptxtAuthTc.IsVisible = false;
        GrouptxtLoteTc.IsVisible = true;

        GrouptxtNCheque.IsVisible = false;
        GrouptxtChequeTitular.IsVisible = true;
        ddResCity.IsVisible = false;

        GrouppickerFechaCheque.IsVisible = false;

        pickerCardId.IsVisible = true;
        ddBankTcId.IsVisible = true;
        pickerPaymentTypeId.IsVisible = true;
        pickerPlanId.IsVisible = true;

        txtCircular.Placeholder = "Glosa";
        lblCircular.Text = "Glosa";
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
        IsLoadingDocs = true;

        AccountMoveDb accountMoveDb = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
        AccountMoveLineDb accountMoveLineDb = new AccountMoveLineDb(App.Session.odooConnection.DbNameSqlite);

        var accMovesByCustomer = await accountMoveDb.GetItemsByPartnerAndCompany(_res_partner, Sel_Company_Id);

        var moveIds = accMovesByCustomer.Select(x => x.id).ToList();

        var paymentTerms = await accountMoveLineDb.GetItemsAsync(
            x => moveIds.Contains(x._move_id) && x.display_type == "payment_term");

        var paymentTermMap = paymentTerms.ToDictionary(x => x._move_id);

        List<MultipleCobrosInvoiceLineAi> multipleCobrosInvoiceLinesAiAux = new(accMovesByCustomer.Count);

        decimal totalResidualPayment = 0;
        totalPayment = (decimal)ParseTool.StringToDouble(txtMonto.Text);
        decimal totalApplied = 0;
        decimal totalInvoicePayment = 0;
        decimal residualAmount = 0;

        if (totalPayment > 0)
        {            
            txtMonto.IsEnabled = false;
        }

        foreach (var accountMoveItem in accMovesByCustomer)
        {
            if (!paymentTermMap.TryGetValue(accountMoveItem.id, out var itemPaymentTerm))
                continue;

            MultipleCobrosInvoiceLineAi cobrosInvoiceLineAiAux = new()
            {
                multiple_cobros_invoice_line_id = accountMoveItem.id,
                invoice_line_id = itemPaymentTerm.id,
                invoice_id = accountMoveItem.id,
                invoice_name = accountMoveItem.name,
                docnum_mask = accountMoveItem.docnum_mask,
                invoice_date = accountMoveItem.invoice_date,
                invoice_date_due = accountMoveItem.invoice_date_due,
                amount_residual = accountMoveItem.amount_residual
            };

            residualAmount += cobrosInvoiceLineAiAux.amount_residual;

            totalResidualPayment = totalPayment - totalApplied;

            if (totalResidualPayment > 0)
            {
                totalInvoicePayment = totalResidualPayment < cobrosInvoiceLineAiAux.amount_residual
                    ? totalResidualPayment
                    : cobrosInvoiceLineAiAux.amount_residual;

                cobrosInvoiceLineAiAux.amount_asigned = totalInvoicePayment;
                totalApplied += totalInvoicePayment;
            }
            else
            {
                cobrosInvoiceLineAiAux.amount_asigned = 0;
            }

            multipleCobrosInvoiceLinesAiAux.Add(cobrosInvoiceLineAiAux);
        }

        var ordered = multipleCobrosInvoiceLinesAiAux.OrderBy(x => x.invoice_date);

        multipleCobrosInvoiceLineAi.Clear();

        foreach (var item in ordered)
        {
            multipleCobrosInvoiceLineAi.Add(item);
        }

        OnPropertyChanged(nameof(detailsCount));

        lblMonto.Text = "($ " + residualAmount.ToString() + ")";

        IsLoadingDocs = false;
        OnPropertyChanged(nameof(totalAssigned));
    }

    private async Task LoadDataForEdition()
    {
        LoadingEditionData = true;

        var tipoEmisionSelected = tipoEmision.Where(x => x.code == multipleCobrosInvoiceLine.Type).FirstOrDefault();

        pickerPaymentMethod.SelectedItem = tipoEmisionSelected;

        AccountJournalDb accountJournalDb = new AccountJournalDb(App.Session.odooConnection.DbNameSqlite);
             
        var selDiario = await accountJournalDb.GetItemAsync(x=>x.id == multipleCobrosInvoiceLine.JournalId);

        switch(tipoEmisionSelected.code)
        {
            case "transfer":
                {
                    SetTransferPayment();
                }
                break;
            case "deposito":
                {
                    SetDepositPayment();
                }
                break;
            case "cash":
                {
                    SetDirectPayment();
                }
                break;
            case "check_day":
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

        await UpdateJournal(tipoEmisionSelected.code);
        
        if (selDiario != null)
        {
            var selDiarioMemory = account_Journals.Where(x => x.id == selDiario.id).FirstOrDefault();
            pickerDiario.SelectedItem = selDiarioMemory;
        }
        
        txtBinTc.Text = multipleCobrosInvoiceLine.CardBinText;
        txtAuthTc.Text = multipleCobrosInvoiceLine.CardVoucher;
        txtLoteTc.Text = multipleCobrosInvoiceLine.LoteVoucher;

        txtChequeTitular.Text = multipleCobrosInvoiceLine.AccHolderName;
        
        var cityDb = new ResCityDb(App.Session.odooConnection.DbNameSqlite);
        
        var itemCity = await cityDb.GetItemAsync(x => x.id == multipleCobrosInvoiceLine.CityId);

        if (itemCity != null)
        {
            //MainThread.BeginInvokeOnMainThread(() =>
            //{

            var citiesCopy = new ObservableCollection<res_city>();

            foreach (var b in Cities)
            {
                citiesCopy.Add(new res_city
                {
                    id = b.id,
                    name = b.name,
                });
            }

            citiesCopy[0] = new res_city { id = 0, name = "No seleccionada" };
            Cities = citiesCopy;

            Cities[0] = itemCity;
                ddResCity.SelectedItem = Cities[0];
            //});
        }

        pickerFechaCheque.Date = multipleCobrosInvoiceLine.WithdrawalDate.Value;

        txtBancoCuenta.Text = multipleCobrosInvoiceLine.AccNumber;
                
        
        if (multipleCobrosInvoiceLine.BankId != null && multipleCobrosInvoiceLine.BankId > 0)
        {
            var banco = Banks.Where(x => x.id == multipleCobrosInvoiceLine.BankId).FirstOrDefault();
            if (banco == null)
            {
                var bank = new BankDb(App.Session.odooConnection.DbNameSqlite);
                banco = (await bank.GetItemsAsync(x => x.id == multipleCobrosInvoiceLine.BankId)).FirstOrDefault();
                Banks.Add(banco);                
            }
            ddBank.SelectedItem = banco;
        }

        var tarjeta = tarjetasItems.Where(x => x.id == multipleCobrosInvoiceLine.CardId).FirstOrDefault();
        pickerCardId.SelectedItem = tarjeta;
        
        if (multipleCobrosInvoiceLine.BankTcId != null && multipleCobrosInvoiceLine.BankTcId > 0)
        {
            var bancoTarjeta = Banks.Where(x => x.id == multipleCobrosInvoiceLine.BankTcId).FirstOrDefault();
            if (bancoTarjeta == null)
            {
                var bank = new BankDb(App.Session.odooConnection.DbNameSqlite);
                bancoTarjeta = (await bank.GetItemsAsync(x => x.id == multipleCobrosInvoiceLine.BankTcId)).FirstOrDefault();
                Banks.Add(bancoTarjeta);                
            }
            ddBankTcId.SelectedItem = bancoTarjeta;
        }

        var tarjetaTipoPago = tarjetasTipoPagoItems.Where(x => x.id == multipleCobrosInvoiceLine.PaymentTypeId).FirstOrDefault();
        pickerPaymentTypeId.SelectedItem = tarjetaTipoPago;

        await RefreshPlan();

        var plazosBancosItem = plazosBancosItems.Where(x => x.id == multipleCobrosInvoiceLine.PlanId).FirstOrDefault();
        pickerPlanId.SelectedItem = plazosBancosItem;

        txtMonto.Text = multipleCobrosInvoiceLine.Amount.ToString(); //.ToString(App.Session.ApplicationCultureInfo);
        totalPayment = (decimal) multipleCobrosInvoiceLine.Amount;

        if(totalPayment > 0)
        {
            txtMonto.IsEnabled = false;
        }

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

        IsLoadingDocs = true;

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

        multipleCobrosInvoiceLineAi.Clear();

        foreach (var item in accountPaymentLinesMem.OrderBy(x => x.invoice_date))
        {
            multipleCobrosInvoiceLineAi.Add(item);
        }

        OnPropertyChanged(nameof(detailsCount));
        OnPropertyChanged(nameof(totalAssigned));

        IsLoadingDocs = false;
    }
    private async void btnLoadDocs_Clicked(object sender, EventArgs e)
    {
        await LoadPaymentLinesForNew();
    }

    private async Task<List<account_move>> EvalLinesRequired()
    {
        AccountMoveDb accountMoveDb = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
        AccountMoveLineDb accountMoveLineDb = new AccountMoveLineDb(App.Session.odooConnection.DbNameSqlite);

        var accMovesByCustomer = await accountMoveDb.GetItemsByPartnerAndCompany(_res_partner, Sel_Company_Id);

        var moveIds = accMovesByCustomer.Select(x => x.id).ToList();

        var paymentTerms = await accountMoveLineDb.GetItemsAsync(
            x => moveIds.Contains(x._move_id) && x.display_type == "payment_term");

        var paymentTermMap = paymentTerms.ToDictionary(x => x._move_id);

        List<MultipleCobrosInvoiceLineAi> multipleCobrosInvoiceLinesAiAux = new(accMovesByCustomer.Count);        
        List<account_move> foundCompatible = new List<account_move>();
        
        foreach (var accountMoveItem in accMovesByCustomer)
        {
            if (!paymentTermMap.TryGetValue(accountMoveItem.id, out var itemPaymentTerm))
                continue;

            foundCompatible.Add(accountMoveItem);            
        }

        var totalFound = foundCompatible
            .Where(x => !multipleCobrosInvoiceLineAi
            .Any(y => y.invoice_id == x.id && y.amount_asigned > 0))
            .ToList();

        return totalFound;
    }

    private async void btnSave_Clicked(object sender, EventArgs e)
    {
        double totalPagadoDbl = 0;

        if (!double.TryParse(txtMonto.Text, out totalPagadoDbl) || totalPagadoDbl == 0)
        {
            await Toast.Make("No se han ingresado valores correctos, no se puede guardar.").Show();
            return;
        }

        if(detailsCount > 0 && totalAssigned < totalPayment)
        {
            var linesNotAssigned = await EvalLinesRequired();
            if (linesNotAssigned.Count > 0)
            {
                await Toast.Make("No se puede guardar hasta asignar a todo el valor seleccionado").Show();
            }
            return;
        }

        //if (detailsCount == 0 && totalAssigned < totalPayment)
        //{            
        //    await Toast.Make("No se puede guardar hasta asignar a todo el valor seleccionado").Show();            
        //    return;
        //}

        if (multipleCobrosInvoiceLine == null)
        {
            multipleCobrosInvoiceLine = new MultipleCobrosInvoiceLine();
            multipleCobrosInvoiceLine.MultipleCobrosInvoiceId = 0;
        }

        multipleCobrosInvoiceLine.CompanyId = Sel_Company_Id.id;
        multipleCobrosInvoiceLine.Resumen = txtRef.Text;
        multipleCobrosInvoiceLine.Circular = txtCircular.Text;
        multipleCobrosInvoiceLine.PartnerId = _res_partner.id;

        multipleCobrosInvoiceLine.Type = ((AppParameter)pickerPaymentMethod.SelectedItem).code;

        if (multipleCobrosInvoiceLine.Type != "cash"
            && multipleCobrosInvoiceLine.Type !="check_day")
        {
            if (pickerDiario.SelectedItem == null)
            {
                await Toast.Make("No se ha seleccionado el diario, no se puede guardar.").Show();
                return;
            }

            multipleCobrosInvoiceLine.JournalId = ((account_journal)pickerDiario.SelectedItem).id;
            multipleCobrosInvoiceLine.journal_name = ((account_journal)pickerDiario.SelectedItem).name;
        }
        else
        {
           
        }
                        
        multipleCobrosInvoiceLine.PartnerType = "customer";
        multipleCobrosInvoiceLine.PaymentType = "inbound";
        multipleCobrosInvoiceLine.PaymentDate = pickerFechaPago.Date;
        
        AppParameter selPaymentM = null;
        multipleCobrosInvoiceLine.NumberCheckText = txtNCheque.Text;
        multipleCobrosInvoiceLine.WithdrawalDate = pickerFechaCheque.Date;

        if (pickerPaymentMethod.SelectedItem != null)
        {
            selPaymentM = (AppParameter)pickerPaymentMethod.SelectedItem;
            
            multipleCobrosInvoiceLine.WithdrawalDate = pickerFechaCheque.Date;
            multipleCobrosInvoiceLine.AccNumber = txtBancoCuenta.Text;

            int bank_id = 0;
            if (ddBank.SelectedItem != null) 
            {
                bank_id = ((ResBank)ddBank.SelectedItem).id;
            }

            multipleCobrosInvoiceLine.BankId = bank_id;

            if (ddResCity.SelectedItem != null)
            {
                var itemCity = (res_city)ddResCity.SelectedItem;
                multipleCobrosInvoiceLine.CityId = itemCity.id;
            }

            if (selPaymentM.code == "transferencia")
            {
                //multipleCobrosInvoiceLine.Circular = txtDepositoConfirmar.Text;
            }

            if (selPaymentM.code == "credit_card") //&& aplica_tarjeta
            {
                if (txtBinTc.Text != null && txtBinTc.Text.Trim() != "" &&                    
                    txtLoteTc.Text != null && txtLoteTc.Text.Trim() != "")
                {
                    multipleCobrosInvoiceLine.CardBinText = txtBinTc.Text;                    
                    multipleCobrosInvoiceLine.LoteVoucher = txtLoteTc.Text;
                }
                else
                {
                    await Toast.Make("Los pagos con tarjeta de crédito requieren Bin y Lote.").Show();
                    return;
                }

                //Se quitará
                ////if (txtBinTc.Text != null && txtBinTc.Text.Trim() != "" &&
                ////    txtAuthTc.Text != null && txtAuthTc.Text.Trim() != "" &&
                ////    txtLoteTc.Text != null && txtLoteTc.Text.Trim() != "")
                ////{
                ////    multipleCobrosInvoiceLine.CardBinText = txtBinTc.Text;
                ////    multipleCobrosInvoiceLine.CardVoucher = txtAuthTc.Text;
                ////    multipleCobrosInvoiceLine.LoteVoucher = txtLoteTc.Text;
                ////}
                ////else
                ////{
                ////    await Toast.Make("Los pagos con tarjeta de crédito requieren Bin, Voucher y Lote.").Show();
                ////    return;
                ////}

                if(txtBinTc.Text.Trim().Length < 6)
                {
                    await Toast.Make("Ingrese al menos 6 dígitos para el BIN.").Show();
                    return;
                }

                var CardId = (TarjetasCredito) pickerCardId.SelectedItem;
                var BankTc = (ResBank) ddBankTcId.SelectedItem;
                multipleCobrosInvoiceLine.CardId = CardId.id;
                multipleCobrosInvoiceLine.BankTcId = BankTc.id;

                if (pickerPaymentTypeId != null)
                {
                    var paymentType = (TarjetasTipoPago)pickerPaymentTypeId.SelectedItem;
                    multipleCobrosInvoiceLine.PaymentTypeId = paymentType.id;
                }

                if (pickerPlanId.SelectedItem != null)
                {
                    var planTarjetasCredito = (TarjetasPlazosBanco)pickerPlanId.SelectedItem;
                    multipleCobrosInvoiceLine.PlanId = planTarjetasCredito.id;
                }
            }

            if(selPaymentM.code == "check_day")
            {

            }

            if (selPaymentM.code == "check") // && aplica_cheque
            {
                if (pickerFechaCheque.Date <= DateTime.Today)
                {
                    await Toast.Make("La fecha del cheque debe ser mayor a la fecha actual.").Show();
                    return;
                }
            }
        }
       
        txtMonto.Text = ParseTool.StringValueFix(txtMonto.Text);

        multipleCobrosInvoiceLine.Amount = (decimal)ParseTool.StringToDouble(txtMonto.Text); //.ToString(App.Session.ApplicationCultureInfo);
        multipleCobrosInvoiceLine.Diferencia = 0;

        List<MultipleCobrosInvoiceLineAi> linesL = new List<MultipleCobrosInvoiceLineAi>();
        if (multipleCobrosInvoiceLineAi != null)
        {
            foreach (var item in multipleCobrosInvoiceLineAi)
            {
                if (item.amount_asigned > 0)
                {                    
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
        SendBackButtonPressed();
    }

    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    ResBank selected_bank { get; set; }
    async Task<ResBank> PopupResBank(object sender, EventArgs e)
    {
        ResBank selected_bank_item = null;

        var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
        var returnResultPopup = new PopupSelectBank(popupSizeConstants);
        returnResultPopup.Company = App.Session.res_Company;
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        var result = await PopupExtensions.ShowPopupAsync<ResBank>(App.Current.MainPage, returnResultPopup);
        
        if (result.Result != null)
        {
            selected_bank_item = result.Result;            
        }

        return selected_bank_item;
    }

    private async void ddBank_SelectedItemChanged(object? sender, object e)
    {
        if (LoadingEditionData)
            return;

        ResBank new_selected_bank = (ResBank)e;
        
        if (new_selected_bank != null && (new_selected_bank.id == -1 || new_selected_bank.id == 0))
        {
            if (new_selected_bank.id == -1)
            {                
                ddBank.SelectedItem = selected_bank;
                var selectedBank = await PopupResBank(sender, null);

                if (selectedBank != null)
                {
                    ddBank.IsEnabled = false;
                    ddBank.ItemsSource = null;

                    var banksCopy = new ObservableCollection<ResBank>();

                    foreach (var b in Banks)
                    {
                        banksCopy.Add(new ResBank
                        {
                            id = b.id,
                            name = b.name,
                        });
                    }

                    banksCopy[0] = selectedBank;
                    Banks = banksCopy;
                    
                    ddBank.ItemsSource = Banks;
                    ddBank.SelectedItem = Banks[0];
                    selected_bank = selectedBank;
                    ddBank.IsEnabled = true;
                    return;
                }
            }
        }

        if (new_selected_bank == null)
        {
            ddBank.IsEnabled = false;
            Debug.WriteLine("Clear");
            ddBank.ItemsSource = null;
            var nsBrand = new ResBank { id = 0, name = "No seleccionada" };

            var banksCopy = new ObservableCollection<ResBank>();

            foreach (var b in Banks)
            {
                banksCopy.Add(new ResBank
                {
                    id = b.id,
                    name = b.name,
                });
            }

            banksCopy[0] = nsBrand;
            Banks = banksCopy;

            ddBank.ItemsSource = Banks;
            ddBank.SelectedItem = nsBrand;
            selected_bank = nsBrand;
            ddBank.IsEnabled = true;
        }
        else
        {
            Debug.WriteLine("Seleccion valida directa...");
            selected_bank = new_selected_bank;
        }        
    }
    
    async Task<res_city> PopupResCity(object sender, EventArgs e)
    {
        res_city selected_bank_item = null;

        var popupSizeConstants = new DMSA.Sync.Core.Controls.Popups.PopupSizeConstants(DeviceDisplay.Current);
        popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
        var returnResultPopup = new PopupSelectCity(popupSizeConstants);
        returnResultPopup.Company = App.Session.res_Company;
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        var result = await PopupExtensions.ShowPopupAsync<res_city>(App.Current.MainPage, returnResultPopup);

        if (result.Result != null)
        {
            selected_bank_item = result.Result;
        }

        return selected_bank_item;
    }

    private async void ddResCity_SelectedItemChanged(object sender, object e)
    {
        res_city new_selected_city = (res_city)e;

        if (new_selected_city != null && (new_selected_city.id == -1 || new_selected_city.id == 0))
        {
            if (new_selected_city.id == -1)
            {
                ddResCity.SelectedItem = selected_city;
                var selectedCity = await PopupResCity(sender, null);

                if (selectedCity != null)
                {
                    ddResCity.IsEnabled = false;
                    ddResCity.ItemsSource = null;
                    Cities[0] = selectedCity;
                    ddResCity.ItemsSource = Cities;
                    ddResCity.SelectedItem = selectedCity;
                    selected_city = selectedCity;
                    ddResCity.IsEnabled = true;
                    return;
                }
            }
        }

        if (new_selected_city == null)
        {
            ddResCity.IsEnabled = false;
            Debug.WriteLine("Clear");
            ddResCity.ItemsSource = null;
            var nsBrand = new res_city { id = 0, name = "No seleccionada" };
            Cities[0] = nsBrand;
            ddResCity.ItemsSource = Cities;
            ddResCity.SelectedItem = nsBrand;
            selected_city = nsBrand;
            ddResCity.IsEnabled = true;
        }
        else
        {
            Debug.WriteLine("Seleccion valida directa...");
            selected_city = new_selected_city;
        }
    }

    public static decimal ToDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0m;

        value = value.Replace(",", "."); // normaliza

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        return 0m;
    }

    public ICommand OnValueChangedCommand => new Command<object>(async item =>
    {
        if (IsLoadingDocs || item == null)
            return;

        var changedItem = item as MultipleCobrosInvoiceLineAi;
        if (changedItem == null)
            return;

        changedItem.EventsOn = false;

        foreach (var current in multipleCobrosInvoiceLineAi)
            current.EventsOn = false;

        Debug.WriteLine("Linea modificada..");

        try
        {
            totalPayment = ToDecimal(txtMonto.Text);

            int startIndex = multipleCobrosInvoiceLineAi.IndexOf(changedItem);
            if (startIndex < 0)
                return;

            decimal acumuladoAnterior = 0m;

            for (int i = 0; i < startIndex; i++)
                acumuladoAnterior += multipleCobrosInvoiceLineAi[i].amount_asigned;

            decimal montoDisponible = totalPayment - acumuladoAnterior;

            if (montoDisponible < 0)
                montoDisponible = 0;
            
            decimal maxPermitido = Math.Min(montoDisponible, changedItem.amount_residual);

            if (changedItem.amount_asigned > maxPermitido)
            {                
                changedItem.amount_asigned = maxPermitido;
                
                await Toast.Make($"El valor excede el máximo permitido ({maxPermitido:0.##})").Show();
            }

            decimal montoRestante = totalPayment - acumuladoAnterior - changedItem.amount_asigned;

            if (montoRestante < 0)
                montoRestante = 0;
            
            for (int i = startIndex + 1; i < multipleCobrosInvoiceLineAi.Count; i++)
            {
                var current = multipleCobrosInvoiceLineAi[i];

                if (montoRestante <= 0)
                {
                    current.amount_asigned = 0;
                    continue;
                }

                decimal saldo = current.amount_residual;

                if (montoRestante >= saldo)
                {
                    current.amount_asigned = saldo;
                    montoRestante -= saldo;
                }
                else
                {
                    current.amount_asigned = montoRestante;
                    montoRestante = 0;
                }

                Debug.WriteLine($"Item {i}: asignado {current.amount_asigned}, restante {montoRestante}");
            }

            OnPropertyChanged(nameof(totalAssigned));

            Dispatcher.Dispatch(() =>
            {
                foreach (var current in multipleCobrosInvoiceLineAi)
                    current.EventsOn = true;
            });
        }
        finally
        {
        }
    });

    private void btnChangeAmount_Clicked(object sender, EventArgs e)
    {
        txtMonto.IsEnabled = true;
    }
}