using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Dynamic;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Models.DMSA.Shared.Security;

namespace ResourceBuilder.Handlers.Models
{
    public class UserService : IUserService
    {
        [Inject]
        IConfiguration configuration { get; set; }
        public HttpClient _httpClient { get; }
        public AppSettings _appSettings { get; }
        private string BaseAddress { get; set; }

        public UserService(HttpClient httpClient, IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;

            //TODO: Asignación simbólica
            //   esta asignacion de la variable ApiBaseAddress solo es simbólica, ya que el BaseAdress
            //   es dinámico según los datos del usuario/password
            //   pero el ApiBaseAddress se usará para comunicación entre aplicaciones
            //   por ejemplo para que el BeebTechApiService comunique mas o menos como modo PUSH
            //   a la aplicación actual que el servicio realizó alguna acción
            httpClient.BaseAddress = new Uri(_appSettings.profile.ApiBaseAddress);
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

        //public async Task<User> RefreshTokenAsync(RefreshRequest refreshRequest)
        //{
        //    string serializedUser = JsonConvert.SerializeObject(refreshRequest);

        //    var requestMessage = new HttpRequestMessage(HttpMethod.Post, "Users/RefreshToken");
        //    requestMessage.Content = new StringContent(serializedUser);

        //    requestMessage.Content.Headers.ContentType
        //        = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        //    var response = await _httpClient.SendAsync(requestMessage);

        //    var responseStatusCode = response.StatusCode;
        //    var responseBody = await response.Content.ReadAsStringAsync();

        //    var returnedUser = JsonConvert.DeserializeObject<User>(responseBody);

        //    return await Task.FromResult(returnedUser);
        //}

        public async Task<User> GetUserByAccessTokenAsync(string accessToken)
        {
            User returnedUser = new User();
            return await Task.FromResult(returnedUser);
        }
    }
}
