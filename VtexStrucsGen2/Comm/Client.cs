using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VtexStrucs.Comm;
using VtexStrucs.Tools;

namespace VtexStrucs.Comm
{
    public class Client : IVtexObject
    {
        public Client(vTex_Account accountParam)
        {
            SetAccount(accountParam);
        }

        public async Task<RestSharp.RestResponse> getByEmail(string email)
        {
            email = email.Trim();
            //string _params = $"?_fields=id,document,email,firstName,lastName,homePhone,phone,checkouttag,rclastcart,carttag,rclastcartvalue,professional,academy,createdIn,updatedIn,birthDate,gender&_page=1&_where= professional = True AND document = \"{document}\"&_sort=createdIn desc";
            string _params = $"?_fields=id,document,email,firstName,lastName,homePhone,phone,checkouttag,rclastcart,carttag,rclastcartvalue,professional,academy,createdIn,updatedIn,birthDate,gender&_page=1&_where=email = \"{email}\"&_sort=createdIn desc";
            string EndPoint = $"https://{baseUrl}.com.br/api/dataentities/CL/search" + _params;
            RSHelper rsh = new RSHelper(headers, EndPoint);
            RestSharp.RestResponse result = await rsh.ExecuteGetAsync(EndPoint);
            return result;
        }

        public async Task<RestSharp.RestResponse> getByDocument(string document)
        {
            //string _params = $"?_fields=id,document,email,firstName,lastName,homePhone,phone,checkouttag,rclastcart,carttag,rclastcartvalue,professional,academy,createdIn,updatedIn,birthDate,gender&_page=1&_where= professional = True AND document = \"{document}\"&_sort=createdIn desc";
            string _params = $"?_fields=id,document,email,firstName,lastName,homePhone,phone,checkouttag,rclastcart,carttag,rclastcartvalue,professional,academy,createdIn,updatedIn,birthDate,gender&_page=1&_where=document = \"{document}\"&_sort=createdIn desc";
            string EndPoint = $"https://{baseUrl}.com.br/api/dataentities/CL/search" + _params;
            RSHelper rsh = new RSHelper(headers, EndPoint);
            RestSharp.RestResponse result = await rsh.ExecuteGetAsync(EndPoint);
            return result;
        }

        public async Task<RestSharp.RestResponse> addDocument(VtexStrucsGen2.Strucs.Vtex.Client client)
        {            
            string EndPoint = $"https://{baseUrl}.com.br/api/dataentities/cl/documents";
            RSHelper rsh = new RSHelper(headers, EndPoint);
            object bodyObject = client;
            RestSharp.RestResponse result = await rsh.ExecutePostAsync(EndPoint, bodyObject);
            return result;
        }

        public async Task<RestSharp.RestResponse> createOrUpdateDocument(VtexStrucsGen2.Strucs.Vtex.Client client)
        {
            string EndPoint = $"https://{baseUrl}.com.br/api/dataentities/CL/documents";
            RSHelper rsh = new RSHelper(headers, EndPoint);            
            object bodyObject = client;
            RestSharp.RestResponse result = await rsh.ExecutePutAsync(EndPoint, bodyObject);
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
    }
}
