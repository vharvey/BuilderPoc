using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VolumeBuilderPoc
{
    public interface ITableStorageProxy
    {
        Task<IEnumerable<T>> ExecuteQuerySegmented<T>(TableQuery<T> query, TableContinuationToken token) where T : ITableEntity, new();
        Task ExecuteBatchAsync(TableBatchOperation batch);
    }
}
