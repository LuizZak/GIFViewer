using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
            pevent.Graphics.Clear(BackColor);
            pevent.Graphics.CompositingMode = CompositingMode.SourceOver;
            pevent.Graphics.CompositingQuality = CompositingQuality.HighSpeed;

            // Return if the background image is null or if the paint message is set to be ignored
            if (BackgroundImage == null || _Paint == false)
                return;

            pevent.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            // Switch the drawing quality depending on the current settings:
            switch (Quality)
            {
                // Low:
                case 1:
                    pevent.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    pevent.Graphics.SmoothingMode = SmoothingMode.None;
                    break;

                // Medium:
                case 2:
                    pevent.Graphics.InterpolationMode = InterpolationMode.High;
                    pevent.Graphics.SmoothingMode = SmoothingMode.None;
                    break;

                // High
                case 3:
                    pevent.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    pevent.Graphics.SmoothingMode = SmoothingMode.None;
                    break;
            }

            // Draw it manually:
            CalculateRectangle();
            pevent.Graphics.DrawImage(BackgroundImage, _rectangle);
        }

        /// <summary>
        /// Calculates a rectangle that fits this object's background image with a Zoom setting
        /// which scales but keeps the aspect ratio of the image
        /// </summary>
        /// <returns></returns>
        public void CalculateRectangle()
        {
            SizeF size2 = BackgroundImage.Size;

            Rectangle bounds = ClientRectangle;

            _rectangle.X = 0;
            _rectangle.Y = 0;

            float widthRatio = bounds.Width / size2.Width;
            float heightRatio = bounds.Height / size2.Height;

            if (widthRatio >= heightRatio)
            {
                _rectangle.Height = bounds.Height;
                _rectangle.Width = (int)((size2.Width * heightRatio) + 0.5f);

                if (bounds.X >= 0)
                {
                    _rectangle.X = (bounds.Width - _rectangle.Width) / 2;
                }
            }
            else
            {
                _rectangle.Width = bounds.Width;
                _rectangle.Height = (int)((size2.Height * widthRatio) + 0.5f);

                if (bounds.Y >= 0)
                {
                    _rectangle.Y = (bounds.Height - _rectangle.Height) / 2;
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
        Rectangle _rectangle;
    }
}