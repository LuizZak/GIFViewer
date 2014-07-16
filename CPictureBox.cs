using System;
using System.Drawing;
using System.Windows.Forms;

namespace GIF_Viewer
{
    /// <summary>
    /// Represents a custom picture box that is setup to minimize flickering as much as possible when redrawing multiple times a second
    /// </summary>
    class CPictureBox : PictureBox
    {
        /// <summary>
        /// Image quality
        /// </summary>
        public int Quality = 3;

        /// <summary>
        /// Initializes a new instance of the CPictureBox object
        /// </summary>
        public CPictureBox()
        {
            // Set some styles to optimize and make things pretty
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// Overriden OnPaintBackground method that redraws the image at every call
        /// </summary>
        /// <param name="pevent">The event message for this paint event</param>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Clear the panel:
            pevent.Graphics.Clear(this.BackColor);

            // Return if the background image is null or if the paint message is set to be ignored
            if (this.BackgroundImage == null || _Paint == false)
                return;

            // Switch the drawing quality depending on the current settings:
            switch (Quality)
            {
                // Low:
                case 1:
                    pevent.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    pevent.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                    break;

                // Medium:
                case 2:
                    pevent.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                    pevent.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    break;

                // High
                case 3:
                    pevent.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    pevent.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    break;
            }

            // Draw it manually:
            CalculateRectangle();
            pevent.Graphics.DrawImage(this.BackgroundImage, rectangle);
        }

        /// <summary>
        /// Calculates a rectangle that fits this object's background image with a Zoom setting
        /// which scales but keeps the aspect ratio of the image
        /// </summary>
        /// <returns></returns>
        public void CalculateRectangle()
        {
            SizeF size2 = BackgroundImage.Size;

            Rectangle bounds = this.ClientRectangle;

            rectangle.X = 0;
            rectangle.Y = 0;

            float widthRatio = ((float)bounds.Width) / (size2.Width);
            float heightRatio = ((float)bounds.Height) / (size2.Height);

            if (widthRatio >= heightRatio)
            {
                rectangle.Height = bounds.Height;
                rectangle.Width = (int)((size2.Width * heightRatio) + 0.5f);

                if (bounds.X >= 0)
                {
                    rectangle.X = (bounds.Width - rectangle.Width) / 2;
                }
            }
            else
            {
                rectangle.Width = bounds.Width;
                rectangle.Height = (int)((size2.Height * widthRatio) + 0.5f);

                if (bounds.Y >= 0)
                {
                    rectangle.Y = (bounds.Height - rectangle.Height) / 2;
                }
            }
        } 

        /// <summary>
        /// The Draw message
        /// </summary>
        const short WM_PAINT = 0x00F;
        /// <summary>
        /// Whether the program should respond to the drawm (WM_PAINT [0x00F]) message
        /// </summary>
        public bool _Paint = true;

        /// <summary>
        /// Overrided Windows message handler used to reduce the flickering of the control
        /// </summary>
        /// <param name="m">The message sent to this Control</param>
        protected override void WndProc(ref Message m)
        {
            // Draw message? Not set to draw? Skip with a zero result!
            if (m.Msg == WM_PAINT && !_Paint)
            {
                m.Result = IntPtr.Zero;
            }
            // If not, Base WndProc it:
            else
                base.WndProc(ref m);
        }

        /// <summary>
        /// Stored rectangle used on each rectangle calculation
        /// </summary>
        Rectangle rectangle;
    }
}