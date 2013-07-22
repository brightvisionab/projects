using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Acl;
using ITHit.WebDAV.Server.Class2;
using ITHit.WebDAV.Server.MicrosoftExtensions;
using WebFileManager.NtfsStreamsEngine;

namespace WebFileManager
{
    /// <summary>
    /// Base class for items(folders, files) in this sample.
    /// </summary>
    public abstract class DavHierarchyItem : IHierarchyItem, ILock, IMsItem
    {
        /// <summary>
        /// Instance of <see cref="FileSystemInfo"/> corresponding to a file represented by this object.
        /// </summary>
        internal FileSystemInfo fileSystemInfo;

        /// <summary>
        /// Instance of <see cref="DavContext"/>.
        /// </summary>
        protected DavContext context;

        /// <summary>
        /// Encoded WebDAV relative path to the item.
        /// </summary>
        private readonly string path;

        /// <summary>
        /// User defined property values.
        /// </summary>
        private List<PropertyValue> propertyValues;

        /// <summary>
        /// Locks acquired on the item.
        /// </summary>
        private List<DateLockInfo> explicitLocks;

        /// <summary>
        /// Initializes a new instance of the DavHierarchyItem class.
        /// </summary>
        /// <param name="fileSystemInfo">Corresponding <see cref="FileSystemInfo"/> to this file/folder.</param>
        /// <param name="context">Instance of <see cref="DavContext"/>.</param>
        /// <param name="parentPath">Encoded relative WebDAV path to parent folder.</param>
        protected DavHierarchyItem(FileSystemInfo fileSystemInfo, DavContext context, string parentPath)
        {
            this.fileSystemInfo = fileSystemInfo;
            this.context = context;
            this.path = parentPath;
        }

        /// <summary>
        /// Gets name of the item.
        /// </summary>
        public string Name
        {
            get { return fileSystemInfo.Name; }
        }

        /// <summary>
        /// Gets date when the item was created in UTC.
        /// </summary>
        public DateTime Created
        {
            get { return fileSystemInfo.CreationTimeUtc; }
        }

        /// <summary>
        /// Gets date when the item was last modified in UTC.
        /// </summary>
        public DateTime Modified
        {
            get { return fileSystemInfo.LastWriteTimeUtc; }
        }

        /// <summary>
        /// Gets path of the item where each part between slashes is encoded.
        /// </summary>
        public virtual string Path
        {
            get { return path; }
        }

        /// <summary>
        /// Returns parent folder of the item.
        /// </summary>
        /// <returns>Parent folder.</returns>
        public DavFolder GetParent()
        {
            //Calculate local file system path of parent item.
            string parentLocalPath = fileSystemInfo.FullName.TrimEnd('\\');
            int index = parentLocalPath.LastIndexOf('\\');
            if (index <= context.RepositoryPath.Length - 1)
            {
                return null;
            }

            var directory = new DirectoryInfo(parentLocalPath.Remove(index));

            //Calculate WebDAV root relative parent path.
            string parentPath = Path.TrimEnd('/');
            int ind = parentPath.LastIndexOf('/');
            if (ind >= 0)
            {
                parentPath = parentPath.Substring(0, ind);
            }

            return new DavFolder(directory, context, parentPath);
        }

        /// <summary>
        /// Creates a copy of this item with a new name in the destination folder.
        /// </summary>
        /// <param name="destFolder">Destination folder.</param>
        /// <param name="destName">Name of the destination item.</param>
        /// <param name="deep">Indicates whether to copy entire subtree.</param>
        /// <param name="multistatus">If some items fail to copy but operation in whole shall be continued, add
        /// information about the error into <paramref name="multistatus"/> using 
        /// <see cref="MultistatusException.AddInnerException(string,ITHit.WebDAV.Server.DavException)"/>.
        /// </param>
        public abstract void CopyTo(
            IItemCollection destFolder,
            string destName,
            bool deep,
            MultistatusException multistatus);

        /// <summary>
        /// Moves this item to the destination folder under a new name.
        /// </summary>
        /// <param name="destFolder">Destination folder.</param>
        /// <param name="destName">Name of the destination item.</param>
        /// <param name="multistatus">If some items fail to copy but operation in whole shall be continued, add
        /// information about the error into <paramref name="multistatus"/> using 
        /// <see cref="MultistatusException.AddInnerException(string,ITHit.WebDAV.Server.DavException)"/>.
        /// </param>
        public abstract void MoveTo(IItemCollection destFolder, string destName, MultistatusException multistatus);

        /// <summary>
        /// Deletes this item.
        /// </summary>
        /// <param name="multistatus">If some items fail to delete but operation in whole shall be continued, add
        /// information about the error into <paramref name="multistatus"/> using
        /// <see cref="MultistatusException.AddInnerException(string,ITHit.WebDAV.Server.DavException)"/>.
        /// </param>
        public abstract void Delete(MultistatusException multistatus);

        /// <summary>
        /// Retrieves user defined property values.
        /// </summary>
        /// <param name="names">Names of dead properties which values to retrieve.</param>
        /// <param name="allprop">Whether all properties shall be retrieved.</param>
        /// <returns>Property values.</returns>
        public IEnumerable<PropertyValue> GetProperties(IList<PropertyName> names, bool allprop)
        {
            if (allprop)
            {
                // get all properties
                return getPropertyValues();
            }

            //Get requeste property values.
            return getPropertyValues().Where(property => names.Contains(property.QualifiedName));
        }

        /// <summary>
        /// Retrieves names of all user defined properties.
        /// </summary>
        /// <returns>Property names.</returns>
        public IEnumerable<PropertyName> GetPropertyNames()
        {
            return getPropertyValues().Select(p => p.QualifiedName);
        }
        /// <summary>
        /// Check that if the item is locked then client has submitted correct lock token.
        /// </summary>
        public void RequireHasToken()
        {
            var allLocks = getExplicitLocks().ToList();
            if (allLocks.Count == 0)
            {
                return;
            }

            IList<string> clientLockTokens = context.Request.ClientLockTokens;
            if (allLocks.Where(l => clientLockTokens.Contains(l.LockToken)).Any())
            {
                return;
            }

            throw new LockedException();
        }

        /// <summary>
        /// Ensure that there are no active locks on the item.
        /// </summary>
        /// <param name="skipShared">Whether shared locks shall be checked.</param>
        public void RequireUnlocked(bool skipShared)
        {
            if (skipShared && getExplicitLocks().Where(l => l.Level == LockLevel.Exclusive).Any())
            {
                throw new LockedException();
            }

            if (!skipShared && getExplicitLocks().Any())
            {
                throw new LockedException();
            }
        }

        /// <summary>
        /// Saves dead property values to NTFS alternate data stream.
        /// </summary>
        /// <param name="setProps">Properties to be set.</param>
        /// <param name="delProps">Properties to be deleted.</param>
        /// <param name="multistatus">We don't use it here.</param>
        public virtual void UpdateProperties(
            IList<PropertyValue> setProps,
            IList<PropertyName> delProps,
            MultistatusException multistatus)
        {
            RequireHasToken();

            foreach (PropertyValue propToSet in setProps)
            {
                var existingProp = getPropertyValues()
                    .Where(p => p.QualifiedName == propToSet.QualifiedName).FirstOrDefault();

                if (existingProp != null)
                {
                    existingProp.Value = propToSet.Value;
                }
                else
                {
                    getPropertyValues().Add(propToSet);
                }
            }

            getPropertyValues().RemoveAll(prop => delProps.Contains(prop.QualifiedName));

            context.FileOperation(
                this,
                () => RewriteStream("Properties", propertyValues),
                Privilege.Write);
        }
        /// <summary>
        /// Retrieves non expired locks for this item.
        /// </summary>
        /// <returns>Enumerable with information about locks.</returns>
        public IEnumerable<LockInfo> GetActiveLocks()
        {
            return getExplicitLocks().Select(
                    l => new LockInfo
                         {
                             IsDeep = l.IsDeep,
                             Level = l.Level,
                             Owner = l.ClientOwner,
                             LockRoot = l.LockRoot,
                             TimeOut = l.Expiration == DateTime.MaxValue ?
                                TimeSpan.MaxValue :
                                l.Expiration - DateTime.UtcNow,
                             Token = l.LockToken
                         });
        }


        /// <summary>
        /// Locks this item.
        /// </summary>
        /// <param name="level">Whether lock is share or exclusive</param>
        /// <param name="isDeep">Whether lock is deep.</param>
        /// <param name="requestedTimeOut">Lock timeout which was requested by client.
        /// Server may ignore this parameter and set any timeout.</param>
        /// <param name="owner">Owner of the lock as specified by client.</param> 
        /// <returns>
        /// Instance of <see cref="LockResult"/> with information about the lock.
        /// </returns>
        public LockResult Lock(LockLevel level, bool isDeep, TimeSpan? requestedTimeOut, string owner)
        {
            RequireUnlocked(level == LockLevel.Shared);

            string token = Guid.NewGuid().ToString();

            TimeSpan timeOut;
            if (!requestedTimeOut.HasValue || requestedTimeOut == TimeSpan.MaxValue)
            {
                // If timeout is absent or infinit timeout requested,
                // grant 5 minute lock.
                timeOut = TimeSpan.FromMinutes(5);
            }
            else
            {
                timeOut = requestedTimeOut.Value;
            }

            DateTime expiration = DateTime.UtcNow + timeOut;

            explicitLocks.Add(new DateLockInfo
                                  {
                                      Expiration = expiration,
                                      IsDeep = false,
                                      Level = level,
                                      LockRoot = Path,
                                      LockToken = token,
                                      ClientOwner = owner,
                                      TimeOut = timeOut
                                  });
            saveLocks();

            return new LockResult(token, timeOut);
        }

        /// <summary>
        /// Updates lock timeout information on this item.
        /// </summary>
        /// <param name="token">Lock token.</param>
        /// <param name="requestedTimeOut">Lock timeout which was requested by client.
        /// Server may ignore this parameter and set any timeout.</param>
        /// <returns>
        /// Instance of <see cref="LockResult"/> with information about the lock.
        /// </returns>
        public RefreshLockResult RefreshLock(string token, TimeSpan? requestedTimeOut)
        {
            DateLockInfo li = getExplicitLocks().Where(l => l.LockToken == token).SingleOrDefault();

            if (li == null)
            {
                throw new DavException("Lock can not be found.", DavStatus.PRECONDITION_FAILED);
            }

            if (requestedTimeOut.HasValue && requestedTimeOut != TimeSpan.MaxValue)
            {
                // Update timeout if it is specified and not Infinity.
                // Otherwise leave previous timeout.
                li.TimeOut = requestedTimeOut.Value;
            }

            li.Expiration = DateTime.UtcNow + li.TimeOut;

            saveLocks();

            return new RefreshLockResult(li.Level, li.IsDeep, li.TimeOut, li.ClientOwner);
        }

        /// <summary>
        /// Removes lock with the specified token from this item.
        /// </summary>
        /// <param name="lockToken">Lock with this token should be removed from the item.</param>
        public void Unlock(string lockToken)
        {
            int i = getExplicitLocks().FindIndex(li => li.LockToken == lockToken);
            if (i >= 0)
            {
                getExplicitLocks().RemoveAt(i);
            }
            else
            {
                throw new DavException("The lock could not be found.", DavStatus.PRECONDITION_FAILED);
            }

            saveLocks();
        }
        /// <summary>
        /// Returns Windows file attributes (readonly, hidden etc.) for this file/folder.
        /// </summary>
        /// <returns>Windows file attributes.</returns>
        public FileAttributes GetFileAttributes()
        {
            if (Name.StartsWith("."))
            {
                return fileSystemInfo.Attributes | FileAttributes.Hidden;
            }
            return fileSystemInfo.Attributes;
        }

        /// <summary>
        /// Sets Windows file attributes (readonly, hidden etc.) on this item.
        /// </summary>
        /// <param name="value">File attributes.</param>
        public void SetFileAttributes(FileAttributes value)
        {
            File.SetAttributes(fileSystemInfo.FullName, value);
        }

        /// <summary>
        /// Retrieves full local path for this file/folder.
        /// </summary>
        /// <returns>Full local path.</returns>
        internal string GetFullPath()
        {
            return fileSystemInfo.FullName.TrimEnd('\\');
        }

        /// <summary>
        /// Returns path of the item in the hierarchy tree.
        /// </summary>
        internal string GetHierarchyPath()
        {
            int index = context.RepositoryPath.Length;
            if (GetFullPath().Length > index)
            {
                index++;
            }

            return GetFullPath().Remove(0, index).Replace('\\', '/');
        }

        /// <summary>
        /// Gets NTFS stream with specified name and deserializes object of type <typeparamref name="T"/>
        /// from it.
        /// </summary>
        /// <typeparam name="T">Type of object serialized in stream.</typeparam>
        /// <param name="streamName">Name of NTFS stream stored as NTFS stream in this file/folder.</param>
        /// <param name="logger">Instance of <see cref="logger"/>.</param>
        /// <returns>Deserialized object.</returns>
        internal T GetStreamAndDeserialize<T>(string streamName, ILogger logger) where T : new()
        {
            return context.FileOperation(
                this,
                () => SerializationUtils.GetStreamAndDeserialize<T>(streamName, fileSystemInfo, logger),
                Privilege.Read);
        }

        /// <summary>
        /// Serializes object <paramref name="o"/> and saves it into specified NTFS stream.
        /// </summary>
        /// <typeparam name="T">Type of object to be serialized.</typeparam>
        /// <param name="streamName">NTFS stream name.</param>
        /// <param name="o">Object to be serialized.</param>
        protected abstract void RewriteStream<T>(string streamName, T o);

        /// <summary>
        /// Deletes NTFS stream with specified name.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="streamName">NTFS stream name.</param>
        protected abstract void DeleteStream(string fileName, string streamName);

        /// <summary>
        /// Creates <see cref="DavFolder"/> with local path <see cref="localPath"/> and encoded WebDAV relative path
        /// <see cref="path"/>.
        /// </summary>
        /// <param name="localPath">Full local folder path.</param>
        /// <param name="webDavPath">Encoded WebDAV relative path for this folder.</param>
        /// <returns>Instance of <see cref="DavFolder"/>.</returns>
        protected DavFolder GetDavFolder(string localPath, string webDavPath)
        {
            return new DavFolder(new DirectoryInfo(localPath), context, webDavPath);
        }

        /// <summary>
        /// Creates <see cref="DavFile"/> with local path <see cref="localPath"/> and encoded WebDAV relative path
        /// <see cref="path"/>.
        /// </summary>
        /// <param name="localPath">Full local folder path.</param>
        /// <param name="webDavPath">Encoded WebDAV relative path for this folder.</param>
        /// <returns>Instance of <see cref="DavFolder"/>.</returns>
        protected DavFile GetDavFile(string localPath, string webDavPath)
        {
            return new DavFile(new FileInfo(localPath), context, webDavPath);
        }

        /// <summary>
        /// Retrieves non-expired locks acquired on this item.
        /// </summary>
        /// <returns>List of locks with their expiration dates.</returns>
        private List<DateLockInfo> getExplicitLocks()
        {
            if (explicitLocks == null)
            {
                explicitLocks = GetStreamAndDeserialize<List<DateLockInfo>>("Locks", context.Logger);
                explicitLocks.RemoveAll(li => li.Expiration <= DateTime.UtcNow);
                explicitLocks.ForEach(l => l.LockRoot = Path);
            }

            return explicitLocks;
        }

        /// <summary>
        /// Retrieves list of user defined propeties for this item.
        /// </summary>
        /// <returns>List of user defined properties.</returns>
        private List<PropertyValue> getPropertyValues()
        {
            if (propertyValues == null)
            {
                // Deserialize property values from alternate data stream.
                propertyValues = GetStreamAndDeserialize<List<PropertyValue>>("Properties", context.Logger);
            }

            return propertyValues;
        }

        /// <summary>
        /// Saves locks acquired on this file/folder in NTFS stream.
        /// </summary>
        private void saveLocks()
        {
            if (getExplicitLocks().Count > 0)
            {
                RewriteStream("Locks", explicitLocks);
            }
            else
            {
                DeleteStream(fileSystemInfo.FullName, "Locks");
            }
        }
    }
}