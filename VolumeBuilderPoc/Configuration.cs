namespace VolumeBuilderPoc
{
    public class Configuration
    {
        public int StampSize => 5;
        public int VolumeCapacity = 20;
        public string StorageConnectionString { get; }
        public string StorageKey { get; internal set; }
        public string StorageAccountName { get; internal set; }
    }
}
