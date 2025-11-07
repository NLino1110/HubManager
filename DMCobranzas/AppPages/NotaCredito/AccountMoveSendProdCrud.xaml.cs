
using DMCobranzas;
using DMCobranzas.Controls;
using DMCobranzas.Models;
using DMCobranzas.Settings.helpers;

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
using Fluid;
using DMSA.Models.Odoo.DMCobranzas;
using CommunityToolkit.Maui.Extensions;
using DMCobranzas.Services.Database.Sqlite;

namespace DMCobranzas.AppPages.NotaCredito;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AccountMoveSendProdCrud : ContentPage
{
    public bool isNewData { get; set; } = false;
    //public int itemIndex { get; set; } = -1;
    public bool saveData { get; set; } = false;

    private account_journal[] credit_note_journals { get; set; }
    private account_journal credit_note_journal { get; set; }
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
    public account_move_send[] accountMovesSend { get; set; }
        
    //Producto seleccionado
    public account_move_line _accountMoveLineSelected { get; set; }

    readonly PopupSizeConstants popupSizeConstants;
	readonly CsharpBindingPopupViewModel csharpBindingPopupViewModel;
    
	public AccountMoveSendProdCrud(res_partner _res_partner_param)
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

    public void SetEditionMode(account_move_send[] _account_move_send)
    {
        editionMode = true;
        accountMovesSend = _account_move_send;        
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

        account_move_send account_ml_item = (account_move_send) obj;
        string newValue = await DisplayPromptAsync("Cant. Devolver", "Ingrese cantidad que se desea devolver", "APLICAR", "CERRAR", account_ml_item.lines[0].quantity.ToString(), 10, Keyboard.Numeric); //, cobCarteraDet.VALORXAPLICAR);

        if (newValue != null && newValue != "")
        {
            //Operaciones de conversión
            account_ml_item.lines[0].quantity = (decimal) ParseTool.StringToDouble(newValue);
            //facNotaCreditoDetAuxiliar.BLOQUEADO = "S";

            //var _items = facNotaCreditoDetAuxiliar;

            var nList = accountMovesSend.ToList();
            
            int indexToReplace = nList.FindIndex(item =>
            item.id == account_ml_item.id &&
            item.sequence == account_ml_item.sequence);

            // Verificar si se encontró el objeto a reemplazar
            if (indexToReplace != -1)
            {
                //Reemplazar el objeto existente con el nuevo objeto
                accountMovesSend[indexToReplace] = account_ml_item;
                Debug.WriteLine("Modificado...");

                //Se fuerza la actualización del visor creando una nueva instancia del arreglo
                var tmpDI = accountMovesSend.ToList();
                accountMovesSend = tmpDI.ToArray();

                collectionView.ItemsSource = accountMovesSend;                
            }
        }
    }

    private async Task ReturnAllItem(bool ReturnAll)
    {
        Debug.WriteLine("ReturnAllItem");
        if(accountMovesSend == null)
        {
            return;
        }

        foreach(var _line_item in accountMovesSend)
        {
            //Operaciones de conversión
            if(ReturnAll)
            {
                _line_item.lines[0].quantity = _line_item.lines[0].quantity;
            }
            else
            {
                _line_item.lines[0].quantity = 0;
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

        var tmpDI = accountMovesSend.ToList();
        accountMovesSend = tmpDI.ToArray();
        collectionView.ItemsSource = accountMovesSend;
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

        //var returnResultPopup = new PopupSearchInvoice(popupSizeConstants);
        var empresa = (res_company) SelectorCmp.SelectedItem;
        //returnResultPopup.empresa = empresa;
        //returnResultPopup.res_Partner = _res_partner;

        ////Evita que se cierre cuando se haga clic (tap) fuera de la ventana
        //returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        //if (!isWindows)
        //    returnResultPopup.Size = this.popupSizeConstants.Large;

        var resultPopupSelectInvoice = new PopupSelectInvoiceItems(popupSizeConstants);
        resultPopupSelectInvoice.Company = empresa;
        resultPopupSelectInvoice.partner = _res_partner;
        var result = await this.ShowPopupAsync(resultPopupSelectInvoice);
        if (result != null)
        {
            _accountMoveLineSelected = (account_move_line)result;            
            txtProducto.Text = _accountMoveLineSelected.name;
            //Cargar documentos de factura
        }

        //await DisplayAlert("Pop Result Returned", $"Result: {result}", "OK");
    }

    public async Task SetDefaultData(res_company empresa)
    {
        default_empresa = empresa;
    }

	async Task PrepareForm()
	{
        if (App.Session.CurrentUserFront.empresas != null)
        {
            Debug.WriteLine("Empresas:");
            Debug.WriteLine(App.Session.CurrentUserFront.empresas.Length);
            Empresas = App.Session.CurrentUserFront.empresas;
            SelectorCmp.ItemsSource = Empresas;
            SelectorCmp.ItemDisplayBinding = new Binding(nameof(res_company.name));
            SelectorCmp.SelectedIndex = 0;

            if (default_empresa != null)
            {
                SelectorCmp.SelectedItem = default_empresa;
            }
        }

        //TODO: NO ELIMINAR ESTE GRUPO DE COMENTARIO
        //var database_journals = new AccountJournalDb();
        //credit_note_journals = (await database_journals.GetItemsAsync()).Where(x => x.credit_note).ToArray();

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

        if (accountMovesSend != null)
        {
            if (_res_partner != null)
            {
                lblTitle.Text = "EDICIÓN - " + _res_partner.name;

                //await LoadForEdition();
                //AccountMoveDb accountMoveDb = new AccountMoveDb();
                //_accountMoveSelected = await accountMoveDb.GetByNameItem(accountMoveSend._ref);
                txtProducto.Text = _accountMoveLineSelected.name;
                //txtObs.Text = accountMovesSend.payment_reference;

                //Se procede a la carga de los datos del modulo
                // solo se carga una factura por vez aunque se hayan almacenado varias previamente
                // en el caso de notas de credito por producto

                AccountModule selected_module = null;
                AccountTypeModule selected_type_module = null;

                selected_module = accountModules.Where(x => x.id == accountMovesSend[0].module_id).FirstOrDefault();
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

                selected_type_module = accountTypeModules.Where(x => x.id == accountMovesSend[0].type_module_id).FirstOrDefault();
                pickerTipoNc.SelectedItem = selected_type_module;

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
        if (accountMovesSend != null)
        {
            if (accountMovesSend.Length > 0)
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
        //List<account_move_send> accountMoveSendLinesMem = new List<account_move_send>();

        foreach (var moveSend in accountMovesSend)
        {
            List<account_move_line_send> accountMoveSendLinesMem = new List<account_move_line_send>();
            foreach (var line in moveSend.lines)
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

            moveSend.lines = accountMoveSendLinesMem.ToArray();
        }

        //accountMovesSend = accountMoveSendLinesMem.ToArray();

        //lblCounter.Text = "Total de documentos " + accountMovesSend.Length.ToString();
        collectionView.ItemsSource = accountMovesSend;
    }

    private async void pickerModulos_SelectedIndexChanged(object sender, EventArgs e)
    {
        Debug.WriteLine("pickerModulos");
        Debug.WriteLine(pickerModulos.SelectedIndex);

        if (pickerModulos.SelectedItem != null)
        {
            var modulo_seleccionado = (AccountModule)pickerModulos.SelectedItem;
            var database = new AccountTypeModuleDb();
            accountTypeModules = (await database.GetItemsAsync()).Where(x => x._module_id == modulo_seleccionado.id).ToArray();

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

    private async Task LoadItems( bool withQuantity )
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

        if (_accountMoveLineSelected == null)
        {
            Toast.Make("Debe seleccionar un producto para poder cargar los items");
            return;
        }

        if (pickerCNJournal.SelectedItem != null)
        {
            credit_note_journal = (account_journal)pickerCNJournal.SelectedItem;
        }
        else
        {
            await Toast.Make("Por favor, seleccione el diario para nota de crédito").Show();
            return;
        }

        //if(txtItem.Text == null || txtItem.Text.Length < 3)
        //{
        //    await Toast.Make("Debe colocar al menos 3 caracteres en el filtro para poder realizar la búsqueda").Show();
        //    return;
        //}

        var empresa = (res_company)SelectorCmp.SelectedItem;
        default_empresa = empresa;

        var simplePopup = new PopupLoadingTask(popupSizeConstants);
        //simplePopup.Size = popupSizeConstants.Small;
        simplePopup.CanBeDismissedByTappingOutsideOfPopup = false;
        this.ShowPopup(simplePopup);



        var databaseInvoice = new AccountMoveDb();
        var resultInvoices = await databaseInvoice.GetItemsAsync(default_empresa.id, _res_partner.id, 25);

        var database = new AccountMoveLineDb();
        var result = await database.GetItemsAsync(_accountMoveLineSelected.productId, resultInvoices.ToArray(), 50);
        //resultItemsSearch = new ObservableCollection<account_move_line>(result);
        //_collectionViewSearch.ItemsSource = resultItemsSearch;

        var resultInvoicesFiltered = await databaseInvoice.GetItemsAsync(result.ToArray(), default_empresa.id, _res_partner.id, 50);

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


        List<account_move_send> result_move_send = new List<account_move_send>();

        foreach (var item in resultInvoicesFiltered)
        {
            //Sino es factura no se agrega
            if (item.move_type != "out_invoice") continue;

            account_move_send account_Move_Send = new account_move_send();
            account_Move_Send.id = item.id;
            account_Move_Send.create_id = 0;
            account_Move_Send.move_type = "out_refund";
            account_Move_Send.invoice_date = item.invoice_date;

            account_Move_Send.partner_id = item._partner_id;
            account_Move_Send.company_id = item._company_id;
            account_Move_Send.document_type = "electronic";
            account_Move_Send.printer_id = 0;
            account_Move_Send.l10n_latam_document_type_id = 45;
            account_Move_Send.journal_id = credit_note_journal.id;
            account_Move_Send.payment_reference = "REFERENCIA TXT";
            account_Move_Send._ref = item.name;

            account_Move_Send.reversed_entry_id = item._reversed_entry_id;

            account_Move_Send.name = null;
            account_Move_Send.create_date = DateTime.Now;
            account_Move_Send.create_uid = 0;
            account_Move_Send.partner_name = _res_partner.name;
            account_Move_Send.partner_email = _res_partner.email;
            account_Move_Send.doc_status = "pending";
            //account_Move_Send.send_date = 0;
            account_Move_Send.group_status = "open";
            account_Move_Send.parent_id = _res_partner.id;
            //account_Move_Send.ref_name = null;
            account_Move_Send.build_mode = "producto";

            account_Move_Send.lines = result_send.Where(p => p.move_id == account_Move_Send.id).ToArray();

            result_move_send.Add(account_Move_Send);
        }

        ObservableCollection<account_move_send> laccountmoveLines = new ObservableCollection<account_move_send>();
        laccountmoveLines = new ObservableCollection<account_move_send>(result_move_send);

        collectionView.ItemsSource = laccountmoveLines;

        await simplePopup.CloseAsync();

        accountMovesSend = result_move_send.ToArray();

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
            selected_module = (AccountModule)pickerModulos.SelectedItem;
        }

        if (pickerTipoNc.SelectedItem != null)
        {
            //await Toast.Make("Por favor, seleccione un tipo de nota de crédito antes de guardar").Show();
            //return;
            selected_type_module = (AccountTypeModule)pickerTipoNc.SelectedItem;
        }

        if (pickerCNJournal.SelectedItem != null)
        {
            credit_note_journal = (account_journal) pickerCNJournal.SelectedItem;
        }
        else
        {
            await Toast.Make("Por favor, seleccione el diario para nota de crédito").Show();
            return;
        }

        if (_res_partner == null)
        {
            await Toast.Make("Por favor, seleccione un cliente antes de guardar").Show();
            return;
        }

        if (accountMovesSend == null)
        {
            await Toast.Make("Por favor, seleccione al menos un producto antes de guardar").Show();
            return;
        }

        if (accountMovesSend != null && accountMovesSend.Length == 0)
        {
            await Toast.Make("Por favor, seleccione al menos un producto antes de guardar").Show();
            return;
        }

        if (txtObs.Text == null || txtObs.Text.Trim().Length == 0)
        {
            await Toast.Make("Por favor, ingrese texto en el campo de Referencia de pago").Show();
            return;
        }

        List<account_move_send> _account_move_send_list = new List<account_move_send>();

        var empresa = (res_company) SelectorCmp.SelectedItem;

        int rowItem = 0;

        //Update
        if (accountMovesSend == null) // && editionMode)
        {
            accountMovesSend = new account_move_send[0];
            //accountMovesSend.parent_id = 0;
        }
        else
        {

        }

        foreach (var itemMoveSend in accountMovesSend)
        {
            decimal quantity_for_return = 0;
            quantity_for_return = itemMoveSend.lines[0].quantity;

            //itemDet.CANTIDADDEVUELTA = "1";

            if (quantity_for_return > 0)
            {
                _account_move_send_list.Add(itemMoveSend);
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

        if (_account_move_send_list.Count() > 0)
        {
            //string fechaActual = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10);
            DateTime fechaActual = DateTime.Now;

            //Guardar info

            foreach (var itemMoveSend in _account_move_send_list)
            {
                itemMoveSend.build_mode = "producto";
                itemMoveSend.company_id = empresa.id;
                itemMoveSend.create_date = fechaActual;
                itemMoveSend.create_uid = App.Session.CurrentUser.uid;
                itemMoveSend.partner_id = _res_partner.id;
                itemMoveSend.partner_name = _res_partner.name;
                itemMoveSend.partner_email = _res_partner.email;
                itemMoveSend.doc_status = "pending";
                itemMoveSend.move_type = "out_refund";
                itemMoveSend.document_type = "electronic";
                itemMoveSend.l10n_latam_document_type_id = 45;
                //itemMoveSend._ref = _accountMoveLineSelected.name;
                
                itemMoveSend.title = itemMoveSend._ref + " " + _accountMoveLineSelected.name;

                //accountMoveSend.invoice_date = _accountMoveLineSelected.invoice_date;
                //itemMoveSend.journal_id = credit_note_journal.id;
                itemMoveSend.journal_id = empresa.credit_note_journal_id_;

                itemMoveSend.module_id = selected_module.id;
                itemMoveSend.type_module_id = selected_type_module.id;

                itemMoveSend.group_status = "open";
                //_account_move_send_New.NOMBREUSUARIO = App.Session.CurrentUser.nombres;
                var tipSel = pickerTipoNc.SelectedItem as st_tiposnc;
                itemMoveSend.payment_reference = txtObs.Text;
                //_account_move_send_New.TIPONOTACREDITO = tipSel.descripcion;

                //_account_move_send_New.DETALLESNC = JsonConvert.SerializeObject(facNotaCreditoDetsNews);

                //TODO: Agregar proceso para guardar detalles (NUEVO MODO ODOO)

                int parent_id = 0;

                if (editionMode)
                {
                    parent_id = itemMoveSend.id;
                }
                else
                {
                    parent_id = itemMoveSend.id;
                }

                foreach (var line_item in itemMoveSend.lines)
                {
                    line_item.parent_move_id = parent_id;
                }
            }
            //accountMoveSend.lines = _account_move_send_list.ToArray();
        }

        accountMovesSend = _account_move_send_list.ToArray();

        saveData = true;
        await Navigation.PopModalAsync();
    }

    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");
        //Debug.WriteLine("Answer: " + answer);
        if (!answer)
        {
            return;
        }
                
        await Navigation.PopModalAsync();
    }

    private void btnRemoveCustomer_Clicked(object sender, EventArgs e)
    {
        _res_partner = null;
        txtCliente.Text = "<NO SELECCIONADO>";
        ClearItems();
    }

    private void btnRemoveInvoice_Clicked(object sender, EventArgs e)
    {
        _accountMoveLineSelected = null;
        txtProducto.Text = "<Sin producto seleccionado>";        
        //ClearItems();
    }

    private void btnClearItems_Clicked(object sender, EventArgs e)
    {
        ClearItems();
    }

    private void ClearItems()
    {
        accountMovesSend = new account_move_send[0];
        collectionView.ItemsSource = accountMovesSend.ToArray();

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