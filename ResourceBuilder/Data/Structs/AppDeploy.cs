namespace ResourceBuilder.Data.Structs
{
    public class AppDeploy
    {
        public AppDeploy() { }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public bool IsDeployed { get; set; }
        
    }
}
