using DMCobranzas.Models;
using System.Collections.Generic;
using System.Linq;
//using Foundation;
using System.Diagnostics;
using System.Windows.Input;
using System.Collections;
using System;
using Newtonsoft.Json;
using DMCobranzas.Settings.helpers;
using CobranzasDMSA.Models.General.Core;
using DMSA.Models.Odoo.Native;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Sample.Models;
using DMCobranzas.Controls.Modals;
using DMCobranzas.Controls;
using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using DMSA.Sync.Core.Database.Sqlite;
using CommunityToolkit.Maui.Extensions;

namespace DMCobranzas.AppPages.NotaCredito;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AccountMoveSendView : ContentPage
{
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
    public account_move_send[] accountMovesSend { get; set; } = new account_move_send[0];

    public static readonly BindableProperty Sel_AccountMoveSendHeaderProperty =
            BindableProperty.Create(nameof(Sel_AccountMoveSendHeader), 
                typeof(AccountMoveSendHeader), 
                typeof(AccountMoveSendView));

    public AccountMoveSendHeader Sel_AccountMoveSendHeader
    {
        get => (AccountMoveSendHeader)GetValue(Sel_AccountMoveSendHeaderProperty);
        set => SetValue(Sel_AccountMoveSendHeaderProperty, value);
    }

    //public static readonly BindableProperty _cobCarteraCabProperty =
    //        BindableProperty.Create(nameof(cobCarteraCab), typeof(CobCarteraCab), typeof(Documentos));

    //public CobCarteraCab cobCarteraCab
    //{
    //    get => (CobCarteraCab)GetValue(_cobCarteraCabProperty);
    //    set => SetValue(_cobCarteraCabProperty, value);
    //}

    public AccountMoveSendView()
	{
		InitializeComponent();
        EditItemCommand = new Command(EditItem);
        DeleteItemCommand = new Command(DeleteItem);
        //LoadData();        
        isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;

        this.popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
                
        BindingContext = this;
    }

    protected override bool OnBackButtonPressed()
    {
        //Debug.WriteLine("Regresar!!");
        //return base.OnBackButtonPressed();
        Toast.Make("Use los botones GUARDAR/CANCELAR").Show();
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
        if (Sel_Res_Partner == null)
        {
            await Toast.Make("Cliente no seleccionado.").Show();
            return;
        }

        AccountMoveSendCrud obj = new AccountMoveSendCrud(Sel_Res_Partner);        
        obj.accountMoveSendHeader = Sel_AccountMoveSendHeader;
        obj.isNewData = true;
        await obj.SetDefaultData(Sel_Company_Id);

        //obj._cobCarteraCab = _cobCarteraCab;
        //obj.Sel_Company_Id = Sel_Company_Id;
        //Se asigna la empresa seleccionada
        //obj.empresa = se;
        //Se asigna título
        //obj.Title = "Cartera Clientes/" + se.nombre;

        await Navigation.PushModalAsync(obj, false);        
        obj.Disappearing += accountPaymentCrud_Disappearing;        
    }

    private async void btnAddMoveByItem_Clicked(object sender, EventArgs e)
    {
        if (Sel_Res_Partner == null)
        {
            await Toast.Make("Cliente no seleccionado.").Show();
            return;
        }

        AccountMoveSendProdCrud obj = new AccountMoveSendProdCrud(Sel_Res_Partner);
        obj.accountMoveSendHeader = Sel_AccountMoveSendHeader;
        obj.isNewData = true;

        await Navigation.PushModalAsync(obj, false);
        obj.Disappearing += accountPaymentCrud_Disappearing;
    }

    public ICommand EditItemCommand { get; set; }

    private async void EditItem(object objParam)
    {
        Debug.WriteLine("EditItem");

        account_move_send account_Move_Send = (account_move_send)objParam;
        //AccountMoveLineSendDb databaseDet = new AccountMoveLineSendDb();
        //var result_send = await databaseDet.GetItemsByParentAsync(account_Move_Send.id);
        //account_Move_Send.lines = result_send.ToArray();
        AccountMoveDb accountMoveDb = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
        var _accountMoveSelected = await accountMoveDb.GetByNameItem(account_Move_Send._ref);

        AccountMoveSendCrud obj = new AccountMoveSendCrud(Sel_Res_Partner);
        await obj.SetDefaultData(Sel_Company_Id);

        obj.isNewData = false;
        obj.accountMoveSend = account_Move_Send;
        obj.itemIndex = accountMovesSend.ToList().IndexOf(obj.accountMoveSend);
        obj.accountMoveSendHeader = Sel_AccountMoveSendHeader;
        obj._accountMoveSelected = _accountMoveSelected;

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
            var nList = accountMovesSend.ToList();
            int itemIndex = nList.IndexOf((account_move_send)objParam);            
            nList.RemoveAt(itemIndex);
            
            accountMovesSend = nList.ToArray();
            await LoadData();
            return;
        }
    }

    //private async void EditItem_Clicked(object sender, EventArgs e)
    //{
    //    CobroNuevo obj = new CobroNuevo();
    //    obj.isNewData = false;
    //    obj.cobReciboDet = null;
    //    //Se asigna la empresa seleccionada
    //    //obj.empresa = se;
    //    //Se asigna título
    //    //obj.Title = "Cartera Clientes/" + se.nombre;

    //    await Navigation.PushModalAsync(obj, false);
    //    //await Navigation.PushModalAsync(obj, true);

    //    obj.Disappearing += Obj_Disappearing;
    //    //SelectorCmp.IsEnabled = false;
    //}

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

    //public string ConvertirAMoneda(string valor)
    //{
    //    if (decimal.TryParse(valor, out decimal numero))
    //    {
    //        return numero.ToString("0.00");
    //    }
    //    else
    //    {
    //        return valor; // Devuelve el valor original si no es un número válido
    //    }
    //}

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

        
        switch(sender.GetType().Name)
        {
            case "AccountMoveSendCrud":
                {
                    Debug.WriteLine(((AccountMoveSendCrud)sender).saveData);

                    if (((AccountMoveSendCrud)sender).saveData)
                    {
                        if (((AccountMoveSendCrud)sender).isNewData)
                        {
                            var task = Task.Run(async () =>
                            {
                                account_move_send accountMoveSendItem = new account_move_send();

                                //Se asigna para posteriormente almacenar
                                accountMoveSendItem = ((AccountMoveSendCrud)sender).accountMoveSend;                               
                                var nList = accountMovesSend.ToList();
                                nList.Add(accountMoveSendItem);
                                accountMovesSend = nList.ToArray();
                                Debug.WriteLine("Guardado nuevo");
                            });
                            Task.WaitAll(task);
                        }
                        else
                        {
                            var task = Task.Run(async () =>
                            {
                                var nList = accountMovesSend.ToList();

                                account_move_send accountMoveSendItem = new account_move_send();
                                accountMoveSendItem = ((AccountMoveSendCrud)sender).accountMoveSend;

                                nList[((AccountMoveSendCrud)sender).itemIndex] = ((AccountMoveSendCrud)sender).accountMoveSend;
                                accountMovesSend = nList.ToArray();
                                Debug.WriteLine("Guardado cambios");
                            });
                            Task.WaitAll(task);
                        }
                    }
                }
                break;
            case "AccountMoveSendProdCrud":
                {
                    if (((AccountMoveSendProdCrud)sender).saveData)
                    {
                        if (((AccountMoveSendProdCrud)sender).isNewData)
                        {
                            var task = Task.Run(async () =>
                            {
                                account_move_send[] accountMoveSendItem;

                                //Se asigna para posteriormente almacenar
                                accountMoveSendItem = ((AccountMoveSendProdCrud)sender).accountMovesSend;
                                var nList = accountMovesSend.ToList();
                                nList.AddRange(accountMoveSendItem);
                                accountMovesSend = nList.ToArray();
                                Debug.WriteLine("Guardado nuevo");
                            });
                            Task.WaitAll(task);
                        }
                        else
                        {
                            var task = Task.Run(async () =>
                            {
                                var nList = accountMovesSend.ToList();

                                account_move_send[] accountMoveSendItem;
                                accountMoveSendItem = ((AccountMoveSendProdCrud)sender).accountMovesSend;

                                foreach(var item in accountMoveSendItem) 
                                {
                                    nList[item.sequence] = item;
                                }

                                //nList[((AccountMoveSendProdCrud)sender).itemIndex] = ((AccountMoveSendProdCrud)sender).accountMoveSend;
                                
                                accountMovesSend = nList.ToArray();
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

    private async Task SummaryPayments()
    {
        totalPagado = 0;

        int idItem = 0;
        foreach (var di in accountMovesSend)
        {
            idItem++;
            //di.sequence = idItem;

            //decimal _totalPagado = di.amount;
            decimal _totalPagado = 0;
            Debug.WriteLine(_totalPagado);

            totalPagado += _totalPagado; // ParseTool.StringToDecimal(item.valor);
        }

        //collectionView.ItemsSource = accountMovesSend;

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

        if ((accountMovesSend == null || accountMovesSend.Length == 0) && editionMode && isFirstLoad)
        {
            if (Sel_Res_Partner == null)
            {
                ResPartnerDb resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
                //Sel_Res_Partner = await resPartnerDb.GetItem(Sel_AccountPaymentHeader.partner_id);
                Sel_Res_Partner = await resPartnerDb.GetItemsAsync(Sel_AccountMoveSendHeader.company_id, Sel_AccountMoveSendHeader.partner_id);
                txtCliente.Text = Sel_Res_Partner.id + "-" + Sel_Res_Partner.name;
            }

            if (Sel_Company_Id == null)
            {
                var cemp = App.Session.CurrentUserFront.empresas.Where(c => c.id == Sel_AccountMoveSendHeader.company_id).FirstOrDefault();

                Sel_Company_Id = new res_company
                {
                    id = Sel_AccountMoveSendHeader.company_id,
                    name = cemp.name
                };

                Title = "Grupo de Notas de Crédito-" + Sel_Company_Id.name;
            }

            //if (cobReciboCab != null && cobReciboCab.DETALLESPAGO!=null && cobReciboCab.DETALLESPAGO.Length > 0)
            if (Sel_AccountMoveSendHeader != null)
            {
                //dataItems = new CobReciboDet[0];
                //var ls_dataItems = JsonConvert.DeserializeObject<List<AccountPayment>>(cobReciboCab.DETALLESPAGO);
                AccountMoveSendDb accountPaymentDb = new AccountMoveSendDb(App.Session.odooConnection.DbNameSqlite);
                var ls_accountPayments = await accountPaymentDb.GetByParent(Sel_AccountMoveSendHeader.id);
                accountMovesSend = ls_accountPayments.ToArray();

                AccountMoveLineSendDb accountPaymentLines = new AccountMoveLineSendDb(App.Session.odooConnection.DbNameSqlite);

                foreach (var accountMoveSend in accountMovesSend)
                {
                    var apl = await accountPaymentLines.GetItemsAsync(accountMoveSend);

                    if (apl.Count() > 0)
                    {
                        accountMoveSend.lines = apl.ToArray();
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
            foreach (var di in accountMovesSend)
            {
                idItem++;
                di.sequence = idItem;
            }

            Title = "Grupo de Notas de Crédito-" + Sel_Company_Id.name;
            //collectionView.ItemsSource = dataItems;
            //await SummaryPayments();
        }

        isFirstLoad = false;

        if (accountMovesSend!=null)
            collectionView.ItemsSource = accountMovesSend;

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
        AccountMoveSendHeaderDb database = new AccountMoveSendHeaderDb(App.Session.odooConnection.DbNameSqlite);
        DateTime fechaActual = DateTime.Now;

        if (editionMode)
        {
            
        }

        //var dataResult = await cobCarteraCab.GetItemsAsync();
        //var cliente = dataResult.Where(cc => cc.CODEMPRESA == _cobCarteraCab.CODEMPRESA &&
        //cc.CODCLIENTE == _cobCarteraCab.CODCLIENTE).FirstOrDefault();

        if (Sel_AccountMoveSendHeader != null)
        {
            //TODO: SI GUARDA Y SE ENVÍA CON LA FECHA CORTADA NO GUARDA BIEN EL API
            // NO CORTAR LA FECHA AQUÍ
            //string fechaActual = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Substring(0,10).Trim();

            //string fechaActual = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        

            //Guardar info
            AccountMoveSendHeader accountPaymentHeader = new AccountMoveSendHeader();
            accountPaymentHeader.company_id = Sel_AccountMoveSendHeader.company_id;
            
            //Update
            if (Sel_AccountMoveSendHeader != null)
            {
                accountPaymentHeader.request_name = Sel_AccountMoveSendHeader.request_name;
                accountPaymentHeader.id = Sel_AccountMoveSendHeader.id;
            }

            accountPaymentHeader.create_datetime = fechaActual;
            accountPaymentHeader.uid = App.Session.CurrentUser.uid;
            accountPaymentHeader.partner_id = Sel_AccountMoveSendHeader.partner_id;
            accountPaymentHeader.partner_name = Sel_AccountMoveSendHeader.partner_name;
            //accountPaymentHeader.payment_amount = totalPagado; //.ToString(App.Session.ApplicationCultureInfo);
            //accountPaymentHeader.total_due = Sel_AccountMoveSendHeader.total_due;
            accountPaymentHeader.request_status = DMSA.Models.MoveStatus.PENDIENTE;
            //
            accountPaymentHeader.emailCustomer = Sel_AccountMoveSendHeader.emailCustomer;
            //accountPaymentHeader.CERRADO = "N";
            accountPaymentHeader.username = App.Session.CurrentUser.nombres;

            List<account_move_send> _accountPayment = new List<account_move_send>();

            //Se obtienen las formas de pago para almacenar            
            _accountPayment = accountMovesSend.ToList();

                       
            //codusuario
            //cabeceraCobro
            //detallesCobro
            //detallesDocumentos
            if (editionMode)
            {
                await database.UpdateAsync(accountPaymentHeader);

                _accountPayment.ForEach(item => item.parent_id = accountPaymentHeader.id);

                AccountMoveSendDb accountPaymentDb = new AccountMoveSendDb(App.Session.odooConnection.DbNameSqlite);
                AccountMoveLineSendDb accountPaymentInvoiceLineDb = new AccountMoveLineSendDb(App.Session.odooConnection.DbNameSqlite);
                //Se eliminan detalles previos para almacenar los nuevos
                //TODO: Se puede considerar crear un algoritmo de reemplazo de datos
                await accountPaymentDb.DeleteItemOfParent(accountPaymentHeader);

                //accountPaymentDb.InsertBatchAsync(cobReciboDet.ToArray());

                foreach(var accountPayment in _accountPayment)
                {
                    //Primero se eliminan las lineas del pago
                    await accountPaymentInvoiceLineDb.DeleteItemOfParent(accountPayment);

                    await accountPaymentDb.InsertAsync(accountPayment);
                    int accPayId = accountPayment.id;

                    if (accountPayment.lines != null)
                    {
                        foreach (var accountMoveSendItem in accountPayment.lines)
                        {
                            accountMoveSendItem.parent_move_id = accPayId;
                            await accountPaymentInvoiceLineDb.InsertAsync(accountMoveSendItem);
                        }
                    }
                    else
                    {
                        //Mostrar mensaje, no se agregó la línea porque probablemente no tenia valor asignado el documento
                    }
                }
                
                //Se obtienen detalles
            }
            
        }
        else
        {
            if (Sel_Res_Partner == null)
            {
                await Toast.Make("No se ha seleccionado cliente para la creación del NC.").Show();
                return;
            }

            AccountMoveSendHeader accountPaymentHeader = new AccountMoveSendHeader();
            //TODO: Asignación de los datos del pago nuevo

            //accountPaymentHeader.company_id = Sel_Res_Partner.company_id;
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

            List<account_move_send> _accountMoveSend = new List<account_move_send>();

            //Se obtienen las formas de pago para almacenar            
            _accountMoveSend = accountMovesSend.ToList();

            await database.InsertAsync(accountPaymentHeader);

            //Se obtiene el nuevo ID
            int newId = accountPaymentHeader.id;

            _accountMoveSend.ForEach(item => item.parent_id = newId);

            AccountMoveSendDb accountPaymentDb = new AccountMoveSendDb(App.Session.odooConnection.DbNameSqlite);
            AccountMoveLineSendDb accountPaymentInvoiceLineDb = new AccountMoveLineSendDb(App.Session.odooConnection.DbNameSqlite);
            //accountPaymentDb.InsertBatchAsync(cobReciboDet.ToArray());

            foreach (var accountMoveSendItem in _accountMoveSend)
            {
                await accountPaymentDb.InsertAsync(accountMoveSendItem);
                int accPayId = accountMoveSendItem.id;

                foreach (var accountPaymentInvoiceLine in accountMoveSendItem.lines)
                {
                    accountPaymentInvoiceLine.parent_move_id = accPayId;
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
        Sel_Res_Partner = null;
        txtCliente.Text = "<NO SELECCIONADO>";
        //ClearItems();
    }

    readonly PopupSizeConstants popupSizeConstants;

    async void btnCustomer_Clicked(object sender, EventArgs e)
    {
        //var returnResultPopup = new PopupResPartnerSelect(popupSizeConstants);
        ////var empresa = (res_company) SelectorCmp.SelectedItem;
        ////returnResultPopup.Company = new Company()
        ////{
        ////    id = empresa.id,
        ////    name = empresa.name,
        ////};

        //returnResultPopup.Company = new res_company()
        //{
        //    id = Sel_Company_Id.id,
        //    name = Sel_Company_Id.name,
        //};

        ////Evita que se cierre cuando se haga clic (tap) fuera de la ventana
        //returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        //if (!isWindows)
        //    returnResultPopup.Size = this.popupSizeConstants.Large;

        //var result = await PopupExtensions.ShowPopupAsync(this, returnResultPopup);
        ////var result = await this.ShowPopupAsync(returnResultPopup);

        //if (result != null)
        //{
        //    Sel_Res_Partner = (res_partner)result;
        //    txtCliente.Text = Sel_Res_Partner.id.ToString() + " - " + Sel_Res_Partner.name;
        //    //_res_partnerItem = resPartner;
        //}

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

    //async void ResultPopupAccountMove(object sender, EventArgs e)
    //{
    //    var returnResultPopup = new PopupAccountMoves(popupSizeConstants);
    //    returnResultPopup.partner = Sel_Res_Partner;
    //    //var empresa = (res_company) SelectorCmp.SelectedItem;
    //    //returnResultPopup.Company = new Company()
    //    //{
    //    //    id = empresa.id,
    //    //    name = empresa.name,
    //    //};

    //    returnResultPopup.Company = new res_company()
    //    {
    //        id = Sel_Company_Id.id,
    //        name = Sel_Company_Id.name,
    //    };

    //    //Evita que se cierre cuando se haga clic (tap) fuera de la ventana
    //    returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

    //    if (!isWindows)
    //        returnResultPopup.Size = this.popupSizeConstants.Large;

    //    var result = await PopupExtensions.ShowPopupAsync(this, returnResultPopup);
    //    //var result = await this.ShowPopupAsync(returnResultPopup);

    //    if (result != null)
    //    {
    //        //var resPartner = (res_partner)result;
    //        //txtCliente.Text = resPartner.id.ToString() + " - " + resPartner.name;
    //        //_res_partnerItem = resPartner;
    //    }
    //}
}