using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.Input;
using DMSA.Models.Odoo.Tools;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DMSA.Sync.Core.Services
{
    public partial class PushRelay
    {        
        private HubConnection _hubConnection;

        string _name;
                
        string _message;

        string _device_id;

        ObservableCollection<string> _messages;
                
        bool _isConnected;

        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string DeviceId
        {
            get { return _device_id; }
            set { _device_id = value; }
        }

        public void _PushRelay(string ServerUrl)
        {            
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(ServerUrl,
                (opts) =>
                {
                    opts.HttpMessageHandlerFactory = (message) =>
                    {
                        if (message is HttpClientHandler clientHandler)
                            // always verify the SSL certificate
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                (sender, certificate, chain, sslPolicyErrors) => { return true; };
                        return message;
                    };

                    opts.WebSocketConfiguration = wsc =>
                        wsc.RemoteCertificateValidationCallback = (sender, certificate, chain, policyErrors) => true;
                })
                .WithAutomaticReconnect()
                .Build();

            var retryTimer = new System.Timers.Timer();
            retryTimer.Elapsed += async (sender, e) =>
            {
                Debug.WriteLine(_hubConnection.State);
                if (_hubConnection.State == HubConnectionState.Disconnected ||
                (DeviceInfo.Platform == DevicePlatform.Android && _hubConnection.State == HubConnectionState.Connecting))
                {
                    Console.WriteLine("Intentando conectar...");
                    try
                    {
                        await _hubConnection.StartAsync();
                        
                        await ReceiveInfoDevice(_hubConnection.ConnectionId);

                        if(_hubConnection.State == HubConnectionState.Connected)
                        {                            
                            retryTimer.Stop();
                        }
                    }
                    catch(Exception ex)
                    {                        
                        Debug.WriteLine("SERVIDOR PUSHHH OFFLINE!!");
                    }
                }
            };

            retryTimer.Interval = 5000;
            retryTimer.Start();
            
            _hubConnection.Reconnected += (string arg) =>
            {
                Debug.WriteLine($"SE RECONECTO!!!");
                ReceiveInfoDevice(_hubConnection.ConnectionId).Wait();
                return Task.CompletedTask;
            };
                        
            _hubConnection.Closed += (Exception arg) =>
            {
                Debug.WriteLine($"SE SALIO!!!");                
                return Task.CompletedTask;
            };
                        
            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Toast.Make($"{user} says {message}").Show();                    
                    Debug.WriteLine($"{user} says {message}");

                });                
            });

            _hubConnection.On<string, string>("ReceiveNotify", (user, message) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Debug.WriteLine($"{user} says {message}");
                });
            });

            _hubConnection.On<string, string>("RequireInfoDevice", (HubId, message) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Toast.Make($"{HubId} RequireInfoDevice").Show();

                    await ReceiveInfoDevice(HubId);

                    Debug.WriteLine($"{HubId} says {message}");
                });
            });

            _hubConnection.On<string, string>("RequireFullInfoDevice", (HubId, message) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Toast.Make($"{HubId} RequireFullInfoDevice").Show();

                    Sensors sensors = new Sensors();
                    var getdataExtra = await sensors.GetDeviceDataAsync();
                    
                    var getdata = GetDeviceInfo(HubId);
                    getdata.CurrentLocation = getdataExtra.CurrentLocation;
                    getdata.battery = getdataExtra.battery;
                    getdata.freeRam = getdataExtra.freeRam;
                    getdata.freeStorage = getdataExtra.freeStorage;
                    getdata.UserData = getdataExtra.UserData;

                    await ReceiveFullInfoDevice(getdata);
                    Debug.WriteLine($"{HubId} says {message}");
                });
            });
        }

        public ConnectedDevice GetDeviceInfo(string HubId)
        {
            ConnectedDevice device = new ConnectedDevice();
            device.Id = HubId;
            device.DeviceId = _device_id;
            device.AppName = AppInfo.Current.Name;
            device.PackageName = AppInfo.Current.PackageName;
            device.VersionString = Constants.Session.AppVersion;
            device.BuildString = AppInfo.Current.BuildString;
            device.UserData = "";
            device.Idiom = DeviceInfo.Current.Idiom.ToString();
            device.Manufacturer = DeviceInfo.Current.Manufacturer;
            device.DeviceName = DeviceInfo.Current.Name;
            device.OsVersion = DeviceInfo.Current.VersionString;
            device.Platform = DeviceInfo.Current.Platform.ToString();
            device.SerialNumber = "";
            device.Model = DeviceInfo.Current.Model;

            return device;
        }


        [Obsolete("Ya no se va a usar")]
        public PushRelay(string ServerPush)
        {
            _PushRelay(ServerPush);
        }

        public PushRelay(string ServerPush, string HubId)
        {
            _PushRelay(ServerPush);
            _device_id = HubId;
        }

        [RelayCommand]
        async Task Connect()
        {
            if (_hubConnection.State == HubConnectionState.Connecting ||
                _hubConnection.State == HubConnectionState.Connected) return;
            try
            {
                await _hubConnection.StartAsync();
                await ReceiveInfoDevice(_hubConnection.ConnectionId);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Connect");
                Debug.WriteLine($"Error: {e}");
            }

            IsConnected = true;
        }

        [RelayCommand]
        async Task ReceiveInfoDevice(string HubId)
        {
            ConnectedDevice device = new ConnectedDevice();
            device.Id = HubId;
            device.DeviceId = _device_id;
            device.AppName = AppInfo.Current.Name;
            device.PackageName = AppInfo.Current.PackageName;
            device.VersionString = Constants.Session.AppVersion;
            device.BuildString = AppInfo.Current.BuildString;
            device.UserData = "";
            device.Idiom = DeviceInfo.Current.Idiom.ToString();
            device.Manufacturer = DeviceInfo.Current.Manufacturer;
            device.DeviceName = DeviceInfo.Current.Name;
            device.OsVersion = DeviceInfo.Current.VersionString;
            device.Platform = DeviceInfo.Current.Platform.ToString();
            device.SerialNumber = "";
            device.Model = DeviceInfo.Current.Model;

            string jsonDataSend = JsonConvert.SerializeObject(device);

            //var currentNetwork = Connectivity.NetworkAccess;
            //if (currentNetwork == NetworkAccess.Internet)
            //{
            //    var ipAddress = NetworkInterface.GetAllNetworkInterfaces();
            //    foreach (var interfaceItem in ipAddress)
            //    {
            //        try
            //        {
            //            Debug.WriteLine($"Interface: {interfaceItem.Description}");
            //            Debug.WriteLine($"Dirección IP del dispositivo: {interfaceItem.GetIPStatistics()}");
            //        }
            //        catch(Exception ex)
            //        {
            //            Debug.WriteLine($"Error obteniendo Ip");
            //            Debug.WriteLine(ex.Message);
            //        }
            //    }
            //    //Console.WriteLine($"Dirección IP del dispositivo: {ipAddress}");
            //}

            try
            {
                await SendMessage("ReceiveInfoDevice", jsonDataSend);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"ReceiveInfoDevice");
                Debug.WriteLine($"Error: {e}");
            }            
        }

        [RelayCommand]
        async Task ReceiveFullInfoDevice(ConnectedDevice device)
        {
            string jsonDataSend = JsonConvert.SerializeObject(device);

            try
            {
                await SendMessage("ReceiveFullInfoDevice", jsonDataSend);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"ReceiveFullInfoDevice");
                Debug.WriteLine($"Error: {e}");
            }
        }

        [RelayCommand]
        async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Message)) return;

            try
            {
                await _hubConnection.InvokeAsync("SendMessage", Name, Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"SendMessage");
                Debug.WriteLine($"Error: {e}");
            }

            Message = string.Empty;
        }

        async Task SendMessage(string CommandName, string RegularString)
        {
            if (string.IsNullOrWhiteSpace(CommandName) || string.IsNullOrWhiteSpace(RegularString)) return;

            await _hubConnection.InvokeAsync(CommandName, RegularString);

            Message = string.Empty;
        }
    }
}
