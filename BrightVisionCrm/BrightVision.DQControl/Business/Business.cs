using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Serialization;
namespace BrightVision.DQControl.Business {

    public interface ITextPrefix {
        /// <summary>
        /// Gets or sets the text prefix
        /// </summary>
        string TextPrefix { get; set; }
    }

    public interface IInputValues {
        /// <summary>
        /// Gets or sets the default input value
        /// </summary>
        string DefaultInputValue { get; set; }
        /// <summary>
        /// Gets or sets the input value
        /// </summary>
        string InputValue { get; set; }
        /// <summary>
        /// Indicates if input field is required
        /// </summary>
        bool Required { get; set; }
    }

    public interface ITextChoiceBase : ITextPrefix, IInputValues {
        /// <summary>
        /// Indicates if the choice is enabled
        /// </summary>
        bool Enabled { get; set; }        
    }
       
    public class MultipleChoiceValue : ITextPrefix {
        /// <summary>
        /// Gets or sets the text prefix of the choice value
        /// </summary>
        [DefaultValue("TextPrefix")]
        public string TextPrefix { get; set; }
        /// <summary>
        /// Indicates if the the choice is checked or not
        /// </summary>
        public bool Checked { get; set; }
    }

    public class OtherChoice : ITextChoiceBase {
        /// <summary>
        /// Gets or sets the text prefix of the other choice
        /// </summary>
        [DefaultValue("TextPrefix")]
        public string TextPrefix { get; set; }
        /// <summary>
        /// Gets or sets the default input value of the other choice
        /// </summary>
        public string DefaultInputValue { get; set; }
        /// <summary>
        /// Gets or sets the input value of the other choice
        /// </summary>       
        public string InputValue { get; set; }
        /// <summary>
        /// Indicates if input field is required
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// Indicates if the other choice is enabled
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// Gets or sets the max width of the textbox
        /// </summary>
        public string TextboxMaxWidth { get; set; }
        /// <summary>
        /// Gets or sets the max height of the textbox
        /// </summary>
        public string TextboxMaxHeight { get; set; } 
    }

    public class DropboxValue {
        /// <summary>
        /// Create an instance of attendie from json string
        /// </summary>
        /// <param name="attendieJSON"></param>
        /// <returns></returns>
        public static DropboxValue Instanciate(string dropboxValueJSON) {            
            DropboxValue objAttendie = null;
            try {
                objAttendie = JsonConvert.DeserializeObject<DropboxValue>(dropboxValueJSON,
                        new JsonSerializerSettings {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        });
            } catch { }
            return objAttendie;
        }        
        /// <summary>
        /// Gets or sets the selection value of the dropbox
        /// </summary>
        public string SelectionValue { get; set; }
        /// <summary>
        /// Gets or sets the other value of the dropbox
        /// </summary>
        public string OtherValue { get; set; }

        /// <summary>
        /// Overriding the equals method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) {
            if (obj == null) return false;
            var att = obj as DropboxValue;
            return (att.SelectionValue == SelectionValue && att.OtherValue == OtherValue);
        }
        /// <summary>
        /// Convert this attendie object to json string
        /// </summary>
        /// <returns></returns>
        public string ToJSONString() {
            return ToJSONString(false);
        }
        /// <summary>
        /// Convert this attendie object to json string
        /// </summary>
        /// <param name="IsFormattingIndented"></param>
        /// <returns></returns>
        public string ToJSONString(bool IsFormattingIndented) {
            try {
                Formatting formatting = Formatting.None;
                if (IsFormattingIndented)
                    formatting = Formatting.Indented;
                string json = JsonConvert.SerializeObject(this, formatting,
                    new JsonSerializerSettings {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    });
                return json;
            } catch { }
            return string.Empty;
        }
        /// <summary>
        /// Get hash code of the object
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return this.GetHashCode();
        }
    }
    
    public class Attendie {
        /// <summary>
        /// Create an instance of attendie from json string
        /// </summary>
        /// <param name="attendieJSON"></param>
        /// <returns></returns>
        public static Attendie Instanciate(string attendieJSON) {
            Attendie objAttendie = null;
            try {
                objAttendie = JsonConvert.DeserializeObject<Attendie>(attendieJSON,
                        new JsonSerializerSettings {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        });
            } catch { 
                throw new Exception("Could not deserialize attendieJSON"); 
            }
            return objAttendie;
        }        
        /// <summary>
        /// Gets or sets the Id of the attendie
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the name of the attendie
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Overriding the equals method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool  Equals(object obj) {
            if (obj == null) return false;
            var att = obj as Attendie;
            return (att.Id == Id && att.Name == Name);
        }
        /// <summary>
        /// Convert this attendie object to json string
        /// </summary>
        /// <returns></returns>
        public string ToJSONString() {
            return ToJSONString(false);
        }
        /// <summary>
        /// Convert this attendie object to json string
        /// </summary>
        /// <param name="IsFormattingIndented"></param>
        /// <returns></returns>
        public string ToJSONString(bool IsFormattingIndented) {
            try {
                Formatting formatting = Formatting.None;
                if (IsFormattingIndented)
                    formatting = Formatting.Indented;
                string json = JsonConvert.SerializeObject(this, formatting,
                    new JsonSerializerSettings {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    });
                return json;
            } catch { }
            return string.Empty;
        }
        /// <summary>
        /// Get hash code of the object
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return this.GetHashCode();
        }
    }

    public class SalesPerson {
        /// <summary>
        /// Create an instance of SalesPerson from json string
        /// </summary>
        /// <param name="salesPersonJSON"></param>
        /// <returns></returns>
        public static SalesPerson Instanciate(string salesPersonJSON) {            
            SalesPerson objSalesPerson = null;
            try {
                objSalesPerson = JsonConvert.DeserializeObject<SalesPerson>(salesPersonJSON,
                        new JsonSerializerSettings {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        });
            } catch {
                //throw new Exception("Could not deserialize salesPersonJSON"); 
                return null;
            }
            return objSalesPerson;
        }
        /// <summary>
        /// Gets or sets the Id of the SalesPerson
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the name of the attendie
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Overriding the equals method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) {
            if (obj == null) return false;
            var att = obj as SalesPerson;
            return (att.Id == Id && att.Name == Name);
        }
        /// <summary>
        /// Convert this salesperson object to json string
        /// </summary>
        /// <returns></returns>
        public string ToJSONString() {
            return ToJSONString(false);
        }
        /// <summary>
        /// Convert this salesperson object to json string
        /// </summary>
        /// <param name="IsFormattingIndented"></param>
        /// <returns></returns>
        public string ToJSONString(bool IsFormattingIndented) {
            try {
                Formatting formatting = Formatting.None;
                if (IsFormattingIndented)
                    formatting = Formatting.Indented;
                string json = JsonConvert.SerializeObject(this, formatting,
                    new JsonSerializerSettings {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    });
                return json;
            } catch { }
            return string.Empty;
        }
        /// <summary>
        /// Get hash code of the object
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return this.GetHashCode();
        }
    }

    public class ScheduleSalesPerson : ITextPrefix {
        /// <summary>
        /// Gets or sets the text prefix of the sales person
        /// </summary>
        [DefaultValue("TextPrefix")]
        public string TextPrefix { get; set; }
        /// <summary>
        /// Gets or sets the sales person selected value
        /// </summary>
        public SalesPerson SalesPersonSelectedValue { get; set; }
    }

    public class ScheduleType : ITextPrefix {
        /// <summary>
        /// Gets or sets the text prefix of the schedule type
        /// </summary>
        [DefaultValue("TextPrefix")]
        public string TextPrefix { get; set; }
        /// <summary>
        /// Gets or sets the schedule type prefix value
        /// </summary>
        public string ScheduleTypeSelectedValue { get; set; }
        /// <summary>
        /// Gets or sets the list of schedule type values
        /// </summary>
        public List<string> ScheduleTypeValues { get; set; }
    }

    public class ScheduleValue {
        
        /// <summary>
        /// Create an instance of ScheduleValue from json string
        /// </summary>
        /// <param name="salesPersonJSON"></param>
        /// <returns></returns>
        public static ScheduleValue Instanciate(string scheduleValueJSON) {
            ScheduleValue objScheduleValue = null;
            try {
                objScheduleValue = JsonConvert.DeserializeObject<ScheduleValue>(scheduleValueJSON,
                        new JsonSerializerSettings {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        });
            } catch {
                //throw new Exception("Could not deserialize scheduleValueJSON");
            }
            return objScheduleValue;
        }
        public string ScheduleId { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Overriding the equals method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) {
            if (obj == null) return false;
            var att = obj as ScheduleValue;
            return (att.ScheduleId == ScheduleId && att.Description == Description);
        }
        /// <summary>
        /// Convert this schedulevalue object to json string
        /// </summary>
        /// <returns></returns>
        public string ToJSONString() {
            return ToJSONString(false);
        }
        /// <summary>
        /// Convert this schedulevalue object to json string
        /// </summary>
        /// <param name="IsFormattingIndented"></param>
        /// <returns></returns>
        public string ToJSONString(bool IsFormattingIndented) {
            try {
                Formatting formatting = Formatting.None;
                if (IsFormattingIndented)
                    formatting = Formatting.Indented;
                string json = JsonConvert.SerializeObject(this, formatting,
                    new JsonSerializerSettings {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    });
                return json;
            } catch { }
            return string.Empty;
        }
        /// <summary>
        /// Get hash code of the object
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return this.GetHashCode();
        }
    }
    [XmlRoot("SmartTextValuesContainer")]
    public class SmartTextValuesContainer {
        [XmlArray("SmartTextValues")]
        [XmlArrayItem("SmartTextValue")]
        public List<SmartTextValue> Values { get; set; }
    }

    [Serializable()]
    public class SmartTextValue {
        public int id { get; set; }
        public string Comment { get; set; }
        public string CreationDate { get; set; }
        public string User { get; set; }
        public int UserId { get; set; }
        public string CustomerContact { get; set; }
        public int CustomerContactId { get; set; }
        public static List<SmartTextValue> Instanciate(string smarttextValueJSON)
        {
            List<SmartTextValue> values = new List<SmartTextValue>();
            try
            {
                JArray jaDiag = JArray.Parse(smarttextValueJSON);
                jaDiag.ForEach(delegate(JToken jt)
                {
                    var osmartTextValue = JsonConvert.DeserializeObject<SmartTextValue>(jt.ToString(),
                      new JsonSerializerSettings
                      {
                          MissingMemberHandling = MissingMemberHandling.Ignore,
                          NullValueHandling = NullValueHandling.Ignore,
                          DefaultValueHandling = DefaultValueHandling.Ignore
                      });
                    values.Add(osmartTextValue);
                });
              
            }
            catch { }
            return values;
        }
        /// <summary>
        /// Convert this smarttextvalue list to json string
        /// </summary>
        /// <returns></returns>
        public static string ToJSONString(List<SmartTextValue> list)
        {
            return JsonConvert.SerializeObject(list, Formatting.Indented);
        }
    }

    public delegate void ComponentDialogNotifyChangedEventHandler(object sender, ComponentNotifyChangedArgs e);

    public class ComponentNotifyChangedArgs : EventArgs {
        public ComponentNotifyChangedArgs(bool isLoaded) {
            IsLoaded = isLoaded;
        }
        public bool IsLoaded { get; set; }
    }
       
}
