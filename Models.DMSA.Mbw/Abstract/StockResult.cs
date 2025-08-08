namespace Models.DMSA.Mbw.Abstract
{
    public class StockResult
    {
        public long CodBodegaAgencia { get; set; } // Cambia el tipo según corresponda (int, long, etc.)
        public string? NombreBodega { get; set; }
        public double CantDisponibleWms { get; set; } // Cambia el tipo si es diferente
        public double MinimoVtaWeb { get; set; } // Cambia el tipo si es diferente
        public double Cantidad { get; set; } // Cambia el tipo si es diferente
        public double CantidadReservada { get; set; } // Cambia el tipo si es diferente
    }
}
