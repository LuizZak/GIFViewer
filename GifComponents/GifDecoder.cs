#region Copyright (C) Simon Bridewell, Kevin Weiner, John Christy
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
#endregion

#region changes
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
#endregion

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;

using GifComponents.Components;

namespace GifComponents
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
        /// The header of the GIF file.
        /// </summary>
        private GifHeader _header;

        /// <summary>
        /// The Logical Screen Descriptor contains the parameters necessary to 
        /// define the area of the display device within which the images will 
        /// be rendered.
        /// The coordinates in this block are given with respect to the 
        /// top-left corner of the virtual screen; they do not necessarily 
        /// refer to absolute coordinates on the display device.
        /// This implies that they could refer to window coordinates in a 
        /// window-based environment or printer coordinates when a printer is 
        /// used. 
        /// This block is REQUIRED; exactly one Logical Screen Descriptor must be
        /// present per Data Stream.
        /// </summary>
        private LogicalScreenDescriptor _lsd;

        /// <summary>
        /// The delay for each frame, in hundredths of a second
        /// </summary>
        private List<int> _frameDelays;

        /// <summary>
        /// The global colour table, if present.
        /// </summary>
        private ColourTable _gct;

        /// <summary>
        /// Netscape extension, if present
        /// </summary>
        private NetscapeExtension _netscapeExtension;

        /// <summary>
        /// Collection of the application extensions in the file.
        /// </summary>
        private Collection<ApplicationExtension> _applicationExtensions;

        /// <summary>
        /// The frames which make up the GIF file.
        /// </summary>
        private Collection<GifFrame> _frames;

        /// <summary>
        /// A list of frames that should not be unloaded from memory because they are used as keyframes
        /// </summary>
        private List<GifFrame> _keyFrames;

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
        /// Holds the <see cref="System.IO.Stream"/> from which the GIF is being
        /// read.
        /// </summary>
        private Stream _stream;

        /// <summary>
        /// Reads a GIF file from specified file/URL source  
        /// (URL assumed if name contains ":/" or "file:")
        /// </summary>
        /// <param name="fileName">
        /// Path or URL of image file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The supplied filename is null.
        /// </exception>
        public GifDecoder(string fileName)
            : this(fileName, false) { }

        /// <summary>
        /// Reads a GIF file from specified file/URL source  
        /// (URL assumed if name contains ":/" or "file:")
        /// </summary>
        /// <param name="fileName">
        /// Path or URL of image file.
        /// </param>
        /// <param name="xmlDebugging">
        /// A boolean value indicating whether or not an XML document should be 
        /// created showing how the GIF stream was decoded.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The supplied filename is null.
        /// </exception>
        public GifDecoder(string fileName, bool xmlDebugging)
            : this(new FileInfo(fileName).OpenRead(), xmlDebugging) { }

        /// <summary>
        /// Reads a GIF file from the specified stream.
        /// </summary>
        /// <param name="inputStream">
        /// A stream to read the GIF data from.
        /// </param>
        public GifDecoder(Stream inputStream)
            : this(inputStream, false) { }

        /// <summary>
        /// Reads a GIF file from the specified stream.
        /// </summary>
        /// <param name="inputStream">
        /// A stream to read the GIF data from.
        /// </param>
        /// <param name="xmlDebugging">
        /// A boolean value indicating whether or not an XML document should be 
        /// created showing how the GIF stream was decoded.
        /// </param>
        public GifDecoder(Stream inputStream, bool xmlDebugging)
            : base(xmlDebugging)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }

            if (inputStream.CanRead == false)
            {
                string message = "The supplied stream cannot be read";
                throw new ArgumentException(message, "inputStream");
            }

            _maxMemoryForBuffer = 1024 * 1024 * 100;   // 100mb for buffering frames
            _maxMemoryForKeyframes = 1024 * 1024 * 15; // 15mb for keyframes
            _maxKeyframeReach = 10;

            _stream = new MemoryStream();
            int bytesRead = 0;
            long copyLength = inputStream.Length;
            byte[] buffer;
            buffer = new byte[copyLength];
            if (copyLength > 0)
            {
                int count = 0;
                while (bytesRead < copyLength)
                {
                    count = inputStream.Read(buffer, (int)bytesRead, (int)(copyLength - bytesRead));
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
        /// Returns an integer that represents the delay for the given frame (in hundredths of a second)
        /// </summary>
        /// <param name="frameIndex">The index of the rame to get the delay from</param>
        /// <returns>The delay, in hundredths of a second</returns>
        public int GetDelayForFrame(int frameIndex)
        {
            return _frameDelays[frameIndex];
        }

        #region Decode() method

        /// <summary>
        /// Decodes the supplied GIF stream.
        /// </summary>
        public void Decode()
        {
            _frameDelays = new List<int>();
            _keyFrames = new List<GifFrame>();
            _loadedFrames = new Queue<GifFrame>();
            _frames = new Collection<GifFrame>();
            _applicationExtensions = new Collection<ApplicationExtension>();
            _gct = null;

            _header = new GifHeader(_stream, XmlDebugging);
            if (_header.ErrorState != ErrorState.Ok)
            {
                return;
            }

            _lsd = new LogicalScreenDescriptor(_stream, XmlDebugging);
            if (TestState(ErrorState.EndOfInputStream))
            {
                return;
            }

            if (_lsd.HasGlobalColourTable)
            {
                _gct = new ColourTable(_stream, _lsd.GlobalColourTableSize, XmlDebugging);
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
            long memPerFrame = _lsd.LogicalScreenSize.Width * _lsd.LogicalScreenSize.Height * 4;

            // Calculate an optional buffer size to enqueue frames on
            _maxFrameQueueSize = (int)(_maxMemoryForBuffer / memPerFrame);

            // Mark keyframes
            _keyframeInterval = 10;

            if ((_keyframeInterval) * memPerFrame > _maxMemoryForKeyframes)
            {
                long totFrames = _maxMemoryForKeyframes / memPerFrame;
                _keyframeInterval = (int)(_frames.Count / Math.Max(1, totFrames));
            }

            for (int i = 0; i < _frames.Count; i += Math.Max(1, _keyframeInterval))
            {
                _frames[i].Keyframe = true;
            }
        }

        #endregion

        #region properties

        /// <summary>
        /// Gets a frame from the GIF file.
        /// </summary>
        public GifFrame this[int index]
        {
            get
            {
                GifFrame frame = _frames[index];

                if (!_loadedFrames.Contains(frame))
                {
                    // Unload a previous frame, is the queue is full
                    while (_loadedFrames.Count >= _maxFrameQueueSize)
                    {
                        GifFrame oldFrame = _loadedFrames.Dequeue();

                        if (oldFrame.Index == index - 1)
                        {
                            _loadedFrames.Enqueue(oldFrame);
                            continue;
                        }

                        if (!oldFrame.Keyframe)
                        {
                            oldFrame.Unload();
                        }
                    }

                    _loadedFrames.Enqueue(frame);
                }

                frame.RecurseToKeyframe(_maxKeyframeReach);
                frame.Decode();

                return frame;
            }
        }

        #region Memory/Performance related properties

        /// <summary>
        /// Gets or sets the maximum memory allocated for buffers
        /// </summary>
        public int MaxMemoryForBuffer
        {
            get { return _maxMemoryForBuffer; }
            set
            {
                this._maxMemoryForBuffer = value;
                ApplyMemoryFields();
            }
        }

        /// <summary>
        /// Gets or sets the maximum memory allocated for keyframes
        /// </summary>
        public int MaxMemoryForKeyframes
        {
            get { return this._maxMemoryForKeyframes; }
            set
            {
                this._maxMemoryForKeyframes = value;
                ApplyMemoryFields();
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of frames to try to backtrack into a keyframe before failing and trying to draw the uncomplete frame anyways
        /// </summary>
        public int MaxKeyframeReach
        {
            get { return _maxKeyframeReach; }
            set
            {
                _maxKeyframeReach = value;
                ApplyMemoryFields();
            }
        }

        #endregion

        #region Header property
        /// <summary>
        /// Gets the header of the GIF stream, containing the signature and
        /// version of the GIF standard used.
        /// </summary>
        public GifHeader Header
        {
            get { return _header; }
        }
        #endregion

        #region Logical Screen descriptor property
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
        public LogicalScreenDescriptor LogicalScreenDescriptor
        {
            get { return _lsd; }
        }
        #endregion

        #region BackgroundColour property
        /// <summary>
        /// Gets the background colour.
        /// </summary>
        [Description("The default background colour for this GIF file." +
                     "This is derived using the background colour index in the " +
                     "Logical Screen Descriptor and looking up the colour " +
                     "in the Global Colour Table.")]
        public Color BackgroundColour
        {
            get
            {
                return Color.FromArgb(_gct[_lsd.BackgroundColourIndex]);
            }
        }
        #endregion

        #region ApplicationExtensions property
        /// <summary>
        /// Gets the application extensions contained within the GIF stream.
        /// This is an array rather than a property because it looks better in
        /// a property sheet control.
        /// </summary>
        [SuppressMessage("Microsoft.Performance",
                         "CA1819:PropertiesShouldNotReturnArrays")]
        public ApplicationExtension[] ApplicationExtensions
        {
            get
            {
                ApplicationExtension[] appExts
                    = new ApplicationExtension[_applicationExtensions.Count];
                _applicationExtensions.CopyTo(appExts, 0);
                return appExts; ;
            }
        }
        #endregion

        #region NetscapeExtension property
        /// <summary>
        /// Gets the Netscape 2.0 application extension, if present.
        /// This contains the animation's loop count.
        /// </summary>
        [Description("Gets the Netscape 2.0 application extension, if " +
                      "present. This contains the animation's loop count.")]
        public NetscapeExtension NetscapeExtension
        {
            get { return _netscapeExtension; }
        }
        #endregion

        #region Frame-related properties

        /// <summary>
        /// Gets the frame count for this GIF file
        /// </summary>
        public int FrameCount
        {
            get { return _frames.Count; }
        }

        #endregion

        #region GlobalColourTable property
        /// <summary>
        /// Gets the global colour table for this GIF data stream, or null if the
        /// frames have local colour tables.
        /// </summary>
        public ColourTable GlobalColourTable
        {
            get { return _gct; }
        }
        #endregion

        #endregion

        #region private methods

        #region private ReadContents method
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
            string message; // for error conditions
            while (!done/* && ConsolidatedState == ErrorState.Ok */)
            {
                int code = Read(inputStream);

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
                                // TODO: handle plain text extension (need support in AnimatedGifEncoder first)
                                // TESTME: plain text label extensions
                                SkipBlocks(inputStream);
                                break;

                            case CodeGraphicControlLabel:
                                lastGce = new GraphicControlExtension(inputStream, XmlDebugging);
                                break;

                            case CodeCommentLabel:
                                // TODO: handle comment extension
                                SkipBlocks(inputStream);
                                break;

                            case CodeApplicationExtensionLabel:
                                ApplicationExtension ext
                                    = new ApplicationExtension(inputStream,
                                                                XmlDebugging);
                                if (ext.ApplicationIdentifier == "NETSCAPE"
                                    && ext.ApplicationAuthenticationCode == "2.0")
                                {
                                    _netscapeExtension = new NetscapeExtension(ext);
                                }
                                else
                                {
                                    // TESTME: ReadContents - non-Netscape application extension
                                    // TODO: Add support to AnimatedGifEncoder for non Netscape application extensions
                                    _applicationExtensions.Add(ext);
                                }
                                break;

                            default: // uninteresting extension
                                // TESTME: ReadContents - uninteresting extension
                                // TODO: Add support to AnimatedGifEncoder for uninteresting extensions
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
        }
        #endregion

        #region private WriteCodeToDebugXml method
        private void WriteCodeToDebugXml(int code)
        {
            if (XmlDebugging)
            {
                WriteDebugXmlStartElement("Code");
                WriteDebugXmlAttribute("Value", ToHex(code));
                WriteDebugXmlAttribute("FrameCount", _frames.Count);
                WriteDebugXmlEndElement();
            }
        }
        #endregion

        #region private AddFrame method
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
            GifFrame previousFrameBut1 = null;
            if (_frames.Count > 0)
            {
                previousFrame = _frames[_frames.Count - 1];
            }
            if (_frames.Count > 1)
            {
                previousFrameBut1 = _frames[_frames.Count - 2];
            }

            // Setup the frame delay
            if (lastGce == null)
            {
                _frameDelays.Add(0);
            }
            else
            {
                _frameDelays.Add(lastGce.DelayTime);
            }

            GifFrame frame = new GifFrame(inputStream, _lsd, _gct, lastGce, previousFrame, previousFrameBut1, _frames.Count, XmlDebugging);
            


            _frames.Add(frame);
            WriteDebugXmlNode(frame.DebugXmlReader);
        }
        #endregion

        #region private WriteToStream method
        /// <summary>
        /// Throws a NotSupportedException.
        /// GifDecoders are only intended to read from, and decode streams, not 
        /// to write to them.
        /// </summary>
        /// <param name="outputStream">
        /// The output stream to write to.
        /// </param>
        public override void WriteToStream(Stream outputStream)
        {
            string message
                = "This method is not implemented because a GifDecoder should not "
                + "be written to a stream. It is meant for reading streams!";
            throw new NotSupportedException(message);
        }
        #endregion

        #endregion
    }
}