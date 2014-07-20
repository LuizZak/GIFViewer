using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

using GIF_Viewer.Views;

/// @title              GIF Viewer
/// @description        Helps visualizing animated and non-animated .GIF files
/// 
/// @author             Luiz Fernando
/// 
/// @version 1.6.0b     Improved .gif speed when seeking frames at any point in the timeline. Utilizing code from the GifComponents (that was in turn based on NGif) open source project.
/// @                   Fixed crash related to .gif files with very fast frame intervals.
/// @                   Swapped the old timeline trackbar with a new more responsive timeline control.
/// @                   Now the value stepper for the minimum frame interval is disabled when the checkbox is also off.
/// @                   Now you can seek the .gif timeline using the arrow keys when the animation panel is focused (clicked on).
/// @                   Fixed half-pixel misalignment on images (only visible when zoomed in).
/// @                   Fixed opening and closing the frame extract form doubling the animation's playback speed on the main form.
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
        GIFFile CurrentGif;

        /// <summary>
        /// Timer used to animate the frames
        /// </summary>
        public Timer AnimationTimer;

        /// <summary>
        /// Current image on the images list
        /// </summary>
        public int currentImage = 0;

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
        public ToolStripMenuItem low;
        /// <summary>
        /// Menu item for the high rendering setting
        /// </summary>
        public ToolStripMenuItem hig;

        /// <summary>
        /// The 'Frame Extract' menu item in the right-click context menu
        /// </summary>
        public ToolStripItem FrameExtract;

        /// <summary>
        /// Programs on the context menu
        /// </summary>
        public Dictionary<string, string> Programs;

        /// <summary>
        /// Generic event handler
        /// </summary>
        public EventHandler hand;

        /// <summary>
        /// Helper object used to check the file association for the .gif extension on Windows
        /// </summary>
        public FileAssociation Association;
        /// <summary>
        /// Whether to ask on startup to change .gif file association to this program
        /// </summary>
        public bool AskOnStartup = true;

        /// <summary>
        /// Windows' Open File Dialog:
        /// </summary>
        OpenFileDialog op = new OpenFileDialog();

        /// <summary>
        /// Main entry point for this Form
        /// </summary>
        /// <param name="args"></param>
        public FormMain(string[] args)
        {
            InitializeComponent();

            CurrentGif = new GIFFile();

            Images = new List<string>();

            nud_minFrameInterval.Value = MinFrameInterval;
            cb_useMinFrameInterval.Checked = UseMinFrameInterval;

            PlayBtn.Enabled = false;
            tlc_timeline.Enabled = false;

            // Create the timer
            AnimationTimer = new Timer();
            AnimationTimer.Tick += new EventHandler(AnimationTimer_Tick);

            try
            {
                // Load the settings:
                LoadSettings();

                // Read off the file association:
                Association = new FileAssociation(".gif");

                // Check .GIF file association
                if (AskOnStartup)
                {
                    if (Association.GetProgram() != "\"" + Application.ExecutablePath + "\" \"%1\"")
                    {
                        DialogResult res = MessageBox.Show(this, "Do you want to associate the .gif files to GIF Viewer? (Hit Cancel to never ask again)", "Question", MessageBoxButtons.YesNoCancel);

                        // Change association
                        if (res == DialogResult.Yes)
                        {
                            try
                            {
                                Association.SetProgram("\"" + Application.ExecutablePath + "\" \"%1\"", Path.GetFileName(Application.ExecutablePath));
                            }
                            catch (Exception e)
                            {
                                ErrorBox.Show("Error associating program: " + e.Message, "Error", e.StackTrace);
                            }
                        }
                        else if (res == System.Windows.Forms.DialogResult.Cancel)
                        {
                            AskOnStartup = false;

                            SaveSettings();
                        }
                    }
                }
            }
            catch (Exception) { }

            // Load a gif file from the command line arguments
            if (args.Length > 0 && File.Exists(args[0]))
            {
                try
                {
                    LoadGIF(args[0]);
                }
                catch (Exception ex)
                {
                    ErrorBox.Show("Error: " + ex.Message, "Error", ex.StackTrace);
                }

                try
                {
                    LoadGIFSInFolder(Path.GetDirectoryName(args[0]));
                }
                catch (Exception) { }
            }
            else
            {
                if (CurrentGif.GIFPath == "")
                {
                    openFile = true;
                }
            }

            // Register the drag 'n drop handlers
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(FormMain_DragEnter);
            this.DragDrop += new DragEventHandler(FormMain_DragDrop);

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// Event fired when the window is shown for the first time
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        private void Form1_Shown(object sender, EventArgs e)
        {
            if (!openFile)
                return;

            // Asks the user for a .gif file using the OpenFileDialog object
            op.Filter = "GIF files (*.gif)|*.gif";

            // The user has chosen a valid .gif file
            if (op.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LoadGIF(op.FileName);
                }
                catch (Exception ex)
                {
                    ErrorBox.Show("Error: " + ex.Message, "Error", ex.StackTrace);

                    // Set the play button as an open file button:
                    PlayBtn.Text = "&Open...";
                    PlayBtn.Enabled = true;

                    return;
                }

                try
                {
                    LoadGIFSInFolder(Path.GetDirectoryName(op.FileName));
                }
                catch (Exception) { }
            }
            // The user has closed the dialog
            else
            {
                // Set the play button as an open file button:
                PlayBtn.Text = "&Open...";
                PlayBtn.Enabled = true;
            }
        }

        /// <summary>
        /// Loads a GIF to show from a file
        /// </summary>
        /// <param name="fileName">The .GIF filepath</param>
        void LoadGIF(string fileName, bool resizeAndRelocate = true)
        {
            // Stops the animation timer, if it's running
            AnimationTimer.Stop();

            // Clear everything if the user provides an empty string:
            if (fileName == "")
            {
                // Dispose and nullify everything:
                CurrentGif.Dispose();

                pb_gif.BackgroundImage = null;

                // Update the form:
                Update();

                return;
            }

            if (!File.Exists(fileName))
            {
                MessageBox.Show("The GIF file trying to be opened '" + fileName + "' could not be found!");
                return;
            }

            try
            {
                // Check the magic number
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

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
                MessageBox.Show("There was an unauthorize exception while opening the file!\nCheck if the program has privileges to acces the file and try again.");
                return;
            }

            // Show the loading label:
            lblLoading.Visible = true;

            Update();

            // Load the GIF file:
            CurrentGif.LoadFromPath(fileName);

            if (!CurrentGif.Loaded)
            {
                PlayBtn.Text = "&Open...";
                PlayBtn.Enabled = true;

                MessageBox.Show("There was an unkown error loading the selected GIF file! ]:", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                lblLoading.Visible = false;
                return;
            }

            // Set the caption
            UpdateTitle();

            // Set up the seek trackBar
            tlc_timeline.Maximum = CurrentGif.GetFrameCount();
            tlc_timeline.CurrentFrame = 1;

            // Disable the play button, frame extract context menu item and trackbar when there is only 1 frame available
            PlayBtn.Enabled = FrameExtract.Enabled = tlc_timeline.Enabled = (CurrentGif.FrameCount > 1);

            // Setup the frame label
            lblFrame.Text = "Frame: " + (CurrentGif.CurrentFrame + 1) + "/" + CurrentGif.FrameCount;

            // Refresh the pictureBox with the new animation
            pb_gif.BackgroundImage = CurrentGif.GIF;

            // Change the window size and location only if windowed
            if (WindowState == FormWindowState.Normal && resizeAndRelocate)
            {
                // Set the client size
                this.ClientSize = new Size(Math.Max(CurrentGif.Width, this.MinimumSize.Width), CurrentGif.Height + panel1.Height + 3);

                // Re-position the client to the center of the current screen so the content is always on the middle of the screen
                CenterToScreen();
            }

            // Hide the loading label:
            lblLoading.Visible = false;

            // Reset the last frame reference:
            this.lastFrame = 0;

            // Start the animation timer
            if (CurrentGif.FrameCount > 1)
            {
                CurrentGif.Playing = true;
                AnimationTimer.Interval = UseMinFrameInterval ? Math.Max(CurrentGif.GetIntervalForCurrentFrame(), MinFrameInterval) : CurrentGif.GetIntervalForCurrentFrame();
                AnimationTimer.Start();
            }

            // Set the play button:
            PlayBtn.Text = "&Stop";
        }

        /// <summary>
        /// Loads all the .GIF images from a folder into the Images list
        /// </summary>
        /// <param name="folder">the folder to search the GIFs on</param>
        void LoadGIFSInFolder(string folder)
        {
            // Load the .GIF images
            Images.Clear();
            Images.AddRange(Directory.GetFiles(folder, "*.gif"));
            
            // Locate the current .GIF image on the images list:
            currentImage = Images.IndexOf(CurrentGif.GIFPath);

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

            if (CurrentGif.Playing)
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
        /// Animates the current GIF
        /// </summary>
        void Animate()
        {
            try
            {
                int newFrame = CurrentGif.CurrentFrame;

                if (!tlc_timeline.DraggingFrame && CurrentGif.Playing)
                {
                    // Change the current frame
                    newFrame = (CurrentGif.CurrentFrame + 1) % CurrentGif.FrameCount;
                    changeTimelineFrame(CurrentGif.CurrentFrame);
                    UpdateFrameText();
                }

                if (newFrame != lastFrame)
                {
                    lastFrame = newFrame;
                    // Redraw the GIF panel
                    pb_gif._Paint = false;

                    CurrentGif.SetCurrentFrame(newFrame);

                    pb_gif._Paint = true;

                    pb_gif.Invalidate();
                }

                if (CurrentGif.CurrentFrame == (CurrentGif.FrameCount) && CurrentGif.CanLoop == false)
                {
                    CurrentGif.Playing = false;
                }
            }
            catch (Exception ex)
            {
                trace(ex);
            }
        }

        /// <summary>
        /// Load the settings from an .ini file located at the program's main path
        /// </summary>
        public void LoadSettings()
        {
            // Set the filename:
            string fileName = Application.StartupPath + "\\settings.ini";

            // Clear the Programs Dictionary:
            Programs = new Dictionary<string, string>();

            // No settings file, leave it blank:
            if (!File.Exists(fileName))
            {
                IniFile = File.Create(fileName);
            }
            else
            {
                // Read the settings:
                IniFile = File.Open(fileName, FileMode.Open);
            }
            StreamReader reader = new StreamReader(IniFile);

            // Temp definitions:
            string line;
            string programName;
            string programPath;
            
            // Main loop:
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine(); // Get a line

                // If the program should ask the user about the .gif file association on startup:
                if (line == "[DontAskAssociate]")
                    AskOnStartup = false;
                // A program:
                else if (line.Contains("="))
                {
                    programName = line.Split('=')[0].Substring(1, line.IndexOf("=") - 2); // Reads off the screen name
                    programPath = line.Split('=')[1].Substring(1, line.Split('=')[1].LastIndexOf('"') - 1); // Reads off the program's path

                    Programs.Add(programName, programPath); // Add this program to thye program's list
                }
            }

            // Close the reader:
            reader.Close();

            // Fill in the contextMenu with the programs:
            cms_gifRightClick.Items.Clear();

            // Loop through each entry in the Programs Dictionary:
            foreach (string k in Programs.Keys)
            {
                // Add a new contex menu:
                ToolStripItem tsi = cms_gifRightClick.Items.Add(k);

                // Set the path to the application:
                tsi.Tag = Programs[k];
            }

            // Add default context menu buttons:
            cms_gifRightClick.Items.Add("Open With...");
            cms_gifRightClick.Items.Add("-");

            // Quality settings:
            ToolStripItemCollection col = ((ToolStripMenuItem)cms_gifRightClick.Items.Add("Quality")).DropDownItems;

            low = col.Add("Low") as ToolStripMenuItem;
            hig = col.Add("High") as ToolStripMenuItem;

            low.Click += new EventHandler(Quality_Change);
            hig.Click += new EventHandler(Quality_Change);

            if (pb_gif.Quality == 1)
                low.Checked = true;
            else if (pb_gif.Quality == 3)
                hig.Checked = true;

            // Frame Extraction:
            FrameExtract = cms_gifRightClick.Items.Add("Extract Frames...");

            FrameExtract.Click += new EventHandler(FrameExtract_Click);
        }

        /// <summary>
        /// Save the settings to an .ini file located at the program's main path
        /// </summary>
        public void SaveSettings()
        {
            // Try to open the settings file:
            string fileName = Application.StartupPath + "\\settings.ini";

            if (File.Exists(fileName))
                File.Delete(fileName);

            // If no settings file exists, create one!
            IniFile = File.Create(fileName);

            // Opens a stream to write the settings down:
            StreamWriter writter = new StreamWriter(IniFile);

            // Iterate through the programs, and write down one at a time
            foreach (string k in Programs.Keys)
            {
                writter.WriteLine("\"" + k + "\"=\"" + Programs[k] + "\"");
            }

            // If the program should ask the user about the .gif file association on startup:
            if (AskOnStartup == false)
            {
                writter.WriteLine("[DontAskAssociate]");
            }

            // Close the stream:
            writter.Close();
        }

        /// <summary>
        /// Updates the form's title text
        /// </summary>
        void UpdateTitle()
        {
            Text = "GIF Viewer " + (Images.Count > 1 ? "[" + (currentImage + 1) + "/" + Images.Count + "]" : "") + " [" + CurrentGif.GIFPath + "] " + CurrentGif.Width + "x" + CurrentGif.Height;
        }

        /// <summary>
        /// Updates this form's current frame text
        /// </summary>
        void UpdateFrameText()
        {
            changeText("Frame: " + (CurrentGif.CurrentFrame + 1) + "/" + CurrentGif.FrameCount);
        }

        /// <summary>
        /// Event fired everytime the user presses the FrameExtract context menu option
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        void FrameExtract_Click(object sender, EventArgs e)
        {
            // No animation? Skip:
            if (CurrentGif.GIF == null)
                return;

            // Skip if animation has only one frame:
            if (CurrentGif.FrameCount <= 1)
            {
                MessageBox.Show(this, "Animation must have atleast 2 frames to export.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return;
            }

            // Stop the animation so it can be loaded into the frame xtractor:
            if (PlayBtn.Text == "&Stop")
                PlayBtn_Click(this, null);

            // Temp path storage:
            string path = CurrentGif.GIFPath;

            // Clear the current gif:
            LoadGIF("");

            // Create the form and assign the Gif path:
            FrameExtract f = new FrameExtract();
            f.CurrentGif = CurrentGif;
            f.Init();
            f.ShowDialog(this);

            // Restore last GIF:
            LoadGIF(path, false);
        }

        /// <summary>
        /// Happens when the user clicks one of the quality selectors on the animation's context menu
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        void Quality_Change(object sender, EventArgs e)
        {
            if (sender == low)
            {
                pb_gif.Quality = 1;

                low.Checked = true;
                hig.Checked = false;
            }
            else if (sender == hig)
            {
                pb_gif.Quality = 3;

                low.Checked = false;
                hig.Checked = true;
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
            // Seek next GIF with the Page Down key
            if (e.KeyData == Keys.PageDown && CurrentGif.GIFPath != "" && Images.Count > 1)
            {
                currentImage = (currentImage + 1) % (Images.Count);

                LoadGIF(Images[currentImage]);

                e.Handled = true;
            }

            // Seek previous GIF with the Page Up key
            if (e.KeyData == Keys.PageUp && CurrentGif.GIFPath != "" && Images.Count > 1)
            {
                currentImage = (currentImage - 1) < 0 ? currentImage = Images.Count - 1 : currentImage - 1;

                LoadGIF(Images[currentImage]);

                e.Handled = true;
            }

            if(CurrentGif.Loaded && CurrentGif.GetFrameCount() > 0)
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
        void FormMain_DragEnter(object sender, DragEventArgs e)
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
        void FormMain_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // If the user is holding SHIFT, add the dragged files to the playlist
            if ((e.KeyState & 4) == 4)
            {
                currentImage = (Images.Count + 1);

                Images.AddRange(files);
                LoadGIF(files[0]);

                UpdateTitle();
            }
            else
            {
                currentImage = 0;

                Images.Clear();
                Images.AddRange(files);
                LoadGIF(files[0]);

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
            CurrentGif.CurrentFrame = newFrame.NewFrame - 1;
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
            if (CurrentGif.FrameCount == 1)
                return;

            // Response for when the button is set to Stop the gif file
            if (PlayBtn.Text == "&Stop")
            {
                // Stop the animation timer if it's running
                AnimationTimer.Stop();
                CurrentGif.Playing = false;

                PlayBtn.Text = "&Play";
            }
            // Response for when the button is set to Play the gif file
            else if(PlayBtn.Text == "&Play")
            {
                // Restart the animation timer
                AnimationTimer.Start();
                CurrentGif.Playing = true;

                PlayBtn.Text = "&Stop";
            }
            // Response for when the button is set to open a .gif file
            else if (PlayBtn.Text == "&Open...")
            {
                if (op.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        LoadGIF(op.FileName);
                    }
                    catch (Exception ex)
                    {
                        ErrorBox.Show("Error: " + ex.Message, "Error", ex.StackTrace);

                        // Set the play button as an open file button:
                        PlayBtn.Text = "&Open...";
                        PlayBtn.Enabled = true;

                        return;
                    }

                    try
                    {
                        LoadGIFSInFolder(Path.GetDirectoryName(op.FileName));
                    }
                    catch (Exception) { }
                }
                else
                {
                    // Set the play button as an open file button:
                    PlayBtn.Text = "&Open...";
                    PlayBtn.Enabled = true;
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
            if (CurrentGif.GIF == null)
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
        /// Changes the current frame on the timeline to the specified ammount
        /// </summary>
        /// <param name="newValue">The new ammount to set</param>
        public void changeTimelineFrame(int newValue)
        {
            tlc_timeline.CurrentFrame = newValue + 1;
        }

        /// <summary>
        /// Changes the lblFrame text
        /// </summary>
        /// <param name="newString">The new label to change the lblFrame to</param>
        public void changeText(string newString)
        {
            lblFrame.Text = newString;
        }

        /// <summary>
        /// Change the PlayBtn label
        /// </summary>
        /// <param name="newString">The new label to change the PlayBtn to</param>
        public void changeBtnText(string newString)
        {
            PlayBtn.Text = newString;
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
            if (e.ClickedItem.Text == "Open With..." && e.ClickedItem.Tag == null)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Executable files|*.exe";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (ofd.FileName.EndsWith("exe"))
                    {
                        ToolStripItem it = cms_gifRightClick.Items.Add(Path.GetFileNameWithoutExtension(ofd.FileName));
                        it.Tag = ofd.FileName;

                        cms_gifRightClick.Items.Remove(it);
                        cms_gifRightClick.Items.Insert(cms_gifRightClick.Items.Count - 1, it);

                        Programs.Add(Path.GetFileNameWithoutExtension(ofd.FileName), ofd.FileName);

                        SaveSettings();

                        System.Diagnostics.Process.Start(ofd.FileName, "\"" + CurrentGif.GIFPath + "\"");
                        this.Close();
                    }
                }

                return;
            }

            // If the user clicks an application but it's not available, ask whether he wants to remove it from the list of programs
            if (e.ClickedItem.Tag != null && !File.Exists(e.ClickedItem.Tag.ToString()))
            {
                if (MessageBox.Show("Program not found. Do you want to remove it from the list?", "File not found", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Programs.Remove(e.ClickedItem.Text);
                    SaveSettings();
                    LoadSettings();
                }

                return;
            }

            // Fire the program with the .gif file as an argument
            System.Diagnostics.Process.Start(e.ClickedItem.Tag.ToString(), "\"" + CurrentGif.GIFPath + "\"");

            // Close this application
            this.Close();
        }

        /// <summary>
        /// Method called when the Updated finds an update to this application
        /// </summary>
        /// <param name="status">The UpdateStatus containing information about the update</param>
        void Update_UpdateAvaibleEvent(Update.UpdateStatus status)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Traces a list of objects on the debug panel
        /// </summary>
        /// <param name="obj">The list of objects to trace</param>
        public static void trace(params object[] obj)
        {
            foreach (object o in obj)
            {
                System.Diagnostics.Debug.Write(o + " ");
            }
            System.Diagnostics.Debug.WriteLine("");
        }

        /// <summary>
        /// Whether to query the user for a .gif file on startup
        /// </summary>
        bool openFile = false;
    }
}