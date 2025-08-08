using System;
using System.Collections.Generic;
using System.Text;

namespace VtexStrucs
{
    public class Siscom2vTexMatchProduct
    {
        public int id_promocion { get; set; }
        public int skuId { get; set; }
        public string Codigo_Siscom { get; set; }
        public string Nombre_Siscom { get; set; }
        public string Codigo_vTex { get; set; }
        public string Nombre_vTex { get; set; }
        public decimal Pvp { get; set; }
        public decimal Pvp_Final { get; set; }
        public decimal? listPrice { get; set; }
        public decimal costPrice { get; set; }
        public bool difPrecio { get; set; }
        public bool AlertaCodigo { get; set; }
        public bool Actualizar { get; set; }
        public bool NoExisteVtex { get; set; }
        public string Mensaje { get; set; }
        public int NumeroFila { get; set; }
        public int stockSiscom { get; set; }
        public int stockvTex { get; set; }
        public decimal per_discount { get; set; }

        public string prod_cod_prov { get; set; }
        public string es_promo_precio { get; set; }
        public string aplicavtex { get; set; }
        public decimal precio_promocion { get; set; }
        public decimal precio_lista { get; set; }
        public string PROD_MARKETPLACE { get; set; }
        public string PROD_ENSAMBLE { get; set; }
    }
}
