using DMOrders.Models.Filters;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.DMOrders.tareas;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using Microsoft.Maui;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Activities
{
    public class ListViewModel : INotifyPropertyChanged
    {
        private ProjectTaskDb _db { get; set; }

        public ObservableCollection<ProjectTask> ItemsData
        {
            get => _itemsData;
            set
            {
                _itemsData = value;
                OnPropertyChanged(nameof(ItemsData));
            }
        }

        private ObservableCollection<ProjectTask> _itemsData;

        private readonly SemaphoreSlim _loadLock = new(1, 1);
        private CancellationTokenSource _cts;
        private string BuildFilterSignature() =>
            $"{filters.getStatus()}|{filters.getDateStart()}|{filters.getDateEnd()}";

        private string _lastFilterSignature;
        public Filters filters { get; set; }
        private int _totalItems = 0;
        private int _pageSize = 14;
        private int _page = 1;

        public bool CanGoNext => (_page * PageSize) < TotalItems;
        public bool CanGoPrevious => _page > 1;

        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        private bool _paginationEnabled = false;
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
            }
        }

        /*************************************************************/

        private ProjectTask _selectedItem;

        public ProjectTask SelectedItem
        {
            get => _selectedItem;
            set
            {
                //if (_selectedActivity != value)
                //{
                //    _selectedActivity = value;
                //    OnPropertyChanged();
                //    OnPropertyChanged(nameof(SelectedActivity));
                //}

                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged();                   
                }
            }
        }


        public ListViewModel(Filters _filters)
        {
            _db = new ProjectTaskDb(App.Session.odooConnection.DbNameSqlite);

            filters = _filters;
            _itemsData = new ObservableCollection<ProjectTask>();
            //LoadDataByTimer();
            //EditCommand = new Command(EditItem);
            //EditCommand = new RelayCommand<ActivityHeader>(EditItem);
        }

        public async Task LoadData()
        {
            int partner_id = App.Session.CurrentUserFront.partner_id;
            int user_id = App.Session.CurrentUserFront.uid;

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
                    filters.getDateStart(),
                    filters.getDateEnd(),
                    filters.getStatus(),
                    0,
                    user_id,
                    Page,
                    PageSize,
                    ct);

                TotalItems = total;

                // Evita recrear la OC (menos churn de UI)
                if (ItemsData == null)
                    ItemsData = new ObservableCollection<ProjectTask>();
                else
                    ItemsData.Clear();

                foreach (var it in items)
                {
                    //ResPartnerDb resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
                    //var partnerItem = await resPartnerDb.GetItemsAsync(it._company_id, it._partner_id);

                    //if (partnerItem != null)
                    //{
                    //    it.partner_display_name = partnerItem?.name ?? "-";
                    //    it.partner_display_address = partnerItem?.street ?? "";
                    //    it.partner_display_status = partnerItem?.active == true ? "Activo" : "Inactivo";
                    //}
                    //else
                    //{
                    //    Debug.WriteLine($"Error cargando partner: No encontrado");
                    //}

                    it.display_username = App.Session.CurrentUserFront.nombres;
                    it.create_user = App.Session.CurrentUserFront.username;
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
                ItemsData = new ObservableCollection<ProjectTask>();
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

        public void LoadDataByTimer()
        {
            LoadData();
            ////// Usamos el dispatcher global de la app para garantizar ejecución en UI
            ////var dispatcher = Application.Current.Dispatcher;

            ////var timer = dispatcher.CreateTimer();
            ////timer.Interval = TimeSpan.FromMilliseconds(300); // delay corto para dejar respirar la UI
            ////timer.IsRepeating = false;

            ////timer.Tick += async (s, e) =>
            ////{
            ////    try
            ////    {
            ////        if (IsLoading) return; // Previene cargas simultáneas
            ////        await LoadData();
            ////    }
            ////    catch (Exception ex)
            ////    {
            ////        Debug.WriteLine($"Error en LoadData: {ex}");
            ////    }
            ////    finally
            ////    {
            ////        timer.Stop();
            ////    }
            ////};

            ////timer.Start();
        }

        //public ICommand EditCommand { get; set; }

        //private async void EditItem(object obj)
        //{
        //    Debug.WriteLine("EditItem");
        //    Details viewObj = new Details();
        //    //objPage.Sel_AccountMoveSendHeader = (AccountMoveSendHeader)obj;
        //    //objPage.editionMode = true;
        //    //objPage.Disappearing += NewGroup_Disappearing;
        //    //await Navigation.PushAsync(viewObj, false);
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

        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
