
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using BrightVision.Common.Business;
using BrightVision.Storage.Queue;
using BrightVision.Storage.Repositories;
using BrightVision.Storage.Models;

using ManagerApplication.Business;
using ManagerApplication.Modules;
using DevExpress.XtraEditors;
using BrightVision.Common.Utilities;

namespace ManagerApplication.Forms
{
    public partial class FrmFuzzyMatching : DevExpress.XtraEditors.XtraForm
    {
        #region Constructors
        public FrmFuzzyMatching()
        {
            InitializeComponent();
            pbFuzzyMatch.Position = 0;
            pbFuzzyMatch.Properties.Maximum = 10;
            pbFuzzyMatch.Update();
        }
        #endregion

        #region Public Events & Args
        #endregion

        #region Public Properties
        public class FuzzyMatchArguments {
            public int user_id;
            public int import_file_id;
            public string country;
            public string fuzzy_ids;
            public int confidence;
            public int similarity;
            public string confidence_operator;
            public string similarity_operator;
            public byte validated;
        }

        public FuzzyMatchWorker FuzzyMatchWorkerObject = null;
        public FuzzyMatchArguments FuzzyMatchArgs = null;
        public string FuzzyMatchField = string.Empty;
        public Thread FuzzyMatchWorkThread = null;

        public int ImportListId { get; set; }
        public List<string> AccountIds { get; set; }
        #endregion
        
        #region Private Properties
        private SSISPackageQueue m_PackageQueue = null;
        private UserTextNotificationRepository m_Repository = null;
        #endregion

        #region Public Methods
        public void StartFuzzyMatch()
        {
            try {
                this.Invoke(new MethodInvoker(delegate { lblProgressTitle.Text = "Initializing ..."; }));
                this.Invoke(new MethodInvoker(delegate {
                    pbFuzzyMatch.Position = 0;
                    pbFuzzyMatch.Properties.Maximum = AccountIds.Count;
                }));

                DataImportUtility.ClearFuzzyLookupAccountMatches(ImportListId, AccountIds, true);
                SSISPackageMessage _PackageMsg = new SSISPackageMessage() {
                    PackageID = Guid.NewGuid().ToString(),
                    PackageType = "Account",
                    Fuzzy_Match_Field = FuzzyMatchField,
                    UserID = UserSession.CurrentUser.UserId,
                    ImportFileID = FuzzyMatchArgs.import_file_id,
                    Country = FuzzyMatchArgs.country,
                    Similarity = FuzzyMatchArgs.similarity,
                    Confidence = FuzzyMatchArgs.confidence,
                    SimilarityOperator = FuzzyMatchArgs.similarity_operator,
                    ConfidenceOperator = FuzzyMatchArgs.confidence_operator,
                    Validated = FuzzyMatchArgs.validated
                };

                m_Repository = new UserTextNotificationRepository();
                m_PackageQueue = new SSISPackageQueue();
                m_PackageQueue.AddMessage(_PackageMsg);

                string _NotificationTitle = string.Empty;
                if (_PackageMsg.Fuzzy_Match_Field == SSISPackageMessage.Fuzzy_Company_Name)
                    _NotificationTitle = "AccountNotification_FuzzyCompanyName";

                System.Threading.Thread.Sleep(500);
                this.Invoke(new MethodInvoker(delegate { lblProgressTitle.Text = "Processing fuzzy matches ..."; }));
                System.Threading.Thread.Sleep(500);

                /**
                 * we will loop until the ssis package returns a message
                 */
                while (true) {
                    try {
                        this.Invoke(new MethodInvoker(delegate { lblProgressTitle.Text = "Processing fuzzy matches ... Getting queue message ..."; }));
                        System.Threading.Thread.Sleep(3000);
                        UserTextNotification[] userToasts = m_Repository.GetNotificationsForUser(UserSession.CurrentUser.UserId.ToString());
                        if (userToasts != null && userToasts.Length > 0) {
                            var data = (
                                from UserTextNotification toast in userToasts
                                where toast.Title == _NotificationTitle
                                orderby toast.Timestamp descending
                                select toast
                            ).ToArray();

                            if (data != null) {
                                this.Invoke(new MethodInvoker(delegate { lblProgressTitle.Text = "Processing fuzzy matches ... Getting processed items ..."; }));
                                if (data[0].MessageText.ToLower().Equals("success")) {
                                    while (true) {
                                        System.Threading.Thread.Sleep(1000);
                                        BrightVision.Model.CTProcessedFuzzyLookupAccount _FuzzyMatching = DataImportUtility.CheckProcessedFuzzyMatchesCompanies(ImportListId);
                                        this.Invoke(new MethodInvoker(delegate { 
                                            pbFuzzyMatch.Position = (int)_FuzzyMatching.items_processed;
                                            pbFuzzyMatch.Update();
                                        }));
                                        if (_FuzzyMatching.items_processed == _FuzzyMatching.total_items)
                                            break;
                                    }
                                    
                                    this.Invoke(new MethodInvoker(delegate { lblProgressTitle.Text = "Done fuzzy matching ..."; }));
                                    m_Repository.DeleteNotification(data[0]);
                                    //m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
                                    //this.PopulateFuzzyLookupAccountList();
                                    this.Invoke(new MethodInvoker(delegate { msgPopupRegular.Show(this, "Bright Manager", "Fuzzy matching successfully finished ..."); })); 
                                    break;
                                }
                            }
                        }
                    }
                    catch { 
                    }
                }

            }
            catch {
            }

        }
        #endregion

        #region Private Methods
        #endregion

        #region Control Events
        private void btnOk_Click(object sender, EventArgs e)
        {
            FuzzyMatchWorkerObject = new FuzzyMatchWorker(this);
            FuzzyMatchWorkThread = new Thread(FuzzyMatchWorkerObject.StartProcess);
            FuzzyMatchWorkThread.Start();
        }
        #endregion        
    }

    #region Fuzzy Matching Worker Class
    public class FuzzyMatchWorker
    {
        private FrmFuzzyMatching m_Worker = null;
        public FuzzyMatchWorker(FrmFuzzyMatching _pWorker)
        {
            m_Worker = _pWorker;
        }
        public void StartProcess()
        {
            m_Worker.StartFuzzyMatch();
        }
        public void EndProcess()
        {
            m_Worker.FuzzyMatchWorkThread.Abort();
        }
    }
    #endregion
}