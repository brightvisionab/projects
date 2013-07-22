using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Acl;
using ITHit.WebDAV.Server.Class1;
using ITHit.WebDAV.Server.ResumableUpload;
using WebFileManager.NtfsStreamsEngine;

namespace WebFileManager
{
    /// <summary>
    /// Represents file in WebDAV repository.
    /// </summary>
    public class DavFile : DavHierarchyItem, IFile, IResumableUpload, IUploadProgress
    {
        /// <summary>
        /// Corresponding <see cref="FileInfo"/>.
        /// </summary>
        private readonly FileInfo fileInfo;

        /// <summary>
        /// Size of chunks to upload/download.
        /// (1Mb) buffer size used when reading and writing file content.
        /// </summary>
        private const int bufSize = 1048576;

        /// <summary>
        /// Initializes a new instance of the DavFile class.
        /// </summary>
        /// <param name="file"><see cref="FileInfo"/> for corresponding object in file system.</param>
        /// <param name="context">Instance of <see cref="DavContext"/></param>
        /// <param name="path">Encoded path relative to WebDAV root.</param>
        public DavFile(FileInfo file, DavContext context, string path)
            : base(file, context, path)
        {
            fileInfo = file;
        }

        /// <summary>
        /// Gets content type.
        /// </summary>
        public virtual string ContentType
        {
            get { return MimeType.GetMimeType(fileSystemInfo.Extension) ?? "application/octet-stream"; }
        }

        /// <summary>
        /// Gets length of the file.
        /// </summary>
        public long ContentLength
        {
            get { return fileInfo.Length; }
        }

        /// <summary>
        /// Gets Etag of the file.
        /// </summary>
        public string Etag
        {
            get
            {
                var serialNumber = GetStreamAndDeserialize<int>("SerialNumber", context.Logger);
                return string.Format("{0}-{1}", Modified, serialNumber);
            }
        }

        /// <summary>
        /// Called when a client is downloading a file. Copies file contents to ouput stream.
        /// </summary>
        /// <param name="output">Stream to copy contents to.</param>
        /// <param name="startIndex">Index from.</param>
        /// <param name="count">Number of bytes.</param>
        public void Read(Stream output, long startIndex, long count)
        {
            //Set timeout to maximum value to be able to download large files.
            HttpContext.Current.Server.ScriptTimeout = int.MaxValue;
            if (context.Request.RawUrl.EndsWith("?download"))
            {
                AddContentDisposition();
            }

            context.FileOperation(
                this,
                () => readInternal(output, startIndex, count),
                Privilege.Read);
        }

        private void AddContentDisposition()
        {
            if (context.Request.UserAgent.Contains("MSIE")) // problem with file extensions in IE
            {
                var fileName = EncodeUtil.EncodeUrlPart(Name);
                context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
            }
            else
                context.Response.AddHeader("Content-Disposition", "attachment;");
        }

        private void readInternal(Stream output, long startIndex, long count)
        {
            var buffer = new byte[bufSize];
            using (var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileStream.Seek(startIndex, SeekOrigin.Begin);
                int bytesRead;
                var toRead = (int)Math.Min(count, bufSize);
                while (toRead > 0 && (bytesRead = fileStream.Read(buffer, 0, toRead)) > 0)
                {
                    output.Write(buffer, 0, bytesRead);
                    count -= bytesRead;
                }
            }
        }

        /// <summary>
        /// Called when a file or its part is being uploaded.
        /// </summary>
        /// <param name="content">Stream with data.</param>
        /// <param name="contentType">Content type.</param>
        /// <param name="startIndex">Starting byte in target file
        /// for which data comes in <paramref name="content"/> stream.</param>
        /// <param name="totalFileSize">Size of file as it will be after all parts are uploaded. -1 if unknown (in case of chunked upload).</param>
        /// <returns>Whether the whole stream has been written. This result is used by the engine to determine
        /// if auto checkin shall be performed (if auto versioning is used).</returns>
        public bool Write(Stream content, string contentType, long startIndex, long totalFileSize)
        {
            RequireHasToken();
            //Set timeout to maximum value to be able to upload large files.
            HttpContext.Current.Server.ScriptTimeout = int.MaxValue;
            return context.FileOperation(
                this,
                () => writeInternal(content, startIndex, totalFileSize),
                Privilege.Write);
        }
        
        private bool writeInternal(Stream content, long startIndex, long totalLength)
        {
            if (startIndex == 0 && fileInfo.Length > 0)
            {
                SafeNativeMethods.TruncateFile(fileInfo);
            }
            RewriteStream("TotalContentLength", totalLength >= 0 ? (object)totalLength : null);
            RewriteStream("SerialNumber", GetStreamAndDeserialize<int>("SerialNumber", context.Logger) + 1);

            using (var fileStream = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                if (fileStream.Length < startIndex)
                {
                    throw new DavException("Previous piece of file was not uploaded.", DavStatus.PRECONDITION_FAILED);
                }

                fileStream.Seek(startIndex, SeekOrigin.Begin);
                var buffer = new byte[bufSize];

                int lastBytesRead;
                while ((lastBytesRead = content.Read(buffer, 0, bufSize)) > 0)
                {
                    fileStream.Write(buffer, 0, lastBytesRead);
                    fileStream.Flush();
                }
            }

            return true;
        }
        
        /// <summary>
        /// Move file to destination folder.
        /// </summary>
        /// <param name="destFolder">Folder to move to.</param>
        /// <param name="destName">New name of the file.</param>
        /// <param name="multistatus">Instance of <see cref="MultistatusException"/> where we put errors which 
        /// occurred with other items involved and continue
        /// moving other items.</param>
        public override void MoveTo(IItemCollection destFolder, string destName, MultistatusException multistatus)
        {
            RequireHasToken();
            context.FileOperation(
                () => moveToInternal(destFolder, destName, multistatus));
        }

        private void moveToInternal(IItemCollection folder, string destName, MultistatusException multistatus)
        {
            var targetFolder = (DavFolder)folder;

            if (targetFolder == null || !Directory.Exists(targetFolder.GetFullPath()))
            {
                throw new DavException("Target directory doesn't exist", DavStatus.CONFLICT);
            }

            var newDirPath = System.IO.Path.Combine(targetFolder.GetFullPath(), destName);
            var targetPath = targetFolder.Path + EncodeUtil.EncodeUrlPart(destName);

            //If an item with the same name exists in target directory - remove it.
            try
            {
                if (File.Exists(newDirPath))
                {
                    GetDavFile(newDirPath, targetPath).Delete(multistatus);
                }
                else if (Directory.Exists(newDirPath))
                {
                    GetDavFolder(newDirPath, targetPath).Delete(multistatus);
                }
            }
            catch (DavException ex)
            {
                //Report exception to client and continue with other items by returning from recursion.
                multistatus.AddInnerException(targetPath, ex);
                return;
            }

            //Move the file.
            try
            {
                File.Move(fileSystemInfo.FullName, newDirPath);
            }
            catch (UnauthorizedAccessException)
            {
                //Exception occurred with the item for which MoveTo was called - fail the operation.
                var ex = new NeedPrivilegesException("Not enough privileges");
                ex.AddRequiredPrivilege(targetPath, Privilege.Bind);
                ex.AddRequiredPrivilege(GetParent().Path, Privilege.Unbind);
                throw ex;
            }
        }

        /// <summary>
        /// Delete the file.
        /// </summary>
        /// <param name="multistatus">We don't use it here.</param>
        public override void Delete(MultistatusException multistatus)
        {
            RequireHasToken();

            context.FileOperation(
                this,
                () => fileSystemInfo.Delete(),
                Privilege.Unbind);
        }

        /// <summary>
        /// Copies file to destination folder.
        /// </summary>
        /// <param name="destFolder">Destination folder.</param>
        /// <param name="destName">New file name.</param>
        /// <param name="deep">We don't use it here.</param>
        /// <param name="multistatus">We report exceptions which occurred with other items here.</param>
        public override void CopyTo(IItemCollection destFolder, string destName, bool deep, MultistatusException multistatus)
        {
            context.FileOperation(
                () => copyToInternal(destFolder, destName, multistatus));
        }

        private void copyToInternal(IItemCollection folder, string destName, MultistatusException multistatus)
        {
            var targetFolder = (DavFolder)folder;

            if (targetFolder == null || !Directory.Exists(targetFolder.GetFullPath()))
            {
                throw new DavException("Target directory doesn't exist", DavStatus.CONFLICT);
            }

            var newDirPath = System.IO.Path.Combine(targetFolder.GetFullPath(), destName);
            var targetPath = targetFolder.Path + EncodeUtil.EncodeUrlPart(destName);

            //If an item with the same name exists - remove it.
            try
            {
                if (File.Exists(newDirPath))
                {
                    GetDavFile(newDirPath, targetPath).Delete(multistatus);
                }
                else if (Directory.Exists(newDirPath))
                {
                    GetDavFolder(newDirPath, targetPath).Delete(multistatus);
                }
            }
            catch (DavException ex)
            {
                //Report error with other item to client.
                multistatus.AddInnerException(targetPath, ex);
                return;
            }

            //Copy the file.
            try
            {
                File.Copy(fileSystemInfo.FullName, newDirPath);
                DeleteStream(newDirPath, "Locks");
            }
            catch (UnauthorizedAccessException)
            {
                //Fail
                var ex = new NeedPrivilegesException("Not enough privileges");
                ex.AddRequiredPrivilege(GetParent().Path, Privilege.Bind);
                throw ex;
            }
        }
        
        /// <summary>
        /// Cancels resumable upload.
        /// </summary>
        public void CancelUpload()
        {
            RequireHasToken();

            // Client do not plan to restore upload. Remove any temporary files / cleanup resources here.
        }

        /// <summary>
        /// Gets date when last chunk was saved to this file.
        /// </summary>
        public DateTime LastChunkSaved
        {
            get { return fileInfo.Exists ? fileInfo.LastWriteTimeUtc : DateTime.MinValue; }
        }

        /// <summary>
        /// Gets number of bytes uploaded sofar.
        /// </summary>
        public long BytesUploaded
        {
            get { return ContentLength; }
        }

        /// <summary>
        /// Gets total length of the file being uploaded.
        /// </summary>
        public long TotalContentLength
        {
            get { return GetStreamAndDeserialize<long>("TotalContentLength", context.Logger); }
        }

        /// <summary>
        /// Return instance of <see cref="IUploadProgress"/> interface.
        /// </summary>
        /// <returns>Just returns this class.</returns>
        public IEnumerable<IResumableUpload> GetUploadProgress()
        {
            yield return this;
        }

        /// <summary>
        /// Serializes object <paramref name="o"/> and saves it into specified NTFS stream.
        /// </summary>
        /// <remarks>
        /// Preserves LastModified time.
        /// </remarks>
        /// <typeparam name="T">Type of object to be serialized.</typeparam>
        /// <param name="streamName">NTFS stream name.</param>
        /// <param name="o">Object to be serialized.</param>
        protected override void RewriteStream<T>(string streamName, T o)
        {
            context.FileOperation(
                this,
                () =>
                    {
                        DateTime lastWriteTimeUtc = fileSystemInfo.LastWriteTimeUtc;

                        SerializationUtils.RewriteStream(
                            streamName,
                            fileSystemInfo,
                            o != null ? SerializationUtils.Serialize(o) : null);

                        // Preserve last modification date.
                        SafeNativeMethods.SetLastWriteTimeUtc(fileSystemInfo.FullName, lastWriteTimeUtc);
                    },
                Privilege.Write);
        }

        /// <summary>
        /// Deletes NTFS stream with specified name.
        /// </summary>
        /// <remarks>
        /// Preserves LastModified time.
        /// </remarks>
        /// <param name="fileName">File name.</param>
        /// <param name="streamName">NTFS stream name.</param>
        protected override void DeleteStream(string fileName, string streamName)
        {
            DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(fileName);

            new AlternateDataStreamInfo(fileName, streamName).Delete();

            // Preserve last modification date.
            SafeNativeMethods.SetLastWriteTimeUtc(fileName, lastWriteTimeUtc);
        }
    }
}
