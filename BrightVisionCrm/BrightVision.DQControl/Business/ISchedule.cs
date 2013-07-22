using System;
using System.Collections.Generic;
using System.ComponentModel;
namespace BrightVision.DQControl.Business {
    /// <summary>
    /// Represents a Schedule interface
    /// </summary>
    public interface ISchedule {
        /// <summary>
        /// Gets or sets the label of view detail summary button
        /// </summary>
        string ViewDetailSummaryButtonLabel { get; set; }
        /// <summary>
        /// Gets or sets the label of the create meeting button
        /// </summary>
        string CreateMeetingButtonLabel { get; set; }
        /// <summary>
        /// Gets or sets the label of the Add caller
        /// </summary>
        string AddCallerButtonLabel { get; set; }
        /// <summary>
        /// Gets or sets the label of the Add additional attendi button
        /// </summary>
        string AddAdditionalAttendieButtonLabel { get; set; }
        /// <summary>
        /// Gets or sets the label of the delete attendie button
        /// </summary>
        string DeleteAttendieButtonLabel { get; set; }
        /// <summary>
        /// Gets or sets the label of the list of available bookings
        /// </summary>
        string ListOfBookingsAvailableLabel { get; set; }
        /// <summary>
        /// Indicates if input field list of bookings available is required
        /// </summary>
        bool ListOfBookingsAvailableRequired { get; set; }
        /// <summary>
        /// Gets or sets the Schedule type
        /// </summary>               
        ScheduleType ScheduleType { get; set; }
        /// <summary>
        /// Gets or sets the Schedule sales person
        /// </summary>
        ScheduleSalesPerson ScheduleSalesPerson { get; set; }
        /// <summary>
        /// Gets or sets the Booking Schedule Value
        /// </summary>
        ScheduleValue ScheduleValue { get; set; }
        /// <summary>
        /// Gets or sets the label of the attendies list
        /// </summary>
        string AttendiesLabel { get; set; }
        /// <summary>
        /// Indicates if input field is required
        /// </summary>
        bool AttendiesRequired { get; set; }
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
