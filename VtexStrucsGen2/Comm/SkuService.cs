using RestSharp;
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
    public class SkuService : IVtexObject
    {
        public SkuService(vTex_Account accountParam)
        {
            SetAccount(accountParam);
        }

        /// <summary>
        /// List
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        
        public async Task<VtexStrucsGen2.Strucs.Vtex.SkuService> GetAsync<T>(string IdSkuServicoValor)
        {
            try
            {
                string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/skuservice/{IdSkuServicoValor}";
                Console.WriteLine(EndPoint);

                RSHelper rsh = new RSHelper(headers, EndPoint);
                
                return await rsh.GetAsync<VtexStrucsGen2.Strucs.Vtex.SkuService>(EndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetAsync2:" + ex.Message);
                Console.WriteLine("Error GetAsync2:" + ex.StackTrace);
                return null;
            }
        }

        public async Task<RestResponse> PostAsync(VtexStrucsGen2.Strucs.Vtex.SkuServicePost paramObject)
        {
            try
            {
                string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/skuservice";
                Console.WriteLine(EndPoint);

                RSHelper rsh = new RSHelper(headers, EndPoint);

                return await rsh.ExecutePostAsync(EndPoint, paramObject);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetAsync2:" + ex.Message);
                Console.WriteLine("Error GetAsync2:" + ex.StackTrace);
                return null;
            }
        }

        public async Task<VtexStrucsGen2.Strucs.Vtex.SkuService> PutAsync<T>(VtexStrucsGen2.Strucs.Vtex.SkuService paramObject)
        {
            try
            {
                string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/skuservice/{paramObject.Id}";
                Console.WriteLine(EndPoint);

                RSHelper rsh = new RSHelper(headers, EndPoint);

                return await rsh.PutAsync<VtexStrucsGen2.Strucs.Vtex.SkuService>(EndPoint, paramObject);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetAsync2:" + ex.Message);
                Console.WriteLine("Error GetAsync2:" + ex.StackTrace);
                return null;
            }
        }

        public async Task<RestResponse> ExecutePutAsync<T>(VtexStrucsGen2.Strucs.Vtex.SkuService paramObject)
        {
            try
            {
                string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/skuservice/{paramObject.Id}";
                Console.WriteLine(EndPoint);

                RSHelper rsh = new RSHelper(headers, EndPoint);

                return await rsh.ExecutePutAsync(EndPoint, paramObject);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetAsync2:" + ex.Message);
                Console.WriteLine("Error GetAsync2:" + ex.StackTrace);
                return null;
            }
        }

        public async Task<VtexStrucsGen2.Strucs.Vtex.SkuService> DeleteAsync<T>(VtexStrucsGen2.Strucs.Vtex.SkuService paramObject)
        {
            try
            {
                string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/skuservice/{paramObject.Id}";
                Console.WriteLine(EndPoint);

                RSHelper rsh = new RSHelper(headers, EndPoint);

                return await rsh.DeleteAsync<VtexStrucsGen2.Strucs.Vtex.SkuService>(EndPoint, paramObject);
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
