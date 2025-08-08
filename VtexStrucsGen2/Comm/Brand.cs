using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VtexStrucs.Comm;
using VtexStrucs.Tools;
using VtexStrucsGen2.Strucs;

namespace VtexStrucs.Comm
{
    public class Brand : IVtexObject
    {
        public Brand(vTex_Account accountParam)
        {
            SetAccount(accountParam);
        }

        public async Task<RestSharp.RestResponse> getById(int brandId)
        {
            string EndPoint = $"https://{baseUrl}.com.br/api/catalog_system/pvt/brand/:{brandId}";
            RSHelper rsh = new RSHelper(headers, EndPoint);
            RestSharp.RestResponse result = await rsh.ExecuteGetAsync(EndPoint);
            return result;
        }

        public async Task<RestSharp.RestResponse> getList()
        {
            string EndPoint = $"https://{baseUrl}.com.br/api/catalog_system/pvt/brand/list";
            RSHelper rsh = new RSHelper(headers, EndPoint);
            RestSharp.RestResponse result = await rsh.ExecuteGetAsync(EndPoint);
            return result;
        }

        public async Task<RestSharp.RestResponse> Create(VtexStrucsGen2.Strucs.Brand brand)
        {
            string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/brand";
            RSHelper rsh = new RSHelper(headers, EndPoint);            
            object bodyObject = brand;
            RestSharp.RestResponse result = await rsh.ExecutePostAsync(EndPoint, bodyObject);
            return result;
        }

        public async Task<RestSharp.RestResponse> Update(VtexStrucsGen2.Strucs.Brand brand)
        {
            string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/brand/:{brand.id}";
            RSHelper rsh = new RSHelper(headers, EndPoint);
            object bodyObject = brand;
            RestSharp.RestResponse result = await rsh.ExecutePutAsync(EndPoint, bodyObject);
            return result;
        }
    }
}
