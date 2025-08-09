using ApiManager;

using DMCobranzas.Models;
using DMCobranzas.Settings.helpers;
using DMCobranzas.Settings.Sqlite;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Sample.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMSA.Models.MovilCobranzas.Api;
using DMSA.Models.Odoo.Native;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using System.Data.Common;
using DMSA.Models.Odoo.DMCobranzas;

namespace DMCobranzas.Controls.Modals;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AccountPaymentCrud : ContentPage
{
    public res_partner _res_partner { get; set; }
    public res_partner_bank _res_partner_bank { get; set; }

    //Arreglo representantivo de las lineas de pago
    public AccountPaymentInvoiceLineAuxiliar[] accountPaymentLines { get; set; }

    public res_company Sel_Company_Id { get; set; }
    //public res_company res_Company { get; set; }
    public AccountPaymentHeader accountPaymentHeader { get; set; }
    public AccountPayment accountPayment { get; set; }

    bool isEmptyDb = false;
    public bool saveData { get; set; } = false;
    public bool isNewData { get; set; } = false;
    public int itemIndex { get; set; } = -1;
    List<account_journal_type> account_Journal_Types { get; set; }
    List<account_journal> account_Journals { get; set; }
    //st_formaspago[] formaspagos { get; set; }
    //st_bancos[] bancos { get; set; }
    //st_cuentas[] cuentas { get; set; }
    //st_tarjetas[] tarjetas { get; set; }
    //st_formaspago selFormaspagos { get; set; }
    //Parametros[] tipoCheque { get; set; }
    //Parametros[] indicador { get; set; }
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
            if (accountPayment==null)
            {
                accountPayment = new AccountPayment();
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

    protected override bool OnBackButtonPressed()
    {
        var tcs = new TaskCompletionSource<bool>();

        Dispatcher.Dispatch(async () =>
        {
            var leave = await DisplayAlert("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");

            if (leave)
            {
                //await Navigation.PushAsync(new MainPage());
                base.OnBackButtonPressed();
                //await Navigation.PopModalAsync();
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

        if (accountPayment != null)
        {
            //MODO EDICION -- SI YA ESTABA GUARDADO PREVIAMENTE
            if (accountPaymentHeader != null)
            {
                var dbCompany = new CompanyDb();
                Sel_Company_Id = await dbCompany.GetItem(accountPaymentHeader.company_id);
            }
            else
            {
                //Si se está editando antes de ser guardado
                var dbCompany = new CompanyDb();
                Sel_Company_Id = await dbCompany.GetItem(accountPayment.company_id);
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

        pickerDiario.SelectedIndexChanged += PickerDiario_SelectedIndexChanged;
        pickerPaymentMethod.SelectedIndexChanged += pickerPaymentMethod_SelectedIndexChanged;

        //Formas de Pago
        var database = new AccountJournalDb();
        var lstDiarios = await database.GetItemsAsync();
        
        if (lstDiarios.Count == 0)
        {
            //await toast.Show(cancellationTokenSource.Token);
            //await DisplayAlert("Alert", "You have been alerted", "OK");
            isEmptyDb = true;
            return;
        }

        AccountJournalDb db = new AccountJournalDb();
        account_Journals = (await db.GetItemsAsync()).Where(
            j => j.CompanyId == Sel_Company_Id.id && j.use_mobile_app == true).ToList();

        account_Journals = account_Journals.OrderBy(j => j.name).ToList();

        pickerDiario.ItemsSource = account_Journals;
        pickerDiario.ItemDisplayBinding = new Binding("name");
        pickerDiario.SelectedIndex = 0;

        //Se asume que el resultado devuelto desde la base de datos está en la primera fila
        account_Journal_Types = new List<account_journal_type>();
        account_Journal_Types.Add(
            new account_journal_type()
            {
                id = 1,
                code = "bank",
                name = "Banco"
            }
        );

        account_Journal_Types.Add(
            new account_journal_type()
            {
                id = 2,
                code = "cash",
                name = "Efectivo"
            }
        );

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


        List<AppParameter> ls_tipoCheque = new List<AppParameter>();
        ls_tipoCheque.Add(new AppParameter() { name = "D", value = "AL DIA" });
        ls_tipoCheque.Add(new AppParameter() { name = "P", value = "POSFECHADO" });

        List<AppParameter> ls_indicador = new List<AppParameter>();
        ls_indicador.Add(new AppParameter() { name = "N", value = "PROPIO" });
        ls_indicador.Add(new AppParameter() { name = "S", value = "TERCEROS" });
    
        await UITools.ShowLoading(_absoluteLayout);
        //Cargar documentos

        //Se define si se cargan los datos de un accountPayment ya existente (edición)
        //o se prepara el formulario para ingresar datos nuevos
        // Debemos tomar en cuenta de que es posible que se deba implementar funciones de agregar
        // facturas nuevas en el detalle
        
        if (accountPayment != null)
        {
            if (_res_partner != null)
            {
                lblTitle.Text = "EDICIÓN - " + _res_partner.name;
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
                lblTitle.Text = "NUEVO - " + _res_partner.name;
            }

            //await LoadPaymentLinesForNew();
            await LoadPaymentLines();
        }

        ChequeGroup.IsVisible = false;
        CreditCardGroup.IsVisible = false;

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

        InitializingForm = false;
    }

    private async void pickerPaymentMethod_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (LoadingEditionData)
            return;
        
        var picker = (Picker) sender;
        int selectedIndex = picker.SelectedIndex;

        if (selectedIndex != -1)
        {
            var paymentMethodSelected = (inbound_payment_method) picker.SelectedItem;

            //accountPaymentHeader
            //accountPaymentHeader.company_id

            CompanyDb companyDb = new CompanyDb();

            //CRASH: 2024
            //FIX:
            //if(accountPaymentHeader!= null)
            //var companyId = await companyDb.GetItem(accountPaymentHeader.company_id);
            var companyId = await companyDb.GetItem(Sel_Company_Id.id);
            
            //companyId.check_journal_id_

            if (paymentMethodSelected.code == "in_third_checks")
            {
                var cuentaDiario = (account_journal) pickerDiario.SelectedItem;

                //check_journal_id_: Contiene el parametro obtenido desde Odoo para saber que cuenta se usa
                // para los cheques
                
                //TODO 2024: QUITADO POR MODO NUEVO
                //if (cuentaDiario.id == companyId.check_journal_id_)
                //{
                    ChequeGroup.IsVisible = true;
                    CreditCardGroup.IsVisible = false;
                //}
            }
            else
            {
                ChequeGroup.IsVisible = false;

                if (paymentMethodSelected.code.Contains("tc_in"))
                {
                    CreditCardGroup.IsVisible = true;
                }
            }
        }
    }

    private async void PickerDiario_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (LoadingEditionData) // || InitializingForm)
            return;
        //Debug.WriteLine(e.);
        //throw new NotImplementedException();

        var picker = (Picker) sender;
        int selectedIndex = picker.SelectedIndex;

        if (selectedIndex != -1)
        {
            //Debug.WriteLine(selectedIndex);
            var cuentaDiario = (account_journal) picker.SelectedItem;
            //Debug.WriteLine(cuentaDiario.code);
            
            //int check_journal_id = 65;

            //if (cuentaDiario.id == check_journal_id)
            //{

            //}

            //TODO: 
            ////if(cuentaDiario.code == "DRV1" || cuentaDiario.code == "CCLI")
            ////{
            ////    ChequeGroup.IsVisible = true;
            ////    //cargar cuentas bancarias

            ////    //PartnerBankDb partnerBankDb = new PartnerBankDb();
            ////    //ObservableCollection<res_partner_bank> l_partnerBank = new ObservableCollection<res_partner_bank>();
            ////    //l_partnerBank = new ObservableCollection<res_partner_bank>(await partnerBankDb.GetItemsAsync());
            ////    ////pickerAccountBank
            ////    //pickerAccountBank.ItemsSource = l_partnerBank;
            ////    //pickerAccountBank.ItemDisplayBinding = new Binding("display");
            ////    //pickerAccountBank.SelectedIndex = 0;
            ////}
            ////else
            ////{
            ////    ChequeGroup.IsVisible = false;
            ////}

            ObservableCollection<inbound_payment_method> l_inbound = new ObservableCollection<inbound_payment_method>();            
            InboundPaymentMethodDb inboundPaymentMethodDb = new InboundPaymentMethodDb();
            //var ipmItems = inboundPaymentMethodDb.GetItemsByParentAsync(cuentaDiario.id);            
            l_inbound = new ObservableCollection<inbound_payment_method>(await inboundPaymentMethodDb.GetItemsByParentAsync(cuentaDiario.id));
            
            pickerPaymentMethod.ItemsSource = l_inbound;
            pickerPaymentMethod.ItemDisplayBinding = new Binding("name");
            pickerPaymentMethod.SelectedIndex = 0;
            //pickerPaymentMethod.ItemDisplayBinding = 
            
            //switch (account_Journal_Types[selectedIndex].code)
            //{
            //}            
        }
    }

    //private async Task LoadPaymentLinesForEdit()
    //{
    //    AccountPaymentInvoiceLineDb accountPaymentLines = new AccountPaymentInvoiceLineDb();

    //    var apl = accountPaymentLines.GetItemsAsync(accountPayment);

    //    if (apl.Count() > 0)
    //    {
    //        accountPayment.lines = apl.ToArray();

    //        List<FacNotaCreditoDetAuxiliar> facNotaCreditoDetAuxiliars = new List<FacNotaCreditoDetAuxiliar>();
    //        FacNotaCreditoCabDb fncDbCab = new FacNotaCreditoCabDb();
    //        FacNotaCreditoDetDb fncDbDet = new FacNotaCreditoDetDb();
    //        var headersFacNc = (await fncDbCab.GetItemsByClientAsync(accountPaymentHeader.CODEMPRESA, accountPaymentHeader.CODCLIENTE)).ToArray();

    //        foreach (var factdet in accountPayment.lines)
    //        {
    //            FacNotaCreditoDetAuxiliar itemN = new FacNotaCreditoDetAuxiliar();
    //            itemN.line_id = factdet.invoice_line_id;
    //            itemN.ARTICULO = factdet.invoice_line_id_name;
    //            itemN.NUMCMPRVENTA = factdet.parent_payment_id;

    //            var detFacNc = (await fncDbDet.GetItemByLineIdAsync(factdet.invoice_line_id));

    //            if (detFacNc != null)
    //            {
    //                var facParent = headersFacNc.Where(i => i.NUMCMPRVENTA.Equals(detFacNc.NUMCMPRVENTA)).FirstOrDefault();

    //                if (facParent != null)
    //                {
    //                    itemN.NUMDOCUMENTO = facParent.NUMDOCUMENTO;
    //                    itemN.FECHAREGISTRO = facParent.FECHAREGISTRO;
    //                }
    //            }

    //            itemN.reconcile_amount = factdet.reconcile_amount;
    //            facNotaCreditoDetAuxiliars.Add(itemN);
    //        }

    //        facturasItems = facNotaCreditoDetAuxiliars.ToArray();

    //        lblCounter.Text = "Total de documentos " + facturasItems.Length.ToString();
    //        collectionView.ItemsSource = facturasItems.ToArray();

    //    }
    //}

    private async Task<string> GetSellerName(int company_id, int user_id)
    {
        UserDb  resUserDb  = new UserDb();
        var res_User = await resUserDb.GetItemsAsync(company_id, user_id);
        
        if(res_User != null)
            return res_User.partner_name.ToUpper();
        return "";
    }

    private async Task<string> GetSellerNameByInvoice(int invoice_id)
    {
        AccountMoveLineDb accountMoveLineDb = new AccountMoveLineDb();
        var accountMoveLine = await accountMoveLineDb.GetItem(invoice_id);

        if (accountMoveLine != null)
        {
            AccountMoveDb accountMoveDb = new AccountMoveDb();
            var accountMove = await accountMoveDb.GetItem(accountMoveLine.moveId);

            if (accountMove != null)
                return await GetSellerName(accountMove._company_id, accountMove._invoice_user_id);
        }

        return "";
    }

    private async Task LoadPaymentLinesForNew()
    {
        AccountMoveDb accountMoveDb = new AccountMoveDb();
        //var accMovesByCustomer = await accountMoveDb.GetItemsByPartnerForPaymentAsync(_res_partner);

        var accMovesByCustomer = await accountMoveDb.GetItemsByPartnerAndCompany(_res_partner, Sel_Company_Id);

        List<AccountPaymentInvoiceLineAuxiliar> accountPaymentInvoiceLineAuxiliars = new List<AccountPaymentInvoiceLineAuxiliar>();

        decimal totalResidualPayment = 0;
        decimal totalPayment = 0;
        decimal totalApplied = 0;
        //decimal valorSaldoDocumento = 0;
        decimal totalInvoicePayment = 0;

        decimal residualAmount = 0;

        totalPayment = (decimal) ParseTool.StringToDouble(txtValor.Text);

        int lineCounter = 0;
        foreach (var accountMoveItem in accMovesByCustomer)
        {
            lineCounter++;
            AccountPaymentInvoiceLineAuxiliar accountPaymentInvoiceLineAuxiliar = new AccountPaymentInvoiceLineAuxiliar();
            accountPaymentInvoiceLineAuxiliar.parent_payment_id = accountMoveItem.id;

            accountPaymentInvoiceLineAuxiliar.invoice_line_id = 0;

            AccountMoveLineDb accountMoveLineDb = new AccountMoveLineDb();
            var itemMoveLine = await accountMoveLineDb.GetItemsByParentAsync(accountMoveItem.id);
            var itemPaymentTerm = itemMoveLine.Where(x => x.display_type == "payment_term").FirstOrDefault();

            if (itemPaymentTerm != null)
            {
                accountPaymentInvoiceLineAuxiliar.invoice_line_id = itemPaymentTerm.id;
            }
            else
            {
                //Mostrar mensaje de advertencia
                // no se encontró término de pago en factura y es requerido para continuar
                // no se agregará factura a la lista
                continue;
            }

            accountPaymentInvoiceLineAuxiliar.invoice_line_id_name = accountMoveItem.name;
            accountPaymentInvoiceLineAuxiliar.invoice_name = accountMoveItem.name;            
            accountPaymentInvoiceLineAuxiliar.invoice_date = accountMoveItem.invoice_date; 
            accountPaymentInvoiceLineAuxiliar.invoice_date_due = accountMoveItem.invoice_date_due;
            accountPaymentInvoiceLineAuxiliar.invoice_origin = accountMoveItem.invoice_origin;
            accountPaymentInvoiceLineAuxiliar.invoice_amount_total = accountMoveItem.amount_total;
            accountPaymentInvoiceLineAuxiliar.invoice_amount_residual = accountMoveItem.amount_residual;

            residualAmount += accountPaymentInvoiceLineAuxiliar.invoice_amount_residual;

            //////////////////////////////////////////////////////////////

            totalResidualPayment = totalPayment - totalApplied;

            accountPaymentInvoiceLineAuxiliar.seller = await GetSellerName(accountMoveItem._company_id, accountMoveItem._invoice_user_id);
            
            if (totalResidualPayment > 0)
            {
                //valorSaldoDocumento = accountPaymentInvoiceLineAuxiliar.invoice_amount_residual; // (+parseFloat("" + data[i].VALORSALDO.replace(",", "")).toFixed(2) - +parseFloat("" + data[i].VALORCHEQUE.replace(",", "")).toFixed(2)) * 1;
                totalInvoicePayment = (totalResidualPayment < accountPaymentInvoiceLineAuxiliar.invoice_amount_residual) ? totalResidualPayment : accountPaymentInvoiceLineAuxiliar.invoice_amount_residual; 
                //((valorFaltaAplicar < valorSaldoDocumento) ? valorFaltaAplicar : valorSaldoDocumento);
                //docItem.VALORXAPLICAR = "" + valorAplicaDocumento;
                accountPaymentInvoiceLineAuxiliar.reconcile_amount = totalInvoicePayment; 
                totalApplied += totalInvoicePayment;
            }
            else
            {
                accountPaymentInvoiceLineAuxiliar.reconcile_amount = 0;
                //docItem.BLOQUEADO = "N";
            }
            accountPaymentInvoiceLineAuxiliars.Add(accountPaymentInvoiceLineAuxiliar);
        }

        accountPaymentInvoiceLineAuxiliars = accountPaymentInvoiceLineAuxiliars.OrderBy(x => x.invoice_date).ToList();
        accountPaymentLines = accountPaymentInvoiceLineAuxiliars.ToArray();

        lblCounter.Text = "Total de documentos " + accountPaymentLines.Length.ToString();
        lblMonto.Text = "($ " + residualAmount.ToString() + ")";
        collectionView.ItemsSource = accountPaymentLines;
    }

    private async Task LoadDataForEdition()
    {       
        AccountJournalDb accountJournalDb = new AccountJournalDb();
             
        var selDiario = await accountJournalDb.GetItem(accountPayment.journal_id);

        if (selDiario != null)
        {
            var selAccType = account_Journal_Types.Where(x => x.code == selDiario.type).FirstOrDefault();
            if (selAccType != null)
            {   
                LoadingEditionData = true;

                if (account_Journals == null)
                {
                    account_Journals = (await accountJournalDb.GetItemsAsync()).Where(j =>
                        j.CompanyId == Sel_Company_Id.id).ToList();
                    account_Journals = account_Journals.OrderBy(j => j.name).ToList();
                    //Se asigna lista al picker
                    pickerDiario.ItemsSource = account_Journals;
                }
                
                //Ahora desde memoria
                var selDiarioMemory = account_Journals.Where(x => x.id == selDiario.id).FirstOrDefault();
                pickerDiario.SelectedItem = selDiarioMemory;

                //Cargando métodos de pago
                ObservableCollection<inbound_payment_method> l_inbound = new ObservableCollection<inbound_payment_method>();
                InboundPaymentMethodDb inboundPaymentMethodDb = new InboundPaymentMethodDb();                
                l_inbound = new ObservableCollection<inbound_payment_method>(await inboundPaymentMethodDb.GetItemsByParentAsync(accountPayment.journal_id));

                pickerPaymentMethod.ItemsSource = l_inbound;
                var paymentLine = l_inbound.Where(p => p.id == accountPayment.payment_method_line_id).FirstOrDefault();
                
                if (paymentLine == null)
                {
                    LoadingEditionData = false;
                    return;
                }

                pickerPaymentMethod.SelectedItem = paymentLine;

                if (paymentLine.code == "manual")
                {
                    ChequeGroup.IsVisible = false;
                    CreditCardGroup.IsVisible = false;
                }

                if (paymentLine.code == "tc_in")
                {
                    ChequeGroup.IsVisible = false;
                    CreditCardGroup.IsVisible = true;

                    txtReferenciaTc.Text = accountPayment.reference_tc;
                    txtAuthTc.Text = accountPayment.auth_tc;
                    txtLoteTc.Text = accountPayment.lote_tc;
                }

                if (paymentLine.code == "in_third_checks")
                {
                    ChequeGroup.IsVisible = true;
                    CreditCardGroup.IsVisible = false;
                }

                PartnerBankDb partnerBankDb = new PartnerBankDb();
                
                var partnerBankItem = await partnerBankDb.GetItem(accountPayment.partner_bank_id);
                if (partnerBankItem != null)
                {
                    //Busqueda de banco
                    BankDb bankDb = new BankDb();
                    var bankItem = await bankDb.GetItem(partnerBankItem.BankId);

                    txtCuenta.Text = partnerBankItem.acc_number;
                    lblAccountBank.Text = bankItem.name;
                    lblAccountHolder.Text = partnerBankItem.acc_holder_name;
                    lblAccountType.Text = partnerBankItem.type_account;
                    stackAccountInfo.IsVisible = true;

                    _res_partner_bank = partnerBankItem;
                }

                //Se sale del modo de carga de datos para edicion
                LoadingEditionData = false;
            }
        }
                
        txtValor.Text = accountPayment.amount.ToString(App.Session.ApplicationCultureInfo);
        pickerFecCobro.Date = accountPayment.date;
        txtRef.Text = accountPayment._ref;
        txtNCheque.Text = accountPayment.number_check_customer;
    }

    private async Task LoadDataForEdition_deprecated()
    {
        //pickerTipoDiario.SelectedIndex = 0;
        //pickerDiario.SelectedIndex = 0;
        AccountJournalDb accountJournalDb = new AccountJournalDb();

        //Desde la base
        //var selDiario = account_Journals.Where(x => x.id == accountPayment.journal_id).FirstOrDefault();
        var selDiario = await accountJournalDb.GetItem(accountPayment.journal_id);

        if (selDiario != null)
        {
            var selAccType = account_Journal_Types.Where(x => x.code == selDiario.type).FirstOrDefault();
            if (selAccType != null)
            {
                LoadingEditionData = true;

                //Carga de arreglo de memoria
                //pickerTipoDiario.SelectedItem = selAccType;
                //Debug.WriteLine(account_Journals.Count());

                //Se carga de la base
                //account_Journals = (await accountJournalDb.GetItemsAsync()).Where(
                //            j => j.type == selAccType.code &&
                //            j.CompanyId == _res_partner.company_id).ToList();

                if (account_Journals == null)
                {
                    account_Journals = (await accountJournalDb.GetItemsAsync()).Where(j =>
                        j.CompanyId == Sel_Company_Id.id).ToList();
                    account_Journals = account_Journals.OrderBy(j => j.name).ToList();
                    //Se asigna lista al picker
                    pickerDiario.ItemsSource = account_Journals;
                }

                //Ahora desde memoria
                var selDiarioMemory = account_Journals.Where(x => x.id == selDiario.id).FirstOrDefault();
                pickerDiario.SelectedItem = selDiarioMemory;

                if (selDiarioMemory.code == "DRV1" || selDiarioMemory.code == "CCLI")
                {
                    ChequeGroup.IsVisible = true;
                    CreditCardGroup.IsVisible = false;
                }

                if (selDiarioMemory.credit_card) // || selDiarioMemory.code.Contains("CCD"))
                {
                    ChequeGroup.IsVisible = false;
                    CreditCardGroup.IsVisible = true;

                    txtReferenciaTc.Text = accountPayment.reference_tc;
                    txtAuthTc.Text = accountPayment.auth_tc;
                    txtLoteTc.Text = accountPayment.lote_tc;
                }

                //Cargando metodos de pago
                ObservableCollection<inbound_payment_method> l_inbound = new ObservableCollection<inbound_payment_method>();
                InboundPaymentMethodDb inboundPaymentMethodDb = new InboundPaymentMethodDb();
                l_inbound = new ObservableCollection<inbound_payment_method>(await inboundPaymentMethodDb.GetItemsByParentAsync(accountPayment.journal_id));

                pickerPaymentMethod.ItemsSource = l_inbound;
                var paymentLine = l_inbound.Where(p => p.id == accountPayment.payment_method_line_id).FirstOrDefault();
                pickerPaymentMethod.SelectedItem = paymentLine;

                PartnerBankDb partnerBankDb = new PartnerBankDb();
                //ObservableCollection<res_partner_bank> l_partnerBank = new ObservableCollection<res_partner_bank>();
                //l_partnerBank = new ObservableCollection<res_partner_bank>(await partnerBankDb.GetItemsAsync());

                var partnerBankItem = await partnerBankDb.GetItem(accountPayment.partner_bank_id);
                if (partnerBankItem != null)
                {
                    //Busqueda de banco
                    BankDb bankDb = new BankDb();
                    var bankItem = await bankDb.GetItem(partnerBankItem.BankId);

                    txtCuenta.Text = partnerBankItem.acc_number;
                    lblAccountBank.Text = bankItem.name;
                    lblAccountHolder.Text = partnerBankItem.acc_holder_name;
                    lblAccountType.Text = partnerBankItem.type_account;
                    stackAccountInfo.IsVisible = true;

                    _res_partner_bank = partnerBankItem;
                }

                //Se sale del modo de carga de datos para edicion
                LoadingEditionData = false;
            }
        }

        //Edición
        txtValor.Text = accountPayment.amount.ToString(App.Session.ApplicationCultureInfo);

        //txtNCtaCheque.Text = accountPayment.cta_cheque;
        //txtGirador.Text = accountPayment.emisor;
        //pickerIndicador.SelectedItem = indicador.Where(x => x.CODPARAMETRO == cobReciboDet.idindicador).FirstOrDefault();

        pickerFecCobro.Date = accountPayment.date;

        //pickerTipCheque.SelectedItem = tipoCheque.Where(x => x.CODPARAMETRO == cobReciboDet.tipocheque).FirstOrDefault();
        //pickerTarjetas.SelectedItem = tarjetas.Where(x => x.codigo == cobReciboDet.idtarjeta).FirstOrDefault();

        txtRef.Text = accountPayment._ref;
        txtNCheque.Text = accountPayment.number_check_customer;
        //txtNDeposito.Text = accountPayment.numerodeposito;
        //txtNCheque.Text = accountPayment.numero_cheque;
        //txtNLote.Text = accountPayment.numero_lote;
        //txtNTarjeta.Text = accountPayment.numero_tarjeta;
    }

    private async Task LoadPaymentLines()
    {
        if(accountPayment!=null)
        {
            if (accountPayment.lines == null)
            {
                return;
            }
        }
        else
        {
            return;
        }

        List<AccountPaymentInvoiceLineAuxiliar> accountPaymentLinesMem = new List<AccountPaymentInvoiceLineAuxiliar>();

        foreach (var line in accountPayment.lines)
        {            
            AccountPaymentInvoiceLineAuxiliar itemN = new AccountPaymentInvoiceLineAuxiliar();
            itemN.id = line.id;
            itemN.parent_payment_id = line.parent_payment_id;
            itemN.invoice_name = line.invoice_line_id_name;
            itemN.reconcile_amount = line.reconcile_amount;
            itemN.invoice_line_id = line.invoice_line_id;
            itemN.invoice_line_id_name = line.invoice_line_id_name;
            itemN.invoice_date = line.invoice_date;
            itemN.amount_residual = line.amount_residual;
            itemN.invoice_amount_residual = line.amount_residual;
            itemN.seller = "----";
            itemN.seller = await GetSellerNameByInvoice(line.invoice_line_id);
            accountPaymentLinesMem.Add(itemN);
        }

        accountPaymentLinesMem = accountPaymentLinesMem.OrderBy(x => x.invoice_date).ToList();

        accountPaymentLines = accountPaymentLinesMem.ToArray();

        lblCounter.Text = "Total de documentos " + accountPaymentLines.Length.ToString();
        collectionView.ItemsSource = accountPaymentLines;
    }

    void HideFields()
    {
        //pickerCuentas.IsVisible = false;
        //pickerBancos.IsVisible = false;
        //pickerTarjetas.IsVisible = false;
        //pickerTipCheque.IsVisible = false;
        pickerFecCobro.IsVisible = false;
        //pickerIndicador.IsVisible = false;
        txtGirador.IsVisible = false;
        txtNCtaCheque.IsVisible = false;
        txtNCheque.IsVisible = false;
        txtNDeposito.IsVisible = false;
        pickerFecDeposito.IsVisible = false;
        txtNTarjeta.IsVisible = false;
        txtNLote.IsVisible = false;
    }

    async void OnPickerTipoDiarioSelectedIndexChanged(object sender, EventArgs e)
    {
        if (LoadingEditionData)
            return;

            //TODO: Verificar casos en los que _cobCarteraCab pasa con NULL
            var picker = (Picker) sender;
        int selectedIndex = picker.SelectedIndex;

        if (selectedIndex != -1)
        {
            switch (account_Journal_Types[selectedIndex].code)
            {
                case "bank":
                    {
                        AccountJournalDb db = new AccountJournalDb();
                        account_Journals = (await db.GetItemsAsync()).Where(
                            j=>j.type == account_Journal_Types[selectedIndex].code &&
                            j.CompanyId == Sel_Company_Id.id).ToList();

                        pickerDiario.ItemsSource = account_Journals;
                        pickerDiario.ItemDisplayBinding = new Binding("name");
                        pickerDiario.SelectedIndex = 0;
                    }
                    break;
                case "cash":
                    {
                        AccountJournalDb db = new AccountJournalDb();
                        account_Journals = (await db.GetItemsAsync()).Where(
                            j => j.type == account_Journal_Types[selectedIndex].code &&
                            j.CompanyId == Sel_Company_Id.id).ToList();

                        pickerDiario.ItemsSource = account_Journals;
                        pickerDiario.ItemDisplayBinding = new Binding("name");
                        pickerDiario.SelectedIndex = 0;
                    }
                    break;
            }

            //monkeyNameLabel.Text = picker.Items[selectedIndex];
            ////Debug.WriteLine( picker.Items[selectedIndex] );
            ////Debug.WriteLine(formaspagos[selectedIndex].codigo);
            ////Debug.WriteLine(formaspagos[selectedIndex].descripcion);
            ////selFormaspagos = formaspagos[selectedIndex];
            ////picker.SelectedItem = selFormaspagos;

            ////switch (formaspagos[selectedIndex].codigo)
            ////{
            ////    case "EF":
            ////        {
            ////            HideFields();
            ////        }
            ////        break;
            ////    case "CH":
            ////        {
            ////            HideFields();
            ////            pickerTipCheque.IsVisible = true;
            ////            pickerFecCobro.IsVisible = true;
            ////            pickerIndicador.IsVisible = true;
            ////            pickerBancos.IsVisible = true;
            ////            txtGirador.IsVisible = true;
            ////            txtNCtaCheque.IsVisible = true;
            ////            txtNCheque.IsVisible = true;
            ////        }
            ////        break;
            ////    case "DP":
            ////        {
            ////            HideFields();
            ////            pickerCuentas.IsVisible = true;
            ////            txtNDeposito.IsVisible = true;
            ////            pickerFecDeposito.IsVisible= true;
            ////        }
            ////        break;
            ////    case "TJ":
            ////        {
            ////            HideFields();
            ////            pickerBancos.IsVisible = true;
            ////            pickerTarjetas.IsVisible = true;
            ////            txtNTarjeta.IsVisible = true;
            ////            txtNLote.IsVisible = true;                        
            ////        }
            ////        break;
            ////    case "TRANBAN":
            ////        {
            ////            HideFields();
            ////            pickerCuentas.IsVisible= true;
            ////            txtNDeposito.IsVisible = true;
            ////            pickerFecDeposito.IsVisible = true;
            ////        }
            ////        break;
            ////}            
        }
    }

    private async void btnLoadDocs_Clicked(object sender, EventArgs e)
    {
        await LoadPaymentLinesForNew();
    }

    private async void btnNewAccountBank_Clicked(object sender, EventArgs e)
    {
        CommunityToolkit.Maui.Sample.Models.PopupSizeConstants popupSizeConstants = new CommunityToolkit.Maui.Sample.Models.PopupSizeConstants(DeviceDisplay.Current);

        var returnResultPopup = new PopupResPartnerBank(popupSizeConstants);
        returnResultPopup.partner = _res_partner;

        //Evita que se cierre cuando se haga clic (tap) fuera de la ventana
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        //if (!isWindows)
        //    returnResultPopup.Size = this.popupSizeConstants.Large;

        var result = await this.ShowPopupAsync(returnResultPopup);

        if (result != null)
        {
            PartnerBankDb partnerBankDb = new PartnerBankDb();
            var new_partnerBank = (res_partner_bank)result;
            
            _res_partner_bank = new_partnerBank;

            new_partnerBank.id = await partnerBankDb.getNewId();

            await partnerBankDb.InsertAsync(new_partnerBank);
            
            txtCuenta.Text = new_partnerBank.acc_number;
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

        var result = await PopupExtensions.ShowPopupAsync(this, returnResultPopup);
        
        if (result != null)
        {
            res_partner_bank Sel_Res_PartnerBank = (res_partner_bank) result;
            _res_partner_bank = Sel_Res_PartnerBank;

            txtCuenta.Text = Sel_Res_PartnerBank.acc_number;
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

        if (!double.TryParse(txtValor.Text, out totalPagadoDbl) || totalPagadoDbl == 0)
        {
            await Toast.Make("No se han ingresado valores correctos, no se puede guardar.").Show();
            return;
        }

        if (accountPayment == null)
        {
            accountPayment = new AccountPayment();
            accountPayment.parent_id = 0;
        }

        string journalCode = "";
        account_journal accountJournal = null;

        if (((account_journal)pickerDiario.SelectedItem) != null)
        {
            accountJournal = ((account_journal)pickerDiario.SelectedItem);
            journalCode = accountJournal.code;
        }        

        accountPayment.company_id = Sel_Company_Id.id;

        accountPayment._ref = txtRef.Text;
        accountPayment.partner_id = _res_partner.id;
        accountPayment.journal_id = ((account_journal)pickerDiario.SelectedItem).id;
        accountPayment.journal_name = ((account_journal)pickerDiario.SelectedItem).name;
        accountPayment.partner_type = "customer";
        accountPayment.payment_type = "inbound";
        accountPayment.date = pickerFecCobro.Date;

        inbound_payment_method selPaymentM = null;
        accountPayment.payment_method_line_id = 0;
        accountPayment.number_check_customer = txtNCheque.Text;
        accountPayment.date_release = pickerFecDeposito.Date;

        if (pickerPaymentMethod.SelectedItem != null)
        {
            selPaymentM = (inbound_payment_method)pickerPaymentMethod.SelectedItem;
            accountPayment.payment_method_line_id = selPaymentM.id;

            if (selPaymentM.code == "tc_in")
            {                
                if (txtReferenciaTc.Text != null && txtReferenciaTc.Text.Trim() != "" &&
                    txtAuthTc.Text != null && txtAuthTc.Text.Trim() != "" &&
                    txtLoteTc.Text != null && txtLoteTc.Text.Trim() != "")
                {
                    accountPayment.reference_tc = txtReferenciaTc.Text;
                    accountPayment.auth_tc = txtAuthTc.Text;
                    accountPayment.lote_tc = txtLoteTc.Text;
                }
                else
                {
                    await Toast.Make("Los pagos con tarjeta de crédito requieren Referencia, Autorización y Lote.").Show();
                    return;
                }
            }

            if(selPaymentM.code == "in_third_checks")
            {
                if (_res_partner_bank != null)
                {
                    accountPayment.partner_bank_id = _res_partner_bank.id;
                    accountPayment.bank_account_id = _res_partner_bank.id;
                }
                else
                {
                    await Toast.Make("Debe seleccionar una cuenta bancaria antes de continuar.").Show();
                    return;
                }
            }
        }       

        //accountPayment.numero_retencion = "0";
        txtValor.Text = ParseTool.StringValueFix(txtValor.Text);
        accountPayment.amount = (decimal)ParseTool.StringToDouble(txtValor.Text); //.ToString(App.Session.ApplicationCultureInfo);

        List<AccountPaymentInvoiceLine> linesL = new List<AccountPaymentInvoiceLine>();
        if (accountPaymentLines != null)
        {
            foreach (var item in accountPaymentLines)
            {
                if (item.reconcile_amount > 0)
                {
                    AccountPaymentInvoiceLine accountPaymentInvoiceLine = new AccountPaymentInvoiceLine();
                    accountPaymentInvoiceLine.reconcile_amount = item.reconcile_amount;
                    accountPaymentInvoiceLine.invoice_line_id = item.invoice_line_id;
                    accountPaymentInvoiceLine.invoice_line_id_name = item.invoice_line_id_name;
                    accountPaymentInvoiceLine.invoice_date = item.invoice_date;
                    accountPaymentInvoiceLine.payment_state = "draft";
                    linesL.Add(accountPaymentInvoiceLine);
                }
            }
        }

        accountPayment.lines = linesL.ToArray();

        Debug.WriteLine("Guardar Datos");
        saveData = true;
        await Navigation.PopModalAsync();
    }

    private async void btnSave_Clicked_deprecated(object sender, EventArgs e)
    {
        //var cmdX = new AsyncRelayCommand(() => Toast.Make("Toast in a Modal Page").Show());
        //if (cmdX.CanExecute(this))
        //{
        //    await cmdX.ExecuteAsync(this);
        //}
        ////cmdX.
        ////await toast.Show(cancellationTokenSource.Token);

        //await Toast.Make("Toast in a Modal Page L2").Show();

        double totalPagadoDbl = 0;

        if (!double.TryParse(txtValor.Text, out totalPagadoDbl) || totalPagadoDbl == 0)
        {
            await Toast.Make("No se han ingresado valores correctos, no se puede guardar.").Show();
            return;
        }

        if (accountPayment == null)
        {
            accountPayment = new AccountPayment();
            accountPayment.parent_id = 0;
        }

        string journalCode = "";
        account_journal accountJournal = null;

        if (((account_journal)pickerDiario.SelectedItem) != null)
        {
            accountJournal = ((account_journal)pickerDiario.SelectedItem);
            journalCode = accountJournal.code;
        }

        //cobReciboDet.idbanco = ((st_bancos)pickerBancos.SelectedItem).codigo;
        //cobReciboDet.descbanco = ((st_bancos)pickerBancos.SelectedItem).descripcion;
        //cobReciboDet.idformapago = ((st_formaspago)pickerFPago.SelectedItem).codigo;
        //cobReciboDet.descformapago = ((st_formaspago)pickerFPago.SelectedItem).descripcion;

        //TODO: Arrastrar hasta acá el código de la compañía
        
        //accountPayment.company_id = _res_partner.company_id;

        accountPayment.company_id = Sel_Company_Id.id;

        accountPayment._ref = txtRef.Text;
        accountPayment.partner_id = _res_partner.id;
        accountPayment.journal_id = ((account_journal)pickerDiario.SelectedItem).id;
        accountPayment.journal_name = ((account_journal)pickerDiario.SelectedItem).name;
        accountPayment.partner_type = "customer";
        accountPayment.payment_type = "inbound";
        accountPayment.date = pickerFecCobro.Date;
        
        //Si se ha seleccionado banco
        if (journalCode == "CCLI")
        {
            if (_res_partner_bank != null)
            {
                accountPayment.partner_bank_id = _res_partner_bank.id;
                accountPayment.bank_account_id = _res_partner_bank.id;
            }
            else
            {
                await Toast.Make("Debe seleccionar una cuenta bancaria antes de continuar.").Show();
                return;
            }
        }

        if (accountJournal.credit_card)
        {
            //if (journalCode == "CCD")
            if(txtReferenciaTc.Text != null && txtReferenciaTc.Text.Trim() != "" &&
                txtAuthTc.Text != null && txtAuthTc.Text.Trim() != "" &&
                txtLoteTc.Text != null &&  txtLoteTc.Text.Trim() != "")
            {
                accountPayment.reference_tc = txtReferenciaTc.Text;
                accountPayment.auth_tc = txtAuthTc.Text;
                accountPayment.lote_tc = txtLoteTc.Text;
            }
            else
            {
                await Toast.Make("Los pagos con tarjeta de crédito requieren Referencia, Autorización y Lote.").Show();
                return;
            }
        }

        accountPayment.payment_method_line_id = 0;
        accountPayment.number_check_customer = txtNCheque.Text;
        accountPayment.date_release = pickerFecDeposito.Date;

        if(pickerPaymentMethod.SelectedItem != null)
        {
            var selPaymentM = (inbound_payment_method)pickerPaymentMethod.SelectedItem;
            accountPayment.payment_method_line_id = selPaymentM.id;
        }

        //ObservableCollection<inbound_payment_method> l_inbound = new ObservableCollection<inbound_payment_method>();
        //InboundPaymentMethodDb inboundPaymentMethodDb = new InboundPaymentMethodDb();
        ////var ipmItems = inboundPaymentMethodDb.GetItemsByParentAsync(cuentaDiario.id);            
        //l_inbound = new ObservableCollection<inbound_payment_method>(inboundPaymentMethodDb.GetItemsByParentAsync(cuentaDiario.id));

        //pickerPaymentMethod.ItemsSource = l_inbound;


        //accountPayment.cta_cheque = txtNCtaCheque.Text;
        //accountPayment.emisor = txtGirador.Text;

        //accountPayment.idindicador = ((Parametros) pickerIndicador.SelectedItem).CODPARAMETRO;
        //accountPayment.descindicador = ((Parametros)pickerIndicador.SelectedItem).VALOR;

        //accountPayment.fcobrocheque = pickerFecCobro.Date.ToString() ;
        //cobReciboDet.tipocheque = ((Parametros)pickerTipCheque.SelectedItem).CODPARAMETRO;         

        //cobReciboDet.idctacia = ((st_cuentas)pickerCuentas.SelectedItem).codigo;
        //cobReciboDet.descctacia = ((st_cuentas)pickerCuentas.SelectedItem).descripcion;

        //cobReciboDet.idtarjeta = ((st_tarjetas)pickerTarjetas.SelectedItem).codigo;
        //cobReciboDet.desctarjeta = ((st_tarjetas)pickerTarjetas.SelectedItem).descripcion;
        //accountPayment.numerodeposito = txtNDeposito.Text;
        //accountPayment.numero_cheque = txtNCheque.Text;
        //accountPayment.numero_lote = txtNLote.Text;        
        //accountPayment.numero_tarjeta = txtNTarjeta.Text;

        //accountPayment.numero_retencion = "0";
        txtValor.Text = ParseTool.StringValueFix(txtValor.Text);
        accountPayment.amount = (decimal) ParseTool.StringToDouble(txtValor.Text); //.ToString(App.Session.ApplicationCultureInfo);

        List<AccountPaymentInvoiceLine> linesL = new List<AccountPaymentInvoiceLine>();
        if (accountPaymentLines != null)
        {
            foreach (var item in accountPaymentLines)
            {
                if (item.reconcile_amount > 0)
                {
                    AccountPaymentInvoiceLine accountPaymentInvoiceLine = new AccountPaymentInvoiceLine();
                    accountPaymentInvoiceLine.reconcile_amount = item.reconcile_amount;
                    accountPaymentInvoiceLine.invoice_line_id = item.invoice_line_id;
                    accountPaymentInvoiceLine.invoice_line_id_name = item.invoice_line_id_name;
                    accountPaymentInvoiceLine.invoice_date = item.invoice_date;
                    accountPaymentInvoiceLine.payment_state = "draft";
                    linesL.Add(accountPaymentInvoiceLine);
                }
            }
        }

        accountPayment.lines = linesL.ToArray();

        Debug.WriteLine("Guardar Datos");
        saveData = true;
        await Navigation.PopModalAsync();
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
            await btnTopTools.RotateTo(90, 200); // Rotar 360 grados en 1000 milisegundos
            btnTopTools.Rotation = 0; // Restablecer la rotación después de la animación
            fntSrc.Glyph = "\uf00d";
            TopTools.IsVisible = true;
        }
        else
        {
            // Animación de rotación
            await btnTopTools.RotateTo(-90, 200); // Rotar 360 grados en 1000 milisegundos
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