using CommunityToolkit.Mvvm.Input;
using DMOrdersUI.Controls.CustomRows;
using DMOrdersUI.Models.Filters;
using DMOrdersUI.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.Native;
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

namespace DMOrdersUI.Pages.Fragments.Product
{
    public partial class ProductListViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<product_product> _itemsData;

        private product_product _selectedItem;
        private bool _isRefreshing;        
        private bool _headerBordersVisible = true;
        private bool _paginationEnabled = false;        

        private int _totalItems = 0;
        private int _pageSize = 14;
        private int _page = 1;

        int company_id = 0;

        private string FilterCode { get; set; }
        private string FilterId { get; set; }
        private string FilterName { get; set; }
        private int FilterBrand { get; set; }
        private int FilterCategory { get; set; }
        private FStatus FilterStatus { get; set; }

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

        public ProductListViewModel(string _FilterCode, string _FilterName, int _FilterBrand, int _FilterCategory, FStatus _FilterStatus)
        {
            FilterCode = _FilterCode;            
            FilterName = _FilterName;
            FilterBrand = _FilterBrand;
            FilterCategory = _FilterCategory;
            FilterStatus = _FilterStatus;

            var task = Task.Run(async () =>
            {
                await LoadData();
            });
            Task.WaitAll(task);
            RefreshCommand = new Command(CmdRefresh);
        }

        public ProductListViewModel()
        {
            var task = Task.Run(async () =>
            {
                await LoadData();
            });

            Task.WaitAll(task);

            RefreshCommand = new Command(CmdRefresh);
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

        public ICommand RefreshCommand { get; set; }

        private async void CmdRefresh()
        {
            IsRefreshing = true;
            // wait 3 secs for demo
            //await Task.Delay(3000);
            //await LoadData();
            await LoadData();
            IsRefreshing = false;
        }

        private async Task LoadData()
        {
            try
            {
                var database = new ProductProductDb();
                var allItems = await database.GetItemsAsync();
                IEnumerable<product_product> filtered = null;

                //Filtro por ID/Code
                if (!string.IsNullOrWhiteSpace(FilterCode))
                {
                    int int_filterCode;

                    if (int.TryParse(FilterCode, out int_filterCode))
                    {
                        filtered = allItems.Where(x => x.id == int_filterCode);
                    }
                }
                else
                {
                    //Filtro por VAT
                    if (FilterCategory > 0)
                    {
                        filtered = allItems.Where(x => x._categ_id == FilterCategory);
                    }
                    else
                    {
                        //Filtro por nombre
                        if (string.IsNullOrWhiteSpace(FilterName))
                        {
                            FilterName = "";
                        }

                        filtered = string.IsNullOrWhiteSpace(FilterName)
                            ? allItems.Where(x=> !x.image_256.Contains("false"))
                            : allItems.Where(x => x.name.Contains(FilterName, StringComparison.OrdinalIgnoreCase));

                        filtered = allItems;
                    }
                }

                Debug.WriteLine("Filtro actual:" + FilterName);

                string userSearch = "";

                //if (int.TryParse(str_codagencia, out company_id)) { }

                TotalItems = filtered.Count();

                var paginated = filtered
                    .Skip((_page - 1) * _pageSize)
                    .Take(_pageSize);

                _itemsData = [.. paginated];
                OnPropertyChanged(nameof(ItemsData));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));

            }
            catch (Exception ex)
            {
                _itemsData = new ObservableCollection<product_product>();
                Debug.WriteLine(ex.ToString());
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
        //private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
