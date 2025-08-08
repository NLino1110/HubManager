using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using System.Dynamic;

using Blazored.Toast.Services;
using Microsoft.Extensions.Configuration;
using DataSourceManager.Tools;
using System.Diagnostics;
using ResourceBuilder.Handlers.Models;
using ResourceBuilder.Handlers;

namespace ResourceBuilder.Pages.Login
{
    public partial class Login : ComponentBase
    {
        private User user;
        public string LoginMesssage { get; set; }
        ClaimsPrincipal claimsPrincipal;

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        //[CascadingParameter]
        //public string bus_id { get; set; }

        [Inject]
        IToastService toastService { get; set; }
        [Inject]
        NavigationManager navigationManager { get; set; }
        //[Inject]
        //IUserService userService { get; set; }
        //[Inject]
        //IConfiguration configuration { get; set; }

        //[Inject]
        //ConfigurationService configurationService { get; set; }

        public Dictionary<string, object> AdditionalAttributesBusy =
            new Dictionary<string, object>();

        string LoginStatusText { get; set; }
        protected async override Task OnInitializedAsync()
        {
            user = new User();
            user.EmailAddress = "";
            user.Password = "";
            user.UserName = "";

            LoginStatusText = "Iniciar sesión";
            //Console.WriteLine(Localizer["Password"].Value);

            //LoginStatusText = Localizer["Login"];

            //            user = new User();

            //            claimsPrincipal = (await authenticationStateTask).User;

            //            if (claimsPrincipal.Identity.IsAuthenticated)
            //            {
            //                navigationManager.NavigateTo("/index", true);
            //            }
            //            else
            //            {
            //#if (DEBUG)
            //                //user.Cmp_Tax_id = "0915885404001";
            //                user.Cmp_Tax_id = "0919826958001";
            //                user.EmailAddress = "Administrator";
            //                user.Password = "r00tXorg060606";
            //#endif
            //            }
        }

        private async Task<bool> ValidateUser()
        {
            AdditionalAttributesBusy.Add("disabled", "");
            //Debug.WriteLine(user.UserName);
            if (user.EmailAddress.ToLower() == "admin" && user.Password=="admin")
            {
                user.AccessToken = "lbu2vdmoYIg1l41U72ks06slZTlZg9hM3mbKkWoQM0kXtLN6xQfK6kv3dyWxo5q4";

                claimsPrincipal = (await authenticationStateTask).User;
                await ((CustomAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsAuthenticated(user);

                //await ((CustomAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsAuthenticated(returnedUserMaster);

                navigationManager.NavigateTo("/index");
                return true;
            }

            AdditionalAttributesBusy.Remove("disabled");
            toastService.ShowError("Usuario/Password incorrecto.");
            return false;
        }

        ////private async Task<bool> ValidateUser()
        ////{
        ////    if (user.Cmp_Tax_id == "0" && user.EmailAddress.ToLower() == "administrator" && user.Password == "r00tXorg1313")
        ////    {
        ////        var returnedUserMaster = await userService.CreateMaster();

        ////        await ((CustomAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsAuthenticated(returnedUserMaster);
        ////        navigationManager.NavigateTo("/index", true);

        ////        return await Task.FromResult(true);
        ////    }

        ////    AdditionalAttributesBusy.Add("disabled", "");
        ////    LoginStatusText = "Iniciando...";
        ////    //assume that user is valid
        ////    //call an API

        ////    //Temporary code

        ////    //user = new User();
        ////    //user.EmailAddress = "philip.cramer@gmail.com";
        ////    //user.Password = "philip.cramer";
        ////    //user.HireDate = new DateTime(2010, 1, 1);
        ////    //user.FirstName = "Philip";
        ////    //user.LastName = "Cramer";
        ////    //user.UserId = 1;

        ////    //Role role = new Role();
        ////    //role.RoleId = 1;
        ////    //role.RoleDesc = "Publisher";
        ////    //user.Role = role;        

        ////    //await ((CustomAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsAuthenticated(user);
        ////    //NavigationManager.NavigateTo("/index");
        ////    //Temporary code

        ////    //var returnedUser = await userService.LoginAsync(user);

        ////    //if (returnedUser.EmailAddress != null)
        ////    //{
        ////    //    await ((CustomAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsAuthenticated(returnedUser);
        ////    //    NavigationManager.NavigateTo("/index");
        ////    //}
        ////    //else
        ////    //{
        ////    //    LoginMesssage = "Invalid username or password";
        ////    //}

        ////    //Search Company Registered By Tax Id
        ////    Company<object> company = new Company<object>();
        ////    dynamic dataParams = new ExpandoObject();
        ////    dataParams.tax_id = user.Cmp_Tax_id;
        ////    CoreResponse response = await company.GetByTaxId(dataParams);

        ////    //dynamic companies = response.data;

        ////    List<dynamic> items = (List<dynamic>)response.data;

        ////    int bus_id = 0;
        ////    string tax_id = "";
        ////    string siteport = "";
        ////    string database_name = "";
        ////    string cmp_name = "";
        ////    int site_master = 0;

        ////    foreach (var itemObj in items)
        ////    {
        ////        if (itemObj.siteport is not DBNull)
        ////        {
        ////            //SitePort for client web site
        ////            bus_id = itemObj.bus_id;
        ////            tax_id = itemObj.tax_id;
        ////            siteport = itemObj.siteport.ToString();
        ////            database_name = itemObj.database_name;
        ////            cmp_name = itemObj.name;
        ////            site_master = itemObj.site_master;
        ////        }
        ////        //itemObj.database_name
        ////    }

        ////    if (bus_id == 0)
        ////    {
        ////        LoginMesssage = "Datos incorrectos. Ruc no encontrado.";
        ////        toastService.ShowError("Datos incorrectos. Ruc no encontrado.");
        ////        AdditionalAttributesBusy.Remove("disabled");
        ////        LoginStatusText = Localizer["Login"];
        ////        return await Task.FromResult(false);
        ////    }

        ////    user.Bus_id = bus_id;
        ////    user.Cmp_Tax_id = tax_id;
        ////    user.Cmp_Database_name = database_name;
        ////    user.Cmp_Name = cmp_name;
        ////    user.Cmp_site_master = site_master;
        ////    //For Now We are figuring the server is localhost
        ////    // each client for ErpNext will take a port number 
        ////    //TODO: Hacer dinámico el ip/url del API
        ////    //http://67.225.226.30/

        ////    //Automatización temporal - 
        ////    string ApiServerUrl = "";
        ////    //ApiServerUrl = configuration["DataServers:Import:Server"].ToString();
        ////    Debug.WriteLine(configurationService.GetAppSettings().UseProfile);
        ////    ApiServerUrl = configurationService.GetAppSettings().profile.ErpNext.Server;//configuration["AppSettings:ErpNextServer"].ToString();
        ////    //userService.SetBaseAddress("http://localhost:" + siteport);
        ////    //userService.SetBaseAddress("http://67.225.226.30:" + siteport);
        ////    userService.SetBaseAddress("http://" + ApiServerUrl + ":" + siteport);
        ////    user.erpdomain = ApiServerUrl;
        ////    user.erpport = siteport;

        ////    var returnedUser = await userService.LoginErpNextAsync(user);

        ////    if (returnedUser != null)
        ////    {
        ////        returnedUser.Cmp_Tax_id = tax_id;
        ////        returnedUser.Cmp_Database_name = database_name;
        ////        returnedUser.Cmp_Name = cmp_name;
        ////        returnedUser.Cmp_site_master = site_master;
        ////        returnedUser.erpdomain = ApiServerUrl;
        ////        returnedUser.erpport = siteport;
        ////        //Role role = new Role();
        ////        //role.RoleId = 1;
        ////        //role.RoleDesc = "Publisher";
        ////        //user.Role = role;        

        ////        await ((CustomAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsAuthenticated(returnedUser);
        ////        navigationManager.NavigateTo("/index", true);
        ////    }
        ////    else
        ////    {
        ////        LoginMesssage = "Contraseña incorrecta. Vuelve a intentarlo.";
        ////        toastService.ShowError("Contraseña incorrecta. Vuelve a intentarlo.");
        ////    }

        ////    AdditionalAttributesBusy.Remove("disabled");
        ////    LoginStatusText = Localizer["Login"];
        ////    return await Task.FromResult(true);
        ////}

    }
}
