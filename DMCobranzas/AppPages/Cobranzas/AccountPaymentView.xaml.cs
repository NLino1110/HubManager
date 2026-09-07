using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using DMCobranzas.Settings.helpers;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Controls.Popups;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.DebitCollection;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using DMSA.Sync.Core.Update;
using Microsoft.Maui.Layouts;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace DMCobranzas.Controls.Modals.TabbedPages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AccountPaymentView : ContentPage
{    
    public bool ClosingSaved { get; set; } = false;
    public res_company Sel_Company_Id { get; set; }
    public res_partner Sel_Res_Partner { get; set; } 
    public decimal totalPagado { get; set; }
    public decimal totalAplicado { get; set; }
    public bool isWindows { get; set; } = false;
    public bool editionMode { get; set; } = false;
    public bool isFirstLoad { get; set; } = true;
    public MultipleCobrosInvoiceLine[] accountPayments { get; set; } = new MultipleCobrosInvoiceLine[0];
    public ICommand EditItemCommand { get; set; }

    public static readonly BindableProperty _cobReciboCabProperty =
            BindableProperty.Create(nameof(Sel_MultipleCobrosInvoice), typeof(MultipleCobrosInvoice), typeof(AccountPaymentView));

    public MultipleCobrosInvoice Sel_MultipleCobrosInvoice
    {
        get => (MultipleCobrosInvoice)GetValue(_cobReciboCabProperty);
        set => SetValue(_cobReciboCabProperty, value);
    }

    public ICommand SeleccionarClienteCommand { get; set; }
    public ICommand VerSaldosCommand { get; set; }
    public ICommand AddCobroCommand { get; set; }
    public ICommand DownloadDataCommand { get; set; }

    public AccountPaymentView()
	{
		InitializeComponent();
        EditItemCommand = new Command(EditItem);
        DeleteItemCommand = new Command(DeleteItem);           
        isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;
        this.popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);

        SeleccionarClienteCommand = new Command(async () =>
        {
            HandleReturnResultPopupButtonClicked(null, null);
        });

        VerSaldosCommand = new Command(async () =>
        {
            ResultPopupAccountMove(null, null);
        });

        AddCobroCommand = new Command(async () =>
        {
            btnAddAccountPayment_Clicked(null, null);
        });

        DownloadDataCommand = new Command(async () =>
        {
            DownloadDataCustomer(null, null);
        });

        BindingContext = this;
    }

    void OnEntryTapped(object sender, EventArgs e)
    {
        HandleReturnResultPopupButtonClicked(sender, e);
        Console.WriteLine("Entry tapped!");
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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        IDispatcherTimer timer;

        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.IsRepeating = false;
        timer.Tick += async (s, e) =>
        {
            await LoadData();
            
            timer.Stop();
            timer.IsRepeating = false;
            Debug.WriteLine("Formas de cobro cargados.......");
        };
        timer.Start();
    }

    private async Task<bool> EnsureReceiptIsEditableAsync(string actionDescription)
    {
        if (!editionMode || Sel_MultipleCobrosInvoice == null)
            return true;

        if (CobrosEstados.CanEdit(Sel_MultipleCobrosInvoice.payment_status))
            return true;

        await DisplayAlertAsync(
            "Atención",
            $"Solo se puede {actionDescription} cuando el cobro está en estado PENDIENTE o ERROR. Estado actual: {Sel_MultipleCobrosInvoice.payment_status}.",
            "Aceptar");
        return false;
    }

    private async void btnAddAccountPayment_Clicked(object sender, EventArgs e)
    {
        if (!await EnsureReceiptIsEditableAsync("modificar el cobro"))
            return;

        if(Sel_Res_Partner == null)
        {
            await DisplayAlertAsync(
            "Confirmación requerida",
            "Es necesario seleccionar un cliente para generar cobros.",
            "Aceptar");
            return;
      
        }

        AccountPaymentCrud obj = new AccountPaymentCrud(Sel_Res_Partner);
        obj.Sel_Company_Id = Sel_Company_Id;
        obj.multipleCobrosInvoice = Sel_MultipleCobrosInvoice;
        obj.isNewData = true;
        obj.Disappearing += accountPaymentCrud_Disappearing;
        await Navigation.PushAsync(obj, false);       

    }

    private async void EditItem(object objParam)
    {
        Debug.WriteLine("EditItem");

        if (!await EnsureReceiptIsEditableAsync("editar formas de cobro"))
            return;

        AccountPaymentCrud obj = new AccountPaymentCrud(Sel_Res_Partner);
        obj.isNewData = false;
        obj.multipleCobrosInvoiceLine = (MultipleCobrosInvoiceLine)objParam;
        obj.itemIndex = accountPayments.ToList().IndexOf(obj.multipleCobrosInvoiceLine);
        obj.multipleCobrosInvoice = Sel_MultipleCobrosInvoice;

        obj.Disappearing += accountPaymentCrud_Disappearing;
        await Navigation.PushAsync(obj, false);
    }

    public ICommand DeleteItemCommand { get; set; }

    private async void DeleteItem(object objParam)
    {
        Debug.WriteLine("DeleteItem");

        if (!await EnsureReceiptIsEditableAsync("eliminar formas de cobro"))
            return;

        bool answer = await DisplayAlert("Eliminar", "Está seguro que desea eliminar este abono?", "Eliminar", "Cancelar");
        
        if (answer)
        {
            var nList = accountPayments.ToList();
            int itemIndex = nList.IndexOf((MultipleCobrosInvoiceLine)objParam);            
            nList.RemoveAt(itemIndex);
            
            accountPayments = nList.ToArray();
            await LoadData();
            return;
        }
    }

    public static T FindClosestObject<T>(List<T> objects, T targetObject)
    {
        // Lista de propiedades públicas del tipo de objeto
        var properties = typeof(T).GetProperties();

        // Calcular la distancia entre cada objeto y el objeto objetivo
        var distances = objects.Select(obj =>
        {
            double distance = 0;

            foreach (var property in properties)
            {
                var targetValue = property.GetValue(targetObject);
                var objectValue = property.GetValue(obj);

                if (targetValue != null && objectValue != null && targetValue.Equals(objectValue))
                {
                    distance++;
                }
            }

            return new { Object = obj, Distance = distance };
        });

        // Obtener el objeto más cercano o null si no se encontró ninguno
        var closestObject = distances.OrderByDescending(d => d.Distance).FirstOrDefault();

        if (closestObject != null && closestObject.Distance > 0)
        {
            return closestObject.Object;
        }

        return default(T);
    }

    public decimal FnToDecimal(string valor)
    {
        if (decimal.TryParse(valor, out decimal numero))
        {
            return numero;
        }
        else
        {
            return 0; // Devuelve el valor original si no es un número válido
        }
    }

    private void accountPaymentCrud_Disappearing(object sender, EventArgs e)
    {
        Debug.WriteLine("Cobro cerrado");
        Debug.WriteLine(((AccountPaymentCrud)sender).saveData);        

        if (((AccountPaymentCrud)sender).saveData)
        {
            if (((AccountPaymentCrud)sender).isNewData)
            {
                var task = Task.Run(async () =>
                {                    
                    MultipleCobrosInvoiceLine cobReciboDet = new MultipleCobrosInvoiceLine();                    
                    cobReciboDet = ((AccountPaymentCrud)sender).multipleCobrosInvoiceLine;
                    var nList = accountPayments.ToList();
                    nList.Add(cobReciboDet);
                    accountPayments = nList.ToArray();
                    Debug.WriteLine("Guardado nuevo");
                });
                Task.WaitAll(task);
            }
            else
            {
                var task = Task.Run(async () =>
                {
                    var nList = accountPayments.ToList();

                    MultipleCobrosInvoiceLine cobReciboDet = new MultipleCobrosInvoiceLine();
                    cobReciboDet = ((AccountPaymentCrud)sender).multipleCobrosInvoiceLine;

                    nList[((AccountPaymentCrud)sender).itemIndex] = ((AccountPaymentCrud)sender).multipleCobrosInvoiceLine;
                    accountPayments = nList.ToArray();
                    Debug.WriteLine("Guardado cambios");
                });
                Task.WaitAll(task);
            }
        }

        LoadData();
    }

    private async Task SummaryPayments()
    {
        totalPagado = 0;

        int idItem = 0;
        foreach (var di in accountPayments)
        {
            idItem++;
            decimal _totalPagado = (decimal) di.Amount;
            Debug.WriteLine(_totalPagado);
            totalPagado += _totalPagado; // ParseTool.StringToDecimal(item.valor);
        }

        collectionView.ItemsSource = accountPayments;
        txtTotalPagado.Text = "TOTAL COBRO: $ " + totalPagado.ToString(App.Session.ApplicationCultureInfo);        
    }

    private async Task LoadData()
    {
        if (editionMode && isFirstLoad && Sel_MultipleCobrosInvoice != null
            && !CobrosEstados.CanEdit(Sel_MultipleCobrosInvoice.payment_status))
        {
            await DisplayAlertAsync(
                "Atención",
                $"Este cobro no se puede editar porque no está en estado PENDIENTE o ERROR. Estado actual: {Sel_MultipleCobrosInvoice.payment_status}.",
                "Aceptar");
            await Navigation.PopAsync();
            return;
        }

        if ((accountPayments == null || accountPayments.Length == 0) && editionMode && isFirstLoad)
        {
            if (Sel_Res_Partner == null)
            {
                ResPartnerDb resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);                
                Sel_Res_Partner = await resPartnerDb.GetItemsAsync(Sel_MultipleCobrosInvoice.company_id, Sel_MultipleCobrosInvoice.partner_id);
                txtCliente.Text = Sel_Res_Partner.id + "-" + Sel_Res_Partner.name;
            }

            if (Sel_Company_Id == null)
            {
                var cemp = App.Session.CurrentUserFront.empresas.Where(c => c.id == Sel_MultipleCobrosInvoice.company_id).FirstOrDefault();

                Sel_Company_Id = new res_company
                {
                    id = Sel_MultipleCobrosInvoice.company_id,
                    name = cemp.name
                };

                Title = "" + Sel_Company_Id.name;
            }

            lblReceiptReceipt.Text = Sel_MultipleCobrosInvoice.receipt_receipts_id.ToString();

            if (Sel_MultipleCobrosInvoice != null)
            {
                //Si entra en modo edición se bloquea
                GridPartner.IsEnabled = false;
                var accountPaymentDb = new MultipleCobrosInvoiceLineDb(App.Session.odooConnection.DbNameSqlite);
                var ls_accountPayments = await accountPaymentDb.GetItemsAsync(x=>x.MultipleCobrosInvoiceId == Sel_MultipleCobrosInvoice.id);
                accountPayments = ls_accountPayments.ToArray();

                var accountPaymentLines = new MultipleCobrosInvoiceLineAiDb(App.Session.odooConnection.DbNameSqlite);

                foreach (var accountPayment in accountPayments)
                {
                    var apl = await accountPaymentLines.GetItemsAsync( x=>x.multiple_cobros_invoice_line_id == accountPayment.Id);

                    if (apl.Count() > 0)
                    {
                        accountPayment.lines = apl.ToArray();
                    }
                }
            }            
        }
        else
        {            
            int idItem = 0;
            foreach (var di in accountPayments)
            {
                idItem++;
                di.sequence = idItem;
            }

            Title = "" + Sel_Company_Id.name;            
        }

        isFirstLoad = false;

        if (accountPayments!=null)
            collectionView.ItemsSource = accountPayments;

        await SummaryPayments();
    }

    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");
        
        if (!answer)
        {
            return;
        }
        await Navigation.PopAsync();
    }

    private async void btnSave_Clicked(object sender, EventArgs e)
    {
        Debug.WriteLine("Guardando ...");

        if (!await EnsureReceiptIsEditableAsync("guardar cambios"))
            return;

        if (accountPayments == null || accountPayments.Length == 0 || totalPagado <= 0)
        {
            await DisplayAlertAsync(
                "Atención",
                "No hay cobros en la lista. Debe agregar al menos un cobro antes de guardar.",
                "Aceptar");
            return;
        }

        var database = new MultipleCobrosInvoiceDb(App.Session.odooConnection.DbNameSqlite);
        DateTime fechaActual = DateTime.Now;

        if (editionMode)
        {
            
        }

        if (Sel_MultipleCobrosInvoice != null)
        {
            MultipleCobrosInvoice multipleCobrosInvoice = new MultipleCobrosInvoice();
            multipleCobrosInvoice.company_id = Sel_MultipleCobrosInvoice.company_id;

            //Update
            if (Sel_MultipleCobrosInvoice != null)
            {
                multipleCobrosInvoice.receipt_name = Sel_MultipleCobrosInvoice.receipt_name;
                multipleCobrosInvoice.id = Sel_MultipleCobrosInvoice.id;
            }

            multipleCobrosInvoice.date = fechaActual;
            multipleCobrosInvoice.create_date = fechaActual;
            multipleCobrosInvoice.create_uid = App.Session.CurrentUserFront.uid;
            multipleCobrosInvoice.user_id = App.Session.CurrentUserFront.uid;
            multipleCobrosInvoice.center_id = App.Session.odooConnection.res_center_default;
            multipleCobrosInvoice.subclasificacion_gasto_id = App.Session.odooConnection.subclasificacion_gasto_default;

            multipleCobrosInvoice.partner_id = Sel_MultipleCobrosInvoice.partner_id;
            multipleCobrosInvoice.partner_name = Sel_MultipleCobrosInvoice.partner_name;
            multipleCobrosInvoice.amount = (float) totalPagado; //.ToString(App.Session.ApplicationCultureInfo);
            multipleCobrosInvoice.total_due = Sel_MultipleCobrosInvoice.total_due;
            multipleCobrosInvoice.payment_status = CobrosEstados.PENDIENTE;
            //
            multipleCobrosInvoice.partner_email = Sel_MultipleCobrosInvoice.partner_email;
            multipleCobrosInvoice.CERRADO = "N";
            multipleCobrosInvoice.user_name = App.Session.CurrentUserFront.nombres;
            multipleCobrosInvoice.state = "draft";

            if (editionMode)
            {
                multipleCobrosInvoice.external_guid = Sel_MultipleCobrosInvoice.external_guid;
                if (string.IsNullOrWhiteSpace(multipleCobrosInvoice.external_guid))
                    multipleCobrosInvoice.external_guid = Guid.NewGuid().ToString("N");
            }

            List<MultipleCobrosInvoiceLine> _accountPayment = new List<MultipleCobrosInvoiceLine>();
      
            _accountPayment = accountPayments.ToList();

            if (editionMode)
            {
                await database.UpdateAsync(multipleCobrosInvoice);

                _accountPayment.ForEach(item => item.MultipleCobrosInvoiceId = multipleCobrosInvoice.id);

                var accountMoveDb = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
                var accountPaymentDb = new MultipleCobrosInvoiceLineDb(App.Session.odooConnection.DbNameSqlite);
                var accountPaymentInvoiceLineDb = new MultipleCobrosInvoiceLineAiDb(App.Session.odooConnection.DbNameSqlite);
                //Se eliminan detalles previos para almacenar los nuevos
                //TODO: Se puede considerar crear un algoritmo de reemplazo de datos
                await accountPaymentDb.DeleteItemOfParent(multipleCobrosInvoice);

                //accountPaymentDb.InsertBatchAsync(cobReciboDet.ToArray());

                foreach(var accountPayment in _accountPayment)
                {
                    //Primero se eliminan las lineas del pago
                    await accountPaymentInvoiceLineDb.DeleteItemOfParent(accountPayment);

                    await accountPaymentDb.InsertAsync(accountPayment);
                    int accPayId = accountPayment.Id;

                    if (accountPayment.lines != null)
                    {
                        foreach (var accountPaymentInvoiceLine in accountPayment.lines)
                        {
                            accountPaymentInvoiceLine.multiple_cobros_invoice_line_id = accPayId;
                            await accountPaymentInvoiceLineDb.InsertAsync(accountPaymentInvoiceLine);

                            if (accountPayment.Type == "check")
                            {                                
                                var accountMove = await accountMoveDb.GetItemAsync(x => x.id == accountPaymentInvoiceLine.invoice_id);
                                if (accountMove != null)
                                {
                                    accountMove.mcl_check_id = accountPayment.Id;
                                    await accountMoveDb.UpdateAsync(accountMove);
                                }
                            }
                        }                        
                    }
                    else
                    {
                        //Mostrar mensaje, no se agregó la línea porque probablemente no tenia valor asignado el documento
                    }
                }
            }
        }
        else
        {

            if(Sel_Res_Partner == null)
            {
                await Toast.Make("No se ha seleccionado cliente para la creación del cobro.").Show();
                return;
            }

            var multipleCobrosInvoice = new MultipleCobrosInvoice();
            //TODO: Asignación de los datos del pago nuevo

            //accountPaymentHeader.company_id = Sel_Res_Partner.company_id;
            multipleCobrosInvoice.company_id = Sel_Company_Id.id;
            multipleCobrosInvoice.partner_name = Sel_Res_Partner.name;
            multipleCobrosInvoice.partner_id = Sel_Res_Partner.id;

            multipleCobrosInvoice.date = fechaActual;
            multipleCobrosInvoice.create_date = fechaActual;
            multipleCobrosInvoice.create_uid = App.Session.CurrentUserFront.uid;
            multipleCobrosInvoice.user_id = App.Session.CurrentUserFront.uid;
            multipleCobrosInvoice.center_id = App.Session.odooConnection.res_center_default;
            multipleCobrosInvoice.subclasificacion_gasto_id = App.Session.odooConnection.subclasificacion_gasto_default;

            multipleCobrosInvoice.amount = (float) totalPagado; //.ToString(App.Session.ApplicationCultureInfo);
            multipleCobrosInvoice.total_due = (float) Sel_Res_Partner.total_due;
            multipleCobrosInvoice.payment_status = CobrosEstados.PENDIENTE;
            //
            multipleCobrosInvoice.partner_email = Sel_Res_Partner.email;
            multipleCobrosInvoice.CERRADO = "N";
            multipleCobrosInvoice.user_name = App.Session.CurrentUserFront.nombres;
            multipleCobrosInvoice.state = "draft";
            multipleCobrosInvoice.note = "DESDE APLICACIÓN MÓVIL";
            multipleCobrosInvoice.external_create_uid = App.Session.CurrentUserFront.uid;
            multipleCobrosInvoice.external_guid = Guid.NewGuid().ToString("N");

            List<MultipleCobrosInvoiceLine> _accountPayment = new List<MultipleCobrosInvoiceLine>();
      
            _accountPayment = accountPayments.ToList();

            await database.InsertAsync(multipleCobrosInvoice);

            int newId = multipleCobrosInvoice.id;

            _accountPayment.ForEach(item => item.MultipleCobrosInvoiceId = newId);

            var accountPaymentDb = new MultipleCobrosInvoiceLineDb(App.Session.odooConnection.DbNameSqlite);
            var accountPaymentInvoiceLineDb = new MultipleCobrosInvoiceLineAiDb(App.Session.odooConnection.DbNameSqlite);
            
            foreach (var accountPayment in _accountPayment)
            {
                await accountPaymentDb.InsertAsync(accountPayment);
                int accPayId = accountPayment.Id;

                foreach (var accountPaymentInvoiceLine in accountPayment.lines)
                {
                    accountPaymentInvoiceLine.multiple_cobros_invoice_line_id = accPayId;
                    await accountPaymentInvoiceLineDb.InsertAsync(accountPaymentInvoiceLine);
                }
            }
        }

        await Toast.Make("Almacenado correctamente.").Show();        
        await Navigation.PopAsync();
    }

    private void btnRemoveCustomer_Clicked(object sender, EventArgs e)
    {        
        txtCliente.Text = "";
    }
    
    readonly PopupSizeConstants popupSizeConstants;

    async void HandleReturnResultPopupButtonClicked(object sender, EventArgs e)
    {
        var returnResultPopup = new PopupSelectPartnerCreditData(popupSizeConstants);
        
        returnResultPopup.Company = new res_company()
        {
            id = Sel_Company_Id.id,
            name = Sel_Company_Id.name,
        };

        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        var result = await PopupExtensions.ShowPopupAsync<res_partner>(this, returnResultPopup);
        
        if (result.Result != null)
        {
            Sel_Res_Partner = (res_partner)result.Result;
            txtCliente.Text = Sel_Res_Partner.id.ToString() + " - " + Sel_Res_Partner.name;            
        }
    }

    async void ResultPopupAccountMove(object sender, EventArgs e)
    {
        if (Sel_Res_Partner == null)
        {
            await DisplayAlertAsync(
                "Confirmación requerida",
                "Es necesario seleccionar un cliente para mostrar saldos.",
                "Aceptar");
            return;
        }

        var returnResultPopup = new PopupSelectInvoice(popupSizeConstants);
        returnResultPopup.LoadAuto = false;
        returnResultPopup.ShowTextSearch = false;
        returnResultPopup.ShowToolBox = false;
        returnResultPopup.partner = Sel_Res_Partner;        

        returnResultPopup.Company = new res_company()
        {
            id = Sel_Company_Id.id,
            name = Sel_Company_Id.name,
        };

        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        var result = await PopupExtensions.ShowPopupAsync(this, returnResultPopup);       

        if (result != null)
        {
            //var resPartner = (res_partner)result;
            //txtCliente.Text = resPartner.id.ToString() + " - " + resPartner.name;
            //_res_partnerItem = resPartner;
        }
    }

    async void DownloadDataCustomer(object sender, EventArgs e)
    {
        if (Sel_Res_Partner == null)
            return;

        bool answer = await DisplayAlertAsync(
            "Atención",
            AccountMoveDocumentDisplay.PartnerSyncConfirmMessage,
            "Si",
            "No");

        if (!answer)
        {
            return;
        }

        await UITools.ShowLoadingPopup(this);
        await UITools.SetNotifyLoadingPopup(AccountMoveDocumentDisplay.PartnerSyncProgressMessage);

        try
        {
            var serverPuller = new ServerPuller();
            await serverPuller.OnlineSyncAccountMoveByResPartner(Sel_Res_Partner.id);

            var databaseAccountMove = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
            var listCustomer = await databaseAccountMove.GetItemsAsync(x => x._partner_id == Sel_Res_Partner.id);
            if (listCustomer != null && listCustomer.Count > 0)
            {
                foreach (var item in listCustomer)
                {
                    await serverPuller.OnlineSyncAccountMoveLineByMove(item.id);
                }
            }

            await Toast.Make("Terminado.").Show();
        }
        catch (Exception ex)
        {
            await Toast.Make("Error:" + ex.Message).Show();
        }
        finally
        {
            await UITools.HideLoadingPopup();
        }
    }
}