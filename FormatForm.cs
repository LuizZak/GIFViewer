using System;
using System.Drawing;
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
        public string SavePath;

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
        readonly FolderBrowserDialog _fbd = new FolderBrowserDialog();

        /// <summary>
        /// List of invalid filename characters
        /// </summary>
        readonly char[] _invalid;

        /// <summary>
        /// Warns the user about invalid characters
        /// </summary>
        readonly ToolTip _invalidTooltip = new ToolTip();

        /// <summary>
        /// The text to show on the Invalid Characters Tooltip
        /// </summary>
        readonly string _invalidText;

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

            _fbd.Description = @"Image save location:";

            cmbFormat.SelectedIndex = 0;

            // Initialize the invalid characters array
            _invalid = new [] { '\\', '/', ':', '*', '?', '\'', '<', '>', '|' };
            _invalidText = "In a file/folder name you cannot use any of the following characters:\n\n ";
            foreach (char c in _invalid)
            {
                _invalidText += c + " ";
            }
            _invalidTooltip.IsBalloon = true;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            _fbd.ShowDialog(this);

            txtPath.Text = _fbd.SelectedPath;
            SavePath = _fbd.SelectedPath;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            Close();
        }

        private void txtBaseFilename_TextChanged(object sender, EventArgs e)
        {
            FileName = txtBaseFilename.Text;
        }

        private void txtBaseFilename_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 8)
            {
                return;
            }

            int index = 0;

            while (index < _invalid.Length - 1)
            {
                if (_invalid[index] == e.KeyChar)
                {
                    e.Handled = true;

                    System.Media.SystemSounds.Asterisk.Play();

                    Point p = Point.Empty;

                    p.Y += txtBaseFilename.Height;

                    _invalidTooltip.Show(_invalidText, txtBaseFilename, p);

                    break;
                }

                index++;
            }
        }

        private void cmbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            string item = cmbFormat.SelectedItem.ToString();
            int spaceIndex = item.IndexOf(" ", StringComparison.Ordinal);

            Extension = spaceIndex > 0 ? item.Substring(0, spaceIndex) : item;
        }
    }
}