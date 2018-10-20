using Microsoft.WindowsAzure.Storage.Table;

namespace VolumeBuilderPoc
{
    public class StampPartEntity : TableEntity
    {
        public StampPartEntity()
        {
        }

        public StampPartEntity(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {
        }

        public string Events { get; set; }
    }
}
