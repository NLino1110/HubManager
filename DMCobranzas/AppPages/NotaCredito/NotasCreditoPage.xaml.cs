//using CloudKit;
using ApiManager;
using DMCobranzas.Controls.Modals;
using DMCobranzas.Models;
using DMCobranzas.Models.Specials;
using DMCobranzas.Services.ApiHub;
using DMCobranzas.Settings.helpers;
using DMCobranzas.Settings.Sqlite;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Sample;
using CommunityToolkit.Maui.Sample.Models;
using CommunityToolkit.Maui.Sample.Pages;
//using CommunityToolkit.Maui.Sample.Pages.Views;
using CommunityToolkit.Maui.Sample.ViewModels.Views;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Collections;
using DMSA.Models.General;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
//using Microsoft.Maui.Controls.Compatibility;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Input;
using System.Xml.Linq;
using DMSA.Models.Odoo.DMCobranzas;

namespace DMCobranzas.AppPages.NotaCredito;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class NotasCreditoPage : ContentPage
{
    int uid { get; set; } = 0;
    res_company[] Empresas { get; set; }

    public bool isWindows { get; set; } = false;
   
    //public ItemsGroupColG _items { get; set; } //= new ObservableCollection<ItemsGroup>();
    
    //public ObservableCollection<ItemsGroupNC> _items { get; set; } //= new ObservableCollection<ItemsGroup>();
    public ObservableCollection<ItemsGroupMoveSend> _items { get; set; } //= new ObservableCollection<ItemsGroup>();

    readonly PopupSizeConstants popupSizeConstants;
    readonly CsharpBindingPopupViewModel csharpBindingPopupViewModel;

    public NotasCreditoPage()
	{
		InitializeComponent();

        if (popupSizeConstants == null)
        {
            this.popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        }
        else
        {
            //this.popupSizeConstants = popupSizeConstants;
        }

        //this.csharpBindingPopupViewModel = csharpBindingPopupViewModel;

        _items = new ObservableCollection<ItemsGroupMoveSend>();

        //SendItemCommand = new Command(SendItem);
        SendItemCommand = new Command(SendItemHeader);
        DeleteCommand = new Command(DeleteItem);
        EditCommand = new Command(EditItem);
        TicketCommand = new Command(TicketItem);

        //var task = Task.Run(() =>
        //{
        //    //await LoadData();
        //    btnBuscar_Clicked(null, null);
        //});

        //Task.WaitAll(task);

        if (App.Session.CurrentUser.empresas != null)
        {
            Empresas = App.Session.CurrentUser.empresas.OrderBy(x=>x.id).ToArray();
            uid = App.Session.CurrentUser.uid;
        }

        SelectorCmp.ItemsSource = Empresas;
        SelectorCmp.SelectedIndex = 0;
        
        dateIni.Date = DateTime.Today.AddMonths(-1);

        isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;

        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        //IDispatcherTimer timer;

        //timer = Dispatcher.CreateTimer();
        //timer.Interval = TimeSpan.FromMilliseconds(500);
        //timer.IsRepeating = false;
        //timer.Tick += async (s, e) =>
        //{
        //    await LoadData();

        //    timer.Stop();
        //};
        //timer.Start();

        //var task = Task.Run(async () =>
        //{
        //    await Task.Delay(2000);
        //    if (isEmptyDb)
        //    {                
        //        await DisplayAlert("Alert", "You have been alerted", "OK");
        //    }
        //});
        //task.Wait();

        //IDispatcherTimer timer;

        //timer = Dispatcher.CreateTimer();
        //timer.Interval = TimeSpan.FromMilliseconds(500);
        //timer.Tick += async (s, e) =>
        //{
        //    await LoadData();
        //    timer.Stop();
        //};
        //timer.Start();
    }

    private async void btnBuscar_Clicked(object sender, EventArgs e)
    {
        _items.Clear();
        await LoadData();
    }

    private async Task LoadData()
    {
        ///collectionView: Contiene una referencia directa que en teoría debería bastar para que se 
        /// actualice la visualizacion de forma directa, no lo logra, por lo cual se están realizando
        /// 2 asignaciones. Considerar optimización para evitar dicho comportamiento.
        await UITools.ShowLoading(_absoluteLayout);
        Debug.WriteLine("Load data.....");
        try
        {
            _items.Clear();

            //TODO: Revisar, no deberíamos tener que volver a reasignar la variable
            _items = new ObservableCollection<ItemsGroupMoveSend>();
            //var task = Task.Run(async () =>
            //{
            var se = (res_company) SelectorCmp.SelectedItem;

            DateTime dateEndField = dateEnd.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            var database = new AccountMoveSendHeaderDb();
            var ls_items = await database.GetItemsAsync(se.id, dateIni.Date, dateEndField,
                 App.Session.CurrentUser.uid, txtSearch.Text.Trim());

            //Se ordenan los registros por FECHA
            //ls_items.Sort((x, y) => x.FECHA.CompareTo(y.FECHA));
            ls_items = ls_items.OrderByDescending(c => c.create_datetime).ToList();

            // Variable para almacenar la fecha actual
            DateTime currentFecha = DateTime.MinValue;
            List<AccountMoveSendHeader> registrosGrupo = new List<AccountMoveSendHeader>();

            foreach (var _accountMoveItem in ls_items)
            {
                if (_accountMoveItem.request_status == DMSA.Models.MoveStatus.ENVIANDO)
                {
                    // Obtén la fecha y hora actual
                    DateTime fechaActual = DateTime.Now;
                    TimeSpan diferenciaDeTiempo = fechaActual - _accountMoveItem.create_datetime;

                    if (diferenciaDeTiempo.TotalMinutes > 5)
                    {
                        _accountMoveItem.request_status = DMSA.Models.MoveStatus.PENDIENTE;
                        await Toast.Make("Solicitud N/C " + _accountMoveItem.request_name + " se regreso a estado PENDIENTE por inactividad.").Show();
                    }
                }

                DateTime fecha = _accountMoveItem.create_datetime; // Convertir la cadena de fecha a DateTime

                if (fecha.Date != currentFecha.Date) // Si la fecha cambia
                {
                    // Ejecutar la función que recibe los registros de la fecha anterior
                    await ProcessItemsGroupMoveSendHeader(registrosGrupo, _items, se);

                    currentFecha = fecha.Date; // Actualizar la fecha actual
                    registrosGrupo = new List<AccountMoveSendHeader>();
                }

                registrosGrupo.Add(_accountMoveItem);
            }

            await ProcessItemsGroupMoveSendHeader(registrosGrupo, _items, se);            

            collectionView.ItemsSource = _items;

        }
        catch(Exception ex)
        {
            Debug.WriteLine("Error: " + ex.Message);
        }

        await UITools.HideLoading(_absoluteLayout);
    }

    private async Task ProcessItemsGroup(List<account_move_send> registrosGrupo,
        ObservableCollection<ItemsGroupNC> _items,
        res_company se)
    {
        bool FoundCerrado = false;

        //foreach (var item in registrosGrupo)
        //{
        //    AccountPaymentDailyDb cobCierreDb = new AccountPaymentDailyDb();
        //    var cierres = await cobCierreDb.GetItemAsync(se.id, item.create_date.ToString("yyyy-MM-dd"));
        //    //YA HA SIDO CERRADO
        //    if (cierres != null)
        //    {
        //        item.group_status = "closed";
        //        FoundCerrado = true;
        //    }
        //}

        if (registrosGrupo.Count() > 0)
        {
            var newGroup = new ItemsGroupNC(registrosGrupo[0].create_date.ToString("yyyy-MM-dd") , 
                registrosGrupo[0].create_date.ToString("yyyy-MM-dd"),
                registrosGrupo[0].create_date.ToString("yyyy-MM-dd"), registrosGrupo);
            newGroup.showButtonCierre = !FoundCerrado;
            _items.Add(newGroup);
        }
    }

    private async Task ProcessItemsGroupMoveSendHeader(List<AccountMoveSendHeader> registrosGrupo,
        ObservableCollection<ItemsGroupMoveSend> _items,
        res_company se)
    {
        bool FoundCerrado = false;

        if (registrosGrupo.Count() > 0)
        {
            var newGroup = new ItemsGroupMoveSend(registrosGrupo[0].create_datetime.ToString("yyyy-MM-dd"),
                registrosGrupo[0].create_datetime.ToString("yyyy-MM-dd"),
                registrosGrupo[0].create_datetime.ToString("yyyy-MM-dd"), registrosGrupo);
            newGroup.showButtonCierre = !FoundCerrado;
            _items.Add(newGroup);
        }
    }

    public ICommand DeleteCommand { get; set; }

    private async void DeleteItem(object obj)
    {
        AccountMoveSendHeader _accountMoveSendHeader = (AccountMoveSendHeader)obj;
        AccountMoveSendDb accountMoveSendDb = new AccountMoveSendDb();
        var movesItems = await accountMoveSendDb.GetByParent(_accountMoveSendHeader.id);

        int countMoves = movesItems.Count;

        bool answer = await DisplayAlert("Eliminar solicitud", $"Está seguro que desea eliminar esta solicitud? {countMoves} Nota(s) de Crédito", "Confirmar", "Cancelar");
        //Debug.WriteLine("Answer: " + answer);
        if (!answer)
        {
            return;
        }

        AccountMoveSendHeaderDb accountMoveSendHeaderDb = new AccountMoveSendHeaderDb();

        await accountMoveSendHeaderDb.DeleteRecursive(_accountMoveSendHeader);
        //accountMoveSendHeaderDb.

        await Toast.Make(_accountMoveSendHeader.partner_name + " eliminado!" ).Show();

        await LoadData();
    }

    public ICommand SendItemCommand { get; set; }

    private async void SendItemHeader(object obj)
    {
        AccountMoveSendHeader _accountMoveSendHeader = (AccountMoveSendHeader)obj;
        AccountMoveSendDb accountMoveSendDb = new AccountMoveSendDb();
        var movesItems = await accountMoveSendDb.GetByParent(_accountMoveSendHeader.id);

        int countMoves = movesItems.Count;

        bool answer = await DisplayAlert("Envío de solicitud", $"Está seguro que desea enviar esta solicitud? {countMoves} Nota(s) de Crédito", "Confirmar", "Cancelar");
        //Debug.WriteLine("Answer: " + answer);
        if (!answer)
        {
            return;
        }

        await UITools.ShowLoadingPopup(this);

        var resultCheck = await SendController.CheckCreditNoteOverdraf(_accountMoveSendHeader);

        if (resultCheck.result.Length > 0)
        {
            await UITools.HideLoadingPopup();

            //Quizás nuevo popup con datos formateados
            await DisplayAlert("Riesgo de sobregiro",
                "Al parecer se han ingresado valores inadecuados para las devoluciones, modifíquelos y vuelva a intentar.",
                "Cancelar");

            Debug.WriteLine(resultCheck.error.message);

            return;
        }

        var result = await SendController.SendRequestCreditNote(_accountMoveSendHeader);

        if (result.result > 0)
        {
            await Toast.Make("Envío de solicitud(es) correcto").Show();
        }
        else
        {
            string error_message = "";
            if(result.error != null)
            {
                error_message = result.error.message;
            }

            await Toast.Make("Envío de solicitud(es) erroneo: " + error_message).Show();
        }

        await UITools.HideLoadingPopup();


        //simplePopup.Close();
        await LoadData();
    }

    public ICommand EditCommand { get; set; }

    private async void EditItem(object obj)
    {
        Debug.WriteLine("EditItem");
        AccountMoveSendView objPage = new AccountMoveSendView();                
        objPage.Sel_AccountMoveSendHeader = (AccountMoveSendHeader)obj;
        objPage.editionMode = true;
        objPage.Disappearing += NewGroup_Disappearing;
        await Navigation.PushAsync(objPage, false);
    }

    public ICommand TicketCommand { get; set; }

    private async void TicketItem(object obj)
    {
        Debug.WriteLine("PrintItem");
        PrintView objPage = new PrintView();
        //CobrosMain objPage = new CobrosMain();
        //Se asigna la empresa seleccionada

        //((ItemsGroup)obj)[0]

        //objPage.setCobReciboCab((CobReciboCab)obj);
        string printTemplate = "";
        Services.Templates.Processor processor = new Services.Templates.Processor();
        switch(obj.GetType().Name)
        {
            case "AccountMoveSendHeader":
                {
                    //printTemplate = await processor.Template_AccountMoveSendNC((account_move_send) obj);
                    printTemplate = await processor.Template_AccountMoveSendNC_V3((AccountMoveSendHeader)obj);
                }
                break;
        }
        
        objPage.setTemplate(printTemplate);

        //Se asigna título
        //obj.Title = "Cartera Clientes/" + se.nombre;
        //objPage.dataItem = (CobReciboCab)obj;
        //objPage.empresa = empresa;
        //objPage.SetTitle();
        await Navigation.PushAsync(objPage, false);
    }

    private async void EnviarNotaCredito(object obj)
    {
        //var secuencia = await database.obtenerSecuenciaRecibo(dataItem.CODEMPRESA, App.Session.CurrentUser.codusuario, fechaActual);
        //string secuencia_final = GenerarCodigoRecibo(App.Session.CurrentUser.codusuario, dataItem.CODEMPRESA, fechaActual, secuencia.ToString());
        Debug.WriteLine("EnviarNotaCredito");
    }

    private void Disappearing_NewNC(object sender, EventArgs e)
    {
        Debug.WriteLine("Busqueda cerrada");
        LoadDataByDispatcher();
        //throw new NotImplementedException();
    }

    void LoadDataByDispatcher()
    {
        IDispatcherTimer timer;

        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.IsRepeating = false;
        timer.Tick += async (s, e) =>
        {
            await LoadData();

            timer.Stop();
        };
        timer.Start();
    }

    void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;

        if (selectedIndex != -1)
        {
            LoadDataByDispatcher();
        }
    }

    private async void btnNewGroup_Clicked(object sender, EventArgs e)
    {
        AccountMoveSendView obj = new AccountMoveSendView();
        var se = (res_company)SelectorCmp.SelectedItem;
        obj.Sel_Company_Id = se;


        obj.Disappearing += NewGroup_Disappearing;
        //SelectorCmp.IsEnabled = false;    

        await Navigation.PushAsync(obj, false);
    }

    private void NewGroup_Disappearing(object sender, EventArgs e)
    {
        //Debug.WriteLine("Busqueda cerrada");
        LoadDataByDispatcher();
        //throw new NotImplementedException();
    }
}