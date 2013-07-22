using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using BrightVision.Model;
using BrightVision.Common.Business;

namespace ManagerApplication.Business
{
    public class BusinessLanguage
    {
        #region Classes
        /// <sumamary>
        /// Language instance
        /// </sumamary>
        public class LanguageInstance
        {
            public int id { get; set; }
            public string code { get; set; }
            public string description { get; set; }
        }
        #endregion

        #region Public Methods
        /// <sumamary>
        /// Get languages
        /// </sumamary>
        public static ObjectQuery GetLanguages()
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objLanguages =
                from objLanguage in objBrightPlatformEntity.languages
                orderby objLanguage.name
                select new LanguageInstance
                {
                    id = objLanguage.id,
                    code = objLanguage.code,
                    description = objLanguage.name
                };

            return (ObjectQuery) objLanguages;
        }

        /// <sumamary>
        /// Save language
        /// </sumamary>
        public static int SaveLanguage(language objLanguage)
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            objBrightPlatformEntity.languages.AddObject(objLanguage);
            objBrightPlatformEntity.SaveChanges();
            return objLanguage.id;
        }

        /// <sumamary>
        /// Save languages
        /// </sumamary>
        public static void SaveUpdatedLanguages(List<LanguageInstance> objList)
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            foreach (LanguageInstance Item in objList)
            {
                var objItem = objBrightPlatformEntity.languages.Where(element => element.id == Item.id).FirstOrDefault();
                objItem.code = Item.code;
                objItem.name = Item.description;
                objBrightPlatformEntity.SaveChanges();
            }
        }
        #endregion
    }
}
