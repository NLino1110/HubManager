using DMSA.Models.Odoo.Abstract;

namespace DMOrders.Pages.Sys;

public partial class Connections : TabbedPage
{
    

    public Connections()
	{
		InitializeComponent();
        BindingContext = new OdooConnectionsViewModel();
    }

    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
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