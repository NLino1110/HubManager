using DMSA.Models.Odoo.Native;

namespace DMOrders.Pages.Fragments.Activities;

public partial class Container : ContentView
{
	public Container()
	{
		InitializeComponent();
        filterActivities.OnSearchButtonClicked += OnSearchButtonClicked;

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
        dataActivities.LoadData(filterActivities.getStatus() ,
            filterActivities.getDateEnd(),
            filterActivities.getDateEnd());

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

    public void LoadInfo(res_partner _data)
    {
        //info.FillData(_data);
    }
}