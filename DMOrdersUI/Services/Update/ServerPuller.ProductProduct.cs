using ApiManager;
using DMOrdersUI.Controls;
using DMOrdersUI.Services.Database.Sqlite;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Origin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrdersUI.Services.Update
{
    public partial class ServerPuller
    {
        private async Task ProcProductProduct(ProgressBarAnimationBehaviorPage obj, string[] ListFiles)
        {
            var database = new ProductProductDb();
            
            int fileIndex = 1;

            foreach (var fileNameJson in ListFiles)
            {
                Debug.WriteLine("Procesando archivo de cache:");
                Debug.WriteLine(fileNameJson);

                obj.SetTitle($"Proc. {Path.GetFileName(fileNameJson)} ({fileIndex}/{ListFiles.Length})");

                string jsonFileItem = File.ReadAllText(fileNameJson);
                var listObjects = JsonConvert.DeserializeObject<ProductProductOrigin>(jsonFileItem);

                int totalItems = listObjects.product_product.Length;
                int curIndex = 1;
                double percentProcess = 0;

                try
                {
                    List<product_product> final_list = new List<product_product>();

                    final_list = listObjects.product_product.ToList();

                    await database.InsertBatchAsync(final_list.ToArray());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine("Error: provocado por " + fileNameJson);
                    Debug.WriteLine("Error: " + listObjects);
                }

                fileIndex++;
            }
        }

        public async Task<bool> OnlineSyncProductProduct()
        {
            var stopwatch = Stopwatch.StartNew();

            DateTime dateIni = appSession.sync_date_since;
            DateTime dateEnd = DateTime.Now;

            ApiManager.HubProductProduct hubmanager = new ApiManager.HubProductProduct(appSession);
            var resultCount = await hubmanager.GetCount();

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / 300;

            var database = new ProductProductDb();

            for (int indice = 0; indice <= countTotal; indice++)
            {
                var responseAll = await hubmanager.GetByCreateDateRange(limit, indice, dateIni, dateEnd);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                Console.WriteLine("Página:" + indice);

                //TODO: Se fuerza la salida para que no se quede ciclado en caso de que haya
                // problemas de conexion con el servidor
                // el objetivo es que el servidor no se sobrecargue

                if (indice >= 600)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }
    }
}
