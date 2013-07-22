using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using ITHit.WebDAV.Server;
using WebFileManager.NtfsStreamsEngine;

namespace WebFileManager
{
    /// <summary>
    /// Utility class to perform serialization of objects.
    /// </summary>
    public static class SerializationUtils
    {
        /// <summary>
        /// Serializes object to string.
        /// </summary>
        /// <param name="o">Object to be serialized.</param>
        /// <returns>Serialized object to string.</returns>
        internal static string Serialize(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            var sr = new XmlSerializer(o.GetType());
            var sb = new StringBuilder();
            using (var w = new StringWriter(sb, System.Globalization.CultureInfo.InvariantCulture))
            {
                sr.Serialize(w, o);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Reads alternate stream with specified name, deserializeses it and returns the result.
        /// </summary>
        /// <typeparam name="T">Type of object serialized in alternate date stream.</typeparam>
        /// <param name="streamName">Alternate data stream name.</param>
        /// <param name="fsi">File system information for the file with the alternate data stream.</param>
        /// <param name="logger">Logger instance.</param>
        /// <returns>Deserialized data. If an error occurrs it is logged and default initialized object is returned.</returns>
        internal static T GetStreamAndDeserialize<T>(string streamName, FileSystemInfo fsi, ILogger logger) where T : new()
        {
            string xml = getAlternateStreamText(streamName, fsi);
            return deserialize<T>(xml, logger);
        }

        /// <summary>
        /// Replaces or creates content of the alternate data stream.
        /// </summary>
        /// <param name="streamName">Alternate data stream name.</param>
        /// <param name="fsi">File which contains alternate data stream.</param>
        /// <param name="newContent">New content of the stream.</param>
        public static void RewriteStream(string streamName, FileSystemInfo fsi, string newContent)
        {
            rewriteStream(fsi.FullName, streamName, newContent);
        }

        private static T deserialize<T>(string xmlString, ILogger logger) where T : new()
        {
            if (string.IsNullOrEmpty(xmlString))
            {
                return new T();
            }

            try
            {
                using (var reader = new StringReader(xmlString))
                {
                    var dsr = new XmlSerializer(typeof(T));
                    return (T)dsr.Deserialize(reader);
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError("Failed to deserialize file", ex);
                return new T();
            }
        }

        private static string getAlternateStreamText(string streamName, FileSystemInfo fsi)
        {
            string propXml = null;
            try
            {
                var adsi = new AlternateDataStreamInfo(fsi.FullName, streamName);
                if (adsi.StreamExists())
                {
                    using (StreamReader sr = adsi.OpenText())
                    {
                        propXml = sr.ReadToEnd();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // no stream found with name specified
            }

            return propXml;
        }

        private static void rewriteStream(string filePath, string streamName, string newContent)
        {
            var asi = new AlternateDataStreamInfo(filePath, streamName);
            asi.Delete();
            if (newContent != null)
            {
                using (FileStream fs = asi.OpenWrite())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(newContent);
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }
    }
}
