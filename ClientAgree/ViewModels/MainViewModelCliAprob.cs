using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using ClientAgree.Models;
using ClientAgree.Utils;
using RestSharp;
using DMSA.Models.General;
using ApiManager;

using Models.DMSA.Mbw.Clientes;

namespace ClientAgree.ViewModels
{
    public class MainViewModelCliAprob : INotifyPropertyChanged
    {
        private List<ClienteAprobacion> _itemsData;
        private ClienteAprobacion _selectedItem;
        private bool _isRefreshing;
        private bool _teamColumnVisible = true;
        private bool _wonColumnVisible = true;
        private bool _headerBordersVisible = true;
        private bool _paginationEnabled = true;
        private ushort _teamColumnWidth = 70;

        //
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
                //await LoadData_old();
                await LoadData();
            });

            Task.WaitAll(task);

            //App.Current.MainPage = new MainPage();

            //ItemsData = DummyDataProvider.GetTeams();
            //ItemsData = new List<ClienteAprobacion>()
            //{
            //    new ClienteAprobacion()
            //    {
            //        IDENTIFICACION="0919826958",
            //        APELLIDOSCLIENTE="CHONILLO VILLON",
            //        APLICACIONORIGEN="",
            //        APLICACIONVERSION="1.0",
            //        CODAGENCIA=0,
            //        CODCLIENTE=0,
            //        CODEMPRESA="0",
            //        CODVENDEDOR="0",
            //        DIRECCIONCLIENTE="BOSQUES DE LA COSTA",
            //        FECHACAMBIOESTADO=DateTime.Now,
            //        FECHAREGISTRO=DateTime.Now,
            //        MARCAEQUIPO="",
            //        MODELOEQUIPO="",
            //        NOMBRESCLIENTE="RONALD STALIN",
            //        NUMPEDIDO=0,
            //        PLATAFORMAMODIFICA="",
            //        PLATAFORMAORIGEN="",
            //        TELEFONOCLIENTE="0992601015",
            //        TIPOIDENTIFICACION="C",
            //        EMAILCLIENTE="ronald.chonillo@gmail.com"
            //    },           
            //};

            RefreshCommand = new Command(CmdRefresh);
        }

        public List<ClienteAprobacion> ItemsData
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

        public ClienteAprobacion SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                Debug.WriteLine("Team Selected : " + value?.IDENTIFICACION);
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
            await LoadData();
            IsRefreshing = false;
        }


        //private async Task LoadData_old()
        //{
        //    string Action = "OBTENER_APROBACIONES";
        //    string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //    string cadenaJson = "{\"indice\":\"" + 0 + "\",\"esActualizacion\":" + "true" + ",\"fechatablet\":\"" + currentDateTime + "\"}";

        //    List<RestSharp.Parameter> parameters = new List<RestSharp.Parameter>();

        //    parameters.Add(RestSharp.Parameter.CreateParameter("codusuario", "caja1", ParameterType.QueryString));
        //    parameters.Add(RestSharp.Parameter.CreateParameter("accion", Action, ParameterType.QueryString));
        //    parameters.Add(RestSharp.Parameter.CreateParameter("cadenaJson", cadenaJson, ParameterType.QueryString));

        //    SoapClient client = new SoapClient();
        //    var result = await client.asyncPostJson_old(parameters.ToArray());
        //    Console.WriteLine(result);

        //    //Se obtiene resultado
        //    var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse_v1>(result);
        //    //Se lee la sección de los resultados de los datos
        //    var dataJson = Newtonsoft.Json.JsonConvert.SerializeObject(resultUser.data);
        //    //Se deserializan los datos y se los convierte a List<>
        //    var dataList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClienteAprobacion>>(dataJson);

        //    ItemsData = dataList;
        //}

        private async Task LoadData()
        {
            try
            {
                if (FilterName == null || FilterName == "" || FilterName.Length == 0)
                {
                    FilterName = "%";
                }

                if (App.Session.CurrentUser.accesos.Length > 0 &&
                    App.Session.CurrentUser.accesos[0].agencias.Length > 0)
                {

                }

                Debug.WriteLine("Filtro actual:" + FilterName);

                string userSearch = App.Session.CurrentUser.codigoUsuario;
                string str_codagencia = App.Session.CurrentUser.accesos[0].agencias[0].CodAgencia.ToString();

                int codagencia = 0;

                if (int.TryParse(str_codagencia, out codagencia)) { }

                HubClienteAprobacion hubClienteAprobacion = new HubClienteAprobacion(App.Session);
                //var clienteA = await hubClienteAprobacion.GetForAgree();
                //var clienteA = await hubClienteAprobacion.GetForAgree(userSearch, 1);
                
                //codstatus=53 Para obtener los que no han sido aprobados/revocados (están en cola)
                // se envía 1 y el api lo asume como 53
                var clienteA = await hubClienteAprobacion.GetForAgree(codagencia, 1, FilterName);
                //var clienteA = await hubClienteAprobacion.ExecuteGetAsync("");

                if (clienteA.data.Length > 0)
                {
                    //if(FilterName!= null && FilterName != "" && FilterName.Length>0)
                    //{
                    //    ItemsData = clienteA.data.Where(dc => dc.APELLIDOSCLIENTE.Contains(FilterName) ||
                    //    dc.NOMBRESCLIENTE.Contains(FilterName) ||
                    //    dc.IDENTIFICACION.Contains(FilterName)
                    //    ).ToList();

                    //    return;
                    //}

                    //ApiResponse_v2 dataListTest = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse_v2>(clienteA.Content);

                    //string Action = "OBTENER_APROBACIONES";
                    //string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    //string cadenaJson = "{\"indice\":\"" + 0 + "\",\"esActualizacion\":" + "true" + ",\"fechatablet\":\"" + currentDateTime + "\"}";

                    //List<RestSharp.Parameter> parameters = new List<RestSharp.Parameter>();

                    //parameters.Add(RestSharp.Parameter.CreateParameter("codusuario", "caja1", ParameterType.QueryString));
                    //parameters.Add(RestSharp.Parameter.CreateParameter("accion", Action, ParameterType.QueryString));
                    //parameters.Add(RestSharp.Parameter.CreateParameter("cadenaJson", cadenaJson, ParameterType.QueryString));

                    //SoapClient client = new SoapClient();
                    //var result = await client.asyncPostJson(parameters.ToArray());
                    //Console.WriteLine(result);

                    ////Se obtiene resultado
                    //var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse_v1>(result);
                    ////Se lee la sección de los resultados de los datos
                    //var dataJson = Newtonsoft.Json.JsonConvert.SerializeObject(resultUser.data);
                    ////Se deserializan los datos y se los convierte a List<>
                    //var dataList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClienteAprobacion>>(dataJson);

                    ItemsData = clienteA.data.ToList();
                }
            }
            catch (Exception ex)
            {
                ItemsData = new List<ClienteAprobacion>();
                Debug.WriteLine(ex.ToString());
            }
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        #endregion INotifyPropertyChanged implementation
    }
}
