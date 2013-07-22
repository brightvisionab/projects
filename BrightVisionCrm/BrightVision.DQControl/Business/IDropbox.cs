using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace BrightVision.DQControl.Business {
    /// <summary>
    /// Represents a dropbox interface
    /// </summary>
    public interface IDropbox {
        /// <summary>
        /// Represents a collection of dropbox values
        /// </summary>
        List<string> DropBoxValues { get; set; }
        /// <summary>
        /// Gets or sets the text prefix of the dropbox
        /// </summary>
        string TextPrefix { get; set; }
        /// <summary>
        /// Gets or sets the selection value of the dropbox
        /// </summary>
        string SelectionValue { get; set; }
        /// <summary>
        /// Indicates if input field selectionvalue is required
        /// </summary>
        bool SelectionValueRequired { get; set; }
        /// <summary>
        /// Gets or sets the default selection value of the dropbox
        /// </summary>
        string DefaultSelectionValue { get; set; }
        /// <summary>
        /// Gets or sets the selection value of the dropbox if other should be specified
        /// </summary>
        string SelectionValueIfOther { get; set; }
        /// <summary>
        /// Indicates if input field selectionvalueifother is required
        /// </summary>
        bool SelectionValueIfOtherRequired { get; set; }
        /// <summary>
        /// Gets or sets the default selection value of the dropbox if other should be specified
        /// </summary>
        string DefaultSelectionValueIfOther { get; set; }
        /// <summary>
        /// Gets or sets the max width of the dropbox
        /// </summary>
        string DropboxMaxWidth { get; set; }
        /// <summary>
        /// Indicates if the Freetextbox or other value is visible
        /// </summary>
        bool SelectionValueIfOtherVisible { get; set; }
    }
}
