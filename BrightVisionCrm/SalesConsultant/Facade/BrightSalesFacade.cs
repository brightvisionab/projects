using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.FileManagement;
using BrightVision.Logging;
using BrightVision.Logging.Enums;
using BrightVision.Common.Events.Core;
using SalesConsultant.PublicProperties;
//using SalesConsultant.Observer;

namespace SalesConsultant.Facade
{
    public static class BrightSalesFacade
    {
        private static WebDavFileManager _webDavFile;
        public static WebDavFileManager WebDavFile
        {
            get {
                if (_webDavFile == null)
                    _webDavFile = new WebDavFileManager();

                return _webDavFile;
            }
        }

        private static Logger _logger;
        public static Logger Logger
        {
            get {
                if (_logger == null)
                    _logger = new Logger( BrightVisionApplication.BrightSales);

                return _logger;
            }
        }

        public static EventAggregator _eventAggregator;
        public static IEventAggregator EventBus
        {
            get {
                if (_eventAggregator == null)
                    _eventAggregator = new EventAggregator();

                return _eventAggregator;
            }
        }

        public static BrightSalesProperty _brightSalesProperty;
        public static BrightSalesProperty Property 
        {
            get {
                if (_brightSalesProperty == null)
                    _brightSalesProperty = new BrightSalesProperty();

                return _brightSalesProperty;
            }
        }
    }
}
