namespace ResourceBuilder.Data.Structs
{
    public class ResultOperation
    {
        public _DataGroup[]? DataGroup { get; set; }
    }

    public class _DataGroup
    {
        public string? name { get; set; }
        public string? id { get; set; }
        public int TotalPages { get; set; }
        public DateTime LastDateCheck { get; set; }
        public bool WasDone { get; set; }
        public int Errors { get; set; }
    }
}
