using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using DMOrders.Controls.CustomRows;
using DMOrders.Models.Filters;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.Native;
using Microsoft.Maui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Product
{
    public partial class ProductListViewModel : INotifyPropertyChanged
    {
        private ProductProductDb _db { get; set; }
        private ObservableCollection<product_product> _itemsData;

        private readonly SemaphoreSlim _loadLock = new(1, 1); // evita cargas simultáneas
        private CancellationTokenSource _cts;
        private string BuildFilterSignature() =>
            $"{filters.getCode()}|{filters.getName()}|{filters.getBrand()}|{filters.getCategory()}|{filters.getStatus()}";

        private string _lastFilterSignature;
        public Filters filters { get; set; }

        private product_product _selectedItem;
        private bool _isRefreshing;        
        private bool _headerBordersVisible = true;
        private bool _paginationEnabled = false;        

        private int _totalItems = 0;
        private int _pageSize = 24;
        private int _page = 1;

        public bool CanGoNext => (_page * PageSize) < TotalItems;
        public bool CanGoPrevious => _page > 1;

        public int TotalPages => (int) Math.Ceiling((double)TotalItems / PageSize);

        public int TotalItems
        {
            get => _totalItems;
            set
            {
                _totalItems = value;
                OnPropertyChanged(nameof(TotalItems));
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));
            }
        }

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

        public product_product SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public ProductListViewModel(Filters _filters)
        {
            _db = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);

            filters = _filters;
            _itemsData = new ObservableCollection<product_product>();
            ItemTappedCommand = new Command<product_product>(OnItemTapped);
        }

        public ICommand ItemTappedCommand { get; }
        public void OnItemTapped(product_product tappedItem)
        {
            foreach (var res_partner_item in _itemsData)
                res_partner_item.IsSelected = false;

            tappedItem.IsSelected = true;
            WeakReferenceMessenger.Default.Send(new ItemSelectedMessage(tappedItem));
        }

        public class ItemSelectedMessage : ValueChangedMessage<product_product>
        {
            public ItemSelectedMessage(product_product value) : base(value) { }
        }

        public ObservableCollection<product_product> ItemsData
        {
            get => _itemsData;
            set
            {
                _itemsData = value;
                OnPropertyChanged(nameof(ItemsData));
            }
        }

        public bool HeaderBordersVisible
        {
            get => _headerBordersVisible;
            set
            {
                _headerBordersVisible = value;
                OnPropertyChanged(nameof(HeaderBordersVisible));
            }
        }

        public bool PaginationEnabled
        {
            get => _paginationEnabled;
            set
            {
                _paginationEnabled = value;
                OnPropertyChanged(nameof(PaginationEnabled));
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                OnPropertyChanged(nameof(PageSize));

                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));
            }
        }

        public int Page
        {
            get => _page;
            set
            {
                _page = value;
                OnPropertyChanged(nameof(Page));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        [RelayCommand]
        void RelayRowTapped()
        {
            Debug.WriteLine("RelayRowTapped called");
        }

        public void LoadDataByTimer()
        {
            // Usamos el dispatcher global de la app para garantizar ejecución en UI
            //var dispatcher = Application.Current.Dispatcher;
            var dispatcher = Dispatcher.GetForCurrentThread();
            var timer = dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(300); // delay corto para dejar respirar la UI
            timer.IsRepeating = false;

            timer.Tick += async (s, e) =>
            {
                try
                {
                    if (IsBusy) return; // Previene cargas simultáneas
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
            var signature = BuildFilterSignature();
            var filtersChanged = signature != _lastFilterSignature;

            Debug.WriteLine(signature);
            Debug.WriteLine(_lastFilterSignature);

            if (filtersChanged)
            {
                Page = 1;
                _lastFilterSignature = signature;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            await _loadLock.WaitAsync(ct);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                IsLoading = true;

                // Llama paginado (NO vuelvas a traer todo)
                //filter_code, filter_name, filter_brand, filter_new, filter_stock, filter_sort,
                var (items, total) = await _db.GetPagedAsync(
                    filters.getCode(),
                    filters.getName(),
                    filters.getBrand(),
                    0,
                    0,
                    filters.getCategory(),
                    filters.getStatus(),
                    0,
                    0,
                    Page,
                    PageSize,
                    ct);

                TotalItems = total;

                // Evita recrear la OC (menos churn de UI)
                if (ItemsData == null)
                    ItemsData = new ObservableCollection<product_product>();
                else
                    ItemsData.Clear();

                foreach (var it in items)
                {
                    ItemsData.Add(it);
                }
            }
            catch (OperationCanceledException ecx)
            {
                // ignorar: una nueva carga comenzó
                Debug.WriteLine(ecx);
            }
            catch (Exception ex)
            {
                ItemsData = new ObservableCollection<product_product>();
                Debug.WriteLine(ex);
            }
            finally
            {
                IsLoading = false;
                _loadLock.Release();
                stopwatch.Stop();
                //Debug.WriteLine($"[CatalogViewerModel] Carga en {stopwatch.ElapsedMilliseconds} ms | TotalItems: {TotalItems}, Page: {Page}, PageSize: {PageSize}, Filtro: {filters.getCode() ?? filters.getName() ?? "sin filtro"}");
            }
        }

        //private async Task LoadData()
        //{
        //    try
        //    {
        //        var database = new ProductProductDb();
        //        var allItems = await database.GetItemsAsync();
        //        IEnumerable<product_product> filtered = null;

        //        //Filtro por ID/Code
        //        if (!string.IsNullOrWhiteSpace(FilterCode))
        //        {
        //            int int_filterCode;

        //            if (int.TryParse(FilterCode, out int_filterCode))
        //            {
        //                filtered = allItems.Where(x => x.id == int_filterCode);
        //            }
        //        }
        //        else
        //        {
        //            //Filtro por VAT
        //            if (FilterCategory > 0)
        //            {
        //                filtered = allItems.Where(x => x._categ_id == FilterCategory);
        //            }
        //            else
        //            {
        //                //Filtro por nombre
        //                if (string.IsNullOrWhiteSpace(FilterName))
        //                {
        //                    FilterName = "";
        //                }

        //                filtered = string.IsNullOrWhiteSpace(FilterName)
        //                    ? allItems.Where(x=> !x.image_256.Contains("false"))
        //                    : allItems.Where(x => x.name.Contains(FilterName, StringComparison.OrdinalIgnoreCase));

        //                filtered = allItems;
        //            }
        //        }

        //        Debug.WriteLine("Filtro actual:" + FilterName);

        //        string userSearch = "";

        //        //if (int.TryParse(str_codagencia, out company_id)) { }

        //        TotalItems = filtered.Count();

        //        var paginated = filtered
        //            .Skip((_page - 1) * _pageSize)
        //            .Take(_pageSize);

        //        _itemsData = [.. paginated];
        //        OnPropertyChanged(nameof(ItemsData));
        //        OnPropertyChanged(nameof(CanGoNext));
        //        OnPropertyChanged(nameof(CanGoPrevious));

        //    }
        //    catch (Exception ex)
        //    {
        //        _itemsData = new ObservableCollection<product_product>();
        //        Debug.WriteLine(ex.ToString());
        //    }
        //}

        public ICommand NextPageCommand => new Command(async () =>
        {
            if (CanGoNext)
            {
                Page++;
                LoadDataByTimer();
            }
        });

        public ICommand PreviousPageCommand => new Command(async () =>
        {
            if (CanGoPrevious)
            {
                Page--;
                LoadDataByTimer();
            }
        });

        public event PropertyChangedEventHandler PropertyChanged;
        //private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
