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
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Models.DMSA.Mbw.Clientes;
using System.Data;
using Entidades.SyncTask;
using Models.DMSA.Shared.General;
using GeneralModels.DMSA.Especiales;
using DataSourceManager;

namespace ApiTradeHub.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotaCreditoDetController : ControllerBase
    {
        DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();
        
        [HttpGet]
        public async Task<ActionResult<ApiResponse_v1>> Get()
        {
            String sql = "select vd.codagencia," +
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

            sql += @" and v.codagencia = 2 and v.codtipocmpr = 'FAC' and v.numcmprventa = 859
and(v.fecharegistro > to_date('01/01/2021', 'YYYY-MM-DD HH24:MI:SS')
or v.fechamodificacion > to_date('01/01/2021', 'YYYY-MM-DD HH24:MI:SS')
or v.fechamodificacionnc > to_date('01/01/2021', 'YYYY-MM-DD HH24:MI:SS'))";

            int numeroDeFilaInicial = 1;
            int cantidadMaximaFilas = 10;

            //List<NotaCreditoDet> lresult = null;
            //lresult = await appDbContext.GENCLIENTEAPROBACION.ToListAsync();
            //List<NotaCreditoDet> lresult = 
            //    appDbContext.SPNotaCreditoDet.FromSqlInterpolated(sql).
            //    Skip(numeroDeFilaInicial - 1).
            //    Take(cantidadMaximaFilas).ToList();

            //List<NotaCreditoDet> lresult = await appDbContext.SPNotaCreditoDet.FromSqlInterpolated($"{sql}").ToListAsync();
            List<NotaCreditoDet> lresult = null;

            //TODO: Deprecated
            //List <NotaCreditoDet> lresult = await appDbContext.SPNotaCreditoDet.FromSqlRaw(sql)
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
                
        [HttpPost]
        public async Task<ActionResult<ClienteAprobacion>> Post([FromBody] ClienteAprobacion document)
        {
            await appDbContext.GENCLIENTEAPROBACION.AddAsync(document);
            await appDbContext.SaveChangesAsync();
            return document;
        }

        [HttpPut()]
        public async Task<ActionResult<ClienteAprobacion>> Put([FromBody] ClienteAprobacion document)
        {
            appDbContext.GENCLIENTEAPROBACION.Update(document);
            await appDbContext.SaveChangesAsync();
            return document;
        }
    }
}
