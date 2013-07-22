using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI;
using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Class1;
using ITHit.WebDAV.Server.Extensibility;

namespace WebFileManager
{
    /// <summary>
    /// This handler processes GET requests to folders returning custom HTML page.
    /// </summary>
    internal class MyCustomGetHandler : IMethodHandler
    {
        /// <summary>
        /// Handler for GET request registered with the engine before registering this one.
        /// We call this default handler to handle GET for files, because this handler
        /// only handles GET for folders.
        /// </summary>
        public IMethodHandler OriginalHandler { get; set; }

        /// <summary>
        /// Gets a value indicating whether output shall be buffered to calculate content length.
        /// Don't buffer output to calculate content length.
        /// </summary>
        public bool EnableOutputBuffering
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether engine shall log response data (even if debug logging is on).
        /// </summary>
        public bool EnableOutputDebugLogging
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the engine shall log request data.
        /// </summary>
        public bool EnableInputDebugLogging
        {
            get { return false; }
        }

        /// <summary>
        /// Handles GET request.
        /// </summary>
        /// <param name="context">Instace of <see cref="DavContextBase"/>.</param>
        /// <param name="item">Instance of <see cref="IHierarchyItem"/> which was returned by
        /// <see cref="DavContextBase.GetHierarchyItem"/> for this request.</param>
        public void ProcessRequest(DavContextBase context, IHierarchyItem item)
        {
            if (item is IFolder)
            {
                // Remember to call EnsureBeforeResponseWasCalled here if your context implementation
                // makes some useful things in BeforeResponse.
                context.EnsureBeforeResponseWasCalled();
                Page page = (Page)System.Web.Compilation.BuildManager.CreateInstanceFromVirtualPath(
                    "~/MyCustomHandlerPage.aspx", typeof(Page));
                page.ProcessRequest(HttpContext.Current);
            }
            else
            {
                OriginalHandler.ProcessRequest(context, item);
            }
        }
        
        /// <summary>
        /// This handler shall only be invoked for IFolder items or if original handler (which
        /// this handler substitutes) shall be called for the item.
        /// </summary>
        /// <param name="item">Instance of <see cref="IHierarchyItem"/> which was returned by
        /// <see cref="DavContextBase.GetHierarchyItem"/> for this request.</param>
        /// <returns>Returns <c>true</c> if this handler can handler this item.</returns>
        public bool AppliesTo(IHierarchyItem item)
        {
            return item is IFolder || OriginalHandler.AppliesTo(item);
        }
    }
}
