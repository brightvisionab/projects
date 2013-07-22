using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using System.Data.Objects;
using System.Data;

namespace BrightVision.Common.Business
{
    public class ObjectCountry
    {
        public static ObjectQuery GetCountries()
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var objCounties = 
                from objCountry in objDbModel.countries
                orderby objCountry.name 
                select new 
                {
                    id = objCountry.id,
                    name = objCountry.name
                };

            return (ObjectQuery) objCounties;
        }
    }
}
