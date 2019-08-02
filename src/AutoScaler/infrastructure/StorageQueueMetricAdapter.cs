using System.Threading.Tasks;
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using AutoScaler.Domain;

namespace AutoScaler.Infrastructure
{
    public class StorageQueueMetricAdapter : IMetricAdapter
    {
        private CloudQueue queue;

        public StorageQueueMetricAdapter(string connectionString, string queueName)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var queueClient = account.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference(queueName);
            if (queue.ExistsAsync().GetAwaiter().GetResult())
            {
                Console.WriteLine($"Successfuly connected to queue {queueName}");
            }
            else
            {
                Console.Error.WriteLine($"Error: Queue {queueName} doesn't exist");
            }
        }

        public async Task<double> GetMetricAsync()
        {
            await queue.FetchAttributesAsync();
            return (double)queue.ApproximateMessageCount.Value;
        }
    }
}