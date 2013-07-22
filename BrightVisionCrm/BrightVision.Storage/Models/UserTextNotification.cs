using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace BrightVision.Storage.Models
{
    public class UserTextNotification : TableServiceEntity
    {
        public string Title { get; set; }
        public string MessageText { get; set; }
        public DateTime MessageDate { get; set; }

        public string TargetUserName
        {
            get
            {
                return this.PartitionKey;
            }
            set
            {
                this.PartitionKey = value;
            }
        }
    }

}
