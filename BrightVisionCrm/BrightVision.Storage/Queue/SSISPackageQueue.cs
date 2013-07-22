using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace BrightVision.Storage.Queue
{
    public class SSISPackageQueue : StdQueue<SSISPackageMessage>
    {
        public SSISPackageQueue()
            : base("ssispackageprofiling")
        {
        }
    }
}
