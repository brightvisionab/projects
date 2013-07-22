using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using System.Data.Services.Client;
using BrightVision.Storage.Models;

namespace BrightVision.Storage.Repositories
{
    internal class UserTextNotificationContext : TableServiceContext
    {
        public UserTextNotificationContext(string baseUri, StorageCredentials credentials)
            : base(baseUri, credentials)
        {

        }

        public IQueryable<UserTextNotification> UserNotifications
        {
            get
            {
                return this.CreateQuery<UserTextNotification>("UserTextNotifications");
            }
        }
    }
}
