using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using DMOrders.Models.Filters;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using DMSA.Sync.Core.Database.Sqlite;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Orders.modals
{
    public partial class CatalogViewerModel : INotifyPropertyChanged
    {
        public ICommand ItemTappedCommand { get; }

        public product_pricelist CurrentPriceList { get; set; }
        private ProductProductDb _db { get; set; }
        public ICommand CommandSelectListItem { get; set; }
        public class ViewModesList
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public ObservableCollection<ViewModesList> viewModesList { get; private set; } = new();

        private int _viewModesListSelectedIndex;
        public int ViewModesListSelectedIndex
        {
            get => _viewModesListSelectedIndex;
            set
            {
                if (_viewModesListSelectedIndex != value)
                {
                    _viewModesListSelectedIndex = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentViewMode));
                }
            }
        }

        public int CurrentViewMode => viewModesList.ElementAtOrDefault(ViewModesListSelectedIndex)?.id ?? 1;

        private ObservableCollection<product_product> _itemsData;
        private product_product _selectedItem;
        private bool _isRefreshing;
        private bool _headerBordersVisible = true;
        private bool _paginationEnabled = false;

        private int _totalItems;
        private int _pageSize = 1;
        private int _page = 1;

        private string filter_code;
        private string filter_name;
        private int filter_brand;        
        private int filter_new;
        private int filter_stock;
        private int filter_sort;        

        public bool CanGoNext => (_page * PageSize) < TotalItems;
        public bool CanGoPrevious => _page > 1;

        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public int TotalItems
        {
            get => _totalItems;
            set
            {
                _totalItems = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));
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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<product_brand> Brands { get; set; } = new();

        private string _lastFilterSignature;

        private string BuildFilterSignature() =>
            $"{filter_code}|{filter_name}|{filter_brand}|{filter_new}|{filter_stock}|{filter_sort}";


        public CatalogViewerModel()
        {
            _db = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
            InitViewModes();
            RefreshCommand = new Command(async () => await CmdRefresh());
            ItemTappedCommand = new Command<product_product>(OnItemTapped);
        }

        //public CatalogViewerModel(product_pricelist product_Pricelist)
        //{
        //    _db = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
        //    CurrentPriceList = product_Pricelist;
        //    InitViewModes();
        //    RefreshCommand = new Command(async () => await CmdRefresh());
        //}

        private void InitViewModes()
        {
            viewModesList.Add(new ViewModesList { id = 1, name = "▤" });
            viewModesList.Add(new ViewModesList { id = 2, name = "▦" });
            viewModesList.Add(new ViewModesList { id = 3, name = "⧉" });

            CommandSelectListItem = new Command(AddSelectedItem);
        }

        public ObservableCollection<product_product> ItemsData
        {
            get => _itemsData;
            set
            {
                _itemsData = value;
                OnPropertyChanged();
            }
        }

        public bool HeaderBordersVisible
        {
            get => _headerBordersVisible;
            set
            {
                _headerBordersVisible = value;
                OnPropertyChanged();
            }
        }

        public bool PaginationEnabled
        {
            get => _paginationEnabled;
            set
            {
                _paginationEnabled = value;
                OnPropertyChanged();
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        private async void AddSelectedItem(object objItem)
        {
            if (objItem != null)
            {
                //((CatalogViewerModel)this.BindingContext).SelectedItem = (product_product)objItem;
                //_parentPopup.Close(objItem);

                //ItemsData.Add((product_product) objItem);
                //Debug.WriteLine(objItem);
            }
            else
            {
                Debug.WriteLine("Error de objeto");
            }
        }

        [RelayCommand]
        private void RelayRowTapped()
        {
            Debug.WriteLine("RelayRowTapped called");
        }

        public ICommand RefreshCommand { get; }

        private async Task CmdRefresh()
        {
            IsRefreshing = true;
            await LoadData();
            IsRefreshing = false;
        }

        public async Task PublicRefresh()
        {
            IsRefreshing = true;
            await LoadData();
            IsRefreshing = false;
        }


        private readonly SemaphoreSlim _loadLock = new(1, 1); // evita cargas simultáneas
        private CancellationTokenSource _cts;

        public async Task LoadData()
        {
            var signature = BuildFilterSignature();
            var filtersChanged = signature != _lastFilterSignature;

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
                int pricelist_id = -1;
                if(CurrentPriceList != null)
                    pricelist_id = CurrentPriceList.id;

                // Llama paginado (NO vuelvas a traer todo)
                var (items, total) = await _db.GetPagedAsync(
                    filter_code, filter_name, filter_brand, filter_new, filter_stock, filter_sort,
                    0,0, pricelist_id,
                    Page, PageSize, ct);

                TotalItems = total;

                // Evita recrear la OC (menos churn de UI)
                if (ItemsData == null)
                    ItemsData = new ObservableCollection<product_product>();
                else
                    ItemsData.Clear();

                foreach (var it in items)
                    ItemsData.Add(it);

                // No asumas ItemsData[0]
                if (ViewModesListSelectedIndex == 2 && ItemsData.Count > 0)
                {
                    SelectedItem = ItemsData[0];
                    // Asegúrate que notifique SelectedItem:
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
            catch (OperationCanceledException)
            {
                // ignorar: una nueva carga comenzó
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
                Debug.WriteLine($"[CatalogViewerModel] Carga en {stopwatch.ElapsedMilliseconds} ms | TotalItems: {TotalItems}, Page: {Page}, PageSize: {PageSize}, Filtro: {filter_code ?? filter_name ?? "sin filtro"}");
            }
        }

        public ICommand NextPageCommand => new Command(async () =>
        {
            if (CanGoNext)
            {
                Page++;
                await LoadData();
            }
        });

        public ICommand PreviousPageCommand => new Command(async () =>
        {
            if (CanGoPrevious)
            {
                Page--;
                await LoadData();
            }
        });

        public void SetFilterCode(string _filter_code)
        {
            filter_code = _filter_code;
        }

        public void SetFilterName(string _filter_name)
        {
            filter_name = _filter_name;
        }

        public void SetFilterBrand(int _filter_brand)
        {
            filter_brand = _filter_brand;
        }
        public void SetFilterNew(int _filter_new)
        {
            filter_new = _filter_new;
        }

        public void SetFilterStock(int _filter_stock)
        {
            filter_stock = _filter_stock;
        }

        public void SetFilterSort(int _filter_sort)
        {
            filter_sort = _filter_sort;
        }

        public void OnItemTapped(product_product tappedItem)
        {
            foreach (var res_partner_item in _itemsData)
                res_partner_item.IsSelected = false;

            tappedItem.IsSelected = true;
            SelectedItem = tappedItem;
            WeakReferenceMessenger.Default.Send(new ItemSelectedMessage(tappedItem));
        }

        public void ClearSelection(product_product tappedItem)
        {
            foreach (var res_partner_item in _itemsData)
                res_partner_item.IsSelected = false;

            //tappedItem.IsSelected = true;
            //SelectedItem = tappedItem;
            //WeakReferenceMessenger.Default.Send(new ItemSelectedMessage(tappedItem));
        }

        public class ItemSelectedMessage : ValueChangedMessage<product_product>
        {
            public ItemSelectedMessage(product_product value) : base(value) { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
