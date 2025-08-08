using ApiManager;
using CommunityToolkit.Mvvm.Input;
using DMOrdersUI.Models.Filters;
using DMOrdersUI.Services.Database.Sqlite;
using DMSA.Models.Clientes;
using DMSA.Models.General;
using DMSA.Models.Odoo.Native;
using Microsoft.Maui;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMOrdersUI.Pages.Fragments.Customers
{
    public partial class ViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<res_partner> _itemsData;
                
        private res_partner _selectedItem;
        private bool _isRefreshing;
        private bool _teamColumnVisible = true;
        private bool _wonColumnVisible = true;
        private bool _headerBordersVisible = true;
        private bool _paginationEnabled = false;
        private ushort _teamColumnWidth = 70;

        private int _totalItems = 0;
        private int _pageSize = 14;
        private int _page = 1;

        int company_id = 0;

        private string FilterCode { get; set; }
        private string FilterId { get; set; }
        private string FilterName { get; set; }
        private FDays FilterDays { get; set; }
        private FStatus FilterStatus { get; set; }


        public bool CanGoNext => (_page * PageSize) < TotalItems;
        public bool CanGoPrevious => _page > 1;

        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

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


        public ViewModel(string _FilterCode, string _FilterId, string _FilterName, FDays _FilterDays, FStatus _FilterStatus)
        {
            FilterCode = _FilterCode;
            FilterId = _FilterId;
            FilterName = _FilterName;
            FilterDays = _FilterDays;
            FilterStatus = _FilterStatus;

            var task = Task.Run(async () =>
            {
                await LoadData();
            });
            Task.WaitAll(task);
            RefreshCommand = new Command(CmdRefresh);
        }

        public ViewModel()
        {
            var task = Task.Run(async () =>
            {
                await LoadData();
            });

            Task.WaitAll(task);

            RefreshCommand = new Command(CmdRefresh);
        }

        public ObservableCollection<res_partner> ItemsData
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

        public bool TeamColumnVisible
        {
            get => _teamColumnVisible;
            set
            {
                _teamColumnVisible = value;
                OnPropertyChanged(nameof(TeamColumnVisible));
            }
        }

        public bool WonColumnVisible
        {
            get => _wonColumnVisible;
            set
            {
                _wonColumnVisible = value;
                OnPropertyChanged(nameof(WonColumnVisible));
            }
        }

        public ushort TeamColumnWidth
        {
            get => _teamColumnWidth;
            set
            {
                _teamColumnWidth = value;
                OnPropertyChanged(nameof(TeamColumnWidth));
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

        public res_partner SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                Debug.WriteLine("Team Selected : " + value?.id);
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

        //private async Task LoadDataStatic()
        //{            
        //    ItemsData = new ObservableCollection<res_partner>()
        //    {
        //        new res_partner()
        //        {
        //            id= 1,
        //            name = "Cliente 1",
        //            display_name = "Cliente 1",

        //        }
        //    };
        //}

        private async Task LoadData()
        {
            try
            {
                var database = new ResPartnerDb();
                var allItems = await database.GetItemsAsync();
                IEnumerable<res_partner> filtered = null;

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
                    if (!string.IsNullOrWhiteSpace(FilterId))
                    {                        
                        filtered = allItems.Where(x => x.vat == FilterId);                        
                    }
                    else
                    {
                        //Filtro por nombre
                        if (string.IsNullOrWhiteSpace(FilterName))
                        {
                            FilterName = "";
                        }

                        filtered = string.IsNullOrWhiteSpace(FilterName)
                            ? allItems
                            : allItems.Where(x => x.name.Contains(FilterName, StringComparison.OrdinalIgnoreCase));
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
                _itemsData = new ObservableCollection<res_partner>();
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
        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    }
}
