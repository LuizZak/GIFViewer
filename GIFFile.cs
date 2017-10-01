using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        /// Image object representing the current GIF frame.
        /// </summary>
        public Bitmap CurrentFrameBitmap { get; private set; }

        /// <summary>
        /// The decoder holding the information about the currently loaded .gif file
        /// </summary>
        private GifDecoder _gifDecoder;

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
            CurrentFrameBitmap?.Dispose();
            _gifDecoder?.Dispose();

            _gifDecoder = null;
        }

        /// <summary>
        /// Unloads all disposable data from frames from memory.
        /// This doesn't dispose the CurrentFrameBitmap property.
        /// </summary>
        public void UnloadCachedData()
        {
            _gifDecoder.UnloadCachedData();
        }

        /// <summary>
        /// Applies the memory settings to the currently loaded gif file
        /// </summary>
        public void ApplyUserSelectedMemorySettings()
        {
            ApplyMemorySettings(Settings.Instance.MaxBufferMemory * 1024 * 1024,
                Settings.Instance.MaxKeyframeMemory * 1024 * 1024, Settings.Instance.MaxKeyframeReach);
        }

        /// <summary>
        /// Applies the memory settings to the currently loaded gif file
        /// </summary>
        public void ApplyMemorySettings(int maxMemoryForBuffer, int maxMemoryForKeyframes, int maxKeyframeReach)
        {
            if (_gifDecoder == null || !Loaded)
                return;

            _gifDecoder.MaxMemoryForBuffer = maxMemoryForBuffer;
            _gifDecoder.MaxMemoryForKeyframes = maxMemoryForKeyframes;
            _gifDecoder.MaxKeyframeReach = maxKeyframeReach;

            _gifDecoder.ApplyMemoryFields();
        }

        /// <summary>
        /// Loads this GIF file's parameters from the given GIF file
        /// </summary>
        /// <param name="path">The gif to load the parameters from</param>
        /// <param name="preloadAllFrames">Whether to pre-load all frame images after loading</param>
        /// <param name="multiThreadedLzwDecoding">Whether to use multi-threading to pre-decode the LZW encoding of the frames</param>
        public void LoadFromPath(string path, bool preloadAllFrames = false, bool multiThreadedLzwDecoding = false)
        {
            Loaded = false;

            // Set the path
            GifPath = path;
            var fileStream = new FileInfo(path).OpenRead();

            // Decode the gif file
            _gifDecoder?.Dispose();

            _gifDecoder = new GifDecoder(fileStream);
            _gifDecoder.Decode();

            if (_gifDecoder.ConsolidatedState != ErrorState.Ok)
            {
                _gifDecoder.Dispose();
                return;
            }

            // Get information from the gif file
            Width = _gifDecoder.LogicalScreenDescriptor.LogicalScreenSize.Width;
            Height = _gifDecoder.LogicalScreenDescriptor.LogicalScreenSize.Height;

            _currentFrame = 0;
            FrameCount = _gifDecoder.FrameCount;

            // Get frame intervals
            Intervals = new int[FrameCount];
            for (int i = 0; i < FrameCount; i++)
            {
                Intervals[i] = _gifDecoder.GetDelayForFrame(i) * 10;
            }

            // Get whether this GIF loops over:
            CanLoop = _gifDecoder.NetscapeExtension != null && _gifDecoder.NetscapeExtension.LoopCount == 0;

            // Pre-decode frames
            if (multiThreadedLzwDecoding)
            {
                int threadCount = Environment.ProcessorCount * 2;

                var tasks = new List<Task>();

                for (int i = 0; i < FrameCount; i++)
                {
                    var frameIndex = i;
                    var thread = new Task(() => { _gifDecoder.PreDecodeFrameAtIndex(frameIndex); });
                    tasks.Add(thread);
                }

                TaskUtils.StartAndWaitAllThrottled(tasks, threadCount);
            }

            if (preloadAllFrames)
            {
                for (int i = 0; i < FrameCount; i++)
                {
                    _gifDecoder.GetDecodedFrameAtIndex(i);
                }
            }

            // Force load of the first frame
            var img = _gifDecoder[0].TheImage;

            CurrentFrameBitmap = new Bitmap(img.Width, img.Height);

            FastBitmap.CopyPixels(_gifDecoder[_currentFrame].TheImage, CurrentFrameBitmap);

            Loaded = true;

            ApplyUserSelectedMemorySettings();
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
            FastBitmap.CopyPixels(_gifDecoder[currentFrame].TheImage, CurrentFrameBitmap);
        }

        /// <summary>
        /// Returns whether a frame at a given index can be rendered independently from any previous frame.
        /// 
        /// Frames are independent if they have one of the following render modes:
        /// 
        /// 1. Not Specified (e.g. every first frame);
        /// 2. RestoreToBackground, with the restore area being the whole frame bounds;
        /// </summary>
        /// <param name="frameIndex">Index of frame to verify</param>
        /// <param name="checkTransparency">
        /// Whether to load the indexed colors from the stream for the frame to validate if its overdraw
        /// fully overlaps the previous frames with non-transparent pixels.
        /// </param>
        public bool IsFrameIndependent(int frameIndex, bool checkTransparency = false)
        {
            return _gifDecoder.IsFrameIndependent(frameIndex, checkTransparency);
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