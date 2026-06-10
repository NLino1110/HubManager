using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using DMCobranzas.Controls;
using DMCobranzas.Controls.Tools;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.Tools;
using DMSA.Sync.Core.Controls.Popups;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Reponses;
using DMSA.Sync.Core.Update.Cloud;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Windows.Input;

namespace DMCobranzas.AppPages.Sys;

public partial class Connections : TabbedPage
{
    string pin_code = "1381";

    public ObservableCollection<string> Tables { get; set; } = new();

    public ICommand DeleteTableCommand { get; }

    UserAccessDb userdb { get; set; }

    private async void LoadTables()
    {
        if(userdb == null)
        {
            return;
        }

        var tables = await userdb.GetTablesAsync();
        var ordered = tables.OrderBy(t => t).ToList();
        Tables.Clear();
        foreach (var t in ordered)
            Tables.Add(t);
    }

    private async Task DeleteTable(string tableName)
    {
        await userdb.DropTableAsync(tableName);        
        await Toast.Make(tableName + " eliminado correctamente").Show();
    }

    public Connections()
	{
		InitializeComponent();

        _database = new OdooConnectionDb();

        LoadConnections();

        DeleteTableCommand = new Command<string>(async (tableName) =>
        {
            var resultPopup = await this.ShowPopupAsync<PasswordPromptResult>(new PasswordPrompt("Ingrese el pin correcto"));
            
            if (resultPopup?.Result?.IsAccepted == true)
            {
                if (resultPopup.Result.Password != pin_code)
                {
                    await Toast.Make("Pin incorrecto, no se eliminar� la tabla").Show();
                    return;
                }
                else
                {
                    await DeleteTable(tableName);
                }
            }            
        });

        SaveCommand = new Command(async () => await SaveConnection());
        NewCommand = new Command(NewConnection);
        DeleteCommand = new Command(async () => await DeleteConnection());
        
        BindingContext = this;

        IDispatcherTimer timer;

        timer = Dispatcher.CreateTimer();
        timer.IsRepeating = false;
        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.Tick += async (s, e) =>
        {
            AppSettingsDb appSettingsDb = new AppSettingsDb(App.Session.odooConnection.DbNameSqlite);
            await appSettingsDb.InitDefault();
            var appSettingItems = await appSettingsDb.GetItemsAsync();
            
            lblDbPath.Text = appSettingsDb.GetDbPath();
            timer.Stop();
        };

        timer.Start();
        
    }

    private bool _isOfflineMode;
    public bool IsOfflineMode
    {
        get => _isOfflineMode;
        set => _isOfflineMode = value;
    }

    private OdooConnectionDb _database;

    private ObservableCollection<OdooConnection> _connections;
    public ObservableCollection<OdooConnection> ConnectionsItems
    {
        get => _connections;
        set
        {
            _connections = value;
            OnPropertyChanged();
        }
    }

    private OdooConnection _selectedConnection = new OdooConnection();
    public OdooConnection SelectedConnection
    {
        get => _selectedConnection;
        set
        {
            _selectedConnection = value ?? new OdooConnection();
            OnPropertyChanged();
        }
    }

    private string _statusMessage;
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand NewCommand { get; }
    public ICommand DeleteCommand { get; }

    private async void LoadConnections()
    {
        var items = await _database.GetItemsAsync();
        ConnectionsItems = new ObservableCollection<OdooConnection>(items);
    }

    private void NewConnection()
    {
        SelectedConnection = new OdooConnection();
    }

    private CancellationTokenSource _cancellationTokenSource;

    private async Task SaveConnection()
    {
        var resultPopup = await this.ShowPopupAsync<PasswordPromptResult>(new PasswordPrompt("Ingrese el pin correcto"));

        if (resultPopup?.Result?.IsAccepted == true)
        {
            if (resultPopup.Result.Password != pin_code)
            {
                await Toast.Make("Pin incorrecto, no se guardar�n cambios").Show();
                return;
            }            
        }
        else
        {
            return;
        }

        try
        {
            if (SelectedConnection.Id == 0)
            {
                if (SelectedConnection.Password != string.Empty)
                {
                    try
                    {
                        string decrypt = CryptoHelper.Decrypt(SelectedConnection.Password);
                        Debug.WriteLine(decrypt);
                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine("Error:" + fe.Message);                        
                        SelectedConnection.Password = CryptoHelper.Encrypt(SelectedConnection.Password);
                    }
                    catch (CryptographicException ex)
                    {
                        Console.WriteLine("Error al descifrar: " + ex.Message);
                        SelectedConnection.Password = CryptoHelper.Encrypt(SelectedConnection.Password);
                    }
                }

                await _database.InsertAsync(SelectedConnection);
                StatusMessage = "Connection added successfully.";
            }
            else
            {
                await _database.UpdateAsync(SelectedConnection);
                StatusMessage = "Connection updated successfully.";
            }
            LoadConnections();
            NewConnection();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task DeleteConnection()
    {
        if (SelectedConnection?.Id != 0)
        {
            try
            {
                //await _database.DeleteAsync(SelectedConnection);
                StatusMessage = "Connection deleted successfully.";
                LoadConnections();
                NewConnection();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }
        else
        {
            StatusMessage = "Select a connection to delete.";
        }
    }

    public async Task RemoveAll()
    {
        await ResetCompanies();
        await ResetUsers();
        _connections = new ObservableCollection<OdooConnection>();
        await _database.DeleteAllAsync(x => x.Id > 0);
        await _database.InitDefault();
        LoadConnections();
    }

    public async Task ResetCompanies()
    {
        var companiesDb = new CompanyDb(App.Session.odooConnection.DbNameSqlite);
        await companiesDb.DeleteAllAsync(x => x.id > 0);
    }

    public async Task ResetUsers()
    {
        var userAccessDb = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
        await userAccessDb.DeleteAllAsync(x => x.uid > 0);
    }


    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void btnEncrypt_Clicked(object sender, EventArgs e)
    {
        SelectedConnection.Password = CryptoHelper.Encrypt(SelectedConnection.Password);
        OnPropertyChanged(nameof(SelectedConnection));
    }

    private async void btnDecrypt_Clicked(object sender, EventArgs e)
    {
        string dp = "";
        dp = CryptoHelper.Decrypt(SelectedConnection.Password);
        EntryPasswordDb.Title = "Password (" + dp + ")";
        OnPropertyChanged(nameof(EntryPasswordDb));
    }

    private async void btnUploadDB_Clicked(object sender, EventArgs e)
    {
        //bool readyForContinue = false;
        var resultPopup = await this.ShowPopupAsync<PasswordPromptResult>(new PasswordPrompt("Ingrese el pin correcto"));

        if (resultPopup?.Result?.IsAccepted == true)
        {
            if (resultPopup.Result.Password != pin_code)
            {
                await Toast.Make("Pin incorrecto, no se subirá la base de datos").Show();                
                return;
            }
            //else
            //{
            //    readyForContinue = true;
            //}
        }
        else
        {
            return;
        }

        if (pickerDb == null || pickerDb.SelectedItem == null)
            return;

        var selectedConnection = (OdooConnection)pickerDb.SelectedItem;

        string dbNameSqlite = selectedConnection.DbNameSqlite;

        await Toast.Make($"Se empezará a subir {dbNameSqlite} a la nube, espere un momento").Show();

        //userdb = new UserAccessDb(selectedConnection.DbNameSqlite);

        Pipeline pipeline = new Pipeline();

        bool successUpload = await pipeline.UploadToFileNoAttach(dbNameSqlite, dbNameSqlite);

        if (successUpload)
        {
            await Toast.Make($"Enviado correctamente {dbNameSqlite}").Show();
        }
    }

    private async void btnDownloadDb_Clicked(object sender, EventArgs e)
    {
        var resultPopup = await this.ShowPopupAsync<PasswordPromptResult>(new PasswordPrompt("Ingrese el pin correcto"));

        if (resultPopup?.Result?.IsAccepted == true)
        {
            if (resultPopup.Result.Password != pin_code)
            {
                await Toast.Make("Pin incorrecto, no se subirá la base de datos").Show();
                return;
            }
        }
        else
        {
            return;
        }

        Pipeline pipeline = new Pipeline();

        bool packageReady = await pipeline.ExistAttachRecord();
        bool isValidData = await pipeline.IsValidData();

        if (!packageReady && !isValidData)
        {
            var packFound = await pipeline.NewestZipPack();

            if (packFound != null)
            {
                await SqliteDbBase<object>.CloseDatabaseAsync();
                
                if (await pipeline.DownloadSqliteZip(
                    packFound,
                    true,
                    async (current, total) => { await UpdateProgressState(null, current, total, "Archivos"); }))
                {
                    await pipeline.InsertAttachRecord(packFound);
                    await pipeline.ResetUserData();
                }
                else
                {
                    await Toast.Make("Hubo un error al descargar/descomprimir archivo.").Show();
                }

                await Toast.Make("Actualización rápida terminada").Show();

                var databaseUserAccess = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
                await databaseUserAccess.FixMissingCurrentUser();
            }
        }
    }

    private async Task UpdateProgressState(ProgressBarPage progressBarPage, int current, int total, string title)
    {
        if (total <= 0) total = 1;

        current = Math.Min(current, total - 1);

        int displayPage = current + 1;
        double percent = (double)displayPage / total;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            progressBarPage.SetSubTitle($"{title} - Página {displayPage} de {total}");
            progressBarPage.SetPercent(percent);
        });
    }

    private async void btnRebuildSettings_Clicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("Rehacer configuración", "¿Desea continuar?", "Sí", "No");
        if (!result)
        {
            return;
        }

        await RemoveAll();
        await Toast.Make("Ejecución correcta...").Show();
    }

    private void ConnectionsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            var selected = e.CurrentSelection[0] as OdooConnection;
            if (selected != null)
            {                
                SelectedConnection = selected;
            }
        }
    }

    private async void btnDBTask_Compact(object sender, EventArgs e)
    {
        ContentPage contentPage = (ContentPage) this.CurrentPage;
        await UITools.ShowLoadingPopup(contentPage);
        await UITools.SetNotifyLoadingPopup("Ejecutando Vaccum...");
        //ExecuteTask executeTask = new ExecuteTask();
        //await executeTask.Vaccum();
        await UITools.HideLoadingPopup();
    }

    private async void btnDBTask_Reload(object sender, EventArgs e)
    {        
        LoadTables();        
    }    

    private async void DbPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var control = (Picker)sender;
        Debug.WriteLine(control.SelectedItem);

        if (control == null || control.SelectedItem == null)
            return;
        
        var selectedConnection = (OdooConnection) control.SelectedItem;
        userdb = new UserAccessDb(selectedConnection.DbNameSqlite);
        LoadTables();
    }
}