using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DMSA.Mbw.Clientes;
using System.Data;
using Models.DMSA.Shared.General;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Oracle.ManagedDataAccess.Client;

namespace ApiTradeHub.Controllers.Customer
{
    //[ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class AprobacionesController : ControllerBase
    {
        DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<ApiResponse_v1>> GetById(string id)
        {
            ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
            apiResponse_V1.success = true;
            apiResponse_V1.final = true;
            apiResponse_V1.exito = true;
            apiResponse_V1.mensaje = "Proceso realizado exitosamente";
            List<ClienteAprobacion> lresult = null;
            lresult = await appDbContext.GENCLIENTEAPROBACION.Where(c => c.IDENTIFICACION == id).ToListAsync();
            apiResponse_V1.data = lresult;
            apiResponse_V1.cantidad_registros = lresult.Count;
            return apiResponse_V1;
        }
                
        //52	Autorizado
        //54	Por Autorizar
        [HttpGet("GetByStatus/{status:int}")]
        public async Task<ActionResult<ApiResponse_v1>> GetByStatus(int status)
        {
            ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
            apiResponse_V1.success = true;
            apiResponse_V1.final = true;
            apiResponse_V1.exito = true;
            apiResponse_V1.mensaje = "Proceso realizado exitosamente";
            List<ClienteAprobacion> lresult = null;
            lresult = await appDbContext.GENCLIENTEAPROBACION.Where(c => c.CODESTADO == status).ToListAsync();
            apiResponse_V1.data = lresult;
            apiResponse_V1.cantidad_registros = lresult.Count;
            return apiResponse_V1;
        }

        //53	No Autorizado
        //52	Autorizado
        //54	Por Autorizar
        [HttpGet("GetFromFacPedido/{codusuario}/{codestado:int}")]
        public async Task<ActionResult<ApiResponse_v1>> GetFromFacPedido(string codusuario, int codestado)
        {
            ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
            apiResponse_V1.success = true;
            apiResponse_V1.final = true;
            apiResponse_V1.exito = true;
            apiResponse_V1.mensaje = "Proceso realizado exitosamente";
            List<ClienteAprobacion> lresult = null;

            //            string sql = $@"select * from (select IDENTIFICACION,
            //                CODAGENCIA,
            //                NUMPEDIDO,
            //CODCLIENTE,
            //NOMBRESCLIENTE,
            //APELLIDOSCLIENTE,
            //TIPOIDENTIFICACION,
            //DIRECCIONCLIENTE,
            //TELEFONOCLIENTE,
            //EMAIL EMAILCLIENTE,
            //CODEMPRESA,
            //CODVENDEDOR,
            //CODUSUARIO,
            //FECHAREGISTRO,
            //FECHACAMBIOESTADO,
            //FECHAMODIFICACION,
            //'' APLICACIONORIGEN,
            //'' APLICACIONVERSION,
            //PLATAFORMAORIGEN,
            //PLATAFORMAMODIFICA,
            //MARCAEQUIPO,
            //MODELOEQUIPO,
            //CODESTADO
            //FROM facpedido
            //where codusuario = '{codusuario}' and codestado = {codestado} and not identificacion like '9999999999%') 
            //where 
            //IDENTIFICACION NOT IN ( SELECT IDENTIFICACION FROM GENCLIENTEAPROBACION) AND
            //rownum <= 100 order by fecharegistro desc";

            string sql = $@"with rws as (
  select IDENTIFICACION,
                CODAGENCIA,
                NUMPEDIDO,
CODCLIENTE,
NOMBRESCLIENTE,
APELLIDOSCLIENTE,
TIPOIDENTIFICACION,
DIRECCIONCLIENTE,
TELEFONOCLIENTE,
EMAIL EMAILCLIENTE,
CODEMPRESA,
CODVENDEDOR,
CODUSUARIO,
FECHACAMBIOESTADO,
FECHAMODIFICACION,
'' APLICACIONORIGEN,
'' APLICACIONVERSION,
PLATAFORMAORIGEN,
PLATAFORMAMODIFICA,
MARCAEQUIPO,
MODELOEQUIPO,
CODESTADO,
FECHAREGISTRO,
count (*) CONTADOR,
         row_number () over (
           partition by IDENTIFICACION
           order by count(*) desc, CODAGENCIA
         ) rn
  from   facpedido
  where codusuario = '{codusuario}' and codestado = {codestado} and not identificacion like '9999999999%' and
IDENTIFICACION NOT IN ( SELECT IDENTIFICACION FROM GENCLIENTEAPROBACION)
  group  by IDENTIFICACION,
                CODAGENCIA,
                NUMPEDIDO,
CODCLIENTE,
NOMBRESCLIENTE,
APELLIDOSCLIENTE,
TIPOIDENTIFICACION,
DIRECCIONCLIENTE,
TELEFONOCLIENTE,
EMAIL,
CODEMPRESA,
CODVENDEDOR,
CODUSUARIO,
FECHACAMBIOESTADO,
FECHAMODIFICACION,
PLATAFORMAORIGEN,
PLATAFORMAMODIFICA,
MARCAEQUIPO,
MODELOEQUIPO,
CODESTADO,
FECHAREGISTRO
order  by FECHAREGISTRO desc
)
  select * from rws
  where  rn <= 1 and rownum <= 100";

            lresult = await appDbContext.GENCLIENTEAPROBACION.FromSqlRaw(sql).ToListAsync();
            apiResponse_V1.data = lresult;
            apiResponse_V1.cantidad_registros = lresult.Count;
            return apiResponse_V1;
        }

        [HttpGet("GetFromFacPedidoAgencia/{codagencia:int}/{codestado:int}/{textobusqueda}")]
        public async Task<ActionResult<ApiResponse_v1>> GetFromFacPedidoAgencia(int codagencia, int codestado, string textobusqueda)
        {
            await ProcessInternal(codagencia);

            ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
            apiResponse_V1.success = true;
            apiResponse_V1.final = true;
            apiResponse_V1.exito = true;
            apiResponse_V1.mensaje = "Proceso realizado exitosamente";
            List<ClienteAprobacion> lresult = null;

            //TODO: Especial temporal, solo para evitar mayores cambios en la app
            if(codestado == 1)
            {
                codestado = 54;
            }

            string sql = $@"select * from (
SELECT 
IDENTIFICACION,
CODAGENCIA,
NUMPEDIDO,
CODCLIENTE,
NOMBRESCLIENTE,
APELLIDOSCLIENTE,
TIPOIDENTIFICACION,
DIRECCIONCLIENTE,
TELEFONOCLIENTE,
EMAILCLIENTE,
CODEMPRESA,
CODVENDEDOR,
CODUSUARIO,
FECHAREGISTRO,
FECHACAMBIOESTADO,
FECHAMODIFICACION,
APLICACIONORIGEN,
APLICACIONVERSION,
PLATAFORMAORIGEN,
PLATAFORMAMODIFICA,
MARCAEQUIPO,
MODELOEQUIPO,
CODESTADO
FROM GENCLIENTEAPROBACION
  where codagencia = '{codagencia}' and codestado = {codestado}
and (nombrescliente like '%{textobusqueda}%' 
  or apellidoscliente like '%{textobusqueda}%' 
  or identificacion  like '%{textobusqueda}%' )
and not identificacion like '9999999999%'
order  by FECHAREGISTRO desc
) where rownum <= 100 order by FECHAREGISTRO desc";

            lresult = await appDbContext.GENCLIENTEAPROBACION.FromSqlRaw(sql).ToListAsync();
            apiResponse_V1.data = lresult;
            apiResponse_V1.cantidad_registros = lresult.Count;
            return apiResponse_V1;
        }

        /// <summary>
        /// Proceso interno para la creación de cola de aprobaciones
        /// Se ha agregado validación de email, es decir solo los que tienen email válido pasarán a la cola
        /// </summary>
        /// <param name="codagencia"></param>
        /// <returns></returns>
        private async Task ProcessInternal(int codagencia)
        {
            int codestado = 1; //facturas pendientes, talvez se deba omitir

            List<ClienteAprobacion> lresult = null;

            string sql = $@"with rws as (
              select IDENTIFICACION,
                            CODAGENCIA,
                            NUMPEDIDO,
            CODCLIENTE,
            NOMBRESCLIENTE,
            APELLIDOSCLIENTE,
            TIPOIDENTIFICACION,
            DIRECCIONCLIENTE,
            TELEFONOCLIENTE,
            EMAIL EMAILCLIENTE,
            CODEMPRESA,
            CODVENDEDOR,
            CODUSUARIO,
            FECHACAMBIOESTADO,
            FECHAMODIFICACION,
            '' APLICACIONORIGEN,
            '' APLICACIONVERSION,
            PLATAFORMAORIGEN,
            PLATAFORMAMODIFICA,
            MARCAEQUIPO,
            MODELOEQUIPO,
            54 CODESTADO,
            FECHAREGISTRO,
            count (*) CONTADOR,
                     row_number () over (
                       partition by IDENTIFICACION
                       order by count(*) desc, CODAGENCIA
                     ) rn
              from   facpedido
              where codagencia = '{codagencia}' and trunc(FECHAREGISTRO) = trunc(sysdate)
             and not identificacion like '9999999999%' and
REGEXP_LIKE (EMAIL,:reg_exp_str) and
            IDENTIFICACION NOT IN ( SELECT IDENTIFICACION FROM GENCLIENTEAPROBACION )
              group  by IDENTIFICACION,
                            CODAGENCIA,
                            NUMPEDIDO,
            CODCLIENTE,
            NOMBRESCLIENTE,
            APELLIDOSCLIENTE,
            TIPOIDENTIFICACION,
            DIRECCIONCLIENTE,
            TELEFONOCLIENTE,
            EMAIL,
            CODEMPRESA,
            CODVENDEDOR,
            CODUSUARIO,
            FECHACAMBIOESTADO,
            FECHAMODIFICACION,
            PLATAFORMAORIGEN,
            PLATAFORMAMODIFICA,
            MARCAEQUIPO,
            MODELOEQUIPO,
            CODESTADO,
            FECHAREGISTRO
            order by FECHAREGISTRO desc
            )
              select * from rws
              where  rn <= 1 and rownum <= 100";

            string reg_exp_str = @"^[a-zA-Z0-9!#$%''\*\+-/=\?^_`\{|\}~]+@[a-zA-Z0-9._%-]+\.[a-zA-Z]{2,4}$";

            var parameters = new[] {
                new OracleParameter("reg_exp_str", OracleDbType.NVarchar2, reg_exp_str, ParameterDirection.Input)
            };

            lresult = await appDbContext.GENCLIENTEAPROBACION.FromSqlRaw(sql, parameters).ToListAsync();

            //foreach (var item in lresult)
            //{
                
            //}

            if(lresult.Count == 0 )
            {
                return;
            }

            try
            {
                await appDbContext.GENCLIENTEAPROBACION.AddRangeAsync(lresult);
                await appDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Guardando en tabla GENCLIENTEAPROBACION" + codagencia.ToString());
                Console.WriteLine("----------------------------------------------------------------------");
                Console.WriteLine(ex.ToString());
            }

        }


        [HttpGet]
        public async Task<ActionResult<ApiResponse_v1>> Get()
        {
            //this.ControllerContext.HttpContext

            ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
            apiResponse_V1.success = true;
            apiResponse_V1.final = true;
            apiResponse_V1.exito = true;
            apiResponse_V1.mensaje = "Proceso realizado exitosamente";
            List<ClienteAprobacion> lresult = null;
            lresult = await appDbContext.GENCLIENTEAPROBACION.ToListAsync();
            apiResponse_V1.data = lresult;
            apiResponse_V1.cantidad_registros = lresult.Count;
            return apiResponse_V1;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse_v1>> Post([FromBody] ClienteAprobacion document)
        {
            ApiResponse_v1 apiResponse_Save_V3 = new ApiResponse_v1();
            apiResponse_Save_V3.cantidad_registros = 0;
            apiResponse_Save_V3.final = true;
            apiResponse_Save_V3.success = true;
            apiResponse_Save_V3.exito = true;
            apiResponse_Save_V3.mensaje = "Proceso realizado exitosamente";

            document.TIPOIDENTIFICACION = document.TIPOIDENTIFICACION?.ToUpper();
            document.NOMBRESCLIENTE = document.NOMBRESCLIENTE?.ToUpper();
            document.APELLIDOSCLIENTE = document.APELLIDOSCLIENTE?.ToUpper();
            document.DIRECCIONCLIENTE = document.DIRECCIONCLIENTE?.ToUpper();
            document.EMAILCLIENTE = document.EMAILCLIENTE?.ToLower();
            document.TELEFONOCLIENTE = document.TELEFONOCLIENTE;
            document.FECHACAMBIOESTADO = DateTime.Now;
            apiResponse_Save_V3.data = document;

            try
            {
                var foundItem = appDbContext.GENCLIENTEAPROBACION.AsNoTracking().Where(i=>i.IDENTIFICACION == document.IDENTIFICACION).FirstOrDefault();
                
                if (foundItem!=null)
                {
                    appDbContext.GENCLIENTEAPROBACION.Update(document);
                    await appDbContext.SaveChangesAsync();
                }
                else
                {
                    await appDbContext.GENCLIENTEAPROBACION.AddAsync(document);
                    await appDbContext.SaveChangesAsync();
                }
                
            }
            catch (Exception ex)
            {
                apiResponse_Save_V3.success = false;
                apiResponse_Save_V3.mensaje = "" + ex.Message;
                apiResponse_Save_V3.data = null;
            }

            return apiResponse_Save_V3;
        }

        [HttpPost("PostFull")]
        public async Task<ActionResult<ApiResponse_v1>> PostFull([FromBody] ClienteAprobacion document)
        {
            ApiResponse_v1 apiResponse_Save_V3 = new ApiResponse_v1();
            apiResponse_Save_V3.cantidad_registros = 0;
            apiResponse_Save_V3.final = true;
            apiResponse_Save_V3.success = true;
            apiResponse_Save_V3.exito = true;
            apiResponse_Save_V3.mensaje = "Proceso realizado exitosamente";

            document.TIPOIDENTIFICACION = document.TIPOIDENTIFICACION?.ToUpper();
            document.NOMBRESCLIENTE = document.NOMBRESCLIENTE?.ToUpper();
            document.APELLIDOSCLIENTE = document.APELLIDOSCLIENTE?.ToUpper();
            document.DIRECCIONCLIENTE = document.DIRECCIONCLIENTE?.ToUpper();
            document.EMAILCLIENTE = document.EMAILCLIENTE?.ToLower();
            document.FECHACAMBIOESTADO = DateTime.Now;
            apiResponse_Save_V3.data = document;
            //document.FECHAREGISTRO = DateTime.Now;

            try
            {
                //Primero se intenta almacenar en la tabla nueva               
                var foundItem = appDbContext.GENCLIENTEAPROBACION.AsNoTracking().Where(i => i.IDENTIFICACION == document.IDENTIFICACION).FirstOrDefault();

                if (foundItem != null)
                {
                    appDbContext.GENCLIENTEAPROBACION.Update(document);
                    await appDbContext.SaveChangesAsync();
                }
                else
                {
                    await appDbContext.GENCLIENTEAPROBACION.AddAsync(document);
                    await appDbContext.SaveChangesAsync();
                }

                //await appDbContext.GENCLIENTEAPROBACION.AddAsync(document);
                //await appDbContext.SaveChangesAsync();

                //Si se almacena correctamente se procede a modificar la tabla de proformas

                string SqlRaw = $@"
                    update facpedido set 
                    TIPOIDENTIFICACION = '{document.TIPOIDENTIFICACION}',
                    IDENTIFICACION = '{document.IDENTIFICACION}',
                    APELLIDOSCLIENTE = '{document.APELLIDOSCLIENTE}',
                    NOMBRESCLIENTE = '{document.NOMBRESCLIENTE}',
                    TELEFONOCLIENTE = '{document.TELEFONOCLIENTE}',
                    DIRECCIONCLIENTE = '{document.DIRECCIONCLIENTE}',
                    EMAIL = '{document.EMAILCLIENTE}'
                    where IDENTIFICACION = '{document.IDENTIFICACION}' 
                    AND NUMPEDIDO = {document.NUMPEDIDO}
                    AND CODAGENCIA = {document.CODAGENCIA}";

                string pattern = @"(;[\s\r\n\t]*|[\s\r\n\t]+(go|use|select|insert|update|delete|create|alter|drop|truncate|merge)[\s\r\n\t]+)";

                // Verificar si hay dos o más instrucciones SQL
                if (Regex.Matches(SqlRaw, pattern, RegexOptions.IgnoreCase).Count >= 2)
                {
                    Debug.WriteLine("Posible intento de sql injection");

                    apiResponse_Save_V3.success = false;
                    apiResponse_Save_V3.mensaje = "Error:" + "Posible intento de sql injection";
                    apiResponse_Save_V3.data = null;
                    return apiResponse_Save_V3;
                }

                //if(ContainsMultipleSqlStatements(SqlRaw))
                //{
                //    Debug.WriteLine("Posible intento de injection");
                //}

                //Aquí código para modificar los datos de la proforma
                int resultExec = await appDbContext.Database.ExecuteSqlRawAsync(SqlRaw);
                if (resultExec == 0)
                {

                }

                //Actualiza datos de la tabla GenClientes
                //await UpdateGenClientes(document);

            }
            catch (Exception ex)
            {
                apiResponse_Save_V3.success = false;
                apiResponse_Save_V3.mensaje = "" + ex.InnerException?.Message;
                apiResponse_Save_V3.data = null;
            }

            return apiResponse_Save_V3;
        }

        [HttpGet("UpdateGenClientes")]
        public async Task<bool> UpdateGenClientes(ClienteAprobacion document)
        {
            document.TIPOIDENTIFICACION = document.TIPOIDENTIFICACION?.ToUpper();
            document.NOMBRESCLIENTE = document.NOMBRESCLIENTE?.ToUpper();
            document.APELLIDOSCLIENTE = document.APELLIDOSCLIENTE?.ToUpper();
            document.DIRECCIONCLIENTE = document.DIRECCIONCLIENTE?.ToUpper();
            document.EMAILCLIENTE = document.EMAILCLIENTE?.ToLower();
            document.FECHACAMBIOESTADO = DateTime.Now;
            
            try
            {
                //Se procede a modificar la tabla de genclientes
                string SqlRaw = $@"
                    update genclientes set 
                    TIPOIDENTIFICACION = '{document.TIPOIDENTIFICACION}',
                    IDENTIFICACION = '{document.IDENTIFICACION}',
                    APELLIDOS = '{document.APELLIDOSCLIENTE}',
                    NOMBRES = '{document.NOMBRESCLIENTE}',
                    TELEFONO3 = '{document.TELEFONOCLIENTE}',
                    DOMICILIO = '{document.DIRECCIONCLIENTE}',
                    EMAIL = '{document.EMAILCLIENTE}'
                    where IDENTIFICACION = '{document.IDENTIFICACION}'";

                string pattern = @"(;[\s\r\n\t]*|[\s\r\n\t]+(go|use|select|insert|update|delete|create|alter|drop|truncate|merge)[\s\r\n\t]+)";

                // Verificar si hay dos o más instrucciones SQL
                if (Regex.Matches(SqlRaw, pattern, RegexOptions.IgnoreCase).Count >= 2)
                {
                    Debug.WriteLine("Posible intento de sql injection");
                    return false;
                }

                //Aquí código para modificar los datos de la proforma
                int resultExec = await appDbContext.Database.ExecuteSqlRawAsync(SqlRaw);
                if (resultExec == 0)
                {

                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        [HttpGet("UpdateGenDireccionesClientes")]
        public async Task<bool> UpdateGenDireccionesClientes(ClienteAprobacion document)
        {
            document.TIPOIDENTIFICACION = document.TIPOIDENTIFICACION?.ToUpper();
            document.NOMBRESCLIENTE = document.NOMBRESCLIENTE?.ToUpper();
            document.APELLIDOSCLIENTE = document.APELLIDOSCLIENTE?.ToUpper();
            document.DIRECCIONCLIENTE = document.DIRECCIONCLIENTE?.ToUpper();
            document.EMAILCLIENTE = document.EMAILCLIENTE?.ToLower();
            document.FECHACAMBIOESTADO = DateTime.Now;

            try
            {
                //Se procede a modificar la tabla de genclientes
                string SqlRaw = $@"
                    update gendireccionesclientes set
                    TELEFONO2 = '{document.TELEFONOCLIENTE}',
                    DIRECCION = '{document.DIRECCIONCLIENTE}',
                    EMAIL = '{document.EMAILCLIENTE}'
                    where IDENTIFICACION = '{document.IDENTIFICACION}'";

                string pattern = @"(;[\s\r\n\t]*|[\s\r\n\t]+(go|use|select|insert|update|delete|create|alter|drop|truncate|merge)[\s\r\n\t]+)";

                // Verificar si hay dos o más instrucciones SQL
                if (Regex.Matches(SqlRaw, pattern, RegexOptions.IgnoreCase).Count >= 2)
                {
                    Debug.WriteLine("Posible intento de sql injection");
                    return false;
                }

                //Aquí código para modificar los datos de la proforma
                int resultExec = await appDbContext.Database.ExecuteSqlRawAsync(SqlRaw);
                if (resultExec == 0)
                {

                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }


        [HttpPut()]
        public async Task<ActionResult<ApiResponse_v1>> Put([FromBody] ClienteAprobacion document)
        {
            ApiResponse_v1 apiResponse_Save_V3 = new ApiResponse_v1();
            apiResponse_Save_V3.cantidad_registros = 0;
            apiResponse_Save_V3.final = true;
            apiResponse_Save_V3.success = true;
            apiResponse_Save_V3.exito = true;
            apiResponse_Save_V3.mensaje = "Proceso realizado exitosamente";

            try
            {
                document.TIPOIDENTIFICACION = document.TIPOIDENTIFICACION?.ToUpper();
                document.NOMBRESCLIENTE = document.NOMBRESCLIENTE?.ToUpper();
                document.APELLIDOSCLIENTE = document.APELLIDOSCLIENTE?.ToUpper();
                document.DIRECCIONCLIENTE = document.DIRECCIONCLIENTE?.ToUpper();
                document.EMAILCLIENTE = document.EMAILCLIENTE?.ToLower();
                document.FECHACAMBIOESTADO = DateTime.Now;
                apiResponse_Save_V3.data = document;
            }
            catch (Exception ex)
            {
                apiResponse_Save_V3.success = false;
                apiResponse_Save_V3.mensaje = "" + ex.InnerException?.Message;
                apiResponse_Save_V3.data = null;
            }

            appDbContext.GENCLIENTEAPROBACION.Update(document);
            await appDbContext.SaveChangesAsync();
            return apiResponse_Save_V3;
        }
         
    }
}
