/**
 * created by: Michael Castillones
 * created on: 05.27.2011 
 */

using System;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

using BrightVision.Common.Utilities;
using BrightVision.Common.UI;
using System.IO;
using System.Net.Sockets;


namespace BrightVision.Common.Business {
    /// <summary>
    /// Servers currently used
    /// </summary>
    public enum BrightVisionServers {
        Unspecified = 0,
        Gothenburg = 1,
        Hamachi = 2,
        DemoEnv = 3
    }    
    /// <summary>
    /// User session handler, for use in accessing user specific objects (e.g. user id), for the entire solution
    /// </summary>
    public sealed class UserSession {
                    
        //db servers
        static string GothenBurg = ConfigManager.ConnectionStrings["GothenBurgEntityConnection"];
        static string Hamachi = ConfigManager.ConnectionStrings["HamachiEntityConnection"];
        static string DemoEnv = ConfigManager.ConnectionStrings["DemoEnvEntityConnection"];
        static string ConnectionPriority = ConfigManager.AppSettings["ConnectionDefaultPriority"].ToString().ToLower();
        static string sProviderConnection = string.Empty;
        public static bool IsEntityConnectionValid(BrightVisionServers server) {
            try {
                EntityConnectionStringBuilder entityBuilder = null;
                if (server == BrightVisionServers.Hamachi) {
                    entityBuilder = new EntityConnectionStringBuilder(Hamachi.Replace("&quot;", "'"));
                    sProviderConnection = entityBuilder.ProviderConnectionString;                    
                }
                else if (server == BrightVisionServers.Gothenburg) {
                    entityBuilder = new EntityConnectionStringBuilder(GothenBurg.Replace("&quot;", "'"));
                    sProviderConnection = entityBuilder.ProviderConnectionString;
                }
                else if (server == BrightVisionServers.DemoEnv) {
                    entityBuilder = new EntityConnectionStringBuilder(DemoEnv.Replace("&quot;", "'"));
                    sProviderConnection = entityBuilder.ProviderConnectionString;
                }
                using (var conn = new SqlConnection(sProviderConnection)) {
                    if (conn.CanOpen())
                        return true;
                    else {
                        System.Windows.Forms.MessageBox.Show(
                            "Could not establish connection to server. Please make sure you are connected to local network or use Hamachi client to connect and try again or contact network administrator.",
                            "Network Connection Error",
                                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return false;
                    }
                }
            } catch {
                System.Windows.Forms.MessageBox.Show(
                            "Could not establish connection to server. Please make sure you are connected to local network or use Hamachi client to connect and try again or contact network administrator.",
                            "Network Connection Error",
                                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
        public static EntityConnection DefaultEntityConnection
        {
            get {
                EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder(GothenBurg.Replace("&quot;", "'"));
                sProviderConnection = entityBuilder.ProviderConnectionString;
                using (var connGothenburg = new SqlConnection(sProviderConnection)) {
                    if (!connGothenburg.CanOpen()) {
                        NotificationDialog.Information("Login", "Cannot establish a connection to the server. Please make sure you are connected to local network or use Hamachi client to connect and try again or contact network administrator.");
                        return null;
                    }
                    else
                        return new EntityConnection(entityBuilder.ToString());
                }
            }
        }
        public static EntityConnection EntityConnection {
            get {
                try {
                    EntityConnectionStringBuilder entityBuilder = null;
                    if (CurrentUser.ServerName == BrightVisionServers.Unspecified) {
                        if (ConnectionPriority == "hamachi") {
                            entityBuilder = new EntityConnectionStringBuilder(Hamachi.Replace("&quot;", "'"));
                            sProviderConnection = entityBuilder.ProviderConnectionString;
                            using (var connHamachi = new SqlConnection(sProviderConnection)) {
                                if (!connHamachi.CanOpen()) {
                                    entityBuilder = new EntityConnectionStringBuilder(GothenBurg.Replace("&quot;", "'"));
                                    sProviderConnection = entityBuilder.ProviderConnectionString;
                                    using (var connGothenburg = new SqlConnection(sProviderConnection)) {
                                        if (connGothenburg.CanOpen())
                                            return new EntityConnection(entityBuilder.ToString());
                                        else {
                                            var exception = new Exception("Could not establish connection to server. Please make sure you are connected to local network or use Hamachi client to connect and try again or contact network administrator.");
                                            exception.Source = "Network Connection Error";
                                            throw exception;
                                        }
                                    }
                                } else {
                                    return new EntityConnection(entityBuilder.ToString());
                                }
                            }
                        } 
                        else if (ConnectionPriority == "gothenburg") {
                            entityBuilder = new EntityConnectionStringBuilder(GothenBurg.Replace("&quot;", "'"));
                            sProviderConnection = entityBuilder.ProviderConnectionString;
                            using (var connGothenburg = new SqlConnection(sProviderConnection)) {
                                if (!connGothenburg.CanOpen()) {
                                    entityBuilder = new EntityConnectionStringBuilder(Hamachi.Replace("&quot;", "'"));
                                    sProviderConnection = entityBuilder.ProviderConnectionString;
                                    using (var connHamachi = new SqlConnection(sProviderConnection)) {
                                        if (connHamachi.CanOpen())
                                            return new EntityConnection(entityBuilder.ToString());
                                        else {
                                            var exception = new Exception("Could not establish connection to server. Please make sure you are connected to local network or use Hamachi client to connect and try again or contact network administrator.");
                                            exception.Source = "Network Connection Error";
                                            throw exception;
                                        }
                                    }
                                } else {
                                    return new EntityConnection(entityBuilder.ToString());
                                }
                            }
                        } else {
                            var exception = new Exception("Could not establish connection to server. Please make sure you are connected to local network or use Hamachi client to connect and try again or contact administrator.");
                            exception.Source = "Network Connection Error";
                            throw exception;
                        }                            
                        
                    } 
                    else if (CurrentUser.ServerName == BrightVisionServers.Gothenburg) {
                        entityBuilder = new EntityConnectionStringBuilder(GothenBurg.Replace("&quot;", "'"));
                        sProviderConnection = entityBuilder.ProviderConnectionString;
                        return new EntityConnection(entityBuilder.ToString());
                    } 
                    else if (CurrentUser.ServerName == BrightVisionServers.DemoEnv) {
                        entityBuilder = new EntityConnectionStringBuilder(DemoEnv.Replace("&quot;", "'"));
                        sProviderConnection = entityBuilder.ProviderConnectionString;
                        return new EntityConnection(entityBuilder.ToString());
                    }
                } 
                catch (Exception ex) {
                    throw ex;
                }

                return null;
            }
        }
        public static string ConnectionString
        {
            get {
                try {
                    if (CurrentUser.ServerName == BrightVisionServers.Unspecified) {
                        if (ConnectionPriority == "hamachi") {
                            string str = Hamachi.Substring(Hamachi.IndexOf("\"") + 1, Hamachi.Length - 2 - Hamachi.IndexOf("\""));
                            return str;
                        }
                        else if (ConnectionPriority == "gothenburg") {
                            string str = GothenBurg.Substring(GothenBurg.IndexOf("\"") + 1, GothenBurg.Length - 2 - GothenBurg.IndexOf("\""));
                            return str;
                        }
                        else if (ConnectionPriority == "demoenv") {
                            string str = DemoEnv.Substring(DemoEnv.IndexOf("\"") + 1, DemoEnv.Length - 2 - DemoEnv.IndexOf("\""));
                            return str;
                        }
                    }
                    else if (CurrentUser.ServerName == BrightVisionServers.Gothenburg) {
                        string str = GothenBurg.Substring(GothenBurg.IndexOf("\"") + 1, GothenBurg.Length - 2 - GothenBurg.IndexOf("\""));
                        return str;
                    }
                    else if (CurrentUser.ServerName == BrightVisionServers.Hamachi) {
                        string str = Hamachi.Substring(Hamachi.IndexOf("\"")+1, Hamachi.Length - 2 - Hamachi.IndexOf("\""));
                        return str;
                    }
                    else if (CurrentUser.ServerName == BrightVisionServers.DemoEnv) {
                        string str = DemoEnv.Substring(DemoEnv.IndexOf("\"") + 1, DemoEnv.Length - 2 - DemoEnv.IndexOf("\""));
                        return str;
                    }
                }
                catch (Exception ex) {
                    throw ex;
                }
                return null;
            }
        }
        public static string ProviderConnection {
            get {
                return sProviderConnection;
            }
        }
        static UserSession m_objUserInstance = null;
        static readonly object objPadlock = new object();

        //hide constructor
        private UserSession() {
        }
        //Singleton approach to get current user session;
        public static UserSession CurrentUser {
            get {
                lock (objPadlock) {
                    if (m_objUserInstance == null){
                        m_objUserInstance = new UserSession();                    
                    }
                    return m_objUserInstance;
                }
            }
        }

        // user information
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }

        // manager app and sales app access rights
        public bool IsManagerAdmin { get; set; }
        public bool IsManagerUser { get; set; }
        public bool IsSalesUser { get; set; }

        // sales application campaign/sub-campaign specific access rights
        public bool IsCampaignOwner { get; set; }
        public bool IsSubCampaignManager { get; set; }
        public bool IsSubCampaignSales { get; set; }
        public bool IsCustomerUser { get; set; }

        //servers
        public BrightVisionServers ServerName { get; set; }
        public string ComputerName { get { return Environment.MachineName; } }
        public string ComputerUserName { get { return Environment.UserName; } }
        public string ComputerIP {
            get {
                string _localIP = string.Empty;
                var _host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in _host.AddressList) {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        _localIP = ip.ToString();
                }
                return _localIP;
            }
        }
        //store the global list of titles
        public object TitleList { get; set; }

    }

    public static class ConfigManager {
        private static Configuration configManager;
        private static Dictionary<string, string> appSettings;
        private static Dictionary<string, string> connectionStrings;
        private static Configuration Config
        {
            get {
                if (configManager == null) {
                    #if !DEBUG
                    configManager = GetConfiguration();
                    #else
                    configManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    #endif
                }
                return configManager;
            }
        }
        public static Dictionary<string, string> AppSettings
        {
            get {
                if (appSettings == null) {
                    appSettings = new Dictionary<string, string>();
                    SetAppSettings();
                }
                return appSettings;
            }
        }


        public static Dictionary<string, string> ConnectionStrings
        {
            get
            {
                if (connectionStrings == null) {
                    connectionStrings = new Dictionary<string, string>();
                    SetConnectionStrings();
                }
                return connectionStrings;
            }
        }

        private static void SetConnectionStrings()
        {
            var keyval = Config.ConnectionStrings.ConnectionStrings;
            foreach (ConnectionStringSettings elmnt in keyval)
            {
                connectionStrings.Add(elmnt.Name, elmnt.ConnectionString);
            }
        }
        private static void SetAppSettings()
        {
            var keyval = Config.AppSettings.Settings;
            foreach (KeyValueConfigurationElement elmnt in keyval) {
                appSettings.Add(elmnt.Key, elmnt.Value);
            }
        }
        private static Configuration GetConfiguration()
        {
            string appName = Environment.GetCommandLineArgs()[0];
            string tmpConfig =string.Empty;
            string tmpConfigTmp = string.Empty;
            CommonApplicationData commonAppFolder = null;
            if (appName.Contains("SalesConsultant"))
            {
                commonAppFolder = new CommonApplicationData("BrightVision", "BrightSales");
                tmpConfig = AppDomain.CurrentDomain.BaseDirectory+"SalesConsultant.exe.config.protected";
                tmpConfigTmp = commonAppFolder.ApplicationFolderPath + "\\bs.tmp";
            }else{
                commonAppFolder = new CommonApplicationData("BrightVision", "BrightManager");
                tmpConfig = AppDomain.CurrentDomain.BaseDirectory + "ManagerApplication.exe.config.protected";
                tmpConfigTmp = commonAppFolder.ApplicationFolderPath + "\\bm.tmp";
            }
            string eC = Resources.p;
            string configFile = File.ReadAllText(tmpConfig, Encoding.UTF8);
            int bitStrength = GetBitStrength(eC);
            eC = RemoveStrengthInString(eC);
           
            string config = SecurityUtility.Decrypt(configFile, bitStrength, eC);
            File.WriteAllText(tmpConfigTmp, config, Encoding.UTF8);
            Configuration configuration = GetConfigFromPath(tmpConfigTmp);
            File.Delete(tmpConfigTmp);
            return configuration;
        }
        //public void UpzipConfig(string tmpConfig)
        //{
        //    ZipFile zfile = new ZipFile(tmpConfig);
        //    zfile.Password = "";
        //}
        private static int GetBitStrength(string ec) {
            string bitStrengthString = ec.Substring(0, ec.IndexOf("</BitStrength>") + 14);
            ec.Substring(0, ec.IndexOf("</BitStrength>") + 14);
            ec = ec.Replace(bitStrengthString, "");
            int bitStrength = Convert.ToInt32(bitStrengthString.Replace("<BitStrength>", "").Replace("</BitStrength>", ""));
            return bitStrength;
        }
        private static string RemoveStrengthInString(string ec) {
            string bitStrengthString = ec.Substring(0, ec.IndexOf("</BitStrength>") + 14);
            return ec.Replace(bitStrengthString, "");
        }
        private static Configuration GetConfigFromPath(string tmpConfigTmp)
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = tmpConfigTmp;
            Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            return configuration;
        }
    }

}
