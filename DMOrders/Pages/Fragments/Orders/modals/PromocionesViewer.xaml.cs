using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.Native;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace DMOrders.Pages.Fragments.Orders.modals;

public partial class PromocionesViewer : ContentView, INotifyPropertyChanged
{
    public ObservableCollection<PromotionBenefit> ItemsData
    {
        get => _itemsData;
        set
        {
            _itemsData = value;
            OnPropertyChanged(nameof(ItemsData));
        }
    }

    private ObservableCollection<PromotionBenefit> _itemsData;

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

    //public PromocionesViewer()
    //{
    //    InitializeComponent();
    //    BindingContext = this;
    //    LoadDataByTimer();
    //}

    public PromocionesViewer(sale_order SaleOrder)
	{
		InitializeComponent();
        BindingContext = this;
        LoadDataByTimer();
    }

    public void LoadDataByTimer()
    {        
        var timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(300);
        timer.IsRepeating = false;

        timer.Tick += async (s, e) =>
        {
            try
            {
                await LoadData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en LoadData: {ex}");
            }
            finally
            {
                timer.Stop();
            }
        };

        timer.Start();
    }

    public async Task LoadData()
    {
        try
        {
            IsLoading = true;

            ItemsData ??= new ObservableCollection<PromotionBenefit>();
            ItemsData.Clear();

            string DbNameSqlite = App.Session.odooConnection.DbNameSqlite;

            PromotionBenefitDb dataDb = new PromotionBenefitDb(DbNameSqlite);
            var items = await dataDb.GetItemsAsync();

            foreach (var it in items)
                ItemsData.Add(it);

            Debug.WriteLine($"Promociones cargadas: {ItemsData.Count}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cargando promociones: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    public Action<List<PromotionBenefit>> ClosePopupAction { get; set; }

    private void OnCloseButtonClicked(object sender, EventArgs e)
    {
        ClosePopupAction?.Invoke(ItemsData.ToList());
    }
   
}