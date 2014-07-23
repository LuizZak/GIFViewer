using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GIF_Viewer;

namespace GIF_Viewer.Utils
{
    /// <summary>
    /// Represents a collection of all the settings of the program
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The settings object
        /// </summary>
        private static Settings settings;

        /// <summary>
        /// Gets an instance of the Settings class
        /// </summary>
        public static Settings Instance
        {
            get
            {
                if(settings == null)
                    settings = new Settings();

                return settings;
            }
        }

        /// <summary>
        /// The maximum allowed memory for buffers
        /// </summary>
        private int maxBufferMemory;

        /// <summary>
        /// The maximum allowed memory for keyframes
        /// </summary>
        private int maxKeyframeMemory;

        /// <summary>
        /// The maximum number of frames allowed to track back until a keyframe before the operation is ignored and the current frame is drawn incomplete
        /// </summary>
        private int maxKeyframeReach;

        /// <summary>
        /// Programs to display on the program's context menu
        /// </summary>
        private Dictionary<string, string> programs;

        /// <summary>
        /// The settings file handler
        /// </summary>
        private Stream IniFile;

        /// <summary>
        /// The maximum allowed memory for buffers
        /// </summary>
        public int MaxBufferMemory { get { return this.maxBufferMemory; } set { this.maxBufferMemory = Math.Min(512, Math.Max(5, value)); } }

        /// <summary>
        /// The maximum allowed memory for keyframes
        /// </summary>
        public int MaxKeyframeMemory { get { return this.maxKeyframeMemory; } set { this.maxKeyframeMemory = Math.Min(512, Math.Max(5, value)); } }

        /// <summary>
        /// The maximum number of frames allowed to track back until a keyframe before the operation is ignored and the current frame is drawn incomplete
        /// </summary>
        public int MaxKeyframeReach { get { return this.maxKeyframeReach; } set { this.maxKeyframeReach = Math.Min(100, Math.Max(1, value)); } }

        /// <summary>
        /// Whether to ignore .gif file association checks when the program starts
        /// </summary>
        public bool DontAskAssociate { get; set; }

        /// <summary>
        /// Gets the programs to display on the program's context menu
        /// </summary>
        public Dictionary<string, string> Programs { get { return this.programs; } set { this.programs = value; } }

        /// <summary>
        /// Private constructor
        /// </summary>
        private Settings()
        {
            maxBufferMemory = 50;
            maxKeyframeMemory = 30;
            maxKeyframeReach = 10;
            DontAskAssociate = false;
            programs = new Dictionary<string, string>();

            LoadSettings();
        }

        /// <summary>
        /// Loads the settings
        /// </summary>
        public void LoadSettings()
        {
            // Close the current .ini file
            if (IniFile != null)
            {
                IniFile.Close();
            }

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
                {
                    DontAskAssociate = true;
                }
                // Memory settings
                else if ((line == "[MaxBufferMemory]" || line == "[MaxKeyframeMemory]" || line == "[MaxKeyframeReach]") && !reader.EndOfStream)
                {
                    int value = 0;
                    if (int.TryParse(reader.ReadLine(), out value))
                    {
                        switch (line)
                        {
                            case "[MaxBufferMemory]":
                                this.MaxBufferMemory = value;
                                break;
                            case "[MaxKeyframeMemory]":
                                this.MaxKeyframeMemory = value;
                                break;
                            case "[MaxKeyframeReach]":
                                this.MaxKeyframeReach = value;
                                break;
                        }
                    }
                }
                // A program:
                else if (line.Contains("="))
                {
                    programName = line.Split('=')[0].Substring(1, line.IndexOf("=") - 2); // Reads off the screen name
                    programPath = line.Split('=')[1].Substring(1, line.Split('=')[1].LastIndexOf('"') - 1); // Reads off the program's path

                    Programs.Add(programName, programPath); // Add this program to thye program's list
                }
            }
        }

        /// <summary>
        /// Saves the settings
        /// </summary>
        public void SaveSettings()
        {
            // Try to open the settings file:
            string fileName = Application.StartupPath + "\\settings.ini";

            if (File.Exists(fileName))
            {
                IniFile.Close();
                File.Delete(fileName);
            }

            // If no settings file exists, create one!
            IniFile = File.Create(fileName);

            // Opens a stream to write the settings down:
            StreamWriter writer = new StreamWriter(IniFile);

            // Iterate through the programs, and write down one at a time
            foreach (string k in Programs.Keys)
            {
                writer.WriteLine("\"" + k + "\"=\"" + Programs[k] + "\"");
            }

            // If the program should ask the user about the .gif file association on startup:
            if (DontAskAssociate)
            {
                writer.WriteLine("[DontAskAssociate]");
            }

            // Write the memory settings
            writer.WriteLine("[MaxBufferMemory]");
            writer.WriteLine(MaxBufferMemory);
            writer.WriteLine("[MaxKeyframeMemory]");
            writer.WriteLine(MaxKeyframeMemory);
            writer.WriteLine("[MaxKeyframeReach]");
            writer.WriteLine(MaxKeyframeReach);

            // Close the stream:
            writer.Close();
        }
    }
}