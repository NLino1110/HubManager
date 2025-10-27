
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using DMOrders.Models.Filters;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.Native;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Customers
{
    public partial class ViewModel : INotifyPropertyChanged
    {
        private ResPartnerDb _db { get; set; }
        private readonly SemaphoreSlim _loadLock = new(1, 1); // evita cargas simultáneas
        private CancellationTokenSource _cts;
        private string BuildFilterSignature() =>
            $"{filters.getCode()}|{filters.getName()}|{filters.getDays()}|{filters.getStatus()}";

        private string _lastFilterSignature;
        public Filters filters { get; set; }

        private ObservableCollection<res_partner> _itemsData;
                
        private res_partner _selectedItem;
        private bool _teamColumnVisible = true;
        private bool _wonColumnVisible = true;
        private bool _headerBordersVisible = true;
        private bool _paginationEnabled = false;
        private ushort _teamColumnWidth = 70;

        private int _totalItems = 0;
        private int _pageSize = 14;
        private int _page = 1;

        int company_id = 0;

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


        //public ViewModel(string _FilterCode, string _FilterId, string _FilterName, FDays _FilterDays, FStatus _FilterStatus)
        //{
        //    _itemsData = new ObservableCollection<res_partner>();            
        //    FilterCode = _FilterCode;
        //    FilterId = _FilterId;
        //    FilterName = _FilterName;
        //    FilterDays = _FilterDays;
        //    FilterStatus = _FilterStatus;
        //    LoadDataByTimer();
        //}        

        public ViewModel(Filters _filters)
        {
            _db = new ResPartnerDb();
            filters = _filters;
            _itemsData = new ObservableCollection<res_partner>();
            ItemTappedCommand = new Command<res_partner>(OnItemTapped);
            //LoadDataByTimer();
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

        [RelayCommand]
        void RelayRowTapped()
        {            
            Debug.WriteLine("RelayRowTapped called");
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
                var (items, total) = await _db.GetPagedAsync(
                    filters.getCode(),
                    filters.getVat(),
                    filters.getName(), 
                    filters.getDays(), 
                    filters.getStatus(), 
                    0, 
                    Page, 
                    PageSize, 
                    ct);

                TotalItems = total;

                // Evita recrear la OC (menos churn de UI)
                if (ItemsData == null)
                    ItemsData = new ObservableCollection<res_partner>();
                else
                    ItemsData.Clear();

                foreach (var it in items)
                    ItemsData.Add(it);
                
            }
            catch (OperationCanceledException ecx)
            {
                // ignorar: una nueva carga comenzó
                Debug.WriteLine(ecx);
            }
            catch (Exception ex)
            {
                ItemsData = new ObservableCollection<res_partner>();
                Debug.WriteLine(ex);
            }
            finally
            {
                IsLoading = false;
                _loadLock.Release();
                stopwatch.Stop();
                Debug.WriteLine($"[CatalogViewerModel] Carga en {stopwatch.ElapsedMilliseconds} ms | TotalItems: {TotalItems}, Page: {Page}, PageSize: {PageSize}, Filtro: {filters.getCode() ?? filters.getName() ?? "sin filtro"}");
            }
        }

        //public async Task ___LoadData()
        //{
        //    if (IsLoading) return;

        //    try
        //    {
        //        if(_itemsData == null)
        //            _itemsData = new ObservableCollection<res_partner>();
                
        //        _itemsData.Clear();

        //        IsLoading = true;

        //        var database = new ResPartnerDb();

        //        // 🔹 Lo ideal: aplicar filtros y paginación en la consulta al DB
        //        var allItems = await database.GetItemsAsync();

        //        // 🔹 Si tu método GetItemsAsync no soporta filtros, entonces:
        //        // var allItemsList = (await database.GetItemsAsync()).ToList();

        //        IEnumerable<res_partner> filtered = allItems; // ya viene filtrado si lo haces en DB

        //        // Filtros en memoria solo si no puedes hacerlos en DB
        //        if (!string.IsNullOrWhiteSpace(filters.getCode()) && int.TryParse(filters.getCode(), out int int_filterCode))
        //        {
        //            filtered = filtered.Where(x => x.id == int_filterCode);
        //        }
        //        else if (!string.IsNullOrWhiteSpace(filters.getCode()))
        //        {
        //            filtered = filtered.Where(x => x.vat == filters.getCode());
        //        }
        //        else if (!string.IsNullOrWhiteSpace(filters.getName()))
        //        {
        //            filtered = filtered.Where(x => x.name.Contains(filters.getName(), StringComparison.OrdinalIgnoreCase));
        //        }

        //        // Materializamos la lista para no volver a recorrerla varias veces
        //        var filteredList = filtered.ToList();

        //        TotalItems = filteredList.Count;

        //        // Paginación en memoria solo si no la hace el DB
        //        var paginated = filteredList
        //            .Skip((_page - 1) * _pageSize)
        //            .Take(_pageSize)
        //            .ToList();
                
        //        _itemsData = new ObservableCollection<res_partner>(paginated);

        //        OnPropertyChanged(nameof(ItemsData));
        //        OnPropertyChanged(nameof(CanGoNext));
        //        OnPropertyChanged(nameof(CanGoPrevious));
        //    }
        //    catch (Exception ex)
        //    {
        //        _itemsData = new ObservableCollection<res_partner>();
        //        Debug.WriteLine(ex);
        //    }
        //    finally
        //    {
        //        IsLoading = false;
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
        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        public ICommand ItemTappedCommand { get; }
        public void OnItemTapped(res_partner tappedItem)
        {
            foreach (var res_partner_item in _itemsData)
                res_partner_item.IsSelected = false;

            tappedItem.IsSelected = true;            
            WeakReferenceMessenger.Default.Send(new ItemSelectedMessage(tappedItem));           
        }

        public class ItemSelectedMessage : ValueChangedMessage<res_partner>
        {
            public ItemSelectedMessage(res_partner value) : base(value) { }
        }
    }
}
