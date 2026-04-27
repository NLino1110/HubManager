using ApiManager;
using DMSA.Models.Odoo.Customers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMDataSafe.ViewModels
{
    public class MainViewModelCliAprob : INotifyPropertyChanged
    {
        private List<CustomerDataConsent> _itemsData;
        private CustomerDataConsent _selectedItem;
        private bool _isRefreshing;
        private bool _teamColumnVisible = true;
        private bool _wonColumnVisible = true;
        private bool _headerBordersVisible = true;
        private bool _paginationEnabled = true;
        private ushort _teamColumnWidth = 70;

        private string FilterName { get; set; }

        public MainViewModelCliAprob(string _FilterName)
        {
            FilterName = _FilterName;
            var task = Task.Run(async () =>
            {
                await LoadData();
            });
            Task.WaitAll(task);
            RefreshCommand = new Command(CmdRefresh);
        }

        public MainViewModelCliAprob()
        {
            
            var task = Task.Run(async () =>
            {                
                await LoadData();
            });

            Task.WaitAll(task);
            
            RefreshCommand = new Command(CmdRefresh);
        }

        public List<CustomerDataConsent> ItemsData
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

        public CustomerDataConsent SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;                
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

        public ICommand RefreshCommand { get; set; }

        private async void CmdRefresh()
        {
            IsRefreshing = true;
            
            //await Task.Delay(3000);
            await LoadData();
            IsRefreshing = false;
        }

        private async Task LoadData()
        {
            try
            {
                if (FilterName == null || FilterName == "" || FilterName.Length == 0)
                {
                    FilterName = "%";
                }

                Debug.WriteLine("Filtro actual:" + FilterName);

                string userSearch = App.Session.CurrentUserFront.uid.ToString();
                //string str_codagencia = App.Session.res_center.id.ToString();

                int[] center_ids = { App.Session.res_center.id};
                

                HubCustomerDataConsent hubClienteAprobacion = new HubCustomerDataConsent(App.Session);
                
                //codstatus=53 Para obtener los que no han sido aprobados/revocados (están en cola)
                // se envía 1 y el api lo asume como 53
                var clienteA = await hubClienteAprobacion.GetForAgree(center_ids, 1, FilterName);
                //var clienteA = await hubClienteAprobacion.ExecuteGetAsync("");

                if (clienteA!= null && clienteA.result != null && clienteA.result.Count > 0)
                {                    
                    ItemsData = clienteA.result.ToList();
                }
            }
            catch (Exception ex)
            {
                ItemsData = new List<CustomerDataConsent>();
                Debug.WriteLine(ex.ToString());
            }
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        #endregion INotifyPropertyChanged implementation
    }
}
