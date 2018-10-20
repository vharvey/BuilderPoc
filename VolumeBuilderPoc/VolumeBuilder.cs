using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VolumeBuilderPoc
{
    public class VolumeBuilder
    {
        private readonly Configuration _config;
        private ITableStorageProxy _tableProxy;

        public VolumeBuilder(Configuration config)
        {
            _config = config;
        }

        public List<Stamp> WriteVolume(IList<Stamp> sourceStamps, int volumeNumber, string tableName)
        {
            var result = new List<Stamp>();

            if ((sourceStamps == null) || !sourceStamps.Any())
                return result;

            var volume = new Volume(volumeNumber);
            var ordered = sourceStamps.OrderByDescending(s => s.Count);
            var remainingStamps = new List<Stamp>(sourceStamps);
            var i = 0;
            var currentStamp = sourceStamps[i];
            while (volume.EventCount + currentStamp.Count < _config.VolumeCapacity)
            {
                volume.Stamps.Add(sourceStamps[i]);
                remainingStamps.Remove(sourceStamps[i]);
                volume.EventCount += sourceStamps[i].Count;
                ++i;
                currentStamp = sourceStamps[i];
            }

            var required = _config.VolumeCapacity - volume.EventCount;
            if (required > 0)
            {
                var split = SplitStamp(currentStamp, required, tableName);
                volume.Stamps.Add(split.Item1);
                remainingStamps.Remove(currentStamp);
                remainingStamps.Insert(0, split.Item2);
            }

            return remainingStamps;
        }

        private Tuple<Stamp, Stamp> SplitStamp(Stamp currentStamp, int required, string tableName)
        {
            var stampToInclude = new Stamp(currentStamp.Id + "a");
            var eventCount = 0;
            var stampPartIndex = 0;
            for (; eventCount + currentStamp.PartCounts[stampPartIndex] < required; ++stampPartIndex)
                stampToInclude.PartCounts.Add(currentStamp.PartCounts[stampPartIndex]);

            var remainder = required - eventCount;
            var task =  SplitStampPart(currentStamp, stampPartIndex, remainder, tableName);
            task.Wait();
            return task.Result;
        }

        private async Task<Tuple<Stamp, Stamp>> SplitStampPart(Stamp currentStamp, int stampPartIndex, int count, string tableName)
        {
            var query = new TableQuery<StampPartEntity>
            {
                FilterString = $"PartitionKey {QueryComparisons.Equal} {currentStamp.Id}"
            };

            var token = new TableContinuationToken();

            var proxy = new TableStorageProxy(_config, tableName);
            var stampParts = await proxy.ExecuteQuerySegmented(query, token);
            var partToSplit = stampParts.Skip(stampPartIndex).First();

            var batch = new TableBatchOperation
            {
                TableOperation.Delete(partToSplit)
            };
            var events = Tuple.Create(string.Join(",", partToSplit.Events.Take(count)), string.Join(",", partToSplit.Events.Skip(count).Select(e => e)));
            batch.Add(TableOperation.InsertOrReplace(new StampPartEntity(partToSplit.PartitionKey, partToSplit.RowKey + 'a') { Events = events.Item1 }));
            batch.Add(TableOperation.InsertOrReplace(new StampPartEntity(partToSplit.PartitionKey, partToSplit.RowKey + 'b') { Events = events.Item2 }));
            await _tableProxy.ExecuteBatchAsync(batch);

            var split = Tuple.Create(new Stamp(currentStamp.Id), new Stamp(currentStamp.Id));
            split.Item1.PartCounts.AddRange(currentStamp.PartCounts.Take(stampPartIndex));
            split.Item1.PartCounts.Add(events.Item1.Length);
            split.Item2.PartCounts.Add(events.Item2.Length);
            split.Item2.PartCounts.AddRange(currentStamp.PartCounts.Skip(stampPartIndex).Select(p => p));
            return split;
        }
    }
}
