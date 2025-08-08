using DataSourceManager;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using VtexStrucs;
using VtexStrucsGen2.Strucs.Vtex;
using static VtexStrucsGen2.Strucs.Vtex.ResponseModels.ProductSearch;

namespace ResourceBuilder.Services.Sync
{
    public partial class Processor
    {
        public async Task EvalServiceEnsamblaje(vTex_Account mAccount,
            int skuId,
            decimal costPrice,
            decimal basePrice,
            bool prod_mkplace,
            string PROD_ENSAMBLE)
        {
            int SkuServiceTypeId = 1;

            VtexStrucs.Comm.Products products = new VtexStrucs.Comm.Products(mAccount);
            
            var prodItem = await products.getBySkuIdFull(skuId.ToString());

            if (prodItem != null)
            {
                //'2', '00-NO ASIGNADO', '0.10'
                //'4', '01-BAJA', '2.68', '3.00'
                //'5', '02-FACIL', '8.93', '10.00'
                //'6', '03-NORMAL', '13.39', '15.00'
                //'7', '04-DIFÍCIL', '16.07', '18.00'
                //'8', '05-MUY DIFICIL', '23.21', '26.00'
                //'9', '06-COMPLEJO', '31.25', '35.00'
                //'10', '07-ENSAMBLAJE GRATIS', '0.01', '0.01'
                //'11', '08-MUY COMPLEJO', '35.71', '40.00'

                int IdSkuServicoValor = 0;

                switch (PROD_ENSAMBLE)
                {
                    case "01":
                        {
                            IdSkuServicoValor = 4;
                        }
                        break;
                    case "02":
                        {
                            IdSkuServicoValor = 5;
                        }
                        break;
                    case "03":
                        {
                            IdSkuServicoValor = 6;
                        }
                        break;
                    case "04":
                        {
                            IdSkuServicoValor = 7;
                        }
                        break;
                    case "05":
                        {
                            IdSkuServicoValor = 8;
                        }
                        break;
                    case "06":
                        {
                            IdSkuServicoValor = 9;
                        }
                        break;
                    case "07":
                        {
                            IdSkuServicoValor = 10;
                        }
                        break;
                    case "08": //Muy Complejo
                        {
                            IdSkuServicoValor = 11;
                        }
                        break;
                }


                //Se usan los codigos compatibles para Vtex
                VtexStrucs.Comm.SkuService skuService = new VtexStrucs.Comm.SkuService(mAccount);

                int ServiceIdForUpdate = 0;
                bool mustAdd = true;

                if (prodItem != null)
                {
                    if (prodItem.Services != null && prodItem.Services.Count > 0)
                    {
                        foreach (var service in prodItem.Services)
                        {
                            if (IdSkuServicoValor > 0)
                            {
                                //Old service type
                                if (service.ServiceTypeId == 1 || service.ServiceTypeId == 2)
                                {
                                    foreach (var option in service.Options)
                                    {
                                        ServiceIdForUpdate = option.Id;

                                        //Se lee el SkuService para obtener el SkuServiceValueId
                                        var skuServiceGet = await skuService.GetAsync<VtexStrucsGen2.Strucs.Vtex.SkuService>(ServiceIdForUpdate.ToString());

                                        if (skuServiceGet != null)
                                        {
                                            if (skuServiceGet.SkuServiceValueId == IdSkuServicoValor && skuServiceGet.SkuServiceTypeId == service.ServiceTypeId)
                                            {
                                                //Ya contiene asignado el servicio correcto
                                                continue;
                                            }
                                        }
                                        
                                        VtexStrucs.Comm.SkuServiceValue skuServiceValue = new VtexStrucs.Comm.SkuServiceValue(mAccount);
                                        var skuServiceValueResult = await skuServiceValue.GetAsync<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue>(IdSkuServicoValor.ToString());

                                        if ((float) Math.Round(option.Price, 2) == (float) Math.Round(skuServiceValueResult.Value, 2))//if (option.Price == skuServiceValueResult.Value)
                                        {
                                            mustAdd = false;
                                            continue;
                                        }

                                        // Solo se requiere el Id
                                        var skuServiceResultDelete = await skuService.DeleteAsync<VtexStrucsGen2.Strucs.Vtex.SkuService>(new VtexStrucsGen2.Strucs.Vtex.SkuService()
                                        {
                                            Id = option.Id,
                                            SkuId = skuId,
                                            SkuServiceTypeId = 2,
                                            SkuServiceValueId = IdSkuServicoValor,
                                            Name = "Servicio de ensamblaje",
                                            Text = "Servicio de ensamblaje",
                                            IsActive = true
                                        });

                                        //Se requiere tambien actualizar el Servicio ya asignado,
                                        //porque no actualiza el precio del ServiceValue origen
                                        //var skuServicePutResult = await skuService.PutAsync<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue>(new VtexStrucsGen2.Strucs.Vtex.SkuService()
                                        //{
                                        //    Id = ServiceIdForUpdate,
                                        //    SkuServiceValueId = IdSkuServicoValor,
                                        //    SkuServiceTypeId = SkuServiceTypeId,
                                        //    Name = "Servicio de ensamblaje",
                                        //    Text = "Servicio de ensamblaje",
                                        //    IsActive = true
                                        //});

                                        //
                                        VtexStrucsGen2.Strucs.Vtex.SkuServicePost ObjForSend = new VtexStrucsGen2.Strucs.Vtex.SkuServicePost();
                                        ObjForSend.SkuServiceTypeId = SkuServiceTypeId;
                                        ObjForSend.SkuServiceValueId = IdSkuServicoValor;
                                        ObjForSend.SkuId = skuId;
                                        ObjForSend.Name = "Servicio de ensamblaje";
                                        ObjForSend.Text = "Servicio de ensamblaje";
                                        ObjForSend.IsActive = true;

                                        //Ingresa una nuevo registro
                                        var skuServiceResult = await skuService.PostAsync(ObjForSend);

                                        mustAdd = false;
                                        continue;
                                    }
                                    continue;
                                }

                                //if (service.ServiceTypeId == 1)
                                //{
                                //    mustAdd = false;
                                //    //continue;
                                //    foreach (var option in service.Options)
                                //    {
                                //        ServiceIdForUpdate = option.Id;

                                //        //Se lee el SkuService para obtener el SkuServiceValueId
                                //        //var skuServiceGet = await skuService.GetAsync<VtexStrucsGen2.Strucs.Vtex.SkuService>(ServiceIdForUpdate.ToString());

                                //        //Se requiere tambien actualizar el Servicio ya asignado,
                                //        //porque no actualiza el precio del ServiceValue origen
                                //        //var skuServicePutResult = await skuService.ExecutePutAsync<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue>(new VtexStrucsGen2.Strucs.Vtex.SkuService()
                                //        //{
                                //        //    Id = ServiceIdForUpdate,                                            
                                //        //    SkuServiceValueId = 4, //00-NO ASIGNADO - para forzar la actualización
                                //        //    SkuServiceTypeId = SkuServiceTypeId,
                                //        //    SkuId = skuId,
                                //        //    Name = "Servicio de ensamblaje",
                                //        //    Text = "Servicio de ensamblaje",
                                //        //    IsActive = true
                                //        //});

                                //        //skuServicePutResult = await skuService.ExecutePutAsync<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue>(new VtexStrucsGen2.Strucs.Vtex.SkuService()
                                //        //{
                                //        //    Id = ServiceIdForUpdate,                                            
                                //        //    SkuServiceValueId = IdSkuServicoValor,
                                //        //    SkuServiceTypeId = SkuServiceTypeId,
                                //        //    SkuId = skuId,
                                //        //    Name = "Servicio de ensamblaje",
                                //        //    Text = "Servicio de ensamblaje",
                                //        //    IsActive = true
                                //        //});

                                //        var skuServiceResultDelete = await skuService.DeleteAsync<VtexStrucsGen2.Strucs.Vtex.SkuService>(new VtexStrucsGen2.Strucs.Vtex.SkuService()
                                //        {
                                //            Id = option.Id,
                                //            SkuId = skuId,
                                //            SkuServiceTypeId = 2,
                                //            SkuServiceValueId = IdSkuServicoValor,
                                //            Name = "Servicio de ensamblaje",
                                //            Text = "Servicio de ensamblaje",
                                //            IsActive = true
                                //        });

                                //        VtexStrucsGen2.Strucs.Vtex.SkuServicePost ObjForSend = new VtexStrucsGen2.Strucs.Vtex.SkuServicePost();
                                //        ObjForSend.SkuServiceTypeId = SkuServiceTypeId;
                                //        ObjForSend.SkuServiceValueId = IdSkuServicoValor;
                                //        ObjForSend.SkuId = skuId;
                                //        ObjForSend.Name = "Servicio de ensamblaje";
                                //        ObjForSend.Text = "Servicio de ensamblaje";
                                //        ObjForSend.IsActive = true;

                                //        //Ingresa una nuevo registro
                                //        var skuServiceResult = await skuService.PostAsync(ObjForSend);

                                //        continue;
                                //    }
                                //}
                            }
                            else
                            {
                                continue;
                            }

                            if (service.ServiceTypeId == 1)
                            {
                                continue;
                            }
                        }
                    }
                }

                if (mustAdd)
                {
                    if (IdSkuServicoValor != 0)
                    //if(ServiceIdForUpdate == 0)
                    {
                        VtexStrucsGen2.Strucs.Vtex.SkuServicePost ObjForSend = new VtexStrucsGen2.Strucs.Vtex.SkuServicePost();
                        ObjForSend.SkuServiceTypeId = SkuServiceTypeId;
                        ObjForSend.SkuServiceValueId = IdSkuServicoValor;
                        ObjForSend.SkuId = skuId;
                        ObjForSend.Name = "Servicio de ensamblaje";
                        ObjForSend.Text = "Servicio de ensamblaje";
                        ObjForSend.IsActive = true;

                        //Ingresa una nuevo registro
                        var skuServiceResult = await skuService.PostAsync(ObjForSend);
#if (DEBUG)
                        Console.WriteLine(skuServiceResult.Content);
#endif
                    }
                }
            }
        }

        public async Task EvalService(vTex_Account mAccount, 
            int skuId, 
            decimal costPrice, 
            decimal basePrice,
            bool prod_mkplace)
        {
            //Servicio de Garantía Extendida
            //Bosque 3 
            //Tempo 2
            int SkuServiceTypeId = 3;

            if(mAccount.Account_Name.ToLower() == "tempodesign")
            {
                SkuServiceTypeId = 2;
            }

            VtexStrucs.Comm.SkuServiceValue skuServiceValue = new VtexStrucs.Comm.SkuServiceValue(mAccount);
            VtexStrucs.Comm.SkuService skuService = new VtexStrucs.Comm.SkuService(mAccount);
            VtexStrucs.Comm.Products products = new VtexStrucs.Comm.Products(mAccount);

            var prodItem = await products.getBySkuIdFull(skuId.ToString());

            bool CategoryExcluded = false;
            foreach (var itemCategory in prodItem.ProductCategories.ToList())
            {
                if(itemCategory.ToString().ToLower().Contains("colchones") ||
                    itemCategory.ToString().ToLower().Contains("accesorios") ||
                    itemCategory.ToString().ToLower().Contains("tecnología") ||
                    itemCategory.ToString().ToLower().Contains("electrohogar")
                    )
                {
                    CategoryExcluded = true;
                    break;
                }
            }

            //prodItem.Categories
            //Evaluar que no sea marketplace
            // que no sea colchon
            // y que sea >= 100
            // Hogar Protegido se excluye skuId == 5952 -- Bosque
            //if (basePrice <= 100 || prod_mkplace || CategoryExcluded || skuId == 5952)
            if (prod_mkplace || CategoryExcluded || skuId == 5952)
            {
                Console.WriteLine("Producto excluido de la sincronización " +
                    skuId.ToString() + " " +
                    basePrice.ToString() + " " +
                    prod_mkplace.ToString() + " " +
                    CategoryExcluded.ToString());
                return;
            }

            int ServiceIdForUpdate = 0;
            bool havingService = false;

            if (prodItem != null)
            {
                var price = await products.getPriceBySku(skuId.ToString());

                float basePriceItem = 0;
                float newCost = basePriceItem;
                float newValue = basePriceItem;

                if( price != null )
                {
                    basePriceItem = (float)price.basePrice * (float)0.10 * (float) 1.12;
                    newCost = basePriceItem;
                    newValue = basePriceItem;
                }

                if (prodItem.Services != null && prodItem.Services.Count > 0)
                {
                    foreach (var service in prodItem.Services)
                    {                        
                        //Old service type
                        if (service.ServiceTypeId == SkuServiceTypeId)
                        {
                            foreach (var option in service.Options)
                            {
                                ServiceIdForUpdate = option.Id;
                                
                                if (price != null)
                                {
                                    //var skuServiceResult = await skuServiceValue.GetAsync<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue>(ServiceIdForUpdate.ToString());

                                    //ID assigned "Id": 2294,
                                    //Update if value is not equals
                                    //New Service Value Price
                                    //{
                                        //"Id": 18,
                                        //"SkuServiceTypeId": 3,
                                        //"Name": "GEX 1Y 5502 157E",
                                        //"Value": 28.9000,
                                        //"Cost": 28.9000
                                    //}

                                    if (Math.Round(option.Price,2) != Math.Round(newCost,2))
                                    {
                                        //Se lee el SkuService para obtener el SkuServiceValueId
                                        var skuServiceGet = await skuService.GetAsync<VtexStrucsGen2.Strucs.Vtex.SkuService>(ServiceIdForUpdate.ToString());
                                        if (skuServiceGet == null)
                                        {
                                            break;
                                        }

                                        //Se procede a eliminar ya que no actualiza el valor
                                        var skuServiceResultDelete = await skuService.DeleteAsync<VtexStrucsGen2.Strucs.Vtex.SkuService>(new VtexStrucsGen2.Strucs.Vtex.SkuService()
                                        {
                                            Id = option.Id,
                                            SkuId = skuId,
                                            SkuServiceTypeId = SkuServiceTypeId,
                                            //SkuServiceValueId = IdSkuServicoValor,
                                            //Name = "Servicio de ensamblaje",
                                            //Text = "Servicio de ensamblaje",
                                            IsActive = true
                                        });

                                        int ServiceValueToAssign = 0;

                                        if (skuServiceGet != null)
                                        {
                                            ServiceValueToAssign = skuServiceGet.SkuServiceValueId;
                                            var skuServiceValuePutResult = await skuServiceValue.PutAsync<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue>(new VtexStrucsGen2.Strucs.Vtex.SkuServiceValue()
                                            {
                                                Id = skuServiceGet.SkuServiceValueId,
                                                Name = "GEX 1Y " + skuId.ToString() + " " + skuId.ToString("X"),
                                                Cost = (decimal) newCost,
                                                Value = (decimal) newValue,
                                                SkuServiceTypeId = SkuServiceTypeId,
                                            });

                                            Console.WriteLine("Valor de servicio actualizado " + skuServiceGet.SkuServiceValueId.ToString() + " " + 
                                                "GEX 1Y " + skuId.ToString() + " " + skuId.ToString("X") +  " (" + option.Price.ToString() + " -> " +  newCost.ToString() + ")");
                                        }
                                        else
                                        {
                                            //VTEX NO TIENE UN ESTADO REAL, SE SALE Y SE PERMITE QUE SE CREE UN NUEVO SERVICIO
                                            havingService = false;
                                            break;
                                        }

                                        VtexStrucsGen2.Strucs.Vtex.SkuServicePost skuServicePost = new VtexStrucsGen2.Strucs.Vtex.SkuServicePost();
                                        skuServicePost.SkuServiceTypeId = SkuServiceTypeId;
                                        skuServicePost.SkuServiceValueId = ServiceValueToAssign;
                                        skuServicePost.SkuId = skuId;
                                        skuServicePost.Name = "Garantía Extendida 1 año";
                                        skuServicePost.Text = "Garantía Extendida 1 año";
                                        skuServicePost.IsActive = true;

                                        var newServiceResult = await skuService.PostAsync(skuServicePost);

                                        //////--------------------------------------------------------------------

                                        ////////Se actualiza al valor nuevo
                                        //////var skuServiceValuePutResult = await skuServiceValue.PutAsync<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue>(new VtexStrucsGen2.Strucs.Vtex.SkuServiceValue()
                                        //////{
                                        //////    Id = skuServiceGet.SkuServiceValueId,
                                        //////    Name = "GEX 1Y " + skuId.ToString() + " " + skuId.ToString("X"),
                                        //////    Cost = newCost,
                                        //////    Value = newValue,
                                        //////    SkuServiceTypeId = SkuServiceTypeId,
                                        //////});

                                        ////////Se requiere tambien actualizar el Servicio ya asignado,
                                        ////////  porque no actualiza el precio del ServiceValue origen

                                        ////////Primero se fuerza la asignación para que se actualice
                                        //////await skuService.PutAsync<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue>(new VtexStrucsGen2.Strucs.Vtex.SkuService()
                                        //////{
                                        //////    Id = ServiceIdForUpdate,
                                        //////    SkuServiceValueId = 30, //ID temporal
                                        //////    SkuServiceTypeId = SkuServiceTypeId,
                                        //////    Name = "Garantía Extendida 1 año",// + skuId.ToString() + " " + skuId.ToString("X"), //Nombre mostrado en carrito
                                        //////    Text = "Garantía Extendida 1 año",
                                        //////    IsActive = true
                                        //////});

                                        ////////Se colocan los datos correctos
                                        //////var skuServicePutResult = await skuService.PutAsync<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue>(new VtexStrucsGen2.Strucs.Vtex.SkuService()
                                        //////{
                                        //////    Id = ServiceIdForUpdate,
                                        //////    SkuServiceValueId = skuServiceGet.SkuServiceValueId,
                                        //////    SkuServiceTypeId = SkuServiceTypeId,
                                        //////    Name = "Garantía Extendida 1 año",// + skuId.ToString() + " " + skuId.ToString("X"), //Nombre mostrado en carrito
                                        //////    Text = "Garantía Extendida 1 año",
                                        //////    IsActive = true
                                        //////});
                                    }
                                }
                            }
                            havingService = true;
                            continue;
                        }
                    }
                }

                if (!havingService)
                {
                    //New Service Value
                    //var newSkuServiceResult = await skuServiceValue.PostAsync<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue>(new VtexStrucsGen2.Strucs.Vtex.SkuServiceValue()
                    var newSkuServiceResponse = await skuServiceValue.PostAsyncResponse(new VtexStrucsGen2.Strucs.Vtex.SkuServiceValue()
                    {
                        Name = "GEX 1Y " + skuId.ToString() + " " + skuId.ToString("X"),
                        Cost = (decimal) newCost,
                        Value = (decimal) newValue,
                        SkuServiceTypeId = SkuServiceTypeId,
                    });

                    //Console.WriteLine(newSkuServiceResponse.Content);
                    
                    //TODO: Comentado porque no se usa 13/01/2025
                    //////if(newSkuServiceResponse.Content.Contains("There is already a value with this name created for this Service Type"))
                    //////{
                    //////    Console.WriteLine("No se creará registro para este producto, hasta eliminar el que ya existe.");
                    //////    Console.WriteLine("Por favor elimine manualmente el que tiene el nombre : " + "GEX 1Y " + skuId.ToString() + " " + skuId.ToString("X"));

                    //////    //Se elimina el registro existente
                    //////    DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();
                    //////    var itemFound = appDbContext.skuServiceValue.Where(x => x.Name == "GEX 1Y " + skuId.ToString() + " " + skuId.ToString("X")).FirstOrDefault();
                    //////    if(itemFound != null)
                    //////    {
                    //////        Console.WriteLine("Encontrado. Id:" + itemFound.Id.ToString());
                    //////        //DE VTEX
                    //////        await skuServiceValue.DeleteAsync(itemFound.Id.ToString());

                    //////        //DE LA BASE DE DATOS
                    //////        appDbContext.skuServiceValue.Remove(itemFound);
                    //////        await appDbContext.SaveChangesAsync();

                    //////        //Se crea el nuevo servicio
                    //////        newSkuServiceResponse = await skuServiceValue.PostAsyncResponse(new VtexStrucsGen2.Strucs.Vtex.SkuServiceValue()
                    //////        {
                    //////            Name = "GEX 1Y " + skuId.ToString() + " " + skuId.ToString("X"),
                    //////            Cost = (decimal) newCost,
                    //////            Value = (decimal) newValue,
                    //////            SkuServiceTypeId = SkuServiceTypeId,
                    //////        });

                    //////        if (newSkuServiceResponse.Content.Contains("There is already a value with this name created for this Service Type"))
                    //////        {
                    //////            Console.WriteLine("NIVEL 2 DE ERROR. Id:");
                    //////            Console.WriteLine("No se creará registro para este producto, hasta eliminar el que ya existe.");
                    //////            Console.WriteLine("Por favor elimine manualmente el que tiene el nombre : " + "GEX 1Y " + skuId.ToString() + " " + skuId.ToString("X"));
                    //////        }
                    //////        //return;
                    //////    }
                    //////}

                    ////////Si la creación fue correcta
                    //////if (newSkuServiceResponse != null)
                    //////{
                    //////    VtexStrucsGen2.Strucs.Vtex.SkuServiceValue newSkuServiceResult = new VtexStrucsGen2.Strucs.Vtex.SkuServiceValue();
                    //////    newSkuServiceResult = JsonConvert.DeserializeObject<VtexStrucsGen2.Strucs.Vtex.SkuServiceValue>(newSkuServiceResponse.Content);
                    //////    VtexStrucsGen2.Strucs.Vtex.SkuServicePost skuServicePost = new VtexStrucsGen2.Strucs.Vtex.SkuServicePost();

                    //////    //Id = 0,
                    //////    skuServicePost.SkuServiceTypeId = SkuServiceTypeId;
                    //////    skuServicePost.SkuServiceValueId = newSkuServiceResult.Id;
                    //////    skuServicePost.SkuId = skuId;
                    //////    //skuServicePost.Name = "GEX 1Y " + skuId.ToString() + " " + skuId.ToString("X");
                    //////    //skuServicePost.Text = "GEX 1Y " + skuId.ToString() + " " + skuId.ToString("X");

                    //////    skuServicePost.Name = "Garantía Extendida 1 año";// + skuId.ToString() + " " + skuId.ToString("X"); //Nombre mostrado en carrito
                    //////    skuServicePost.Text = "Garantía Extendida 1 año";

                    //////    skuServicePost.IsActive = true;

                    //////    //Crear un nuevo registro de servicio
                    //////    var newServiceResult = await skuService.PostAsync(skuServicePost);

                    //////    //------------------------------------------------------------------
                    //////    //Se agrega nuevo servicio a la tabla
                    //////    Entidades.SyncTask.SkuServiceValue skuServiceValueItem2 = new Entidades.SyncTask.SkuServiceValue();
                    //////    skuServiceValueItem2.SkuServiceTypeId = skuServicePost.SkuServiceTypeId;
                    //////    skuServiceValueItem2.Value = newSkuServiceResult.Value;
                    //////    skuServiceValueItem2.Name = newSkuServiceResult.Name;
                    //////    skuServiceValueItem2.Cost = newSkuServiceResult.Cost;
                    //////    skuServiceValueItem2.Id = newSkuServiceResult.Id;
                    //////    try
                    //////    {
                    //////        DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();
                    //////        var itemFoundExists = appDbContext.skuServiceValue.
                    //////            Where(x => x.Name == newSkuServiceResult.Name).
                    //////            Where(x => x.SkuServiceTypeId == newSkuServiceResult.SkuServiceTypeId).
                    //////            Where(x => x.Value == newSkuServiceResult.Value).FirstOrDefault();

                    //////        if (itemFoundExists == null)
                    //////        {
                    //////            await appDbContext.skuServiceValue.AddAsync(skuServiceValueItem2);
                    //////            await appDbContext.SaveChangesAsync();
                    //////        }
                    //////    }
                    //////    catch (Exception e)
                    //////    {
                    //////        Console.WriteLine("Error al insertar: " + e.Message);
                    //////    }
                    //////}                    
                }
            }
            
            //if (IdSkuServicoValor != 0)
            //if(ServiceIdForUpdate == 0)
            //{
            //    //Ingresa una nuevo registro
            //    var skuServiceResultAsign = await skuService.PostAsync<VtexStrucsGen2.Strucs.Vtex.SkuService>(new VtexStrucsGen2.Strucs.Vtex.SkuService()
            //    {
            //        Id = 0,
            //        //SkuId = skuId,
            //        SkuServiceTypeId = SkuServiceTypeId,
            //        //SkuServiceValueId = IdSkuServicoValor,
            //        Name = "Servicio de ensamblaje",
            //        Text = "Servicio de ensamblaje",
            //        IsActive = true
            //    });
            //}


            //product.Services
        }
    }
}
