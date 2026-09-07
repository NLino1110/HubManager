using DMSA.Models.Odoo.Native;

namespace DMOrders.Pages.Fragments.Orders;

public partial class Container : ContentView
{
	public Container()
	{
		InitializeComponent();
        filterOrders.OnSearchButtonClicked += OnSearchButtonClicked;

        if (App.Session.CurrentUser != null)
        {
            //customImageHeaderView.Title = App.Session.CurrentUser.username;
            //customImageHeaderView.Subtitle = App.Session.CurrentUser.nombres;
        }
    }

    private void OnSearchButtonClicked(object? sender, EventArgs e)
    {
        dataOrders.LoadData(filterOrders);
    }

    public void ReloadData()
    {
        dataOrders.LoadData(filterOrders);
    }

    public void LoadInfo(res_partner _data)
    {
        //info.FillData(_data);
    }

    public void ReloadFilter()
    {
        filterOrders.LoadTopCustomersAsync();
    }
}
