
using CobranzasDMSA_Odoo;
using CobranzasDMSA_Odoo.Controls;
using CobranzasDMSA_Odoo.Models;
using CobranzasDMSA_Odoo.Settings.helpers;
using CobranzasDMSA_Odoo.Settings.Sqlite;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Sample.Models;
using CommunityToolkit.Maui.Sample.ViewModels.Views;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Sample;

using DMSA.Models.MovilCobranzas.Api;
using DMSA.Models.Odoo.Native;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace CobranzasDMSA_Odoo.AppPages.NotaCredito;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AccountMoveSendCrud : ContentPage
{
    public bool isNewData { get; set; } = false;
    public int itemIndex { get; set; } = -1;
    public bool saveData { get; set; } = false;

    private account_journal credit_note_journal { get; set; }
    private account_journal[] credit_note_journals { get; set; }

    private res_company default_empresa { get; set; }
    public res_company[] Empresas { get; set; }
    //Se utiliza par la busqueda del cliente
    public res_partner _res_partner { get; set; }
    public AccountModule[] accountModules { get; set; }
    public AccountTypeModule[] accountTypeModules { get; set; }
    public bool isWindows { get; set; } = false;
    private bool editionMode { get; set; } = false;
    private string TypeSearch { get; set; }
    public AccountMoveSendHeader accountMoveSendHeader { get; set; }
    public account_move_send accountMoveSend { get; set; }
    private account_move_line_send[] _account_move_lines_send_items { get; set; }
    //Factura seleccionada
    public account_move _accountMoveSelected { get; set; }
    readonly PopupSizeConstants popupSizeConstants;
	readonly CsharpBindingPopupViewModel csharpBindingPopupViewModel;
    
	public AccountMoveSendCrud(res_partner _res_partner_param)
	{
        _res_partner = _res_partner_param;

		InitializeComponent();

		if(popupSizeConstants == null)
		{
			this.popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        }
		else
		{
            this.popupSizeConstants = popupSizeConstants;
        }
		
		this.csharpBindingPopupViewModel = csharpBindingPopupViewModel;
        isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;
        //isWindows = false;

        DeleteCommand = new Command(DeleteItem);
        ReturnItemCommand = new Command(ReturnItem);

        TypeSearch = "item";

        IDispatcherTimer timer;

        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.IsRepeating = false;
        timer.Tick += async (s, e) =>
        {
            //SelectorCmp.SelectedItem = empresa;
            await PrepareForm();
            //if(editionMode)
            //{
            //    await _LoadSolicitudNC();
            //}
            timer.Stop();
        };
        timer.Start();

        txtObs.Text = "";

        //btnStatus.IsEnabled
        //SetBinding();
        BindingContext = this;
    }

    protected override bool OnBackButtonPressed()
    {
        var tcs = new TaskCompletionSource<bool>();

        Dispatcher.Dispatch(async () =>
        {
            var leave = await DisplayAlert("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");

            if (leave)
            {                
                base.OnBackButtonPressed();                
            }
        });

        return true;
        //return base.OnBackButtonPressed();
    }

    public void SetEditionMode(account_move_send _account_move_send)
    {
        editionMode = true;
        accountMoveSend = _account_move_send;
        //tabFormasPago.editionMode = editionMode;
        //tabDocumentos.editionMode = editionMode;
    }
        
    public ICommand DeleteCommand { get; set; }

    private void DeleteItem(object obj)
    {
        Debug.WriteLine("DeleteItem");
    }

    public ICommand ReturnItemCommand { get; set; }

    private async void ReturnItem(object obj)
    {
        Debug.WriteLine("ReturnItem");

        account_move_line_send account_ml_item = (account_move_line_send) obj;
        string newValue = await DisplayPromptAsync("Cant. Devolver", "Ingrese cantidad que se desea devolver", "APLICAR", "CERRAR", account_ml_item.quantity.ToString(), 10, Keyboard.Numeric); //, cobCarteraDet.VALORXAPLICAR);

        if (newValue != null && newValue != "")
        {
            //Operaciones de conversión
            account_ml_item.quantity = (decimal) ParseTool.StringToDouble(newValue);
            //facNotaCreditoDetAuxiliar.BLOQUEADO = "S";

            //var _items = facNotaCreditoDetAuxiliar;

            var nList = _account_move_lines_send_items.ToList();
            //int indexToReplace = nList.FindIndex(item => item.NUMDOCUMENTO == account_ml_item.NUMDOCUMENTO && 
            //item.ARTICULO == account_ml_item.ARTICULO &&
            //item.NUMCMPRVENTADET == account_ml_item.NUMCMPRVENTADET);

            int indexToReplace = nList.FindIndex(item =>
            item.account_id == account_ml_item.account_id &&
            item.sequence == account_ml_item.sequence);

            // Verificar si se encontró el objeto a reemplazar
            if (indexToReplace != -1)
            {
                //Reemplazar el objeto existente con el nuevo objeto
                _account_move_lines_send_items[indexToReplace] = account_ml_item;
                Debug.WriteLine("Modificado...");

                //Se fuerza la actualización del visor creando una nueva instancia del arreglo
                var tmpDI = _account_move_lines_send_items.ToList();
                _account_move_lines_send_items = tmpDI.ToArray();

                collectionView.ItemsSource = _account_move_lines_send_items;                
            }
        }
    }

    private async Task ReturnAllItem(bool ReturnAll)
    {
        Debug.WriteLine("ReturnAllItem");
        if(_account_move_lines_send_items == null)
        {
            return;
        }

        foreach(var _line_item in _account_move_lines_send_items)
        {
            //Operaciones de conversión
            if(ReturnAll)
            {
                _line_item.quantity = _line_item.quantity;
            }
            else
            {
                _line_item.quantity = 0;
            }
            
            //_line_item.BLOQUEADO = "S";

            //var _items = facNotaCreditoDetAuxiliar;

            //var nList = facNotaCreditoDets.ToList();
            //int indexToReplace = nList.FindIndex(item => item.NUMDOCUMENTO == facNotaCreditoDetAuxiliar.NUMDOCUMENTO &&
            //item.ARTICULO == facNotaCreditoDetAuxiliar.ARTICULO &&
            //item.NUMCMPRVENTADET == facNotaCreditoDetAuxiliar.NUMCMPRVENTADET);

            //// Verificar si se encontró el objeto a reemplazar
            //if (indexToReplace != -1)
            //{
            //    //Reemplazar el objeto existente con el nuevo objeto
            //    facNotaCreditoDets[indexToReplace] = facNotaCreditoDetAuxiliar;
            //    Debug.WriteLine("Modificado...");

            //    //Se fuerza la actualización del visor creando una nueva instancia del arreglo
            //    var tmpDI = facNotaCreditoDets.ToList();
            //    facNotaCreditoDets = tmpDI.ToArray();

            //    collectionView.ItemsSource = facNotaCreditoDets;
            //}
        }

        var tmpDI = _account_move_lines_send_items.ToList();
        _account_move_lines_send_items = tmpDI.ToArray();
        collectionView.ItemsSource = _account_move_lines_send_items;
    }

    async void HandleReturnResultPopupButtonClicked(object sender, EventArgs e)
    {
        var empresa = (res_company) SelectorCmp.SelectedItem;
        
        var resultPopupSelectInvoice = new PopupSelectPartner(popupSizeConstants);
        resultPopupSelectInvoice.Company = empresa;
        resultPopupSelectInvoice.DetailMode = 1;

        resultPopupSelectInvoice.CanBeDismissedByTappingOutsideOfPopup = false;

        var result = await this.ShowPopupAsync(resultPopupSelectInvoice);
        if (result != null)
        {
            var resPartner = (res_partner) result;
            txtCliente.Text = resPartner.id.ToString() + " - " + resPartner.name;
            _res_partner = resPartner;
        }

        //await DisplayAlert("Pop Result Returned", $"Result: {result}", "OK");
    }

    async void PopupSearchInvoiceButtonClicked(object sender, EventArgs e)
    {
        if (_res_partner == null)
        {
            await Toast.Make("Debe seleccionar un cliente.").Show();
            return;
        }

        var returnResultPopup = new PopupSelectInvoice(popupSizeConstants);
        var empresa = (res_company) SelectorCmp.SelectedItem;
        returnResultPopup.Company = empresa;
        returnResultPopup.partner = _res_partner;

        //Evita que se cierre cuando se haga clic (tap) fuera de la ventana
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        if (!isWindows)
            returnResultPopup.Size = this.popupSizeConstants.Large;

        var resultPopupSelectInvoice = new PopupSelectInvoice(popupSizeConstants);
        resultPopupSelectInvoice.Company = empresa;
        resultPopupSelectInvoice.partner = _res_partner;
        var result = await this.ShowPopupAsync(resultPopupSelectInvoice);
        if (result != null)
        {
            _accountMoveSelected = (account_move)result;
            //txtFactura.Text = _res_partnerItem.name;
            txtFactura.Text = _accountMoveSelected.name;
            //Cargar documentos de factura

            LblInvoiceDate.Text = _accountMoveSelected.invoice_date.ToString("yyyy-MM-dd");
            LblJournal.Text = "DIARIO:" + _accountMoveSelected._journal_id.ToString();
            LblSaleShop.Text = "ESTAB:" + _accountMoveSelected._printer_id.ToString();
        }

        //await DisplayAlert("Pop Result Returned", $"Result: {result}", "OK");
    }

    public async Task SetDefaultData(res_company empresa)
    {
        default_empresa = empresa;
    }

	async Task PrepareForm()
	{
        if (App.Session.CurrentUser.empresas != null)
        {
            Debug.WriteLine("Empresas:");
            Debug.WriteLine(App.Session.CurrentUser.empresas.Length);
            Empresas = App.Session.CurrentUser.empresas;
            SelectorCmp.ItemsSource = Empresas;
            SelectorCmp.ItemDisplayBinding = new Binding(nameof(res_company.name));
            SelectorCmp.SelectedIndex = 0;

            if (default_empresa != null)
            {                
                SelectorCmp.SelectedItem = Empresas.FirstOrDefault(x => x.id == default_empresa.id);
                SelectorCmp.IsEnabled = false;
            }
        }

        //TODO: NO ELIMINAR ESTE GRUPO DE COMENTARIO
        var database_journals = new AccountJournalDb();
        credit_note_journal = (await database_journals.GetItemsAsync()).Where(
            x => x.credit_note && 
            x.CompanyId == default_empresa.id).FirstOrDefault();

        if(credit_note_journal == null)
        {
            Toast.Make("Se requieren un diarios para notas de crédito.");
            return;
        }
        else
        {
            Toast.Make("Diario para notas de crédito " + credit_note_journal.id + "-" +  credit_note_journal.name); 
        }

        //credit_note_journals = (await database_journals.GetItemsAsync()).Where(x=>x.credit_note).ToArray();

        //if (credit_note_journals.Length == 0)
        //{
        //    Toast.Make("Se requieren diarios para notas de crédito.");
        //    return;
        //}

        //pickerCNJournal.ItemsSource = credit_note_journals;
        //pickerCNJournal.ItemDisplayBinding = new Binding(nameof(account_journal.name));
        //pickerCNJournal.SelectedIndex = 0;

        var database = new AccountModuleDb();
        accountModules = (await database.GetItemsAsync()).ToArray();

        if (accountModules.Length == 0)
        {
            return;
        }

        pickerModulos.ItemsSource = accountModules;
        pickerModulos.ItemDisplayBinding = new Binding(nameof(AccountModule.name));
        Debug.WriteLine("Módulos cargados!");

        if (accountMoveSend != null)
        {
            if (_res_partner != null)
            {
                lblTitle.Text = "EDICIÓN - " + _res_partner.name;

                //await LoadForEdition();
                //AccountMoveDb accountMoveDb = new AccountMoveDb();
                //_accountMoveSelected = await accountMoveDb.GetByNameItem(accountMoveSend._ref);
                txtFactura.Text = _accountMoveSelected.name;
                txtObs.Text = accountMoveSend.payment_reference;

                LblInvoiceDate.Text = _accountMoveSelected.invoice_date.ToString("yyyy-MM-dd");
                LblJournal.Text = "DIARIO:" + _accountMoveSelected._journal_id.ToString();
                LblSaleShop.Text = "ESTAB:" + _accountMoveSelected._printer_id.ToString();

                AccountModule selected_module = null;
                AccountTypeModule selected_type_module = null;
                
                selected_module = accountModules.Where(x => x.id == accountMoveSend.module_id).FirstOrDefault();
                pickerModulos.SelectedItem = selected_module;

                var database_type = new AccountTypeModuleDb();
                accountTypeModules = (await database_type.GetItemsAsync()).Where(x => x._module_id == selected_module.id).ToArray();

                if (accountTypeModules.Length == 0)
                {
                    //return;
                }
                
                pickerTipoNc.ItemsSource = accountTypeModules;
                pickerTipoNc.ItemDisplayBinding = new Binding(nameof(AccountTypeModule.name));
                pickerTipoNc.SelectedIndex = 0;

                selected_type_module = accountTypeModules.Where(x => x.id == accountMoveSend.type_module_id).FirstOrDefault();
                pickerTipoNc.SelectedItem = selected_type_module;

                SelectorCmp.IsEnabled = false;
            }

            await LoadAccountMoveSendLines();
        }
        else
        {
            if (_res_partner != null)
            {
                lblTitle.Text = "NUEVO - " + _res_partner.name;
            }

            await LoadAccountMoveSendLines();
        }
    }

    private async Task LoadAccountMoveSendLines()
    {
        if (accountMoveSend != null)
        {
            if (accountMoveSend.lines == null)
            {
                return;
            }
        }
        else
        {
            return;
        }

        //collectionView.ItemsSource = laccountmoveLines;
        //_account_move_lines_send_items = result_send.ToArray();

        //TODO: Cargar detalles
        List<account_move_line_send> accountMoveSendLinesMem = new List<account_move_line_send>();

        foreach (var line in accountMoveSend.lines)
        {
            account_move_line_send itemN = new account_move_line_send();
            itemN.line_id = line.line_id;
            itemN.parent_move_id = line.parent_move_id;
            itemN.name = line.name;
            itemN.account_id = line.account_id;
            itemN.move_id = line.move_id;
            itemN.currency_id = line.currency_id;
            itemN.price_unit = line.price_unit;

            itemN.price_total = line.price_total;
            itemN.quantity = line.quantity;
            itemN.product_id = line.product_id;
            itemN.sequence = line.sequence;
            itemN.original_quantity = line.original_quantity;
            
            accountMoveSendLinesMem.Add(itemN);
        }

        _account_move_lines_send_items = accountMoveSendLinesMem.ToArray();

        //lblCounter.Text = "Total de documentos " + _account_move_lines_send_items.Length.ToString();
        collectionView.ItemsSource = _account_move_lines_send_items;
    }

    private async void pickerModulos_SelectedIndexChanged(object sender, EventArgs e)
    {
        Debug.WriteLine("pickerModulos");
        Debug.WriteLine(pickerModulos.SelectedIndex);

        if(pickerModulos.SelectedItem != null)
        {
            var modulo_seleccionado = (AccountModule) pickerModulos.SelectedItem;
            var database = new AccountTypeModuleDb();
            accountTypeModules = (await database.GetItemsAsync()).Where(x=>x._module_id == modulo_seleccionado.id).ToArray();
           
            if (accountTypeModules.Length == 0)
            {
                return;
            }

            pickerTipoNc.ItemsSource = accountTypeModules;
            pickerTipoNc.ItemDisplayBinding = new Binding(nameof(AccountTypeModule.name));
            pickerTipoNc.SelectedIndex = 0;
        }
    }

    private async void btnLoadItems_Clicked(object sender, EventArgs e)
    {
        await LoadItems(false);
    }

    private async void btnLoadItemsAndBack_Clicked(object sender, EventArgs e)
    {
        await LoadItems(true);
    }

    private async Task LoadItems(bool withQuantity)
    {
        if (_res_partner == null)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            string text = "Debe seleccionar un cliente para poder cargar los productos.";
            ToastDuration duration = ToastDuration.Short;
            double fontSize = 14;
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
            return;
        }

        if (_accountMoveSelected == null)
        {
            Toast.Make("Debe seleccionar una factura para poder cargar los items");
            return;
        }

        //if(txtItem.Text == null || txtItem.Text.Length < 3)
        //{
        //    await Toast.Make("Debe colocar al menos 3 caracteres en el filtro para poder realizar la búsqueda").Show();
        //    return;
        //}

        var empresa = (res_company) SelectorCmp.SelectedItem;
        default_empresa = empresa;

        var simplePopup = new PopupLoadingTask(popupSizeConstants);
        simplePopup.Size = popupSizeConstants.Small;
        simplePopup.CanBeDismissedByTappingOutsideOfPopup = false;
        this.ShowPopup(simplePopup);

        //switch (TypeSearch)
        //{
        //    case "item":
        //        {
        //            //facNotaCreditoDets_ls = (await facNotaCreditoDetDb.GetItemsArticuloAsync(default_empresa.id, facNotaCreditoCab.CODCLIENTE, txtItem.Text));
        //        }
        //        break;
        //    case "factura":
        //        {
        //            //facNotaCreditoDets_ls = (await facNotaCreditoDetDb.GetItemsFacturaAsync(default_empresa.id, facNotaCreditoCab.CODCLIENTE, txtItem.Text));
        //        }
        //        break;
        //}

        AccountMoveLineDb database = new AccountMoveLineDb();
        var result = await database.GetItemsByParentAsync(_accountMoveSelected.id);
        //Agrega filtro para mostrar solo productos
        result = result.Where(x => x.display_type == "product").ToList();

        List<account_move_line_send> result_send = new List<account_move_line_send>();

        foreach (var item in result)
        {
            account_move_line_send account_Move_Line_Send = new account_move_line_send();
            account_Move_Line_Send.price_unit = item.price_unit;
            account_Move_Line_Send.price_total = item.price_total;
            account_Move_Line_Send.original_quantity = item.quantity;
            if(withQuantity)
            {
                account_Move_Line_Send.quantity = item.quantity;
            }
            else
            {
                account_Move_Line_Send.quantity = 0;
            }
            

            account_Move_Line_Send.account_id = item.accountId;
            account_Move_Line_Send.product_id = item.productId;
            account_Move_Line_Send.name = item.name;
            account_Move_Line_Send.currency_id = 2;
            //account_Move_Line_Send.line_id = item.line_ids;
            account_Move_Line_Send.move_id = item.moveId;

            result_send.Add(account_Move_Line_Send);
        }

        ObservableCollection<account_move_line_send> laccountmoveLines = new ObservableCollection<account_move_line_send>();
        laccountmoveLines = new ObservableCollection<account_move_line_send>(result_send);

        collectionView.ItemsSource = laccountmoveLines;

        simplePopup.Close();

        _account_move_lines_send_items = result_send.ToArray();

        lblSummary.Text = "Total registros " + laccountmoveLines.Count();
    }

    private async void btnDelete_Clicked(object sender, EventArgs e)
    {
        Debug.WriteLine("Quitar");

        Button button = (Button) sender;
        var item = (account_move_line) button.CommandParameter;

        Debug.WriteLine(item.name);
    }
        
    private async void btnSave_Clicked(object sender, EventArgs e)
    {
        AccountModule selected_module = null;
        AccountTypeModule selected_type_module = null;

        if (editionMode)
        {

        }
        
        if (pickerModulos.SelectedItem != null)
        {
            //await Toast.Make("Por favor, seleccione un módulo antes de guardar").Show();
            //return;
            selected_module = (AccountModule) pickerModulos.SelectedItem;
        }

        if(pickerTipoNc.SelectedItem != null)
        {
            //await Toast.Make("Por favor, seleccione un tipo de nota de crédito antes de guardar").Show();
            //return;
            selected_type_module = (AccountTypeModule) pickerTipoNc.SelectedItem;
        }

        //TODO: NO ELIMINAR ESTA SECCION DE CODIGO
        //if (pickerCNJournal.SelectedItem != null)
        //{
        //    credit_note_journal = (account_journal) pickerCNJournal.SelectedItem;
        //}
        //else
        //{
        //    await Toast.Make("Por favor, seleccione el diario para nota de crédito").Show();
        //    return;
        //}

        if (_res_partner == null)
        {
            await Toast.Make("Por favor, seleccione un cliente antes de guardar").Show();
            return;
        }

        if (_account_move_lines_send_items == null)
        {
            await Toast.Make("Por favor, seleccione al menos un producto antes de guardar").Show();
            return;
        }

        if (_account_move_lines_send_items != null && _account_move_lines_send_items.Length == 0)
        {
            await Toast.Make("Por favor, seleccione al menos un producto antes de guardar").Show();
            return;
        }

        if (txtObs.Text == null || txtObs.Text.Trim().Length == 0)
        {
            await Toast.Make("Por favor, ingrese texto en el campo de Referencia de pago").Show();
            return;
        }

        List<account_move_line_send> _account_move_line_send_list = new List<account_move_line_send>();

        var empresa = (res_company) SelectorCmp.SelectedItem;

        int rowItem = 0;

        foreach(var itemDet in _account_move_lines_send_items)
        {
            decimal quantity_for_return = 0;
            quantity_for_return = itemDet.quantity;

            //itemDet.CANTIDADDEVUELTA = "1";

            if (quantity_for_return > 0)
            {
                _account_move_line_send_list.Add(itemDet);
                rowItem++;
            }

            //if(rowItem == 5)
            //{
            //    break;
            //}            
        }

        if(rowItem == 0)
        {
            await Toast.Make("No ha ingresado ninguna devolución, debe ingresar al menos una para poder proceder.").Show();
            return;
        }

        if (_account_move_line_send_list.Count() > 0)
        {
            //string fechaActual = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10);
            DateTime fechaActual = DateTime.Now;

            //Guardar info
                        
            //Update
            if (accountMoveSend == null) // && editionMode)
            {                
                accountMoveSend = new account_move_send();
                accountMoveSend.parent_id = 0;
            }
            else
            {
                
            }

            accountMoveSend.build_mode = "factura";
            accountMoveSend.company_id = empresa.id;
            accountMoveSend.create_date = fechaActual;
            accountMoveSend.create_uid = App.Session.CurrentUser.uid;
            accountMoveSend.partner_id = _res_partner.id;
            accountMoveSend.partner_name = _res_partner.name;
            accountMoveSend.partner_email = _res_partner.email;
            accountMoveSend.doc_status = "pending";
            accountMoveSend.move_type = "out_refund";
            accountMoveSend.document_type = "electronic";
            accountMoveSend.l10n_latam_document_type_id = 45;
            accountMoveSend._ref = _accountMoveSelected.name;
            accountMoveSend.title = _accountMoveSelected.name;
            accountMoveSend.reversed_entry_id = _accountMoveSelected.id;
            accountMoveSend.invoice_date = _accountMoveSelected.invoice_date;
            //accountMoveSend.journal_id = 52;

            //accountMoveSend.journal_id = credit_note_journal.id;
            accountMoveSend.journal_id = empresa.credit_note_journal_id_;

            accountMoveSend.module_id = selected_module.id;
            accountMoveSend.type_module_id = selected_type_module.id;            

            accountMoveSend.group_status = "open";
            //_account_move_send_New.NOMBREUSUARIO = App.Session.CurrentUser.nombres;
            var tipSel = pickerTipoNc.SelectedItem as st_tiposnc;
            accountMoveSend.payment_reference = txtObs.Text;
            //_account_move_send_New.TIPONOTACREDITO = tipSel.descripcion;

            //_account_move_send_New.DETALLESNC = JsonConvert.SerializeObject(facNotaCreditoDetsNews);

            //TODO: Agregar proceso para guardar detalles (NUEVO MODO ODOO)

            int parent_id = 0;

            if (editionMode)
            {                
                parent_id = accountMoveSend.id;
            }
            else
            {                
                parent_id = accountMoveSend.id;
            }

            foreach (var line_item in _account_move_line_send_list)
            {
                line_item.parent_move_id = parent_id;
            }

            accountMoveSend.lines = _account_move_line_send_list.ToArray();
        }

        saveData = true;
        await Navigation.PopModalAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Verificar la variable para decidir si permitir o no el cierre de la página
        if (true)
        {
            // Si no se permite el cierre, evitar que la página se cierre
            Navigation.PopModalAsync(false);
        }
    }

    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        //bool answer = await DisplayAlert("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");
        ////Debug.WriteLine("Answer: " + answer);
        //if (!answer)
        //{
        //    return;
        //}
        SendBackButtonPressed();
        //await Navigation.PopModalAsync();
    }

    private void btnRemoveCustomer_Clicked(object sender, EventArgs e)
    {
        _res_partner = null;
        txtCliente.Text = "<NO SELECCIONADO>";
        ClearItems();
    }

    private void btnRemoveInvoice_Clicked(object sender, EventArgs e)
    {
        _accountMoveSelected = null;
        txtFactura.Text = "<Sin factura seleccionada>";

        LblInvoiceDate.Text = "-";
        LblJournal.Text = "-";
        LblSaleShop.Text = "-";

        ClearItems();
    }

    private void btnClearItems_Clicked(object sender, EventArgs e)
    {
        ClearItems();
    }

    private void ClearItems()
    {
        _account_move_lines_send_items = new account_move_line_send[0];
        collectionView.ItemsSource = _account_move_lines_send_items.ToArray();
        lblSummary.Text = "Total registros 0";
    }

    private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        //Debug.WriteLine("Radio:");
        //Debug.WriteLine(e.Value);
        // Obtén el RadioButton actualmente seleccionado
        if (sender is RadioButton radioButton && radioButton.IsChecked)
        {
            var valorSeleccionado = radioButton.Value;
            TypeSearch = valorSeleccionado.ToString();
            Debug.WriteLine("Radio:" + TypeSearch);
            
            // Aquí puedes usar el valor seleccionado según tu lógica
            // Por ejemplo, si el valor es "item" o "factura"
        }
    }

    private async void chkSelectAll_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        await ReturnAllItem(e.Value);
        Debug.WriteLine(e.Value.ToString());
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
    }
}