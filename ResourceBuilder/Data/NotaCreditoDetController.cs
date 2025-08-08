using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Data;
using DataSourceManager;
using Models.DMSA.Shared.General;
using Models.DMSA.Shared.Especiales;

namespace ResourceBuilder.Data
{
    public class NotaCreditoDetController
    {
        DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();
        
        public async Task<ApiResponse_v1> GetData(int numeroDeFilaInicial, int p2, string p3)
        {
            //string fechaActualizacion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") ;
            string fechaActualizacion = DateTime.Now.ToString("2021-01-01 00:00:00");
            bool actualiza = true;

            //Genparametros diasNotaCredito = new GenparametrosDAOEXT().obtenerParametroPorEmpresa(1L, "DIAS_NOTA_CREDITO_VENTA_APP");
            var diasNotaCredito = appDbContext.GENPARAMETROS.Where(p => p.CodParametro == "DIAS_NOTA_CREDITO_VENTA_APP").FirstOrDefault();

            var aplicaFueraRango =  appDbContext.GENPARAMETROS.Where(p=>p.CodParametro== "APLICA_FAC_NC_FUERA_RANGO_APP").FirstOrDefault();

            string sqlExtra = "";

            if(aplicaFueraRango!=null)
            {
                //Aplica fuera de rango
                if(aplicaFueraRango.Valor=="S")
                {
                    //TODO: MIGRAR ESTA SECCIÓN
                    //Genparametros valoresFactura = new GenparametrosDAOEXT().obtenerParametroPorEmpresa(1L, "VALORES_FC_NC_FUERA_RANGO_APP");
                    //String codagencia = valoresFactura.getValor().split("-")[0];
                    //String codtipocmpr = valoresFactura.getValor().split("-")[1];
                    //String numcmprventa = valoresFactura.getValor().split("-")[2];

                    //sql += " and v.codagencia= " + codagencia + " and v.codtipocmpr= '" + codtipocmpr + "' and v.numcmprventa = " + numcmprventa;
                }
                else
                {
                    if (diasNotaCredito != null)
                    {
                        sqlExtra += " and (v.fecharegistro >= sysdate - " + diasNotaCredito.Valor;
                        sqlExtra += " or v.fechamodificacion >= sysdate - " + diasNotaCredito.Valor + ")";
                    }
                }

                if (actualiza && "S" != aplicaFueraRango.Valor)
                {
                    //sql+=" and v.fecha >= sysdate - trunc(sysdate -(to_date('"+fechaActualizacion+"','YYYY-MM-DD HH24:MI:SS'))) ";
                    //sql+=" and v.fecha >= sysdate - trunc(sysdate - trunc((to_date('"+fechaActualizacion+"','YYYY-MM-DD HH24:MI:SS'))))";

                    //APonce 30/11/2021
                    //actualizar mayor a la fecha, hora actual
                    sqlExtra += " and (v.fecharegistro > to_date('" + fechaActualizacion + "','YYYY-MM-DD HH24:MI:SS')";
                    sqlExtra += " or v.fechamodificacion > to_date('" + fechaActualizacion + "','YYYY-MM-DD HH24:MI:SS') ";
                    sqlExtra += " or v.fechamodificacionnc > to_date('" + fechaActualizacion + "','YYYY-MM-DD HH24:MI:SS'))";
                }
            }

            string sql = "select vd.codagencia," +
                        "       vd.codtipocmpr," +
                        "       vd.numcmprventa," +
                        "		vd.numcmprventadet," +
                        "       ae.codalterno || '-' || ae.descripcion ARTICULO," +
                        "       vd.costo," +
                        "       vd.cantidad," +
                        "       vd.precio," +
                        "       vd.subtotal," +
                        "       vd.descuento," +
                        "       vd.porcdescuento," +
                        "       vd.impuesto," +
                        "       vd.porcimpuesto," +
                        "       vd.total," +
                        "       vd.espremioopromocion," +
                        "       vd.cantidaddevuelta," +
                        "		nvl(vd.numcmprventadetaplica,0) NUMCMPRVENTADETAPLICA," +
                        "		vd.codarticulo," +
                        "		nvl(vd.cantidadnoconforme,0) CANTIDADNOCONFORME" +                        
                        "     from faccmprventa v " +
                        "     inner join genagencias a on (v.codagencia=a.codagencia)" +
                        "     inner join genempresas em on (a.codempresa=em.codempresa)" +
                        "     inner join cnttipocmpr t on (v.codtipocmpr=t.codtipocmpr and t.codtipocmpr = 'FAC')" +
                        "     inner join genestados e on (v.codestado=e.codestado)" +
                        "     inner join genclientes c on (v.codcliente=c.codcliente)" +
                        "     inner join cxcdocumento cd on (v.codagencia=cd.codagencia and v.codtipocmpr=cd.tipocmpr and v.numcmprventa=cd.refnumero and cd.tipocmpr = 'FAC')" +
                        " 	  inner join genvendedores vnd on (vnd.codempresa = em.codempresa and vnd.codvendedor = v.codvendedor) " +
                        " 	  inner join gentipovendedor tv on (tv.codtipovendedor=vnd.codtipovendedor) " +
                        "     inner join faccmprventadet vd on (vd.codagencia=v.codagencia and vd.codtipocmpr=v.codtipocmpr and vd.numcmprventa=v.numcmprventa)" +
                        "     inner join genarticulos ae on (vd.codarticulo=ae.codarticulo)" +
                        "     where em.lineagrupoempresarial = 1" +
                        "      and v.codtipocmpr='FAC' " + 
                        "      and v.codestado != 3" + 
                        "      and v.codcliente not in (643, 5314,4299,19271)" + //SE DESCARTA MARCO , CONSUMIDOR FINAL, DMUJERES
                        "      and nvl(v.esservicio,'N') = 'N' " +
                        "	   and tv.codtipovendedor = 2 " +
                        "      and v.tipopago != 'A' " +
                        "      and (vd.cantidad-vd.cantidaddevuelta)>0";

            sql += sqlExtra;
                        
            int cantidadMaximaFilas = 300;

            //List<NotaCreditoDet> lresult = null;
            //lresult = await appDbContext.GENCLIENTEAPROBACION.ToListAsync();
            //List<NotaCreditoDet> lresult = 
            //    appDbContext.SPNotaCreditoDet.FromSqlInterpolated(sql).
            //    Skip(numeroDeFilaInicial - 1).
            //    Take(cantidadMaximaFilas).ToList();

            //List<NotaCreditoDet> lresult = await appDbContext.SPNotaCreditoDet.FromSqlInterpolated($"{sql}").ToListAsync();


            List<NotaCreditoDet> lresult = null;

            //List<NotaCreditoDet> lresult = await appDbContext.SPNotaCreditoDet.FromSqlRaw(sql)
            //    .Skip(numeroDeFilaInicial - 1)
            //    .Take(cantidadMaximaFilas)
            //    .ToListAsync();

            ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
            apiResponse_V1.success = true;
            apiResponse_V1.final = true;
            apiResponse_V1.exito = true;
            //List<ClienteAprobacion> lresult = null;
            //lresult = await appDbContext.GENCLIENTEAPROBACION.ToListAsync();
            apiResponse_V1.data = lresult;
            apiResponse_V1.cantidad_registros = lresult.Count;
            return apiResponse_V1;
        }
                
        //[HttpPost]
        //public async Task<ActionResult<ClienteAprobacion>> Post([FromBody] ClienteAprobacion document)
        //{
        //    await appDbContext.GENCLIENTEAPROBACION.AddAsync(document);
        //    await appDbContext.SaveChangesAsync();
        //    return document;
        //}

        //[HttpPut()]
        //public async Task<ActionResult<ClienteAprobacion>> Put([FromBody] ClienteAprobacion document)
        //{
        //    appDbContext.GENCLIENTEAPROBACION.Update(document);
        //    await appDbContext.SaveChangesAsync();
        //    return document;
        //}
    }
}
