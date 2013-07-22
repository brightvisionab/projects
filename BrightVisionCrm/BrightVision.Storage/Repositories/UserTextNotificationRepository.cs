using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using BrightVision.Storage.Models;

namespace BrightVision.Storage.Repositories
{
    public class UserTextNotificationRepository : StorageBase
    {
        public const string EntitySetName = "UserTextNotifications";

        CloudTableClient tableClient;
        UserTextNotificationContext notificationContext;
        UserTextNotificationContext notificationContext2;

        public UserTextNotificationRepository()
            : base()
        {
            tableClient = new CloudTableClient(StorageBase.TableBaseUri, StorageBase.Credentials);
            notificationContext = new UserTextNotificationContext(StorageBase.TableBaseUri, StorageBase.Credentials);
            notificationContext2 = new UserTextNotificationContext(StorageBase.TableBaseUri, StorageBase.Credentials);
            tableClient.CreateTableIfNotExist(EntitySetName);
        }

        public UserTextNotification[] GetNotificationsForUser(string userId)
        {
            var q = from notification in notificationContext.UserNotifications
                    where notification.TargetUserName == userId
                    select notification;

            return q.ToArray();
        }

        public void AddNotification(UserTextNotification notification)
        {
            try {
                notification.RowKey = Guid.NewGuid().ToString();
                notificationContext.AddObject(EntitySetName, notification);                
                notificationContext.SaveChanges();
            } catch {
            }
        }

        public void DeleteNotification(UserTextNotification notification) {
            try {
                notificationContext2.AttachTo(EntitySetName, notification,"*");
                notificationContext2.DeleteObject(notification);
                notificationContext2.SaveChanges();
            } catch {
            }
        }
    }
}
