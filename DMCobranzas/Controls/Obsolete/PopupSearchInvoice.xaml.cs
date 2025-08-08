using CobranzasDMSA_Odoo;
using CobranzasDMSA_Odoo.Models;
using CobranzasDMSA_Odoo.Settings.helpers;
using CobranzasDMSA_Odoo.Settings.Sqlite;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Sample.Models;
using CommunityToolkit.Maui.Views;
using DMSA.Models.MovilCobranzas.Api;
using DMSA.Models.Odoo.Native;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

namespace CommunityToolkit.Maui.Sample;

[Obsolete]
public partial class PopupSearchInvoice : Popup
{
    res_company[] Empresas { get; set; }
    public res_company empresa { get; set; }
    public res_partner res_Partner { get; set; }
    public bool isWindows { get; set; } = false;
    public account_move[] dataItems = null;
    
    public PopupSearchInvoice(PopupSizeConstants popupSizeConstants)
	{
		InitializeComponent();

		Size = popupSizeConstants.Medium;
		ResultWhenUserTapsOutsideOfPopup = "Clic afuera";
        
        isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;

        if (App.Session.CurrentUser.empresas != null)
        {
            Empresas = App.Session.CurrentUser.empresas;
        }

        //SelectorCmp.ItemsSource = Empresas;
        //SelectorCmp.SelectedIndex = 0;
        //SelectorCmp.SelectedItem = empresa;

        CommandSelectListItem = new Command(SelectListItem);

        IDispatcherTimer timer;

        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.Tick += async (s, e) =>
        {
            //SelectorCmp.SelectedItem = empresa;

            //lblTitle.Text = empresa.name + "-" + res_Partner.name;
            lblTitle.Text = res_Partner.name;

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

    public ICommand CommandSelectListItem { get; set; }

    private async void SelectListItem(object objItem)
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

        //var database = new CobCarteraCabDb();
        //empresa = (Empresa) SelectorCmp.SelectedItem;

        //var result = await database.GetItemsAsync(empresa.empresa, txtBusqueda.Text.ToUpper());
        //dataItems = result.ToArray();
        //collectionView.ItemsSource = dataItems;
        ////dataItems[0].NOMBRECLIENTE
        //Debug.WriteLine(result.Count);

        await UITools.ShowLoading(_absoluteLayout);
        var database = new AccountMoveDb();
        //empresa = (res_company)SelectorCmp.SelectedItem;

        //var result = await database.GetItemsAsync(empresa.id, res_Partner.id, txtBusqueda.Text.ToUpper(), 25);
        //dataItems = result.ToArray();
        //collectionView.ItemsSource = dataItems;
        //dataItems[0].NOMBRECLIENTE
        var result = await database.GetItemsAsync(empresa.id, res_Partner.id, txtBusqueda.Text, 25);
        ObservableCollection<account_move> laccountmoves = new ObservableCollection<account_move>();
        laccountmoves = new ObservableCollection<account_move>(result);
        collectionView.ItemsSource = laccountmoves;

        await UITools.HideLoading(_absoluteLayout);
        //Debug.WriteLine(result.Count);
    }

    private async void btnSearchLast20_Clicked(object sender, EventArgs e)
    {
        await UITools.ShowLoading(_absoluteLayout);
        var database = new AccountMoveDb();
        //empresa = (res_company)SelectorCmp.SelectedItem;

        //var result = await database.GetItemsAsync(empresa.id, res_Partner.id, "", 20);
        //dataItems = result.ToArray();
        //collectionView.ItemsSource = dataItems;
        //dataItems[0].NOMBRECLIENTE
        var result = await database.GetItemsAsync(empresa.id, res_Partner.id, "", 25);

        ObservableCollection<account_move> laccountmoves = new ObservableCollection<account_move>();
        laccountmoves = new ObservableCollection<account_move>(result);
        collectionView.ItemsSource = laccountmoves;

        await UITools.HideLoading(_absoluteLayout);
        //Debug.WriteLine(result.Count);
    }
}
