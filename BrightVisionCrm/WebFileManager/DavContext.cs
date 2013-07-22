using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Web;

using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Acl;
using ITHit.WebDAV.Server.Quota;

namespace WebFileManager
{
    /// <summary>
    /// Implementation of <see cref="DavContext"/>.
    /// Resolves hierarchy items by paths.
    /// </summary>
    public class DavContext : DavContextBase
    {
        /// <summary>
        /// Path to folder where files are stored.
        /// </summary>
        private readonly string repositoryPath;

        /// <summary>
        /// Instance of <see cref="ILogger"/> to be used for logging.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Currently logged in identity.
        /// </summary>
        private readonly WindowsIdentity windowsIdentity;

        /// <summary>
        /// Disk full windows error code.
        /// </summary>
        private const int ERROR_DISK_FULL = 0x70;
        /// <summary>
        /// Initializes a new instance of the DavContext class.
        /// </summary>
        /// <param name="httpContext"><see cref="HttpContext"/> instance.</param>
        /// <param name="repositoryPath">Local path to repository.</param>
        /// <param name="logger"><see cref="ILogger"/> instance.</param>
        public DavContext(HttpContext httpContext, string repositoryPath, ILogger logger) : base(httpContext)
        {
            this.logger = logger;
            this.repositoryPath = repositoryPath;
            if (!Directory.Exists(repositoryPath))
            {
                logger.LogError("Repository path specified in Web.config is invalid.", null);
            }

            windowsIdentity = httpContext.User.Identity as WindowsIdentity;
        }

        /// <summary>
        /// Gets repository path.
        /// </summary>
        public string RepositoryPath
        {
            get { return repositoryPath; }
        }

        /// <summary>
        /// Gets <see cref="ILogger"/> instance.
        /// </summary>
        public ILogger Logger
        {
            get { return logger; }
        }

        /// <summary>
        /// Creates <see cref="IHierarchyItem"/> instance by path.
        /// </summary>
        /// <param name="path">Item relative path including query string.</param>
        /// <returns>Instance of corresponding <see cref="IHierarchyItem"/> or null if item is not found.</returns>
        public override IHierarchyItem GetHierarchyItem(string path)
        {
            path = path.Trim();
            //remove query string.
            int ind = path.IndexOf('?');
            if (ind > 0)
            {
                path = path.Remove(ind);
            }

            //Convert to local file system path by decoding every part, reversing slashes and appending
            //to repository root.
            string[] encodedParts = path.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            string[] decodedParts = encodedParts.Select<string, string>(EncodeUtil.DecodeUrlPart).ToArray();
            string fullPath = Path.Combine(repositoryPath, string.Join("\\", decodedParts));

            path = path.Trim(new[] { '/' });
            
            //Many clients don't follow rule to specify folder path ending with slash,
            //and we cannot rely on it, so check if a folder is requested.
            var directory = new DirectoryInfo(fullPath);
            if (directory.Exists)
            {
                string correctFolderPath = string.IsNullOrEmpty(path) ? "/" : "/" + path + "/";
                return new DavFolder(directory, this, correctFolderPath);
            }
            
            //Check if a file is requested
            var file = new FileInfo(fullPath);
            if (file.Exists)
            {
                return new DavFile(file, this, "/" + path);
            }

            logger.LogDebug("Could not resolve path:" + fullPath);
            
            return null; // no hierarchy item that coresponds to path parameter was found in the repository
        }

        /// <summary>
        /// Performs file system operation with translating exceptions to those expected by WebDAV engine.
        /// </summary>
        /// <param name="item">Item on which operation is performed.</param>
        /// <param name="action">The action to be performed.</param>
        /// <param name="privilege">Privilege which is needed to perform the operation. If <see cref="UnauthorizedAccessException"/> is thrown
        /// this method will convert it to <see cref="NeedPrivilegesException"/> exception and specify this privilege in it.</param>
        public void FileOperation(IHierarchyItem item, Action action, Privilege privilege)
        {
            try
            {
                action();
            }
            catch (UnauthorizedAccessException)
            {
                var ex = new NeedPrivilegesException("Not enough privileges");
                ex.AddRequiredPrivilege(item.Path, privilege);
                throw ex;
            }
            catch (IOException ex)
            {
                int hr = Marshal.GetHRForException(ex);
                if (hr == ERROR_DISK_FULL)
                {
                    throw new InsufficientStorageException();
                }

                throw new DavException(ex.Message, DavStatus.CONFLICT);
            }
        }

        /// <summary>
        /// Performs file system operation with translating exceptions to those expected by WebDAV engine, except
        /// <see cref="UnauthorizedAccessException"/> which must be caught and translated manually.
        /// </summary>        
        /// <param name="action">The action to be performed.</param>
        public void FileOperation(Action action)
        {
            try
            {
                action();
            }
            catch (IOException ex)
            {
                int hr = Marshal.GetHRForException(ex);
                if (hr == ERROR_DISK_FULL)
                {
                    throw new InsufficientStorageException();
                }

                throw new DavException(ex.Message, DavStatus.CONFLICT);
            }
        }

        /// <summary>
        /// Performs file system operation with translating exceptions to those expected by WebDAV engine.
        /// </summary>
        /// <param name="item">Item on which operation is performed.</param>
        /// <param name="func">The action to be performed.</param>
        /// <param name="privilege">Privilege which is needed to perform the operation.
        /// If <see cref="UnauthorizedAccessException"/> is thrown  this method will convert it to
        /// <see cref="NeedPrivilegesException"/> exception and specify this privilege in it.</param>
        /// <typeparam name="T">Type of operation's result.</typeparam>
        /// <returns>Result returned by <paramref name="func"/>.</returns>
        public T FileOperation<T>(IHierarchyItem item, Func<T> func, Privilege privilege)
        {
            try
            {
                return func();
            }
            catch (UnauthorizedAccessException)
            {
                var ex = new NeedPrivilegesException("Not enough privileges");
                ex.AddRequiredPrivilege(item.Path, privilege);
                throw ex;
            }
            catch (IOException ex)
            {
                int hr = Marshal.GetHRForException(ex);
                if (hr == ERROR_DISK_FULL)
                {
                    throw new InsufficientStorageException();
                }

                throw new DavException(ex.Message, DavStatus.CONFLICT);
            }
        }
    }
}
