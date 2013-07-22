using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightVision.DQControl.Business
{
    interface ISmartText
    {
        /// <summary>
        /// Gets or sets the text prefix of the textbox
        /// </summary>               
        string TextPrefix { get; set; }
        /// <summary>
        /// Gets or sets the input value of the textbox
        /// </summary>
        string InputValue { get; set; }
        /// <summary>
        /// Indicates if input field is required
        /// </summary>
        bool Required { get; set; }
        /// <summary>
        /// Gets or sets the default input value of the textbox
        /// </summary>
        string DefaultInputValue { get; set; }
        /// <summary>
        /// Gets or sets the header comment text
        /// </summary>
        string HeaderCommentText { get; set; }
        /// <summary>
        /// Gets or sets the header creation date text
        /// </summary>
        string HeaderCreationDateText { get; set; }
        /// <summary>
        /// Gets or sets the header user text
        /// </summary>
        string HeaderUserText { get; set; }
        /// <summary>
        /// Gets or sets the header customer contact text
        /// </summary>
        string HeaderCustomerContactText { get; set; }
        /// <summary>
        /// Gets or sets the order by of the history
        /// </summary>
        string OrderBy { get; set; }
        /// <summary>
        /// Gets or sets the order by direction
        /// </summary>
        string OrderDirection { get; set; }
        /// <summary>
        /// represent the values of smarttext
        /// </summary>
        List<SmartTextValue> SmartTextValues { get; set; }
    }
}
