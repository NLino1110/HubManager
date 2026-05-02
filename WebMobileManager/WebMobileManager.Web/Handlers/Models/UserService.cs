using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components;

namespace WebMobileManager.Web.Handlers.Models
{
    public class UserService
    {
        [Inject]
        IConfiguration configuration { get; set; }
        public HttpClient _httpClient { get; }
        public AppSettings _appSettings { get; }
        private string BaseAddress { get; set; }

        public UserService(HttpClient httpClient, IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            //httpClient.BaseAddress = new Uri(_appSettings.profile.ApiBaseAddress);
            httpClient.BaseAddress = new Uri("https://example.com");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "BlazorServer");
            _httpClient = httpClient;
        }

        public void SetBaseAddress(string NewBaseAddress)
        {
            BaseAddress = NewBaseAddress;
        }

        public string GetBaseAddress()
        {
            return BaseAddress;
        }

        public async Task<User> LoginAsync(User user)
        {
            return await Task.FromResult(user);
        }

        public string GenerateToken(int userId)
        {   
            return "newToken";
        }

        private User UserConvert(string responseBodyUsr)
        {
            User returnedUser = new User();
            return returnedUser;
        }

        public async Task<User> CreateMaster()
        {
            User UserMaster = new User();            

            return UserMaster;
        }

        public async Task<User> LoginErpNextAsync(User user)
        {
            return await Task.FromResult(user);
        }

        public async Task<User> RegisterUserAsync(User user)
        {            

            return await Task.FromResult(user);
        }

        public async Task<User> GetUserByAccessTokenAsync(string accessToken)
        {
            User returnedUser = new User();
            return await Task.FromResult(returnedUser);
        }
    }
}
