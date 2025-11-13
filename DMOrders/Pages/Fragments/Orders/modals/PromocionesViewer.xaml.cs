using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.@abstract;
using DMSA.Models.Odoo.Native;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace DMOrders.Pages.Fragments.Orders.modals;

public partial class PromocionesViewer : ContentView, INotifyPropertyChanged
{
    private sale_order SaleOrder { get; set; }
    private PromotionBenefit selectedBromotionBenefit { get; set; }
    public ObservableCollection<PromotionEvalResult> ItemsData
    {
        get => _itemsData;
        set
        {
            _itemsData = value;

            if(_ItemsDataBenefits != null)
                _ItemsDataBenefits.Clear();
            else
                _ItemsDataBenefits = new ObservableCollection<PromotionBenefit>();

            foreach (var promo in _itemsData)
            {
                if (promo.Items != null)
                {
                    foreach (var benefit in promo.Items)
                    {
                        _ItemsDataBenefits.Add(benefit.Promotion);
                    }
                }
            }

            OnPropertyChanged(nameof(ItemsData));
            OnPropertyChanged(nameof(ItemsDataBenefits));
        }
    }

    private ObservableCollection<PromotionEvalResult> _itemsData;

    public ObservableCollection<PromotionBenefit> ItemsDataBenefits
    {
        get => _ItemsDataBenefits;
        set
        {
            _ItemsDataBenefits = value;
            OnPropertyChanged(nameof(ItemsDataBenefits));
        }
    }

    private ObservableCollection<PromotionBenefit> _ItemsDataBenefits;

    public ObservableCollection<product_product> promoGifts
    {
        get => _promoGifts;
        set
        {
            _promoGifts = value;
            OnPropertyChanged(nameof(promoGifts));
        }
    }

    private ObservableCollection<product_product> _promoGifts;

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged(nameof(IsLoading));
            Debug.WriteLine("_isLoading");
            Debug.WriteLine(_isLoading);
        }
    }

    public int PromosCount
    {
        get => ItemsData?.Count ?? 0;
    }

    public PromocionesViewer(sale_order SaleOrderParam)
	{
		InitializeComponent();
        BindingContext = this;        
        SaleOrder = SaleOrderParam;
        //LoadDataByTimer();
    }

    private async void detail_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (promoGifts != null)
            promoGifts.Clear();
        else
            promoGifts = new ObservableCollection<product_product>();

        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            selectedBromotionBenefit = (PromotionBenefit) e.CurrentSelection[0];
            //await LoadDetailInfo(selectedBromotionBenefit);
            //promoGifts.Clear();            
            //Debug.WriteLine(selectedBromotionBenefit.name);

            var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);

            if (selectedBromotionBenefit._promotion_type_id == 2) // es regalo
            {
                foreach(var itemResult in _itemsData)
                {
                    foreach(var itemEval in itemResult.Items)
                    {
                        if(itemEval.Promotion.id == selectedBromotionBenefit.id)
                        {
                            if(itemEval.RuleSet == null || itemEval.RuleSet._product_id <= 0)
                                continue;

                            var productGift = await productDb.GetByProductTemplate(itemEval.RuleSet._product_id);
                            promoGifts.Add(productGift);                            
                            break;
                        }
                    }
                }

                OnPropertyChanged(nameof(promoGifts));
            }

            if (selectedBromotionBenefit._promotion_type_id == 4) // es NXN
            {
                
            }

            if (selectedBromotionBenefit._promotion_type_id == 6) // es descuento
            {
                
            }

        }
    }

    private async Task LoadDetailInfo(PromotionBenefit promotionBenefit)
    {        
        
    }

    //public void LoadDataByTimer()
    //{
    //    var timer = Dispatcher.CreateTimer();
    //    timer.Interval = TimeSpan.FromMilliseconds(300);
    //    timer.IsRepeating = false;

    //    timer.Tick += async (s, e) =>
    //    {
    //        try
    //        {
    //            await EvalPromotions();
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.WriteLine($"Error en LoadData: {ex}");
    //        }
    //        finally
    //        {
    //            timer.Stop();
    //        }
    //    };

    //    timer.Start();
    //}

    //public async Task EvalPromotions()
    //{
    //    try
    //    {
    //        IsLoading = true;

    //        ItemsData ??= new ObservableCollection<PromotionBenefit>();
    //        ItemsData.Clear();

    //        string DbNameSqlite = App.Session.odooConnection.DbNameSqlite;

    //        PromotionBenefitDb dataDb = new PromotionBenefitDb(DbNameSqlite);
    //        var items = await dataDb.GetItemsAsync("authorized");

    //        //foreach (var it in items)
    //        //    ItemsData.Add(it);

    //        Debug.WriteLine($"Promociones cargadas: {ItemsData.Count}");
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine($"Error cargando promociones: {ex}");
    //    }
    //    finally
    //    {
    //        IsLoading = false;

    //        //IsFinishedAnalysis = true;
    //    }
    //}

    private async void AddGift(object sender, EventArgs e)
    {        
        //Debug.WriteLine(sender);
        Button button = (Button) sender;
        product_product product = (product_product)button.BindingContext;
        Debug.WriteLine(product.name);

        foreach (var itemLine in SaleOrder.order_line)
        {
            if (itemLine[2] != null)
            {
                var lineObject = (sale_order_line)itemLine[2];
                if( lineObject.product_id == product.id )
                {
                    // ya existe la linea
                    await Application.Current.Windows[0].Page.DisplayAlert("Información", "El producto seleccionado ya se encuentra en el pedido.", "OK");
                    return;
                }
            }
        }

        var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);
        
        int ordinal = 0;

        var line = new sale_order_line
        {
            _order_id = SaleOrder.id,
            ordinal = ordinal,
            product_id = product.id,
            product_display = product.name,
            product_code = product.code,
            qty_to_deliver = 1,
            product_uom_qty_real = 1,
            product_uom_qty = 1,
            uom_category_display = "UND",
            price_subtotal = 0,
            discount = 100,
            price_tax = 0,
            price_total = 0,
            is_gift = true,
            promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(selectedBromotionBenefit)
        };

        await saleOrderLineDb.InsertAsyncAutoOrdinal(line);        
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    public Action<List<PromotionEvalResult>> ClosePopupAction { get; set; }

    private void OnCloseButtonClicked(object sender, EventArgs e)
    {
        ClosePopupAction?.Invoke(ItemsData.ToList());
    }   
}