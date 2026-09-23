using ApiManagerOdoo.Accounting;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.Input;
using DMCobranzas.Models.UI;
using DMCobranzas.Settings.helpers;
using DMSA.Sync.Core.Helpers;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core;
using DMSA.Sync.Core.Controls.Popups;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Accounting;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Newtonsoft.Json;

namespace DMCobranzas.AppPages.NotaCredito;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CreditNoteRequestCrud : ContentPage
{
    public bool isNewData { get; set; } = false;
    public int itemIndex { get; set; } = -1;
    public bool saveData { get; set; } = false;
    private account_journal credit_note_journal { get; set; }
    private res_company default_res_company { get; set; }
    public res_company[] Empresas { get; set; }
    public res_partner _res_partner { get; set; }

    public bool ShowClientPermanentlyClosingBanner =>
        _res_partner?.client_permanently_closing == true;

    private bool ClientPermanentlyClosingForUom =>
        _res_partner?.client_permanently_closing == true;
    public TypeParentNc[] typeParentNcList { get; set; }
    public TypeNc[] accountTypeModules { get; set; }
    public bool isWindows { get; set; } = false;
    private bool editionMode { get; set; } = false;
    private string TypeSearch { get; set; }
    public CreditNoteRequestGroup accountMoveSendHeader { get; set; }
    public credit_note_request creditNoteRequest { get; set; }
    public ObservableCollection<credit_note_request_detail> _creditNoteReqDetails_items { get; set; }
    public account_move _accountMoveSelected { get; set; }

    readonly PopupSizeConstants popupSizeConstants;
    public ObservableCollection<account_move> Invoices { get; set; } = new();
    public ObservableCollection<account_move_line_view> InvoicesLinesForSelect { get; set; } = new();

    private bool IsLoadingData { get; set; }

    private IconData _icon;
    public IconData Icon
    {
        get => _icon;
        set
        {
            if (_icon == value)
                return;

            _icon = value;
            OnPropertyChanged();
        }
    }

    private int _detailsCount;
    public int detailsCount
    {
        get
        {
            //OnPropertyChanged();
            if (_creditNoteReqDetails_items != null)
            {
                _detailsCount = _creditNoteReqDetails_items.Count();
            }
            else
            {
                _detailsCount = 0;
            }
            return _detailsCount;
        }
    }

    public ObservableCollection<IconData> Icons { get; } =
        [
            new IconData
            {
                Name = "invoice",
                Description = "Una factura",
                IconSource = "\uf058",
                FontFamily = "FontAwesome5Solid"
            },
            new IconData
            {
                Name = "invoices",
                Description = "Varias facturas",
                IconSource = "\uf0ae",
                FontFamily = "FontAwesome5Solid"
            }
        ];

    public CreditNoteRequestCrud(res_partner _res_partner_param, res_company _res_company)
    {
        _res_partner = _res_partner_param;
        default_res_company = _res_company;

        InitializeComponent();

        if (popupSizeConstants == null)
        {
            this.popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        }
        else
        {
            this.popupSizeConstants = popupSizeConstants;
        }

        isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;

        DeleteCommand = new Command(DeleteItem);
        ReturnItemCommand = new Command(ReturnItem);

        TypeSearch = "item";

        PrepareForm();

        txtReason.Text = "";

        Icon = Icons.FirstOrDefault();

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
                await Navigation.PopAsync();
            }
        });

        return true;
    }

    public void SetEditionMode(credit_note_request _account_move_send)
    {
        editionMode = true;
        creditNoteRequest = _account_move_send;
    }

    public ICommand DeleteCommand { get; set; }

    private void DeleteItem(object obj)
    {
        //_creditNoteReqDetails_items.Remove((credit_note_request_detail) obj);
        var RemoveForItem = (credit_note_request_detail)obj;
        var foundItemForRemove = _creditNoteReqDetails_items.Where(x => x.line_id == RemoveForItem.line_id).FirstOrDefault();

        if (foundItemForRemove != null)
        {
            _creditNoteReqDetails_items.Remove(foundItemForRemove);
            OnPropertyChanged(nameof(_creditNoteReqDetails_items));
            OnPropertyChanged(nameof(detailsCount));
        }

        Debug.WriteLine("DeleteItem");
    }

    public ICommand ReturnItemCommand { get; set; }


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

    private async void ReturnItem(object obj)
    {
        Debug.WriteLine("ReturnItem");

        credit_note_request_detail account_ml_item = (credit_note_request_detail)obj;
        string newValue = await DisplayPromptAsync("Cant. Devolver", "Ingrese cantidad que se desea devolver", "APLICAR", "CERRAR", account_ml_item.quantity.ToString(), 10, Keyboard.Numeric); //, cobCarteraDet.VALORXAPLICAR);

        if (newValue != null && newValue != "")
        {
            account_ml_item.quantity = (decimal)ParseTool.StringToDouble(newValue);

            var nList = _creditNoteReqDetails_items.ToList();

            int indexToReplace = nList.FindIndex(item =>
            item.account_id == account_ml_item.account_id &&
            item.sequence == account_ml_item.sequence);

            if (indexToReplace != -1)
            {
                _creditNoteReqDetails_items[indexToReplace] = account_ml_item;
                Debug.WriteLine("Modificado...");

                var tmpDI = _creditNoteReqDetails_items.ToList();
                _creditNoteReqDetails_items = new ObservableCollection<credit_note_request_detail>(tmpDI);

                collectionView.ItemsSource = _creditNoteReqDetails_items;
            }
        }
    }

    private async Task ReturnAllItem(bool ReturnAll)
    {
        Debug.WriteLine("ReturnAllItem");
        if (_creditNoteReqDetails_items == null)
        {
            return;
        }

        foreach (var _line_item in _creditNoteReqDetails_items)
        {
            if (ReturnAll)
            {
                _line_item.quantity = _line_item.quantity;
            }
            else
            {
                _line_item.quantity = 0;
            }
        }

        var tmpDI = _creditNoteReqDetails_items.ToList();
        _creditNoteReqDetails_items = new ObservableCollection<credit_note_request_detail>(tmpDI);
        collectionView.ItemsSource = _creditNoteReqDetails_items;
    }

    async void HandleReturnResultPopupButtonClicked(object sender, EventArgs e)
    {
        var empresa = (res_company)SelectorCmp.SelectedItem;

        var resultPopupSelectInvoice = new PopupSelectPartner(popupSizeConstants);
        resultPopupSelectInvoice.Company = empresa;
        resultPopupSelectInvoice.DetailMode = 1;

        resultPopupSelectInvoice.CanBeDismissedByTappingOutsideOfPopup = false;

        var result = await this.ShowPopupAsync(resultPopupSelectInvoice);
        if (result != null)
        {
            var resPartner = (res_partner)result;
            txtCliente.Text = resPartner.id.ToString() + " - " + resPartner.name;
            _res_partner = resPartner;
            await RefreshPartnerFromLocalDbAsync();
        }
    }

    async Task<account_move> PopupAccountMove(object sender, EventArgs e)
    {
        account_move selected_account_move = null;
        var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        popupSizeConstants.CalculateSizes(DeviceDisplay.Current);

        var returnResultPopup = new PopupSelectInvoice(popupSizeConstants);

        var empresa = (res_company)SelectorCmp.SelectedItem;

        returnResultPopup.Company = App.Session.res_Company;
        returnResultPopup.Company = empresa;
        returnResultPopup.partner = _res_partner;
        returnResultPopup.LoadAuto = true;

        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        var result = await PopupExtensions.ShowPopupAsync<account_move>(App.Current.MainPage, returnResultPopup);

        if (result.Result != null)
        {
            selected_account_move = result.Result;
        }

        return selected_account_move;
    }

    async Task<account_move_line_view> PopupProductInMove(object sender, EventArgs e)
    {
        account_move_line_view selected_product = null;
        var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        popupSizeConstants.CalculateSizes(DeviceDisplay.Current);

        var returnResultPopup = new PopupSelectProductInMove(popupSizeConstants);

        var empresa = (res_company)SelectorCmp.SelectedItem;

        returnResultPopup.Company = App.Session.res_Company;
        returnResultPopup.ResPartner = _res_partner;
        //returnResultPopup.LoadAuto = true;

        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        var result = await PopupExtensions.ShowPopupAsync<account_move_line_view>(App.Current.MainPage, returnResultPopup);

        if (result.Result != null)
        {
            selected_product = result.Result;
        }

        return selected_product;
    }

    private async void ddInvoicesLines_SelectedItemChanged(object? sender, object e)
    {
        if (IsLoadingData) return;

        account_move_line new_selected_account_move_line = (account_move_line)e;

        if (new_selected_account_move_line != null && (new_selected_account_move_line.id == -1 || new_selected_account_move_line.id == 0))
        {
            if (new_selected_account_move_line.id == -1)
            {
                var selectedProduct = await PopupProductInMove(sender, null);

                if (selectedProduct != null)
                {
                    ddInvoicesLines.IsEnabled = false;
                    ddInvoicesLines.SelectedItem = InvoicesLinesForSelect.FirstOrDefault();
                    List<account_move_line> linesForAdd = new List<account_move_line>();
                    linesForAdd.Add((account_move_line)selectedProduct);
                    await LoadItemsMode2(false, linesForAdd);
                    ddInvoicesLines.IsEnabled = true;
                    return;
                }
            }
        }

        if (new_selected_account_move_line == null)
        {

        }
        else
        {
            if (new_selected_account_move_line.id != -1 && new_selected_account_move_line.id != 0)
            {
                ddInvoicesLines.SelectedItem = InvoicesLinesForSelect.FirstOrDefault();
                List<account_move_line> linesForAdd = new List<account_move_line>();
                linesForAdd.Add((account_move_line)new_selected_account_move_line);
                await LoadItemsMode2(false, linesForAdd);
            }
            else
            {
                Debug.WriteLine("Seleccion cancelada..");
                ddInvoicesLines.SelectedItem = InvoicesLinesForSelect.FirstOrDefault();
            }
        }
    }

    private async void ddInvoices_SelectedItemChanged(object? sender, object e)
    {
        if (IsLoadingData) return;

        account_move new_selected_account_move = (account_move)e;

        if (new_selected_account_move != null && (new_selected_account_move.id == -1 || new_selected_account_move.id == 0))
        {
            if (new_selected_account_move.id == -1)
            {
                ddInvoices.SelectedItem = _accountMoveSelected;

                var selectedAccountMove = await PopupAccountMove(sender, null);

                if (selectedAccountMove != null)
                {
                    //selectedAccountMove.name = selectedAccountMove.name + " - " + 
                    //    selectedAccountMove.docnum_mask + " - " + 
                    //    selectedAccountMove.invoice_date.ToString("yyyy-MM-dd") + 
                    //    " - $" + selectedAccountMove.amount_total.ToString("F2");

                    ddInvoices.IsEnabled = false;
                    ddInvoices.ItemsSource = null;
                    Invoices[0] = selectedAccountMove;
                    ddInvoices.ItemsSource = Invoices;
                    ddInvoices.SelectedItem = selectedAccountMove;
                    _accountMoveSelected = selectedAccountMove;
                    ddInvoices.IsEnabled = true;
                    return;
                }
            }
        }

        if (new_selected_account_move == null)
        {
            ddInvoices.IsEnabled = false;
            Debug.WriteLine("Clear");
            ddInvoices.ItemsSource = null;
            var nsAccountMove = new account_move { id = 0, name = "No seleccionada" };
            Invoices[0] = nsAccountMove;
            ddInvoices.ItemsSource = Invoices;
            ddInvoices.SelectedItem = nsAccountMove;
            _accountMoveSelected = nsAccountMove;
            ddInvoices.IsEnabled = true;
            btnClearItems_Clicked(null, null);
        }
        else
        {
            _accountMoveSelected = new_selected_account_move;

            if (_accountMoveSelected.id == 0)
            {
                Debug.WriteLine("0 seleccionado..");
                btnClearItems_Clicked(null, null);
                return;
            }
        }

        if (_accountMoveSelected.id != -1 && _accountMoveSelected.id != 0)
        {
            Debug.WriteLine("Lanzar cambio m1");
            btnClearItems_Clicked(null, null);
        }
        else
        {
            Debug.WriteLine("Seleccion cancelada..");
        }
    }

    //async void PopupSearchInvoiceButtonClicked(object sender, EventArgs e)
    //{
    //    if (_res_partner == null)
    //    {
    //        await Toast.Make("Debe seleccionar un cliente.").Show();
    //        return;
    //    }


    //    var empresa = (res_company) SelectorCmp.SelectedItem;
    //    var resultPopupSelectInvoice = new PopupSelectInvoice(popupSizeConstants);
    //    resultPopupSelectInvoice.Company = empresa;
    //    resultPopupSelectInvoice.partner = _res_partner;
    //    resultPopupSelectInvoice.LoadAuto = true;
    //    var result = await this.ShowPopupAsync<account_move>(resultPopupSelectInvoice);

    //    if (result.Result != null)
    //    {
    //        _accountMoveSelected = (account_move)result.Result;            
    //        //txtFactura.Text = _accountMoveSelected.name;            

    //        LblInvoiceDate.Text = _accountMoveSelected.invoice_date.ToString("yyyy-MM-dd");
    //        LblJournal.Text = "DIARIO:" + _accountMoveSelected._journal_id.ToString();
    //        LblSaleShop.Text = "ESTAB:" + _accountMoveSelected._printer_id.ToString();
    //    }
    //}

    async Task PrepareForm()
    {
        await RefreshPartnerFromLocalDbAsync();

        if (App.Session.CurrentUserFront.empresas != null)
        {
            Debug.WriteLine("Empresas:");
            Debug.WriteLine(App.Session.CurrentUserFront.empresas.Length);
            Empresas = App.Session.CurrentUserFront.empresas;
            SelectorCmp.ItemsSource = Empresas;
            SelectorCmp.ItemDisplayBinding = new Binding(nameof(res_company.name));
            SelectorCmp.SelectedIndex = 0;

            if (default_res_company != null)
            {
                SelectorCmp.SelectedItem = Empresas.FirstOrDefault(x => x.id == default_res_company.id);
                SelectorCmp.IsEnabled = false;
            }
        }

        var database_journals = new AccountJournalDb(App.Session.odooConnection.DbNameSqlite);
        credit_note_journal = (await database_journals.GetItemsAsync()).Where(
            x => x._company_id == default_res_company.id &&
            x.id == default_res_company.credit_note_journal_id_).FirstOrDefault();

        if (credit_note_journal == null)
        {
            await Toast.Make("Se requieren un diarios para notas de crédito.").Show();
            return;
        }
        else
        {
            await Toast.Make("Diario para notas de crédito " + credit_note_journal.id + "-" + credit_note_journal.name).Show();
        }

        var accountMoveDb = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
        var accountMoveLineDb = new AccountMoveLineDb(App.Session.odooConnection.DbNameSqlite);
        var database = new TypeParentNcDb(App.Session.odooConnection.DbNameSqlite);
        typeParentNcList = (await database.GetItemsAsync(x => x.nc_type == "ventas"
        && x.motivo_val_dev_nc == "devolucion"
        && x.code != "12345"
        && x.nc_type != "Modulo 1")).ToArray();

        if (typeParentNcList.Length == 0)
        {
            await Toast.Make("No se encontraron módulos compatibles.").Show();
            return;
        }

        pickerModulos.ItemsSource = typeParentNcList;
        pickerModulos.ItemDisplayBinding = new Binding(nameof(TypeParentNc.name));
        Debug.WriteLine("Módulos cargados!");

        if (creditNoteRequest != null)
        {
            if (_res_partner != null)
            {
                Title = "" + _res_partner.name;

                if (creditNoteRequest.mainAccountMove != null)
                {
                    _accountMoveSelected = await accountMoveDb.GetItemAsync(x => x.id == creditNoteRequest.mainAccountMove);

                    LblInvoiceDate.Text = _accountMoveSelected.invoice_date.ToString("yyyy-MM-dd");
                    LblJournal.Text = "DIARIO:" + _accountMoveSelected._journal_id.ToString();
                    LblSaleShop.Text = "ESTAB:" + _accountMoveSelected._printer_id.ToString();
                }

                txtReason.Text = creditNoteRequest.reason;
                TypeParentNc selected_module = null;
                TypeNc selected_type_module = null;

                selected_module = typeParentNcList.Where(x => x.id == creditNoteRequest.parent_nc_id).FirstOrDefault();
                pickerModulos.SelectedItem = selected_module;

                var database_type = new TypeNcDb(App.Session.odooConnection.DbNameSqlite);
                accountTypeModules = (await database_type.GetItemsAsync(x => x.id > 0 && x._parent_id == selected_module.id)).ToArray();

                if (accountTypeModules.Length == 0)
                {
                    //return;
                }

                pickerTipoNc.ItemsSource = accountTypeModules;
                pickerTipoNc.ItemDisplayBinding = new Binding(nameof(TypeNc.name));
                pickerTipoNc.SelectedIndex = 0;

                selected_type_module = accountTypeModules.Where(x => x.id == creditNoteRequest.type_module_id).FirstOrDefault();
                pickerTipoNc.SelectedItem = selected_type_module;

                Icon = Icons.FirstOrDefault(x => x.Name == creditNoteRequest.request_type);

                SelectorCmp.IsEnabled = false;
            }

            await LoadAccountMoveSendLines();
        }
        else
        {
            if (_res_partner != null)
            {
                Title = "[*]" + _res_partner.name;
            }

            await LoadAccountMoveSendLines();
        }

        var lastInvoices = (await accountMoveDb.GetItemsAsync(
            x => x._partner_id == _res_partner.id
                && x.move_type == "out_invoice"))
            .OrderByDescending(x => x.invoice_date)
            .Take(5)
            .ToList();

        var invoiceIds = lastInvoices
            .Select(x => x.id)
            .ToList();

        var lastInvoicesLines = (await accountMoveLineDb.GetByTopLines(_res_partner));

        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsLoadingData = true;
            if (_accountMoveSelected != null)
            {
                Invoices =
                   [
                        _accountMoveSelected,
                        new account_move { id = -1, name = "🔍 Buscar..." },
                   ];
            }
            else
            {
                Invoices =
                    [
                        new account_move { id = 0, name = "No seleccionada" },
                        new account_move { id = -1, name = "🔍 Buscar..." },
                    ];
            }

            foreach (var invoiceItem in lastInvoices)
            {
                Invoices.Add(invoiceItem);
            }

            InvoicesLinesForSelect =
                    [
                        new account_move_line_view { id = 0, product_name = "Seleccionar producto/facturas" },
                        new account_move_line_view { id = -1, product_name = "🔍 Buscar..." },
                    ];

            foreach (var invoiceLineItem in lastInvoicesLines)
            {
                InvoicesLinesForSelect.Add(invoiceLineItem);
            }

            ddInvoices.ItemDisplayBinding = new Binding("display_name");
            ddInvoices.ItemsSource = Invoices;
            ddInvoices.SelectedItem = Invoices[0];

            ddInvoicesLines.ItemDisplayBinding = new Binding(nameof(account_move_line_view.display_name));
            ddInvoicesLines.ItemsSource = InvoicesLinesForSelect;
            ddInvoicesLines.SelectedItem = InvoicesLinesForSelect.FirstOrDefault();

            IsLoadingData = false;
        });
    }

    private async Task LoadAccountMoveSendLines()
    {
        IsLoadingDocs = true;
        //OnPropertyChanged(nameof(IsLoadingDocs));
        _creditNoteReqDetails_items = new ObservableCollection<credit_note_request_detail>();

        if (creditNoteRequest != null)
        {
            if (creditNoteRequest.lines == null)
            {
                IsLoadingDocs = false;
                return;
            }
        }
        else
        {
            IsLoadingDocs = false;
            return;
        }

        List<credit_note_request_detail> linesForAdd = creditNoteRequest.lines.ToList();
        var productIds = linesForAdd.Select(x => x.product_id).ToList();
        var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
        var products = await productDb.GetItemsAsync(x => productIds.Contains(x.id));
        var account_moves_ids = linesForAdd.Select(x => x.move_id).ToList();
        var accountMoveDb = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
        var accountMoves = await accountMoveDb.GetItemsAsync(x => account_moves_ids.Contains(x.id));

        foreach (var lineItem in linesForAdd)
        {
            var product_display_name = products.FirstOrDefault(x => x.id == lineItem.product_id)?.display_name;
            var docnum_mask = accountMoves.FirstOrDefault(x => x.id == lineItem.move_id)?.docnum_mask;
            var invoice_date = accountMoves.FirstOrDefault(x => x.id == lineItem.move_id)?.invoice_date;

            lineItem.display_name = product_display_name;
            lineItem.docnum_mask = docnum_mask;
            lineItem.invoice_date = invoice_date;
            lineItem.invoice_header = CreditNoteUomDisplayHelper.BuildInvoiceHeader(docnum_mask, invoice_date);
        }

        await CreditNoteUomDisplayHelper.EnrichLinesPricingFromInvoiceAsync(
            linesForAdd,
            App.Session.odooConnection.DbNameSqlite);

        await CreditNoteUomDisplayHelper.EnrichLinesAsync(
            linesForAdd,
            App.Session.odooConnection.DbNameSqlite,
            ClientPermanentlyClosingForUom);

        CreditNoteUomDisplayHelper.SyncQuantityInvoicedFromAvailable(linesForAdd);

        _creditNoteReqDetails_items = new ObservableCollection<credit_note_request_detail>(linesForAdd);
        collectionView.ItemsSource = _creditNoteReqDetails_items;
        IsLoadingDocs = false;
        //OnPropertyChanged(nameof(IsLoadingDocs));
        OnPropertyChanged(nameof(detailsCount));
    }

    private async Task RefreshPartnerFromLocalDbAsync()
    {
        if (_res_partner == null || _res_partner.id <= 0)
            return;

        var partnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
        var freshPartner = await partnerDb.GetItem(_res_partner.id);
        if (freshPartner != null)
            _res_partner = freshPartner;

        OnPropertyChanged(nameof(ShowClientPermanentlyClosingBanner));
    }

    private async void pickerModulos_SelectedIndexChanged(object sender, EventArgs e)
    {
        Debug.WriteLine("pickerModulos");
        Debug.WriteLine(pickerModulos.SelectedIndex);

        if (pickerModulos.SelectedItem != null)
        {
            var modulo_seleccionado = (TypeParentNc)pickerModulos.SelectedItem;
            var database = new TypeNcDb(App.Session.odooConnection.DbNameSqlite);
            accountTypeModules = (await database.GetItemsAsync(x => x._parent_id == modulo_seleccionado.id)).ToArray();

            if (accountTypeModules.Length == 0)
            {
                return;
            }

            pickerTipoNc.ItemsSource = accountTypeModules;
            pickerTipoNc.ItemDisplayBinding = new Binding(nameof(TypeNc.name));
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
        if (Icon.Name == "invoice")
        {
            await LoadItemsMode1(withQuantity);
        }
        else
        {
            if (Icon.Name == "invoices")
            {
                await AutofillAllQty();
            }
        }
    }

    private async Task AutofillAllQty()
    {
        IsLoadingDocs = true;
        if (_creditNoteReqDetails_items == null)
        {
            IsLoadingDocs = false;
            return;
        }

        foreach (var item in _creditNoteReqDetails_items)
        {
            item.quantity = item.quantity_available;
        }

        //collectionView.ItemsSource = _creditNoteReqDetails_items;
        OnPropertyChanged(nameof(_creditNoteReqDetails_items));
        OnPropertyChanged(nameof(detailsCount));
        IsLoadingDocs = false;

    }

    private async Task LoadItemsMode1(bool withQuantity)
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

        if (!HasValidInvoiceSelected())
        {
            await DisplayAlertAsync(
                "Atención",
                "Por favor, seleccione una factura antes de agregar productos",
                "Aceptar");
            return;
        }

        //if(txtItem.Text == null || txtItem.Text.Length < 3)
        //{
        //    await Toast.Make("Debe colocar al menos 3 caracteres en el filtro para poder realizar la búsqueda").Show();
        //    return;
        //}

        var empresa = (res_company)SelectorCmp.SelectedItem;
        default_res_company = empresa;

        IsLoadingDocs = true;

        AccountMoveLineDb database = new AccountMoveLineDb(App.Session.odooConnection.DbNameSqlite);
        var result = await database.GetItemsByParentAsync(_accountMoveSelected.id);
        result = result.Where(x => x.display_type == "product").ToList();
        List<credit_note_request_detail> result_send = new List<credit_note_request_detail>();

        var productIds = result.Select(x => x._product_id).ToList();

        var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
        var products = await productDb.GetItemsAsync(x => productIds.Contains(x.id));

        float tax_amount = 0;

        var taxDb = new AccountTaxDb(App.Session.odooConnection.DbNameSqlite);
        var taxes = await taxDb.GetItemsAsync(x => x._company_id == default_res_company.id);
        var tax_amounts = taxes.ToDictionary(x => x.id, x => x.amount);

        var hubAccountMoveLine = new HubAccountMoveLine(App.Session);
        var resultUpdateItems = await hubAccountMoveLine.GetAccountMoveLinesByMove(_accountMoveSelected.id);

        account_move_line[] server_movelines = null;

        if (resultUpdateItems != null && resultUpdateItems.result != null)
        {
            server_movelines = resultUpdateItems.result;
        }

        foreach (var item in result)
        {
            Debug.WriteLine($"Cantidad disponible {item.quantity_available}");
            if (item.quantity_available < 1)
            {
                //item.quantity_available = 50;
                continue;
            }

            if (item.discount == 0 && item.tax_ids_json == "[]")
            {
                var resultUpdateItem = server_movelines.Where(x => x.id == item.id).FirstOrDefault();
                if (resultUpdateItem != null && item.discount != resultUpdateItem.discount)
                {
                    item.discount = resultUpdateItem.discount;
                    await database.UpdateAsync(item);
                }
            }

            if (item._tax_ids != null && item._tax_ids.Length > 0)
            {
                var taxId = item._tax_ids[0];
                if (tax_amounts.ContainsKey(taxId))
                {
                    tax_amount = tax_amounts[taxId];
                }
            }

            credit_note_request_detail creditNoteRequestDetail_Send = new credit_note_request_detail();
            creditNoteRequestDetail_Send.price_unit = item.price_unit;
            creditNoteRequestDetail_Send.price_return = item.price_unit;

            if (tax_amount > 0)
            {
                creditNoteRequestDetail_Send.siv_price_unit = Math.Round(item.price_unit / (1 + ((decimal)tax_amount / 100)), 4, MidpointRounding.AwayFromZero);
                creditNoteRequestDetail_Send.siv_price_return = Math.Round(item.price_unit / (1 + ((decimal)tax_amount / 100)), 4, MidpointRounding.AwayFromZero);
            }
            else
            {
                creditNoteRequestDetail_Send.siv_price_unit = item.price_unit;
                creditNoteRequestDetail_Send.siv_price_return = item.price_unit;
            }

            creditNoteRequestDetail_Send.price_total = item.price_total;
            creditNoteRequestDetail_Send.price_subtotal = item.price_subtotal;
            creditNoteRequestDetail_Send.quantity_available_base = item.quantity_available_base;
            creditNoteRequestDetail_Send.quantity_available_invoice = item.quantity_available;
            creditNoteRequestDetail_Send.quantity_available = item.quantity_available;
            creditNoteRequestDetail_Send.quantity_invoiced = item.quantity_available;
            creditNoteRequestDetail_Send.original_quantity = item.quantity_available;
            creditNoteRequestDetail_Send.docnum_mask = _accountMoveSelected.docnum_mask;
            creditNoteRequestDetail_Send.invoice_date = _accountMoveSelected.invoice_date;
            creditNoteRequestDetail_Send.invoice_line_uom_id = item._product_uom_id;
            creditNoteRequestDetail_Send.product_uom_id = item._product_uom_id;
            creditNoteRequestDetail_Send.discount_balance = item.discount_balance;
            creditNoteRequestDetail_Send.discount = item.discount;
            creditNoteRequestDetail_Send.discount_percentage = item.discount;
            creditNoteRequestDetail_Send.analitica_id = item._analitica_id;
            creditNoteRequestDetail_Send.tax_ids_json = item.tax_ids_json;

            creditNoteRequestDetail_Send.quantity = 0;

            creditNoteRequestDetail_Send.account_id = item._account_id;
            creditNoteRequestDetail_Send.product_id = item._product_id;
            creditNoteRequestDetail_Send.name = item.name;
            creditNoteRequestDetail_Send.currency_id = 2;
            creditNoteRequestDetail_Send.move_id = item._move_id;
            creditNoteRequestDetail_Send.line_id = item.id;
            creditNoteRequestDetail_Send.display_type = string.IsNullOrWhiteSpace(item.display_type)
                ? "product"
                : item.display_type;
            creditNoteRequestDetail_Send.partner_id = _res_partner?.id ?? _accountMoveSelected._partner_id;
            creditNoteRequestDetail_Send.amount_currency = item.price_subtotal;
            creditNoteRequestDetail_Send.disc_amount = item.discount_balance;

            creditNoteRequestDetail_Send.display_name = products.FirstOrDefault(x => x.id == item._product_id)?.display_name;
            creditNoteRequestDetail_Send.invoice_header = CreditNoteUomDisplayHelper.BuildInvoiceHeader(
                creditNoteRequestDetail_Send.docnum_mask,
                creditNoteRequestDetail_Send.invoice_date);

            result_send.Add(creditNoteRequestDetail_Send);
        }

        await CreditNoteUomDisplayHelper.EnrichLinesAsync(
            result_send,
            App.Session.odooConnection.DbNameSqlite,
            ClientPermanentlyClosingForUom);

        CreditNoteUomDisplayHelper.SyncQuantityInvoicedFromAvailable(result_send);

        if (withQuantity)
        {
            foreach (var line in result_send)
                line.quantity = line.quantity_available;
        }

        _creditNoteReqDetails_items = new ObservableCollection<credit_note_request_detail>(result_send);
        collectionView.ItemsSource = _creditNoteReqDetails_items;
        OnPropertyChanged(nameof(detailsCount));
        IsLoadingDocs = false;

        //await UITools.HideLoadingPopup();
        Debug.WriteLine("Terminada la carga de datos!");
    }

    private async Task LoadItemsMode2(bool withQuantity, List<account_move_line> linesForAdd)
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

        var empresa = (res_company)SelectorCmp.SelectedItem;
        default_res_company = empresa;

        IsLoadingDocs = true;

        var productIds = linesForAdd.Select(x => x._product_id).ToList();
        var account_moves_ids = linesForAdd.Select(x => x._move_id).ToList();

        var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
        var products = await productDb.GetItemsAsync(x => productIds.Contains(x.id));

        var accountMoveDb = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
        var accountMoves = await accountMoveDb.GetItemsAsync(x => account_moves_ids.Contains(x.id));

        float tax_amount = 0;

        var taxDb = new AccountTaxDb(App.Session.odooConnection.DbNameSqlite);
        var taxes = await taxDb.GetItemsAsync(x => x._company_id == default_res_company.id);
        var tax_amounts = taxes.ToDictionary(x => x.id, x => x.amount);

        AccountMoveLineDb databaseAML = new AccountMoveLineDb(App.Session.odooConnection.DbNameSqlite);

        var linesIds = linesForAdd.Select(x => x.id).ToList();
        var hubAccountMoveLine = new HubAccountMoveLine(App.Session);
        var resultUpdateItems = await hubAccountMoveLine.GetAccountMoveLineByIds(linesIds.ToArray(), 100, 0);

        account_move_line[] server_movelines = null;

        if (resultUpdateItems != null && resultUpdateItems.result != null)
        {
            server_movelines = resultUpdateItems.result;
        }

        foreach (var item in linesForAdd)
        {
            var product_display_name = products.FirstOrDefault(x => x.id == item._product_id)?.display_name;
            var docnum_mask = accountMoves.FirstOrDefault(x => x.id == item._move_id)?.docnum_mask;
            var invoice_date = accountMoves.FirstOrDefault(x => x.id == item._move_id)?.invoice_date;

            Debug.WriteLine($"Cantidad disponible {item.quantity_available}");
            if (item.quantity_available < 1)
            {
                //item.quantity_available = 50;
                continue;
            }

            if (item.discount == 0 && item.tax_ids_json == "[]")
            {
                var resultUpdateItem = server_movelines.Where(x => x.id == item.id).FirstOrDefault();
                if (resultUpdateItem != null && item.discount != resultUpdateItem.discount)
                {
                    var account_Move_Line_For_Update = (await databaseAML.GetItemsAsync(x => x.id == item.id)).FirstOrDefault();
                    if (account_Move_Line_For_Update != null)
                    {
                        account_Move_Line_For_Update.discount = resultUpdateItem.discount;
                        await databaseAML.UpdateAsync(account_Move_Line_For_Update);

                        item.discount = resultUpdateItem.discount;
                    }
                }
            }

            //buscamos si existe previamente para no agregar duplicado  
            var existingItem = _creditNoteReqDetails_items.FirstOrDefault(x => x.line_id == item.id);
            if (existingItem != null)
            {
                await Toast.Make("Linea ya existe " + product_display_name).Show();
                continue;
            }

            if (item._tax_ids != null && item._tax_ids.Length > 0)
            {
                var taxId = item._tax_ids[0];
                if (tax_amounts.ContainsKey(taxId))
                {
                    tax_amount = tax_amounts[taxId];
                }
            }

            credit_note_request_detail creditNoteRequestDetail_Send = new credit_note_request_detail();

            creditNoteRequestDetail_Send.price_unit = item.price_unit;
            creditNoteRequestDetail_Send.price_return = item.price_unit;

            if (tax_amount > 0)
            {
                creditNoteRequestDetail_Send.siv_price_unit = Math.Round(item.price_unit / (1 + ((decimal)tax_amount / 100)), 4, MidpointRounding.AwayFromZero);
                creditNoteRequestDetail_Send.siv_price_return = Math.Round(item.price_unit / (1 + ((decimal)tax_amount / 100)), 4, MidpointRounding.AwayFromZero);
            }
            else
            {
                creditNoteRequestDetail_Send.siv_price_unit = item.price_unit;
                creditNoteRequestDetail_Send.siv_price_return = item.price_unit;
            }

            creditNoteRequestDetail_Send.price_total = item.price_total;
            creditNoteRequestDetail_Send.price_subtotal = item.price_subtotal;
            creditNoteRequestDetail_Send.quantity_available_base = item.quantity_available_base;
            creditNoteRequestDetail_Send.quantity_available_invoice = item.quantity_available;
            creditNoteRequestDetail_Send.quantity_available = item.quantity_available;
            creditNoteRequestDetail_Send.quantity_invoiced = item.quantity_available;
            creditNoteRequestDetail_Send.original_quantity = item.quantity_available;
            creditNoteRequestDetail_Send.docnum_mask = docnum_mask;
            creditNoteRequestDetail_Send.invoice_date = invoice_date;
            creditNoteRequestDetail_Send.invoice_line_uom_id = item._product_uom_id;
            creditNoteRequestDetail_Send.product_uom_id = item._product_uom_id;
            creditNoteRequestDetail_Send.discount_balance = item.discount_balance;
            creditNoteRequestDetail_Send.discount = item.discount;
            creditNoteRequestDetail_Send.discount_percentage = item.discount;
            creditNoteRequestDetail_Send.analitica_id = item._analitica_id;
            creditNoteRequestDetail_Send.tax_ids_json = item.tax_ids_json;

            creditNoteRequestDetail_Send.quantity = 0;

            creditNoteRequestDetail_Send.account_id = item._account_id;
            creditNoteRequestDetail_Send.product_id = item._product_id;
            creditNoteRequestDetail_Send.name = item.name;
            creditNoteRequestDetail_Send.currency_id = 2;
            creditNoteRequestDetail_Send.move_id = item._move_id;
            creditNoteRequestDetail_Send.line_id = item.id;
            creditNoteRequestDetail_Send.display_type = string.IsNullOrWhiteSpace(item.display_type)
                ? "product"
                : item.display_type;
            creditNoteRequestDetail_Send.partner_id = _res_partner?.id ?? 0;
            creditNoteRequestDetail_Send.amount_currency = item.price_subtotal;
            creditNoteRequestDetail_Send.disc_amount = item.discount_balance;
            creditNoteRequestDetail_Send.display_name = product_display_name;
            creditNoteRequestDetail_Send.invoice_header = CreditNoteUomDisplayHelper.BuildInvoiceHeader(
                creditNoteRequestDetail_Send.docnum_mask,
                creditNoteRequestDetail_Send.invoice_date);

            await CreditNoteUomDisplayHelper.EnrichLinesAsync(
                new[] { creditNoteRequestDetail_Send },
                App.Session.odooConnection.DbNameSqlite,
                ClientPermanentlyClosingForUom);

            CreditNoteUomDisplayHelper.SyncQuantityInvoicedFromAvailable(new[] { creditNoteRequestDetail_Send });

            if (withQuantity)
                creditNoteRequestDetail_Send.quantity = creditNoteRequestDetail_Send.quantity_available;

            _creditNoteReqDetails_items.Add(creditNoteRequestDetail_Send);
        }

        collectionView.ItemsSource = _creditNoteReqDetails_items;
        OnPropertyChanged(nameof(detailsCount));
        IsLoadingDocs = false;

        //await UITools.HideLoadingPopup();
        Debug.WriteLine("Terminada la carga de datos!");
    }

    private async void btnDelete_Clicked(object sender, EventArgs e)
    {
        Debug.WriteLine("Quitar");

        Button button = (Button)sender;
        var item = (account_move_line)button.CommandParameter;

        Debug.WriteLine(item.name);
    }

    private bool HasValidInvoiceSelected()
        => _accountMoveSelected != null && _accountMoveSelected.id > 0;

    private async void btnSave_Clicked(object sender, EventArgs e)
    {
        TypeParentNc selected_module = null;
        TypeNc selected_type_module = null;

        if (editionMode)
        {

        }

        if (pickerModulos.SelectedItem != null)
        {
            selected_module = (TypeParentNc)pickerModulos.SelectedItem;
        }

        if (pickerTipoNc.SelectedItem != null)
        {
            selected_type_module = (TypeNc)pickerTipoNc.SelectedItem;
        }

        if (selected_module == null)
        {
            await DisplayAlertAsync(
                "Atención",
                "Por favor, seleccione el tipo de módulo antes de guardar",
                "Aceptar");
            return;
        }

        if (selected_type_module == null)
        {
            await DisplayAlertAsync(
              "Atención",
              "Por favor, seleccione un tipo de nota de crédito antes de guardar",
              "Aceptar");
            return;
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
            await DisplayAlertAsync(
             "Atención",
             "Por favor, seleccione un cliente antes de guardar",
             "Aceptar");
            return;
        }

        if (Icon.Name == "invoice" && !HasValidInvoiceSelected())
        {
            await DisplayAlertAsync(
                "Atención",
                "Por favor, seleccione una factura antes de guardar",
                "Aceptar");
            return;
        }

        if (_creditNoteReqDetails_items == null)
        {
            await DisplayAlertAsync(
               "Atención",
               "Por favor, seleccione al menos un producto antes de guardar",
               "Aceptar");
            return;
        }

        if (_creditNoteReqDetails_items != null && _creditNoteReqDetails_items.Count == 0)
        {
            await DisplayAlertAsync(
                "Atención",
                "Por favor, seleccione al menos un producto antes de guardar",
                "Aceptar");
            return;
        }


        if (txtReason.Text == null || txtReason.Text.Trim().Length == 0)
        {
            await DisplayAlertAsync(
               "Atención",
               "Por favor, ingrese texto en el campo de Motivo Retorno",
               "Aceptar");
            return;
        }

        List<credit_note_request_detail> _account_move_line_send_list = new List<credit_note_request_detail>();

        var empresa = (res_company)SelectorCmp.SelectedItem;

        int rowItem = 0;

        foreach (var itemDet in _creditNoteReqDetails_items)
        {
            if (itemDet.quantity <= 0)
                continue;

            await CreditNoteUomDisplayHelper.PrepareLineForLocalStorageAsync(
                itemDet,
                App.Session.odooConnection.DbNameSqlite,
                ClientPermanentlyClosingForUom);

            var lineForStorage = JsonConvert.DeserializeObject<credit_note_request_detail>(
                JsonConvert.SerializeObject(itemDet));
            CreditNoteUomDisplayHelper.CopyPersistedLocalFields(itemDet, lineForStorage);
            _account_move_line_send_list.Add(lineForStorage);
            rowItem++;
        }

        if (rowItem == 0)
        {
            await DisplayAlertAsync(
               "Atención",
               "No ha ingresado ninguna cantidad en devolución, debe ingresar al menos una para poder proceder.",
               "Aceptar");
            return;
        }

        if (_account_move_line_send_list.Count() > 0)
        {
            DateTime fechaActual = DateTime.Now;

            if (creditNoteRequest == null)
            {
                creditNoteRequest = new credit_note_request();
                creditNoteRequest.parent_id = 0;
            }
            else
            {

            }

            //creditNoteRequest.build_mode = "factura";
            creditNoteRequest.company_id = empresa.id;
            creditNoteRequest.create_date = fechaActual;
            creditNoteRequest.create_uid = App.Session.CurrentUser.uid;
            creditNoteRequest.external_create_uid = App.Session.CurrentUserFront.uid;
            creditNoteRequest.external_guid = Guid.NewGuid().ToString("N");
            creditNoteRequest.request_date = fechaActual;
            creditNoteRequest.partner_id = _res_partner.id;
            creditNoteRequest.partner_name = _res_partner.name;
            creditNoteRequest.partner_email = _res_partner.email;
            creditNoteRequest.doc_status = "pending";
            creditNoteRequest.move_type = "out_invoice";
            creditNoteRequest.move_type_nc = "devolucion";
            creditNoteRequest.tipo_nc = "out_refund";

            //Define el tipo
            creditNoteRequest.request_type = Icon.Name;

            if (creditNoteRequest.request_type == "invoice")
            {
                creditNoteRequest.mainAccountMove = _accountMoveSelected.id;
                creditNoteRequest._ref = _accountMoveSelected.docnum_mask;
                creditNoteRequest.title = _accountMoveSelected.name;
            }
            else
            {
                creditNoteRequest.mainAccountMove = null;
                creditNoteRequest._ref = null;
                creditNoteRequest.title = null;
            }

            //creditNoteRequest.reversed_entry_id = _accountMoveSelected.id;            
            creditNoteRequest.journal_id = empresa.credit_note_journal_id_;
            creditNoteRequest.center_id = App.Session.res_center.id;

            var resCenterLineDb = new ResCenterLineDb(App.Session.odooConnection.DbNameSqlite);
            var res_center_line = (await resCenterLineDb.GetItemsAsync(x => x.center_id_ == App.Session.res_center.id)).FirstOrDefault();

            if (res_center_line == null)
            {

            await DisplayAlertAsync(
              "Atención",
              "No se encontró configuración de centro para el centro actual.",
              "Aceptar");
                return;
            }

            int res_center_line_id = res_center_line.id;

            var docAuthorizationLineDb = new DocAuthorizationLineDb(App.Session.odooConnection.DbNameSqlite);
            var docAuthorizationLine = (await docAuthorizationLineDb.GetItemsAsync(
                x => x.center_id_ == App.Session.res_center.id
                && x.document_type_id_ == 4)).FirstOrDefault();

            if (docAuthorizationLine == null)
            {
                await DisplayAlertAsync(
                  "Atención",
                  "No se encontró configuración de autorización para el centro actual.",
                  "Aceptar");
                return;
            }

            int doc_authorization_line_id = docAuthorizationLine.id;

            var taxSustentoDb = new DocTaxSustentDb(App.Session.odooConnection.DbNameSqlite);
            var taxSustento = (await taxSustentoDb.GetItemsAsync(x => x.code == "01")).FirstOrDefault();

            if (taxSustento == null)
            {
                await DisplayAlertAsync(
                  "Atención",
                  "No se encontró configuración de sustento para el centro actual.",
                  "Aceptar");
                return;
            }

            int tax_sustento_id = taxSustento.id;

            creditNoteRequest.res_center_line_id = res_center_line_id;
            creditNoteRequest.doc_authorization_line_id = doc_authorization_line_id;
            creditNoteRequest.doc_tax_sustent_id = tax_sustento_id;
            creditNoteRequest.parent_nc_id = selected_module.id;
            creditNoteRequest.type_module_id = selected_type_module.id;
            creditNoteRequest.display_parent_nc = selected_module.name;
            creditNoteRequest.display_type_module = selected_type_module.name;
            creditNoteRequest.group_status = "open";
            creditNoteRequest.reason = txtReason.Text;
            creditNoteRequest.state = "draft";
            creditNoteRequest.state = "intro";

            int parent_id = 0;
            parent_id = creditNoteRequest.id;

            foreach (var line_item in _account_move_line_send_list)
            {
                line_item.parent_id = parent_id;
            }

            creditNoteRequest.lines = _account_move_line_send_list.ToArray();
        }

        saveData = true;
        await Navigation.PopAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        SendBackButtonPressed();
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
        _creditNoteReqDetails_items = new ObservableCollection<credit_note_request_detail>();
        collectionView.ItemsSource = _creditNoteReqDetails_items.ToArray();
        OnPropertyChanged(nameof(detailsCount));
    }

    private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is RadioButton radioButton && radioButton.IsChecked)
        {
            var valorSeleccionado = radioButton.Value;
            TypeSearch = valorSeleccionado.ToString();
            Debug.WriteLine("Radio:" + TypeSearch);
        }
    }

    private async void chkSelectAll_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        await ReturnAllItem(e.Value);
        Debug.WriteLine(e.Value.ToString());
    }

    [RelayCommand]
    private async Task SelectionChanged(object parameter)
    {
        Debug.WriteLine("SelectionChanged");
        Debug.WriteLine(Icon);

        if (Icon.Name == "invoice")
        {
            ddInvoices.IsVisible = true;
            ddInvoicesLines.IsVisible = false;
        }
        else
        {
            ddInvoices.IsVisible = false;
            ddInvoicesLines.IsVisible = true;
        }
    }
}