using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace BrightVision.Storage.Queue
{
    public class StdQueue<T> : StorageBase where T : QueueMessageBase, new()
    {
        protected CloudQueue queue;
        protected CloudQueueClient client;

        public StdQueue(string queueName)
        {
            client = new CloudQueueClient(StorageBase.QueueBaseUri, StorageBase.Credentials);
            queue = client.GetQueueReference(queueName);
            queue.CreateIfNotExist();
        }

        public void AddMessage(T message)
        {
            CloudQueueMessage msg = new CloudQueueMessage(message.ToBinary());
            queue.AddMessage(msg);
        }

        public void DeleteMessage(CloudQueueMessage msg)
        {
            queue.DeleteMessage(msg);
        }

        public CloudQueueMessage GetMessage()
        {
            return queue.GetMessage(TimeSpan.FromSeconds(60));
        }
    }
}
