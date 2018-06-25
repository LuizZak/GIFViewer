using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

using GIF_Viewer.Controls;
using GIF_Viewer.Views;
using GIF_Viewer.Utils;
using GIF_Viewer.FileExts;

#pragma warning disable 1587

/// @title              GIF Viewer
/// @description        Helps visualizing animated and non-animated .GIF files
/// 
/// @author             Luiz Fernando
/// 
/// @version 1.7.0b     Fixed File Association strategy causing issues w/ registry access.
/// @                   Fixed an issue w/ some Gif files that did not rendered entirely in some frames near the bottom.
/// 
/// @version 1.6.6b     Added a setting to customize the default minimum frame delay setting (as well as disabling it on startup).
/// 
/// @version 1.6.5b     Now opening another .gif file in single instance mode will move the main window instance to the front.
/// 
/// @version 1.6.4b     Adding extra shortcuts for navigating the images in the folder with the arrow keys, while the focus is in the image.
/// 
/// @version 1.6.3b     Making the program store settings under the AppData folder. May fix crashes related to non-administrator users running the program.
///                     Optimizing decoding process of gif files slightly.
/// 
/// @version 1.6.2b     Added an option to allow only a single instance of the program to run at the same time.
/// 
/// @version 1.6.1b     Fixing opening instances of the program while one instance was previously running causing crashes on the subsequent instances.
/// 
/// @version 1.6.0b     Improved .gif speed when seeking frames at any point in the timeline. Utilizing code from the GifComponents (that was in turn based on NGif) open source project.
/// @                   Fixed crash related to .gif files with very fast frame intervals.
/// @                   Swapped the old timeline trackbar with a new more responsive timeline control.
/// @                   Now the value stepper for the minimum frame interval is disabled when the checkbox is also off.
/// @                   Now you can seek the .gif timeline using the arrow keys when the animation panel is focused (clicked on).
/// @                   Fixed half-pixel misalignment on images (only visible when zoomed in).
/// @                   Fixed opening and closing the frame extract form doubling the animation's playback speed on the main form.
/// @                   Added a settings panel to edit the main settings of the program.
/// 
/// @version 1.4.7b     Now the program will fail silently when trying to load the .gif files in the folder. Still on the hunt for runtime exceptions.
/// @                   Added an error form that has a copiable field for copying information about errors.
/// 
/// @version 1.4.5b     Fixed not working with no administrator rights (might fix issues with Windows 8 too)
/// 
/// @version 1.4.0b     Added a drag 'n' drop functionalty to load gif files. Holding SHIFT while dropping the files will add the files dragged to the playlist queue instead of replacing the current one.
/// @                   Fixed a small issue where dragging the whole timeline on the timeline control would result in an incorrect offset for the right knob.
/// @                   Now the form displays the index of current GIF file / total gif files on the title text when there's more than one GIF file in the current folder.
/// @                   Fixed a bug where the Play button would not be enabled when set to the 'Open...' state at the application launch.
/// @                   
/// @                   Abstracted the GIF file into its own class GIFFile.
/// 
/// @version 1.3.7b     Added a mouse wheel scroll zoom functionality to the TimelineControl.
/// @                   Added a small icon on the Play/Stop button to warn the user when opening non-GIF image files.
/// @                   Added a close button to the Frame Extract panel.
/// @                   Added icons to some of the buttons and controls to cheer things up.
/// @                   Now dragging the selected timeline on the TimelineControl drags both knobs at the same time.
/// @                   Now the default save path for the extracted frames is the Desktop folder.
/// @                   Incresed overall smoothness and responsiveness of the Frame Extract timeline and Main Form timeline controls.
/// @                   Fixed a bug that would sometimes make the GIF box flicker when playing heavier GIF files with the window maximized.
/// @                   Fixed a bug where the GIF box would reposition incorrectly when switching the window size sometimes.
/// @                   Fixed a crash related to non-standard GIF files with missing GIF properties.
/// @                   Fixed a crash related to registry access permissions.
/// @                   Fixed a crashes related to the frame extraction method.
/// @                   Fixed double clicking the Frame Extract's GIF box with the right mouse button stopping the GIF playback.
/// @                   
/// @                   Updated the TimelineControl to support negative ranges.
/// @                   Changed the TimelineControl to display the provided range value instead of adding 1 to the range.
/// @                   Changed the animation managing method from threading to a safer Timer Control method to prevent threading issues when rendering GIF files.
/// @                   Updated the main form and the FrameExtract form to match the changes made on both forms Animate and UpdateAnimations methods.
/// @                   Fixed a typo in a method name inside the CPictureBox class.
/// @                   Changed the window repositioning code on Form1.LoadGIF() to simply call CenterToScreen().
/// 
/// @version 1.1.0b     Fixed a bug with association at the Startup always reassociating the application.
/// @                   Fixed a bug in which the Extract Frame form would not update the currently selected frame if the animation was paused by the user.
/// @                   Removed the unused Form1.ImagesFromGif() method.
/// @                   Fixed Page Up and Page Down keys reloading the current .gif file when it was the only .gif file on the folder it was on.
/// @                   Removed the two arguments and a redundant check for the Dragging flag on Form1.Animate() method.
/// @                   Removed the Thread.Sleep() call inside the FrameExtract.Animate() method to speed up the waiting process.
/// @                   Removed some debug code through the project.
/// @                   Added a bunch of XML code through the project.
/// @                   Added a new icon for the program.
/// 
/// @version 1.0.0b     Fixed a serious lag on the Frame Extract window .gif animating system. Animations should now play more smoothly.
/// @                   The frame extract panel now warns the user about the .gif file format not being animated through a sulfix at the end of the .gif extension in the combobox.
/// @                   Pressing 'Esc' now quits the program.
/// @                   Removed the 'Medium' quality settings.
/// @                   Changes made to the 'Min FPS' input box are now reflected instantly
/// 
/// @version 0.9.5b     Small issue that happened when the user clicked the separator in the context menu is now fixed.
/// @                   Now the 'Frame Extract' button is disabled when the GIF file has only 1 frame.
/// @                   Now the Play button is disabled when the GIF has only 1 frame.
/// @                   Changed the 'Frame Extract' button label to 'Extract Frames...'.
/// @                   Fixed the window offset to show up the correct image size.
/// 
/// @version 0.9.4b     Now the program changes the play button to an Open button if no file is specified at startup.
/// @                   The program now disables the timeline TrackBar when no GIF is provided at startup.
/// @                   Fixed an issue with the TimelineControl.
/// 
/// @version 0.9.2b     The program now asks the user on startup if it should assign itself to the .GIF file format.
/// 
/// @version 0.9.1b     .NET framework version set from 4.0 to 3.5 to assure compatibility.
/// 
/// @version 0.9.0b     Implemented a fully working frame extraction utility.
/// 
/// @version 0.5.6b     Now the CPictureBox control uses an independent drawing function to make things faster.
/// @                   Now the knob that is currently under the mouse in the TimelineControl is drawed over the other knob.
/// @                   The TimelineControl now has a playhead, which is a red line that can be used to indicates an specific frame.
/// @                   Now the TimelineControl applies an offset correction to the knob drag code to make it more fluid.
/// @                   Fix the issue with the Frame Extractor form that breaks the GIF animation
/// 
/// @version 0.5.3b     Fixed an issue where the program would crash when closing if an invalid GIF was provided.
/// @                   The TimelineControl OnPaint routine is now a little bit faster.
/// @                   The TimelineControl from the Frame Extract form now shows the correct number of frames.
/// @                   Added some Style rules to the TimelineControl, as well as optimizing it.
/// @                   Fixed the TimelineControl OnPaint so it behaves corretly on all widths.
/// @                   Now the knobs on the TimelineControl highlight when the user hovers the mouse over them.
/// 
/// @version 0.4.7b     Added support to browse GIF files in a path. Choose a GIF file and use PageDown/PageUp to browse other GIFs inpath.
/// @                   Fixed small problem with the multi-file system skipping the last GIF.
/// @                   Now the quality selection works correctly.
/// @                   Minor fixes to ajust to the new multi-file style and also added some dumb-checks and small fixes in some places.
/// @                   Now form only resizes when in windowed mode.
/// @                   Added a TimelineControl control. It will be used in the new Frame Extract form.
/// 
/// @version 0.3.7b     Added a Quality parameter, can be changed on the image's context menu. * WON'T WORK YET *
/// 
/// @version 0.3.6b     Fixed an issue where the animation and form threads could both try to access the GIF file handler at the same time.
/// @                   Resolved an issue where the trackbar control would advance the frames using the old frame storing system.
/// @                   Now when the animation ends and it's not looped, the animation is stopped.
/// @                   Added some lines of code to fix the window position on launch.
/// @                   Minor fixes to make the threads happy.
/// @                   Organized the source code a bit.
///                     
/// @version 0.3.2b     Fixed the large memory usage by using .NET built-in GIF frame extraction system.
/// 
/// @version 0.3.1b     Added a minimum frame interval optional value.
/// @                   Fixed incorrect form sizing on image load.
/// 
/// @version 0.3.0.1b   CanLoop property added. True when animation should loop over.
/// @                   Fixed issue where animation thread would impede the program from closing completely.
/// 
/// @version 0.3b       Independent frame animation system implemented.
/// 
/// @version 0.2b       Initial Version.
/// 
/// @todo               Fix reported issues of not working on Windows 8
/// @todo               Allow the user to zoom in and out of the images
/// @todo               Provide automatic ClickOnce update
/// @todo               * DONE 0.9.2b * Make it ask the user to associate to the .gif extension on startup
/// @todo               * DONE 0.9.0b * Make a frame extraction utillity
/// @todo               * DONE 0.5.6b * Fix the issue with the Frame Extractor form that breaks the GIF animation
/// @todo               * DONE 0.5.6b * Apply an offset correction to the knob drag code to make it more fluid
/// @todo               * DONE 0.5.3b * Fix the Timeline OnPaint routine to behave correctly on all widths
/// @todo               * DONE 0.5.3b * Fix the thumbs on the Timeline controls, right now they behave as if there were an extra frame
/// @todo               * DONE 0.5.3b * Optimize the TimelineControl control OnPaint routine
/// @todo               * FIXD 0.4.7b * Make the quality selection works correctly
/// @todo               * DONE 0.3.6b * Add a ContextMenu for the image panel that pops in with an option to open the file with another program
/// @todo               * DONE 0.3.6b * Save the optional programs for the ContextMenu on a .ini file
/// @todo               * DONE 0.3.2b * Deal with the cross-threading issue where the animation and form threads both try to access the GIF animtion
/// @todo               * DONE 0.3.6b * Deal with the large memory usage by using the GIF image to show the frames instead of storing them separetely
/// 
#pragma warning restore 1587
namespace GIF_Viewer
{
    /// <summary>
    /// Main window form for the application
    /// </summary>
    public partial class FormMain : Form
    {
        /// <summary>
        /// The current GIF file being played
        /// </summary>
        readonly GifFile _currentGif;

        /// <summary>
        /// Timer used to animate the frames
        /// </summary>
        public Timer AnimationTimer;

        /// <summary>
        /// Current image on the images list
        /// </summary>
        public int CurrentImage;

        /// <summary>
        /// Images contained in this folder
        /// </summary>
        public List<string> Images;

        // Minimum Frame Interval settings
        /// <summary>
        /// Whether to use a minimum frame interval between frames so the GIF doesn't
        /// plays at maximum speed
        /// </summary>
        public bool UseMinFrameInterval = true;
        /// <summary>
        /// Minimum frame interval (in ms)
        /// </summary>
        public int MinFrameInterval = 50;

        /// <summary>
        /// The settings file handler
        /// </summary>
        public Stream IniFile;

        // The quality settings menu items
        /// <summary>
        /// Menu item for the Low rendering setting
        /// </summary>
        private ToolStripMenuItem _lowQualityMenuItem;
        // The quality settings menu items
        /// <summary>
        /// Menu item for the Medium rendering setting
        /// </summary>
        private ToolStripMenuItem _mediumQualityMenuItem;
        /// <summary>
        /// Menu item for the high rendering setting
        /// </summary>
        private ToolStripMenuItem _highQualityMenuItem;

        /// <summary>
        /// The 'Frame Extract' menu item in the right-click context menu
        /// </summary>
        public ToolStripItem FrameExtract;

        /// <summary>
        /// Helper object used to check the file association for the .gif extension on Windows
        /// </summary>
        public GifFileAssociation Association;

        /// <summary>
        /// Windows' Open File Dialog:
        /// </summary>
        private readonly OpenFileDialog _op = new OpenFileDialog();

        /// <summary>
        /// Main entry point for this Form
        /// </summary>
        /// <param name="args"></param>
        public FormMain(string[] args)
        {
            InitializeComponent();
            
            _currentGif = new GifFile();

            Images = new List<string>();

            nud_minFrameInterval.Value = MinFrameInterval;
            cb_useMinFrameInterval.Checked = UseMinFrameInterval;

            PlayBtn.Enabled = false;
            tlc_timeline.Enabled = false;

            // Create the timer
            AnimationTimer = new Timer();
            AnimationTimer.Tick += AnimationTimer_Tick;

            try
            {
                // Load the settings:
                LoadSettings();

                // Read off the file association:
                Association = new GifFileAssociation();

                // Check .GIF file association
                if (Association.HasWriteAccess() && !Settings.Instance.DontAskAssociate)
                {
                    if (!Association.IsAssociated())
                    {
                        var res = MessageBox.Show(this, @"Do you want to associate the .gif files to GIF Viewer? (Hit Cancel to never ask again)", @"Question", MessageBoxButtons.YesNoCancel);

                        // Change association
                        if (res == DialogResult.Yes)
                        {
                            try
                            {
                                Association.Associate();
                            }
                            catch (Exception e)
                            {
                                ErrorBox.Show("Error associating program: " + e.Message, "Error", e.StackTrace);
                            }
                        }
                        else if (res == DialogResult.Cancel)
                        {
                            Settings.Instance.DontAskAssociate = true;

                            SaveSettings();
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            ProcessCommandLine(args);

            // Register the drag 'n drop handlers
            DragEnter += FormMain_DragEnter;
            DragDrop += FormMain_DragDrop;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            
            // Load playback settings
            UseMinFrameInterval = Settings.Instance.SetMinimumDelayOnStartup;
            MinFrameInterval = Settings.Instance.MinimumFrameDelay;

            cb_useMinFrameInterval.Checked = UseMinFrameInterval;
            nud_minFrameInterval.Value = MinFrameInterval;
        }

        /// <summary>
        /// Processes a given string of command line into this form instance
        /// </summary>
        /// <param name="args">The commang line to process</param>
        public void ProcessCommandLine(string[] args)
        {
            // Load a gif file from the command line arguments
            if (args.Length <= 0 || !File.Exists(args[0]))
            {
                if (_currentGif.GifPath == "")
                    _openFile = true;

                return;
            }

            LoadGifAsync(args[0], true);
        }

        /// <summary>
        /// Event fired when the window is shown for the first time
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void Form1_Shown(object sender, EventArgs e)
        {
            AllowDrop = true;

            if (!_openFile)
                return;

            // Asks the user for a .gif file using the OpenFileDialog object
            _op.Filter = @"GIF files (*.gif)|*.gif";

            // The user has chosen a valid .gif file
            if (_op.ShowDialog() != DialogResult.OK)
            {
                // Set the play button as an open file button:
                PlayBtn.Text = @"&Open...";
                PlayBtn.Enabled = true;
                return;
            }

            LoadGifAsync(_op.FileName, true);
        }

        private void LoadGifAsync(string path, bool loadGifsInFolder)
        {
            LoadGif(path).ContinueWith(task =>
            {
                if (IsDisposed || !Visible)
                {
                    return;
                }

                if (task.Exception == null)
                {
                    if (loadGifsInFolder)
                        LoadGifsInFolder(Path.GetDirectoryName(path));

                    return;
                }

                ErrorBox.Show("Error: " + task.Exception.Message, "Error", task.Exception.StackTrace);

                // Set the play button as an open file button:
                PlayBtn.Text = @"&Open...";
                PlayBtn.Enabled = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        void UnloadGif()
        {
            // Stops the animation timer, if it's running
            AnimationTimer.Stop();

            // Dispose and nullify everything:
            _currentGif.Dispose();

            pb_gif.BackgroundImage = null;

            // Update the form:
            Update();
        }

        /// <summary>
        /// Loads all the .GIF images from a folder into the Images list
        /// </summary>
        /// <param name="folder">the folder to search the GIFs on</param>
        void LoadGifsInFolder(string folder)
        {
            // Load the .GIF images
            Images.Clear();
            Images.AddRange(Directory.GetFiles(folder, "*.gif"));
            
            // Locate the current .GIF image on the images list:
            CurrentImage = Images.IndexOf(_currentGif.GifPath);

            UpdateTitle();
        }

        /// <summary>
        /// Event fired every time the AnimationTimer ticks. Updates the animation to the next frame
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            Animate();

            if (_currentGif.Playing)
            {
                AnimationTimer.Interval = UseMinFrameInterval ? Math.Max(_currentGif.GetIntervalForCurrentFrame(), MinFrameInterval) : _currentGif.GetIntervalForCurrentFrame();
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
        /// Animates the current GIF
        /// </summary>
        void Animate()
        {
            try
            {
                int newFrame = _currentGif.CurrentFrame;

                if (!tlc_timeline.DraggingFrame && _currentGif.Playing)
                {
                    // Change the current frame
                    newFrame = (_currentGif.CurrentFrame + 1) % _currentGif.FrameCount;
                    ChangeTimelineFrame(newFrame);
                }

                if (newFrame != _lastFrame)
                {
                    _lastFrame = newFrame;
                    // Redraw the GIF panel
                    pb_gif._Paint = false;

                    _currentGif.SetCurrentFrame(newFrame);
                    UpdateFrameText();

                    pb_gif._Paint = true;

                    pb_gif.Invalidate();
                }

                if (_currentGif.CurrentFrame == (_currentGif.FrameCount) && _currentGif.CanLoop == false)
                {
                    _currentGif.Playing = false;
                }
            }
            catch (Exception ex)
            {
                Trace(ex);
            }
        }

        /// <summary>
        /// Load the settings from an .ini file located at the program's main path
        /// </summary>
        public void LoadSettings()
        {
            Settings.Instance.LoadSettings();

            // Fill in the contextMenu with the programs:
            cms_gifRightClick.Items.Clear();

            // Loop through each entry in the Programs Dictionary:
            foreach (var k in Settings.Instance.Programs.Keys)
            {
                // Add a new contex menu:
                var tsi = cms_gifRightClick.Items.Add(k);

                // Set the path to the application:
                tsi.Tag = Settings.Instance.Programs[k];
            }

            // Add default context menu buttons:
            cms_gifRightClick.Items.Add("Open With...");
            cms_gifRightClick.Items.Add("-");

            // Quality settings:
            var col = ((ToolStripMenuItem)cms_gifRightClick.Items.Add("Quality")).DropDownItems;

            _lowQualityMenuItem = (ToolStripMenuItem)col.Add("Low");
            _mediumQualityMenuItem = (ToolStripMenuItem)col.Add("Medium");
            _highQualityMenuItem = (ToolStripMenuItem)col.Add("High");

            _lowQualityMenuItem.Click += Quality_Change;
            _mediumQualityMenuItem.Click += Quality_Change;
            _highQualityMenuItem.Click += Quality_Change;

            switch (pb_gif.Quality)
            {
                case 1:
                    _lowQualityMenuItem.Checked = true;
                    break;
                case 2:
                    _mediumQualityMenuItem.Checked = true;
                    break;
                case 3:
                    _highQualityMenuItem.Checked = true;
                    break;
            }

            // Frame Extraction:
            FrameExtract = cms_gifRightClick.Items.Add("Extract Frames...");

            FrameExtract.Click += FrameExtract_Click;
        }

        /// <summary>
        /// Save the settings to an .ini file located at the program's main path
        /// </summary>
        public void SaveSettings()
        {
            // Try to open the settings file:
            var fileName = Application.StartupPath + "\\settings.ini";

            if (File.Exists(fileName))
                File.Delete(fileName);

            Settings.Instance.SaveSettings();
        }

        /// <summary>
        /// Updates the form's title text
        /// </summary>
        void UpdateTitle()
        {
            Text = @"GIF Viewer " + (Images.Count > 1 ? "[" + (CurrentImage + 1) + "/" + Images.Count + "]" : "") +
                   @" [" + _currentGif.GifPath + @"] " + _currentGif.Width + @"x" + _currentGif.Height;
        }

        /// <summary>
        /// Updates this form's current frame text
        /// </summary>
        void UpdateFrameText()
        {
            ChangeFrameLabelText("Frame: " + (_currentGif.CurrentFrame + 1) + "/" + _currentGif.FrameCount);
        }

        /// <summary>
        /// Event fired everytime the user presses the FrameExtract context menu option
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void FrameExtract_Click(object sender, EventArgs e)
        {
            // No animation? Skip:
            if (_currentGif.CurrentFrameBitmap == null)
                return;

            // Skip if animation has only one frame:
            if (_currentGif.FrameCount <= 1)
            {
                MessageBox.Show(this, @"Animation must have atleast 2 frames to export.", @"Info", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Stop the animation so it can be loaded into the frame xtractor:
            if (PlayBtn.Text == @"&Stop")
                PlayBtn_Click(this, null);

            // Temp path storage:
            string path = _currentGif.GifPath;

            // Clear the current gif:
            UnloadGif();

            // Create the form and assign the Gif path:
            var f = new FrameExtract(_currentGif);
            f.Init();
            f.ShowDialog(this);

            // Restore last GIF:
            LoadGifAsync(path, false);
        }

        /// <summary>
        /// Happens when the user clicks one of the quality selectors on the animation's context menu
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void Quality_Change(object sender, EventArgs e)
        {
            _lowQualityMenuItem.Checked = false;
            _mediumQualityMenuItem.Checked = false;
            _highQualityMenuItem.Checked = false;

            if (sender == _lowQualityMenuItem)
            {
                pb_gif.Quality = 1;

                _lowQualityMenuItem.Checked = true;
            }
            else if (sender == _mediumQualityMenuItem)
            {
                pb_gif.Quality = 2;

                _mediumQualityMenuItem.Checked = true;
            }
            else if (sender == _highQualityMenuItem)
            {
                pb_gif.Quality = 3;

                _highQualityMenuItem.Checked = true;
            }

            pb_gif.Invalidate();
        }

        /// <summary>
        /// Event fired everytime the user presses a keyboard key while the window is focused
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            // Seek next GIF with the Page Down or Right key
            if ((e.KeyData == Keys.PageDown || e.KeyData == Keys.Right) && _currentGif.GifPath != "" && Images.Count > 1 && ActiveControl == pb_gif)
            {
                CurrentImage = (CurrentImage + 1) % (Images.Count);

                LoadGifAsync(Images[CurrentImage], false);

                e.Handled = true;
                e.SuppressKeyPress = true;
            }

            // Seek previous GIF with the Page Up or Left key
            if ((e.KeyData == Keys.PageUp || e.KeyData == Keys.Left) && _currentGif.GifPath != "" && Images.Count > 1 && ActiveControl == pb_gif)
            {
                CurrentImage = (CurrentImage - 1) < 0 ? CurrentImage = Images.Count - 1 : CurrentImage - 1;

                LoadGifAsync(Images[CurrentImage], false);

                e.Handled = true;
                e.SuppressKeyPress = true;
            }

            // Quit
            if (e.KeyData == Keys.Escape)
            {
                Close();
            }
        }

        /// <summary>
        /// Event fired whenever the user enters the form with the mouse dragging a file
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void FormMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// Event fired whenever the user enters the form with the mouse dragging a file
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void FormMain_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // If the user is holding SHIFT, add the dragged files to the playlist
            if ((e.KeyState & 4) == 4)
            {
                CurrentImage = (Images.Count + 1);

                Images.AddRange(files);
                LoadGifAsync(files[0], true);

                UpdateTitle();
            }
            else
            {
                CurrentImage = 0;

                Images.Clear();
                Images.AddRange(files);
                LoadGifAsync(files[0], true);

                UpdateTitle();
            }
        }

        /// <summary>
        /// Event fired everytime the user selects a frame on the timeline
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="newFrame">The new frame selected on the timeline control</param>
        private void tlc_timeline_FrameChanged(object sender, FrameChangedEventArgs newFrame)
        {
            _currentGif.CurrentFrame = newFrame.NewFrame - 1;
            UpdateFrameText();

            AnimationTimer.Stop();
            AnimationTimer.Interval = 15;
            AnimationTimer.Start();
        }

        /// <summary>
        /// Event fired everytime the user clicks the Play button
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void PlayBtn_Click(object sender, EventArgs e)
        {
            // Skip play toggling if the gif has only 1 frame
            if (_currentGif.FrameCount == 1)
                return;

            // Response for when the button is set to Stop the gif file
            if (PlayBtn.Text == @"&Stop")
            {
                // Stop the animation timer if it's running
                AnimationTimer.Stop();
                _currentGif.Playing = false;

                PlayBtn.Text = @"&Play";
            }
            // Response for when the button is set to Play the gif file
            else if(PlayBtn.Text == @"&Play")
            {
                // Restart the animation timer
                AnimationTimer.Start();
                _currentGif.Playing = true;

                PlayBtn.Text = @"&Stop";
            }
            // Response for when the button is set to open a .gif file
            else if (PlayBtn.Text == @"&Open...")
            {
                if (_op.ShowDialog() != DialogResult.OK)
                {
                    // Set the play button as an open file button:
                    PlayBtn.Text = @"&Open...";
                    PlayBtn.Enabled = true;
                    return;
                }

                try
                {
                    LoadGifAsync(_op.FileName, false);
                }
                catch (Exception ex)
                {
                    ErrorBox.Show("Error: " + ex.Message, "Error", ex.StackTrace);

                    // Set the play button as an open file button:
                    PlayBtn.Text = @"&Open...";
                    PlayBtn.Enabled = true;

                    return;
                }

                try
                {
                    LoadGifsInFolder(Path.GetDirectoryName(_op.FileName));
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Event fired everytime the user right clicks the gif area
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void pb_gif_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
                cms_gifRightClick.Show(MousePosition);

            // Throw the focus on the panel to allow for left-right keyboard shortcuts
            pb_gif.Focus();
        }
        /// <summary>
        /// Event fired everytime the context menu is opening
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void cms_gifRightClick_Opening(object sender, CancelEventArgs e)
        {
            // If no GIf is specified:
            if (_currentGif.CurrentFrameBitmap == null)
            {
                // Cancel the action:
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Event fired everytime the user changes the value of the 'Use Min ms' numeric up and down control
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void nud_minFrameInterval_ValueChanged(object sender, EventArgs e)
        {
            MinFrameInterval = (int)nud_minFrameInterval.Value;
        }

        /// <summary>
        /// Event fired everytime the user releases a keyboard key on the 'Use Min ms' numeric up and down control
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void nud_minFrameInterval_KeyUp(object sender, KeyEventArgs e)
        {
            // If you modify the variable once, it calls the ValueChanged method.
            nud_minFrameInterval.Value = nud_minFrameInterval.Value;
        }

        /// <summary>
        /// Event fired everytime the 'Use Min ms' checkbox is clicked
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void cb_useMinFrameInterval_CheckedChanged(object sender, EventArgs e)
        {
            UseMinFrameInterval = cb_useMinFrameInterval.Checked;

            nud_minFrameInterval.Enabled = cb_useMinFrameInterval.Checked;
        }

        /// <summary>
        /// Function called whenever the user clicks an item on the right-click context menu
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void cms_gifRightClick_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // Separator, skip:
            if (e.ClickedItem.Text == "")
                return;

            // Quality settings:
            if (cms_gifRightClick.Items.IndexOf(e.ClickedItem) > cms_gifRightClick.Items.Count - 3)
                return;

            cms_gifRightClick.Close();

            // Open the .gif file with another application
            if (e.ClickedItem.Text == @"Open With..." && e.ClickedItem.Tag == null)
            {
                OpenFileDialog ofd = new OpenFileDialog { Filter = @"Executable files|*.exe" };

                if (ofd.ShowDialog() != DialogResult.OK || !ofd.FileName.EndsWith("exe"))
                    return;

                var it = cms_gifRightClick.Items.Add(Path.GetFileNameWithoutExtension(ofd.FileName));
                it.Tag = ofd.FileName;

                cms_gifRightClick.Items.Remove(it);
                cms_gifRightClick.Items.Insert(cms_gifRightClick.Items.Count - 1, it);

                Settings.Instance.Programs.Add(Path.GetFileNameWithoutExtension(ofd.FileName), ofd.FileName);

                SaveSettings();

                Process.Start(ofd.FileName, "\"" + _currentGif.GifPath + "\"");
                Close();

                return;
            }

            // If the user clicks an application but it's not available, ask whether he wants to remove it from the list of programs
            if (e.ClickedItem.Tag != null && !File.Exists(e.ClickedItem.Tag.ToString()))
            {
                if (MessageBox.Show(@"Program not found. Do you want to remove it from the list?", @"File not found", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Settings.Instance.Programs.Remove(e.ClickedItem.Text);
                    SaveSettings();
                    LoadSettings();
                }

                return;
            }

            // Fire the program with the .gif file as an argument
            if (e.ClickedItem.Tag != null)
            {
                Process.Start(e.ClickedItem.Tag.ToString(), "\"" + _currentGif.GifPath + "\"");
                // Close this application
                Close();
            }
        }

        /// <summary>
        /// Event fired everytime the user clicks the Settings button
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void btn_configuration_Click(object sender, EventArgs e)
        {
            var settings = new SettingsForm();

            if (settings.ShowDialog(this) == DialogResult.OK)
            {
                _currentGif?.ApplyUserSelectedMemorySettings();
            }
        }

        /// <summary>
        /// Changes the current frame on the timeline to the specified ammount
        /// </summary>
        /// <param name="newValue">The new ammount to set</param>
        public void ChangeTimelineFrame(int newValue)
        {
            tlc_timeline.CurrentFrame = newValue + 1;
        }

        /// <summary>
        /// Changes the lblFrame text
        /// </summary>
        /// <param name="newString">The new label to change the lblFrame to</param>
        public void ChangeFrameLabelText(string newString)
        {
            lblFrame.Text = newString;
        }

        /// <summary>
        /// Traces a list of objects on the debug panel
        /// </summary>
        /// <param name="obj">The list of objects to trace</param>
        public static void Trace(params object[] obj)
        {
            foreach (object o in obj)
            {
                Debug.Write(o + " ");
            }
            Debug.WriteLine("");
        }

        /// <summary>
        /// Whether to query the user for a .gif file on startup
        /// </summary>
        private bool _openFile;
    }

    public partial class FormMain
    {
        /// <summary>
        /// Loads a GIF to show from a file
        /// </summary>
        /// <param name="fileName">The .GIF filepath</param>
        /// <param name="resizeAndRelocate">Whether to resize and relocate the main form window</param>
        async Task LoadGif(string fileName, bool resizeAndRelocate = true)
        {
            // Stops the animation timer, if it's running
            AnimationTimer.Stop();

            // Clear everything if the user provides an empty string:
            if (fileName == "")
            {
                UnloadGif();

                return;
            }

            if (!File.Exists(fileName))
            {
                MessageBox.Show(@"The GIF file trying to be opened '" + fileName + @"' could not be found!");
                return;
            }

            try
            {
                // Check the magic number
                var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                if (fs.ReadByte() != 0x47 || fs.ReadByte() != 0x49 || fs.ReadByte() != 0x46)
                {
                    fileFormatWarningImage.Visible = true;
                }
                else
                {
                    fileFormatWarningImage.Visible = false;
                }

                fs.Close();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(@"There was an unauthorize exception while opening the file!\nCheck if the program has privileges to acces the file and try again.");
                return;
            }

            // Show the loading label:
            lblLoading.Visible = true;

            Update();

            // Load the GIF file:
            await _currentGif.LoadFromPath(fileName).ContinueWith(result =>
            {
                if (IsDisposed || !Visible)
                    return;

                if (!_currentGif.Loaded)
                {
                    PlayBtn.Text = @"&Open...";
                    PlayBtn.Enabled = true;

                    MessageBox.Show(@"There was an unkown error loading the selected GIF file! ]:", @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    lblLoading.Visible = false;
                    return;
                }

                // Set the caption
                UpdateTitle();

                // Set up the seek trackBar
                tlc_timeline.Maximum = _currentGif.GetFrameCount();
                tlc_timeline.CurrentFrame = 1;

                // Disable the play button, frame extract context menu item and trackbar when there is only 1 frame available
                PlayBtn.Enabled = FrameExtract.Enabled = tlc_timeline.Enabled = (_currentGif.FrameCount > 1);

                // Setup the frame label
                lblFrame.Text = @"Frame: " + (_currentGif.CurrentFrame + 1) + @"/" + _currentGif.FrameCount;

                // Refresh the pictureBox with the new animation
                pb_gif.BackgroundImage = _currentGif.CurrentFrameBitmap;

                // Change the window size and location only if windowed
                if (WindowState == FormWindowState.Normal && resizeAndRelocate)
                {
                    // Set the client size
                    ClientSize = new Size(Math.Max(_currentGif.Width, MinimumSize.Width), _currentGif.Height + panel1.Height + 3);

                    // Re-position the client to the center of the current screen so the content is always on the middle of the screen
                    CenterToScreen();
                }

                // Hide the loading label:
                lblLoading.Visible = false;

                // Reset the last frame reference:
                _lastFrame = 0;

                // Start the animation timer
                if (_currentGif.FrameCount > 1)
                {
                    _currentGif.Playing = true;
                    AnimationTimer.Interval = UseMinFrameInterval ? Math.Max(_currentGif.GetIntervalForCurrentFrame(), MinFrameInterval) : _currentGif.GetIntervalForCurrentFrame();
                    AnimationTimer.Start();
                }

                // Set the play button:
                PlayBtn.Text = @"&Stop";

                // Focus the gif:
                pb_gif.Select();
                pb_gif.Focus();
                ActiveControl = pb_gif;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}