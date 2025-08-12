using DMOrders.Pages.data;
using UraniumUI.ViewExtensions;

namespace DMOrders.Pages;

public partial class TabCustomers : ContentView
{
	public TabCustomers()
	{
		InitializeComponent();
        filterCustomer.OnSearchButtonClicked += OnSearchButtonClicked;

        customImageHeaderView.Title = App.Session.CurrentUser.username;
        customImageHeaderView.Subtitle = App.Session.CurrentUser.nombres;
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if(dataCustomers != null)
        {
            dataCustomers.HeightRequest = height - 100;
            //dataCustomers.WidthRequest = width;
        }        
    }

    private void OnSearchButtonClicked(object? sender, EventArgs e)
    {

        dataCustomers.LoadData(filterCustomer.getName());

        //filterCustomer.getCode();
        //filterCustomer.getId();
        //filterCustomer.getName();
        //filterCustomer.getDays();
        //filterCustomer.getStatus();

        //var label = new Label { Text = "Resultado de la búsqueda" };
        //var stackLayout = new StackLayout
        //{
        //    Children = { label }
        //};

        //Customers customers = new Customers();

        //ScrollViewContent.Content = customers;

        // Desactivar el botón btnNew
        //pagingFragment.DisableNewButton();

        // Ejecutar lógica de PagingFragment si es necesario
        //pagingFragment.ExecuteSearch();

    }
}