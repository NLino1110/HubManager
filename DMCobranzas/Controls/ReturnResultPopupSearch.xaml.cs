using CobranzasDMSA_Odoo;
using CobranzasDMSA_Odoo.Models;
using CobranzasDMSA_Odoo.Settings.helpers;
using CobranzasDMSA_Odoo.Settings.Sqlite;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Sample.Models;
using CommunityToolkit.Maui.Views;
using DMSA.Models.MovilCobranzas.Api;
using DMSA.Models.Odoo.Native;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

namespace CommunityToolkit.Maui.Sample;

public partial class ReturnResultPopupSearch : Popup
{
    res_company[] Empresas { get; set; }
    public res_company empresa { get; set; }
    public bool isWindows { get; set; } = false;
    public FacNotaCreditoCab[] dataItems = null;
    
    public ReturnResultPopupSearch(PopupSizeConstants popupSizeConstants)
	{
		InitializeComponent();

		Size = popupSizeConstants.Medium;
		ResultWhenUserTapsOutsideOfPopup = "Clic afuera";
        
        isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;

        if (App.Session.CurrentUser.empresas != null)
        {
            Empresas = App.Session.CurrentUser.empresas;
        }

        SelectorCmp.ItemsSource = Empresas;
        //SelectorCmp.SelectedIndex = 0;
        //SelectorCmp.SelectedItem = empresa;
        
        CommandVerSaldos = new Command(VerSaldos);

        IDispatcherTimer timer;

        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.Tick += async (s, e) =>
        {
            SelectorCmp.SelectedItem = empresa;

            await PrepareForm();
            timer.Stop();
        };
        timer.Start();

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            txtBusqueda.Keyboard = Keyboard.Create(KeyboardFlags.CapitalizeCharacter);
        }

        BindingContext = this;
    }

    async Task PrepareForm()
    {
        
    }

    void ButtonClose_Clicked(object? sender, EventArgs e) => Close(null);

    public ICommand CommandVerSaldos { get; set; }

    private async void VerSaldos(object objItem)
    {
        Debug.WriteLine("VerSaldos");
        if (objItem != null)
        {
            Close(objItem);
        }
        else
        {
            Debug.WriteLine("Error de objeto");
        }
    }

    private async void btnSearch_Clicked(object sender, EventArgs e)
    {
        if (txtBusqueda.Text == null || txtBusqueda.Text.ToUpper().Trim().Length < 2)
        {
            await Toast.Make("Debe ingresar al menos 2 caracteres para poder realizar la búsqueda").Show();
            return;
        }

        await UITools.ShowLoading(_absoluteLayout);
        //var database = new FacNotaCreditoCabDb();
        empresa = (res_company)SelectorCmp.SelectedItem;

        //var result = await database.GetItemsGroupByCustomerAsync(empresa.id, txtBusqueda.Text.ToUpper(), 25);
        //dataItems = result.ToArray();
        //collectionView.ItemsSource = dataItems;
        //dataItems[0].NOMBRECLIENTE

        await UITools.HideLoading(_absoluteLayout);
        //Debug.WriteLine(result.Count);
    }
}

public class SubstringPortion0Converter: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string result = "";
        if (value != null)
        {
            FacNotaCreditoCab facNotaCreditoCab = (FacNotaCreditoCab) value;
            result = "??"; // facNotaCreditoCab.DATOS_CLIENTE;
            //result = facNotaCreditoCab.DATOS_CLIENTE;

            //string[] sp_DATOS_CLIENTE = result.Split("-");
            //result = sp_DATOS_CLIENTE[0];
        }
        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


public class SubstringPortion1Converter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string result = "";
        if (value != null)
        {
            FacNotaCreditoCab facNotaCreditoCab = (FacNotaCreditoCab)value;
            result = facNotaCreditoCab.VAT;
            //result = facNotaCreditoCab.DATOS_CLIENTE;

            //string[] sp_DATOS_CLIENTE = result.Split("-");
            //result = sp_DATOS_CLIENTE[1];
        }
        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SubstringPortion2Converter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string result = "";
        if (value != null)
        {
            FacNotaCreditoCab facNotaCreditoCab = (FacNotaCreditoCab)value;
            result = facNotaCreditoCab.DATOS_CLIENTE;
            //string[] sp_DATOS_CLIENTE = result.Split("-");
            //result = sp_DATOS_CLIENTE[2];
        }
        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}