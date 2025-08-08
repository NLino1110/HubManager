using Newtonsoft.Json;
using System.Data;
using VtexStrucs;

namespace ResourceBuilder.Services.Sync
{
    public partial class TaskManager
    {
        public int completados = 0;
        public int multiplicador = 50;
        public decimal numproc;
        public List<Siscom2vTexMatchProduct> Lista = new List<Siscom2vTexMatchProduct>();
        string[] allSkus;

        public async Task<string[]> getDataVtexAllSkus(vTex_Account mAccount)
        {
            string[] allSkus = null;

            try
            {
                string url = $"https://{mAccount.Account_Name}.vtexcommercestable.com.br/api/catalog_system/pvt/sku/stockkeepingunitids?page=1&pagesize=10000";

                // Crear instancia de HttpClient
                using (var httpClient = new HttpClient())
                {
                    // Configurar encabezados de autenticación
                    httpClient.DefaultRequestHeaders.Add("x-vtex-api-appKey", mAccount.vTexApiKey);
                    httpClient.DefaultRequestHeaders.Add("x-vtex-api-appToken", mAccount.vTexApiToken);

                    // Realizar la solicitud HTTP
                    HttpResponseMessage response = await httpClient.GetAsync(url);

                    // Verificar si la respuesta es exitosa
                    response.EnsureSuccessStatusCode();

                    // Leer y procesar la respuesta
                    string result = await response.Content.ReadAsStringAsync();
                    //result = result.Replace("[", "").Replace("]", "");
                    //allSkus = result.Split(",");

                    allSkus = JsonConvert.DeserializeObject<string[]>(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en extracción de skus desde Vtex: " + ex.Message);
            }

            return allSkus;
        }

        public async Task<bool> QueueAnalyzer(vTex_Account cuenta)
        {            
            Console.WriteLine("Iniciando Procesar");

            int count = 0;
            completados = 0;
            
            allSkus = await getDataVtexAllSkus(cuenta);

            if (allSkus == null)
            {
                //LogManager.Log().LogError("Procesar - Error al obtener listado de SKUs (terminado antes de tiempo)");
                Console.WriteLine("Procesar - Error al obtener listado de SKUs (terminado antes de tiempo)");
                return false;
            }

            if (allSkus.Length > multiplicador)
                numproc = Math.Ceiling((decimal)allSkus.Length / multiplicador);
            else
                numproc = allSkus.Length;

            List<Task> taskArray = new List<Task>();
            for (int i = 0; i < numproc; i++)
            {
                bool lastGroup = false;
                int proc = i;

                if (!((i + 1) < numproc))
                    lastGroup = true;

                //taskArray.Add(await Task.Factory.StartNew(async () => await ProcessGroup(proc, cuenta, lastGroup)));
                taskArray.Add(Task.Factory.StartNew(() => processorService.ProcessGroup(proc, cuenta, lastGroup, allSkus)));
                //break;
            }

            Task.WaitAll(taskArray.ToArray());

            Console.WriteLine("Terminado Procesar");
            return true;         
        }
    }
}
