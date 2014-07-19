﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GIF_Viewer.Views;

namespace GIF_Viewer
{
    /// <summary>
    /// Frame extractor utility form
    /// </summary>
    public partial class FrameExtract : Form
    {
        #region GIF ANIMATION SETTINGS
        /// <summary>
        /// The GIF file that the frames will be extracted from
        /// </summary>
        public GIFFile CurrentGif;

        /// <summary>
        /// Whether the current GIF file is playing
        /// </summary>
        public bool playing = true;

        /// <summary>
        /// Timer used to animate the frames
        /// </summary>
        public Timer AnimationTimer;

        // Minimum Frame Interval settings
        public bool UseMinFrameInterval = true;
        public int MinFrameInterval = 50;
        #endregion

        /// <summary>
        /// Describes the range of frames to play
        /// </summary>
        public Point Range;

        /// <summary>
        /// The format form assigned to this form
        /// </summary>
        FormatForm formatForm = new FormatForm();

        /// <summary>
        /// Initializes a new instance of the FrameExtract class
        /// </summary>
        public FrameExtract()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// Initializes this FrameExtract window and prepare it to display the .GIF file provided through the properties
        /// </summary>
        public void Init()
        {
            // Create the timer
            AnimationTimer = new System.Windows.Forms.Timer();
            AnimationTimer.Tick += new EventHandler(AnimationTimer_Tick);

            LoadGIF(CurrentGif.GIFPath);

            // timelineControl1.maximum = Frames - 1;
            tlc_timeline.Minimum = 1;
            tlc_timeline.Maximum = CurrentGif.GetFrameCount() - 1;
            tlc_timeline.SecondKnob.Value = CurrentGif.GetFrameCount() - 1;

            Range = tlc_timeline.GetRange();
        }

        #region GIF ANIMATION ROUTINES

        /// <summary>
        /// Event fired every time the AnimationTimer ticks. Updates the animation to the next frame
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            Animate();

            if (playing)
            {
                AnimationTimer.Interval = UseMinFrameInterval ? Math.Max(CurrentGif.GetIntervalForCurrentFrame(), MinFrameInterval) : CurrentGif.GetIntervalForCurrentFrame();
            }
            else
            {
                AnimationTimer.Stop();
            }
        }

        /// <summary>
        /// The last frame that was renderedon the picture box
        /// </summary>
        int lastFrame = 0;
        /// <summary>
        /// Animate the current GIF
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The EventArgs for this event</param>
        void Animate()
        {
            try
            {
                if (playing)
                {
                    if (Range.Y > 0)
                    {
                        CurrentGif.currentFrame = (++CurrentGif.currentFrame) % (Range.X + Range.Y);

                        if (CurrentGif.currentFrame < Range.X - 1)
                            CurrentGif.currentFrame = Range.X - 1;

                        if (CurrentGif.currentFrame > CurrentGif.Frames)
                            CurrentGif.currentFrame = CurrentGif.Frames;
                    }
                    else
                    {
                        CurrentGif.currentFrame = Range.X - 1;
                    }
                }

                if (CurrentGif.currentFrame != lastFrame)
                {
                    // Change the frame
                    cpb_preview._Paint = false;

                    CurrentGif.SetCurrentFrame(CurrentGif.currentFrame);

                    cpb_preview._Paint = true;
                    cpb_preview.Invalidate();

                    ChangeFrame(CurrentGif.currentFrame);
                }
            }
            catch (Exception ex)
            {
                FormMain.trace(ex);
            }
        }

        #endregion

        /// <summary>
        /// Changes the timeline control's currently displayed frame
        /// </summary>
        /// <param name="frame">The new frame to set</param>
        void ChangeFrame(int frame)
        {
            tlc_timeline.CurrentFrame = frame + 1;
        }

        /// <summary>
        /// Loads a GIF to show from a file
        /// </summary>
        /// <param name="fileName">The .GIF filepath</param>
        void LoadGIF(string fileName)
        {
            // Clear everything if the user provides an empty string:
            if (fileName == "")
            {
                // Dispose and nullify everything:
                CurrentGif.Dispose();

                cpb_preview.BackgroundImage = null;

                AnimationTimer.Stop();

                // Update the form:
                Update();

                return;
            }

            // Get the intervals:
            CurrentGif.LoadFromPath(fileName);

            // Set the caption
            Text = "Extract Frames from [" + fileName + "] " + CurrentGif.Width + "x" + CurrentGif.Height;

            // Refresh the pictureBox with the new animation
            cpb_preview.BackgroundImage = CurrentGif.GIF;

            // Change the window size and location only if windowed
            if (WindowState == FormWindowState.Normal)
            {
                // Set the client size
                this.ClientSize = new Size(Math.Max(CurrentGif.Width, this.MinimumSize.Width), CurrentGif.Height + panel1.Height + 6);

                // And position
                int px = Screen.PrimaryScreen.Bounds.Width / 2 - Width / 2;
                int py = Screen.PrimaryScreen.Bounds.Height / 2 - Height / 2;

                this.Location = new Point(px, py);
            }

            // Starts a new animation thread
            if (CurrentGif.GetFrameCount() > 1)
            {
                AnimationTimer.Interval = UseMinFrameInterval ? Math.Max(CurrentGif.GetIntervalForCurrentFrame(), MinFrameInterval) : CurrentGif.GetIntervalForCurrentFrame();
                AnimationTimer.Start();
            }
        }

        /// <summary>
        /// Export the selected frames into a folder
        /// </summary>
        public void ExportFrames()
        {
            // Get the current time to use in the iterator:
            DateTime t = DateTime.Now;

            // Get the path and trim the leading characters:
            string path = formatForm.SavePath.Trim(' ', '\\');

            // Get the filename and extension:
            string fileName = formatForm.FileName;
            string extension = formatForm.Extension;

            // Get a valid imageformat to use in the savind process:
            ImageFormat format = GetFormatByString(extension);

            // Get the frame range:
            Point range = tlc_timeline.GetRange();
            range.X -= 1;

            // Unload the GIF from the memory, so we can work it with:
            LoadGIF("");

            // Load the GIF file:
            Image m = Image.FromFile(CurrentGif.GIFPath);

            // Get the frame dimension to advance the frames:
            FrameDimension frameDimension = new FrameDimension(m.FrameDimensionsList[0]);

            try
            {
                for (int x = range.X; x < (range.X + 1) + range.Y; x++)
                {
                    // Get the name:
                    string name = fileName;

                    // Replace the tokens:
                    while (name.Contains("{%i}"))
                        name = name.Replace("{%i}", (x + 1) + "");
                    while (name.Contains("{%h}"))
                        name = name.Replace("{%h}", "" + t.Hour);
                    while (name.Contains("{%m}"))
                        name = name.Replace("{%m}", "" + t.Minute);
                    while (name.Contains("{%s}"))
                        name = name.Replace("{%s}", "" + t.Second);

                    // Create the bitmap and the graphics:
                    Bitmap b = new Bitmap(m.Width, m.Height);
                    Graphics g = Graphics.FromImage(b);

                    // Advance to the desired frame:
                    m.SelectActiveFrame(frameDimension, x);

                    // Draw the image:
                    g.DrawImageUnscaled(m, 0, 0);

                    // Save the image down to the path with the given format:
                    b.Save(path + "\\" + name + extension, format);

                    // Dispose the bitmap and the graphics:
                    g.Dispose();
                    b.Dispose();
                }
            }
            catch (Exception e)
            {
                ErrorBox.Show("Error exporting frames: " + e.Message, "Error", e.StackTrace);
            }

            // Dispose the GIF:
            m.Dispose();

            // Reload the GIF:
            LoadGIF(CurrentGif.GIFPath);
        }

        /// <summary>
        /// Returns an ImageFormat based on the file extension provided
        /// </summary>
        /// <param name="format">A valid image file extension (including the dot)</param>
        /// <returns>An ImageFormat that represents the provided image file extension</returns>
        public ImageFormat GetFormatByString(string format)
        {
            ImageFormat imageFormat = ImageFormat.Bmp;
            
            switch (format)
            {
                case ".bmp":
                    imageFormat = ImageFormat.Bmp;
                    break;
                case ".jpg":
                    imageFormat = ImageFormat.Jpeg;
                    break;
                case ".gif":
                    imageFormat = ImageFormat.Gif;
                    break;
                case ".png":
                    imageFormat = ImageFormat.Png;
                    break;
                case ".tiff":
                    imageFormat = ImageFormat.Tiff;
                    break;
                case ".exif":
                    imageFormat = ImageFormat.Exif;
                    break;
                case ".emf":
                    imageFormat = ImageFormat.Emf;
                    break;
                case ".wmf":
                    imageFormat = ImageFormat.Wmf;
                    break;
            }

            return imageFormat;
        }

        /// <summary>
        /// Event fired everytime the user presses a keyboard key while the window is focused
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void FrameExtract_KeyDown(object sender, KeyEventArgs e)
        {
            if (CurrentGif.Loaded && CurrentGif.GetFrameCount() > 0)
            {
                // Seek GIF timeline
                if (e.KeyData == Keys.Left)
                {
                    if (tlc_timeline.CurrentFrame > 0)
                    {
                        tlc_timeline.ChangeFrame(tlc_timeline.CurrentFrame - 1);
                    }
                }
                else if (e.KeyData == Keys.Right)
                {
                    if (tlc_timeline.CurrentFrame < tlc_timeline.Maximum)
                    {
                        tlc_timeline.ChangeFrame(tlc_timeline.CurrentFrame + 1);
                    }
                }
            }
        }

        /// <summary>
        /// Event fired everytime the user clicks 'Extract Frames...' button
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void btn_extract_Click(object sender, EventArgs e)
        {
            // TODO: Add the frame extract code here
            if (formatForm.ShowDialog(this) == DialogResult.OK)
            {
                ExportFrames();
            }
        }

        /// <summary>
        /// Event fired when the user clicks on the close button
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Event fired everytime the user has changed the range of the timeline displaying timelineControl1 control
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="newRange">The new range of the timeline control</param>
        private void tlc_timeline_RangeChangedEvent(object sender, RangeChangedEventArgs newRange)
        {
            lastFrame = -1;
            ChangeFrame(newRange.NewRange.X - 1);
            CurrentGif.currentFrame = newRange.NewRange.X - 1;

            AnimationTimer.Stop();
            AnimationTimer.Interval = 32;
            AnimationTimer.Start();

            Range = newRange.NewRange;
        }

        /// <summary>
        /// Event fired everytime the user clicks the GIF displaying cPictureBox1 control
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void cpb_preview_MouseClick(object sender, MouseEventArgs e)
        {
            // Toggle playback
            if (e.Button == MouseButtons.Left)
            {
                playing = !playing;

                if (playing)
                {
                    AnimationTimer.Start();
                }
                else
                {
                    AnimationTimer.Stop();
                }
            }
            // Toggle quality
            else if(e.Button == MouseButtons.Right)
            {
                if (cpb_preview.Quality == 1)
                    cpb_preview.Quality = 3;
                else if (cpb_preview.Quality == 3)
                    cpb_preview.Quality = 1;
            }

            lastMouseButton = e.Button;
        }

        /// <summary>
        /// Event fired everytime the user clicks the GIF displaying cPictureBox1 control
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void cpb_preview_DoubleClick(object sender, EventArgs e)
        {
            // Toggle playback
            if (lastMouseButton == MouseButtons.Left)
            {
                playing = !playing;

                if (playing)
                {
                    AnimationTimer.Start();
                }
                else
                {
                    AnimationTimer.Stop();
                }
            }
            // Toggle quality
            else if (lastMouseButton == MouseButtons.Right)
            {
                if (cpb_preview.Quality == 1)
                    cpb_preview.Quality = 3;
                else if (cpb_preview.Quality == 3)
                    cpb_preview.Quality = 1;
            }
        }

        /// <summary>
        /// Event fired everytime the user clicks the little help button at the bottom-right corner of the window
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void btnHelp_Click(object sender, EventArgs e)
        {
            // Show the help dialog
            FrameExtractorHelp helpDialog = new FrameExtractorHelp();
            helpDialog.ShowDialog(this);
        }

        /// <summary>
        /// The last mouse button that was pressed on the FrameExtract
        /// </summary>
        MouseButtons lastMouseButton;
    }
}