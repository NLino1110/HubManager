namespace Models.DMSA.Shared.Structs
{
    public class ItemBuild
    {
        public string Name { get; set; }
        public string ActioName { get; set; }
        public bool Process { get; set; }

        //Specials
        public int progress_percentage { get; set; }
    }
}
