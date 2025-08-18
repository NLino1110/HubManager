
using CommunityToolkit.Mvvm.Input;
using DMOrders.Models.Filters;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.Native;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Customers
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
            LoadDataByTimer();
        }        

        public ViewModel()
        {
            LoadDataByTimer();
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

        public async Task LoadData()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var database = new ResPartnerDb();

                // 🔹 Lo ideal: aplicar filtros y paginación en la consulta al DB
                var allItems = await database.GetItemsAsync();

                // 🔹 Si tu método GetItemsAsync no soporta filtros, entonces:
                // var allItemsList = (await database.GetItemsAsync()).ToList();

                IEnumerable<res_partner> filtered = allItems; // ya viene filtrado si lo haces en DB

                // Filtros en memoria solo si no puedes hacerlos en DB
                if (!string.IsNullOrWhiteSpace(FilterCode) && int.TryParse(FilterCode, out int int_filterCode))
                {
                    filtered = filtered.Where(x => x.id == int_filterCode);
                }
                else if (!string.IsNullOrWhiteSpace(FilterId))
                {
                    filtered = filtered.Where(x => x.vat == FilterId);
                }
                else if (!string.IsNullOrWhiteSpace(FilterName))
                {
                    filtered = filtered.Where(x => x.name.Contains(FilterName, StringComparison.OrdinalIgnoreCase));
                }

                // Materializamos la lista para no volver a recorrerla varias veces
                var filteredList = filtered.ToList();

                TotalItems = filteredList.Count;

                // Paginación en memoria solo si no la hace el DB
                var paginated = filteredList
                    .Skip((_page - 1) * _pageSize)
                    .Take(_pageSize)
                    .ToList();

                _itemsData = new ObservableCollection<res_partner>(paginated);

                OnPropertyChanged(nameof(ItemsData));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));
            }
            catch (Exception ex)
            {
                _itemsData = new ObservableCollection<res_partner>();
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }



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
        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    }
}
