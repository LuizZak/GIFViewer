using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GIF_Viewer
{
    /// <summary>
    /// Specifies an interface where the user can choose the save path and file format for a frame extract form
    /// </summary>
    public partial class FormatForm : Form
    {
        /// <summary>
        /// The save path
        /// </summary>
        public string SavePath = @"C:\";

        /// <summary>
        /// The filename (minus extension)
        /// </summary>
        public string FileName = "Frame-{%i}";

        /// <summary>
        /// The extension format to use
        /// </summary>
        public string Extension = ".bmp";

        /// <summary>
        /// The folder browser attached to this form
        /// </summary>
        FolderBrowserDialog fbd = new FolderBrowserDialog();

        /// <summary>
        /// List of invalid filename characters
        /// </summary>
        char[] Invalid;

        /// <summary>
        /// Warns the user about invalid characters
        /// </summary>
        ToolTip invalidTooltip = new ToolTip();

        /// <summary>
        /// The text to show on the Invalid Characters Tooltip
        /// </summary>
        string invalidText = "";

        /// <summary>
        /// Initializes a new instance of the FormatForm class
        /// </summary>
        public FormatForm()
        {
            InitializeComponent();

            // Get the desktop path
            SavePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            txtPath.Text = SavePath;
            txtBaseFilename.Text = FileName;

            fbd.Description = "Image save location:";

            cmbFormat.SelectedIndex = 0;

            // Initialize the invalid characters array
            Invalid = new char[] { '\\', '/', ':', '*', '?', '\'', '<', '>', '|' };
            invalidText = "In a file/folder name you cannot use any of the following characters:\n\n ";
            foreach (char c in Invalid)
            {
                invalidText += c + " ";
            }
            invalidTooltip.IsBalloon = true;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            fbd.ShowDialog(this);

            txtPath.Text = fbd.SelectedPath;
            SavePath = fbd.SelectedPath;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            this.Close();
        }

        private void txtBaseFilename_TextChanged(object sender, EventArgs e)
        {
            this.FileName = txtBaseFilename.Text;
        }

        private void txtBaseFilename_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 8)
            {
                return;
            }

            int index = 0;

            while (index < Invalid.Length - 1)
            {
                if (Invalid[index] == e.KeyChar)
                {
                    e.Handled = true;

                    System.Media.SystemSounds.Asterisk.Play();

                    Point p = Point.Empty;

                    p.Y += txtBaseFilename.Height;

                    invalidTooltip.Show(invalidText, txtBaseFilename, p);

                    break;
                }

                index++;
            }
        }

        private void cmbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cmbFormat.SelectedItem.ToString().IndexOf(" ") > 0)
                Extension = cmbFormat.SelectedItem.ToString().Substring(0, cmbFormat.SelectedItem.ToString().IndexOf(" "));
            else
                Extension = cmbFormat.SelectedItem.ToString();
        }
    }
}