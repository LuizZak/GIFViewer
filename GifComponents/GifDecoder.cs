// 
// This file is part of the GifComponents library.
// GifComponents is free software; you can redistribute it and/or
// modify it under the terms of the Code Project Open License.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// Code Project Open License for more details.
// 
// You can read the full text of the Code Project Open License at:
// http://www.codeproject.com/info/cpol10.aspx
//
// GifComponents is a derived work based on NGif written by gOODiDEA.NET
// and published at http://www.codeproject.com/KB/GDI-plus/NGif.aspx,
// with an enhancement by Phil Garcia published at
// http://www.thinkedge.com/blogengine/post/2008/02/20/Animated-GIF-Encoder-for-NET-Update.aspx
//
// Simon Bridewell makes no claim to be the original author of this library,
// only to have created a derived work.

/*
 No copyright asserted on the source code of this class.  May be used for
 any purpose, however, refer to the Unisys LZW patent for any additional
 restrictions.  Please forward any corrections to kweiner@fmsware.com.

 @author Kevin Weiner, FM Software; LZW decoder adapted from John Cristy's ImageMagick.
 @version 1.03 November 2003
 
 Modified by Simon Bridewell, June 2009 - January 2012:
 Downloaded from 
 http://www.thinkedge.com/blogengine/post/2008/02/20/Animated-GIF-Encoder-for-NET-Update.aspx
 http://www.thinkedge.com/BlogEngine/file.axd?file=NGif_src2.zip
 
 1. Adapted for FxCop code analysis compliance and documentation comments converted 
	to .net XML comments.
 2. Added comments relating the properties to data items specified in
 	http://www.w3.org/Graphics/GIF/spec-gif89a.txt
 3. Added property getters to expose the components of the GIF file.
 4. Refactored large amounts of functionality into separate classes which encapsulate 
	the types of the components of a GIF file.
 5. Removed all private declarations which are not components of a GIF file.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using GIF_Viewer.GifComponents.Components;
using GIF_Viewer.GifComponents.Enums;

namespace GIF_Viewer.GifComponents
{
    /// <summary>
    /// Class GifDecoder - Decodes a GIF file into one or more frames and 
    /// exposes its properties, components and any error states.
    /// </summary>
    /// <remarks>
    /// It would be nice to make GifDecoder derive from AsynchronousOperation, however
    /// this would mean all the methods inherited from GifComponent are not available.
    /// If we were to make GifComponent derive from AsynchronousOperation then all
    /// the different GifComponent classes would need to implement 
    /// AsynchronousOperation.
    /// </remarks>
    public class GifDecoder : GifComponent
    {
        /// <summary>
        /// A reference to the last frame that had a disposal mode of NoDisposal
        /// </summary>
        private GifFrame _lastNoDisposalFrame;

        /// <summary>
        /// The delay for each frame, in hundredths of a second
        /// </summary>
        private List<int> _frameDelays;

        /// <summary>
        /// Collection of the application extensions in the file.
        /// </summary>
        private Collection<ApplicationExtension> _applicationExtensions;

        /// <summary>
        /// The frames which make up the GIF file.
        /// </summary>
        private Collection<GifFrame> _frames;

        /// <summary>
        /// The frames that are currently loaded. This queue is used to unload frames after a long period
        /// </summary>
        private Queue<GifFrame> _loadedFrames;

        /// <summary>
        /// The base interval for keyframes
        /// </summary>
        private int _keyframeInterval;

        /// <summary>
        /// The maximum frame queue size
        /// </summary>
        private int _maxFrameQueueSize;

        /// <summary>
        /// The maximum memory allocated for buffers
        /// </summary>
        private int _maxMemoryForBuffer;

        /// <summary>
        /// The maximum memory allocated for keyframes
        /// </summary>
        private int _maxMemoryForKeyframes;

        /// <summary>
        /// The maximum number of frames to try to backtrack into a keyframe before failing and trying to draw the uncomplete frame anyways
        /// </summary>
        private int _maxKeyframeReach;

        /// <summary>
        /// Holds the <see cref="System.IO.Stream"/> from which the GIF is being read.
        /// </summary>
        private readonly Stream _stream;
        
        /// <summary>
        /// Gets a frame from the GIF file.
        /// </summary>
        public GifFrame this[int index] => GetDecodedFrameAtIndex(index);

        /// <summary>
        /// Gets or sets the maximum memory allocated for buffers
        /// </summary>
        public int MaxMemoryForBuffer
        {
            get => _maxMemoryForBuffer;
            set
            {
                _maxMemoryForBuffer = value;
                ApplyMemoryFields();
            }
        }

        /// <summary>
        /// Gets or sets the maximum memory allocated for keyframes
        /// </summary>
        public int MaxMemoryForKeyframes
        {
            get => _maxMemoryForKeyframes;
            set
            {
                _maxMemoryForKeyframes = value;
                ApplyMemoryFields();
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of frames to try to backtrack into a keyframe before failing and trying to draw the uncomplete frame anyways
        /// </summary>
        public int MaxKeyframeReach
        {
            get => _maxKeyframeReach;
            set
            {
                _maxKeyframeReach = value;
                ApplyMemoryFields();
            }
        }

        /// <summary>
        /// Gets the header of the GIF stream, containing the signature and
        /// version of the GIF standard used.
        /// </summary>
        public GifHeader Header { get; private set; }

        /// <summary>
        /// Gets the logical screen descriptor.
        /// </summary>
        /// <remarks>
        /// The Logical Screen Descriptor contains the parameters necessary to 
        /// define the area of the display device within which the images will 
        /// be rendered.
        /// The coordinates in this block are given with respect to the 
        /// top-left corner of the virtual screen; they do not necessarily 
        /// refer to absolute coordinates on the display device.
        /// This implies that they could refer to window coordinates in a 
        /// window-based environment or printer coordinates when a printer is 
        /// used.
        /// </remarks>
        [Description("The Logical Screen Descriptor contains the parameters " +
                     "necessary to define the area of the display device " +
                     "within which the images will be rendered. " +
                     "The coordinates in this block are given with respect " +
                     "to the top-left corner of the virtual screen; they do " +
                     "not necessarily refer to absolute coordinates on the " +
                     "display device. " +
                     "This implies that they could refer to window " +
                     "coordinates in a window-based environment or printer " +
                     "coordinates when a printer is used.")]
        public LogicalScreenDescriptor LogicalScreenDescriptor { get; private set; }

        /// <summary>
        /// Gets the background color.
        /// </summary>
        [Description("The default background color for this GIF file." +
                     "This is derived using the background color index in the " +
                     "Logical Screen Descriptor and looking up the color " +
                     "in the Global Color Table.")]
        public Color BackgroundColor => Color.FromArgb(GlobalColorTable[LogicalScreenDescriptor.BackgroundColorIndex]);

        /// <summary>
        /// Gets the application extensions contained within the GIF stream.
        /// This is an array rather than a property because it looks better in
        /// a property sheet control.
        /// </summary>
        [SuppressMessage("Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays")]
        public IReadOnlyList<ApplicationExtension> ApplicationExtensions => _applicationExtensions;

        /// <summary>
        /// Gets the Netscape 2.0 application extension, if present.
        /// This contains the animation's loop count.
        /// </summary>
        [Description("Gets the Netscape 2.0 application extension, if " +
                     "present. This contains the animation's loop count.")]
        public NetscapeExtension NetscapeExtension { get; private set; }

        /// <summary>
        /// Gets the frame count for this GIF file
        /// </summary>
        public int FrameCount => _frames.Count;

        /// <summary>
        /// Gets the global color table for this GIF data stream, or null if the
        /// frames have local color tables.
        /// </summary>
        public ColorTable GlobalColorTable { get; private set; }
        
        /// <summary>
        /// Reads a GIF file from the specified stream.
        /// </summary>
        /// <param name="inputStream">
        /// A stream to read the GIF data from.
        /// </param>
        public GifDecoder(Stream inputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            if (inputStream.CanRead == false)
            {
                const string message = "The supplied stream cannot be read";
                throw new ArgumentException(message, nameof(inputStream));
            }

            _maxMemoryForBuffer = 1024 * 1024 * 100;   // 100mb for buffering frames
            _maxMemoryForKeyframes = 1024 * 1024 * 15; // 15mb for keyframes
            _maxKeyframeReach = 10;

            _stream = new MemoryStream();
            int bytesRead = 0;
            long copyLength = inputStream.Length;
            byte[] buffer = new byte[copyLength];
            if (copyLength > 0)
            {
                while (bytesRead < copyLength)
                {
                    var count = inputStream.Read(buffer, bytesRead, (int)(copyLength - bytesRead));
                    if (count == 0)
                    {
                        break;
                    }
                    bytesRead += count;
                }
            }
            _stream.Write(buffer, 0, buffer.Length);
            _stream.Position = 0;
        }

        /// <summary>
        /// Unloads all disposable data from frames from memory.
        /// This doesn't dispose the CurrentFrameBitmap property.
        /// </summary>
        public void UnloadCachedData()
        {
            foreach (var frame in _frames)
            {
                frame.Unload(false);
            }
        }

        /// <summary>
        /// Returns an integer that represents the delay for the given frame (in hundredths of a second)
        /// </summary>
        /// <param name="frameIndex">The index of the rame to get the delay from</param>
        /// <returns>The delay, in hundredths of a second</returns>
        public int GetDelayForFrame(int frameIndex)
        {
            return _frameDelays[frameIndex];
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
            // First frame is always independent
            if (frameIndex == 0)
                return true;

            // Get directly- GetDecodedFrameAtIndex pre-renders the frame before returning.
            var previousFrame = _frames[frameIndex - 1];

            var previousDisposalMethod = previousFrame.GraphicControlExtension.DisposalMethod;

            if (previousDisposalMethod == DisposalMethod.RestoreToPrevious && frameIndex < 2)
                previousDisposalMethod = DisposalMethod.RestoreToBackgroundColor;

            if (previousDisposalMethod == DisposalMethod.NotSpecified)
                return true;

            if (previousDisposalMethod == DisposalMethod.DoNotDispose)
            {
                var frame = _frames[frameIndex];

                var prevRect = previousFrame.ImageDescriptor.Region;

                var targetRegion = new Rectangle(Point.Empty, LogicalScreenDescriptor.LogicalScreenSize);
                if (prevRect != targetRegion)
                {
                    return false;
                }

                if (!frame.GraphicControlExtension.HasTransparentColor)
                    return true;
                
                // Client asked for a fast check- be conservative and assume the frame could
                // potentially be transparent at some point.
                if (!checkTransparency)
                    return false;

                // Pre-decode the LZW, if it hasn't been, already
                frame.DecodeLzw();

                var transpColor = frame.GraphicControlExtension.TransparentColorIndex;

                return !frame.DecodedImageBytes.PixelIndexes.Any(b => transpColor == b);
            }

            return false;
        }

        /// <summary>
        /// Decodes the supplied GIF stream.
        /// </summary>
        public void Decode()
        {
            _frameDelays = new List<int>();
            _loadedFrames = new Queue<GifFrame>();
            _frames = new Collection<GifFrame>();
            _applicationExtensions = new Collection<ApplicationExtension>();
            GlobalColorTable = null;

            Header = new GifHeader(_stream);
            if (Header.ErrorState != ErrorState.Ok)
            {
                return;
            }

            LogicalScreenDescriptor = new LogicalScreenDescriptor(_stream);
            if (TestState(ErrorState.EndOfInputStream))
            {
                return;
            }

            if (LogicalScreenDescriptor.HasGlobalColorTable)
            {
                GlobalColorTable = new ColorTable(_stream, LogicalScreenDescriptor.GlobalColorTableSize);
            }

            if (ConsolidatedState == ErrorState.Ok)
            {
                ReadContents(_stream);
            }

            ApplyMemoryFields();
        }

        /// <summary>
        /// Applies the max memory usage fields to the frames on the program
        /// </summary>
        public void ApplyMemoryFields()
        {
            long memPerFrame = LogicalScreenDescriptor.LogicalScreenSize.Width * LogicalScreenDescriptor.LogicalScreenSize.Height * 4;

            // Calculate an optional buffer size to enqueue frames on
            _maxFrameQueueSize = (int)(_maxMemoryForBuffer / memPerFrame);

            // Mark keyframes
            _keyframeInterval = 10;

            foreach (var fr in _frames)
            {
                fr.Keyframe = false;
            }

            if (_maxMemoryForKeyframes == 0)
            {
                _keyframeInterval = 0;
                return;
            }

            if (_keyframeInterval * memPerFrame > _maxMemoryForKeyframes)
            {
                long totFrames = _maxMemoryForKeyframes / memPerFrame;
                _keyframeInterval = (int)(_frames.Count / Math.Max(1, totFrames));
            }

            for (int i = 0; i < _frames.Count; i += Math.Max(1, _keyframeInterval))
            {
                _frames[i].Keyframe = true;
            }
        }

        /// <summary>
        /// Gets a frame (decoding itself and all previous dependent frames, if needed) at a given index.
        /// </summary>
        public GifFrame GetDecodedFrameAtIndex(int index)
        {
            var frame = _frames[index];

            if (_maxFrameQueueSize > 0 && !_loadedFrames.Contains(frame))
            {
                // Unload a previous frame, is the queue is full
                while (_loadedFrames.Count >= _maxFrameQueueSize)
                {
                    var oldFrame = _loadedFrames.Dequeue();

                    if (oldFrame.Index == index - 1)
                    {
                        _loadedFrames.Enqueue(oldFrame);
                        // Avoid an infinite recursion here
                        if (_loadedFrames.Count == 1)
                            break;
                        continue;
                    }

                    if (!oldFrame.Keyframe)
                    {
                        oldFrame.Unload();
                    }
                }

                _loadedFrames.Enqueue(frame);
            }

            frame.DecodeRecursingToKeyframe(_maxKeyframeReach);
            frame.DecodeAndRender();

            return frame;
        }

        /// <summary>
        /// Pre-decodes all independent data for a frame, to prepare it to be later decoded.
        /// This call is blocking until the decoding is complete.
        /// </summary>
        public void PreDecodeFrameAtIndex(int index)
        {
            _frames[index].DecodeLzw();
        }
        
        /// <summary>
        /// Main file parser. Reads GIF content blocks.
        /// </summary>
        /// <param name="inputStream">
        /// Input stream from which the GIF data is to be read.
        /// </param>
        private void ReadContents(Stream inputStream)
        {
            // read GIF file content blocks
            bool done = false;
            GraphicControlExtension lastGce = null;
            while (!done && inputStream.Position < inputStream.Length /* && ConsolidatedState == ErrorState.Ok */)
            {
                int code = Read(inputStream);

                string message; // for error conditions
                switch (code)
                {

                    case CodeImageSeparator:
                        AddFrame(inputStream, lastGce);
                        break;

                    case CodeExtensionIntroducer:
                        code = Read(inputStream);
                        switch (code)
                        {
                            case CodePlaintextLabel:
                                // TODO: handle plain text extension
                                // TESTME: plain text label extensions
                                SkipBlocks(inputStream);
                                break;

                            case CodeGraphicControlLabel:
                                lastGce = new GraphicControlExtension(inputStream);
                                break;

                            case CodeCommentLabel:
                                // TODO: handle comment extension
                                SkipBlocks(inputStream);
                                break;

                            case CodeApplicationExtensionLabel:
                                var ext = new ApplicationExtension(inputStream);
                                if (ext.ApplicationIdentifier == "NETSCAPE"
                                    && ext.ApplicationAuthenticationCode == "2.0")
                                {
                                    NetscapeExtension = new NetscapeExtension(ext);
                                }
                                else
                                {
                                    // TESTME: ReadContents - non-Netscape application extension
                                    _applicationExtensions.Add(ext);
                                }
                                break;

                            default: // uninteresting extension
                                // TESTME: ReadContents - uninteresting extension
                                SkipBlocks(inputStream);
                                break;
                        }
                        break;

                    case CodeTrailer:
                        // We've reached an explicit end-of-data marker, so stop
                        // processing the stream.
                        done = true;
                        break;

                    case 0x00: // bad byte, but keep going and see what happens
                        // TESTME: unexpected 0x00 read from input stream
                        message
                            = "Unexpected block terminator encountered at "
                            + "position " + inputStream.Position
                            + " after reading " + _frames.Count + " frames.";
                        SetStatus(ErrorState.UnexpectedBlockTerminator,
                                   message);
                        break;

                    case -1: // reached the end of the input stream
                        // TESTME: end of stream without finding 0x3b trailer
                        message
                            = "Reached the end of the input stream without "
                            + "encountering trailer 0x3b";
                        SetStatus(ErrorState.EndOfInputStream, message);
                        break;

                    default:
                        // TESTME: unrecognised code
                        message
                            = "Bad data block introducer: 0x"
                            + code.ToString("X", CultureInfo.InvariantCulture).PadLeft(2, '0')
                            + " (" + code + ") at position " + inputStream.Position
                            + " ("
                            + inputStream.Position.ToString("X",
                                                           CultureInfo.InvariantCulture)
                            + " hex) after reading "
                            + _frames.Count + " frames.";
                        SetStatus(ErrorState.BadDataBlockIntroducer, message);
                        break;
                }
            }

            // Recurse graphics control excention
            _frames.Last()?.RecurseGraphicControlExtension();

            for (int i = 0; i < _frames.Count; i++)
            {
                _frames[i].IsIndependent = IsFrameIndependent(i);
            }
        }
        
        /// <summary>
        /// Reads a frame from the input stream and adds it to the collection
        /// of frames.
        /// </summary>
        /// <param name="inputStream">
        /// Input stream from which the frame is to be read.
        /// </param>
        /// <param name="lastGce">
        /// The graphic control extension most recently read from the input
        /// stream.
        /// </param>
        private void AddFrame(Stream inputStream, GraphicControlExtension lastGce)
        {
            GifFrame previousFrame = null;
            if (_frames.Count > 0)
            {
                previousFrame = _frames[_frames.Count - 1];
            }

            // Setup the frame delay
            _frameDelays.Add(lastGce?.DelayTime ?? 0);

            var frame = new GifFrame(inputStream, LogicalScreenDescriptor, GlobalColorTable, lastGce, previousFrame, _lastNoDisposalFrame, _frames.Count);
            if (lastGce == null || lastGce.DisposalMethod == DisposalMethod.DoNotDispose ||
                lastGce.DisposalMethod == DisposalMethod.NotSpecified)
            {
                _lastNoDisposalFrame = frame;
            }
            
            _frames.Add(frame);
        }
    }
}