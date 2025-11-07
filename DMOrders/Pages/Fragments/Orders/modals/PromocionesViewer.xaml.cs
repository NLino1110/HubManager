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
    private PromotionEvalResult selectedBromotionBenefit { get; set; }
    public ObservableCollection<PromotionEvalResult> ItemsData
    {
        get => _itemsData;
        set
        {
            _itemsData = value;
            OnPropertyChanged(nameof(ItemsData));
        }
    }

    private ObservableCollection<PromotionEvalResult> _itemsData;

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
        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            selectedBromotionBenefit = (PromotionEvalResult) e.CurrentSelection[0];
            //await LoadDetailInfo(selectedBromotionBenefit);
            //promoGifts.Clear();

            
            //Debug.WriteLine(selectedBromotionBenefit.name);
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

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    public Action<List<PromotionEvalResult>> ClosePopupAction { get; set; }

    private void OnCloseButtonClicked(object sender, EventArgs e)
    {
        ClosePopupAction?.Invoke(ItemsData.ToList());
    }
   
}