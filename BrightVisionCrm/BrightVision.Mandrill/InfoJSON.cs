using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightVision.Mandrill
{
    public class InfoJSON
    {
        private string _username;
        private string _created_at;
        private string _public_id;
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Created_at
        {
            get { return _created_at; }
            set { _created_at = value; }
        }


        private class status
        {
            private StatsTemplate _today = null;
        }

        private class StatsTemplate
        {
            private string _sent;
        }
    }
}
