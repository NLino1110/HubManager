using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using VtexStrucs.Comm;
using VtexStrucs.Tools;

namespace VtexStrucs.Comm
{
    public class Supplier : IVtexObject
    {
        public Supplier(vTex_Account accountParam)
        {
            SetAccount(accountParam);
        }

        //accountName   bosque
        //environment   vtexcommercestable
        //private string accountName   = "bosque";
        //private string environment = "vtexcommercestable";
        //private string baseUrl = "";

        //private Dictionary<string, string> headers =
        //   new Dictionary<string, string>();

        //public Products()
        //{
        //    baseUrl = $"{accountName}.{environment}";
        //    headers.Add("Content-Type", "application/json");
        //    headers.Add("Accept", "application/json");
        //    headers.Add("X-VTEX-API-AppToken", "VACQRFWMQRRQUJULDWJKQRUDRYVDTOZQBTINYNQGLNERMVUOXLWLGYYEFZZSJTTBRBOAAGMPGDZCMCQVEHGHSOJDUEHZOXWRUCZLGTNQYGECZWRCIDOCNMJDRLFWYNAI");
        //    headers.Add("X-VTEX-API-AppKey", "vtexappkey-bosque-GQKMQB");
        //}

        //private string[] getDataVtexAllSkus()
        //{
        //    string[] allSkus = null;

        //    try
        //    {
        //        string cUrlPrice = string.Format("https://{0}.vtexcommercestable.com.br/api/catalog_system/pvt/sku/stockkeepingunitids?page=1&pagesize=10000", cuenta.Account_Name);
        //        var httpWebRequest = (HttpWebRequest)WebRequest.Create(cUrlPrice);
        //        httpWebRequest.ContentType = "application/json;charset=utf-8'";
        //        httpWebRequest.Headers.Add("x-vtex-api-appKey", cuenta.vTexApiKey);
        //        httpWebRequest.Headers.Add("x-vtex-api-appToken", cuenta.vTexApiSecret);
        //        httpWebRequest.Method = "GET";
        //        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        //        //vTexPrice jsonres;


        //        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        //        {
        //            var result = streamReader.ReadToEnd();
        //            result = result.Replace("[", "").Replace("]", "");
        //            allSkus = result.Split(",");
        //            //jsonres = JsonConvert.DeserializeObject<vTexPrice>(result);
        //        }

        //        //foreach(string itemSku in allSkus)
        //        //{
        //        //    Console.WriteLine("Sku:" + itemSku);
        //        //}                
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error en extracción de skus desde Vtex" + ex.Message);
        //    }

        //    return allSkus;
        //}

        /// <summary>
        /// List
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<VtexStrucs.Strucs.Supplier> GetAsync<T>(string supplierId)
        {
            try
            {
                string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/supplier/{supplierId}";

                RSHelper rsh = new RSHelper(headers, EndPoint);

                return await rsh.GetAsync<VtexStrucs.Strucs.Supplier>(EndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<VtexStrucs.Strucs.Supplier> GetAsync2<T>(string supplierId)
        {
            try
            {
                string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/supplier/{supplierId}";
                Console.WriteLine(EndPoint);

                RSHelper rsh = new RSHelper(headers, EndPoint);
                
                return await rsh.GetAsync<VtexStrucs.Strucs.Supplier>(EndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetAsync2:" + ex.Message);
                Console.WriteLine("Error GetAsync2:" + ex.StackTrace);
                return null;
            }
        }
    }
}
