using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

    public static class ToolTipResources
    {
        public static string StyleSheet
        {
            get
            {
                return @"
                    body { font:10pt Tahoma }
		            p    {margin:0px 0px; padding:0px 0px}           
                ";
                
            }
        }
        public static Image nurture
        {
            get { return SalesConsultant.Properties.Resources.nurture; }
        }

        public static Image todo
        {
            get { return SalesConsultant.Properties.Resources.todo; }
        }

        public static Image make_call
        {
            get { return SalesConsultant.Properties.Resources.make_call; }
        }
        public static Image date_go { 
            get { 
                return SalesConsultant.Properties.Resources.date_go; 
            } 
        }
        public static Image date_previous { 
            get { 
                return SalesConsultant.Properties.Resources.date_previous; 
            } 
        }
        public static Image assigned_to_me {
            get{
                return SalesConsultant.Properties.Resources.assigned_to_me;
            }
        }
        public static Image assigned_to_other
        {
            get
            {
                return SalesConsultant.Properties.Resources.assigned_to_other;
            }
        }
        public static Image assigned_to_team
        {
            get{
                 
                return SalesConsultant.Properties.Resources.assigned_to_team;
            }
        }
        public static Image checkbox_check {
            get {
                return SalesConsultant.Properties.Resources.checked16x16;
            }
        }
        public static Image checkbox_uncheck
        {
            get
            {
                return SalesConsultant.Properties.Resources.unchecked16x16;
            }
        }
        public static Image nurture_log
        {
            get
            {
                return SalesConsultant.Properties.Resources.nurture_log;
            }
        }
        public static Image busy_signal
        {
            get
            {
                return SalesConsultant.Properties.Resources.busy_signal;
            }
        }
        public static Image call_refered_to
        {
            get
            {
                return SalesConsultant.Properties.Resources.call_refered_to;
            }
        }
        public static Image completed
        {
            get
            {
                return SalesConsultant.Properties.Resources.completed;
            }
        }
        public static Image flag_purple
        {
            get
            {
                return SalesConsultant.Properties.Resources.flag_purple;
            }
        }
        public static Image no_answer
        {
            get
            {
                return SalesConsultant.Properties.Resources.no_answer_blue;
            }
        }
        public static Image play
        {
            get
            {
                return SalesConsultant.Properties.Resources.play;
            }
        }
        public static Image call_mobile
        {
            get
            {
                return SalesConsultant.Properties.Resources.call_mobile;
            }
        }
    }
