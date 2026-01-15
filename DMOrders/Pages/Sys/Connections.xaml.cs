using CommunityToolkit.Maui.Alerts;
using DMSA.Models.Odoo.Abstract;
using DMSA.Sync.Core.Database.Sqlite;

namespace DMOrders.Pages.Sys;

public partial class Connections : TabbedPage
{
    public Connections()
	{
		InitializeComponent();
        BindingContext = new OdooConnectionsViewModel();

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

    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void btnSendCloud_Clicked(object sender, EventArgs e)
    {        
        
    }

    private async void btnRebuildSettings_Clicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("Rehacer configuración", "¿Desea continuar?", "Sí", "No");
        if (!result)
        {
            return;
        }

        var vm = BindingContext as OdooConnectionsViewModel;
        await vm.RemoveAll();
        await Toast.Make("Ejecución correcta...").Show();
    }

    private void ConnectionsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            var selected = e.CurrentSelection[0] as OdooConnection;
            if (selected != null)
            {
                var vm = BindingContext as OdooConnectionsViewModel;
                vm.SelectedConnection = selected;
            }
        }
    }
}