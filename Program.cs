using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GIF_Viewer.Utils;
using GIF_Viewer.Views;
using Microsoft.VisualBasic.ApplicationServices;

namespace GIF_Viewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] args)
        {
            if (args.Length > 0)
            {
                // Detect '--extract'
                if (args[0] == "--extract")
                {
                    // initialize console handles
                    AttachConsole(-1);

                    var handler = new ExtractCommandLineHandler(args);
                    return handler.Execute();
                }
                if (args[0] == "--help")
                {
                    // initialize console handles
                    AttachConsole(-1);

                    var handler = new ExtractCommandLineHandler(args);
                    handler.WriteHelp();
                    return 0;
                }
            }

            // Setup and run the application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new FormMain(args);

            // Load the program settings
            Settings.Instance.LoadSettings();

            // Create the application base.
            var applicationBase = new WindowsApplicationBase(form, Settings.Instance.SingleInstance);
            applicationBase.StartupNextInstance += (sender, eventArgs) =>
            {
                if (!Settings.Instance.SingleInstance)
                    return;

                eventArgs.BringToForeground = true;
                form.TopMost = true;
                form.ProcessCommandLine(eventArgs.CommandLine.ToArray());
                form.BringToFront();
                form.TopMost = false;
                form.Focus();
            };

            applicationBase.Run(args);

            return 0;
        }

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int procId);
    }

    /// <summary>
    /// Class used to manage singleton instances of the application
    /// </summary>
    public class WindowsApplicationBase : WindowsFormsApplicationBase
    {
        /// <summary>
        /// Initializes a new instance of the WindowsApplicationBase class
        /// </summary>
        /// <param name="form">The target form to open</param>
        /// <param name="singleInstance">Whether the application should be single-instance</param>
        public WindowsApplicationBase(Form form, bool singleInstance)
        {
            IsSingleInstance = singleInstance;
            MainForm = form;
        }
    }

    /// <summary>
    /// For command line instructions to '--extract' frames from a GIF file.
    /// </summary>
    public class ExtractCommandLineHandler
    {
        private readonly string[] _args;

        public ExtractCommandLineHandler(string[] args)
        {
            _args = args;
        }

        public int Execute()
        {
            if (_args[0] != "--extract")
            {
                Console.WriteLine(@"Expected '--extract' command and arguments.");
                return -1;
            }

            // Collect arguments
            var arguments = new Dictionary<string, string>();
            // Collect parameters
            for (int i = 0; i < _args.Length - 1; i++)
            {
                var arg = _args[i];
                if (arg == "--extract")
                {
                    if (i == _args.Length - 1)
                    {
                        Console.WriteLine(@"--extraxct: Expected file path of gif file to extract.");
                        return -1;
                    }

                    arguments["extract"] = _args[i + 1];
                    i++; // Skip argument
                }
                else if (arg == "--directory")
                {
                    if (i == _args.Length - 1)
                    {
                        Console.WriteLine(@"--directory: Expected directory to export frame images to.");
                        return -1;
                    }

                    arguments["directory"] = _args[i + 1];
                    i++; // Skip argument
                }
                else if (arg == "--filename")
                {
                    if (i == _args.Length - 1)
                    {
                        Console.WriteLine(@"--filename: Expected filename.");
                        return -1;
                    }

                    arguments["filename"] = _args[i + 1];
                    i++; // Skip argument
                }
                else if (arg == "--format")
                {
                    if (i == _args.Length - 1)
                    {
                        Console.WriteLine(@"--format: Expected file format.");
                        return -1;
                    }

                    arguments["format"] = _args[i + 1];
                    i++; // Skip argument
                }
                else if (arg == "--startFrame")
                {
                    if (i == _args.Length - 1)
                    {
                        Console.WriteLine(@"--startFrame: Expected starting frame index.");
                        return -1;
                    }

                    arguments["startFrame"] = _args[i + 1];
                    i++; // Skip argument
                }
                else if (arg == "--endFrame")
                {
                    if (i == _args.Length - 1)
                    {
                        Console.WriteLine(@"--endFrame: Expected ending frame index.");
                        return -1;
                    }

                    arguments["endFrame"] = _args[i + 1];
                    i++; // Skip argument
                }
                else if (arg == "--overwrite")
                {
                    arguments["overwrite"] = "overwrite";
                }
            }

            // Verify GIF file path
            if (!arguments.TryGetValue("extract", out string extractPath))
            {
                Console.WriteLine(@"--extract: Expected .gif file path.");
                return -1;
            }

            if (!File.Exists(extractPath))
            {
                Console.WriteLine($@"--extract: File ""{extractPath}"" does not exist.");
                return -1;
            }

            if (!arguments.TryGetValue("directory", out string directory))
            {
                Console.WriteLine(@"Missing --directory format.");
                return -1;
            }

            if (!Directory.Exists(directory))
            {
                Console.WriteLine($@"--directory: Target directory ""{directory}"" does not exists.");
                return -1;
            }

            // Pre-load GIF file
            var gif = new GifFile();
            
            gif.LoadFromPath(extractPath);
            var frameCount = gif.FrameCount;

            gif.Dispose();

            if (!gif.Loaded)
            {
                Console.WriteLine(@"Failed to load gif file: The file doesn't seem to be a valid .gif, or is corrupted.");
                return -1;
            }

            // File and format
            string filename = arguments.ContainsKey("filename") ? arguments["filename"] : "{%name}-{%frame}";
            if (!IsFileNameValid(filename))
            {
                Console.WriteLine(@"--filename: Invalid filename.");
                return -1;
            }

            string formatArg = arguments.ContainsKey("format") ? arguments["format"] : "png";
            var format = GetFormatByString($".{formatArg}");
            if (format == null)
            {
                Console.WriteLine($@"--format: Invalid format {formatArg}.");
                return -1;
            }

            // Start/End frames
            int startFrame = 0;
            int endFrame = frameCount - 1;

            // Now validate args
            if (arguments.TryGetValue("startFrame", out string startFrameArg))
            {
                if (!int.TryParse(startFrameArg, out startFrame))
                {
                    Console.WriteLine($@"--startFrame: Invalid frame index {startFrameArg}");
                    return -1;
                }
            }
            if (arguments.TryGetValue("endFrame", out string endFrameArg))
            {
                if (!int.TryParse(endFrameArg, out endFrame))
                {
                    Console.WriteLine($@"--endFrame: Invalid frame index {endFrameArg}");
                    return -1;
                }
                if (endFrame >= gif.FrameCount)
                {
                    Console.WriteLine($@"--endFrame: Specified frame index {endFrame} is out of bounds of 0-{gif.FrameCount - 1}. Assuming {gif.FrameCount - 1} instead...");
                    endFrame = gif.FrameCount - 1;
                }
            }

            // Overwrite
            var overwrite = arguments.ContainsKey("overwrite");

            Console.WriteLine(@"Extracting...");

            try
            {
                ExtractGifFrames(extractPath, directory, filename, format, startFrame, endFrame, overwrite);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }

            Console.WriteLine(@"Successful.");

            return 0;
        }

        /// <summary>
        /// Writes into the Console the instructions for this command.
        /// </summary>
        public void WriteHelp()
        {
            Console.WriteLine(@"Usage:");
            Console.WriteLine(@"--extract <.gif file path> --directory <directory to export files to> [--filename <filename>] [--format <bmp|gif|png|jpg|tiff|exit|emf|wmf>] [--startFrame <number>] [--endFrame <number>] [--overwrite]");
            Console.WriteLine();
            Console.WriteLine(@"--filename:   Filename for each gif frame. Doesn't require extension from format (it's appended automatically).");
            Console.WriteLine(@"              Has support for the following custom formats:");
            Console.WriteLine(@"              {%name} - Filename from <.gif file path> argument, without extension.");
            Console.WriteLine(@"              {%frame} - Replaced w/ frame index (starts at 1).");
            Console.WriteLine(@"              {%h}, {%m}, {%s} - Replaced w/ current system's hour, minutes and seconds.");
            Console.WriteLine(@"              Sample format string: '--filename ""{%name}-{%frame}""'");
            Console.WriteLine();
            Console.WriteLine(@"              Defaults to ""{%name}-{%frame}"".");
            Console.WriteLine();
            Console.WriteLine(@"--format:     bmp|gif|png|jpg|tiff|exit|emf|wmf.");
            Console.WriteLine(@"              'gif' option is non-animated.");
            Console.WriteLine();
            Console.WriteLine(@"              Defaults to 'png'.");
            Console.WriteLine();
            Console.WriteLine(@"--startFrame: The 0-th indexed frame to start extracting from.");
            Console.WriteLine();
            Console.WriteLine(@"              Defaults to 0.");
            Console.WriteLine();
            Console.WriteLine(@"--endFrame:   The 0-th indexed frame to stop extracting at, inclusively.");
            Console.WriteLine();
            Console.WriteLine(@"              Defaults to the last valid frame index of the input gif.");
            Console.WriteLine();
            Console.WriteLine(@"--overwrite:  If specified, overwites output files on specified directory if they already exist. If no --overwite is provided,");
            Console.WriteLine(@"              aborts on first name clash.");
        }

        /// <summary>
        /// Performs a frame extraction of a given file path, into a target directory, with a specified file name
        /// and file format.
        /// </summary>
        /// <param name="gifPath"></param>
        /// <param name="targetDirectory"></param>
        /// <param name="fileName"></param>
        /// <param name="format"></param>
        /// <param name="startFrame"></param>
        /// <param name="endFrame"></param>
        /// <param name="allowOverwrite"></param>
        public static void ExtractGifFrames(string gifPath, string targetDirectory, string fileName, ImageFormat format, int startFrame, int endFrame, bool allowOverwrite = true)
        {
            // Get the current time to use in the iterator:
            var t = DateTime.Now;

            // Load the GIF file:
            var gif = new GifFile();
            gif.LoadFromPath(gifPath);

            var sourceName = Path.GetFileNameWithoutExtension(gifPath);

            // Get the frame dimension to advance the frames:
            var extension = GetExtensionForFormat(format);

            try
            {
                for (int frame = startFrame; frame < startFrame + 1 + endFrame; frame++)
                {
                    string name = ReplaceFileName(fileName, t, sourceName, frame);

                    // Create the bitmap and the graphics:
                    gif.SetCurrentFrame(frame);
                    var b = gif.CurrentFrameBitmap;
                    
                    var finalPath = targetDirectory + "\\" + name + extension;
                    if (!allowOverwrite && File.Exists(finalPath))
                    {
                        throw new Exception($"File already exists at \"{finalPath}\"");
                    }

                    // Save the image down to the path with the given format:
                    b.Save(finalPath, format);
                }
            }
            catch (Exception e)
            {
                ErrorBox.Show("Error exporting frames: " + e.Message, "Error", e.StackTrace);
            }

            // Dispose the GIF:
            gif.Dispose();
        }

        private static string ReplaceFileName(string fileName, DateTime t, string sourceName, int frameIndex)
        {
            // Get the name:
            string name = fileName;

            // Replace the tokens:
            while (name.Contains("{%name}"))
                name = name.Replace("{%name}", sourceName);
            while (name.Contains("{%frame}"))
                name = name.Replace("{%frame}", frameIndex + 1 + "");
            while (name.Contains("{%h}"))
                name = name.Replace("{%h}", "" + t.Hour);
            while (name.Contains("{%m}"))
                name = name.Replace("{%m}", "" + t.Minute);
            while (name.Contains("{%s}"))
                name = name.Replace("{%s}", "" + t.Second);
            return name;
        }

        /// <summary>
        /// Returns an ImageFormat based on the file extension provided
        /// </summary>
        /// <param name="format">A valid image file extension (including the dot)</param>
        /// <returns>An ImageFormat that represents the provided image file extension</returns>
        public static ImageFormat GetFormatByString(string format)
        {
            switch (format)
            {
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".jpg":
                    return ImageFormat.Jpeg;
                case ".gif":
                    return ImageFormat.Gif;
                case ".png":
                    return ImageFormat.Png;
                case ".tiff":
                    return ImageFormat.Tiff;
                case ".exif":
                    return ImageFormat.Exif;
                case ".emf":
                    return ImageFormat.Emf;
                case ".wmf":
                    return ImageFormat.Wmf;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns a file extension based on the ImageFormat  provided
        /// </summary>
        /// <param name="format">An ImageFormat</param>
        /// <returns>A file extension fit for the image format</returns>
        public static string GetExtensionForFormat(ImageFormat format)
        {
            if (Equals(format, ImageFormat.Bmp))
            {
                return ".bmp";
            }
            if (Equals(format, ImageFormat.Jpeg))
            {
                return ".jpg";
            }
            if (Equals(format, ImageFormat.Gif))
            {
                return ".gif";
            }
            if (Equals(format, ImageFormat.Png))
            {
                return ".png";
            }
            if (Equals(format, ImageFormat.Tiff))
            {
                return ".tiff";
            }
            if (Equals(format, ImageFormat.Exif))
            {
                return ".exif";
            }
            if (Equals(format, ImageFormat.Emf))
            {
                return ".emf";
            }
            if (Equals(format, ImageFormat.Wmf))
            {
                return ".wmf";
            }

            return ".bmp";
        }

        public static bool IsFileNameValid(string filename)
        {
            // Apply replacements then flatten filename
            var replacedName = ReplaceFileName(filename, DateTime.Now, "abc", 0);

            try
            {
                var unused = Path.GetFullPath(replacedName);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}