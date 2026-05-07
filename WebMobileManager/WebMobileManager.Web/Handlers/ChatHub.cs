using DMSA.Models.Odoo.Tools;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace WebMobileManager.Web.Handlers
{
    public class ChatHub : Hub
    {
        private readonly ConnectionManager _connectionManager;

        public ChatHub(ConnectionManager connectedUsers)
        {
            _connectionManager = connectedUsers;
        }

        public ChatHub()
        {
            _connectionManager = new ConnectionManager();
        }

        public void ClearDevices()
        {
            _connectionManager.ClearDevices();
        }

        public List<ConnectedDevice> GetDevices()
        {
            return _connectionManager.GetDevices();
        }

        public override async Task OnConnectedAsync()
        {
            ConnectedDevice connectedDevice = new ConnectedDevice();
            connectedDevice.Id = Context.ConnectionId;            
            
            _connectionManager.AddDevice(connectedDevice);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            ConnectedDevice connectedDevice = new ConnectedDevice();
            connectedDevice.Id = Context.ConnectionId;

            Console.WriteLine("DisconnectedAsync");
            Console.WriteLine(connectedDevice.Id);
            _connectionManager.RemoveDevice(connectedDevice);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task RequireInfoDevice(string Id)
        {            

            if (Clients == null)
            {
                return;
            }

            Console.WriteLine("RequireInfoDevice");
            Console.WriteLine(Id);
            await Clients.Client(Id).SendAsync("RequireInfoDevice", Id, "extra-data");
        }

        public async Task ReceiveInfoDevice(string JsonDeviceData)
        {
            if(JsonDeviceData == null) return;

            Console.WriteLine("ReceiveInfoDevice");
            Console.WriteLine(JsonDeviceData);

            ConnectedDevice device = new ConnectedDevice();            
            device = JsonConvert.DeserializeObject<ConnectedDevice>(JsonDeviceData);
            _connectionManager.UpdateDevice(device);
        }

        public async Task RequireFullInfoDevice(string Id)
        {
            if (Clients == null)
            {
                return;
            }

            Console.WriteLine("RequireFullInfoDevice");
            Console.WriteLine(Id);
            await Clients.Client(Id).SendAsync("RequireFullInfoDevice", Id, "extra-data");            
        }

        public async Task ReceiveFullInfoDevice(string JsonDeviceData)
        {
            if (string.IsNullOrEmpty(JsonDeviceData)) return;
            Console.WriteLine("ReceiveInfoDevice");
            Console.WriteLine(JsonDeviceData);
            ConnectedDevice device = new ConnectedDevice();
            device = JsonConvert.DeserializeObject<ConnectedDevice>(JsonDeviceData);
            _connectionManager.UpdateDevice(device);
        }        

        public async Task SendMessage(string user, string message)
        {
            if (Clients == null)
            {
                return;
            }

            Console.WriteLine("SendMessage");
            Console.WriteLine(user);
            Console.WriteLine(message);

            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendMessageDevice(string connectionId, string user, string message)
        {
            if(Clients == null)
            {
                return;
            }

            Console.WriteLine("SendMessage");
            Console.WriteLine(user);
            Console.WriteLine(message);

            await Clients.Client(connectionId).SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendNotifyDevice(string connectionId, string user, string message)
        {
            if (Clients == null)
            {
                return;
            }

            Console.WriteLine("SendNotifyDevice");
            Console.WriteLine(user);
            Console.WriteLine(message);

            await Clients.Client(connectionId).SendAsync("ReceiveNotify", user, message);
        }

        public async Task SendToIndividual(string connectionId, string message)
        {
            await Clients.Client(connectionId).SendAsync(GetMessageToSend(message));
        }

        private string GetMessageToSend(string originalMessage)
        {
            return $"User connection id: {Context.ConnectionId}. Message: {originalMessage}";
        }        
    }    
}
