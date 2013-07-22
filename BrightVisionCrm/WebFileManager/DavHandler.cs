using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

using ITHit.WebDAV.Server;

namespace WebFileManager
{
    /// <summary>
    /// This handler processes WebDAV requests.
    /// </summary>
    public class DavHandler : IHttpHandler
    {
        /// <summary>
        /// If debug logging is enabled reponses are output as formatted XML,
        /// all requests and response headers and most bodies are logged.
        /// If debug logging is disabled only errors are logged.
        /// </summary>
        private static readonly bool debugLoggingEnabled =
            "true".Equals(
                ConfigurationManager.AppSettings["DebugLoggingEnabled"],
                StringComparison.InvariantCultureIgnoreCase);
        /// <summary>
        /// Path to the folder which stores WebDAV files.
        /// </summary>
        private static readonly string repositoryPath =
            (ConfigurationManager.AppSettings["RepositoryPath"] ?? string.Empty).TrimEnd('\\');
 

        /// <summary>
        /// Gets a value indicating whether another request can use the
        /// <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.
        /// </returns>
        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the
        /// intrinsic server objects (for example, Request, Response, Session, and Server) used to service
        /// HTTP requests. 
        /// </param>
        public void ProcessRequest(HttpContext context)
        {
            DavEngine engine = getOrInitializeEngine(context);

            context.Response.BufferOutput = false;
            string physicalRepositoryPath = repositoryPath.StartsWith("~") ?
                context.Server.MapPath(repositoryPath) : repositoryPath;
            var ntfsDavContext = new DavContext(context, physicalRepositoryPath, engine.Logger);
            engine.Run(ntfsDavContext);
        }

        /// <summary>
        /// Initializes engine.
        /// </summary>
        /// <param name="context">Instance of <see cref="HttpContext"/>.</param>
        /// <returns>Initialized <see cref="DavEngine"/>.</returns>
        private DavEngine initializeEngine(HttpContext context)
        {

            var engine = new DavEngine
            {
                Logger = Logger.Instance,
                //Use idented responses if debug logging is enabled.
                OutputXmlFormatting = debugLoggingEnabled ? Formatting.Indented : Formatting.None,
            };
            //engine.License = @"<?xml version=""1.0"" encoding=""utf-8""?><License><Data><Product>IT Hit WebDAV Server .Net v3</Product><LicensedTo><![CDATA[Ulysses Maglana]]></LicensedTo><Quantity>1</Quantity><IssueDate><![CDATA[Monday, August 27, 2012]]></IssueDate><ExpirationDate><![CDATA[Thursday, September 27, 2012]]></ExpirationDate><Type>Evaluation</Type></Data><Signature><![CDATA[nAE6oE4vntJu5YnGtlpR2+ofsEWZxrHzV+2LCejMcFAuKSWL6oWgRFbhPh8rKI+hDlrSiaz7LStiPfXVfYGH6RAXM/EhLmRkIaxRtOKv2eIbtoY/GRhmyw3P4ViIILRWe/brlrhiisimaPbw3mpi6FPr3Jiknlemx+lkNKXnFM4=]]></Signature></License>";
            engine.License = @"<?xml version=""1.0"" encoding=""utf-8""?><License><Data><Product>IT Hit WebDAV Server .Net v3</Product><LicensedTo><![CDATA[Orville rosillo]]></LicensedTo><Quantity>1</Quantity><IssueDate><![CDATA[Wednesday, September 26, 2012]]></IssueDate><ExpirationDate><![CDATA[Friday, October 26, 2012]]></ExpirationDate><Type>Evaluation</Type></Data><Signature><![CDATA[ZemFCrgqW9rCmX7dNepifTEiLZKAsAHRZElENFrx/VDsheB8wnRtYjWNq7ZwrYONi7d0dGGhKAFg7GpfDoQo1FiMltOILSxCf0111M8BHtzS0lYh/cjpbgiw1SI/SsleLlXLSomR1/4KHTYvbzRVJyCcuJcBy8fW51doSjCMMEk=]]></Signature></License>";
            // set custom handler to process GET requests to folders and display info about how to connect to server
            var handler = new MyCustomGetHandler();
            handler.OriginalHandler = engine.RegisterMethodHandler("GET", handler);
            return engine;
        }

        /// <summary>
        /// Initializes or gets engine singleton.
        /// </summary>
        /// <param name="context">Instance of <see cref="HttpContext"/>.</param>
        /// <returns>Instance of <see cref="DavEngine"/>.</returns>
        private DavEngine getOrInitializeEngine(HttpContext context)
        {
            //we don't use any double check lock pattern here because nothing wrong
            //is going to happen if we created occasionally several engines.
            const string ENGINE_KEY = "$DavEngine$";
            if (context.Application[ENGINE_KEY] == null)
            {
                context.Application[ENGINE_KEY] = initializeEngine(context);
            }

            return (DavEngine)context.Application[ENGINE_KEY];
        }
    }
}
