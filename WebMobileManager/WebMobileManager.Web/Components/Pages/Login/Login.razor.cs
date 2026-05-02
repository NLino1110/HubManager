using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebMobileManager.Web.Handlers;
using WebMobileManager.Web.Handlers.Models;

namespace WebMobileManager.Web.Components.Pages.Login
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Usuario requerido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password requerido")]
        public string Password { get; set; }
    }


    public partial class Login : ComponentBase
    {
        [Inject] CustomAuthenticationStateProvider AuthProvider { get; set; }

        private LoginModel model = new();

        private bool isBusy = false;

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
                

        private async Task ValidateUser()
        {
            isBusy = true;            

            if (user.EmailAddress.ToLower() == "admin" && user.Password == "admin")
            {
                user.AccessToken = "token_fake";

                await ((CustomAuthenticationStateProvider)AuthenticationStateProvider)
                    .MarkUserAsAuthenticated(user);

                navigationManager.NavigateTo("/", true);
                return;
            }

            LoginMesssage = "Usuario o contraseña incorrectos";
            isBusy = false;
        }


        private async Task OnValidSubmit(EditContext context)
        {
            isBusy = true;

            if (model.Email.ToLower() == "admin" && model.Password == "admin")
            {
                var user = new User
                {
                    UserName = model.Email,
                    EmailAddress = model.Email,
                    AccessToken = "token_fake"
                };

                await localStorageService.SetItemAsync("token", user.AccessToken);
                await AuthProvider.MarkUserAsAuthenticated(user);
                navigationManager.NavigateTo("/", true);
                return;
            }

            LoginMesssage = "Usuario o contraseña incorrectos";
            isBusy = false;
        }
    }
}
