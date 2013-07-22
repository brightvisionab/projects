using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Objects.DataClasses;

using BrightVision.Model;

namespace ManagerApplication.Business {
    public class Objects {        
        public class QuestionLanguageTags {
            public CTQuestionLanguage questionlanguage { get; set; }
            public EntityCollection<CTQuestionTags> questiontags { get; set; }
        }       
    }
    
}


