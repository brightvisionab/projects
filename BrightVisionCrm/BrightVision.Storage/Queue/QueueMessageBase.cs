using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.WindowsAzure.StorageClient;

namespace BrightVision.Storage.Queue
{
    public interface IQueueMessage
    {
        CloudQueueMessage OriginalMessage { get; set; }
    }

    [Serializable]
    public class QueueMessageBase
    {
        public byte[] ToBinary()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            ms.Position = 0;
            bf.Serialize(ms, this);
            byte[] output = ms.GetBuffer();
            ms.Close();
            return output;
        }

        public static T FromMessage<T>(CloudQueueMessage m)
        {
            byte[] buffer = m.AsBytes;
            MemoryStream ms = new MemoryStream(buffer);
            ms.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            return (T)bf.Deserialize(ms);
        }
    }

}
