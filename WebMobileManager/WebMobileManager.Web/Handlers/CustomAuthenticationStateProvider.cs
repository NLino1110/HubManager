using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Security.Principal;
using WebMobileManager.Web.Handlers.Models;
using WebMobileManager.Web.Services;

namespace WebMobileManager.Web.Handlers
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        public ILocalStorageService _localStorageService { get; }
        public UserService _userService { get; set; }
        private readonly HttpClient _httpClient;
        
        public CustomAuthenticationStateProvider(ILocalStorageService localStorageService,
            UserService userService,
            HttpClient httpClient)
        {            
            _localStorageService = localStorageService;
            _userService = userService;
            _httpClient = httpClient;
        }


        //public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        //{
        //    try
        //    {
        //        var accessToken = await _localStorageService.GetItemAsync("accessToken");

        //        if (!string.IsNullOrEmpty(accessToken))
        //        {
        //            //var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Usuario")}, "apiauth");
        //            User user = new User
        //            {
        //                EmailAddress = "rchonillo@macronegocios.ec",
        //                Password = "admin",
        //                UserName = "rchonillo",
        //            };

        //            var identity = GetClaimsIdentity(user);

        //            return new AuthenticationState(new ClaimsPrincipal(identity));
        //        }
        //    }
        //    catch
        //    {

        //    }

        //    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        //}

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var accessToken = await _localStorageService.GetItemAsync("accessToken");

                if (string.IsNullOrEmpty(accessToken))
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var identity = GetClaimsIdentity(new User { UserName = "rchonillo", EmailAddress = "..." });
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }


        public async Task MarkUserAsAuthenticated(User user)
        {
            if (user.AccessToken == null || user.AccessToken == "")
            {
                return;
            }

            await _localStorageService.SetItemAsync("accessToken", user.AccessToken);
            await _localStorageService.SetItemAsync("refreshToken", user.RefreshToken);

            var identity = GetClaimsIdentity(user);

            var claimsPrincipal = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _localStorageService.RemoveItemAsync("refreshToken");
            await _localStorageService.RemoveItemAsync("accessToken");

            var identity = new ClaimsIdentity();

            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        private ClaimsIdentity GetClaimsIdentity(User user)
        {            
            var claimsIdentity = new ClaimsIdentity();

            if (user != null && user.EmailAddress != null)
            {
                claimsIdentity = new ClaimsIdentity(new[]
                                {
                                    new Claim(ClaimTypes.Name, user.UserName),                                    
                                    new Claim("IsUserEmployedBefore1990", IsUserEmployedBefore1990(user))

                                }, "apiauth_type");
            }

            return claimsIdentity;
        }

        private string IsUserEmployedBefore1990(User user)
        {
            return (user.HireDate?.Year < 1990) ? "true" : "false";
        }
    }
}
