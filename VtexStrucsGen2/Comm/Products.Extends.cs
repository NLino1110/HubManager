using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VtexStrucs;
using VtexStrucs.Strucs;
using VtexStrucs.Tools;

namespace VtexStrucs.Comm
{
    public partial class Products
    {
        public async Task<RestResponse> setNewPrice(int prod_vtex_sku, decimal listPrice, decimal costPrice, decimal basePrice, decimal markup)
        {
            vTexPriceforChange precio = new vTexPriceforChange
            {
                listPrice = listPrice,
                costPrice = costPrice,
                markup = markup//,
                //basePrice = basePrice
            };

            //c0b03bac-5dce-449a-b6f6-ff263ec2bd8b
            string skuId = prod_vtex_sku.ToString();

            string EndPoint = $"https://api.vtex.com/{account.Account_Name}/pricing/prices/{skuId}";

            RSHelper rsh = new RSHelper(headers, EndPoint);
            return await rsh.ExecutePutAsync(EndPoint, precio);

            //RSHelper rsh = new RSHelper(headers);
            //string Response = rsh.PUT(EndPoint, JsonSerializer.Serialize(precio));

            //if (Response != "")
            //{

            //}

            //return Response;
        }

        public string UpdateProdData(Product item)
        {
            string productId = item.Id.ToString();
            string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/product/{productId}";

            RSHelper rsh = new RSHelper(headers, EndPoint);
            var Response = rsh.ExecutePostAsync(EndPoint, new
            {
                Name = item.Name, /*Requerido*/
                DepartmentId = item.DepartmentId, /*Requerido*/
                CategoryId = item.CategoryId, /*Requerido*/
                BrandId = item.BrandId, /*Requerido*/
                Description = item.Description,
                KeyWords = item.KeyWords,
                ReleaseDate = item.ReleaseDate,
                Title = item.Title,
                MetaTagDescription = item.MetaTagDescription,
                RefId = item.RefId,
                IsActive = item.IsActive
            });

            return "";
        }

        public async Task<VtexStrucs.Strucs.Product> getBySkuIdMain(string productId)
        {
            //c0b03bac-5dce-449a-b6f6-ff263ec2bd8b

            //Esta API hace extra la información de la data del producto en tiempo real
            //  TODO: se harán cambios para utilizar esta API al momento de llenar los datos de la db
            string EndPoint = $"https://{baseUrl}.com.br/api/catalog/pvt/product/{productId}";

            VtexStrucs.Strucs.Product vtexprod = null;

            RSHelper rsh = new RSHelper(headers, EndPoint);
            vtexprod = await rsh.GetAsync<VtexStrucs.Strucs.Product>(EndPoint);

            return vtexprod;
        }
    }
}
