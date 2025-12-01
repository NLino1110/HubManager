using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.Native;
using Spinner.MAUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Product;

public class infoDetail
{
    public int id { get; set; }
    public string Description { get; set; }
    public decimal Value { get; set; }
}

public partial class Info : ContentView
{
    public static readonly BindableProperty StockListProperty =
    BindableProperty.Create(
        nameof(StockList),
        typeof(ObservableCollection<infoDetail>),
        typeof(Info),
        default(ObservableCollection<infoDetail>));

    public ObservableCollection<infoDetail> StockList
    {
        get => (ObservableCollection<infoDetail>)GetValue(StockListProperty);
        set => SetValue(StockListProperty, value);
    }

    public static readonly BindableProperty PricesListProperty =
    BindableProperty.Create(
        nameof(PricesList),
        typeof(ObservableCollection<infoDetail>),
        typeof(Info),
        default(ObservableCollection<infoDetail>));

    public ObservableCollection<infoDetail> PricesList
    {
        get => (ObservableCollection<infoDetail>)GetValue(PricesListProperty);
        set => SetValue(PricesListProperty, value);
    }

    private product_product data;

    public static readonly BindableProperty Base64SourceProperty =
             BindableProperty.Create(
                 nameof(Base64Source),
                 typeof(string),
                 typeof(Info),
                 string.Empty,
                 propertyChanged: OnBase64SourceChanged);

    private static void OnBase64SourceChanged(BindableObject bindable, object oldValue, object newValue)
    {        
        //MemoryStream stream = new MemoryStream(Convert.FromBase64String((string)newValue));        
        //((Image)bindable).Source = ImageSource.FromStream(() => stream);        
    }

    public string Base64Source
    {
        set
        {
            SetValue(Base64SourceProperty, value);
        }
        get
        {
            return (string)GetValue(Base64SourceProperty);
        }
    }

    public Info()
	{
		InitializeComponent();
        BindingContext = this;
        StockList = new ObservableCollection<infoDetail>();
        PricesList = new ObservableCollection<infoDetail>();
    }

    public async Task FillData(product_product _data)
    {
        data = _data;
        LabelTitle.Text = data.name;
        lblCode.Text = data.code;
        lblForSale.Text = data.active ? "Activo" : "Inactivo";

        lblMarcaNombre.Text = "---";
        lblCategoriaNombre.Text = "---";
        lblIvaInc.Text = "---";
        lblIvaCalc.Text = "---";

        Base64Source = data.image_256 ?? string.Empty;

        StockList.Clear();
        StockList.Add(new infoDetail { id = 1, Description = "Disponible", Value = (decimal)data.qty_available });
        StockList.Add(new infoDetail { id = 2, Description = "Disponible Virtual", Value = (decimal)data.virtual_available });
        StockList.Add(new infoDetail { id = 3, Description = "Cantidad Libre", Value = (decimal)data.free_qty });

        PricesList.Clear();
        
        var priceListDb = new ProductPricelistDb(App.Session.odooConnection.DbNameSqlite);
        var priceLists = await priceListDb.GetItemsAsync(x=>x.active);
        var priceListDict = priceLists.ToDictionary(x => x.id, x => x.name);

        var priceListProductsDb = new ProductPricelistItemDb(App.Session.odooConnection.DbNameSqlite);
        var priceListItems = await priceListProductsDb.GetItemsAsync(x=>x._product_tmpl_id == data._product_tmpl_id);

        if (priceListItems != null && priceListItems.Count > 0)
        {
            foreach (var item in priceListItems)
            {
                string namePriceList = priceListDict.TryGetValue(item._pricelist_id, out string name) ? name : "Desconocido";
                PricesList.Add(new infoDetail { id = item.id, Description = namePriceList, Value = item.fixed_price });
            }
        }

        //MemoryStream stream = new MemoryStream(Convert.FromBase64String((string)Base64Source));
        //productImage.Source = ImageSource.FromStream(() => stream);     

        //OnPropertyChanged(nameof(Base64Source));

        //byte[] imageBytes = Convert.FromBase64String(data.image_256);

        //productImage = new Image
        //{
        //    Source = ImageSource.FromStream(() => new MemoryStream(imageBytes)),
        //    //Aspect = Aspect.AspectFill,
        //    //HeightRequest = 200,
        //    //WidthRequest = 200
        //};
    }
}