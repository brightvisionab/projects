using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using BrightVision.Model;
using BrightVision.Common.Business;

namespace SalesConsultant.Business
{
    public class BusinessLanguage
    {
        #region Classes
        public class LanguageInstance {
            public int id { get; set; }
            public string code { get; set; }
            public string description { get; set; }
        }
        #endregion

        #region Public Methods
        public static List<LanguageInstance> GetLanguages()
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                var _lstLanguages =
                    from objLanguage in _efDbContext.languages
                    orderby objLanguage.name
                    select new LanguageInstance {
                        id = objLanguage.id,
                        code = objLanguage.code,
                        description = objLanguage.name
                    };

                return _lstLanguages.ToList();
            }
        }
        public static int SaveLanguage(language pLanguage)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
            {
                //_efDbContext.ExecuteStoreQuery
                //_efDbContext.FIGetCallLists(0,0).encrypt;
                _efDbContext.languages.AddObject(pLanguage);
                _efDbContext.SaveChanges();
                return pLanguage.id;
            }
        }
        public static void SaveUpdatedLanguages(List<LanguageInstance> pList)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                foreach (LanguageInstance Item in pList) {
                    language _eftLanguage = _efDbContext.languages.Where(element => element.id == Item.id).FirstOrDefault();
                    _eftLanguage.code = Item.code;
                    _eftLanguage.name = Item.description;
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(_eftLanguage);
                }
            }
        }
        #endregion
    }
}
