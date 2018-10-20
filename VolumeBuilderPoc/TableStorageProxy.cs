using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace VolumeBuilderPoc
{
    class TableStorageProxy : ITableStorageProxy
    {
        private readonly Configuration _config;
        private readonly CloudTable _table;

        public TableStorageProxy(Configuration config, string tableName)
        {
            _config = config;
            var acct = new CloudStorageAccount(new StorageCredentials(_config.StorageAccountName, _config.StorageKey), true);
            var client = acct.CreateCloudTableClient();
            var table = client.GetTableReference(tableName);
            var task = table.CreateIfNotExistsAsync();
            task.Wait();
        }

        public Task ExecuteBatchAsync(TableBatchOperation batch)
        {
            return _table.ExecuteBatchAsync(batch);
        }

        public async Task<IEnumerable<T>> ExecuteQuerySegmented<T>(TableQuery<T> query, TableContinuationToken token) where T : ITableEntity, new()
        {
            var entities = new List<T>();
            do
            {
                var result = await _table.ExecuteQuerySegmentedAsync(query, token);
                entities.AddRange(result.Results);
                token = result.ContinuationToken;
            } while (token != null);

            return entities;
        }
    }
}
