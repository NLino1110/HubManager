namespace ResourceBuilder.Data
{
    public class JsonResponseObject
    {
        public bool success { get; set; }
        public bool exito { get; set; }
        public int cantidad_registros { get; set; }
        public bool final { get; set; }
        public dynamic? data { get; set; }
    }    
}
