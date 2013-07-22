using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using BrightVision.Model;
using BrightVision.Common.Business;

namespace ManagerApplication.Business
{
    public class BusinessTitle
    {
        #region Classes
        /// <sumamary>
        /// Title instance
        /// </sumamary>
        public class TitleInstance
        {
            public int id { get; set; }
            public int language_id { get; set; }
            public int? ssyk { get; set; }
            public DateTime? datecreated { get; set; }
        }
        #endregion

        #region Public Methods
        /// <sumamary>
        /// Get languages
        /// </sumamary>
        public static ObjectQuery GetTitles()
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objTitles =
                from objTitle in objBrightPlatformEntity.titles
                orderby objTitle.occurences
                select new TitleInstance
                {
                    id = objTitle.id,
                    language_id = objTitle.language_id,
                    ssyk = objTitle.ssyk,
                    datecreated = objTitle.date_created
                };

            return (ObjectQuery) objTitles;
        }

        /// <sumamary>
        /// Save title
        /// </sumamary>
        public static void SaveTitle(title objTitle)
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            objBrightPlatformEntity.titles.AddObject(objTitle);
            objBrightPlatformEntity.SaveChanges();
        }

        /// <sumamary>
        /// Save titles
        /// </sumamary>
        public static void SaveUpdatedTitles(List<TitleInstance> objList)
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            foreach (TitleInstance Item in objList)
            {
                var objItem = objBrightPlatformEntity.titles.Where(element => element.id == Item.id).FirstOrDefault();
                objItem.language_id = Item.language_id;
                objItem.ssyk = Item.ssyk;
                objItem.date_created = Item.datecreated;
                objBrightPlatformEntity.SaveChanges();
            }
        }
        #endregion
    }
}
