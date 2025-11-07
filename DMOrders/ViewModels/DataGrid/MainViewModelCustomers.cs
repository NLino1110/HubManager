using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using RestSharp;
using DMSA.Models.General;
using ApiManager;
using DMSA.Models.Clientes;
using DMSA.Models.Odoo.Native;
using DMOrders.Services.Database.Sqlite;

namespace DMOrders.ViewModels.DataGrid
{
    public class MainViewModelCustomers : INotifyPropertyChanged
    {
        private List<res_partner> _itemsData;
        private res_partner _selectedItem;
        private bool _isRefreshing;
        private bool _teamColumnVisible = true;
        private bool _wonColumnVisible = true;
        private bool _headerBordersVisible = true;
        private bool _paginationEnabled = true;
        private ushort _teamColumnWidth = 70;
                
        private string FilterName { get; set; }

        public MainViewModelCustomers(string _FilterName)
        {
            FilterName = _FilterName;
            var task = Task.Run(async () =>
            {
                await LoadData();
            });
            Task.WaitAll(task);
            RefreshCommand = new Command(CmdRefresh);
        }

        public MainViewModelCustomers()
        {
            var task = Task.Run(async () =>
            {
                await LoadData();
            });

            Task.WaitAll(task);

            RefreshCommand = new Command(CmdRefresh);
        }

        public List<res_partner> ItemsData
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

        private async Task LoadDataStatic()
        {            
            ItemsData = new List<res_partner>()
            {
                new res_partner()
                {
                    id= 1,
                    name = "Cliente 1",
                    display_name = "Cliente 1",

                }
            };
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

                string userSearch = "";
                string str_codagencia = "";

                int codagencia = 0;

                if (int.TryParse(str_codagencia, out codagencia)) { }

                var database = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
                
                //ItemsData = (await database.GetItemsAsync()).Where(x=>x.name.Contains(FilterName)).ToList();

                ItemsData = (await database.GetItemsAsync()).Take(100).ToList();
                Debug.WriteLine("Total de clientes:" + ItemsData.Count);

            }
            catch (Exception ex)
            {
                ItemsData = new List<res_partner>();
                Debug.WriteLine(ex.ToString());
            }
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        #endregion INotifyPropertyChanged implementation
    }
}
