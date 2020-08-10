﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using GIF_Viewer.Controls;

namespace GIF_Viewer.Views
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
        public GifFile CurrentGif;

        /// <summary>
        /// Whether the current GIF file is playing
        /// </summary>
        public bool Playing = true;

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
        readonly FormatForm _formatForm = new FormatForm();

        /// <summary>
        /// Initializes a new instance of the FrameExtract class
        /// </summary>
        public FrameExtract()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// Initializes a new instance of the FrameExtract class with a gif to manipulate
        /// </summary>
        /// <param name="gif">The gif file to manipulate on this form</param>
        public FrameExtract(GifFile gif)
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            CurrentGif = gif;
        }

        /// <summary>
        /// Initializes this FrameExtract window and prepare it to display the .GIF file provided through the properties
        /// </summary>
        public void Init()
        {
            // Create the timer
            AnimationTimer = new Timer();
            AnimationTimer.Tick += AnimationTimer_Tick;

            LoadGif(CurrentGif.GifPath);

            // timelineControl1.maximum = Frames - 1;
            tlc_timeline.Minimum = 1;
            tlc_timeline.Maximum = CurrentGif.FrameCount - 1;
            tlc_timeline.SecondKnob.Value = CurrentGif.FrameCount - 1;

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

            if (Playing)
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
        int _lastFrame;
        /// <summary>
        /// Animate the current GIF
        /// </summary>
        void Animate()
        {
            try
            {
                int newFrame = CurrentGif.CurrentFrame;

                if (Playing)
                {
                    if (Range.Y > 0)
                    {
                        newFrame = (++newFrame) % (Range.X + Range.Y);

                        if (newFrame < Range.X - 1)
                            newFrame = Range.X - 1;

                        if (newFrame > CurrentGif.FrameCount)
                            newFrame = CurrentGif.FrameCount;
                    }
                    else
                    {
                        newFrame = Range.X - 1;
                    }
                }

                if (newFrame != _lastFrame)
                {
                    // Change the frame
                    cpb_preview._Paint = false;

                    CurrentGif.SetCurrentFrame(newFrame);

                    cpb_preview._Paint = true;
                    cpb_preview.Invalidate();

                    ChangeFrame(newFrame);
                }
            }
            catch (Exception ex)
            {
                FormMain.Trace(ex);
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
        void LoadGif(string fileName)
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
            CurrentGif.LoadFromPath(fileName).ContinueWith(task =>
            {
                // Set the caption
                Text = @"Extract Frames from [" + fileName + @"] " + CurrentGif.Width + @"x" + CurrentGif.Height;

                // Refresh the pictureBox with the new animation
                cpb_preview.BackgroundImage = CurrentGif.CurrentFrameBitmap;

                // Change the window size and location only if windowed
                if (WindowState == FormWindowState.Normal)
                {
                    // Set the client size
                    ClientSize = new Size(Math.Max(CurrentGif.Width, MinimumSize.Width), CurrentGif.Height + panel1.Height + 6);

                    // And position
                    int px = Screen.PrimaryScreen.Bounds.Width / 2 - Width / 2;
                    int py = Screen.PrimaryScreen.Bounds.Height / 2 - Height / 2;

                    Location = new Point(px, py);
                }

                // Starts a new animation thread
                if (CurrentGif.FrameCount > 1)
                {
                    AnimationTimer.Interval = UseMinFrameInterval ? Math.Max(CurrentGif.GetIntervalForCurrentFrame(), MinFrameInterval) : CurrentGif.GetIntervalForCurrentFrame();
                    AnimationTimer.Start();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Export the selected frames into a folder
        /// </summary>
        public void ExportFrames()
        {
            // Get the path and trim the leading characters:
            string path = _formatForm.SavePath.Trim(' ', '\\');

            // Get the filename and extension:
            string fileName = _formatForm.FileName;
            string extension = _formatForm.Extension;

            // Get a valid imageformat to use in the savind process:
            var format = ExtractCommandLineHandler.GetFormatByString(extension);

            // Get the frame range:
            var range = tlc_timeline.GetRange();
            range.X -= 1;

            // Unload the GIF from the memory, so we can work it with:
            LoadGif("");

            ExtractCommandLineHandler.ExtractGifFrames(CurrentGif.GifPath, path, fileName, format, range.X, range.Y);

            // Reload the GIF:
            LoadGif(CurrentGif.GifPath);
        }

        /// <summary>
        /// Eveny fired when the form is being closed
        /// </summary>
        /// <param name="e">The arguments for this event</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            AnimationTimer.Stop();
            AnimationTimer.Dispose();
        }

        /// <summary>
        /// Event fired every time the user presses a keyboard key while the window is focused
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void FrameExtract_KeyDown(object sender, KeyEventArgs e)
        {
            if (!CurrentGif.Loaded || CurrentGif.FrameCount <= 0)
                return;

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

        /// <summary>
        /// Event fired every time the user clicks 'Extract Frames...' button
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void btn_extract_Click(object sender, EventArgs e)
        {
            if (_formatForm.ShowDialog(this) == DialogResult.OK)
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
            Close();
        }

        /// <summary>
        /// Event fired every time the user has changed the range of the timeline displaying timelineControl1 control
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="newRange">The new range of the timeline control</param>
        private void tlc_timeline_RangeChangedEvent(object sender, RangeChangedEventArgs newRange)
        {
            _lastFrame = -1;
            ChangeFrame(newRange.NewRange.X - 1);
            CurrentGif.CurrentFrame = newRange.NewRange.X - 1;

            AnimationTimer.Stop();
            AnimationTimer.Interval = 32;
            AnimationTimer.Start();

            Range = newRange.NewRange;
        }

        /// <summary>
        /// Event fired every time the user clicks the GIF displaying cPictureBox1 control
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void cpb_preview_MouseClick(object sender, MouseEventArgs e)
        {
            // Toggle playback
            if (e.Button == MouseButtons.Left)
            {
                Playing = !Playing;

                if (Playing)
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

            _lastMouseButton = e.Button;
        }

        /// <summary>
        /// Event fired every time the user clicks the GIF displaying cPictureBox1 control
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void cpb_preview_DoubleClick(object sender, EventArgs e)
        {
            // Toggle playback
            if (_lastMouseButton == MouseButtons.Left)
            {
                Playing = !Playing;

                if (Playing)
                {
                    AnimationTimer.Start();
                }
                else
                {
                    AnimationTimer.Stop();
                }
            }
            // Toggle quality
            else if (_lastMouseButton == MouseButtons.Right)
            {
                if (cpb_preview.Quality == 1)
                    cpb_preview.Quality = 3;
                else if (cpb_preview.Quality == 3)
                    cpb_preview.Quality = 1;
            }
        }

        /// <summary>
        /// Event fired every time the user clicks the little help button at the bottom-right corner of the window
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void btnHelp_Click(object sender, EventArgs e)
        {
            // Show the help dialog
            var helpDialog = new FrameExtractorHelp();
            helpDialog.ShowDialog(this);
        }

        /// <summary>
        /// The last mouse button that was pressed on the .gif preview panel
        /// </summary>
        private MouseButtons _lastMouseButton;
    }
}