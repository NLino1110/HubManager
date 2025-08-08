using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VtexStrucs.Comm;
using VtexStrucs.Tools;

namespace VtexStrucs.Comm
{
    public class Order : IVtexObject
    {
        public Order(vTex_Account accountParam)
        {
            SetAccount(accountParam);
        }

        public async Task<RestSharp.RestResponse> getById(string id)
        {
            string EndPoint = $"https://{baseUrl}.com.br/api/oms/pvt/orders/{id}";
            RSHelper rsh = new RSHelper(headers, EndPoint);
            RestSharp.RestResponse result = await rsh.ExecuteGetAsync(EndPoint);
            return result;
        }

        public async Task<RestSharp.RestResponse> getByDateRange(string dateString, string page)
        {
            //f_creationDate=creationDate:[2016-01-01T02:00:00.000Z TO 2021-01-01T01:59:59.999Z]
            string parameters = $"f_creationDate=creationDate:[{dateString}]&page={page}";
            string EndPoint = $"https://{baseUrl}.com.br/api/oms/pvt/orders?{parameters}";
            RSHelper rsh = new RSHelper(headers, EndPoint);
            RestSharp.RestResponse result = await rsh.ExecuteGetAsync(EndPoint);
            return result;
        }

        public async Task<RestSharp.RestResponse> cancelOrder(string id)
        {                        
            string EndPoint = $"https://{baseUrl}.com.br/inventory/reservations/{id}/cancel";
            RSHelper rsh = new RSHelper(headers, EndPoint);
            RestSharp.RestResponse result = await rsh.ExecuteGetAsync(EndPoint);
            return result;
        }
    }
}
