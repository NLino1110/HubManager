using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using DMCobranzas.Controls;
using DMCobranzas.Controls.Tools;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.Abstract.Server;
using DMSA.Models.Odoo.Abstract.Server.Dto;
using DMSA.Models.Odoo.Tools;
using DMSA.Sync.Core.Controls.Popups;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Reponses;
//using DMSA.Sync.Core.Update.Cloud.v1_5;
using DMSA.Sync.Core.Update.Cloud.v2_0;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Windows.Input;
using static ApiManager.HubMnsaAttachment;

namespace DMCobranzas.AppPages.Sys;

public partial class Connections : TabbedPage
{
    string pin_code = "1381";

    public ObservableCollection<string> Tables { get; set; } = new();
    public ICommand DeleteTableCommand { get; }
    UserAccessDb userdb { get; set; }
    DatabaseStruct selectedDatabase { get; set; }
    public ObservableCollection<DatabaseStruct> _dbStructItems { get; set; }
    public ObservableCollection<DatabaseStruct> dbStructItems
    {
        get => _dbStructItems;
        set
        {
            _dbStructItems = value;
            OnPropertyChanged();
        }
    }

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

        dbStructItems = new ObservableCollection<DatabaseStruct>();

        foreach (var item in items)
        {
            dbStructItems.Add(
                new DatabaseStruct
                {
                    Host = item.Host,
                    Name = item.DbNameSqlite,
                    Size = 0,
                    Path = item.DbNameSqlite,
                    OriginalDBName = item.DbName
                });

            dbStructItems.Add(
                new DatabaseStruct
                {
                    Host = item.Host,
                    Name = item.DbNameSqlite + "_static",
                    Size = 0,
                    Path = item.DbNameSqlite + "_static",
                    OriginalDBName = item.DbName
                });
        }
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
        if (selectedDatabase == null)
        {
            await Toast.Make($"Seleccione base de datos").Show();
            return;
        }

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

        if (selectedDatabase == null)
            return;        

        string dbNameSqlite = selectedDatabase.Name;

        await Toast.Make($"Se empezará a subir {dbNameSqlite} a la nube, espere un momento").Show();

        //V1_5
        //var pipeline = new Pipeline();
        //(var attachData, bool successUpload, responseUpload file_upload_response) = await pipeline.UploadSqliteZipNonAttach(dbNameSqlite);

        //string message_server = "";
        //if (file_upload_response != null)
        //    message_server = file_upload_response.message;

        //if (successUpload)
        //{
        //    await Toast.Make($"Enviado correctamente {dbNameSqlite} - {message_server}").Show();
        //}
        //else
        //{            
        //    await Toast.Make($"ERROR: No se envió correctamente {dbNameSqlite} - {message_server}").Show();
        //}

        var pipeline = new Pipeline();

        (PackageResponseDto file_upload_response, bool successUpload) = await pipeline.UploadSqliteZip(selectedDatabase, null);

        if (file_upload_response.success_upload)
        {
            await Toast.Make($"Enviado correctamente {dbNameSqlite}").Show();
        }
        else
        {
            await Toast.Make($"ERROR: No se envió correctamente {dbNameSqlite} - {file_upload_response.error}").Show();
        }

    }

    private bool MatchPackageWithSelection(string packageName)
    {
        var dbNameSqlite = selectedDatabase.Name;

        if (string.IsNullOrWhiteSpace(packageName))
            return false;

        var parts = packageName.Split('_');

        if (parts.Length < 5)
            return false;

        var app_id = parts[1];

        var nameParts = parts.Skip(2).Take(parts.Length - 3);

        var extractedDbName = string.Join("_", nameParts);

        return extractedDbName.Equals(dbNameSqlite, StringComparison.OrdinalIgnoreCase)
               && app_id == App.Session.AppCodeOdoo;
    }

    private async void btnDownloadDb_Clicked(object sender, EventArgs e)
    {
        if (selectedDatabase == null)
        {
            await Toast.Make($"Seleccione base de datos").Show();
            return;
        }

        string packageName = await DisplayPromptAsync(
            "Restauración de datos",
            "Ingrese Nombre del paquete que desea restaurar:",
            "OK",
            "Cancelar",
            placeholder: "ej. pk_01_prod1_macronegocios_00000000000000",
            maxLength: 50,
            keyboard: Keyboard.Text
        );

        if (String.IsNullOrEmpty(packageName))
        {
            await Toast.Make($"Nombre de paquete nulo o vacío").Show();
            return;
        }

        if (!MatchPackageWithSelection(packageName))
        {
            await Toast.Make($"Nombre de paquete no coincide con la base de datos o no pertenece a esta aplicación.").Show();
            return;
        }

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

        await Toast.Make($"Inciada restauración de base de datos {packageName}").Show();

        //V1_5
        //var pipeline = new Pipeline();

        //await SqliteDbBase<object>.CloseDatabaseAsync();        
        //if (await pipeline.DownloadSqliteZipByPackage(packageName, true, null))
        //{

        //}
        //else
        //{
        //    await Toast.Make("Hubo un error al descargar/descomprimir archivo.").Show();
        //}

        //await Toast.Make($"Restauración de base de datos terminada {packageName}").Show();

        //V2_0
        await SqliteDbBase<object>.CloseDatabaseAsync();
        var pipeline = new Pipeline();
        var packageForDownload = new Package();
        packageForDownload.name = packageName;

        if (await pipeline.DownloadPackage(packageForDownload, true, null))
        {
            await Toast.Make($"Restauración de base de datos terminada {packageName}").Show();
        }
        else
        {
            await Toast.Make("Hubo un error al descargar/descomprimir archivo.").Show();
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
        bool result = await DisplayAlertAsync("Rehacer configuración", "¿Desea continuar?", "Sí", "No");
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
        
        var selectedConnection = (DatabaseStruct) control.SelectedItem;
        selectedDatabase = selectedConnection;
        userdb = new UserAccessDb(selectedConnection.Name);
        LoadTables();
    }
}