using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;

namespace BrightVision.Windows.Azure.Storage.Blob
{
    public class WindowsAzureStorageBlob
    {
        #region Varaibles
        private CloudBlobContainer pCloudContainer;
        //private string pWindowsAzureStorageAccountName = "lii";
        //private string pWindowsAzureStorageAccountKey = "GUmKQvgTqn2t3CxJb7M4b4hYxocBJ5C9GbDec+Rm4JIxuUX1qKqVf2WzI/5w5UAMPVSMUs3DGY9FFFGl1KK5ZA==";
        //private string pWindowsAzureStorageContainerName = "iii";

        #endregion

        #region Initialize
        public bool InitialzeWindowsAzureStorage(string WindowsAzureStorageAccountName, string WindowsAzureStorageAccountKey, string WindowsAzureStorageContainerName)
        {
            try
            {
                string connectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", WindowsAzureStorageAccountName, WindowsAzureStorageAccountKey);

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve a reference to a container. 
                pCloudContainer = blobClient.GetContainerReference(WindowsAzureStorageContainerName);

                // Create the container if it doesn't already exist.
                pCloudContainer.CreateIfNotExists();

                pCloudContainer.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            }
            catch
            {
                return false;
            }
            return true;
        }
        #endregion

        public bool ProcessUpload(string file, string contentType = "application/pdf", string additionalParam = "")
        {
            string filename = Path.GetFileName(file);
            //string filenameNoExt = Path.GetFileNameWithoutExtension(file);

            try
            {

                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = pCloudContainer.GetBlockBlobReference(additionalParam + filename);

                // Create or overwrite the "myblob" blob with contents from a local file.
                using (var fileStream = System.IO.File.OpenRead(file))
                {
                    blockBlob.Properties.ContentType = contentType;
                    blockBlob.UploadFromStream(fileStream);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public bool ProcessDownload(string bolbFile, string file, ref string msg)
        {
            try
            {

                // Retrieve reference to a blob.
                CloudBlockBlob blockBlob = pCloudContainer.GetBlockBlobReference(bolbFile);

                // Save blob contents to a file.
                //using (var fileStream = System.IO.File.OpenWrite(path + "\\" + 	bolbFile.Replace("/","_")))
                if (File.Exists(file)) File.Delete(file);
                using (var fileStream = System.IO.File.OpenWrite(file))
                {
                    blockBlob.DownloadToStream(fileStream);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }

            return true;
        }

        public bool IsBlobFileExist(string bolbFile)
        {
            CloudBlockBlob blockBlob = pCloudContainer.GetBlockBlobReference(bolbFile);

            if (blockBlob.Exists()) return true;
            return false;
        }
    }
}
