using CommunityToolkit.Maui.Alerts;
using DMCobranzas.Models.Specials;
using DMCobranzas.Settings.helpers;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Controls.Popups;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using DMSA.Sync.Core.Update.Pusher;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMCobranzas.AppPages.NotaCredito;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class NotasCreditoPage : ContentPage
{
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged(nameof(IsLoading));
            Debug.WriteLine("_isLoading");
            Debug.WriteLine(_isLoading);
        }
    }

    int uid { get; set; } = 0;
    res_company[] Empresas { get; set; }

    public res_company Sel_Company_Id { get; set; }

    public bool isWindows { get; set; } = false;
   
    public ObservableCollection<ItemsGroupMoveSend> _items { get; set; } 

    readonly PopupSizeConstants popupSizeConstants;
    
    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
                return;

            _searchText = value;
            OnPropertyChanged();

            // aquí puedes ejecutar búsqueda si quieres
            //PerformSearch();
        }
    }

    public ICommand DeleteCommand { get; set; }

    public ICommand SendItemCommand { get; set; }

    public ICommand EditCommand { get; set; }

    public ICommand TicketCommand { get; set; }
    public NotasCreditoPage()
	{
		InitializeComponent();

        if (popupSizeConstants == null)
        {
            this.popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        }
        else
        {
            
        }

        _items = new ObservableCollection<ItemsGroupMoveSend>();

        //SendItemCommand = new Command(SendItem);
        SendItemCommand = new Command(SendItemHeader);
        DeleteCommand = new Command(DeleteItem);
        EditCommand = new Command(EditItem);
        TicketCommand = new Command(TicketItem);

        if (App.Session.CurrentUserFront.empresas != null)
        {
            Empresas = App.Session.CurrentUserFront.empresas.OrderBy(x=>x.id).ToArray();
            uid = App.Session.CurrentUserFront.uid;
        }

        SelectorCmp.ItemsSource = Empresas;
        SelectorCmp.SelectedIndex = 0;

        Sel_Company_Id = Empresas[0];

        dateIni.Date = DateTime.Today.AddMonths(-1);

        isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;

        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    private async void btnBuscar_Clicked(object sender, EventArgs e)
    {
        _items.Clear();
        await LoadData();
    }

    private async Task LoadData()
    {        
        if (IsLoading)
            return;

        IsLoading = true;

        Debug.WriteLine("Load data.....");
        try
        {
            _items.Clear();
            _items = new ObservableCollection<ItemsGroupMoveSend>();            
            DateTime dateEndField = dateEnd.Date.Value.AddHours(23).AddMinutes(59).AddSeconds(59);

            var database = new CreditNoteRequestGroupDb(App.Session.odooConnection.DbNameSqlite);
            var ls_items = await database.GetItemsAsync(Sel_Company_Id.id, dateIni.Date.Value, dateEndField,
                 App.Session.CurrentUserFront.uid, SearchText.Trim());

            //Se ordenan los registros por FECHA            
            ls_items = ls_items.OrderByDescending(c => c.create_datetime).ToList();

            // Variable para almacenar la fecha actual
            DateTime currentFecha = DateTime.MinValue;
            List<CreditNoteRequestGroup> registrosGrupo = new List<CreditNoteRequestGroup>();

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
                    await ProcessItemsGroupMoveSendHeader(registrosGrupo, _items, Sel_Company_Id);

                    currentFecha = fecha.Date; // Actualizar la fecha actual
                    registrosGrupo = new List<CreditNoteRequestGroup>();
                }

                registrosGrupo.Add(_accountMoveItem);
            }

            await ProcessItemsGroupMoveSendHeader(registrosGrupo, _items, Sel_Company_Id);            

            collectionView.ItemsSource = _items;

        }
        catch(Exception ex)
        {
            Debug.WriteLine("Error: " + ex.Message);
        }

        IsLoading = false;
    }

    private async Task ProcessItemsGroup(List<credit_note_request> registrosGrupo,
        ObservableCollection<ItemsGroupNC> _items,
        res_company se)
    {
        bool FoundCerrado = false;

        if (registrosGrupo.Count() > 0)
        {
            var newGroup = new ItemsGroupNC(registrosGrupo[0].create_date.ToString("yyyy-MM-dd") , 
                registrosGrupo[0].create_date.ToString("yyyy-MM-dd"),
                registrosGrupo[0].create_date.ToString("yyyy-MM-dd"), registrosGrupo);
            newGroup.showButtonCierre = !FoundCerrado;
            _items.Add(newGroup);
        }
    }

    private async Task ProcessItemsGroupMoveSendHeader(List<CreditNoteRequestGroup> registrosGrupo,
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

    private async void DeleteItem(object obj)
    {
        CreditNoteRequestGroup _accountMoveSendHeader = (CreditNoteRequestGroup)obj;
        CreditNoteRequestDb accountMoveSendDb = new CreditNoteRequestDb(App.Session.odooConnection.DbNameSqlite);
        var movesItems = await accountMoveSendDb.GetByParent(_accountMoveSendHeader.id);

        int countMoves = movesItems.Count;

        bool answer = await DisplayAlertAsync("Eliminar solicitud", $"Está seguro que desea eliminar esta solicitud? {countMoves} Nota(s) de Crédito", "Confirmar", "Cancelar");
        //Debug.WriteLine("Answer: " + answer);
        if (!answer)
        {
            return;
        }

        CreditNoteRequestGroupDb accountMoveSendHeaderDb = new CreditNoteRequestGroupDb(App.Session.odooConnection.DbNameSqlite);
        await accountMoveSendHeaderDb.DeleteRecursive(_accountMoveSendHeader);        
        await Toast.Make(_accountMoveSendHeader.partner_name + " eliminado!" ).Show();
        await LoadData();
    }

    private async void SendItemHeader(object obj)
    {
        CreditNoteRequestGroup _accountMoveSendHeader = (CreditNoteRequestGroup)obj;
        CreditNoteRequestDb accountMoveSendDb = new CreditNoteRequestDb(App.Session.odooConnection.DbNameSqlite);
        var movesItems = await accountMoveSendDb.GetByParent(_accountMoveSendHeader.id);

        int countMoves = movesItems.Count;

        bool answer = await DisplayAlertAsync("Envío de solicitud", $"Está seguro que desea enviar esta solicitud? {countMoves} Nota(s) de Crédito", "Confirmar", "Cancelar");
        //Debug.WriteLine("Answer: " + answer);
        if (!answer)
        {
            return;
        }

        await UITools.ShowLoadingPopup(this);

        //var resultCheck = await DebitCollection.CheckCreditNoteOverdraf(_accountMoveSendHeader);

        //if (resultCheck.result.Length > 0)
        //{
        //    await UITools.HideLoadingPopup();
            
        //    await DisplayAlert("Riesgo de sobregiro",
        //        "Al parecer se han ingresado valores inadecuados para las devoluciones, modifíquelos y vuelva a intentar.",
        //        "Cancelar");

        //    Debug.WriteLine(resultCheck.error.message);

        //    return;
        //}

        var result = await DebitCollection.SendRequestCreditNote(_accountMoveSendHeader);

        if (result != null && result.result!= null && result.result.Count > 0)
        {
            await Toast.Make("Envío de solicitud(es) correcto").Show();
        }
        else
        {
            string error_message = "";
            if(result != null && result.error != null)
            {
                error_message = result.error.data.message;
                error_message = ParseTool.CleanServerMessage_v1(error_message, true);
            }

            await Toast.Make("Envío de solicitud(es) erroneo: " + error_message).Show();
        }

        await UITools.HideLoadingPopup();
        await LoadData();
    }

    private async void EditItem(object obj)
    {
        Debug.WriteLine("EditItem");
        CreditNoteRequestGroupView objPage = new CreditNoteRequestGroupView();                
        objPage.Sel_CreditNoteRequestGroup = (CreditNoteRequestGroup)obj;
        objPage.editionMode = true;
        objPage.Disappearing += NewGroup_Disappearing;
        await Navigation.PushAsync(objPage, false);
    }

    private async void TicketItem(object obj)
    {
        Debug.WriteLine("PrintItem");
        PrintView objPage = new PrintView();        
        string printTemplateHtml = "";
        string printTemplatePlain = "";
        byte[] printTemplateData = null;
        Services.Templates.Processor processor = new Services.Templates.Processor();
        switch(obj.GetType().Name)
        {
            case "CreditNoteRequestGroup":
                {                    
                    (printTemplateData, printTemplateHtml, printTemplatePlain) = await processor.Template_AccountMoveSendNC((CreditNoteRequestGroup)obj);
                }
                break;
        }
        
        objPage.setTemplatePreview(printTemplateHtml);
        objPage.setTemplatePlain(printTemplatePlain);
        objPage.setData(printTemplateData);        
        await Navigation.PushAsync(objPage, false);
    }

    private void Disappearing_NewNC(object sender, EventArgs e)
    {
        Debug.WriteLine("Busqueda cerrada");
        LoadDataByDispatcher();
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
        CreditNoteRequestGroupView obj = new CreditNoteRequestGroupView();
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