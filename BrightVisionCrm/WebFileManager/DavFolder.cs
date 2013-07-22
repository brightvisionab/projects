using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Acl;
using ITHit.WebDAV.Server.Class1;
using WebFileManager.NtfsStreamsEngine;

namespace WebFileManager
{
    /// <summary>
    /// Folder in WebDAV repository.
    /// </summary>
    public class DavFolder : DavHierarchyItem, IFolder
    {
        /// <summary>
        /// Corresponding instance of <see cref="DirectoryInfo"/>.
        /// </summary>
        private readonly DirectoryInfo dirInfo;

        /// <summary>
        /// Initializes a new instance of the DavFolder class.
        /// </summary>
        /// <param name="directory">Instance of <see cref="DirectoryInfo"/> class with information about the folder in file system.</param>
        /// <param name="context">Instance of <see cref="DavContext"/>.</param>
        /// <param name="path">Relative to WebDAV root folder path.</param>
        public DavFolder(DirectoryInfo directory, DavContext context, string path)
            : base(directory, context, path)
        {
            dirInfo = directory;
        }

        /// <summary>
        /// Retrieves childrent of this folder.
        /// </summary>
        /// <param name="propNames">List of properties to retrieve with the children. They will be queried by the engine later.</param>
        /// <returns>Children of the folder.</returns>
        public virtual IEnumerable<IHierarchyItem> GetChildren(IList<PropertyName> propNames)
        {
            //Enumerate all child files and folders.
            FileSystemInfo[] fileInfos =
                context.FileOperation(
                    this,
                    () => dirInfo.GetFileSystemInfos(),
                    Privilege.Read);

            // return DavFile or DavFolder for every child item.
            foreach (FileSystemInfo item in fileInfos)
            {
                var childPath = Path + EncodeUtil.EncodeUrlPart(item.Name);
                if (item is DirectoryInfo)
                {
                    yield return new DavFolder((DirectoryInfo)item, context, childPath + "/");
                }
                else
                {
                    yield return new DavFile((FileInfo)item, context, childPath);
                }
            }
        }

        /// <summary>
        /// Creates file with name <paramref name="name"/> in this folder.
        /// </summary>
        /// <param name="name">Name of the file.</param>
        /// <returns>The file just created.</returns>
        public IFile CreateFile(string name)
        {
            RequireHasToken();

            string fileName = System.IO.Path.Combine(fileSystemInfo.FullName, name);
            
            context.FileOperation(
                this,
                () => new FileStream(fileName, FileMode.CreateNew).Dispose(),
                Privilege.Bind);

            return GetDavFile(fileName, Path + EncodeUtil.EncodeUrlPart(name));
        }

        /// <summary>
        /// Create folder with specified name in the folder.
        /// </summary>
        /// <param name="name">Name of the folder.</param>
        public void CreateFolder(string name)
        {
            RequireHasToken();

            context.FileOperation(
                this,
                () => dirInfo.CreateSubdirectory(name),
                Privilege.Bind);
        }

        /// <summary>
        /// Delete this item.
        /// </summary>
        /// <param name="multistatus"><see cref="MultistatusException"/> to populate with child files and folders failed to delete.</param>
        public override void Delete(MultistatusException multistatus)
        {
            if (GetParent() == null)
            {
                throw new DavException("Cannot delete root.", DavStatus.ACCESS_DENIED);
            }
            RequireHasToken();

            bool allChildrenDeleted = true;
            foreach (IHierarchyItem child in GetChildren(new PropertyName[0]))
            {
                try
                {
                    child.Delete(multistatus);
                }
                catch (DavException ex)
                {
                    //continue the operation if a child failed to delete. Tell client about it by adding to multistatus.
                    multistatus.AddInnerException(child.Path, ex);
                    allChildrenDeleted = false;
                }
            }

            if (allChildrenDeleted)
            {
                context.FileOperation(
                    this,
                    () => dirInfo.Delete(),
                    Privilege.Unbind);
            }
        }
        
        /// <summary>
        /// Move this folder to folder <paramref name="destFolder"/>.
        /// </summary>
        /// <param name="destFolder">Destination folder.</param>
        /// <param name="destName">Name for this folder at destination.</param>
        /// <param name="multistatus">Instance of <see cref="MultistatusException"/>
        /// to fill with errors ocurred while moving child items.</param>
        public override void MoveTo(IItemCollection destFolder, string destName, MultistatusException multistatus)
        {
            RequireHasToken();

            var targetFolder = destFolder as DavFolder;
            if (targetFolder == null)
            {
                throw new DavException("Target folder doesn't exist", DavStatus.CONFLICT);
            }

            if (isRecursive(targetFolder))
            {
                throw new DavException("Cannot move folder to its subtree.", DavStatus.FORBIDDEN);
            }

            string newDirPath = System.IO.Path.Combine(targetFolder.GetFullPath(), destName);
            string targetItemPath = targetFolder.Path + EncodeUtil.EncodeUrlPart(destName);
        
            try
            {
                //remove item with the same name at destination if it exists.
                if (File.Exists(newDirPath))
                {
                    GetDavFile(newDirPath, targetItemPath).Delete(multistatus);
                }

                if (Directory.Exists(newDirPath))
                {
                    GetDavFolder(newDirPath, targetItemPath).Delete(multistatus);
                }

                // Create folder in destination.
                targetFolder.CreateFolder(destName);
            }
            catch (DavException ex)
            {
                //Continue the operation but report error with destination path to client.
                multistatus.AddInnerException(targetItemPath, ex);
                return;
            }

            //Move child items
            bool movedAll = true;
            var createdFolder = GetDavFolder(newDirPath, targetItemPath);
            foreach (DavHierarchyItem item in GetChildren(new PropertyName[0]))
            {
                try
                {
                    item.MoveTo(createdFolder, item.Name, multistatus);
                }
                catch (DavException ex)
                {
                    //continue the operation but report error with child item to client.
                    multistatus.AddInnerException(item.Path, ex);
                    movedAll = false;
                }
            }

            if (movedAll)
            {
                Delete(multistatus);
            }
        }

        /// <summary>
        /// Copies the folder to another folder.
        /// </summary>
        /// <param name="destFolder">Destination parent folder.</param>
        /// <param name="destName">Destination folder name.</param>
        /// <param name="deep">Whether child items shall be copied.</param>
        /// <param name="multistatus">Instance of <see cref="MultistatusException"/>
        /// to fill with errors ocurred while moving child items.</param>
        public override void CopyTo(
            IItemCollection destFolder,
            string destName,
            bool deep,
            MultistatusException multistatus)
        {
            var targetFolder = destFolder as DavFolder;
            if (targetFolder == null)
            {
                throw new DavException("Target folder doesn't exist", DavStatus.CONFLICT);
            }

            if (isRecursive(targetFolder))
            {
                throw new DavException("Cannot copy to subfolder", DavStatus.FORBIDDEN);
            }

            string newDirLocalPath = System.IO.Path.Combine(targetFolder.GetFullPath(), destName);
            string targetItemPath = targetFolder.Path + EncodeUtil.EncodeUrlPart(destName);

            // Create folder at destination.
            try
            {
                if (!Directory.Exists(newDirLocalPath))
                {
                    targetFolder.CreateFolder(destName);
                }
            }
            catch (DavException ex)
            {
                //continue, but report error to client for the target item.
                multistatus.AddInnerException(targetItemPath, ex);
            }

            //Copy children
            var createdFolder = GetDavFolder(newDirLocalPath, targetItemPath);
            foreach (DavHierarchyItem item in GetChildren(new PropertyName[0]))
            {
                if (!deep && item is DavFolder)
                {
                    continue;
                }

                try
                {
                    item.CopyTo(createdFolder, item.Name, deep, multistatus);
                }
                catch (DavException ex)
                {
                    // If a child item failed to copy we continue but report error to client.
                    multistatus.AddInnerException(item.Path, ex);
                }
            }
        }

        /// <summary>
        /// Serializes object <paramref name="o"/> and saves it into specified NTFS stream.
        /// </summary>
        /// <typeparam name="T">Type of object to be serialized.</typeparam>
        /// <param name="streamName">NTFS stream name.</param>
        /// <param name="o">Object to be serialized.</param>
        protected override void RewriteStream<T>(string streamName, T o)
        {
            context.FileOperation(
                this,
                () => SerializationUtils.RewriteStream(
                    streamName,
                    fileSystemInfo, o != null ? SerializationUtils.Serialize(o) : null),
                Privilege.Write);
        }

        /// <summary>
        /// Deletes NTFS stream with specified name.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="streamName">NTFS stream name.</param>
        protected override void DeleteStream(string fileName, string streamName)
        {
            new AlternateDataStreamInfo(fileName, streamName).Delete();
        }

        /// <summary>
        /// Determines whether <paramref name="destFolder"/> is inside this folder.
        /// </summary>
        /// <param name="destFolder">Folder to check.</param>
        /// <returns>Returns <c>true</c> if <paramref name="destFolder"/> is inside thid folder.</returns>
        private bool isRecursive(DavFolder destFolder)
        {
            return destFolder.Path.StartsWith(Path);
        }
    }
}
