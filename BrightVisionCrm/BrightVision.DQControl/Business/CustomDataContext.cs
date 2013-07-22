using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Model;
using BrightVision.Common.Business;
namespace BrightVision.DQControl.Business {
    public static class CustomDataContext {
        public static List<ContactAttendie> GetContactAttendies(int accountid) {
            using (var BPContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                var query = BPContext.ExecuteStoreQuery<ContactAttendie>(
                   @"select  Attending		= cast(0 as bit),
		                    AccountID		= b.account_id,
		                    ContactID		= a.id,
		                    Name			= case when first_name is null then '' when first_name = '' then '' else first_name + ' ' end + isnull(last_name,''),
		                    [Address]		= isnull(address_1,'') + ' ' + isnull(address_2 ,''),
		                    City			= isnull(city,''),		
		                    Telephone		= case when direct_phone is null then '' when direct_phone = '' then '' else first_name + ' ' end + isnull(mobile,''),
		                    Email			= isnull(email,'')
                    from	contacts a
                    join    account_contacts b on a.id = b.contact_id
                    where   b.account_id = " + accountid.ToString());
                return query.ToList();
            };
        }        
    }
}
