using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using System.Configuration;
using BrightVision.Common.Business;

namespace BrightVision.Storage
{
    public abstract class StorageBase
    {
        private static StorageCredentialsAccountAndKey credentials;
        private static string tableBaseUri;
        private static string blobBaseUri;
        private static string queueBaseUri;

        public StorageBase()
        {
            if (StorageBase.Credentials == null)
            {
                LoadConfiguration();
            }            
        }

        private static void LoadConfiguration()
        {
            string accountName = ConfigManager.AppSettings["AccountName"];
            string accountSharedKey = ConfigManager.AppSettings["AccountSharedKey"];
            tableBaseUri = ConfigManager.AppSettings["TableStorageEndpoint"];
            queueBaseUri = ConfigManager.AppSettings["QueueStorageEndpoint"];
            blobBaseUri = ConfigManager.AppSettings["BlobStorageEndpoint"];
            credentials = new StorageCredentialsAccountAndKey(accountName, accountSharedKey);                        
        }

        protected static StorageCredentialsAccountAndKey Credentials
        {
            get {
                return credentials;
            }
        }

        protected static string TableBaseUri
        {
            get
            {
                return tableBaseUri;
            }
        }

        protected static string QueueBaseUri
        {
            get
            {
                return queueBaseUri;
            }
        }

        protected static string BlobBaseUri
        {
            get
            {
                return blobBaseUri;
            }
        }
        
    }
}
