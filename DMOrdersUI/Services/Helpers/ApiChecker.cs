using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrdersUI.Services.Helpers
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class ApiChecker
    {
        private readonly string apiUrl;
        private readonly TimeSpan timeout = TimeSpan.FromSeconds(5); // Tiempo de espera de 5 segundos

        public ApiChecker(string apiUrl)
        {
            this.apiUrl = apiUrl;
        }

        public async Task<bool> IsApiAvailable()
        {
            try
            {
                var handler = new HttpClientHandler();
                
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {                    
                    return true;
                };

                using (HttpClient client = new HttpClient(handler))
                {
                    // Configurar el tiempo de espera para la solicitud HTTP
                    client.Timeout = timeout;
                    
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Verificar si la respuesta es exitosa (código 2xx)
                    response.EnsureSuccessStatusCode();

                    return true; // La API está disponible
                }
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine(e.Message);
                // Manejar la excepción en caso de error de conexión o recurso no disponible
                return false;
            }
            catch (TaskCanceledException e)
            {
                Debug.WriteLine(e.Message);
                // Manejar la excepción en caso de que la tarea se cancele debido al tiempo de espera
                return false;
            }
        }
    }
}
