
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using BrightVision.Model;
using BrightVision.Common.Business;

namespace SalesConsultant.Business
{
    public class EventFollowUp
    {
        #region Business Methods
        public static void Delete(int pEventId)
        {
            using (BrightPlatformEntities _eftDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                event_followup_log _eftEvent = _eftDbContext.event_followup_log.FirstOrDefault(i => i.id == pEventId);
                if (_eftEvent != null) {
                    _eftDbContext.event_followup_log.DeleteObject(_eftEvent);
                    _eftDbContext.SaveChanges();
                    _eftDbContext.Detach(_eftEvent);
                }
            }
        }
        public static List<CTScEventAndFollowUpLog> GetEventFollowUpLogs(int pSubCampaignId, int pAccountId)
        {
            List<CTScEventAndFollowUpLog> _lstData = new List<CTScEventAndFollowUpLog>();
            using (BrightPlatformEntities _eftDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _lstData = _eftDbContext.FIScGetEventAndFollowUpLogs(pSubCampaignId, pAccountId, UserSession.CurrentUser.UserId).ToList();
            }

            return _lstData;
        }
        public static void Save(List<CTScEventAndFollowUpLog> pEventList)
        {
            using (BrightPlatformEntities _eftDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                event_followup_log _eftEvent = null;
                foreach (CTScEventAndFollowUpLog _item in pEventList) {
                    _eftEvent = _eftDbContext.event_followup_log.FirstOrDefault(i => i.id == _item.id);
                    if (_eftEvent == null)
                        continue;

                    _eftEvent.done = _item.done;
                    _eftEvent.assigned_user = Convert.ToBoolean(_item.user_taken) ? UserSession.CurrentUser.UserId : 0;
                    _eftEvent.date_of_transaction = _item.date_of_transaction;
                    _eftEvent.start_time = Convert.ToDateTime(_item.start_time).TimeOfDay;
                    _eftEvent.end_time = Convert.ToDateTime(_item.end_time).TimeOfDay;
                    _eftEvent.title = _item.title;
                    _eftEvent.event_type = _item.event_type;
                    _eftEvent.short_message = _item.short_message;
                    _eftDbContext.event_followup_log.ApplyCurrentValues(_eftEvent);
                }

                if (pEventList.Count > 0) {
                    _eftDbContext.SaveChanges();
                    _eftDbContext.Detach(_eftEvent);
                }
            }
        }
        public static bool FollowUpCallExists(event_followup_log pData)
        {
            using (BrightPlatformEntities _eftDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                event_followup_log _item = _eftDbContext.event_followup_log.FirstOrDefault(
                    i => i.contact_id == pData.contact_id
                    && i.subcampaign_id == pData.subcampaign_id
                    && i.account_id == pData.account_id
                    && i.done == false
                    && pData.event_status.Equals("Follow-Up Call")
                );
                if (_item != null) {
                    _eftDbContext.Detach(_item);
                    return true;
                }

                return false;
            }
        }
        public static void Save(event_followup_log pData, bool pSaveExistingFollowUpCallsAsDone = false)
        {
            using (BrightPlatformEntities _eftDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                if (pSaveExistingFollowUpCallsAsDone) {
                    List<event_followup_log> _ToUpdateList = _eftDbContext.event_followup_log.Where(
                        i => i.contact_id == pData.contact_id
                        && i.subcampaign_id == pData.subcampaign_id
                        && i.account_id == pData.account_id
                        && i.done == false
                        && pData.event_status.Equals("Follow-Up Call")
                    ).ToList();
                    if (_ToUpdateList.Count > 0) {
                        foreach (event_followup_log _item in _ToUpdateList) {
                            event_followup_log _handler = _eftDbContext.event_followup_log.FirstOrDefault(x => x.id == _item.id);
                            _item.done = true;
                            _eftDbContext.event_followup_log.ApplyCurrentValues(_item);
                            _eftDbContext.Detach(_handler);
                        }
                    }
                }

                _eftDbContext.event_followup_log.AddObject(pData);
                _eftDbContext.SaveChanges();
                _eftDbContext.Detach(pData);
            }
        }
        public static void Save(int pEventId, bool pIsDone)
        {
            using (BrightPlatformEntities _eftDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                event_followup_log _eftEvent = _eftDbContext.event_followup_log.FirstOrDefault(i => i.id == pEventId);
                _eftEvent.done = pIsDone;
                _eftDbContext.SaveChanges();
                _eftDbContext.Detach(_eftEvent);
            }
        }
        #endregion
    }
}
