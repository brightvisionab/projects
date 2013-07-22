using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Common.Events.Core;

namespace ManagerApplication.Facade
{
    public static class BrightManagerFacade
    {
        public static EventAggregator _eventAggregator;
        public static IEventAggregator EventBus
        {
            get {
                if (_eventAggregator == null)
                    _eventAggregator = new EventAggregator();

                return _eventAggregator;
            }
        }
    }
}
