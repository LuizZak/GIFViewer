using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

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
        private static Settings _settings;

        /// <summary>
        /// Gets an instance of the Settings class
        /// </summary>
        public static Settings Instance
        {
            get { return _settings ?? (_settings = new Settings()); }
        }

        /// <summary>
        /// The maximum allowed memory for buffers
        /// </summary>
        private int _maxBufferMemory;

        /// <summary>
        /// The maximum allowed memory for keyframes
        /// </summary>
        private int _maxKeyframeMemory;

        /// <summary>
        /// The maximum number of frames allowed to track back until a keyframe before the operation is ignored and the current frame is drawn incomplete
        /// </summary>
        private int _maxKeyframeReach;

        /// <summary>
        /// The settings file handler
        /// </summary>
        private Stream _iniFile;

        /// <summary>
        /// The maximum allowed memory for buffers
        /// </summary>
        public int MaxBufferMemory { get { return _maxBufferMemory; } set { _maxBufferMemory = Math.Min(512, Math.Max(5, value)); } }

        /// <summary>
        /// The maximum allowed memory for keyframes
        /// </summary>
        public int MaxKeyframeMemory { get { return _maxKeyframeMemory; } set { _maxKeyframeMemory = Math.Min(512, Math.Max(5, value)); } }

        /// <summary>
        /// The maximum number of frames allowed to track back until a keyframe before the operation is ignored and the current frame is drawn incomplete
        /// </summary>
        public int MaxKeyframeReach { get { return _maxKeyframeReach; } set { _maxKeyframeReach = Math.Min(100, Math.Max(1, value)); } }

        /// <summary>
        /// Whether to ignore .gif file association checks when the program starts
        /// </summary>
        public bool DontAskAssociate { get; set; }

        /// <summary>
        /// Gets the programs to display on the program's context menu
        /// </summary>
        public Dictionary<string, string> Programs { get; set; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private Settings()
        {
            _maxBufferMemory = 50;
            _maxKeyframeMemory = 30;
            _maxKeyframeReach = 10;
            DontAskAssociate = false;
            Programs = new Dictionary<string, string>();

            LoadSettings();
        }

        /// <summary>
        /// Loads the settings
        /// </summary>
        public void LoadSettings()
        {
            // Close the current .ini file
            if (_iniFile != null)
            {
                _iniFile.Close();
            }

            // Set the filename:
            string fileName = Application.StartupPath + "\\settings.ini";

            // Clear the Programs Dictionary:
            Programs = new Dictionary<string, string>();

            // No settings file, leave it blank:
            _iniFile = !File.Exists(fileName) ? File.Create(fileName) : File.Open(fileName, FileMode.Open);

            StreamReader reader = new StreamReader(_iniFile);

            // Temp definitions:
            string line;
            string programName;
            string programPath;
            
            // Main loop:
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine(); // Get a line

                if (line == null)
                    break;

                // If the program should ask the user about the .gif file association on startup:
                if (line == "[DontAskAssociate]")
                {
                    DontAskAssociate = true;
                }
                // Memory settings
                else if ((line == "[MaxBufferMemory]" || line == "[MaxKeyframeMemory]" || line == "[MaxKeyframeReach]") && !reader.EndOfStream)
                {
                    int value;
                    if (int.TryParse(reader.ReadLine(), out value))
                    {
                        switch (line)
                        {
                            case "[MaxBufferMemory]":
                                MaxBufferMemory = value;
                                break;
                            case "[MaxKeyframeMemory]":
                                MaxKeyframeMemory = value;
                                break;
                            case "[MaxKeyframeReach]":
                                MaxKeyframeReach = value;
                                break;
                        }
                    }
                }
                // A program:
                else if (line.Contains("="))
                {
                    programName = line.Split('=')[0].Substring(1, line.IndexOf("=", StringComparison.Ordinal) - 2); // Reads off the screen name
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
                _iniFile.Close();
                File.Delete(fileName);
            }

            // If no settings file exists, create one!
            _iniFile = File.Create(fileName);

            // Opens a stream to write the settings down:
            StreamWriter writer = new StreamWriter(_iniFile);

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