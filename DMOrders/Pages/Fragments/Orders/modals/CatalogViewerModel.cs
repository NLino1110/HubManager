using CommunityToolkit.Mvvm.Input;
using DMOrders.Models.Filters;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.Native;
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

        private readonly string FilterCode;
        private readonly string FilterName;
        private readonly int FilterBrand;
        private readonly int FilterCategory;
        private readonly FStatus FilterStatus;

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

        product_brand selected_brand { get; set; }
        public ObservableCollection<product_brand> Brands { get; set; } = new();

        //public CatalogViewerModel(string filterCode, string filterName, int filterBrand, int filterCategory, FStatus filterStatus)
        //{
        //    FilterCode = filterCode;
        //    FilterName = filterName;
        //    FilterBrand = filterBrand;
        //    FilterCategory = filterCategory;
        //    FilterStatus = filterStatus;

        //    InitViewModes();
        //    RefreshCommand = new Command(async () => await CmdRefresh());
        //    _ = LoadData();
        //}

        public CatalogViewerModel()
        {
            InitViewModes();
            RefreshCommand = new Command(async () => await CmdRefresh());
            _ = LoadData();
        }

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

        public async Task LoadData()
        {
            Debug.WriteLine($"[CatalogViewerModel] LoadData Initialized");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                
                IsLoading = true;
                var database = new ProductProductDb();
                Debug.WriteLine(FilterCode);
                var allItems = await database.GetItemsAsync();
                IEnumerable<product_product> filtered = allItems;

                if (!string.IsNullOrWhiteSpace(FilterCode) && int.TryParse(FilterCode, out var intFilterCode))
                {
                    filtered = filtered.Where(x => x.id == intFilterCode);
                }
                else if (FilterCategory > 0)
                {
                    filtered = filtered.Where(x => x._categ_id == FilterCategory);
                }
                else if (!string.IsNullOrWhiteSpace(FilterName))
                {
                    filtered = filtered.Where(x => x.name.Contains(FilterName, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    //filtered = filtered.Where(x => !x.image_256.Contains("false"));
                }

                TotalItems = filtered.Count();

                ItemsData = new ObservableCollection<product_product>(
                    filtered.Skip((Page - 1) * PageSize).Take(PageSize));

                Debug.WriteLine(ViewModesListSelectedIndex); 

                if(ViewModesListSelectedIndex == 2)
                {
                    _selectedItem = ItemsData[0];
                    OnPropertyChanged();
                }
            }
            catch (Exception ex)
            {
                ItemsData = new ObservableCollection<product_product>();
                Debug.WriteLine(ex);
            }
            finally
            {
                IsLoading = false;
                stopwatch.Stop();
                Debug.WriteLine($"[CatalogViewerModel] Carga completada en {stopwatch.ElapsedMilliseconds} ms | TotalItems: {TotalItems}, Page: {Page}, PageSize: {PageSize}, Filtro: {FilterCode ?? FilterName ?? "sin filtro"}");
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
