using DataSourceManager.Tools;

//using Entidades.Externals;
//using Entidades.Sac;
//using Entidades.Customer;
//using Entidades.SyncTask;
//using Entidades.Tmp;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Models.DMSA.Mbw.Clientes;

//using Models.DMSA.Mbw.Especiales;

using Models.DMSA.Mbw.Inventario;
using Models.DMSA.Mbw.Core;
using Models.DMSA.Mbw.Sales;
using Models.DMSA.Mbw.Query;
using System.Data.Entity.Infrastructure;

namespace DataSourceManager
{
    public partial class AppDbContext : DbContext
    {
        //public virtual DbSet<Saldo> SacSaldo { set; get; }

        //public virtual DbSet<SacDocument> SacDocument { set; get; }

        //public virtual DbSet<CustomerData> CustomerData { set; get; }

        //public virtual DbSet<SkuServiceValue> skuServiceValue { set; get; }
        //public virtual DbSet<SkuService> skuService { set; get; }

        ////Extras temporales
        //public virtual DbSet<Tmp_004> tmp_004 { set; get; }

        //MACRONEGOCIOS MY BUSSINESS WEB
        public virtual DbSet<ClienteAprobacion> GENCLIENTEAPROBACION { set; get; }

        //public virtual DbSet<NotaCreditoDet> SPNotaCreditoDet { set; get; }
        public virtual DbSet<OdooLopdp> ODDO_LOPDP { set; get; }

        public virtual DbSet<GenParametros> GENPARAMETROS { set; get; }

        //MBW
        public virtual DbSet<GenMarca> GENMARCAS { set; get; }

        public virtual DbSet<GenEmpresa> GENEMPRESAS { set; get; }
        public virtual DbSet<FacPedido> FACPEDIDOS { set; get; }
        public virtual DbSet<FacPedidoDet> FACPEDIDODET { set; get; }
        public virtual DbSet<GenAgencias> GENAGENCIAS { set; get; }

        //public virtual DbSet<GenParametros> GENPARAMETROS { set; get; }
        public virtual DbSet<CntTipoCmpr> CNTTIPOCMPR { set; get; }

        public virtual DbSet<GenClientes> GENCLIENTES { set; get; }
        public virtual DbSet<GenUnidadesMedida> GENUNIDADESMEDIDA { set; get; }
        public virtual DbSet<FacNivelesPrecios> FACNIVELESPRECIOS { set; get; }
        public virtual DbSet<GenVendedores> GENVENDEDORES { set; get; }
        public virtual DbSet<GenMonedas> GENMONEDAS { set; get; }
        public virtual DbSet<Faccmprventa> FACCMPRVENTA { set; get; }

        public virtual DbSet<FacPromociones> FACPROMOCIONES { set; get; }
        public virtual DbSet<FacCmprVentaNoRealizada> FACCMPRVENTANOREALIZADA { set; get; }
        public virtual DbSet<FacRangoDescuentoLote> FACRANGODESCUENTOLOTE { set; get; }
        public virtual DbSet<FacBonificadosXArticulo> FACBONIFICADOSXARTICULO { set; get; }
        public virtual DbSet<FacBonificadosXLinea> FACBONIFICADOSXLINEA { set; get; }
        public virtual DbSet<GenEstados> GENESTADOS { set; get; }

        public virtual DbSet<InvStock> INVSTOCKS { set; get; }
        public virtual DbSet<InvstockLotes> INVSTOCKLOTES { set; get; }
        public virtual DbSet<InvLotes> INVLOTES { set; get; }
        public virtual DbSet<GenArticulos> GENARTICULOS { set; get; }
        public virtual DbSet<GenEquivalenciaUniMedidas> GENEQUIVALENCIAUNIMEDIDAS { set; get; }

        public virtual DbSet<BodegasXAgencia> BODEGASXAGENCIA { set; get; }
        public virtual DbSet<CajCajas> CAJCAJAS { set; get; }

        public virtual DbSet<ArticulosXEmpresa> ARTICULOSXEMPRESA { set; get; }
        public virtual DbSet<GenArticulosWeb> GENARTICULOSWEB { set; get; }
        public virtual DbSet<FacPreciosVenta> FACPRECIOSVENTA { set; get; }
        public virtual DbSet<FacPreciosAlmacen> FACPRECIOSALMACEN { set; get; }

        public virtual DbQuery<StockResult> StockResultsQuery { get; set; }
        public virtual DbQuery<InvoiceHeader> InvoiceHeaderQuery { get; set; }        
        public virtual DbQuery<InvoiceDetails> InvoiceDetailsQuery { get; set; }
        public virtual DbQuery<InvoicePayments> InvoicePaymentsQuery { get; set; }
    }
}