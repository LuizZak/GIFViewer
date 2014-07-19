using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace GIF_Viewer
{
    /// <summary>
    /// Represents a GIF file
    /// </summary>
    public class GIFFile : IDisposable
    {
        /// <summary>
        /// Path to the GIF file
        /// </summary>
        public string GIFPath = "";
        /// <summary>
        /// Image object representing the current GIF
        /// </summary>
        public Image GIF;

        /// <summary>
        /// Whether there is a GIF file currently loaded on this GIFFile object
        /// </summary>
        public bool Loaded = false;

        /// <summary>
        /// Whether the GIF file is playing
        /// </summary>
        public bool Playing = false;
        /// <summary>
        /// Gets the ammount of frames on this gif
        /// </summary>
        public int FrameCount
        {
            get { return this.frameCount; }
        }
        /// <summary>
        /// Gets or sets the current frame being displayed
        /// </summary>
        public int CurrentFrame
        {
            get { return this.currentFrame; }
            set { SetCurrentFrame(value); }
        }
        /// <summary>
        /// Intervals (in ms) between each frame
        /// </summary>
        public int[] Intervals;
        /// <summary>
        /// Gets the current frame interval (in milliseconds)
        /// </summary>
        public int CurrentInterval
        {
            get { return this.currentInterval; }
        }
        /// <summary>
        /// Whether the GIF file should loop
        /// </summary>
        public bool CanLoop = false;

        /// <summary>
        /// The Width of this GIF file
        /// </summary>
        public int Width;
        /// <summary>
        /// The Height of this GIF file
        /// </summary>
        public int Height;

        /// <summary>
        /// Current frame interval (in milliseconds)
        /// </summary>
        private int currentInterval;

        /// <summary>
        /// Ammount of frames on this gif
        /// </summary>
        private int frameCount;

        /// <summary>
        /// The current frame being displayed
        /// </summary>
        private int currentFrame;

        /// <summary>
        /// FrameDimension object for this GIF file
        /// </summary>
        public FrameDimension frameDimension;

        GifComponents.GifDecoder dec;

        /// <summary>
        /// Disposes of this GIF file
        /// </summary>
        public void Dispose()
        {
            GIF.Dispose();
            dec.Dispose();
        }

        /// <summary>
        /// Loads this GIF file's parameters from the given GIF file
        /// </summary>
        /// <param name="path">The gif to load the parameters from</param>
        public void LoadFromPath(string path)
        {
            dec = new GifComponents.GifDecoder(path);

            dec.Decode();

            Loaded = false;

            GIFPath = path;

            Image gif = GIF;

            if (gif != null)
                gif.Dispose();

            gif = Image.FromFile(GIFPath);

            Width = gif.Width;
            Height = gif.Height;

            frameDimension = new FrameDimension(gif.FrameDimensionsList[0]);

            // Get the interval bytes
            byte[] b;
            try
            {
                b = gif.GetPropertyItem(20736).Value;
            }
            catch (Exception)
            {
                b = new byte[] { 0, 0, 0, 0 };
            }

            Intervals = new int[b.Length / 4];

            // Loop:
            int j = 0;
            for (int i = 0; i < b.Length; i += 4 /* Intervals are stored once every 4 bytes */)
            {
                // Iterate through the intervals and store them on the array
                Intervals[j++] = b[i] * 10;
            }

            // Reset current frame:
            currentFrame = 0;

            // Get whether this GIF loops over:
            try
            {
                CanLoop = BitConverter.ToInt16(gif.GetPropertyItem(20737).Value, 0) != 1;
            }
            catch (Exception)
            {
                CanLoop = false;
            }

            // Get the total frames
            frameCount = gif.GetFrameCount(frameDimension);

            //GIF = dec[0].TheImage;
            GIF = gif;
            //GIF = dec.Frames[0].TheImage;
            //GIF = new Bitmap(dec.LogicalScreenDescriptor.LogicalScreenSize.Width, dec.LogicalScreenDescriptor.LogicalScreenSize.Height);

            // Force load of the first frame
            Image img = dec[0].TheImage;

            Loaded = true;
        }

        /// <summary>
        /// Returns an interval in ms for the given frame
        /// </summary>
        /// <param name="frame">The frame to get the interval of</param>
        /// <returns>The interval for the frame, in ms</returns>
        public int GetIntervalForFrame(int frame)
        {
            return (Intervals[frame] == 0 ? 1 : Intervals[frame]);
        }

        /// <summary>
        /// Gets the interval in ms for the current frame of this GIF
        /// </summary>
        /// <returns>The interval in ms for the current frame of this GIF</returns>
        public int GetIntervalForCurrentFrame()
        {
            return GetIntervalForFrame(CurrentFrame);
        }

        /// <summary>
        /// Changes the current frame of this GIF file, changing the GIF file's active frame in the process
        /// </summary>
        /// <param name="currentFrame">The new current frame</param>
        public void SetCurrentFrame(int currentFrame)
        {
            this.currentFrame = currentFrame;
            this.currentInterval = Intervals[currentFrame];
            GIF_Viewer.Utils.FastBitmap.CopyPixels((Bitmap)dec[currentFrame].TheImage, (Bitmap)GIF);
            //GIF.SelectActiveFrame(frameDimension, currentFrame);
        }

        /// <summary>
        /// Returns the frame count of this Gif file
        /// </summary>
        /// <returns>The frame count of this Gif file</returns>
        public int GetFrameCount()
        {
            return frameCount;
        }
    }
}