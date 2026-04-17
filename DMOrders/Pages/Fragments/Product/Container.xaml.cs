using DMSA.Models.Odoo.Native;

namespace DMOrders.Pages.Fragments.Product;

public partial class Container : ContentView
{
	public Container()
	{
		InitializeComponent();
        filterProducts.OnSearchButtonClicked += OnSearchButtonClicked;

        if (App.Session.CurrentUser != null)
        {
            //customImageHeaderView.Title = App.Session.CurrentUser.username;
            //customImageHeaderView.Subtitle = App.Session.CurrentUser.nombres;
        }
    }

    //protected override void OnSizeAllocated(double width, double height)
    //{
    //    base.OnSizeAllocated(width, height);

    //    //if(dataCustomers != null)
    //    //{
    //    //    dataCustomers.HeightRequest = height - 100;        
    //    //}        
    //}

    private void OnSearchButtonClicked(object? sender, EventArgs e)
    {
        dataProducts.LoadData(filterProducts);
    }

    public void ReloadData()
    {
        dataProducts.LoadData(filterProducts);
    }

    public async Task LoadInfo(product_product _data)
    {
        await info.FillData(_data);
    }

    public void ReloadFilter()
    {
        filterProducts.LoadTopMarcasAsync();
        filterProducts.LoadTopCategoriesAsync();
    }
}