using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using ResourceBuilder.Handlers.Models;

namespace ResourceBuilder.Handlers
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        public ILocalStorageService _localStorageService { get; }
        public IUserService _userService { get; set; }
        private readonly HttpClient _httpClient;

        //private User m_loggedUser { get; set; }

        public CustomAuthenticationStateProvider(ILocalStorageService localStorageService,
            IUserService userService,
            HttpClient httpClient)
        {
            //throw new Exception("CustomAuthenticationStateProviderException");
            _localStorageService = localStorageService;
            _userService = userService;
            _httpClient = httpClient;
        }

        //public User GetLoggedUser ()
        //{
        //    return m_loggedUser;
        //}

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var accessToken = await _localStorageService.GetItemAsync<string>("accessToken");

            ClaimsIdentity identity;

            if (accessToken != null && accessToken != string.Empty)
            {
                //User user = await _userService.GetUserByAccessTokenAsync(accessToken);
                //m_loggedUser = user;
                User user = new User
                {
                    EmailAddress = "rchonillo@macronegocios.ec",
                    Password = "admin",
                    UserName = "rchonillo",
                };

                identity = GetClaimsIdentity(user);
            }
            else
            {
                identity = new ClaimsIdentity();
            }

            var claimsPrincipal = new ClaimsPrincipal(identity);

            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
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
            //TODO: Send Company ID and Name
            var claimsIdentity = new ClaimsIdentity();

            if (user != null && user.EmailAddress != null)
            {
                claimsIdentity = new ClaimsIdentity(new[]
                                {
                                    new Claim(ClaimTypes.Name, user.UserName),
                                    //new Claim(ClaimTypes.Role, user.Role.RoleDesc),
                                    //new Claim("cmp_tax_id", user.Cmp_Tax_id),
                                    //new Claim("cmp_name", user.Cmp_Name),
                                    //new Claim("cmp_database_name", user.Cmp_Database_name),
                                    //new Claim("bus_id", user.Bus_id.ToString()),
                                    //new Claim("site_master", user.Cmp_site_master.ToString()),
                                    //new Claim("erpdomain", user.erpdomain),
                                    //new Claim("erpport", user.erpport),
                                    new Claim("IsUserEmployedBefore1990", IsUserEmployedBefore1990(user))

                                }, "apiauth_type");
            }

            return claimsIdentity;
        }

        private string IsUserEmployedBefore1990(User user)
        {
            if (user.HireDate.Year < 1990)
                return "true";
            else
                return "false";
        }
    }
}
