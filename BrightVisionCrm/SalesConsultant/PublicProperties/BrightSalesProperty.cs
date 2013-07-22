
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesConsultant.PublicProperties
{
    public class BrightSalesProperty
    {
        #region Fields
        private CampaignListProperty _campaign;
        private DialogEditorProperty _dialogEditor;
        private CampaignBookingProperty m_CampaignBooking;
        private CommonProperty m_CommonProperty;
        private CommonProperty m_EventLogProperty;
        private EventsProperty m_EventsProperty;
        #endregion

        #region Properties
        public CampaignListProperty CampaignList
        {
            get {
                if(_campaign == null)
                    _campaign = new CampaignListProperty();

                return _campaign;
            }
        }
        public DialogEditorProperty DialogEditor { 
            get { 
                if(_dialogEditor == null)
                    _dialogEditor = new DialogEditorProperty();

                return _dialogEditor;
            } 
        }
        public CampaignBookingProperty CampaignBooking {
            get {
                if (m_CampaignBooking == null) {
                    m_CampaignBooking = new CampaignBookingProperty();
                    
                }

                return m_CampaignBooking;
            }
        }
        public CommonProperty CommonProperty
        {
            get {
                if (m_CommonProperty == null)
                    m_CommonProperty = new CommonProperty();

                return m_CommonProperty;
            }
        }
        public CommonProperty EventLogProperty
        {
            get {
                if (m_EventLogProperty == null)
                    m_EventLogProperty = new CommonProperty();

                return m_EventLogProperty;
            }
        }
        public EventsProperty EventsProperty
        {
            get {
                if (m_EventsProperty == null)
                    m_EventsProperty = new EventsProperty();

                return m_EventsProperty;
            }
        }
        #endregion
    }
}
