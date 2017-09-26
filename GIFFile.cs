using System;
using System.Drawing;
using GIF_Viewer.GifComponents;
using GIF_Viewer.GifComponents.Enums;
using GIF_Viewer.Utils;

namespace GIF_Viewer
{
    /// <summary>
    /// Represents a GIF file
    /// </summary>
    public class GifFile : IDisposable
    {
        /// <summary>
        /// Path to the GIF file
        /// </summary>
        public string GifPath = "";
        /// <summary>
        /// Image object representing the current GIF
        /// </summary>
        public Bitmap Gif;

        /// <summary>
        /// The decoder holding the information about the currently loaded .gif file
        /// </summary>
        private GifDecoder _gifDecoded;

        /// <summary>
        /// Whether there is a GIF file currently loaded on this GIFFile object
        /// </summary>
        public bool Loaded { get; private set; }

        /// <summary>
        /// Whether the GIF file is playing
        /// </summary>
        public bool Playing = false;
        /// <summary>
        /// Gets the ammount of frames on this gif
        /// </summary>
        public int FrameCount { get; private set; }

        /// <summary>
        /// Gets or sets the current frame being displayed
        /// </summary>
        public int CurrentFrame
        {
            get => _currentFrame;
            set => SetCurrentFrame(value);
        }
        /// <summary>
        /// Intervals (in ms) between each frame
        /// </summary>
        public int[] Intervals;
        /// <summary>
        /// Gets the current frame interval (in milliseconds)
        /// </summary>
        public int CurrentInterval { get; set; }

        /// <summary>
        /// Whether the GIF file should loop
        /// </summary>
        public bool CanLoop;

        /// <summary>
        /// Gets the Width of this GIF file
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the Height of this GIF file
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The current frame being displayed
        /// </summary>
        private int _currentFrame;

        /// <summary>
        /// Disposes of this GIF file
        /// </summary>
        public void Dispose()
        {
            Gif.Dispose();
            _gifDecoded.Dispose();

            _gifDecoded = null;
        }

        /// <summary>
        /// Applies the memory settings to the currently loaded gif file
        /// </summary>
        public void ApplyMemorySettings()
        {
            if (_gifDecoded == null || !Loaded)
                return;

            _gifDecoded.MaxMemoryForBuffer    = Settings.Instance.MaxBufferMemory * 1024 * 1024;
            _gifDecoded.MaxMemoryForKeyframes = Settings.Instance.MaxKeyframeMemory * 1024 * 1024;
            _gifDecoded.MaxKeyframeReach = Settings.Instance.MaxKeyframeReach;

            _gifDecoded.ApplyMemoryFields();
        }

        /// <summary>
        /// Loads this GIF file's parameters from the given GIF file
        /// </summary>
        /// <param name="path">The gif to load the parameters from</param>
        public void LoadFromPath(string path)
        {
            Loaded = false;

            // Set the path
            GifPath = path;

            // Decode the gif file
            _gifDecoded = new GifDecoder(path);
            _gifDecoded.Decode();

            if (_gifDecoded.ConsolidatedState != ErrorState.Ok)
            {
                _gifDecoded.Dispose();
                return;
            }

            // Get information from the gif file
            Width = _gifDecoded.LogicalScreenDescriptor.LogicalScreenSize.Width;
            Height = _gifDecoded.LogicalScreenDescriptor.LogicalScreenSize.Height;

            _currentFrame = 0;
            FrameCount = _gifDecoded.FrameCount;

            // Get frame intervals
            Intervals = new int[FrameCount];
            for (int i = 0; i < FrameCount; i++)
            {
                Intervals[i] = _gifDecoded.GetDelayForFrame(i) * 10;
            }

            // Get whether this GIF loops over:
            CanLoop = _gifDecoded.NetscapeExtension != null && _gifDecoded.NetscapeExtension.LoopCount == 0;

            // Force load of the first frame
            var img = _gifDecoded[0].TheImage;

            Gif = new Bitmap(img.Width, img.Height);

            FastBitmap.CopyPixels(_gifDecoded[_currentFrame].TheImage, Gif);

            Loaded = true;

            ApplyMemorySettings();
        }

        /// <summary>
        /// Returns an interval in ms for the given frame
        /// </summary>
        /// <param name="frame">The frame to get the interval of</param>
        /// <returns>The interval for the frame, in ms</returns>
        public int GetIntervalForFrame(int frame)
        {
            return Intervals[frame] == 0 ? 1 : Intervals[frame];
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
            if (_currentFrame == currentFrame)
                return;

            _currentFrame = currentFrame;
            CurrentInterval = Intervals[currentFrame];
            FastBitmap.CopyPixels(_gifDecoded[currentFrame].TheImage, Gif);
        }

        /// <summary>
        /// Returns the frame count of this Gif file
        /// </summary>
        /// <returns>The frame count of this Gif file</returns>
        public int GetFrameCount()
        {
            return FrameCount;
        }
    }
}