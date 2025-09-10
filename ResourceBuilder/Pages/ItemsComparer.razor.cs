using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using BlazorTable;
using CobranzasDMSA.Models.General.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using ResourceBuilder.Data.Structs;
using ResourceBuilder.Data.Structs.SyncTask;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

//using Vtex_Tools.Components;
//using Vtex_Tools.Data;
//using Vtex_Tools.Shared;
using VtexStrucs.Strucs;

namespace ResourceBuilder.Pages
{
    public partial class ItemsComparer
    {
        [Inject]
        IToastService toastService { get; set; }

        [CascadingParameter]
        public IModalService modalService { get; set; }
        
        [Parameter]
        public List<ItemForCompare> listData { get; set; }

        private ITable<ItemForCompare> Table;

        //private CompanyAccountT[] listCompany;
        private List<Empresa> listCompany { get; set; }
        private Empresa SelectedAccount;
        private int m_cac_id;

        private string KeywordSearch="";
        private string TypeSearch { get; set; }

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async void ChangeVal(ItemForCompare context, bool val)
        {
            //ItemForCompare
            //Console.WriteLine(val.ToString());
            //context.siscomMktp = val;

            //ApiClientCustom.ProductDb productDb = new ApiClientCustom.ProductDb();
            //ApiClientCustom.ProductApi productApi = new ApiClientCustom.ProductApi();

            //string mktPlaceStr = "null";
            //if(val)
            //{
            //    mktPlaceStr = "'S'";
            //}

            //if (mktPlaceStr == "'S'")
            //{
            //    productDb.UpdateMktp(context.siscomRefId, mktPlaceStr);
            //    //Actualizar de la base del middleware

            //    context.productSync.prod_mkplace = true;
            //    //await productApi.Update(context.productSync);

            //}
            //else
            //{
            //    productDb.UpdateMktp(context.siscomRefId, mktPlaceStr);
            //    //Actualizar de la base del middleware
            //    context.productSync.prod_mkplace = false;
            //    //await productApi.Update(context.productSync);
            //}

            //DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();
            //var itemForChange = appDbContext.product_sync.
            //    Where(x => x.prod_vtex_sku == context.productSync.prod_vtex_sku).
            //    Where(x => x.account_name == SelectedAccount.cac_name).
            //    FirstOrDefault();
            //itemForChange.prod_mkplace = context.productSync.prod_mkplace;
            //appDbContext.product_sync.Update(itemForChange);
            //await appDbContext.SaveChangesAsync();
        }
        
        private async Task<bool> ChangeBool(bool blValue)
        {
            //var parameters = new ModalParameters();

            //string Message = $"Desea desactivar Marketplace ¿Desea continuar?";

            //if (!blValue)
            //{
            //    Message = $"Desea activar Marketplace ¿Desea continuar?";
            //}
                                    
            //parameters.Add(nameof(DisplayMessageCustom.Message), Message);
            //var options = new ModalOptions
            //{
            //    UseCustomLayout = true,
            //    DisableBackgroundCancel = true
            //};

            //var messageForm = modalService.Show<DisplayMessageCustom>("Marketplace", parameters, options);

            //var result = await messageForm.Result;

            //if (result.Confirmed)
            //{
            //    //_message = result.Data?.ToString() ?? string.Empty;
            //    return true;
            //}
            //else
            //{
                
            //}

            return false;
        }

        private VtexStrucs.vTexPrice emptyPrice = new VtexStrucs.vTexPrice()
                                    {
                                        basePrice = 0,
                                        costPrice = 0,
                                        itemId = "",
                                        listPrice = 0,
                                        markup = 0
                                    };

        private VtexStrucs.vtexStock emptyStock = new VtexStrucs.vtexStock()
                                    {
                                        skyId = "",
                                        balance = new List<VtexStrucs.vtexStockBalance>()
                                        {
                                            new VtexStrucs.vtexStockBalance()
                                            {
                                                hasUnlimitedQuantity = false,
                                                reservedQuantity = 0,
                                                totalQuantity = 0,
                                                warehouseId = "",
                                                warehouseName = ""
                                            }
                                        }
                                    };

        private ProductSync emptyProductSync = new ProductSync()
        {
            prod_vtex_sku = 0,
            prod_vtex_id = "",
            prod_name = "",
            prod_pvp = 0,
            prod_pvp_ant = 0,
            prod_stock = 0            
        };

        protected override async void OnInitialized()
        {
            //ApiClientCustom.Security.CompanyAccountApi companyApi = new ApiClientCustom.Security.CompanyAccountApi();
            //listCompany = await companyApi.GetAll();

            //DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();            
            //listCompany = appDbContext.CompanyAccount.ToList();
            
            listCompany = new List<Empresa>();

            TypeSearch = "0";

            StateHasChanged();
            
            base.OnInitialized();
        }

        private async void OnSelectAccount(ChangeEventArgs e)
        {
            int.TryParse(e.Value.ToString(), out m_cac_id);

            //SelectedAccount = listCompany.Where(x => x.cac_id == m_cac_id).First();

            //foreach (var item in listCompany)
            //{
            //    if (m_cac_id != 0 && m_cac_id == item.cac_id)
            //    {
            //        SelectedAccount = item;
            //    }
            //}

            StateHasChanged();           
        }

        private async void OnSelectType(ChangeEventArgs e)
        {
            TypeSearch = e.Value.ToString();

            //await LoadData();
            //StateHasChanged();
            //await InvokeAsync(() =>
            //{
            //    StateHasChanged();
            //});
        }

        async Task ImgPreview(string imgUrl)
        {
            //Se reemplaza este texto porque se asume que se esta enviando el parametro del tamaño de la imagen
            //-100-100
            imgUrl = imgUrl.Replace("-100-100", "");
            imgUrl = imgUrl.Replace("-55-55", "");

            //var parameters = new ModalParameters();
            //parameters.Add(nameof(ImagePreview.img_url), imgUrl);

            //ModalOptions modalOptions = new ModalOptions();
            //modalOptions.Size = ModalSize.Large;

            //var messageForm = modalService.Show<ImagePreview>("Vista Previa", parameters, modalOptions);
            //var result = await messageForm.Result;

            //if (!result.Cancelled)
            //{
            //    //_message = result.Data?.ToString() ?? string.Empty;
            //}
            //else
            //{
            //    return;
            //}
        }

        private async Task DissociateSKUService(ItemForCompare context, int SkuService, string skuServiceName)
        {
            var parameters = new ModalParameters();

            //string Message = $"Se eliminará el servicio {skuServiceName} del producto {context.productFull.ProductName}. ¿Desea continuar?";
            //parameters.Add(nameof(DisplayMessageCustom.Message), Message);
            //var options = new ModalOptions
            //{
            //    UseCustomLayout = true,
            //    DisableBackgroundCancel = true
            //};
            //var messageForm = modalService.Show<DisplayMessageCustom>("Eliminar servicio de Sku", parameters, options);

            //var result = await messageForm.Result;

            //if (result.Cancelled)
            //{
            //    return;
            //}

            //VtexStrucs.Comm.Account account = new VtexStrucs.Comm.Account();
            //account.cac_name = SelectedAccount.cac_name;
            //account.cac_appkey = SelectedAccount.cac_appkey;
            //account.cac_apptoken = SelectedAccount.cac_apptoken;
            //account.cac_environment = SelectedAccount.cac_environment;
            //account.cac_id = SelectedAccount.cac_id;

            //VtexStrucs.Comm.SkuService skuService = new VtexStrucs.Comm.SkuService(account);

            //var skuServiceResultDelete = await skuService.DeleteAsync<VtexStrucsGen2.Strucs.Vtex.SkuService>(new VtexStrucsGen2.Strucs.Vtex.SkuService()
            //{
            //    Id = SkuService
            //});

            toastService.ShowInfo("Servicio eliminado");
        }

        private async Task LoadVtexData(ItemForCompare context)
        {
            //VtexStrucs.Comm.Account account = new VtexStrucs.Comm.Account();
            //account.cac_name = SelectedAccount.cac_name;
            //account.cac_appkey = SelectedAccount.cac_appkey;
            //account.cac_apptoken = SelectedAccount.cac_apptoken;
            //account.cac_environment = SelectedAccount.cac_environment;
            //account.cac_id = SelectedAccount.cac_id;

            //VtexStrucs.Comm.Products products = new VtexStrucs.Comm.Products(account);
            //var prices = await products.getPriceBySku(context.productFull.Id.ToString());

            ////context.price = Convert.ToDouble( prices.basePrice );
            ////context.listPrice = Convert.ToDouble( prices.listPrice );
            //context.price = prices;

            //var stock = await products.getStockBySku(context.productFull.Id.ToString());
            //context.stock = stock;
            ////context.stock = 0;
            ////context.reserved = 0;
            ////foreach ( var stockItem in stock.balance)
            ////{
            ////    context.reserved += stockItem.reservedQuantity;
            ////    context.stock += stockItem.totalQuantity;
            ////}

            //var newDataProduct = await products.getByProductId(context.productFull.ProductId);
            //VtexStrucs.Comm.Supplier supplier = new VtexStrucs.Comm.Supplier(account);

            //if (newDataProduct.SupplierId != null && newDataProduct.SupplierId.ToString() != "")
            //{
            //    var newDataSupplier = await supplier.GetAsync<VtexStrucs.Strucs.Supplier>(newDataProduct.SupplierId.ToString());
            //    if (newDataSupplier != null)
            //    {                    
            //        context.supplier = newDataSupplier;
            //        //Console.WriteLine(newDataSupplier.Name);
            //    }
            //}

            await InvokeAsync(() =>
            {

            });
        }

        private async Task LoadSyncData(ItemForCompare context)
        {
            //ApiClientCustom.ProductApi productDb = new ApiClientCustom.ProductApi();

            //string refId = context.productFull.AlternateIdValues != null && context.productFull.AlternateIdValues.Count > 0 ? context.productFull.AlternateIdValues[0] : "";

            //var productSyncs = await productDb.Get(new Entidades.SyncTask.ProductSync() { 
            //    prod_vtex_sku = context.productFull.Id,
            //    prod_vtex_id = refId,
            //    prod_prov = 0,
            //    account_name = context.accountName
            //});

            //foreach (var productItem in productSyncs)
            //{
            //    context.productSync = productItem;
            //    //    context.syncId = productItem.prod_vtex_sku.ToString();
            //    //    context.syncRefId = productItem.prod_vtex_id;
            //    //    context.syncName = productItem.prod_name;
            //    //    context.syncPvp = Convert.ToDouble(productItem.prod_pvp.Value);

            //    //    context.syncPvpAnt = 0;
            //    //    if (productItem.prod_pvp_ant != null)
            //    //    {
            //    //        context.syncPvpAnt = Convert.ToDouble(productItem.prod_pvp_ant.Value);
            //    //    }
            //    //    context.syncStock = Convert.ToDouble(productItem.prod_stock);
            //}

            await InvokeAsync(() =>
            {

            });
        }

        private async Task LoadSiscomData(ItemForCompare context)
        {
            //ApiClientCustom.ProductDb productDb = new ApiClientCustom.ProductDb();

            //DataTable datatableMain = productDb.GetProductData(context.productFull.AlternateIdValues[0], context.accountName.ToLower());
            //if (datatableMain != null)
            //{
            //    foreach (DataRow rowItem in datatableMain.Rows)
            //    {
            //        context.siscomId = rowItem["SKUID"].ToString();
            //        context.siscomRefId = rowItem["CODIGO_SISCOM"].ToString();
            //        context.siscomName = rowItem["NOMBRE_SISCOM"].ToString();
            //        context.siscomPvp = double.Parse(rowItem["PVP"].ToString());
            //        context.siscomEstado = rowItem["prod_estado"].ToString();
            //        //context.siscomPvpAnt = double.Parse(rowItem["PVP"].ToString());
            //        context.PROD_PESO_KG = double.Parse(rowItem["prod_peso_kg"].ToString());

            //        if (rowItem["STOCK"].ToString() != "")
            //        {
            //            context.siscomStock = double.Parse(rowItem["STOCK"].ToString());
            //        }
            //        else
            //        {
            //            context.siscomStock = 0;
            //        }

            //        if (rowItem["prod_marketplace"].ToString() == "S")
            //        {
            //            context.siscomMktp = true;
            //        }
            //        else
            //        {
            //            context.siscomMktp = false;
            //        }

            //        context.siscomEnsambleId = rowItem["prod_ensamble"].ToString();

            //        ApiClientCustom.EnsambleDb ensambleDb = new ApiClientCustom.EnsambleDb();
            //        var ensamble = ensambleDb.GetById(rowItem["prod_ensamble"].ToString());
            //        if (ensamble.Count > 0)
            //        {
            //            context.siscomEnsambleNombre = ensamble[0].DETALLE;
            //            context.siscomEnsambleCosto = ensamble[0].COSTO;
            //        }
            //    }
            //}

            //if (context.accountName.ToLower().Equals("bosque") || context.accountName.ToLower().Equals("tempodesign"))
            //{
            //    DataTable datatable = productDb.GetProductPromo(context.productFull.AlternateIdValues[0], context.accountName);
            //    List<ItemForCompare_Promocion> promoItems = new List<ItemForCompare_Promocion>();
            //    if (datatable != null)
            //    {
            //        foreach (DataRow rowItem in datatable.Rows)
            //        {
            //            //context.siscomId = rowItem["SKUID"].ToString();
            //            //context.siscomName = rowItem["NOMBRE_SISCOM"].ToString();

            //            //List<ItemForCompare_Promocion> promoItems = context.siscomPromociones.ToList();

            //            promoItems.Add(
            //                            new ItemForCompare_Promocion()
            //                            {
            //                                idPromocion = int.Parse(rowItem["ID_PROMOCION"].ToString()),
            //                                aplicaVtex = rowItem["APLICAVTEX"].ToString(),
            //                                descuento = double.Parse(rowItem["PORCENTAJE"].ToString()),
            //                                nombrePromocion = rowItem["PROMOCION"].ToString(),
            //                                precio = double.Parse(rowItem["PRECIO_PROMOCION"].ToString()),
            //                                precioLista = double.Parse(rowItem["PRECIO_LISTA"].ToString()),
            //                                esPromoPrecio = rowItem["ESPROMOCIONPRECIO"].ToString(),
            //                            });
            //        }
            //    }

            //    context.siscomPromociones = promoItems.ToArray();
            //}

            await InvokeAsync(()=>
            {

            });
        }

        private async Task TestApi()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            headers.Add("Accept", "application/json");
            headers.Add("X-VTEX-API-AppToken", "vtexappkey-bosque-GQKMQB");
            headers.Add("X-VTEX-API-AppKey", "VACQRFWMQRRQUJULDWJKQRUDRYVDTOZQBTINYNQGLNERMVUOXLWLGYYEFZZSJTTBRBOAAGMPGDZCMCQVEHGHSOJDUEHZOXWRUCZLGTNQYGECZWRCIDOCNMJDRLFWYNAI");

            string skuId = "1799";
            string EndPointServer = "https://bosque.vtexcommercestable.com.br/";
            string EndPointApi = $"api/catalog_system/pvt/sku/stockkeepingunitbyid/{skuId}";
            string EndPoint = $"https://bosque.vtexcommercestable.com.br/api/catalog_system/pvt/sku/stockkeepingunitbyid/{skuId}";

            RestClientOptions restClientOptions = new RestClientOptions();
            restClientOptions.RemoteCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            restClientOptions.BaseUrl = new Uri($"{EndPoint}");

            RestClient _client = new RestClient(restClientOptions);

            var restRequest = new RestRequest();
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeaders(headers);

            var result1 = await _client.ExecuteGetAsync<VtexStrucs.Strucs.ProductFull>(restRequest);

            if (result1 != null)
            {
                string strCats = result1.Data.ProductCategories.ToString();
                //string json = dynamicObject.ToString(); // suppose `dynamicObject` is your input
                //Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(strCats);
            }

            return;
        }

        void EvalKeyPress(KeyboardEventArgs e)
        {
            //Console.WriteLine(e.Key);
            if (e.Key == "Enter")
            {
                //await LoadComparer();
                return;
            }
        }
        

        void EvalKeyDown(KeyboardEventArgs e)
        {
            //Console.WriteLine(e.Key);
            if(e.Key=="Enter")
            {
                LoadComparer();
                //return;
            }
        }

        private async Task LoadComparer()
        {
            if (SelectedAccount == null)
            {
                return;
            }

            //////var account = new VtexStrucs.Comm.Account()
            //////{
            //////    cac_appkey = SelectedAccount.cac_appkey,
            //////    cac_apptoken = SelectedAccount.cac_apptoken,
            //////    cac_environment = SelectedAccount.cac_environment.ToLower(),
            //////    cac_id = SelectedAccount.cac_id,
            //////    cac_name = SelectedAccount.cac_name.ToLower()
            //////};

            //////VtexStrucs.Comm.Products products = new VtexStrucs.Comm.Products(account);

            //////string[] productsSku = new string[] { "0" };

            ////////await InvokeAsync(() =>
            ////////{
            //////modalProcess.Show();
            ////////    StateHasChanged();
            ////////});

            //////switch (TypeSearch)
            //////{
            //////    //0 Sku
            //////    case "0":
            //////        {
            //////            if(KeywordSearch=="")
            //////            {
            //////                toastService.ShowInfo("No se ha ingresado el dato para búsqueda");
            //////                modalProcess.Hide();
            //////                return;
            //////            }

            //////            //productsSku = new string[] { KeywordSearch };
            //////            productsSku = KeywordSearch.Split(" ");
            //////        }
            //////        break;
            //////    //Código
            //////    case "1":
            //////        {
            //////            if (KeywordSearch == "")
            //////            {
            //////                toastService.ShowInfo("No se ha ingresado el dato para búsqueda");
            //////                modalProcess.Hide();
            //////                return;
            //////            }

            //////            //Busca el producto en el API de Vtex por RefId
            //////            string prodResult = await products.getSkuByRefId(KeywordSearch);
                        
            //////            if(prodResult == "SKU not found")
            //////            {
            //////                toastService.ShowWarning("No se han obtenido resultados en vtex");

            //////                //Realizar busqueda en siscom
            //////                prodResult = "-1";
            //////                //return;
            //////            }

            //////            Console.WriteLine(prodResult);

            //////            productsSku = new string[] { prodResult };
            //////        }
            //////        break;
            //////    //Keyword
            //////    case "2":
            //////        {
            //////            if (KeywordSearch == "")
            //////            {
            //////                toastService.ShowInfo("No se ha ingresado el dato para búsqueda");
            //////                modalProcess.Hide();
            //////                return;
            //////            }

            //////            //Busca el producto en el API de Vtex por RefId
            //////            var prodResult = await products.getByFilter(KeywordSearch);
            //////            //var prodResult = await products.getProductSearch("ft=" + KeywordSearch);

            //////            listData = new List<ItemForCompare>();

            //////            //foreach (var prodItem in prodResult)
            //////            //{
            //////            //    listData.Add(new ItemForCompare()
            //////            //    {
            //////            //        //ProductId = prodItem.productId,
            //////            //        ProductName = prodItem.productName + " - " + prodItem.items[0].name,
            //////            //        //IsActive = prodItem.IsActive,
            //////            //        //brand = prodItem.brandId,

            //////            //        //SkuId = prodItem.items[0].itemId,
            //////            //        RefId = prodItem.productReference,
            //////            //        SkuImageUrl = prodItem.items[0].images[0].imageUrl,

            //////            //        accountName = SelectedAccount.cac_name,

            //////            //        siscomId = "",
            //////            //        siscomName = "",
            //////            //        siscomPromociones = new ItemForCompare_Promocion[]
            //////            //            {

            //////            //            }
            //////            //    });
            //////            //}

            //////            foreach (var prodItem in prodResult.items)
            //////            {
            //////                foreach (var skuItem in prodItem.stockKeepingUnitBasicDtoCollection)
            //////                {
            //////                    listData.Add(new ItemForCompare()
            //////                    {
            //////                        productFull = new ProductFull()
            //////                        {
            //////                            ProductId = prodItem.productId.Value,
            //////                            ProductName = prodItem.productName + " - " + skuItem.skuName,
            //////                            IsActive = prodItem.isActive.Value,
            //////                            BrandName = prodItem.brand,
            //////                            Id = skuItem.id.Value,
            //////                            AlternateIdValues = new List<string>() { skuItem.refId },
            //////                            ImageUrl = skuItem.imageUrl,
            //////                            DetailUrl = prodItem.detailUrl,
            //////                            Dimension = new Dimension()
            //////                            {
            //////                                cubicweight = 0,
            //////                                height = 0,
            //////                                length = 0,
            //////                                weight = 0,
            //////                                width = 0
            //////                            }                                        
            //////                        },
            //////                        productByFilter = prodResult,
            //////                        accountName = SelectedAccount.cac_name,

            //////                        siscomId = "",
            //////                        siscomName = "",
            //////                        siscomPromociones = new ItemForCompare_Promocion[]
            //////                        {

            //////                        },
            //////                        price = emptyPrice,
            //////                        stock = emptyStock,
            //////                        productSync = emptyProductSync,
            //////                        supplier = new Supplier()
            //////                        {
            //////                            Id = 0,
            //////                            Name = ""                                        
            //////                        }
            //////                    });
            //////                }
            //////            }

            //////            Console.WriteLine(prodResult);
            //////            modalProcess.Hide();
            //////            return;
            //////            //productsSku = new string[] { prodResult };
            //////        }
            //////        break;
            //////    //Buscar todos
            //////    case "3":
            //////        {
            //////            //Se procederá a sacar todos los datos de Vtex
            //////            // Mensaje de confirmación

            //////            productsSku = products.getDataVtexAllSkus();

            //////            //productsSku = new string[] {
            //////            //    "4",
            //////            //    "5",
            //////            //    "6"
            //////            //};
            //////        }
            //////        break;
            //////}

            //////int intCount = 0;            
            
            //////listData = new List<ItemForCompare>();

            //////List<Task> taskArray = new List<Task>();

            ////////int completedIterations = 0;

            //////foreach (var skuItem in productsSku)
            //////{
            //////    if (skuItem == "-1")
            //////    {
            //////        modalProcess.Hide();
            //////        return;
            //////    }

            //////    taskArray.Add(Task.Factory.StartNew(async () => {
            //////        //Get información Vtex
            //////        VtexStrucs.Strucs.ProductFull prodItem = null;
            //////        string strResult = "";

            //////        string skuItem_search = skuItem;

            //////        try
            //////        {
            //////            //strResult = products.getBySkuIdFullString(skuItem).Result;
            //////            //skuItem debe pasar por un intento de conversion a int para validar
            //////            if(!int.TryParse(skuItem_search, out _))
            //////            {
            //////                skuItem_search = "0";
            //////            }
            //////            prodItem = products.getBySkuIdFull(skuItem_search).Result;
                                                
            //////        }
            //////        catch(Exception e)
            //////        {
            //////            listData.Add(new ItemForCompare()
            //////            {
            //////                productFull = new ProductFull()
            //////                {
            //////                    Id = int.Parse(skuItem_search),
            //////                    ProductId = int.Parse(skuItem_search),
            //////                    ProductName = "No encontrado",
            //////                    IsActive = false,
            //////                    ProductRefId = KeywordSearch,
            //////                    ImageUrl = "/img/not_found.png",
            //////                },

            //////                siscomId = "",
            //////                siscomName = "",
            //////                accountName = SelectedAccount.cac_name,
            //////                siscomPromociones = new ItemForCompare_Promocion[]
            //////                {

            //////                },
            //////                price = emptyPrice,
            //////                stock = emptyStock,
            //////                productSync = emptyProductSync
            //////            }); ;
            //////        }

            //////        //prodItem.Result.ProductRefId
            //////        if (prodItem != null)
            //////        {
            //////            listData.Add(new ItemForCompare()
            //////            {
            //////                productFull = prodItem,
            //////                supplier = new Supplier(),
            //////                accountName = SelectedAccount.cac_name,                            
            //////                siscomId = "",
            //////                siscomName = "",
            //////                siscomPromociones = new ItemForCompare_Promocion[]
            //////                {

            //////                },
            //////                price = emptyPrice,
            //////                stock = emptyStock,
            //////                productSync = emptyProductSync
            //////                //Services = prodItem.Services
            //////            });
            //////        }

            //////        //Interlocked.Increment(ref completedIterations);
            //////        //Console.WriteLine("completedIterations:" + completedIterations.ToString());

            //////        //Información Datos Sincronizador
            //////        //

            //////        //Información Siscom

            //////        intCount++;

            //////        float currentPercentage = 0;
            //////        currentPercentage = ((float)intCount / (float)productsSku.Length) * 100;
            //////        Console.WriteLine("Percentage:" + currentPercentage.ToString() + "%");

            //////        string modalBody = @"<div class=""container"">
            //////                  <div class=""row p-2"">
            //////                    <div class=""col"">
            //////                      Procesando productos
            //////                    </div>
            //////                  </div>
            //////                  <div class=""row p-2"">
            //////                    <div class=""col"">
            //////                      <div class=""progress"">
            //////                        <div class=""progress-bar progress-bar-striped progress-bar-animated"" role=""progressbar"" aria-valuenow=""0"" aria-valuemin=""0"" aria-valuemax=""100"" style=""width: " + Math.Round(currentPercentage).ToString() + @"%""></div>
            //////                      </div>
            //////                    </div>
            //////                  </div>
            //////                </div>";

            //////        modalProcess.SetBody(modalBody);
            //////        modalProcess.Hide();
            //////        await InvokeAsync(() =>
            //////        {
            //////            StateHasChanged();
            //////        });
            //////    }));

            //////    //taskArray[intCount] = Task.Factory.StartNew(() => {                
            //////    //    var prodItem = products.getBySkuIdFull(skuItem);
            //////    //    listData.Add(new ItemForCompare()
            //////    //    {
            //////    //        ProductId = prodItem.Result.Id,
            //////    //        SkuId = prodItem.Result.Id,
            //////    //        ProductName = prodItem.Result.ProductName
            //////    //    });
            //////    //});

            //////    //if (intCount == 100)
            //////    //{
            //////    //    break;
            //////    //}
                
            //////}

            //await Task.Factory.ContinueWhenAny(taskArray.ToArray(), completedTask =>
            //{
            //    Console.WriteLine("{0} listData: ", listData.Count.ToString());
            //});

            //////await Task.Factory.ContinueWhenAll(taskArray.ToArray(), 
            //////    completedTasks => {
            //////        Console.WriteLine("{0} listData: ", listData.Count.ToString());
            //////        //toastService.ShowInfo("Productos de la lista agregados.");
            //////        //modalProcess.Hide();
            //////    }
            //////);
            //return null;
        }
    }
}
