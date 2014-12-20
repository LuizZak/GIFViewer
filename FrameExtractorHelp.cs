using System;
using System.Windows.Forms;

namespace GIF_Viewer
{
    /// <summary>
    /// Class representing the help window that pops up when the user clicks the help button on the Frame Extract dialog
    /// </summary>
    public partial class FrameExtractorHelp : Form
    {
        /// <summary>
        /// Initializes a new instance of the FrameExtractorHelp class
        /// </summary>
        public FrameExtractorHelp()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event fired when the user clicks the 'Close' button
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The EventArgs for this event</param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}