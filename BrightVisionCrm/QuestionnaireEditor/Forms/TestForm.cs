using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using BrightVision.DQControl.Business;
namespace QuestionnaireEditor.Forms {
    public partial class TestForm : DevExpress.XtraEditors.XtraForm {
        public TestForm() {
            InitializeComponent();
        }


        private void simpleButton3_Click(object sender, EventArgs e) {
            memoEditCompressed.Text = "";
            memoEditText.Text = "";
        }

        private void simpleButton2_Click(object sender, EventArgs e) {            
            memoEditCompressed.Text = ToJSONString(memoEditText.Text, checkEditIndented.Checked);
            try {
                var CQ = CampaignQuestionnaire.InstanciateWith(memoEditCompressed.Text);

                var jobj = JObject.Parse(CQ.ToJSONString());
                JArray jarray = new JArray();
                jarray.Add(jobj);
                var q = jarray.ToString();
                var x = jobj.ToString(Formatting.None);


            } catch( Exception ex) {
                var exx = ex;
            }
        }
        public string ToJSONString(string text, bool IsFormattingIndented) {
            string json = string.Empty;
            Formatting formatting = Formatting.None;
            if (IsFormattingIndented)
               formatting = Formatting.Indented;
            try {
                var jobj = JObject.Parse(text);
                json = jobj.ToString(formatting);
            } catch {
                try {
                    var jobj = JArray.Parse(text);
                    json = jobj.ToString(formatting);
                    json = UnescapeString(jobj[1].ToString());
                } catch { }
            }
            

            return json;
        }
        public string UnescapeString(string str) {
            string oldStr = System.Text.RegularExpressions.Regex.Unescape(str);
            oldStr = oldStr.Substring(1);
            return oldStr.Substring(0, oldStr.Length - 1);
        }
        public static CampaignQuestionnaire InstanciateWith(string contentJSON) {
            if (string.IsNullOrEmpty(contentJSON)) return null;
            CampaignQuestionnaire objQuestion = null;
            objQuestion = JsonConvert.DeserializeObject<CampaignQuestionnaire>(contentJSON,
                    new JsonSerializerSettings {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
            return objQuestion;
        }
    }
}
