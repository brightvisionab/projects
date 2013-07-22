using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace BrightVision.DQControl.Business {
    /// <summary>
    /// Campaign Questionnaire object
    /// </summary>
    public class CampaignQuestionnaire {
        /// <summary>
        /// Gets or sets the type of questionnaire. (see Constants for more info)
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the global value or default global value of any property
        /// </summary>
        public string GlobalValue { get; set; }
        /// <summary>
        /// Gets or sets the question form of campaign questionnaire object
        /// </summary>
        public QuestionForm Form { get; set; }

        public string ToJSONString() {
            return ToJSONString(false);
        }
        public string ToJSONString(bool IsFormattingIndented) {
            Formatting formatting = Formatting.None;
            if (IsFormattingIndented)
                formatting = Formatting.Indented;
            string json = JsonConvert.SerializeObject(this, formatting,
                new JsonSerializerSettings {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                });
            return json;
        }
        public static CampaignQuestionnaire Instanciate(string questionTypeConstants) {                        
            CampaignQuestionnaire objQuestion = null;
            string json = string.Empty;
            switch (questionTypeConstants) {
                case QuestionTypeConstants.Dropbox :
                    json = Properties.Resources.dropbox; break;
                case QuestionTypeConstants.Textbox: 
                    json = Properties.Resources.textbox; break;
                case QuestionTypeConstants.MultipleChoice: 
                    json = Properties.Resources.multiplechoice; break;
                case QuestionTypeConstants.Schedule:
                    json = Properties.Resources.schedule; break;
                case QuestionTypeConstants.SmartText:
                    json = Properties.Resources.smarttext; break;
            }
            objQuestion = JsonConvert.DeserializeObject<CampaignQuestionnaire>(json,
                    new JsonSerializerSettings {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
            return objQuestion;
        }
        public static CampaignQuestionnaire InstanciateWith(string contentJSON) {
            if(string.IsNullOrEmpty(contentJSON))return null;
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

    /// <summary>
    /// TO DO: Implement this resolver for each type of component for serialization
    /// </summary>
    public class DynamicContractResolver : DefaultContractResolver {
        private readonly char _startingWithChar;
        public DynamicContractResolver(char startingWithChar) {
            _startingWithChar = startingWithChar;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
            
            // only serializer properties that start with the specified character
            properties =
              properties.Where(p => p.PropertyName.StartsWith(_startingWithChar.ToString())).ToList();

            return properties;
        }
    }

}
