using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using System.Runtime.Serialization.Formatters.Binary;   //binary formatter
using System.IO;                                        //memory stream

using System.Data.Objects.DataClasses;
using Newtonsoft.Json.Linq;
using BrightVision.Model;

namespace QuestionnaireEditor.Business {
    public class Objects {        
        public class QuestionLanguageTags {
            public CTQuestionLanguage questionlanguage { get; set; }
            public EntityCollection<CTQuestionTags> questiontags { get; set; }
        }
    }
}


