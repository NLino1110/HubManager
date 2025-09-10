//using EmailQueue;
using DataSourceManager;
using DataSourceManager.MySql;
using DataSourceManager.MySql.Struct;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Math;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
//using Vtex_Tools.Code.Tools;
using VtexStrucs;
using VtexStrucs.Comm;
using VtexStrucs.Strucs;

namespace ResourceBuilder.Services.Sync
{
    public partial class Processor
    {
        public string for_test_id = "";
        public string[] allSkus_for_test = null;
        public string whereForTest = "";

        //private string ref_id_for_test = "16226.0911";
        //private string sku_for_test = "939";

        //private string for_test_id = $"AND Codigo_Siscom  like '%23140.192%'";
        //private string[] allSkus_for_test = new string[] { "5628" };
        //private string whereForTest = " and prod_vtex_sku = 5628";

        public int completados = 0;
        public int multiplicador = 50;
        public decimal numproc;
        public List<Siscom2vTexMatchProduct> Lista = new List<Siscom2vTexMatchProduct>();
        //public List<proceso> Procesos = new List<proceso>();
        //public vTex_Account cuenta;
        //public DataManager data;
        //string[] allSkus;        
        //List<ProductSync> items_up;

        List<VtexStrucs.Strucs.Supplier> suppliers =  new List<VtexStrucs.Strucs.Supplier>();

        //VtexFixer
        List<ProductSync> productSync_up;
        public List<proceso> procForFix = new List<proceso>();
        public decimal numproc_up;

        public VtexStrucs.vTex_Account mAccount;

        //public vTex_Account mAccount;
        private readonly AppDbContext _appDbContext;
        private readonly MySqlDbContext _mysqlDbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Processor(MySqlDbContext context, 
            AppDbContext appContext, 
            IWebHostEnvironment webHostEnvironment)
        {
            _mysqlDbContext = context;
            _webHostEnvironment = webHostEnvironment;
            _appDbContext = appContext;
        }

        public void SetAccount(vTex_Account _mAccount)
        {
            mAccount = _mAccount;
        }

        //public Processor(vTex_Account mCuenta, DataManager mData)
        //{
        //    //obtiene los datos de vtex
        //    cuenta = mCuenta;
        //    data = mData;

        //    mAccount = new VtexStrucs.Comm.Account();
        //    mAccount.cac_appkey = mCuenta.vTexApiKey;
        //    mAccount.cac_apptoken = mCuenta.vTexApiSecret;
        //    mAccount.cac_name = mCuenta.Account_Name;
        //    //mAccount.cac_name = mCuenta.Internal_Account_Name;
        //    mAccount.cac_environment = "vtexcommercestable";
        //    mAccount.cac_id = 0;            
        //}

        public async Task<bool> VtexDataFixer()
        {
            //LogManager.Log().LogInformation("Iniciando VtexDataFixer " + mAccount.cac_name + " " + DateTime.Now);
            var data = _mysqlDbContext.product_sync.Where(d => d.account_name == mAccount.Account_Name && d.prod_status == 1).ToList();

            //CoreResponse coreResponse = await product.GetAllForFix(mAccount.cac_name, whereForTest);

            if (data != null)
            {
                //CASE
                //    WHEN `prod_active`= 1 THEN 1
                //    WHEN `prod_active`= 0 THEN 0
                //    ELSE 0
                //END `prod_active`,

                //CASE
                //    WHEN `prod_active_up`= 1 THEN 1
                //    WHEN `prod_active_up`= 0 THEN 0
                //    ELSE 0
                //END `prod_active_up`,

                //items_up = (List<dynamic>) coreResponse.data;

                //Deserialización correcta
                productSync_up = data;
                //productSync_up = new List<Entidades.SyncTask.ProductSync>();
                //productSync_up = JsonConvert.DeserializeObject<List<Entidades.SyncTask.ProductSync>>(JsonConvert.SerializeObject(data));

                int countItems = productSync_up.Count;

                if (countItems > multiplicador)
                    numproc_up = Math.Ceiling((decimal)countItems / multiplicador);
                else
                    numproc_up = countItems + 1;

                //Task[] taskArray = new Task[(int) numproc];

                List<Task> taskArray = new List<Task>();
                for (int i = 0; i < numproc_up; i++)
                {
                    bool lastGroup = false;
                    int proc = i + 1;

                    if (!((i + 1) < numproc_up))
                        lastGroup = true;

                    taskArray.Add(Task.Factory.StartNew(() => ProcessGroup_Up(proc, mAccount, lastGroup, countItems)));
                }

                Task.WaitAll(taskArray.ToArray());
            }

            //LogManager.Log().LogInformation("Terminado VtexDataFixer " + mAccount.cac_name);
            return true;
        }

        public async void ProcessGroup_Up(int proc, vTex_Account cuenta, bool lastGroup, int countItems)
        {
            int multiplicador_m = multiplicador;
            int ilFinal = proc * multiplicador_m;

            if (proc * multiplicador < countItems)
            {
                multiplicador_m = 1;
            }

            if (countItems - proc * multiplicador < 50) // || proc * multiplicador - allSkus.Length < 50)
            {
                ilFinal = countItems - 1;
            }

            for (int il = (proc * multiplicador - multiplicador); il <= ilFinal; il++)
            {                
                ProductSync productSyncCurrent = productSync_up[il];

                //Producto excluido de analisis (Hogar Protegido)
                if (productSyncCurrent.prod_vtex_sku == 5952)
                {
                    continue;
                }

                decimal markup = 0;
                
                if(productSyncCurrent.prod_disc_up != null)
                    markup = productSyncCurrent.prod_disc_up.Value;

                //Add here the optimizer code
                VtexStrucs.Comm.Products prod = new VtexStrucs.Comm.Products(mAccount);

                if (productSyncCurrent.prod_pvp != productSyncCurrent.prod_pvp_up || //Precio por cambiar
                        productSyncCurrent.prod_stock != productSyncCurrent.prod_stock_up ||
                        productSyncCurrent.prod_disc != productSyncCurrent.prod_disc_up ||
                        productSyncCurrent.prod_costo != productSyncCurrent.prod_costo_up ||
                        productSyncCurrent.prod_pvp_ant != productSyncCurrent.prod_pvp_ant_up ||
                        productSyncCurrent.prod_active != productSyncCurrent.prod_active_up
                        ) //Stock por cambiar
                {
                    //evaluar los campos que se van a modificar
                    if (productSyncCurrent.prod_pvp != productSyncCurrent.prod_pvp_up || productSyncCurrent.prod_disc != productSyncCurrent.prod_disc_up ||
                        productSyncCurrent.prod_costo != productSyncCurrent.prod_costo_up || productSyncCurrent.prod_pvp_ant != productSyncCurrent.prod_pvp_ant_up)
                    {
                        decimal basePrice = 0;
                        decimal listPrice = 0;
                        decimal costPrice = 0;

                        decimal currentPrice = 0;

                        if (productSyncCurrent.prod_pvp_up != null)
                        {
                            currentPrice = productSyncCurrent.prod_pvp_up.Value;
                        }

                        if (productSyncCurrent.prod_disc_up != 0)
                        {
                            //listPrice = productSyncCurrent.prod_pvp_up.Value; //Se mantiene el precio de venta para
                            //                                                  //que se realice el calculo por medio de markup
                            //costPrice = productSyncCurrent.prod_pvp_up.Value;

                            //Promoción con porcentaje de descuento
                            if (productSyncCurrent.prod_pvp_ant_up != null)
                            {
                                listPrice = productSyncCurrent.prod_pvp_ant_up.Value; //linea modificada 3/2/2022 15:33
                                costPrice = productSyncCurrent.prod_pvp_ant_up.Value;
                            }

                            //Calculo de descuento, nuevo basePrice (aunque no se utiliza para actualizar en vtex)
                            if (productSyncCurrent.prod_pvp_up != null)
                            {
                                basePrice = productSyncCurrent.prod_pvp_up.Value;
                                //basePrice = Math.Round(basePrice - ((basePrice * markup) / 100), 2);
                            }

                            markup = -markup;
                        }
                        else
                        {
                            //Promoción aplicada directamente al precio                                
                            //markup = prod_disc_up;

                            listPrice = productSyncCurrent.prod_pvp_ant_up.Value;
                            costPrice = 0;
                            if (productSyncCurrent.prod_pvp_up != null)
                            {
                                costPrice = productSyncCurrent.prod_pvp_up.Value;  //costPrice en Vtex es el pvp
                            }
                        }

                        //
                        var responseSetValue = await prod.setNewPrice(productSyncCurrent.prod_vtex_sku, listPrice, costPrice, basePrice, markup);

                        //03/08/2022
                        //TODO: Quitar comentario en producción
                        // Porción de Código agregado para realizar el proceso de sincronización de Servicio de Garantía Extendida
                        //VtexStrucs.Strucs.ProductFull prodItem = products.getBySkuIdFull(skuItem_search).Result;
                        if (cuenta.Account_Name == "bosque")
                        {
                            //Lee la configuracion para saber si debe sincronizar los servicios
                            // por ahora solo sincronizaría Garantía Extendida
                            bool SyncServices = true;
                            
                            //TODO: SETTING
                            //bool.TryParse(ConfigurationHelper.GetValue("AppSettings:AutoUpdater:SyncInEnabled"), out SyncServices);

                            if (SyncServices)
                            {
                                //await EvalService(mAccount, productSyncCurrent.prod_vtex_sku, costPrice, basePrice, productSyncCurrent.prod_mkplace);
                                await EvalService(mAccount, productSyncCurrent.prod_vtex_sku, currentPrice, currentPrice, productSyncCurrent.prod_mkplace);
                                await EvalServiceEnsamblaje(mAccount, 
                                    productSyncCurrent.prod_vtex_sku, 
                                    currentPrice, 
                                    currentPrice, 
                                    productSyncCurrent.prod_mkplace, 
                                    productSyncCurrent.prod_ensamble);
                            }
                        }
                        //(int prod_vtex_sku, decimal listPrice, decimal costPrice, decimal basePrice, decimal markup)
                    }

                    if (productSyncCurrent.prod_stock_up != null)
                    {
                        if (productSyncCurrent.prod_stock != productSyncCurrent.prod_stock_up)
                        {
                            await prod.setNewStock(productSyncCurrent.prod_vtex_sku, (int)productSyncCurrent.prod_stock_up.Value);
                        }
                    }
                    else
                    {
                        productSyncCurrent.prod_stock_up = 0;
                    }

                    if (productSyncCurrent.prod_costo_up == null)
                    {                        
                        productSyncCurrent.prod_costo_up = 0;
                    }

                    if (productSyncCurrent.prod_costo == null)
                    {
                        productSyncCurrent.prod_costo = 0;
                    }

                    if (productSyncCurrent.prod_active != productSyncCurrent.prod_active_up)
                    {
                        Product productMod = new Product();
                        //Se obtiene estructura del producto para poder luego ser enviada al API                        
                        productMod = await prod.getBySkuIdMain(productSyncCurrent.productid.ToString());

                        productMod.IsActive = productSyncCurrent.prod_active_up == 1 ? true : false;
                                                
                        prod.UpdateProdData(productMod);
                    }
                    
                    if (
                        (productSyncCurrent.prod_vtex_id.ToUpper().EndsWith("_W") || productSyncCurrent.prod_vtex_id.ToUpper().EndsWith("-W")) && 
                        (productSyncCurrent.prod_costo != productSyncCurrent.prod_costo_up)
                        )
                    {
                        //prod.setNewStock(prod_vtex_sku, prod_stock_up);
                        //Envío de EMAIL
                        // Agregar a la cola de envío / enviar directamente
                        //string email_body = "Costo de producto modificado, " +
                        //    productSyncCurrent.prod_name + " " +
                        //        "código:" + productSyncCurrent.prod_vtex_id +
                        //        ", sku: " + productSyncCurrent.prod_vtex_sku +
                        //        ", nuevo costo: " + productSyncCurrent.prod_costo_up.ToString();
                        //GondorCalls(email_body);Send
                        
                        decimal max_cost = Math.Max(productSyncCurrent.prod_costo.Value, productSyncCurrent.prod_costo_up.Value);
                        decimal min_cost = Math.Min(productSyncCurrent.prod_costo.Value, productSyncCurrent.prod_costo_up.Value);
                        decimal diff_cost = Math.Abs(max_cost - min_cost);

                        decimal percent_cost = max_cost * 10 / 100;

                        if (diff_cost > percent_cost)
                        {
                            //La diferencia entre el nuevo costo y el costo anterior es de mas del 10%
                            // se genera alerta o se prepara para confirmación
                            //stuck for aproveed
                            //SendEmail(
                            //    "Cambio en costo de producto" + productSyncCurrent.prod_vtex_id + " " +
                            //    productSyncCurrent.prod_name,
                            //    "La diferencia entre el nuevo costo y el costo anterior es de mas del 10%, " +
                            //    productSyncCurrent.prod_vtex_id + " " +
                            //    productSyncCurrent.prod_name);
                        }

                        if (productSyncCurrent.prod_costo_up.Value > 0)
                        {
                            //TODO: Cambio hacia ORACLE (Aqui no aplica)
                            //TODO: REVISAR OPCION DE CAMBIO DE COSTO
                            //ProductDb productDb = new ProductDb(data);
                            //productDb.UpdateCost(
                            //    productSyncCurrent.prod_vtex_id,
                            //    productSyncCurrent.prod_costo_up);
                        }
                    }

                    //actualizar a la tabla para que se desmarque
                    ProductFull productFull = new ProductFull();

                    //Data.Mysql.Product<object> productChange = new Data.Mysql.Product<object>();
                    var productChange = _mysqlDbContext.product_sync.Where(d => 
                    d.prod_vtex_id == productSyncCurrent.prod_vtex_id &&
                    d.prod_vtex_sku == productSyncCurrent.prod_vtex_sku)
                        .FirstOrDefault();

                    productFull.AlternateIds = new AlternateIds();
                    productFull.AlternateIds.RefId = productSyncCurrent.prod_vtex_id;
                    productFull.Id = productSyncCurrent.prod_vtex_sku;
                    productFull.account_name = mAccount.Account_Name;

                    if (productSyncCurrent.prod_pvp_up == null ||
                        productSyncCurrent.prod_pvp_ant_up == null ||
                        productSyncCurrent.prod_disc_up == null)
                    {
                        decimal prod_pvp_ant = 0;

                        if (productSyncCurrent.prod_pvp_ant == null)
                        {
                            prod_pvp_ant = 0;
                        }
                        else
                        {
                            prod_pvp_ant = productSyncCurrent.prod_pvp_ant.Value;
                        }

                        //Temporalmente se colocan los valores para que se iguales
                        // aquí caen los de Marketplace
                        // null = en ciertos campos
                        //await productChange.SetFixed(productFull,
                        //    productSyncCurrent.prod_pvp.Value,
                        //    (int)productSyncCurrent.prod_stock_up.Value,
                        //    productSyncCurrent.prod_disc.Value,
                        //    productSyncCurrent.prod_costo_up.Value,
                        //    prod_pvp_ant,
                        //    productSyncCurrent.prod_active_up.Value
                        //    );

                        productChange.prod_pvp = productSyncCurrent.prod_pvp;
                        productChange.prod_stock = productSyncCurrent.prod_stock_up;
                        productChange.prod_disc = productSyncCurrent.prod_disc;
                        productChange.prod_costo = productSyncCurrent.prod_costo_up;
                        productChange.prod_pvp_ant = prod_pvp_ant;
                        productChange.prod_active = productSyncCurrent.prod_active_up;
                        productChange.prod_status = 0;
                        productChange.write_date = DateTime.Now;
                        _mysqlDbContext.SaveChanges();
                    }
                    else
                    {
                        //await productChange.SetFixed(productFull,
                        //    productSyncCurrent.prod_pvp_up.Value,
                        //    (int)productSyncCurrent.prod_stock_up.Value,
                        //    productSyncCurrent.prod_disc_up.Value,
                        //    productSyncCurrent.prod_costo_up.Value,
                        //    productSyncCurrent.prod_pvp_ant_up.Value,
                        //    productSyncCurrent.prod_active_up.Value
                        //    );

                        productChange.prod_pvp = productSyncCurrent.prod_pvp;
                        productChange.prod_stock = productSyncCurrent.prod_stock_up;
                        productChange.prod_disc = productSyncCurrent.prod_disc_up;
                        productChange.prod_costo = productSyncCurrent.prod_costo_up;
                        productChange.prod_pvp_ant = productSyncCurrent.prod_pvp_ant_up;
                        productChange.prod_active = productSyncCurrent.prod_active_up;
                        productChange.prod_status = 0;
                        productChange.write_date = DateTime.Now;
                        _mysqlDbContext.SaveChanges();
                    }
                }
                else
                {
                    //Está en la lista pero no tiene valores distintos, se le cambia solo el estado                    
                    ProductFull productFull = new ProductFull();
                    
                    //Data.Mysql.Product<object> productChange = new Data.Mysql.Product<object>();

                    productFull.AlternateIds = new AlternateIds();
                    productFull.AlternateIds.RefId = productSyncCurrent.prod_vtex_id;
                    productFull.Id = productSyncCurrent.prod_vtex_sku;
                    productFull.account_name = mAccount.Account_Name;
                    //await productChange.SetFixedNoChanges(productFull);

                    var productChange = _mysqlDbContext.product_sync.Where(d =>
                    d.prod_vtex_id == productSyncCurrent.prod_vtex_id &&
                    d.prod_vtex_sku == productSyncCurrent.prod_vtex_sku)
                        .FirstOrDefault();

                    productChange.prod_status = 0;
                    productChange.write_date = DateTime.Now;
                    _mysqlDbContext.SaveChanges();
                }
            }
        }

        private string GetSelectCommand()
        {
            string SelectCommand = "";
            string fieldSkuName = "";
            string stockName = "";
            string almNum = "";

            if (mAccount.Account_Name.ToLower().Equals("dmujeres"))
            {
                SelectCommand = @"SELECT * FROM (
                     Select
                            ID_PROMOCION as id_promocion,
                            vtex_sku_id AS skuId,  
                            prod_codigo as Codigo_Siscom, 
                            prod_nombre as Nombre_Siscom, 
                            prod_pvp as pvp, 
                            q_vtex.precio_promocion(prod_codigo) as pvp_final, 
                            nvl(q_vtex.stock(prod_codigo, 'BOSQUE'), 0) AS stockSiscom,
                            nvl(PROMOCION,'-') AS per_name,
                            nvl(PORCENTAJE, 0) AS per_discount,
                            prod_costult,
                            prod_costpact AS costPrice,
                            FECHAINICIO,
                            PROD_CODIGO_PROV AS prod_cod_prov,
                            ESPROMOCIONPRECIO AS es_promo_precio,
                            APLICAVTEX AS aplicavtex,
                            PRECIO_PROMOCION AS precio_promocion,
                            prod_pvp as precio_lista,
                            PROD_MARKETPLACE,
                            PROD_ENSAMBLE
                            from producto
                            LEFT JOIN (SELECT CAB.ID_PROMOCION , PROMOCION, 
                     CODPRODUCTO, PORCENTAJE ,
                     CAB.FECHAINICIO,CAB.HORAINICIO,
                     CAB.FECHAFINAL,CAB.HORAFINAL,
                     CAB.ESTADO,
                     CAB.ESPROMOCIONPRECIO,
                     CAB.APLICAVTEX,                     
                     PRO.PRECIO_PROMOCION,
                     PRO.PRECIO_LISTA
                     FROM PROMOCION_PRODUCTO PRO
                     JOIN PROMOCION_CABECERA CAB ON
			            PRO.ID_PROMOCION = CAB.ID_PROMOCION
                        AND CAB.TIPO_PROMO = 1
			            AND PRO.ESTADO = 'A'
			            AND ( 
			            CURRENT_DATE BETWEEN TO_DATE(TO_CHAR(CAB.FECHAINICIO,'MM/DD/YYYY') || ' ' || CAB.HORAINICIO, 'MM/DD/YYYY hh24:mi:ss')   AND 
			            TO_DATE(TO_CHAR(CAB.FECHAFINAL ,'MM/DD/YYYY') || ' ' || CAB.HORAFINAL , 'MM/DD/YYYY hh24:mi:ss') AND 
			            CAB.APLICAVTEX = 'S'
		            )
		            AND CAB.ESTADO = 'A'
                     JOIN PROMOCION_ALMACENES ALM ON
			            PRO.ID_PROMOCION = ALM.ID_PROMOCION
                        AND ALM.ESTADO = 'A'
		            AND CODALMACEN = 104) prod_disc on
			            PROD_CODIGO = CODPRODUCTO		
                    where prod_estado='A'                        
                    ) " + for_test_id +
                            " order by skuId, per_discount desc";
            }


            if (mAccount.Account_Name.ToLower().Equals("tempodesign"))
            {
                SelectCommand = @"SELECT * FROM (
                     Select
                            ID_PROMOCION as id_promocion,
                            vtex_sku_tempo_id AS skuId,  
                            prod_codigo as Codigo_Siscom, 
                            prod_nombre as Nombre_Siscom, 
                            prod_pvp as pvp, 
                            q_vtex.precio_promocion(prod_codigo) as pvp_final, 
                            nvl(q_vtex.stock(prod_codigo, 'TEMPO'), 0) AS stockSiscom,
                            nvl(PROMOCION,'-') AS per_name,
                            nvl(PORCENTAJE, 0) AS per_discount,
                            prod_costult,
                            prod_costpact AS costPrice,
                            FECHAINICIO,
                            PROD_CODIGO_PROV AS prod_cod_prov,
                            ESPROMOCIONPRECIO AS es_promo_precio,
                            APLICAVTEX AS aplicavtex,
                            PRECIO_PROMOCION AS precio_promocion,
                            prod_pvp as precio_lista,
                            PROD_MARKETPLACE,
                            PROD_ENSAMBLE
                            from producto
                            LEFT JOIN (SELECT CAB.ID_PROMOCION , PROMOCION, 
                     CODPRODUCTO, PORCENTAJE ,
                     CAB.FECHAINICIO,CAB.HORAINICIO,
                     CAB.FECHAFINAL,CAB.HORAFINAL,
                     CAB.ESTADO,
                     CAB.ESPROMOCIONPRECIO,
                     CAB.APLICAVTEX,                     
                     PRO.PRECIO_PROMOCION,
                     PRO.PRECIO_LISTA
                     FROM PROMOCION_PRODUCTO PRO
                     JOIN PROMOCION_CABECERA CAB ON
			            PRO.ID_PROMOCION = CAB.ID_PROMOCION
                        AND CAB.TIPO_PROMO = 1
			            AND PRO.ESTADO = 'A'
			            AND ( 
			            CURRENT_DATE BETWEEN TO_DATE(TO_CHAR(CAB.FECHAINICIO,'MM/DD/YYYY') || ' ' || CAB.HORAINICIO, 'MM/DD/YYYY hh24:mi:ss')   AND 
			            TO_DATE(TO_CHAR(CAB.FECHAFINAL ,'MM/DD/YYYY') || ' ' || CAB.HORAFINAL , 'MM/DD/YYYY hh24:mi:ss') AND 
			            CAB.APLICAVTEX = 'S'
		            )
		            AND CAB.ESTADO = 'A'
                     JOIN PROMOCION_ALMACENES ALM ON
			            PRO.ID_PROMOCION = ALM.ID_PROMOCION
                        AND ALM.ESTADO = 'A'
		            AND CODALMACEN = 105) prod_disc on
			            PROD_CODIGO = CODPRODUCTO		
                    where prod_estado='A'                        
                    ) " + for_test_id +
                            " order by skuId, per_discount desc";
            }

            if (mAccount.Account_Name.ToLower().Equals("tempodesignpan"))
            {
                SelectCommand = @"SELECT 
	                vtex_sku_tempo_id AS skuId,  
	                prod_nombre as Nombre_Siscom,
	                TRNP_NUMERO id_promocion, 
	                CMPP_FECINI FECHAINICIO,
	                CMPP_FECFIN FECHAFINAL,
	                TRNP_CODPROD Codigo_Siscom, 
                    prod_pvp as pvp, 
	                TRNP_PVP precio_lista, 
	                TRNP_PVPNUEVO, 
	                TRNP_DESCPROM per_discount, 
	                TRNP_CANTIDAD , 
	                TRNP_PRECIO_PRECIADOR precio_promocion,
	                SISCOMPA.q_vtex.precio_promocion(PRO.prod_codigo) as pvp_final, 
	                SISCOMPA.q_vtex.stock( TRNP_CODPROD, 'TEMPODESIGNPAN') AS stockSiscom,
	                'S' aplicavtex,
                    'N' es_promo_precio,
                    'N' PROD_MARKETPLACE,
	                PC.CMPP_ALMPROM
	                from SISCOMPA.PRODUCTO PRO
	                JOIN SISCOMPA.PRE_TRN PT ON
	                 TRNP_CODPROD = PRO.PROD_CODIGO 
	                JOIN SISCOMPA.PRE_CMP PC ON
		                PT.TRNP_NUMERO = PC.CMPP_NUMERO
	                    AND PC.CMPP_PRMMPROM = 'DP'
		                AND PC.CMPP_STATCMP = 'I'
	                    AND PC.CMPP_FECFIN BETWEEN TRUNC(TO_DATE(SYSDATE, 'DD-MM-YY')) AND TRUNC(TO_DATE(CMPP_FECFIN, 'DD-MM-YY')) + 0.99999
	                    AND PC.CMPP_ALMPROM like '%8%' " + for_test_id +
                                            " order by skuId, per_discount desc";
            }

            //SelectCommand = Regex.Replace(SelectCommand, @"\t|\n|\r", "");
            return SelectCommand;
        }

        ////public async Task<bool> QueueAnalyzer()
        ////{
        ////    //LogManager.Log().LogInformation("Iniciando Procesar");
        ////    Console.WriteLine("Iniciando Procesar");

        ////    string SelectCommand = GetSelectCommand();

        ////    int count = 0;
        ////    completados = 0;
        ////    DataTable dt = null; // data.ExecuteDataTable(SelectCommand);

        ////    if (dt != null)
        ////    {
        ////        foreach (DataRow row2 in dt.Rows)
        ////        {
        ////            count++;
        ////            Siscom2vTexMatchProduct ent = new Siscom2vTexMatchProduct();
        ////            ent.NumeroFila = count;
        ////            SiscomDataTransport.RowToEntidad(row2, ent);
        ////            //if (ent.Codigo_Siscom.Contains(ref_id_for_test))
        ////            //{
        ////            //Console.WriteLine("Encontrado!");
        ////            //}

        ////            //Permite pasar por alto las promociones con nombre que contengan
        ////            // la palabra COMBO
        ////            //if(row2["PROMOCION"].ToString().ToLower().Contains("combo") || 
        ////            //    row2["PROMOCION"].ToString().ToLower().Contains("combos"))
        ////            //{
        ////            //    continue;
        ////            //}

        ////            Lista.Add(ent);
        ////        }
        ////    }

        ////    //foreach (Siscom2vTexMatchProduct item in Lista)
        ////    //{
        ////    //    VtexStrucs.Comm.Products prodM = new VtexStrucs.Comm.Products(mAccount);
        ////    //    string itemSku = prodM.getSkuByRefId(item.Codigo_Siscom.Trim());
        ////    //}
            
        ////    //Paso 1 --Leemos todos los SKUS disponibles para la cuenta especificada
        ////    allSkus = await getDataVtexAllSkus();

        ////    if(allSkus == null)
        ////    {
        ////        //LogManager.Log().LogError("Procesar - Error al obtener listado de SKUs (terminado antes de tiempo)");
        ////        Console.WriteLine("Procesar - Error al obtener listado de SKUs (terminado antes de tiempo)");
        ////        return false;
        ////    }

        ////    //allSkus = new string[] { "2876" };
        ////    //allSkus = new string[] { "75" };
        ////    if (allSkus_for_test != null)
        ////    {
        ////        allSkus = allSkus_for_test;
        ////    }

        ////    if (allSkus.Length > multiplicador)
        ////        numproc = Math.Ceiling((decimal) allSkus.Length / multiplicador);
        ////    else
        ////        numproc = allSkus.Length;

        ////    //Task[] taskArray = new Task[(int) numproc];
            
        ////    List<Task> taskArray = new List<Task>();
        ////    for (int i = 0; i < numproc; i++)
        ////    {
        ////        bool lastGroup = false;
        ////        int proc = i;

        ////        if (!((i + 1) < numproc))
        ////            lastGroup = true;

        ////        //taskArray.Add(await Task.Factory.StartNew(async () => await ProcessGroup(proc, cuenta, lastGroup)));
        ////        taskArray.Add(Task.Factory.StartNew(() => ProcessGroup(proc, mAccount, lastGroup)));                
        ////        //break;
        ////    }
            
        ////    Task.WaitAll(taskArray.ToArray());

        ////    //Task t = Task.WhenAll(taskArray);
        ////    //try
        ////    //{
        ////    //    t.Wait();
        ////    //}
        ////    //catch { }

        ////    //LogManager.Log().LogInformation("Terminado Procesar");
        ////    Console.WriteLine("Terminado Procesar");
        ////    return true;

        ////    //Task<int>[] taskArray = new Task<int>[(int)numproc];
        ////    //for (int i = 0; i < numproc; i++)
        ////    //{
        ////    //    int proc = i + 1;
        ////    //    taskArray[i] = Task<int>.Factory.StartNew(() => getDataVtex(proc, cuenta));
        ////    //    //   taskArray[i].Wait();
        ////    //}

        ////    //Task.WaitAll(taskArray);            
        ////}

        public async Task ProcessGroup(int proc, vTex_Account cuenta, bool lastGroup, string[] allSkus)
        {
            //Console.WriteLine("Iniciado PC");

            //foreach (var ent in Lista.Where(x => x.NumeroFila >= (proc * multiplicador - multiplicador) && x.NumeroFila <= proc * multiplicador))
            //{

            //}
            
            int multiplicador_m = multiplicador;
            int ilFinal = (proc + 1 ) * multiplicador;

            if (proc * multiplicador < allSkus.Length)
            {
                multiplicador_m = 1;
            }

            if(allSkus.Length - proc * multiplicador < 50) // || proc * multiplicador - allSkus.Length < 50)
            {
                ilFinal = allSkus.Length;
            }

            Console.WriteLine("Procesando desde " + (proc * multiplicador).ToString() + "     hasta: " + (ilFinal - 1).ToString());

            for (int il = (proc * multiplicador); il < ilFinal; il++)
            {
                //4253
                //if (allSkus[il] != "5350")
                //    continue;

                //Console.WriteLine(il);
                string itemSku = allSkus[il];

                //Producto excluido de analisis (Hogar Protegido)
                if(itemSku == "5952" && cuenta.Account_Name.ToLower() == "dmujeresec")
                {
                    continue;
                }

                //Console.WriteLine("Procesando SKU: " + itemSku + "     index: " + il.ToString());

                //if (itemSku == sku_for_test)
                //{
                //    Console.WriteLine("Sku encontrado en lista: solicitado-> " + itemSku);
                //}

                //if(true)
                //    continue;
                //if (il > 903 && il < 950)
                //{
                //    //945
                //    Console.WriteLine("Índice Encontrado! " + il.ToString());
                //}

                //new Task(async () => {
                //Data.Mysql.Product<object> product = new Data.Mysql.Product<object>();

                ProductFull prodData = new ProductFull();
                prodData.Id = int.Parse(itemSku);
                prodData.account_name = mAccount.Account_Name;

                int? SupplierId = 0;
                string currentRefId = "";

                bool blAlreadyExist = false;
                bool blInQueue = false;
                bool blDataChanged = false;
                decimal? prod_pvp_up = 0;
                int prod_stock_up = 0;
                decimal prod_disc = 0;
                decimal prod_disc_up = 0;
                decimal basePrice = 0;

                decimal prod_costo = 0;
                decimal prod_costo_up = 0;

                decimal prod_pvp_ant = 0;
                decimal prod_pvp_ant_up = 0;

                string prod_url_img_01 = "";

                DateTime aud_mod_date = DateTime.Now;

                var _mysqlDbContext_2 = new DataSourceManager.MySql.MySqlDbContext();

                bool hasData = _mysqlDbContext.product_sync.Any();

                var data = _mysqlDbContext.product_sync.Where(d => 
                d.account_name == prodData.account_name && 
                d.prod_vtex_sku == prodData.Id).ToList();

                //CoreResponse coreResponse = await product.GetBySkuAndAccount(prodData);

                //Cast de datos a clase ejecutado correctamente
                //List<Entidades.SyncTask.ProductSync> productSync_in = new List<Entidades.SyncTask.ProductSync>();
                //productSync_in = JsonConvert.DeserializeObject<List<Entidades.SyncTask.ProductSync>>(JsonConvert.SerializeObject(coreResponse.data));

                //if(productSync_in != null)
                if (data != null)
                {
                    //Deserialización correcta
                    //List<Entidades.SyncTask.ProductSync> productAsync = new List<Entidades.SyncTask.ProductSync>();
                    //productAsync = JsonConvert.DeserializeObject<List<Entidades.SyncTask.ProductSync>>(JsonConvert.SerializeObject(coreResponse.data));

                    //List<dynamic> items = (List<dynamic>)coreResponse.data;

                    //foreach( var itemObj in productSync_in)
                    foreach (var itemObj in data)
                    {
                        //Producto ya ingresado en la base de datos
                        //Console.WriteLine("Ya existente " + itemObj.prod_vtex_sku);
                        prodData.AlternateIds = new AlternateIds();
                        prodData.AlternateIds.RefId = itemObj.prod_vtex_id;
                        prodData.ProductName = itemObj.prod_name;
                        currentRefId = itemObj.prod_vtex_id;

                        if (itemObj.write_date is not null)
                            aud_mod_date = DateTime.Parse(itemObj.write_date.ToString());

                        if (itemObj.prod_prov is not null)
                            SupplierId = itemObj.prod_prov;

                        if (itemObj.prod_pvp is not null)
                            prod_pvp_up = itemObj.prod_pvp;

                        if (itemObj.prod_disc is not null)
                            prod_disc_up = (decimal) itemObj.prod_disc;

                        if (itemObj.prod_stock is not null)
                            prod_stock_up = (int) itemObj.prod_stock;

                        if (itemObj.prod_costo is not null)
                        {
                            prod_costo_up = (decimal) itemObj.prod_costo;
                        }
                        else
                        {
                            prod_costo_up = 0;
                        }

                        if (itemObj.prod_pvp_ant is not null)
                            prod_pvp_ant = (decimal) itemObj.prod_pvp_ant;

                        if (itemObj.prod_pvp_ant_up is not null)
                            prod_pvp_ant_up = (decimal) itemObj.prod_pvp_ant_up;

                        if (itemObj.prod_url_img_01 is not null)
                            prod_url_img_01 = itemObj.prod_url_img_01;

                        if (itemObj.prod_status is not null)
                            if (itemObj.prod_status == 1)
                            {
                                blInQueue = true;
                            }

                        blAlreadyExist = true;
                        //break;
                    }
                }


                //if MarketPlace PASS!!!
                if (prodData.AlternateIds != null && 
                    (prodData.AlternateIds.RefId.ToUpper().EndsWith("_W") ||
                    prodData.AlternateIds.RefId.ToUpper().EndsWith("-W")))
                {
                    //SALIR DEL PROCESO SI ES QUE ES MARKETPLACE
                    //Console.WriteLine("Marketplace encontrado " + prodData.AlternateIds.RefId.ToUpper());
                    //continue;
                    //prodData.AlternateIds = null;
                }

                if (il > 903 && il < 950)
                {
                    //945
                    //Console.WriteLine("Índice Encontrado L2! " + il.ToString());
                }

                //Algoritmo de analisis para posibles cambios de los datos locales
                // en caso de que el registro no exista
                // en caso de que no encuentr el productId
                // en caso de que no se haya modificado hace mucho tiempo (3 días)
                VtexStrucs.Comm.Products prod = new VtexStrucs.Comm.Products(mAccount);

                if (!blAlreadyExist || 
                    prodData.ProductId == 0 || 
                    (DateTime.Now - aud_mod_date).TotalDays > 3 ||
                    prod_url_img_01 == "")
                {
                    //if ((DateTime.Now - aud_mod_date).TotalDays > 3 && (DateTime.Now - aud_mod_date).TotalDays < 10)
                    //{
                    //    Console.WriteLine((DateTime.Now - aud_mod_date).TotalDays);
                    //}

                    if (prodData.ProductId == 0)
                    {
                        blDataChanged = true;
                    }

                    string oldRefId = "";
                    
                    if (blAlreadyExist)
                    {
                        oldRefId = prodData.AlternateIds.RefId;
                    }

                    prodData = await prod.getBySkuIdFull(itemSku);

                    //Producto de Vtex esta activo?
                    if(prodData.IsActive)
                    {

                    }
                    
                    prodData.account_name = mAccount.Account_Name;
                    prod_url_img_01 = prodData.ImageUrl;

                    if(prodData.Images != null && prodData.Images.Count>0)
                    {
                        prod_url_img_01 = prodData.Images[0].ImageUrl;
                    }

                    if (prodData.AlternateIds !=null && oldRefId != prodData.AlternateIds.RefId)
                    {
                        //Console.WriteLine("RefId modificado o producto no existe en la base");
                        blDataChanged = true;
                    }
                }

                Product prodDataWithSupplier = null;

                if (SupplierId == 0)
                {
                    if (prodData.ProductId > 0)
                    {
                        prodDataWithSupplier = await prod.getBySkuIdMain(prodData.ProductId.ToString());
                    }

                    if (prodDataWithSupplier != null && prodDataWithSupplier.SupplierId != null)
                    {
                        SupplierId = prodDataWithSupplier.SupplierId;
                        if (SupplierId.HasValue)
                        {
                            //suppliers = (suppliers.Distinct()).ToList();
                            //suppliers.Add(new Supplier { Id = SupplierId.Value });
                        }
                    }
                    else
                    {

                    }
                }

                //prodData.ProductName
                //prodData.AlternateIds.RefId

                var price = await prod.getPriceBySku(itemSku);
                var stock = await prod.getStockBySku(itemSku);

                if (prodData.AlternateIds != null && prodData.AlternateIds.RefId != "")
                {
                    try
                    {
                        //prod_pvp_up = price.listPrice;
                        prod_pvp_up = price.basePrice;
                        prod_pvp_ant_up = price.listPrice.Value;

                        prod_disc_up = Math.Abs(Math.Round(price.markup, 2));
                        price.markup = Math.Abs(price.markup);

                        if(price.listPrice != null)
                            prod_pvp_ant_up = price.listPrice.Value;
                        

                        if (stock.balance.Count > 0)
                            if (stock.balance[0].totalQuantity > 0)
                            {
                                //Console.WriteLine(stock.balance[0].totalQuantity);
                                prod_stock_up = stock.balance[0].totalQuantity;
                            }
                        
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Error al realizar cálculos " + itemSku + " " + prodData.AlternateIds.RefId);
                        Console.WriteLine(ex.Message);
                    }

                    //Insertar/Actualizar en la base de datos MySql
                    if (!blAlreadyExist && !blInQueue)
                    {
                        if (prodData.AlternateIds.RefId == null || prodData.AlternateIds.RefId == "")
                        {
                            prodData.AlternateIds.RefId = "0";
                        }
                        //Agrega nuevo producto
                        //var responseAdd = await product.Add(prodData, price, stock, SupplierId, prod_costo_up, prod_url_img_01);

                        ProductSync productSync = new ProductSync();
                        productSync.prod_vtex_id = prodData.AlternateIds.RefId;
                        productSync.prod_vtex_sku = prodData.Id;
                        productSync.account_name = prodData.account_name;
                        productSync.prod_name = prodData.ProductName;
                        productSync.prod_po = 0;
                        productSync.prod_pvp = price.costPrice;
                        productSync.prod_pvp_ant = price.listPrice;
                        productSync.prod_disc = price.markup;
                        productSync.prod_prov = SupplierId;
                        productSync.productid = prodData.ProductId;

                        int totalQuantity = 0;
                        if (stock.balance.Count > 0)
                        {
                            totalQuantity = stock.balance[0].totalQuantity;
                        }

                        productSync.prod_stock = totalQuantity;
                        productSync.prod_stock_up = totalQuantity;

                        productSync.prod_costo = prod_costo_up;
                        productSync.prod_costo_up = prod_costo_up;
                        productSync.prod_url_img_01 = prod_url_img_01;

                        _mysqlDbContext.Add(productSync);
                        _mysqlDbContext.SaveChanges();
                    }
                    else
                    {
                        //Prepara el producto para la cola de proceso
                        //await product.Update(prodData, price, stock);
                    }


                    //foreach (Siscom2vTexMatchProduct item in Lista.Where(x => x.skuId == prodData.Id && 
                    //        x.Codigo_Siscom.Trim() == prodData.AlternateIds.RefId.Trim()))
                    if (prodData.AlternateIds.RefId != null)
                    {
                        //if (prodData.AlternateIds.RefId!=null && prodData.AlternateIds.RefId.Contains(ref_id_for_test))
                        //{
                        //    Console.WriteLine("Iniciando búsqueda en lista: solicitado-> " + prodData.AlternateIds.RefId);
                        //}

                        try
                        {
                            //TODO: SOLO SELECCIONAR EL QUE TENGA PROMOCION DE PRECIO SI ES QUE
                            // EXISTE OTRA PROMOCION
                            //Siscom2vTexMatchProduct itemsResult = Lista.Where(x =>
                            //    x.Codigo_Siscom.Trim() == prodData.AlternateIds.RefId.Trim() ||
                            //    x.Codigo_Siscom.Trim().Contains(prodData.AlternateIds.RefId.Trim())
                            //    ).OrderBy(x=> x.es_promo_precio)
                            //    .Last<Siscom2vTexMatchProduct>();

                            List<Siscom2vTexMatchProduct> itemsResult = Lista.Where(x =>
                                x.Codigo_Siscom.Trim() == prodData.AlternateIds.RefId.Trim() ||
                                x.Codigo_Siscom.Trim().Equals(prodData.AlternateIds.RefId.Trim())
                                ).OrderBy(x => x.precio_promocion).ToList();

                            bool SelectPromo = false;
                            foreach (var item in itemsResult)
                            //if (item != null)
                            {
                                //Se redonde el porcentaje
                                item.per_discount = Math.Round(item.per_discount, 2);


                                //if (prodData.AlternateIds.RefId.Contains(ref_id_for_test))
                                //{
                                //    Console.WriteLine("Encontrado en lista: solicitado-> " + prodData.AlternateIds.RefId);
                                //}

                                //No toma en cuenta el producto si la promocion no aplica para vtex
                                //if (item.id_promocion > 0)
                                //{
                                //    //if (item.aplicavtex == null)
                                //    //    continue;
                                //}
                                //else
                                //{
                                if (item.aplicavtex != null)
                                {
                                    if (item.aplicavtex != "S")
                                    {
                                        //No se aplicará promoción, pero hay que evaluar el
                                        //precio actual para quitar el descuento en caso de que exista anteriormente


                                        continue;
                                    }
                                    else
                                    {
                                        SelectPromo = true;
                                    }
                                }
                                else
                                {
                                    SelectPromo = true;
                                }
                                //}

                                //if (item.aplicavtex == null)
                                //{
                                //    if (item.id_promocion > 0)
                                //        continue;

                                //    //continue;
                                //}
                                //else
                                //{
                                //    if (item.aplicavtex != "S")
                                //    {
                                //        continue;
                                //    }
                                //    else
                                //    {
                                //        if (item.id_promocion == 0)
                                //            continue;
                                //    }
                                //}
                                
                                //Productos de MarketPlace
                                //if (item.Codigo_Siscom.Trim().ToUpper().EndsWith("_W") || 
                                //    item.Codigo_Siscom.Trim().ToUpper().EndsWith("-W"))
                                //{
                                //    if(item.stockSiscom == 0)
                                //        item.stockSiscom = 5;
                                //}

                                //Vtex Discount
                                // Bug with discounts lower to 1
                                if(item.per_discount < 1)
                                {
                                    //prod_disc_up = item.per_discount;
                                    if (Math.Equals(item.per_discount, 0.01m))
                                    {
                                        prod_disc_up = 0;
                                        item.per_discount = 0;
                                        item.precio_lista = 0;
                                    }
                                    else
                                    {
                                        prod_disc_up = Math.Round(item.per_discount, 2);
                                        item.per_discount = 0;
                                    }
                                }

                                item.Pvp = Math.Abs(Math.Round(item.Pvp, 2));
                                item.costPrice = Math.Abs(Math.Round(item.costPrice, 2));

                                if (item.es_promo_precio == "S")
                                {
                                    //item.precio_lista
                                    //item.precio_promocion
                                    //prod_pvp_up = item.precio_promocion;
                                    //prod_disc_up = 0;
                                    item.Pvp = item.precio_promocion;
                                    //Precio anterior en Vtex es costo
                                    //item.costPrice = item.precio_lista;                                    
                                    item.per_discount = 0;

                                    //Se evalúa si la diferencia de precios es 0.01 
                                    // se lo iguala ya que no se necesita dicha diferencia en vtex

                                    decimal dec_for_compare = 0.01m;

                                    //if (prodData.AlternateIds.RefId.Contains(ref_id_for_test))
                                    //{                                        
                                    //    //Console.WriteLine(Math.Equals(item.precio_lista - item.Pvp, dec_for_compare));
                                    //}

                                    if (Math.Equals(item.precio_lista - item.Pvp, dec_for_compare))
                                    {
                                        item.Pvp = item.precio_lista;
                                    }
                                }
                                else
                                {
                                    //Promocion por porcentaje
                                    // 07/06/2022
                                    item.Pvp = item.precio_promocion;

                                    //En caso de que el producto no se encuentre en ninguna promoción
                                    if (item.es_promo_precio == null)
                                    {
                                        item.Pvp = item.Pvp_Final;
                                    }

                                    //if (item.per_discount < 1)
                                    //{                                        
                                    //    prod_disc_up = Math.Round(item.per_discount, 2);
                                    //    item.per_discount = 0;
                                    //}
                                }

                                //Console.WriteLine("Encontrado");
                                if (item.Pvp != prod_pvp_up || item.stockSiscom != prod_stock_up ||
                                    item.per_discount != prod_disc_up || item.costPrice != prod_costo_up ||
                                    item.precio_lista != prod_pvp_ant_up ||
                                    blDataChanged)
                                {

                                    if (prod_pvp_up == null)
                                        prod_pvp_up = 0;

                                    //Se asume que en la tabla no existía costo anterior cuando es 0
                                    if (prod_costo_up == 0)
                                        prod_costo_up = item.costPrice;

                                    //Siscom Discount
                                    //if (item.per_discount < 1)
                                    //{
                                    //    item.per_discount = 0;
                                    //}

                                    //Console.WriteLine("Encontrado");
                                    //ADD PRODUCT TO QUEUE UPDATING status field

                                    if (prod_costo_up != item.costPrice)
                                    {
                                        decimal max_cost = Math.Max(prod_costo_up, item.costPrice);
                                        decimal min_cost = Math.Min(prod_costo_up, item.costPrice);
                                        decimal diff_cost = Math.Abs(max_cost - min_cost);

                                        decimal percent_cost = max_cost * 10 / 100;

                                        if (diff_cost > percent_cost)
                                        {
                                            //La diferencia entre el nuevo costo y el costo anterior es de mas del 10%
                                            // se genera alerta o se prepara para confirmación
                                            //stuck for aproveed
                                            //SendEmail(
                                            //    "Cambio en costo de producto" + prodData.AlternateIds.RefId + " " +
                                            //    prodData.NameComplete,
                                            //    "La diferencia entre el nuevo costo y el costo anterior es de mas del 10%, " +
                                            //    prodData.AlternateIds.RefId + " " +
                                            //    prodData.NameComplete);
                                        }
                                    }

                                    int isMarketPlace = 0;
                                    if (item.PROD_MARKETPLACE != null && item.PROD_MARKETPLACE.ToLower() == "s")
                                    {
                                        isMarketPlace = 1;
                                    }

                                    string prod_ensamble = item.PROD_ENSAMBLE;
                                    //item.PROD_ENSAMBLE
                                    
                                    var productForUpdate = _mysqlDbContext.product_sync.Where(d=>
                                    d.prod_vtex_id == prodData.AlternateIds.RefId &&
                                    d.prod_vtex_sku == prodData.Id &&
                                    d.account_name == prodData.account_name).FirstOrDefault();


                                    if (productForUpdate != null)
                                    {
                                        productForUpdate.prod_pvp = prod_pvp_up;
                                        productForUpdate.prod_disc = prod_disc_up;
                                        productForUpdate.prod_pvp_up = prod_pvp_up;
                                        productForUpdate.prod_disc_up = prod_disc_up;
                                        productForUpdate.prod_pvp_ant = prod_pvp_ant;
                                        productForUpdate.prod_pvp_ant_up = prod_pvp_ant_up;
                                        productForUpdate.prod_prov = SupplierId;
                                        productForUpdate.productid = prodData.Id;
                                        productForUpdate.prod_url_img_01 = prod_url_img_01;
                                        productForUpdate.prod_status = 1;
                                        productForUpdate.prod_ensamble = prod_ensamble;                                        

                                        if (isMarketPlace == 1)
                                        {
                                            productForUpdate.prod_stock = prod_stock_up;
                                            productForUpdate.prod_stock_up = prod_stock_up;
                                            productForUpdate.prod_costo = prod_costo_up;
                                            productForUpdate.prod_costo_up = item.costPrice;
                                        }

                                        _mysqlDbContext.Add(productForUpdate);
                                        _mysqlDbContext.SaveChanges();
                                    }

                                    //await product.Update(prodData,
                                    //       prod_pvp_up, //prod_pvp
                                    //       item.Pvp, //prod_pvp_up
                                    //       prod_stock_up,
                                    //       item.stockSiscom,
                                    //       prod_disc_up,
                                    //       item.per_discount,
                                    //       prod_costo_up,
                                    //       item.costPrice,
                                    //       prod_pvp_ant_up, //prod_pvp_ant
                                    //       item.precio_lista, //prod_pvp_ant_up
                                    //       SupplierId,
                                    //       prod_url_img_01,
                                    //       1,
                                    //       isMarketPlace,
                                    //       prod_ensamble);

                                    //if (!prodData.AlternateIds.RefId.ToUpper().EndsWith("_W") && !prodData.AlternateIds.RefId.ToUpper().EndsWith("-W"))
                                    //{
                                       
                                    //}
                                }

                                //Si ya se ha seleccionado promoción (dentro de la lista devuelta) sale del bucle
                                if (SelectPromo)
                                    break;
                            }
                        }
                        catch (Exception e)
                        {   
                            if (prodData.AlternateIds.RefId != null)
                            {
                                Console.WriteLine("Error: al buscar " + prodData.AlternateIds.RefId);
                                //if (prodData.AlternateIds.RefId.Contains(ref_id_for_test))
                                //{
                                //    Console.WriteLine("Error: solicitado-> " + prodData.AlternateIds.RefId);
                                //}
                            }
                                

                            //Evaluar código de error, en caso de ser un error por no encontrarlo en la lista
                            // debería eliminar el registro de la base de datos para que lo vuelva a agregar 
                            // con los datos correctos en la próxima ejecución
                            if (e.Source.Contains("System.Linq"))
                            {
                                //Console.WriteLine(e.Message);
                            }
                            else
                            {
                                Console.WriteLine(e.Message);
                            }
                        }                           
                    }
                }                
            }
            
            if(lastGroup)
            {
                //LogManager.Log().LogInformation("Terminado último grupo");
                Console.WriteLine("Terminado último grupo");
            }
            //Console.WriteLine("Terminado PC");            
        }
                
        private void WriteLog(string Mensaje, int proceso)
        {            
            //Logs.Write(Mensaje, proceso);
            Console.WriteLine(Mensaje);
        }

        public async Task<string[]> getDataVtexAllSkus()
        {
            string[] allSkus = null;

            try
            {
                string url = $"https://{mAccount.Account_Name}.vtexcommercestable.com.br/api/catalog_system/pvt/sku/stockkeepingunitids?page=1&pagesize=10000";

                // Crear instancia de HttpClient
                using (var httpClient = new HttpClient())
                {
                    // Configurar encabezados de autenticación
                    httpClient.DefaultRequestHeaders.Add("x-vtex-api-appKey", mAccount.vTexApiKey);
                    httpClient.DefaultRequestHeaders.Add("x-vtex-api-appToken", mAccount.vTexApiToken);

                    // Realizar la solicitud HTTP
                    HttpResponseMessage response = await httpClient.GetAsync(url);

                    // Verificar si la respuesta es exitosa
                    response.EnsureSuccessStatusCode();

                    // Leer y procesar la respuesta
                    string result = await response.Content.ReadAsStringAsync();
                    //result = result.Replace("[", "").Replace("]", "");
                    //allSkus = result.Split(",");
                    
                    allSkus = JsonConvert.DeserializeObject<string[]>(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en extracción de skus desde Vtex: " + ex.Message);
            }

            return allSkus;
        }

        private string[] __getDataVtexAllSkus()
        {
            string[] allSkus = null;

            try
            {
                string cUrlPrice = string.Format("https://{0}.vtexcommercestable.com.br/api/catalog_system/pvt/sku/stockkeepingunitids?page=1&pagesize=10000", mAccount.Account_Name);
                var httpWebRequest = (HttpWebRequest) WebRequest.Create(cUrlPrice);
                httpWebRequest.ContentType = "application/json;charset=utf-8'";
                httpWebRequest.Headers.Add("x-vtex-api-appKey", mAccount.vTexApiKey);
                httpWebRequest.Headers.Add("x-vtex-api-appToken", mAccount.vTexApiToken);
                httpWebRequest.Method = "GET";
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //vTexPrice jsonres;
                
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    result = result.Replace("[", "").Replace("]","");
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

        int getDataVtex(int proc, vTex_Account cuenta)
        {
            int retorno = 0;
            WriteLog("Inciando proceso ..." + proc.ToString(), proc);
            foreach (var ent in Lista.Where(x => x.NumeroFila >= (proc * multiplicador - multiplicador) && x.NumeroFila <= proc * multiplicador))
            {
                WriteLog("Obteniendo información de " + ent.Nombre_Siscom + " SKU: " + ent.skuId.ToString(), proc);
                retorno++;
                try
                {
                    string cUrlPrice = string.Format("https://api.vtex.com/{0}/pricing/prices/{1}", cuenta.Account_Name, ent.skuId);
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(cUrlPrice);
                    httpWebRequest.ContentType = "application/json;charset=utf-8'";
                    httpWebRequest.Headers.Add("x-vtex-api-appKey", cuenta.vTexApiKey);
                    httpWebRequest.Headers.Add("x-vtex-api-appToken", cuenta.vTexApiToken);
                    httpWebRequest.Method = "GET";
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    vTexPrice jsonres;
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        jsonres = JsonConvert.DeserializeObject<vTexPrice>(result);
                    }
                    ent.listPrice = jsonres.listPrice;
                    ent.costPrice = jsonres.costPrice;
                    ent.difPrecio = ent.listPrice != ent.Pvp_Final;
                }
                catch (Exception ex)
                {
                    ent.difPrecio = true;
                    WriteLog("Falló la obtención de datos de " + ent.Nombre_Siscom + "Error: " + ex.Message, proc);
                    ent.Nombre_vTex = "No encontrado";
                    ent.NoExisteVtex = true;
                }

                /// intenta obtener datos de stock
                try
                {
                    string cBod = "1_1";
                    string pato = "";
                    if (cuenta.Account_Name == "tempodesignpan")
                        cBod = "1cd771c";

                    string cUrlPrice = string.Format("https://logistics.vtexcommercestable.com.br/api/logistics/pvt/inventory/skus/{1}?an={0}&skuid={1}", cuenta.Account_Name, ent.skuId);
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(cUrlPrice);
                    httpWebRequest.ContentType = "application/json;charset=utf-8'";
                    httpWebRequest.Headers.Add("x-vtex-api-appKey", cuenta.vTexApiKey);
                    httpWebRequest.Headers.Add("x-vtex-api-appToken", cuenta.vTexApiToken);
                    httpWebRequest.Method = "GET";
                    if (ent.skuId == 772)
                        pato = "entro";
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    vtexStock jsonres;
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        jsonres = JsonConvert.DeserializeObject<vtexStock>(result);
                    }
                    if (jsonres.balance.Where(x => x.warehouseId.ToString() == cBod).Count() > 0)
                    {
                        ent.stockvTex = jsonres.balance.Where(x => x.warehouseId.Trim() == cBod).First().totalQuantity;
                    }
                    else
                    {
                        WriteLog("No se encontró warehouse ", proc);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog("Falló la obtención de datos de  stock " + ent.Nombre_Siscom + "Error: " + ex.Message, proc);
                    ent.Nombre_vTex = "No encontrado";
                    ent.stockvTex = 0;
                    ent.NoExisteVtex = true;

                }

            }//foreach 
            return retorno;
        }
    }// fom Program
    public class proceso
    {
        public int numero { get; set; }
        public vTex_Account cuenta { get; set; }
    }

}
