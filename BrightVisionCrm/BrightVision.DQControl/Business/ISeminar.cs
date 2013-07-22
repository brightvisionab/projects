using System;
using System.Collections.Generic;
using System.ComponentModel;
namespace BrightVision.DQControl.Business {
    /// <summary>
    /// Represents a Seminar interface
    /// </summary>
    public interface ISeminar {
        /// <summary>
        /// Gets or sets the label of add contact button
        /// </summary>
        string AddContactButtonLabel { get; set; }
        /// <summary>
        /// Gets or sets the label of the attendies list
        /// </summary>
        string AttendiesLabel { get; set; }
        ///// <summary>
        ///// Gets or sets the calendar option
        ///// </summary>      
        SeminarSchedule SeminarSchedule { get; set; }
        /// <summary>
        /// Gets or sets the list of attendies.
        /// </summary>
        List<Attendie> Attendies { get; set; }
        /// <summary>
        /// Gets or sets the other choice object
        /// </summary>
        List<OtherChoice> OtherChoices { get; set; }
    }
}
