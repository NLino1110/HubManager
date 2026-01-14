using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Sample.Models;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Controls.Popups;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.DebitCollection;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace DMCobranzas.Controls.Modals.TabbedPages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AccountPaymentView : ContentPage
{
    private int subclasificacion_gasto_id_default = 73;
    public bool ClosingSaved { get; set; } = false;

    //public AccountPaymentHeader Sel_AccountPaymentHeader { get; set; }
    public res_company Sel_Company_Id { get; set; }
    public res_partner Sel_Res_Partner { get; set; }
    //public CobCarteraCab _cobCarteraCab { get; set; }

    public decimal totalPagado { get; set; }
    public decimal totalAplicado { get; set; }

    public bool isWindows { get; set; } = false;
    public bool editionMode { get; set; } = false;
    public bool isFirstLoad { get; set; } = true;
    public MultipleCobrosInvoiceLine[] accountPayments { get; set; } = new MultipleCobrosInvoiceLine[0];

    public static readonly BindableProperty _cobReciboCabProperty =
            BindableProperty.Create(nameof(Sel_AccountPaymentHeader), typeof(MultipleCobrosInvoice), typeof(AccountPaymentView));

    public MultipleCobrosInvoice Sel_AccountPaymentHeader
    {
        get => (MultipleCobrosInvoice)GetValue(_cobReciboCabProperty);
        set => SetValue(_cobReciboCabProperty, value);
    }

    //public static readonly BindableProperty _cobCarteraCabProperty =
    //        BindableProperty.Create(nameof(cobCarteraCab), typeof(CobCarteraCab), typeof(Documentos));

    //public CobCarteraCab cobCarteraCab
    //{
    //    get => (CobCarteraCab)GetValue(_cobCarteraCabProperty);
    //    set => SetValue(_cobCarteraCabProperty, value);
    //}

    public AccountPaymentView()
	{
		InitializeComponent();
        EditItemCommand = new Command(EditItem);
        DeleteItemCommand = new Command(DeleteItem);
        
        //LoadData();        
        isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;

        this.popupSizeConstants2 = new DMSA.Sync.Core.Controls.Popups.PopupSizeConstants(DeviceDisplay.Current);
        this.popupSizeConstants = new CommunityToolkit.Maui.Sample.Models.PopupSizeConstants(DeviceDisplay.Current);
                
        BindingContext = this;
    }

    void OnEntryTapped(object sender, EventArgs e)
    {
        // Código a ejecutar cuando se "tapea" en el Entry.
        HandleReturnResultPopupButtonClicked(sender, e);
        Console.WriteLine("Entry tapped!");
    }

    //protected override bool OnBackButtonPressed()
    //{
    //    //Debug.WriteLine("Regresar!!");
    //    //return base.OnBackButtonPressed();
    //    Toast.Make("Use los botones GUARDAR/CANCELAR").Show();
    //    return true;
    //}

    protected override bool OnBackButtonPressed()
    {
        var tcs = new TaskCompletionSource<bool>();

        Dispatcher.Dispatch(async () =>
        {
            //if (LockEdition)
            //{
            //    await Navigation.PopModalAsync();
            //}

            //if (SearchProductView.IsVisible)
            //{
            //    await Toast.Make("Primero cierre la búsqueda de productos.").Show();
            //    return;
            //}

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
            //txtTotalPagado.Text = "TOTAL PAGO: $ " + totalPagado.ToString("0.00"); ;
            //txtTotalAplicado.Text = "TOTAL APLICADO: $ " + totalPagado.ToString("0.00");

            ////Cargar datos
            ////
            ////txtNomCliente.Text = cobCarteraCab.CODCLIENTE + " - " + cobCarteraCab.NOMBRECLIENTE;
            //var database = new CobCarteraDetDb();
            ////var result = await database.GetItemsAsync(empresa.empresa, txtBusqueda.Text.ToUpper());
            //var result = await database.GetItemsAsync();
            //dataItems = result.Where(y => y.CODCLIENTE == cobCarteraCab.CODCLIENTE).ToArray();
            //collectionView.ItemsSource = dataItems;
            timer.Stop();
            timer.IsRepeating = false;
            Debug.WriteLine("Formas de pago cargados.......");
        };
        timer.Start();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        //CobroNuevo obj = new CobroNuevo(_cobCarteraCab, Sel_Res_Partner);
        
        //obj.accountPaymentHeader = Sel_AccountPaymentHeader;
        //obj.isNewData = true;

        ////obj._cobCarteraCab = _cobCarteraCab;

        ////obj.Sel_Company_Id = Sel_Company_Id;
        ////Se asigna la empresa seleccionada
        ////obj.empresa = se;
        ////Se asigna título
        ////obj.Title = "Cartera Clientes/" + se.nombre;

        //await Navigation.PushModalAsync(obj, false);
        ////await Navigation.PushModalAsync(obj, true);

        //obj.Disappearing += Obj_Disappearing;
        ////SelectorCmp.IsEnabled = false;
    }

    private async void btnAddAccountPayment_Clicked(object sender, EventArgs e)
    {
        if(Sel_Res_Partner == null)
        {
            await Toast.Make("Cliente no seleccionado.").Show();
            return;
        }

        AccountPaymentCrud obj = new AccountPaymentCrud(Sel_Res_Partner);
        obj.Sel_Company_Id = Sel_Company_Id;
        obj.accountPaymentHeader = Sel_AccountPaymentHeader;
        obj.isNewData = true;

        //obj._cobCarteraCab = _cobCarteraCab;

        //obj.Sel_Company_Id = Sel_Company_Id;
        //Se asigna la empresa seleccionada
        //obj.empresa = se;
        //Se asigna título
        //obj.Title = "Cartera Clientes/" + se.nombre;

        //await Navigation.PushAsync(obj, false);
        await Navigation.PushModalAsync(obj, false);
        obj.Disappearing += accountPaymentCrud_Disappearing;
    }

    public ICommand EditItemCommand { get; set; }

    private async void EditItem(object objParam)
    {
        Debug.WriteLine("EditItem");

        //if (_cobCarteraCab == null && editionMode)
        //{
        //    //Hay que obtener los datos de cartera cuando el origen es el modo edición
        //    CobCarteraCabDb cobCarteraCabDb = new CobCarteraCabDb();
        //    _cobCarteraCab = await cobCarteraCabDb.GetItemAsync(Sel_AccountPaymentHeader.CODCLIENTE);
        //}

        AccountPaymentCrud obj = new AccountPaymentCrud(Sel_Res_Partner);
        obj.isNewData = false;
        obj.accountPayment = (MultipleCobrosInvoiceLine)objParam;
        obj.itemIndex = accountPayments.ToList().IndexOf(obj.accountPayment);
        obj.accountPaymentHeader = Sel_AccountPaymentHeader;

        //Se asigna la empresa seleccionada
        //obj.empresa = se;
        //Se asigna título
        //obj.Title = "Cartera Clientes/" + se.nombre;

        await Navigation.PushModalAsync(obj, false);
        //await Navigation.PushModalAsync(obj, true);

        obj.Disappearing += accountPaymentCrud_Disappearing;
    }

    public ICommand DeleteItemCommand { get; set; }

    private async void DeleteItem(object objParam)
    {
        Debug.WriteLine("DeleteItem");
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
            return 0; // Devuelve el valor original si no es un número válido
        }
    }

    private void accountPaymentCrud_Disappearing(object sender, EventArgs e)
    {
        Debug.WriteLine("Pago cerrado");
        Debug.WriteLine(((AccountPaymentCrud)sender).saveData);
        //throw new NotImplementedException();

        if (((AccountPaymentCrud)sender).saveData)
        {
            if (((AccountPaymentCrud)sender).isNewData)
            {
                var task = Task.Run(async () =>
                {
                    //var database = new CobReciboCabDb();
                    MultipleCobrosInvoiceLine cobReciboDet = new MultipleCobrosInvoiceLine();
                    

                    //Se asigna para posteriormente almacenar
                    cobReciboDet = ((AccountPaymentCrud)sender).accountPayment;
                    //FacNotaCreditoDet[] facturasItems = ((AccountPaymentCrud)sender).facturasItems;

                    //TODO: REVISAR ESTA ASIGNACION parece innecesaria
                    //cobReciboDet.valor = ParseTool.ConvertirAMoneda(cobReciboDet.valor);

                    //cobReciboDet.cta_cheque = "";
                    //cobReciboDet.descbanco = "";

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
                    cobReciboDet = ((AccountPaymentCrud)sender).accountPayment;

                    //TODO: PARECE INNECESARIO
                    //cobReciboDet.valor = ParseTool.StringToDouble(cobReciboDet.valor).ToString(App.Session.ApplicationCultureInfo);

                    nList[((AccountPaymentCrud)sender).itemIndex] = ((AccountPaymentCrud)sender).accountPayment;
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
            //di.sequence = idItem;

            decimal _totalPagado = (decimal) di.Amount;
            Debug.WriteLine(_totalPagado);

            totalPagado += _totalPagado; // ParseTool.StringToDecimal(item.valor);
        }

        collectionView.ItemsSource = accountPayments;

        txtTotalPagado.Text = "TOTAL PAGO: $ " + totalPagado.ToString(App.Session.ApplicationCultureInfo); ;
        //txtTotalAplicado.Text = "TOTAL APLICADO: $ " + totalPagado.ToString(App.Session.ApplicationCultureInfo);
    }

    private async Task LoadData()
    {
        //if (_cobCarteraCab != null) //&& !editionMode
        //{
        //    //Desde seleccion de cartera
        //    Title = "PAGOS-" + _cobCarteraCab.NOMBRECLIENTE;
        //}
        //else
        //{
            //if(Sel_AccountPaymentHeader != null) //&& editionMode
            //{
            //    //Desde listado para edición
            //    Title = "PAGOS-" + Sel_AccountPaymentHeader.partner_name;
            //}
        //}

        if ((accountPayments == null || accountPayments.Length == 0) && editionMode && isFirstLoad)
        {
            if (Sel_Res_Partner == null)
            {
                ResPartnerDb resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
                //Sel_Res_Partner = await resPartnerDb.GetItem(Sel_AccountPaymentHeader.partner_id);
                Sel_Res_Partner = await resPartnerDb.GetItemsAsync(Sel_AccountPaymentHeader.company_id, Sel_AccountPaymentHeader.partner_id);
                txtCliente.Text = Sel_Res_Partner.id + "-" + Sel_Res_Partner.name;
            }

            if (Sel_Company_Id == null)
            {
                var cemp = App.Session.CurrentUserFront.empresas.Where(c => c.id == Sel_AccountPaymentHeader.company_id).FirstOrDefault();

                Sel_Company_Id = new res_company
                {
                    id = Sel_AccountPaymentHeader.company_id,
                    name = cemp.name
                };

                Title = "Pagos-" + Sel_Company_Id.name;
            }

            //if (cobReciboCab != null && cobReciboCab.DETALLESPAGO!=null && cobReciboCab.DETALLESPAGO.Length > 0)
            if (Sel_AccountPaymentHeader != null)
            {
                //Si entra en modo edición se bloquea
                GridPartner.IsEnabled = false;

                //dataItems = new CobReciboDet[0];
                //var ls_dataItems = JsonConvert.DeserializeObject<List<AccountPayment>>(cobReciboCab.DETALLESPAGO);
                var accountPaymentDb = new MultipleCobrosInvoiceLineDb(App.Session.odooConnection.DbNameSqlite);
                var ls_accountPayments = await accountPaymentDb.GetItemsAsync(x=>x.MultipleCobrosInvoiceId == Sel_AccountPaymentHeader.id);
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
            //var database = new CobReciboCabDb();
            //var result = await database.GetItemsAsync();
            //dataItems = result.ToArray();
            int idItem = 0;
            foreach (var di in accountPayments)
            {
                idItem++;
                di.sequence = idItem;
            }

            Title = "Pagos-" + Sel_Company_Id.name;
            //collectionView.ItemsSource = dataItems;
            //await SummaryPayments();
        }

        isFirstLoad = false;

        if (accountPayments!=null)
            collectionView.ItemsSource = accountPayments;

        //Se deben cargar los AccountPayment y los AccountPaymentInvoiceLine
        // para que se mantenga el flujo de cambios a lo largo dle proceso


        await SummaryPayments();
    }

    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");
        //Debug.WriteLine("Answer: " + answer);
        if (!answer)
        {
            return;
        }
        await Navigation.PopAsync();
    }

    private async void btnSave_Clicked(object sender, EventArgs e)
    {
        //bool answer = await DisplayAlert("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");
        ////Debug.WriteLine("Answer: " + answer);
        //if (!answer)
        //{
        //    return;
        //}

        //CobCarteraCabDb cobCarteraCab = new CobCarteraCabDb();
        var database = new MultipleCobrosInvoiceDb(App.Session.odooConnection.DbNameSqlite);
        DateTime fechaActual = DateTime.Now;

        if (editionMode)
        {
            
        }

        //var dataResult = await cobCarteraCab.GetItemsAsync();
        //var cliente = dataResult.Where(cc => cc.CODEMPRESA == _cobCarteraCab.CODEMPRESA &&
        //cc.CODCLIENTE == _cobCarteraCab.CODCLIENTE).FirstOrDefault();

        if (Sel_AccountPaymentHeader != null)
        {
            //TODO: SI GUARDA Y SE ENVÍA CON LA FECHA CORTADA NO GUARDA BIEN EL API
            // NO CORTAR LA FECHA AQUÍ
            //string fechaActual = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Substring(0,10).Trim();

            //string fechaActual = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            //Guardar info
            MultipleCobrosInvoice accountPaymentHeader = new MultipleCobrosInvoice();
            accountPaymentHeader.company_id = Sel_AccountPaymentHeader.company_id;

            //Update
            if (Sel_AccountPaymentHeader != null)
            {
                accountPaymentHeader.recipe_name = Sel_AccountPaymentHeader.recipe_name;
                accountPaymentHeader.id = Sel_AccountPaymentHeader.id;
            }

            accountPaymentHeader.date = fechaActual;
            accountPaymentHeader.create_date = fechaActual;
            accountPaymentHeader.create_uid = App.Session.CurrentUserFront.uid;
            accountPaymentHeader.user_id = App.Session.CurrentUserFront.uid;
            accountPaymentHeader.center_id = App.Session.odooConnection.res_center_default;
            accountPaymentHeader.subclasificacion_gasto_id = subclasificacion_gasto_id_default;

            accountPaymentHeader.partner_id = Sel_AccountPaymentHeader.partner_id;
            accountPaymentHeader.partner_name = Sel_AccountPaymentHeader.partner_name;
            accountPaymentHeader.amount = (float) totalPagado; //.ToString(App.Session.ApplicationCultureInfo);
            accountPaymentHeader.total_due = Sel_AccountPaymentHeader.total_due;
            accountPaymentHeader.payment_status = DMSA.Models.CobrosEstados.PENDIENTE;
            //
            accountPaymentHeader.partner_email = Sel_AccountPaymentHeader.partner_email;
            accountPaymentHeader.CERRADO = "N";
            accountPaymentHeader.user_name = App.Session.CurrentUserFront.nombres;
            accountPaymentHeader.state = "draft";
            List<MultipleCobrosInvoiceLine> _accountPayment = new List<MultipleCobrosInvoiceLine>();

            //Se obtienen las formas de pago para almacenar            
            _accountPayment = accountPayments.ToList();

            //List<detallesDocumentos> detallesDocumentos = new List<detallesDocumentos>();
            //detallesDocumentos = tabDocumentos.dataItems.ToList();
                       
            //codusuario
            //cabeceraCobro
            //detallesCobro
            //detallesDocumentos
            if (editionMode)
            {
                await database.UpdateAsync(accountPaymentHeader);

                _accountPayment.ForEach(item => item.MultipleCobrosInvoiceId = accountPaymentHeader.id);

                var accountPaymentDb = new MultipleCobrosInvoiceLineDb(App.Session.odooConnection.DbNameSqlite);
                var accountPaymentInvoiceLineDb = new MultipleCobrosInvoiceLineAiDb(App.Session.odooConnection.DbNameSqlite);
                //Se eliminan detalles previos para almacenar los nuevos
                //TODO: Se puede considerar crear un algoritmo de reemplazo de datos
                await accountPaymentDb.DeleteItemOfParent(accountPaymentHeader);

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
                await Toast.Make("No se ha seleccionado cliente para la creación del pago.").Show();
                return;
            }

            var accountPaymentHeader = new MultipleCobrosInvoice();
            //TODO: Asignación de los datos del pago nuevo

            //accountPaymentHeader.company_id = Sel_Res_Partner.company_id;
            accountPaymentHeader.company_id = Sel_Company_Id.id;
            accountPaymentHeader.partner_name = Sel_Res_Partner.name;
            accountPaymentHeader.partner_id = Sel_Res_Partner.id;

            accountPaymentHeader.date = fechaActual;
            accountPaymentHeader.create_date = fechaActual;
            accountPaymentHeader.create_uid = App.Session.CurrentUserFront.uid;
            accountPaymentHeader.user_id = App.Session.CurrentUserFront.uid;
            accountPaymentHeader.center_id = App.Session.odooConnection.res_center_default;
            accountPaymentHeader.subclasificacion_gasto_id = subclasificacion_gasto_id_default;

            accountPaymentHeader.amount = (float) totalPagado; //.ToString(App.Session.ApplicationCultureInfo);
            accountPaymentHeader.total_due = (float) Sel_Res_Partner.total_due;
            accountPaymentHeader.payment_status = DMSA.Models.CobrosEstados.PENDIENTE;
            //
            accountPaymentHeader.partner_email = Sel_Res_Partner.email;
            accountPaymentHeader.CERRADO = "N";
            accountPaymentHeader.user_name = App.Session.CurrentUserFront.nombres;
            accountPaymentHeader.state = "draft";

            //List<ReceiptReceiptsLine> receiptLines = new List<ReceiptReceiptsLine>();
            //var receiptReceiptsLineDb = new ReceiptReceiptsLineDb(App.Session.odooConnection.DbNameSqlite);

            //if (accountPaymentHeader.receipt_receipts_id == 0)
            //{
                
            //    receiptLines = (await receiptReceiptsLineDb.GetItemsAsync(x => x._sale_user_id == accountPaymentHeader.user_id && x.state == "draft"))
            //        .OrderBy(x => x.number_seq)
            //        .Take(1)
            //        .ToList();

            //    if (receiptLines.Count > 0)
            //    {
            //        accountPaymentHeader.receipt_receipts_id = receiptLines[0]._receipt_receipts_id;
            //        accountPaymentHeader.receipt_receipts_line_id = receiptLines[0].number_seq;
            //        accountPaymentHeader.recipe_name = database.BuildName(accountPaymentHeader, 
            //            App.Session.CurrentUserFront.username, 
            //            accountPaymentHeader.receipt_receipts_line_id);

            //        //await database.UpdateAsync(accountPaymentHeader);

            //        receiptLines[0].state = "used";
            //        await receiptReceiptsLineDb.UpdateAsync(receiptLines[0]);
            //    }
            //}

            List<MultipleCobrosInvoiceLine> _accountPayment = new List<MultipleCobrosInvoiceLine>();

            //Se obtienen las formas de pago para almacenar            
            _accountPayment = accountPayments.ToList();

            await database.InsertAsync(accountPaymentHeader);

            //Se actualiza el estado del recibo utilizado
            //if (receiptLines.Count > 0)
            //{
            //    receiptLines[0].state = "used";
            //    await receiptReceiptsLineDb.UpdateAsync(receiptLines[0]);
            //}

            //Se obtiene el nuevo ID
            int newId = accountPaymentHeader.id;

            _accountPayment.ForEach(item => item.MultipleCobrosInvoiceId = newId);

            var accountPaymentDb = new MultipleCobrosInvoiceLineDb(App.Session.odooConnection.DbNameSqlite);
            var accountPaymentInvoiceLineDb = new MultipleCobrosInvoiceLineAiDb(App.Session.odooConnection.DbNameSqlite);
            //accountPaymentDb.InsertBatchAsync(cobReciboDet.ToArray());

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

    private async Task SaveNewRecord()
    {
        
    }

    private void btnRemoveCustomer_Clicked(object sender, EventArgs e)
    {
        //_res_partnerItem = null;
        txtCliente.Text = "<NO SELECCIONADO>";
        //ClearItems();
    }

    readonly CommunityToolkit.Maui.Sample.Models.PopupSizeConstants popupSizeConstants;
    readonly DMSA.Sync.Core.Controls.Popups.PopupSizeConstants popupSizeConstants2;

    async void HandleReturnResultPopupButtonClicked(object sender, EventArgs e)
    {
        var returnResultPopup = new DMSA.Sync.Core.Controls.Popups.PopupSelectPartnerCreditData(popupSizeConstants2);
        //var empresa = (res_company) SelectorCmp.SelectedItem;
        //returnResultPopup.Company = new Company()
        //{
        //    id = empresa.id,
        //    name = empresa.name,
        //};

        returnResultPopup.Company = new res_company()
        {
            id = Sel_Company_Id.id,
            name = Sel_Company_Id.name,
        };

        //Evita que se cierre cuando se haga clic (tap) fuera de la ventana
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        //if (!isWindows)
            //returnResultPopup.Size = this.popupSizeConstants.Large;

        var result = await PopupExtensions.ShowPopupAsync<res_partner>(this, returnResultPopup);
        //var result = await this.ShowPopupAsync(returnResultPopup);

        if (result.Result != null)
        {
            Sel_Res_Partner = (res_partner)result.Result;
            txtCliente.Text = Sel_Res_Partner.id.ToString() + " - " + Sel_Res_Partner.name;
            //_res_partnerItem = resPartner;
        }
    }

    async void ResultPopupAccountMove(object sender, EventArgs e)
    {
        if (Sel_Res_Partner == null)
        {
            await Toast.Make("Cliente no seleccionado.").Show();
            return;
        }

        //var returnResultPopup = new PopupAccountMoves(popupSizeConstants);
        var returnResultPopup = new PopupSelectInvoice(popupSizeConstants);
        returnResultPopup.LoadAuto = true;
        returnResultPopup.ShowTextSearch = false;
        returnResultPopup.ShowToolBox = false;
        returnResultPopup.partner = Sel_Res_Partner;
        //var empresa = (res_company) SelectorCmp.SelectedItem;
        //returnResultPopup.Company = new Company()
        //{
        //    id = empresa.id,
        //    name = empresa.name,
        //};

        returnResultPopup.Company = new res_company()
        {
            id = Sel_Company_Id.id,
            name = Sel_Company_Id.name,
        };

        //Evita que se cierre cuando se haga clic (tap) fuera de la ventana
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        //if (!isWindows)
        //    returnResultPopup.Size = this.popupSizeConstants.Large;

        var result = await PopupExtensions.ShowPopupAsync(this, returnResultPopup);
        //var result = await this.ShowPopupAsync(returnResultPopup);

        if (result != null)
        {
            //var resPartner = (res_partner)result;
            //txtCliente.Text = resPartner.id.ToString() + " - " + resPartner.name;
            //_res_partnerItem = resPartner;
        }
    }
}