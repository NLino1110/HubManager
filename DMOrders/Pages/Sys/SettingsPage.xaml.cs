
using DMOrders.AppPages.Sys;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Services;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo;
using DMSA.Models.Odoo.Tools;
using System.Diagnostics;
using System.Security.Cryptography;

namespace DMOrders.Pages.Sys;

public partial class SettingsPage : ContentPage //, IDisposable //, INotifyPropertyChanged
{
    //BluetoothPrinterManager btPrinterManager { get; set; }

    //private ObservableCollection<DeviceLocal> m_deviceList { get; set; }
    //public List<DeviceLocal> deviceList
    //{
    //    get { return m_deviceList; }
    //    set
    //    {
    //        m_deviceList = value;
    //        OnPropertyChanged(nameof(deviceList));
    //    }
    //}

    //public event PropertyChangedEventHandler PropertyChanged;

    //protected override void OnPropertyChanged(string propertyName)
    //{
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //}
    
    public SettingsPage()
	{        
        //Scan(null, null);
        InitializeComponent();

        IDispatcherTimer timer;

        timer = Dispatcher.CreateTimer();
        timer.IsRepeating = false;
        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.Tick += async (s, e) =>
        {
            AppSettingsDb appSettingsDb = new AppSettingsDb();
            await appSettingsDb.InitDefault();
            var appSettingItems = await appSettingsDb.GetItemsAsync();

            collectionView.ItemsSource = appSettingItems;

            lblDbPath.Text = appSettingsDb.GetDbPath();
            timer.Stop();
        };
        timer.Start();        

        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        if (Navigation.ModalStack.Count() == 0)
        {
            btnClose.IsVisible = false;
        }

        base.OnAppearing();
    }

    private async void btnSave_Clicked(object sender, EventArgs e)
    {  
        var returnResultPopup = new PasswordPromptPage();
        returnResultPopup.TitleBox = "Ingrese el pin correcto para aplicar cambios.";
                
        var result = await PopupExtensions.ShowPopupAsync<string>(this, returnResultPopup);
                
        if (result.Result != null && result.Result.ToString() == "1381")
        {
            AppSettingsDb appSettingsDb = new AppSettingsDb();

            foreach (AppSettings itemSetting in collectionView.ItemsSource)
            {
                itemSetting.write_date = DateTime.Now;
                
                if (itemSetting.name == "back_user_password")
                {
                    try
                    {
                        string decrypt = CryptoHelper.Decrypt(itemSetting.value);
                        Debug.WriteLine(decrypt);
                    }
                    catch(FormatException fe)
                    {
                        Console.WriteLine("Error:" + fe.Message);
                        //Aquí se detecta que no es un texto encriptado, es decir que el valor fue modificado
                        // y se requiere volver a encryptar
                        // ya que la idea es que no se muestre desencriptado en el mantenimiento
                        // solo será desencriptado cuando se use para la conexión
                        itemSetting.value = CryptoHelper.Encrypt(itemSetting.value);
                    }
                    catch (CryptographicException ex)
                    {
                        Console.WriteLine("Error al descifrar: " + ex.Message);
                        itemSetting.value = CryptoHelper.Encrypt(itemSetting.value);
                    }
                }
                await appSettingsDb.InsertAsync(itemSetting);
            }

            await Toast.Make("Cambios de configuración aplicados").Show();
            if (Navigation.ModalStack.Count() > 0)
            {
                await Navigation.PopModalAsync();
            }
        }
        else
        {
            //await DisplayAlert("Alerta", "PIN incorrecto, no se aplicarán los cambios", "OK");

            var returnAlertTest = new CustomAlertDialog();
            returnAlertTest.TitleBox = "ALERTA!";
            returnAlertTest.SubTitleBox = "PIN incorrecto, no se aplicarán los cambios.";
            var resultAlert = await PopupExtensions.ShowPopupAsync(this, returnAlertTest);
        }        
    }

    public void Dispose()
    {
        //btPrinterManager.Dispose();
        //throw new NotImplementedException();
    }

    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void DropData(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Eliminar", "Está seguro que desea eliminar los datos ingresados?", "Continuar", "Cancelar");
        //Debug.WriteLine("Answer: " + answer);
        if (!answer)
        {
            return;
        }

        //Eliminar datos de los archivos de actualizacion
        string DeviceStorage = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tmp");

        if (Directory.Exists(DeviceStorage))
        {
            Directory.Delete(DeviceStorage, true);
        }

        AppSettingsDb appSettingsDb = new AppSettingsDb();
        await appSettingsDb.TruncateAsync();

        //ParametrosDb database = new ParametrosDb();
        ////Se eliminan todos los datos de la tabla antes de volver a insertar
        //await database.TruncateAsync();

        //AccountPaymentHeaderDb database_r = new AccountPaymentHeaderDb();
        //await database_r.Drop();
        //await database_r.TruncateAsync();

        //CobReciboDetDb database_rd = new CobReciboDetDb();
        //await database_rd.Drop();
        //await database_rd.TruncateAsync();

        //CobCierreDb database_cobCierreDb = new CobCierreDb();
        //await database_cobCierreDb.TruncateAsync();

        //SolicitudesNCDb database_solicitudesNC = new SolicitudesNCDb();
        //await database_solicitudesNC.Drop();
        //await database_solicitudesNC.TruncateAsync();

        UserAccessDb database_CobUsuarios = new UserAccessDb();
        await database_CobUsuarios.Drop();

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        string text = "Datos eliminados!";
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        var toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
    }
}