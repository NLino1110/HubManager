using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using WebMobileManager.Web.Handlers.Models;
using WebMobileManager.Web.Handlers;

namespace WebMobileManager.Web.Components.Pages
{
    public partial class Login : ComponentBase
    {
        private User user { get; set; }
        public string LoginMesssage { get; set; }
        ClaimsPrincipal claimsPrincipal;

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Inject]
        NavigationManager navigationManager { get; set; }
        
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
        }

        private async Task<bool> ValidateUser()
        {
            AdditionalAttributesBusy.Add("disabled", "");
            //Debug.WriteLine(user.UserName);
            if (user.EmailAddress.ToLower() == "admin" && user.Password=="admin")
            {
                user.AccessToken = "lbu2vdmoYIg1l41U72ks06slZTlZg9hM3mbKkWoQM0kXtLN6xQfK6kv3dyWxo5q4";

                claimsPrincipal = (await authenticationStateTask).User;
                //await ((CustomAuthenticationStateProvider) AuthenticationStateProvider).MarkUserAsAuthenticated(user);

                //await ((CustomAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsAuthenticated(returnedUserMaster);

                navigationManager.NavigateTo("/index");
                return true;
            }

            AdditionalAttributesBusy.Remove("disabled");
            //toastService.ShowError("Usuario/Password incorrecto.");
            return false;
        }
    }
}
