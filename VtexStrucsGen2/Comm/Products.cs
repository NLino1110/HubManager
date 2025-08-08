using RestSharp;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using VtexStrucs.Strucs;
using VtexStrucs.Tools;

namespace VtexStrucs.Comm
{
    public partial class Products : IVtexObject
    {
        public Products(vTex_Account accountParam)
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

        /// <summary>
        /// List
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Task<VtexStrucs.Strucs.ProductByFilter> getByFilter(string filter)
        {
            string EndPoint = $"https://{baseUrl}.com.br/api/catalog_system/pvt/products/GetProducts/?filter={filter}&page=1&pagesize=50";
            RSHelper rsh = new RSHelper(headers, EndPoint);
            return rsh.GetAsync<VtexStrucs.Strucs.ProductByFilter>(EndPoint);
        }

        public Task<VtexStrucsGen2.Strucs.Vtex.ResponseModels.ProductSearch[]> getProductSearch(string filter)
        {
            //filter example
            //skuId:4
            //productName:marisol
            string EndPoint = $"https://{baseUrl}.com.br/api/catalog_system/pub/products/search/?{filter}";
            RSHelper rsh = new RSHelper(headers, EndPoint);
            return rsh.GetAsync<VtexStrucsGen2.Strucs.Vtex.ResponseModels.ProductSearch[]>(EndPoint);
        }

        //public Task<string> getByFilter(string filter)
        //{
        //    string EndPoint = $"https://{baseUrl}.com.br/api/catalog_system/pvt/products/GetProducts/?filter={filter}&page=1&pagesize=50";
        //    RSHelper rsh = new RSHelper(headers, EndPoint);
        //    return rsh.GetAsync<string>(EndPoint);
        //}

        public Task<string> getSkuByRefId(string refId)
        {
            //c0b03bac-5dce-449a-b6f6-ff263ec2bd8b
            refId = refId.Replace(" ", "%20");

            string EndPoint = $"https://{baseUrl}.com.br/api/catalog_system/pvt/sku/stockkeepingunitidbyrefid/{refId}";

            RSHelper rsh = new RSHelper(headers, EndPoint);
            return rsh.GetAsync<string>(EndPoint);
        }

        public Task<VtexStrucs.Strucs.ProductFull> getBySkuIdFull(string skuId)
        {
            //c0b03bac-5dce-449a-b6f6-ff263ec2bd8b

            //INFO: Esta api extrae la información completa del producto
            //  pero tiene indexacion retrasada y los campos no coinciden con el
            //  formulario de ingreso en el UI de vtex
            string EndPoint = $"https://{baseUrl}.com.br/api/catalog_system/pvt/sku/stockkeepingunitbyid/{skuId}";
            RSHelper rsh = new RSHelper(headers, EndPoint);
            return rsh.GetAsync<VtexStrucs.Strucs.ProductFull>(EndPoint);
        }

        public Task<string> getBySkuIdFullString(string skuId)
        {
            //c0b03bac-5dce-449a-b6f6-ff263ec2bd8b

            //INFO: Esta api extrae la información completa del producto
            //  pero tiene indexacion retrasada y los campos no coinciden con el
            //  formulario de ingreso en el UI de vtex
            string EndPoint = $"https://{baseUrl}.com.br/api/catalog_system/pvt/sku/stockkeepingunitbyid/{skuId}";
            RSHelper rsh = new RSHelper(headers, EndPoint);
            return rsh.GetAsync<string>(EndPoint);
        }

        public async Task<vTexPrice> getPriceBySku(string skuId)
        {
            vTexPrice vtexprice = new vTexPrice();
            //c0b03bac-5dce-449a-b6f6-ff263ec2bd8b

            string EndPoint = $"https://api.vtex.com/{account.Account_Name}/pricing/prices/{skuId}";

            RSHelper rsh = new RSHelper(headers, EndPoint);
            try
            {
                var Response = await rsh.GetAsync<vTexPrice>(EndPoint);

                //if (Response.Contains("Price not found") || Response.ToLower().Contains("502 bad gateway"))
                //{
                //    Response = "";
                //}

                if (Response != null)
                {
                
                        //vtexprice = JsonSerializer.Deserialize<vTexPrice>(Response);
                        vtexprice = Response;
                
                }
            }
            catch (Exception e)
            {
                vtexprice.listPrice = 0;
                //Console.WriteLine("Error getPriceBySku: " + Response);
                Console.WriteLine("Error getPriceBySku: " + e.Message);
            }

            return vtexprice;
        }

        public async Task<vtexStock> getStockBySku(string skuId)
        {
            vtexStock vtexstock = new vtexStock();
            //c0b03bac-5dce-449a-b6f6-ff263ec2bd8b

            string EndPoint = $"https://logistics.vtexcommercestable.com.br/api/logistics/pvt/inventory/skus/{skuId}?an={account.Account_Name}&skuid={skuId}";

            RSHelper rsh = new RSHelper(headers, EndPoint);
            var Response = await rsh.GetAsync<vtexStock>(EndPoint);
            
            if (Response != null)
            {
                try
                {
                    //vtexstock = JsonSerializer.Deserialize<vtexStock>(Response.Result);
                    vtexstock = Response;
                    return vtexstock;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error getStockBySku: " + Response);
                    Console.WriteLine("Error getStockBySku: " + e.Message);
                }
            }

            //Bodega Bosque
            string cBod = "1_1";

            if (account.Account_Name == "tempodesignpan")
                cBod = "1cd771c";

            if (vtexstock.balance.Where(x => x.warehouseId.ToString() == cBod).Count() > 0)
            {
                //ent.stockvTex = vtexstock.balance.Where(x => x.warehouseId.Trim() == cBod).First().totalQuantity;
            }
            else
            {
                //WriteLog("No se encontró warehouse ", proc);
            }

            return vtexstock;

        }
        public string[] getDataVtexAllSkus()
        {
            string[] allSkus = null;

            try
            {
                string cUrlPrice = string.Format("https://{0}.vtexcommercestable.com.br/api/catalog_system/pvt/sku/stockkeepingunitids?page=1&pagesize=10000", account.Account_Name);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(cUrlPrice);
                httpWebRequest.ContentType = "application/json;charset=utf-8'";
                httpWebRequest.Headers.Add("x-vtex-api-appKey", account.vTexApiKey);
                httpWebRequest.Headers.Add("x-vtex-api-appToken", account.vTexApiToken);
                httpWebRequest.Method = "GET";
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //vTexPrice jsonres;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    result = result.Replace("[", "").Replace("]", "");
                    allSkus = result.Split(",");
                    //jsonres = JsonConvert.DeserializeObject<vTexPrice>(result);
                }

                //foreach(string itemSku in allSkus)
                //{
                //    Console.WriteLine("Sku:" + itemSku);
                //}                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en extracción de skus desde Vtex" + ex.Message);
            }

            return allSkus;
        }

        public Task<RestSharp.RestResponse> setNewStock(int prod_vtex_sku, int prod_stock_up)
        {
            string cBod = "1_1";
            //if (accountName == "tempodesignpan")
            //    cBod = "1cd771c";

            //var urlBase = string.Format("https://logistics.vtexcommercestable.com.br/api/logistics/pvt/inventory/skus/{1}/warehouses/{2}?an={0}", cuenta.Account_Name, item.skuId, cBod);
            //var client = new RestClient(urlBase);
            ////string content = new Strin("{\"listPrice\" : " + item.Pvp_Final.ToString() + ", \"costPrice\" : " + item.Pvp_Final.ToString() + ", \"markup\" : 0}", Encoding.UTF8, "application/json");
            vTexStockUpdater stock = new vTexStockUpdater
            {
                dateUtcOnBalanceSystem = null,
                quantity = prod_stock_up,
                unlimitedQuantity = false
            };

            //c0b03bac-5dce-449a-b6f6-ff263ec2bd8b
            string skuId = prod_vtex_sku.ToString();
            //https://logistics.vtexcommercestable.com.br/api/logistics/pvt/inventory/skus/{1}/warehouses/{2}?an={0}
            string EndPoint = $"https://logistics.vtexcommercestable.com.br/api/logistics/pvt/inventory/skus/{skuId}/warehouses/{cBod}?an={account.Account_Name}";

            RSHelper rsh = new RSHelper(headers, EndPoint);
            return rsh.ExecutePutAsync(EndPoint, stock);

            //RSHelper rsh = new RSHelper(headers);
            //string Response = rsh.PUT(EndPoint, JsonSerializer.Serialize(stock));
        }

        //public Task<VtexStrucs.Strucs.vTexStockUpdater> setNewStock(int prod_vtex_sku, int prod_stock_up)
        //{
        //    string cBod = "1_1";
        //    //if (accountName == "tempodesignpan")
        //    //    cBod = "1cd771c";

        //    //var urlBase = string.Format("https://logistics.vtexcommercestable.com.br/api/logistics/pvt/inventory/skus/{1}/warehouses/{2}?an={0}", cuenta.Account_Name, item.skuId, cBod);
        //    //var client = new RestClient(urlBase);
        //    ////string content = new Strin("{\"listPrice\" : " + item.Pvp_Final.ToString() + ", \"costPrice\" : " + item.Pvp_Final.ToString() + ", \"markup\" : 0}", Encoding.UTF8, "application/json");
        //    vTexStockUpdater stock = new vTexStockUpdater
        //    {
        //        dateUtcOnBalanceSystem = null,
        //        quantity = prod_stock_up,
        //        unlimitedQuantity = false
        //    };

        //    //c0b03bac-5dce-449a-b6f6-ff263ec2bd8b
        //    string skuId = prod_vtex_sku.ToString();
        //                        //https://logistics.vtexcommercestable.com.br/api/logistics/pvt/inventory/skus/{1}/warehouses/{2}?an={0}
        //    string EndPoint = $"https://logistics.vtexcommercestable.com.br/api/logistics/pvt/inventory/skus/{skuId}/warehouses/{cBod}?an={account.cac_name}";

        //    RSHelper rsh = new RSHelper(headers, EndPoint);
        //    return rsh.PutAsync<vTexStockUpdater>(EndPoint, stock);

        //    //RSHelper rsh = new RSHelper(headers);
        //    //string Response = rsh.PUT(EndPoint, JsonSerializer.Serialize(stock));
        //}

        public Task<VtexStrucs.Strucs.ProductFull> Update(ProductFull product)
        {
            string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/product/{product.ProductId}";

            RSHelper rsh = new RSHelper(headers, EndPoint);
            
            return rsh.PutAsync<ProductFull>(EndPoint, product);
        }

        //public string PutPrice(int prod_vtex_sku, decimal prod_pvp_up)
        //{           

        //}

        //public string setNewPrice(int prod_vtex_sku, decimal listPrice, decimal costPrice, decimal basePrice, decimal markup)
        //{
        //    vTexPriceforChange precio = new vTexPriceforChange
        //    {
        //        listPrice = listPrice,
        //        costPrice = costPrice,                
        //        markup = markup//,
        //        //basePrice = basePrice
        //    };

        //    //c0b03bac-5dce-449a-b6f6-ff263ec2bd8b
        //    string skuId = prod_vtex_sku.ToString();

        //    string EndPoint = $"https://api.vtex.com/{account.cac_name}/pricing/prices/{skuId}";

        //    RSHelper rsh = new RSHelper(headers);
        //    string Response = rsh.PUT(EndPoint, JsonSerializer.Serialize(precio));

        //    if (Response != "")
        //    {

        //    }

        //    return Response;
        //}

        public async Task<VtexStrucs.Strucs.Product> getByProductId(int productId)
        {
            //c0b03bac-5dce-449a-b6f6-ff263ec2bd8b

            //Esta API hace extra la información de la data del producto en tiempo real
            //  TODO: se harán cambios para utilizar esta API al momento de llenar los datos de la db
            string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/product/{productId.ToString()}";

            RSHelper rsh = new RSHelper(headers, EndPoint);

            //RestResponse
            var response = await rsh.ExecuteGetAsync(EndPoint);
            
            VtexStrucs.Strucs.Product vtexprod = new Product();

            vtexprod = JsonSerializer.Deserialize<VtexStrucs.Strucs.Product>(response.Content);

            return vtexprod;
        }

        //public vTexPrice getPriceBySku(string skuId)
        //{
        //    vTexPrice vtexprice = new vTexPrice();
        //    //c0b03bac-5dce-449a-b6f6-ff263ec2bd8b

        //    string EndPoint = $"https://api.vtex.com/{account.cac_name}/pricing/prices/{skuId}";

        //    RSHelper rsh = new RSHelper(headers);
        //    string Response = rsh.GET(EndPoint);

        //    if (Response.Contains("Price not found") || Response.ToLower().Contains("502 bad gateway"))
        //    {
        //        Response = "";
        //    }

        //    if (Response != "")
        //    {
        //        try
        //        {
        //            vtexprice = JsonSerializer.Deserialize<vTexPrice>(Response);
        //        }
        //        catch(Exception e)
        //        {
        //            Console.WriteLine("Error getPriceBySku: " + Response);
        //            Console.WriteLine("Error getPriceBySku: " + e.Message);
        //        }
        //    }

        //    return vtexprice;
        //}

        //public vtexStock getStockBySku(string skuId)
        //{
        //    vtexStock vtexstock = new vtexStock();
        //    //c0b03bac-5dce-449a-b6f6-ff263ec2bd8b

        //    string EndPoint = $"https://logistics.vtexcommercestable.com.br/api/logistics/pvt/inventory/skus/{skuId}?an={account.cac_name}&skuid={skuId}";

        //    RSHelper rsh = new RSHelper(headers);
        //    string Response = rsh.GET(EndPoint);

        //    if (Response != "")
        //    {
        //        try
        //        {
        //            vtexstock = JsonSerializer.Deserialize<vtexStock>(Response);
        //            return vtexstock;
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine("Error getStockBySku: " + Response);
        //            Console.WriteLine("Error getStockBySku: " + e.Message);
        //        }
        //    }            

        //    //Bodega Bosque
        //    string cBod = "1_1";

        //    if (account.cac_name == "tempodesignpan")
        //        cBod = "1cd771c";

        //    if (vtexstock.balance.Where(x => x.warehouseId.ToString() == cBod).Count() > 0)
        //    {
        //        //ent.stockvTex = vtexstock.balance.Where(x => x.warehouseId.Trim() == cBod).First().totalQuantity;
        //    }
        //    else
        //    {
        //        //WriteLog("No se encontró warehouse ", proc);
        //    }

        //    return vtexstock;

        //    //string cBod = "1_1";
        //    //string pato = "";
        //    //if (cuenta.Account_Name == "tempodesignpan")
        //    //    cBod = "1cd771c";

        //    //string cUrlPrice = string.Format("https://logistics.vtexcommercestable.com.br/api/logistics/pvt/inventory/skus/{1}?an={0}&skuid={1}", cuenta.Account_Name, ent.skuId);
        //    //var httpWebRequest = (HttpWebRequest)WebRequest.Create(cUrlPrice);
        //    //httpWebRequest.ContentType = "application/json;charset=utf-8'";
        //    //httpWebRequest.Headers.Add("x-vtex-api-appKey", cuenta.vTexApiKey);
        //    //httpWebRequest.Headers.Add("x-vtex-api-appToken", cuenta.vTexApiSecret);
        //    //httpWebRequest.Method = "GET";
        //    //if (ent.skuId == 772)
        //    //    pato = "entro";
        //    //var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        //    //vtexStock jsonres;
        //    //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        //    //{
        //    //    var result = streamReader.ReadToEnd();
        //    //    jsonres = JsonConvert.DeserializeObject<vtexStock>(result);
        //    //}
        //    //if (jsonres.balance.Where(x => x.warehouseId.ToString() == cBod).Count() > 0)
        //    //{
        //    //    ent.stockvTex = jsonres.balance.Where(x => x.warehouseId.Trim() == cBod).First().totalQuantity;
        //    //}
        //    //else
        //    //{
        //    //    WriteLog("No se encontró warehouse ", proc);
        //    //}
        //    //return null;
        //}



        //public string PutStock(Product item, vTexStockUpdater stock)
        //{
        //    //c0b03bac-5dce-449a-b6f6-ff263ec2bd8b
        //    string skuId = item.SkuId.ToString();

        //    string EndPoint = $"https://logistics.vtexcommercestable.com.br/api/logistics/pvt/inventory/skus/{skuId}?an={account.cac_name}&skuid={skuId}";

        //    RSHelper rsh = new RSHelper(headers);
        //    string Response = rsh.PUT(EndPoint, JsonSerializer.Serialize(stock));

        //    if (Response != "")
        //    {

        //    }

        //    return Response;
        //}

        //public string SendChange(Product item)
        //{
        //    //c0b03bac-5dce-449a-b6f6-ff263ec2bd8b
        //    string productId = item.SkuId.ToString();
        //    string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/product/{productId}";

        //    RSHelper rsh = new RSHelper(headers);
        //    string Response = rsh.PUT(EndPoint, new {            
        //        Name = item.Name,
        //        Description = item.Description,
        //        CategoryId = item.CategoryId,
        //        BrandId = item.BrandId,
        //        KeyWords = item.KeyWords,
        //        ReleaseDate = item.ReleaseDate,
        //        Title = item.Title,
        //        MetaTagDescription = item.MetaTagDescription,
        //        RefId = item.RefId
        //    });

        //    if (Response != "")
        //    {

        //    }

        //    return Response;
        //}

    }
}
