using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using GifComponents;

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
        /// The decoder holding the information about the currently loaded .gif file
        /// </summary>
        private GifDecoder GIFDecoded;

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
        /// Gets the Width of this GIF file
        /// </summary>
        public int Width
        {
            get { return width; }
        }
        /// <summary>
        /// Gets the Height of this GIF file
        /// </summary>
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        /// The Width of this GIF file
        /// </summary>
        private int width;
        /// <summary>
        /// The Height of this GIF file
        /// </summary>
        private int height;

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
        /// Disposes of this GIF file
        /// </summary>
        public void Dispose()
        {
            GIF.Dispose();
            GIFDecoded.Dispose();
        }

        /// <summary>
        /// Loads this GIF file's parameters from the given GIF file
        /// </summary>
        /// <param name="path">The gif to load the parameters from</param>
        public void LoadFromPath(string path)
        {
            // Set the path
            GIFPath = path;

            // Decode the gif file
            GIFDecoded = new GifComponents.GifDecoder(path);
            GIFDecoded.Decode();

            // Get information from the gif file
            width = GIFDecoded.LogicalScreenDescriptor.LogicalScreenSize.Width;
            height = GIFDecoded.LogicalScreenDescriptor.LogicalScreenSize.Height;

            currentFrame = 0;
            frameCount = GIFDecoded.FrameCount;

            // Get frame intervals
            Intervals = new int[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                Intervals[i] = GIFDecoded.GetDelayForFrame(i) * 10;
            }

            // Get whether this GIF loops over:
            CanLoop = (GIFDecoded.NetscapeExtension != null && GIFDecoded.NetscapeExtension.LoopCount == 0);

            // Force load of the first frame
            Image img = GIFDecoded[0].TheImage;

            GIF = new Bitmap(img.Width, img.Height);

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
            if (this.currentFrame == currentFrame)
                return;

            this.currentFrame = currentFrame;
            this.currentInterval = Intervals[currentFrame];
            GIF_Viewer.Utils.FastBitmap.CopyPixels((Bitmap)GIFDecoded[currentFrame].TheImage, (Bitmap)GIF);
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