using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using VtexStrucs.Comm;
using VtexStrucs.Tools;

namespace VtexStrucs.Comm
{
    public class SkuServiceValue : IVtexObject
    {
        public SkuServiceValue(vTex_Account accountParam)
        {
            SetAccount(accountParam);
        }

        /// <summary>
        /// List
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        
        public async Task<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue> GetAsync<T>(string IdSkuServicoValor)
        {
            try
            {
                string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/skuservicevalue/{IdSkuServicoValor}";
                Console.WriteLine(EndPoint);

                RSHelper rsh = new RSHelper(headers, EndPoint);
                
                return await rsh.GetAsync<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue>(EndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetAsync2:" + ex.Message);
                Console.WriteLine("Error GetAsync2:" + ex.StackTrace);
                return null;
            }
        }

        public async Task<RestResponse> DeleteAsync(string IdSkuServicoValor)
        {
            try
            {
                string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/skuservicevalue/{IdSkuServicoValor}";
                Console.WriteLine(EndPoint);

                RSHelper rsh = new RSHelper(headers, EndPoint);

                return await rsh.DeleteAsync(EndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteAsync:" + ex.Message);
                Console.WriteLine("Error DeleteAsync:" + ex.StackTrace);
                return null;
            }
        }

        public async Task<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue> PostAsync<T>(VtexStrucsGen2.Strucs.Vtex.SkuServiceValue paramObject)
        {
            try
            {
                string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/skuservicevalue";
                Console.WriteLine(EndPoint);

                RSHelper rsh = new RSHelper(headers, EndPoint);

                return await rsh.PostAsync<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue>(EndPoint, paramObject);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetAsync2:" + ex.Message);
                Console.WriteLine("Error GetAsync2:" + ex.StackTrace);
                return null;
            }
        }

        public async Task<RestSharp.RestResponse> PostAsyncResponse(VtexStrucsGen2.Strucs.Vtex.SkuServiceValue paramObject)
        {
            try
            {
                string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/skuservicevalue";
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

        [Description("Update service value")]
        public async Task<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue> PutAsync<T>(VtexStrucsGen2.Strucs.Vtex.SkuServiceValue paramObject)
        {
            try
            {
                string skuServiceValueId = paramObject.Id.ToString();
                string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/skuservicevalue/{skuServiceValueId}";
                Console.WriteLine(EndPoint);

                RSHelper rsh = new RSHelper(headers, EndPoint);

                return await rsh.PutAsync<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue>(EndPoint, paramObject);
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
