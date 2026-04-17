using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.Input;
using DMSA.Models.Odoo.Tools;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DMCobranzas.Services
{
    public partial class PushRelay
    {
        //private readonly HubConnection _hubConnection;
        private HubConnection _hubConnection;

        //[ObservableProperty]
        string _name;

        //[ObservableProperty]
        string _message;

        //[ObservableProperty]
        ObservableCollection<string> _messages;

        //[ObservableProperty]
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

        public void _PushRelay(string ServerUrl)
        {
            //$"https://192.168.204.66:2443/chatHub"
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

                        //--------------------------------//
                        //Update deviceinfo for server data
                        await ReceiveInfoDevice(_hubConnection.ConnectionId);

                        if(_hubConnection.State == HubConnectionState.Connected)
                        {
                            //Se detiene cuando se conecta por primera vez
                            // de ahi en adelante .WithAutomaticReconnect() hace el trabajo
                            retryTimer.Stop();
                        }
                    }
                    catch(Exception ex)
                    {
                        //En Android no dispara el error
                        Debug.WriteLine("SERVIDOR PUSHHH OFFLINE!!");
                    }
                }
            };

            retryTimer.Interval = 5000; // Intervalo de 5 segundos
            retryTimer.Start();
            
            _hubConnection.Reconnected += (string arg) =>
            {
                Debug.WriteLine($"SE RECONECTOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO!!!!");
                ReceiveInfoDevice(_hubConnection.ConnectionId).Wait();
                return Task.CompletedTask;
            };

            //NO FUNCIONA CON .WithAutomaticReconnect()
            _hubConnection.Closed += (Exception arg) =>
            {
                Debug.WriteLine($"SE SALIOOOOO!!!");
                //retryTimer.Stop();
                return Task.CompletedTask;
            };

            //Messages ??= new ObservableCollection<string>();

            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                //if (MainThread.IsMainThread)
                //{
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Toast.Make($"{user} says {message}").Show();
                    //Messages.Add($"{user} says {message}");
                    Debug.WriteLine($"{user} says {message}");
                });
                //}
            });

            _hubConnection.On<string, string>("ReceiveNotify", (user, message) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    //Messages.Add($"{user} says {message}");
                    Debug.WriteLine($"{user} says {message}");
                });
            });

            _hubConnection.On<string, string>("RequireInfoDevice", (DeviceId, message) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Toast.Make($"{DeviceId} RequireInfoDevice").Show();

                    await ReceiveInfoDevice(DeviceId);

                    Debug.WriteLine($"{DeviceId} says {message}");
                });
            });
        }

        public PushRelay()
        {
            _PushRelay("https://192.168.204.66:2443/chatHub");
        }

        public PushRelay(string ServerPush)
        {
            _PushRelay(ServerPush);
        }

        [RelayCommand]
        async Task Connect()
        {
            if (_hubConnection.State == HubConnectionState.Connecting ||
                _hubConnection.State == HubConnectionState.Connected) return;
            try
            {
                await _hubConnection.StartAsync();

                //--------------------------------//
                //Update deviceinfo for server data
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
        async Task ReceiveInfoDevice(string DeviceId)
        {
            //TODO: Add session data on UserData

            ConnectedDevice device = new ConnectedDevice();
            device.Id = DeviceId;
            device.AppName = AppInfo.Current.Name;
            device.PackageName = AppInfo.Current.PackageName;
            device.VersionString = App.Session.AppVersion; // AppInfo.Current.VersionString;
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
            //Debug.WriteLine($"{DeviceId} says {message}");
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

        //[RelayCommand]
        //async Task SendMessage()
        //{
        //    if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Message)) return;

        //    try
        //    {
        //        await _hubConnection.InvokeAsync("SendMessage", Name, Message);
        //    }
        //    catch(Exception e)
        //    {
        //        Debug.WriteLine($"SendMessage");
        //        Debug.WriteLine($"Error: {e}");
        //    }            

        //    Message = string.Empty;
        //}

        //[RelayCommand]
        async Task SendMessage(string CommandName, string RegularString)
        {
            if (string.IsNullOrWhiteSpace(CommandName) || string.IsNullOrWhiteSpace(RegularString)) return;

            await _hubConnection.InvokeAsync(CommandName, RegularString);

            Message = string.Empty;
        }
    }
}
