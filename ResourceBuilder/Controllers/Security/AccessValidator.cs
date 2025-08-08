using Models.DMSA.Shared.Structs;
using System.Security.Cryptography;

namespace ResourceBuilder.Controllers.Security
{
    public static class AccessValidator
    {
        //private readonly IConfiguration _config;
        static public bool IsValidApiKey(string apiKey)
        {
            //var validApiKey = _config.GetValue<string>("ApiKey");
            var validApiKey = "9+7e3A7t4qI1Rl8XQ2GjKjs8KhZ9Y8p1MfbQvKlkmP4=";
            return apiKey == validApiKey;
        }

        public static string GenerateApiKey(int size = 32)
        {
            // Crear un array de bytes aleatorios
            var byteArray = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(byteArray);

                // Convertir a una cadena base64 para que sea fácil de usar
                return Convert.ToBase64String(byteArray);
            }
        }

        public static ApiResponseGlobal BuildUnauthorized()
        {
            return new ApiResponseGlobal()
            {
                count = 0,
                create_id = 0,
                message = "Invalid API Key!",
                info = "{'data':''}",
                object_name = "",
                responseCode = 401,
                success = false
            };
        }
    }
}
