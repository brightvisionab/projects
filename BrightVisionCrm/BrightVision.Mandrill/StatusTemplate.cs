using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightVision.Mandrill
{
    class StatusTemplate
    {
        private string _sent;
        private string _hard_bounces;
        private string _soft_bounces;
        private string _rejects;
        private string _complaints;
        private string _unsubs;
        private string _opens;
        private string _unique_opens;
        private string _clicks;
        private string _unique_clicks;

        public string sent
        {
            get { return _sent; }
            set { _sent = value; }
        }

        public string hard_bounces
        {
            get { return _hard_bounces; }
            set { _hard_bounces = value; }
        }

        public string soft_bounces
        {
            get { return _soft_bounces; }
            set { _soft_bounces = value; }
        }

        public string rejects
        {
            get { return _rejects; }
            set { _rejects = value; }
        }

        public string complaints
        {
            get { return _complaints; }
            set { _complaints = value; }
        }

        public string unsubs
        {
            get { return _unsubs; }
            set { _unsubs = value; }
        }

        public string opens
        {
            get { return _opens; }
            set { _opens = value; }
        }

        public string unique_opens
        {
            get { return _unique_opens; }
            set { _unique_opens = value; }
        }

        public string clicks
        {
            get { return _clicks; }
            set { _clicks = value; }
        }

        public string unique_clicks
        {
            get { return _unique_clicks; }
            set { _unique_clicks = value; }
        }
    }
}
