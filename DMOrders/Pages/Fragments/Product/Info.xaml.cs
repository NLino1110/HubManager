using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using Spinner.MAUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Text.RegularExpressions;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using DMSA.Sync.Core.Database.Sqlite;


namespace DMOrders.Pages.Fragments.Product;

public class infoDetail
{
    public int id { get; set; }
    public string Description { get; set; }
    public decimal Value { get; set; }
}

public partial class Info : ContentView
{
    ProductPricelistItemDb priceListProductsDb { get; set; }


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
        priceListProductsDb = new ProductPricelistItemDb(App.Session.odooConnection.DbNameSqlite);
    }

    public async Task FillData(product_product _data)
    {
        data = _data;
        LabelTitle.Text = data.name;
        lblCode.Text = data.code;
        lblBarCode.Text = !string.IsNullOrEmpty(data.barcode) && data.barcode != "false" ? data.barcode : "-";
        lblForSale.Text = data.active ? "Activo" : "Inactivo";

        lblMarcaNombre.Text = _data.marca_display;
        lblCategoriaNombre.Text = _data.categoria_display;
        lblIvaInc.Text = "---";
        lblIvaCalc.Text = "---";

        Base64Source = data.image_256 ?? string.Empty;

        StockList.Clear();
        PricesList.Clear();

        await FillInventory(data);
        await FillPrices(data);

        //if (!string.IsNullOrEmpty(data.image_256))
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
        
    public static class Cache
    {
        public static Dictionary<int, string> PriceListDict { get; set; }
        = new Dictionary<int, string>();

        public static Dictionary<int, List<product_pricelist_item>> PriceListItemsByTemplate
            = new Dictionary<int, List<product_pricelist_item>>();

        public static Dictionary<int, string> StockWarehouseListDict 
            = new Dictionary<int, string>();

        public static Dictionary<int, List<stock_quant>> StockQuantListDict
            = new Dictionary<int, List<stock_quant>>();
    }

    public string ObtenerCodigo(string linea)
    {
        var match = Regex.Match(linea, @"\((.*?)\)");
        return match.Success ? match.Groups[1].Value : null;
    }

    private async Task<List<infoDetail>> FillInventory(product_product _data)
    {
        if (Cache.StockWarehouseListDict.Count == 0)
        {
            var whListDb = new StockWareHouseDb(App.Session.odooConnection.DbNameSqlite);
            var whLists = await whListDb.GetItemsAsync(x => x.active);
            Cache.StockWarehouseListDict = whLists.ToDictionary(x => x.id, x => x.name);
        }

        List<stock_quant> stockQuantItems;

        var stopwatch = Stopwatch.StartNew();
        if (!Cache.StockQuantListDict.TryGetValue(data.id, out stockQuantItems))
        {
            var stockQuantDb = new StockQuantDb(App.Session.odooConnection.DbNameSqlite);

            stockQuantItems = await stockQuantDb.GetItemsAsync(
                x => x._product_id == data.id
            );

            Cache.StockQuantListDict[data._product_tmpl_id] = stockQuantItems;
        }

        stopwatch.Stop();

        Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
            stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

        stopwatch = Stopwatch.StartNew();
        foreach (var item in stockQuantItems)
        {
            string nameWarehouse = Cache.StockWarehouseListDict.TryGetValue(item._warehouse_id, out string name)
                ? name
                : "Desconocido";

            if(nameWarehouse.Equals("Desconocido"))
            {
                nameWarehouse = ObtenerCodigo(item.display_name) ?? nameWarehouse;
            }

            StockList.Add(new infoDetail
            {
                id = item.id,
                Description = nameWarehouse,
                Value = (decimal) item.quantity
            });
        }

        stopwatch.Stop();

        Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
            stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

        return null;
    }

    private async Task<List<product_pricelist_item>> FillPrices(product_product _data)
    {
        if (Cache.PriceListDict.Count == 0)
        {
            var priceListDb = new ProductPricelistDb(App.Session.odooConnection.DbNameSqlite);
            var priceLists = await priceListDb.GetItemsAsync(x => x.active);
            Cache.PriceListDict = priceLists.ToDictionary(x => x.id, x => x.name);
        }        

        List<product_pricelist_item> priceListItems;
                
        if (!Cache.PriceListItemsByTemplate.TryGetValue(data._product_tmpl_id, out priceListItems))
        {            
            

            priceListItems = await priceListProductsDb.GetItemsAsync(
                x => x._product_tmpl_id == data._product_tmpl_id
            );
                        
            Cache.PriceListItemsByTemplate[data._product_tmpl_id] = priceListItems;
        }

        priceListItems = Cache.PriceListItemsByTemplate[data._product_tmpl_id];

        // 5. Agregar al observable list
        foreach (var item in priceListItems)
        {
            string namePriceList = Cache.PriceListDict.TryGetValue(item._pricelist_id, out string name)
                ? name
                : "Desconocido";

            PricesList.Add(new infoDetail
            {
                id = item.id,
                Description = namePriceList,
                Value = item.fixed_price
            });
        }

        return null;
    }
}