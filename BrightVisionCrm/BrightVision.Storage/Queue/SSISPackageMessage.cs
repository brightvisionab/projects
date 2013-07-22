using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace BrightVision.Storage.Queue
{
    [Serializable]
    public class SSISPackageMessage : QueueMessageBase
    {
        public const string Fuzzy_Company_Name = "fuzzy_company_name";
        // To do: Add other constant fields for fuzzy match.
        //public const string Fuzzy_Org_No = "org_no";

        public SSISPackageMessage()
            : base()
        {
        
        }
        
        public string PackageID { get; set; }
        public string PackageType { get; set; }
        public string Fuzzy_Match_Field { get; set; }
        public int ImportFileID { get; set; }
        public int UserID { get; set; }
        public string Country { get; set; }
        public int Confidence { get; set; }
        public int Similarity { get; set; }
        public string ConfidenceOperator { get;set; }
        public string SimilarityOperator { get;set; }
        public byte Validated { get; set; }
    }
}
