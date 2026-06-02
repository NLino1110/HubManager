using System.Diagnostics;
using System.Windows.Input;
using DMSA.Models.Odoo.Native;
using CommunityToolkit.Maui.Alerts;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using DMSA.Sync.Core.Database.Sqlite;
using CommunityToolkit.Maui.Extensions;
using DMSA.Sync.Core.Controls.Popups;
using DMSA.Models.Odoo.Accounting;
using DMSA.Sync.Core.Database.Sqlite.Accounting;

namespace DMCobranzas.AppPages.NotaCredito;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CreditNoteRequestGroupView : ContentPage
{
    public bool ClosingSaved { get; set; } = false;

    public res_company Sel_Company_Id { get; set; }
    public res_partner Sel_Res_Partner { get; set; }

    public decimal totalPagado { get; set; }
    public decimal totalAplicado { get; set; }

    public bool isWindows { get; set; } = false;
    public bool editionMode { get; set; } = false;
    public bool isFirstLoad { get; set; } = true;
    public credit_note_request[] creditNoteRequests { get; set; } = new credit_note_request[0];

    public ICommand AddCNCommand { get; set; }

    public static readonly BindableProperty Sel_CreditNoteRequestGroupProperty =
            BindableProperty.Create(nameof(Sel_CreditNoteRequestGroup), 
                typeof(CreditNoteRequestGroup), 
                typeof(CreditNoteRequestGroupView));

    public CreditNoteRequestGroup Sel_CreditNoteRequestGroup
    {
        get => (CreditNoteRequestGroup)GetValue(Sel_CreditNoteRequestGroupProperty);
        set => SetValue(Sel_CreditNoteRequestGroupProperty, value);
    }

    public CreditNoteRequestGroupView()
	{
		InitializeComponent();
        EditItemCommand = new Command(EditItem);
        DeleteItemCommand = new Command(DeleteItem);           
        isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;
        this.popupSizeConstants = new DMSA.Sync.Core.Controls.Popups.PopupSizeConstants(DeviceDisplay.Current);
                
        AddCNCommand = new Command(async () =>
        {
            btnAddCreditNote_Clicked(null, null);
        });
        BindingContext = this;
    }

    protected override bool OnBackButtonPressed()
    {
        var tcs = new TaskCompletionSource<bool>();

        Dispatcher.Dispatch(async () =>
        {
            var leave = await DisplayAlertAsync("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");

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
            Debug.WriteLine("Formas de pago cargados.......");
        };
        timer.Start();
    }

    private async void btnAddCreditNote_Clicked(object sender, EventArgs e)
    {
        if (Sel_Res_Partner == null)
        {
            await Toast.Make("Cliente no seleccionado.").Show();
            return;
        }

        CreditNoteRequestCrud obj = new CreditNoteRequestCrud(Sel_Res_Partner, Sel_Company_Id);        
        obj.accountMoveSendHeader = Sel_CreditNoteRequestGroup;
        obj.isNewData = true;

        await Navigation.PushAsync(obj, false);
        obj.Disappearing += accountPaymentCrud_Disappearing;        
    }


    public ICommand EditItemCommand { get; set; }

    private async void EditItem(object objParam)
    {
        Debug.WriteLine("EditItem");

        credit_note_request account_Move_Send = (credit_note_request)objParam;        
        AccountMoveDb accountMoveDb = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
        var _accountMoveSelected = await accountMoveDb.GetItemAsync(x=>x.id == account_Move_Send.mainAccountMove);

        CreditNoteRequestCrud obj = new CreditNoteRequestCrud(Sel_Res_Partner, Sel_Company_Id);
        
        obj.isNewData = false;
        obj.creditNoteRequest = account_Move_Send;
        obj.itemIndex = creditNoteRequests.ToList().IndexOf(obj.creditNoteRequest);
        obj.accountMoveSendHeader = Sel_CreditNoteRequestGroup;
        obj._accountMoveSelected = _accountMoveSelected;

        await Navigation.PushAsync(obj, false);
        
        obj.Disappearing += accountPaymentCrud_Disappearing;
    }

    public ICommand DeleteItemCommand { get; set; }

    private async void DeleteItem(object objParam)
    {
        Debug.WriteLine("DeleteItem");
        bool answer = await DisplayAlertAsync("Eliminar", "Está seguro que desea eliminar este item?", "Eliminar", "Cancelar");
        
        if (answer)
        {
            var nList = creditNoteRequests.ToList();
            int itemIndex = nList.IndexOf((credit_note_request)objParam);            
            nList.RemoveAt(itemIndex);
            
            creditNoteRequests = nList.ToArray();
            await LoadData();
            return;
        }
    }

    // Función para buscar el objeto más parecido en una lista
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
            return 0;
        }
    }

    private void accountPaymentCrud_Disappearing(object sender, EventArgs e)
    {
        Debug.WriteLine("Visor nota de credito cerrada");
        
        switch(sender.GetType().Name)
        {
            case "CreditNoteRequestCrud":
                {
                    Debug.WriteLine(((CreditNoteRequestCrud)sender).saveData);

                    if (((CreditNoteRequestCrud)sender).saveData)
                    {
                        if (((CreditNoteRequestCrud)sender).isNewData)
                        {
                            var task = Task.Run(async () =>
                            {
                                credit_note_request accountMoveSendItem = new credit_note_request();

                                //Se asigna para posteriormente almacenar
                                accountMoveSendItem = ((CreditNoteRequestCrud)sender).creditNoteRequest;                               
                                var nList = creditNoteRequests.ToList();
                                nList.Add(accountMoveSendItem);
                                creditNoteRequests = nList.ToArray();
                                Debug.WriteLine("Guardado nuevo");
                            });
                            Task.WaitAll(task);
                        }
                        else
                        {
                            var task = Task.Run(async () =>
                            {
                                var nList = creditNoteRequests.ToList();

                                credit_note_request accountMoveSendItem = new credit_note_request();
                                accountMoveSendItem = ((CreditNoteRequestCrud)sender).creditNoteRequest;

                                nList[((CreditNoteRequestCrud)sender).itemIndex] = ((CreditNoteRequestCrud)sender).creditNoteRequest;
                                creditNoteRequests = nList.ToArray();
                                Debug.WriteLine("Guardado cambios");
                            });
                            Task.WaitAll(task);
                        }
                    }
                }
                break;
        }

        LoadData();
    }

    private async Task SummaryData()
    {
        totalPagado = 0;

        int idItem = 0;
        foreach (var di in creditNoteRequests)
        {
            idItem++;
            decimal _totalPagado = 0;
            Debug.WriteLine(_totalPagado);
            totalPagado += _totalPagado; 
        }

        txtTotalPagado.Text = "TOTAL PAGO: $ " + totalPagado.ToString(App.Session.ApplicationCultureInfo);        
    }

    private async Task LoadData()
    {   
        if ((creditNoteRequests == null || creditNoteRequests.Length == 0) && editionMode && isFirstLoad)
        {
            if (Sel_Res_Partner == null)
            {
                ResPartnerDb resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);                
                Sel_Res_Partner = await resPartnerDb.GetItemsAsync(Sel_CreditNoteRequestGroup.company_id, Sel_CreditNoteRequestGroup.partner_id);
                txtCliente.Text = Sel_Res_Partner.id + "-" + Sel_Res_Partner.name;
            }

            if (Sel_Company_Id == null)
            {
                var cemp = App.Session.CurrentUserFront.empresas.Where(c => c.id == Sel_CreditNoteRequestGroup.company_id).FirstOrDefault();
                
                Sel_Company_Id = cemp;

                Title = "" + Sel_Company_Id.name;
            }

            if (Sel_CreditNoteRequestGroup != null)
            {
                GridPartner.IsEnabled = false;

                CreditNoteRequestDb accountPaymentDb = new CreditNoteRequestDb(App.Session.odooConnection.DbNameSqlite);
                var ls_accountPayments = await accountPaymentDb.GetByParent(Sel_CreditNoteRequestGroup.id);
                creditNoteRequests = ls_accountPayments.ToArray();

                CreditNoteRequestDetailDb accountPaymentLines = new CreditNoteRequestDetailDb(App.Session.odooConnection.DbNameSqlite);

                var typeNcDb = new TypeNcDb(App.Session.odooConnection.DbNameSqlite);
                var typeParentDb = new TypeParentNcDb(App.Session.odooConnection.DbNameSqlite);

                var typeNcs = (await typeNcDb.GetItemsAsync(x=>x.id > 0)).ToArray();
                var typeParents = (await typeParentDb.GetItemsAsync(x => x.id > 0)).ToArray();

                foreach (var creditNoteReqItem in creditNoteRequests)
                {
                    var display_parent_nc = typeParents.Where(tp => tp.id == creditNoteReqItem.parent_nc_id).FirstOrDefault();
                    var display_type_nc = typeNcs.Where(tp => tp.id == creditNoteReqItem.type_module_id).FirstOrDefault();
                    creditNoteReqItem.display_parent_nc = display_parent_nc.name;
                    creditNoteReqItem.display_type_module = display_type_nc.name;

                    var apl = await accountPaymentLines.GetItemsAsync(creditNoteReqItem);

                    if (apl.Count() > 0)
                    {
                        creditNoteReqItem.lines = apl.ToArray();
                    }
                }
            }            
        }
        else
        {            
            int idItem = 0;
            foreach (var di in creditNoteRequests)
            {
                idItem++;
                di.sequence = idItem;
            }

            Title = "" + Sel_Company_Id.name;            
        }

        isFirstLoad = false;

        if (creditNoteRequests!=null)
            collectionView.ItemsSource = creditNoteRequests;

        await SummaryData();
    }

    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlertAsync("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");
        
        if (!answer)
        {
            return;
        }
        await Navigation.PopAsync();
    }

    private async void btnSave_Clicked(object sender, EventArgs e)
    {        
        CreditNoteRequestGroupDb database = new CreditNoteRequestGroupDb(App.Session.odooConnection.DbNameSqlite);
        DateTime fechaActual = DateTime.Now;

        if (editionMode)
        {
            
        }

        if (Sel_CreditNoteRequestGroup != null)
        {            
            CreditNoteRequestGroup creditNoteRequestGroup = new CreditNoteRequestGroup();
            creditNoteRequestGroup.company_id = Sel_CreditNoteRequestGroup.company_id;
            
            if (Sel_CreditNoteRequestGroup != null)
            {
                creditNoteRequestGroup.request_name = Sel_CreditNoteRequestGroup.request_name;
                creditNoteRequestGroup.id = Sel_CreditNoteRequestGroup.id;
            }

            creditNoteRequestGroup.create_datetime = fechaActual;
            creditNoteRequestGroup.uid = App.Session.CurrentUser.uid;
            creditNoteRequestGroup.partner_id = Sel_CreditNoteRequestGroup.partner_id;
            creditNoteRequestGroup.partner_name = Sel_CreditNoteRequestGroup.partner_name;            
            creditNoteRequestGroup.request_status = DMSA.Models.MoveStatus.PENDIENTE;            
            creditNoteRequestGroup.emailCustomer = Sel_CreditNoteRequestGroup.emailCustomer;            
            creditNoteRequestGroup.username = App.Session.CurrentUser.nombres;

            List<credit_note_request> _accountPayment = new List<credit_note_request>();
       
            _accountPayment = creditNoteRequests.ToList();
                                  
            if (editionMode)
            {
                await database.UpdateAsync(creditNoteRequestGroup);

                _accountPayment.ForEach(item => item.parent_id = creditNoteRequestGroup.id);

                CreditNoteRequestDb accountPaymentDb = new CreditNoteRequestDb(App.Session.odooConnection.DbNameSqlite);
                CreditNoteRequestDetailDb accountPaymentInvoiceLineDb = new CreditNoteRequestDetailDb(App.Session.odooConnection.DbNameSqlite);
                
                await accountPaymentDb.DeleteItemOfParent(creditNoteRequestGroup);

                foreach(var accountPayment in _accountPayment)
                {                    
                    await accountPaymentInvoiceLineDb.DeleteItemOfParent(accountPayment);
                    await accountPaymentDb.InsertAsync(accountPayment);
                    int accPayId = accountPayment.id;

                    if (accountPayment.lines != null)
                    {
                        foreach (var accountMoveSendItem in accountPayment.lines)
                        {
                            accountMoveSendItem.parent_id = accPayId;
                            await accountPaymentInvoiceLineDb.InsertAsync(accountMoveSendItem);
                        }
                    }
                    else
                    {
                        
                    }
                } 
            }            
        }
        else
        {
            if (Sel_Res_Partner == null)
            {
                await Toast.Make("No se ha seleccionado cliente para la creación del NC.").Show();
                return;
            }

            CreditNoteRequestGroup accountPaymentHeader = new CreditNoteRequestGroup();
            
            accountPaymentHeader.company_id = Sel_Company_Id.id;
            accountPaymentHeader.partner_name = Sel_Res_Partner.name;
            accountPaymentHeader.partner_id = Sel_Res_Partner.id;
            accountPaymentHeader.create_datetime = fechaActual;
            accountPaymentHeader.uid = App.Session.CurrentUser.uid;
            
            //accountPaymentHeader.payment_amount = totalPagado; //.ToString(App.Session.ApplicationCultureInfo);
            //accountPaymentHeader.total_due = Sel_Res_Partner.total_due;
            accountPaymentHeader.request_status = DMSA.Models.MoveStatus.PENDIENTE;
            //
            accountPaymentHeader.emailCustomer = Sel_Res_Partner.email;
            //accountPaymentHeader.CERRADO = "N";
            accountPaymentHeader.username = App.Session.CurrentUser.nombres;

            List<credit_note_request> _accountMoveSend = new List<credit_note_request>();

            //Se obtienen las formas de pago para almacenar            
            _accountMoveSend = creditNoteRequests.ToList();

            await database.InsertAsync(accountPaymentHeader);

            //Se obtiene el nuevo ID
            int newId = accountPaymentHeader.id;

            _accountMoveSend.ForEach(item => item.parent_id = newId);

            CreditNoteRequestDb accountPaymentDb = new CreditNoteRequestDb(App.Session.odooConnection.DbNameSqlite);
            CreditNoteRequestDetailDb accountPaymentInvoiceLineDb = new CreditNoteRequestDetailDb(App.Session.odooConnection.DbNameSqlite);
            //accountPaymentDb.InsertBatchAsync(cobReciboDet.ToArray());

            foreach (var accountMoveSendItem in _accountMoveSend)
            {
                await accountPaymentDb.InsertAsync(accountMoveSendItem);
                int accPayId = accountMoveSendItem.id;

                foreach (var accountPaymentInvoiceLine in accountMoveSendItem.lines)
                {
                    accountPaymentInvoiceLine.parent_id = accPayId;
                    await accountPaymentInvoiceLineDb.InsertAsync(accountPaymentInvoiceLine);
                }
            }
        }

        await Toast.Make("Almacenado correctamente.").Show();        
        await Navigation.PopAsync();
    }

    private void btnRemoveCustomer_Clicked(object sender, EventArgs e)
    {
        Sel_Res_Partner = null;
        txtCliente.Text = "";
    }

    readonly PopupSizeConstants popupSizeConstants;

    void OnEntryTapped(object sender, EventArgs e)
    {        
        btnCustomer_Clicked(sender, e);
        Console.WriteLine("Entry tapped!");
    }

    async void btnCustomer_Clicked(object sender, EventArgs e)
    {        
        //var resultPopupSelectInvoice = new PopupSelectPartnerSingle(popupSizeConstants);
        var resultPopupSelectInvoice = new PopupSelectPartner(popupSizeConstants);
        resultPopupSelectInvoice.Company = Sel_Company_Id;
        resultPopupSelectInvoice.DetailMode = 1;
        resultPopupSelectInvoice.CanBeDismissedByTappingOutsideOfPopup = false;        

        var result = await this.ShowPopupAsync<res_partner>(resultPopupSelectInvoice);
        if (result.Result != null)
        {
            var resPartner = (res_partner)result.Result;
            txtCliente.Text = resPartner.id.ToString() + " - " + resPartner.name;
            Sel_Res_Partner = resPartner;
        }
    }
}