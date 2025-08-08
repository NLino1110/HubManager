using DMOrdersUI.Controls;
using DMOrdersUI.Services.Database.Sqlite;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Origin;
using DMSA.Models.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrdersUI.Pages.Sys
{
    [Obsolete("Debe ser eliminado, movido a ServerPuller, y debe poder notificarse los cambios")]
    public partial class UpdateData
    {
        private async Task ProcProductProduct(ProgressBarAnimationBehaviorPage obj, string[] ListFiles)
        {
            var database = new ProductProductDb();
            //if (ListFiles.Length > 0)
            //{
            //    await database.Truncate();
            //}
            int fileIndex = 1;

            foreach (var fileNameJson in ListFiles)
            {
                Debug.WriteLine("Procesando archivo de cache:");
                Debug.WriteLine(fileNameJson);

                obj.SetTitle($"Proc. {Path.GetFileName(fileNameJson)} ({fileIndex}/{ListFiles.Length})");

                //string jsonFileItem = File.ReadAllText(Path.Combine(DeviceStorageJson, ZipFileName.Replace(".zip", ".json")));
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

        [Obsolete("Debe ser eliminado, se migro a ServerPuller")]
        public async Task<bool> OnlineSyncProductProduct(AppSession _appSession)
        {
            DateTime dateTimeIni = DateTime.Now;
            
            int year = 2025;
            int month = 1;
            int day = 1;

            ApiManager.HubProductProduct hubmanager = new ApiManager.HubProductProduct(_appSession);
            var resultCount = await hubmanager.GetCount();

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int limit = App.Session.db_limit_default;
            int countTotal = resultCount.result / limit;

            var database = new ProductProductDb();

            for (int indice = 0; indice <= countTotal; indice++)
            {
                var responseAll = await hubmanager.GetByCreateDate(limit, indice, year, month, day );

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

            TimeSpan span = (DateTime.Now - dateTimeIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }
    }
}
