using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using ResourceBuilder.Data;
using ResourceBuilder.Data.Structs;
using ResourceBuilder.Services.Automata;
using ResourceBuilder.Shared;
using System.Reflection;

using DMSA.Models.General.Responses;
using System.DirectoryServices.Protocols;
using DMSA.Models.Security;
using DMSA.Models.Odoo.General.Responses;
using Models.DMSA.Shared.Structs;
using Models.DMSA.Shared.Tools;

namespace ResourceBuilder.Pages
{
    public partial class ZipBuilder
    {
        string status_process { get; set; }
        ItemBuild current_buildItem {  get; set; }
        public Dictionary<string, object> AdditionalAttributes = new Dictionary<string, object>();
        ElementReference progressBarElement;
        private int progressValue = 0;

        [Inject]
        BuilderService builderService { get; set; }

        [Inject]
        IModalService modalService { get; set; }

        [Inject]
        IToastService toastService { get; set; }
        
        ItemBuild[] itemBuilds { get; set; }

        public Dictionary<string, object> AdditionalAttributesBusy =
            new Dictionary<string, object>();

        protected override Task OnInitializedAsync()
        {
            //listado = chatHub.ObtenerClientesConectados();
            current_buildItem = new ItemBuild() { 
                ActioName = "",
                Name = "Name",
                Process = true
            };
            return base.OnInitializedAsync();
        }

        private async Task LaunchCacheBuilder(dynamic document)
        {
            List<ItemBuild> builds = new List<ItemBuild>();
            //builds.Add(new ItemBuild() { Name = "FACNOTACREDITOCAB", ActioName = "OBTENER_FAC_NOTACREDITO_CAB", Process = true });
            //builds.Add(new ItemBuild() { Name = "FACNOTACREDITODET", ActioName = "OBTENER_FAC_NOTACREDITO_DET", Process = true });
            //builds.Add(new ItemBuild() { Name = "COBCARTERACAB", ActioName = "OBTENER_CARTERA_CAB", Process = true });
            //builds.Add(new ItemBuild() { Name = "COBCARTERADET", ActioName = "OBTENER_CARTERA_DET", Process = true });

            //TODO: buscar la tarea con el nombre "CacheBuilder" por ahora sumiremos que es la numero 0
            builds = ConfigurationHelper.GetAppSettings().profile.Tasks[0].items.ToList();

            toastService.ShowInfo("Iniciando el proceso de generación de cache!");
            AdditionalAttributesBusy.Add("disabled", "");
            foreach (var item in builds)
            {
                if (item.Process)
                {
                    bool res1 = await builderService.SendRequest(item);
                }
            }

            toastService.ShowInfo("Terminado el proceso de generación de cache!");
            AdditionalAttributesBusy.Remove("disabled");
            //var parameters = new ModalParameters();

            ////string Message = "Desea procesar este <documento>?";
            //parameters.Add(nameof(ModalEmailSend.ItemForSend), document);
            //var options = new ModalOptions
            //{
            //    UseCustomLayout = true,
            //    DisableBackgroundCancel = true
            //};
            //var messageForm = ModalService.Show<ModalEmailSend>("Enviar email", parameters, options);

            //var result = await messageForm.Result;

            //if (!result.Cancelled)
            //{
            //    toastService.ShowInfo("Documento agregado a la cola de envío");
            //    StateHasChanged();
            //}
            //else
            //{
            //    return;
            //}
        }

        private async Task LaunchCacheBuilderV2(dynamic document)
        {
            List<ItemBuild> builds = new List<ItemBuild>();
            builds.Add(new ItemBuild() { Name = "FACNOTACREDITOCAB", ActioName = "OBTENER_FAC_NOTACREDITO_CAB", Process = false });
            builds.Add(new ItemBuild() { Name = "FACNOTACREDITODET", ActioName = "OBTENER_FAC_NOTACREDITO_DET", Process = true });
            //builds.Add(new ItemBuild() { Name = "COBCARTERACAB", ActioName = "OBTENER_CARTERA_CAB", Process = true });
            //builds.Add(new ItemBuild() { Name = "COBCARTERADET", ActioName = "OBTENER_CARTERA_DET", Process = true });

            toastService.ShowInfo("Iniciando el proceso de generación de cache!");
            AdditionalAttributesBusy.Add("disabled", "");
            foreach (var item in builds)
            {
                if (item.Process)
                {
                    bool res1 = await builderService.SendRequestV2(item);
                }
            }

            toastService.ShowInfo("Terminado el proceso de generación de cache!");
            AdditionalAttributesBusy.Remove("disabled");
            //var parameters = new ModalParameters();

            ////string Message = "Desea procesar este <documento>?";
            //parameters.Add(nameof(ModalEmailSend.ItemForSend), document);
            //var options = new ModalOptions
            //{
            //    UseCustomLayout = true,
            //    DisableBackgroundCancel = true
            //};
            //var messageForm = ModalService.Show<ModalEmailSend>("Enviar email", parameters, options);

            //var result = await messageForm.Result;

            //if (!result.Cancelled)
            //{
            //    toastService.ShowInfo("Documento agregado a la cola de envío");
            //    StateHasChanged();
            //}
            //else
            //{
            //    return;
            //}
        }

        

        private async Task LaunchCacheBuilderOdoo(dynamic document)
        {
            var appSetting = ConfigurationHelper.GetAppSettings();
            AppSession _appSession = new AppSession();
            _appSession.odooConnection.Host = appSetting.profile.Odoo.Host;

            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key,
                access_token = appSetting.profile.Odoo.access_token,
                username = appSetting.profile.Odoo.User,
                password = appSetting.profile.Odoo.Password,
                uid = int.Parse(appSetting.profile.Odoo.uid),
                databasename = appSetting.profile.Odoo.Database
            };

            ResponseAuthenticate responseUser;

            User user = new User();
            user.username = _appSession.CurrentUser.username;
            user.password = _appSession.CurrentUser.password;
            user.databasename = _appSession.CurrentUser.databasename;

            DateTime currentDate = DateTime.Now;
            
            ApiManager.HubUser hubUser = new ApiManager.HubUser(_appSession);
            responseUser = await hubUser.TryLoginRpcWeb(user, currentDate);

            builderService.setAuthentication(responseUser);

            List<ItemBuild> builds = new List<ItemBuild>();

            //TODO: buscar la tarea con el nombre "CacheBuilder" por ahora sumiremos que es la numero 0
            builds = ConfigurationHelper.GetAppSettings().profile.Tasks[0].items.ToList();

            toastService.ShowInfo("Iniciando el proceso de generación de cache!");
            AdditionalAttributesBusy.Add("disabled", "");
            foreach (var item in builds)
            {
                if (item.Process)
                {
                    //bool res1 = await builderService.SendRequestOdoo(item);
                }
            }

            toastService.ShowInfo("Terminado el proceso de generación de cache!");
            AdditionalAttributesBusy.Remove("disabled");            
        }

        private async Task LaunchCacheBuilderOdooChucks(dynamic document)
        {
            var appSetting = ConfigurationHelper.GetAppSettings();
            AppSession _appSession = new AppSession();
            _appSession.odooConnection.Host = appSetting.profile.Odoo.Host;

            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key,
                access_token = appSetting.profile.Odoo.access_token,
                username = appSetting.profile.Odoo.User,
                password = appSetting.profile.Odoo.Password,
                uid = int.Parse(appSetting.profile.Odoo.uid),
                databasename = appSetting.profile.Odoo.Database
            };

            ResponseAuthenticate responseUser;

            User user = new User();
            user.username = _appSession.CurrentUser.username;
            user.password = _appSession.CurrentUser.password;
            user.databasename = _appSession.CurrentUser.databasename;

            DateTime currentDate = DateTime.Now;

            ApiManager.HubUser hubUser = new ApiManager.HubUser(_appSession);
            responseUser = await hubUser.TryLoginRpcWeb(user, currentDate);

            builderService.setAuthentication(responseUser);

            List<ItemBuild> builds = new List<ItemBuild>();

            //TODO: buscar la tarea con el nombre "CacheBuilder" por ahora sumiremos que es la numero 0
            builds = ConfigurationHelper.GetAppSettings().profile.Tasks[0].items.ToList();

            toastService.ShowInfo("Iniciando el proceso de generación de cache!");
            AdditionalAttributesBusy.Add("disabled", "");

            status_process = "Iniciando el proceso de generación de cache!";
            foreach (var item in builds)
            {
                if (item.Process)
                {
                    status_process = "Procesando " + item.ActioName + " ...";
                    current_buildItem = item;
                    //_ = InvokeAsync(StateHasChanged);
                    //bool res1 = await builderService.SendRequestOdooChunks(item);
                }
            }

            Console.WriteLine("Terminado el proceso de generación de cache!");
            toastService.ShowInfo("Terminado el proceso de generación de cache!");
            AdditionalAttributesBusy.Remove("disabled");

            status_process = "Done...";
        }

        private async Task Test(dynamic document)
        {
            var parameters = new ModalParameters();
            parameters.Add(nameof(DisplayMessageCustom.Message), "Desea aplicar esta configuración?");

            var options = new ModalOptions
            {
                UseCustomLayout = true,
                DisableBackgroundCancel = true
            };
            
            var messageForm = modalService.Show<DisplayMessageCustom>("Setup", parameters, options);

            var result = await messageForm.Result;

            if (result.Cancelled)
            {
                return;
            }

            toastService.ShowError("Contraseña incorrecta. Vuelve a intentarlo.");
            //ToastService toastService = new ToastService();
            //toastService.ShowError("Error");
        }

        private async Task CleanData()
        {
            var parameters = new ModalParameters();

            string Message = "Desea eliminar los datos para poder generar nuevos?";
            parameters.Add(nameof(DisplayMessageCustom.Message), Message);
            var options = new ModalOptions
            {
                UseCustomLayout = true,
                DisableBackgroundCancel = true
            };
            var messageForm = modalService.Show<DisplayMessageCustom>("Borrar datos", parameters, options);

            var result = await messageForm.Result;

            if (!result.Cancelled)
            {
                toastService.ShowInfo("Se canceló borrado de datos");
                //StateHasChanged();
            }
            else
            {
                return;
            }
        }

        private async Task TryData()
        {
            var appSetting = ConfigurationHelper.GetAppSettings();
            AppSession _appSession = new AppSession();
            _appSession.odooConnection.Host = appSetting.profile.Odoo.Host;

            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key,
                access_token = appSetting.profile.Odoo.access_token,
                uid = int.Parse(appSetting.profile.Odoo.uid),
                databasename = appSetting.profile.Odoo.Database,                
                username = appSetting.profile.Odoo.User,
                password = appSetting.profile.Odoo.Password,
            };


        }

        private void LoadCron()
        {
            
        }
    }
}
