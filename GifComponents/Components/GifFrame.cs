#region Copyright (C) Simon Bridewell, Kevin Weiner
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
 Amended by Simon Bridewell June 2009-February 2010:
 1. Made member variables private.
 2. Added various properties to expose all the elements of the GifFrame.
 3. Added constructors for use in both encoding and decoding.
 4. Derive from GifComponent.
 5. Added constructor( Stream... )
 6. Removed code to swap out transparent colour and replace with black
    (bug 2940635).
 7. Corrected decoding of frames with transparent pixels (bug 2940669)
*/
#endregion

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;

using GIF_Viewer.Utils;

namespace GifComponents.Components
{
    /// <summary>
    /// A single image frame from a GIF file.
    /// Originally a nested class within the GifDecoder class by Kevin Weiner.
    /// Downloaded from 
    /// http://www.thinkedge.com/BlogEngine/file.axd?file=NGif_src2.zip
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class GifFrame : GifComponent
    {
        #region declarations
        private int _index;
        private bool _keyframe;
        private Image _image;
        private int _delay;
        private bool _expectsUserInput;
        private Point _position;
        private GifHeader _header;
        private ColourTable _localColourTable;
        private GraphicControlExtension _extension;
        private ImageDescriptor _imageDescriptor;
        private Color _backgroundColour;
        private Stream inputStream;
        private LogicalScreenDescriptor logicalScreenDescriptor;
        private ColourTable globalColourTable;
        private GraphicControlExtension graphicControlExtension;
        /// <summary>
        /// Whether this GifFrame is loaded on memory
        /// </summary>
        private bool isLoaded;
        /// <summary>
        /// Whether this frame requires redraw, since it lacks all the meaningful information from previous frames
        /// </summary>
        private bool requiresRedraw;
        /// <summary>
        /// The offset of the stream for this frame
        /// </summary>
        private long streamOffset;
        /// <summary>
        /// Field used by keyframes to signal that their drawing state is fully ready.
        /// When all previous frames before a keyframe are drawn, the keyframe is said to be 'ready' to be used as a proper keyframe.
        /// </summary>
        private bool keyframeReady;
        /// <summary>
        /// Whether the image for this frame was created with partial information (lacking information from all the previous frames)
        /// </summary>
        private bool isImagePartial;
        private GifFrame previousFrame;
        private GifFrame previousFrameBut1;
        #endregion

        #region constructors

        #region constructor( Stream, , , , , , )
        /// <summary>
        /// Creates and returns a GifFrame by reading its data from the supplied
        /// input stream.
        /// </summary>
        /// <param name="inputStream">
        /// A stream containing the data which makes the GifStream, starting 
        /// with the image descriptor for this frame.
        /// </param>
        /// <param name="logicalScreenDescriptor">
        /// The logical screen descriptor for the GIF stream.
        /// </param>
        /// <param name="globalColourTable">
        /// The global colour table for the GIF stream.
        /// </param>
        /// <param name="graphicControlExtension">
        /// The graphic control extension, if any, which precedes this image in
        /// the input stream.
        /// </param>
        /// <param name="previousFrame">
        /// The frame which precedes this one in the GIF stream, if present.
        /// </param>
        /// <param name="previousFrameBut1">
        /// The frame which precedes the frame before this one in the GIF stream,
        /// if present.
        /// </param>
        /// <param name="header">The header of the GIF file</param>
        /// <param name="index">The index of this frame on the owning animation</param>
        public GifFrame(Stream inputStream,
                         LogicalScreenDescriptor logicalScreenDescriptor,
                         ColourTable globalColourTable,
                         GraphicControlExtension graphicControlExtension,
                         GifFrame previousFrame,
                         GifFrame previousFrameBut1,
                         GifHeader header,
                         int index)
        {
            if (logicalScreenDescriptor == null)
            {
                throw new ArgumentNullException("logicalScreenDescriptor");
            }

            if (graphicControlExtension == null)
            {
                SetStatus(ErrorState.NoGraphicControlExtension, "");
                // use a default GCE
                graphicControlExtension = new GraphicControlExtension(GraphicControlExtension.ExpectedBlockSize, DisposalMethod.NotSpecified, false, false, 100, 0);
            }

            this._index = index;
            this.logicalScreenDescriptor = logicalScreenDescriptor;
            this.globalColourTable = globalColourTable;
            this.graphicControlExtension = graphicControlExtension;
            this.isLoaded = false;
            this.requiresRedraw = true;
            this.streamOffset = inputStream.Position;
            this.inputStream = inputStream;
            this.isImagePartial = true;
            this._header = header;
            this.previousFrame = previousFrame;
            this.previousFrameBut1 = previousFrameBut1;

            Skip();
        }
        #endregion

        #endregion

        #region properties

        #region read/write properties

        #region Delay property
        /// <summary>
        /// Gets and sets the delay in hundredths of a second before showing 
        /// the next frame.
        /// </summary>
        [Description("The delay in hundredths of a second before showing " +
                      "the next frame in the animation")]
        public int Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }
        #endregion

        #region BackgroundColour property
        /// <summary>
        /// Gets and sets the background colour of the current frame
        /// </summary>
        [Description("The background colour for this frame.")]
        public Color BackgroundColour
        {
            get { return _backgroundColour; }
            set { _backgroundColour = value; }
        }
        #endregion

        #region ExpectsUserInput property
        /// <summary>
        /// Gets a flag indicating whether the device displaying the animation
        /// should wait for user input (e.g. a mouse click or key press) before
        /// displaying the next frame.
        /// </summary>
        /// <remarks>
        /// This is actually a property of the graphic control extension.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// An attempt was made to set this property for a GifFrame which was
        /// created by a GifDecoder.
        /// </exception>
        [Description("Gets a flag indicating whether the device displaying " +
                      "the animation should wait for user input (e.g. a mouse " +
                      "click or key press) before displaying the next frame.")]
        public bool ExpectsUserInput
        {
            get
            {
                if (_extension == null)
                {
                    return _expectsUserInput;
                }
                else
                {
                    return _extension.ExpectsUserInput;
                }
            }
            set
            {
                if (_extension == null)
                {
                    _expectsUserInput = value;
                }
                else
                {
                    string message
                        = "This GifFrame was returned by a GifDecoder so this "
                        + "property is read-only";
                    throw new InvalidOperationException(message);
                }
            }
        }
        #endregion

        #region Position property
        /// <summary>
        /// Gets and sets the position of this frame's image within the logical
        /// screen.
        /// </summary>
        /// <remarks>
        /// This is actually a property of the image descriptor.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// An attempt was made to set this property for a GifFrame which was
        /// created by a GifDecoder.
        /// </exception>
        [Description("Gets and sets the position of this frame's image " +
                      "within the logical screen.")]
        public Point Position
        {
            get
            {
                if (_imageDescriptor == null)
                {
                    return _position;
                }
                else
                {
                    return _imageDescriptor.Position;
                }
            }
            set
            {
                if (_imageDescriptor == null)
                {
                    _position = value;
                }
                else
                {
                    string message
                        = "This GifFrame was returned by a GifDecoder so this "
                        + "property is read-only";
                    throw new InvalidOperationException(message);
                }
            }
        }
        #endregion

        /// <summary>
        /// Gets or sets whether this frame is a keyword
        /// </summary>
        public bool Keyframe
        {
            get { return _keyframe; }
            set { _keyframe = value; }
        }

        #endregion

        #region read-only properties

        /// <summary>
        /// The index of this frame on the animation
        /// </summary>
        public int Index
        {
            get { return _index; }
        }

        #region TheImage property
        /// <summary>
        /// Gets the image held in this frame.
        /// </summary>
        [Description("The image held in this frame")]
        [Category("Set by decoder")]
        public Image TheImage
        {
            get { return _image; }
        }
        #endregion

        #region LocalColourTable property
        /// <summary>
        /// Gets the local colour table for this frame.
        /// </summary>
        [Description("The local colour table for this frame")]
        [Category("Set by decoder")]
        public ColourTable LocalColourTable
        {
            get { return _localColourTable; }
        }
        #endregion

        #region GraphicControlExtension property
        /// <summary>
        /// Gets the graphic control extension which precedes this image.
        /// </summary>
        [Description("The graphic control extension which precedes this image.")]
        [Category("Set by decoder")]
        public GraphicControlExtension GraphicControlExtension
        {
            get { return _extension; }
        }
        #endregion

        #region ImageDescriptor property
        /// <summary>
        /// Gets the image descriptor for this frame.
        /// </summary>
        [Category("Set by decoder")]
        [Description("The image descriptor for this frame. This contains the " +
                      "size and position of the image, and flags indicating " +
                      "whether the colour table is global or local, whether " +
                      "it is sorted, and whether the image is interlaced.")]
        public ImageDescriptor ImageDescriptor
        {
            get { return _imageDescriptor; }
        }
        #endregion

        #endregion

        #endregion

        #region public override WriteToStream method
        /// <summary>
        /// Not implemented in this class because writing out of frames is performed
        /// by the WriteFrame method of the AnimatedGifEncoder class
        /// </summary>
        /// <param name="outputStream">
        /// The output stream to write to.
        /// </param>
        public override void WriteToStream(Stream outputStream)
        {
            string message
                = "This method is not implemented because writing of GifFrames is "
                + "performed by the WriteFrame method of the AnimatedGifEncoder class";
            throw new NotImplementedException(message);
        }
        #endregion

        #region public methods

        /// <summary>
        /// Recursively checks whether the current sequence of frames requires redrawing
        /// </summary>
        public void CheckRequiresRedraw()
        {
            // Base case: This frame is the first, and has an image attached to it
            if (this._index == 0 && this._image != null)
            {
                this.requiresRedraw = false;
                return;
            }

            // If the disposal mode is set to restore to the background, and the frame image is set, it is said to be valid
            if (graphicControlExtension.DisposalMethod == DisposalMethod.RestoreToBackgroundColour)
            {

            }

            // Check if the previous frame requires redraw, if either this frame or the previous require redraw, mark this one as requiring a redraw
            if (this.previousFrame != null)
            {
                this.requiresRedraw = this._image == null || this.previousFrame.requiresRedraw;
            }

            if (this._keyframe && !keyframeReady)
            {
                keyframeReady = !requiresRedraw;
            }
        }

        /// <summary>
        /// Recurses the drawing until a keyframe is hit
        /// </summary>
        public bool RecurseToKeyframe(int maxDepth)
        {
            if (maxDepth <= 0)
                return false;

            if (!requiresRedraw || keyframeReady)
                return true;

            bool redraw = false;

            if (previousFrame != null)
            {
                redraw = previousFrame.RecurseToKeyframe(maxDepth - 1);
            }

            if (redraw)
            {
                Decode();
            }

            return redraw;
        }

        /// <summary>
        /// Unloads the data from this frame from memory and marks this frame as not loaded
        /// </summary>
        public void Unload()
        {
            if (!this.isLoaded)
                return;

            this._image.Dispose();
            this._imageDescriptor.Dispose();

            this._image = null;
            this._imageDescriptor = null;

            this.requiresRedraw = true;
            this.isLoaded = false;
        }

        /// <summary>
        /// Decodes the contents of the GifFrame from the binded stream
        /// </summary>
        /// <param name="force">Whether to force redraw, even if the frame is already drawn</param>
        public void Decode(bool force = false)
        {
            // Image preparation and reutilization checks
            if (isLoaded && !requiresRedraw && !force)
            {
                return;
            }
            if (isLoaded && (requiresRedraw || force))
            {
                Unload();
            }

            // Prepare the stream
            inputStream.Position = streamOffset;

            _extension = graphicControlExtension;

            int transparentColourIndex = graphicControlExtension.TransparentColourIndex;

            ImageDescriptor imageDescriptor = new ImageDescriptor(inputStream);

            #region determine the colour table to use for this frame
            Color backgroundColour = Color.FromArgb(0); // TODO: is this the right background colour?
            // TODO: use backgroundColourIndex from the logical screen descriptor?
            ColourTable activeColourTable;
            if (imageDescriptor.HasLocalColourTable)
            {
                _localColourTable = new ColourTable(inputStream, imageDescriptor.LocalColourTableSize);
                activeColourTable = _localColourTable; // make local table active
            }
            else
            {
                if (globalColourTable == null)
                {
                    // We have neither local nor global colour table, so we
                    // won't be able to decode this frame.
                    Bitmap emptyBitmap = new Bitmap(logicalScreenDescriptor.LogicalScreenSize.Width, logicalScreenDescriptor.LogicalScreenSize.Height);
                    _image = emptyBitmap;
                    _delay = graphicControlExtension.DelayTime;
                    SetStatus(ErrorState.FrameHasNoColourTable, "");
                    return;
                }
                activeColourTable = globalColourTable; // make global table active
                if (logicalScreenDescriptor.BackgroundColourIndex
                    == transparentColourIndex)
                {
                    backgroundColour = Color.FromArgb(0);
                }
            }
            #endregion

            // decode pixel data
            int pixelCount = imageDescriptor.Size.Width * imageDescriptor.Size.Height;
            TableBasedImageData tbid = new TableBasedImageData(inputStream, pixelCount);
            if (tbid.PixelIndexes.Length == 0)
            {
                // TESTME: constructor - PixelIndexes.Length == 0
                // TODO: probably not possible as TBID constructor rejects 0 pixels
                Bitmap emptyBitmap = new Bitmap(logicalScreenDescriptor.LogicalScreenSize.Width, logicalScreenDescriptor.LogicalScreenSize.Height);
                _image = emptyBitmap;
                _delay = graphicControlExtension.DelayTime;
                SetStatus(ErrorState.FrameHasNoImageData, "");
                return;
            }

            // Skip any remaining blocks up to the next block terminator (in
            // case there is any surplus data before the next frame)
            SkipBlocks(inputStream);

            if (graphicControlExtension != null)
            {
                _delay = graphicControlExtension.DelayTime;
            }
            _imageDescriptor = imageDescriptor;
            _backgroundColour = backgroundColour;
            _image = CreateBitmap(tbid, logicalScreenDescriptor, imageDescriptor, activeColourTable, graphicControlExtension, previousFrame, previousFrameBut1);
            
            CheckRequiresRedraw();

            if (this._image != null && previousFrame != null)
            {
                isImagePartial = previousFrame.isImagePartial;
            }

            isLoaded = true;
        }

        #endregion

        #region private methods

        private void RecursiveDecodeBack()
        {
            if (previousFrame != null && previousFrame != this)
                previousFrame.Decode();
        }

        /// <summary>
        /// Skips the stream past the frame
        /// </summary>
        private void Skip()
        {
            inputStream.Position = streamOffset;

            if (logicalScreenDescriptor == null)
            {
                throw new ArgumentNullException("logicalScreenDescriptor");
            }

            ImageDescriptor imageDescriptor = new ImageDescriptor(inputStream);

            if (imageDescriptor.HasLocalColourTable)
            {
                ColourTable.SkipOnStream(inputStream, imageDescriptor.LocalColourTableSize);
            }

            // decode pixel data
            int pixelCount = imageDescriptor.Size.Width * imageDescriptor.Size.Height;

            TableBasedImageData.SkipOnStream(inputStream);
        }

        /// <summary>
        /// Propagates the graphics control extension through all the frames
        /// </summary>
        private void RecurseGraphicControlExtension()
        {
            if (previousFrame != null)
                previousFrame.RecurseGraphicControlExtension();

            if (graphicControlExtension == null)
            {
                SetStatus(ErrorState.NoGraphicControlExtension, "");
                // use a default GCE
                graphicControlExtension = new GraphicControlExtension(GraphicControlExtension.ExpectedBlockSize,
                                                   DisposalMethod.NotSpecified,
                                                   false,
                                                   false,
                                                   100,
                                                   0);
            }

            _extension = graphicControlExtension;
        }

        #region private static CreateBitmap( ) method

        /// <summary>
        /// Sets the pixels of the decoded image.
        /// </summary>
        /// <param name="imageData">
        /// Table based image data containing the indices within the active
        /// colour table of the colours of the pixels in this frame.
        /// </param>
        /// <param name="lsd">
        /// The logical screen descriptor for the GIF stream.
        /// </param>
        /// <param name="id">
        /// The image descriptor for this frame.
        /// </param>
        /// <param name="activeColourTable">
        /// The colour table to use with this frame - either the global colour
        /// table or a local colour table.
        /// </param>
        /// <param name="gce">
        /// The graphic control extension, if any, which precedes this image in
        /// the input stream.
        /// </param>
        /// <param name="previousFrame">
        /// The frame which precedes this one in the GIF stream, if present.
        /// </param>
        /// <param name="previousFrameBut1">
        /// The frame which precedes the frame before this one in the GIF stream,
        /// if present.
        /// </param>
        private Bitmap CreateBitmap(TableBasedImageData imageData, LogicalScreenDescriptor lsd, ImageDescriptor id, ColourTable activeColourTable, GraphicControlExtension gce, GifFrame previousFrame, GifFrame previousFrameBut1)
        {
            int[] pixelsForThisFrameInt = new int[lsd.LogicalScreenSize.Width * lsd.LogicalScreenSize.Height];
            Bitmap baseImage = GetBaseImage(previousFrame, previousFrameBut1, lsd, gce, activeColourTable);

            // copy each source line to the appropriate place in the destination
            int pass = 1;
            int interlaceRowIncrement = 8;
            int interlaceRowNumber = 0; // the row of pixels we're currently 
            // setting in an interlaced image.
            bool hasTransparent = gce.HasTransparentColour;
            int transparentColor = gce.TransparentColourIndex;
            int logicalWidth = lsd.LogicalScreenSize.Width;
            int logicalHeight = lsd.LogicalScreenSize.Height;

            int[] colorTableIndices = activeColourTable.IntColours;
            byte[] pixelIndices = imageData.PixelIndexes;
            int numColors = activeColourTable.Length;

            for (int i = 0; i < id.Size.Height; i++)
            {
                int pixelRowNumber = i;
                if (id.IsInterlaced)
                {
                    #region work out the pixel row we're setting for an interlaced image
                    if (interlaceRowNumber >= id.Size.Height)
                    {
                        pass++;
                        switch (pass)
                        {
                            case 2:
                                interlaceRowNumber = 4;
                                break;
                            case 3:
                                interlaceRowNumber = 2;
                                interlaceRowIncrement = 4;
                                break;
                            case 4:
                                interlaceRowNumber = 1;
                                interlaceRowIncrement = 2;
                                break;
                        }
                    }
                    #endregion
                    pixelRowNumber = interlaceRowNumber;
                    interlaceRowNumber += interlaceRowIncrement;
                }

                // Colour in the pixels for this row
                pixelRowNumber += id.Position.Y;
                if (pixelRowNumber < logicalHeight)
                {
                    int k = pixelRowNumber * logicalWidth;
                    int dx = k + id.Position.X; // start of line in dest
                    int dlim = dx + id.Size.Width; // end of dest line
                    if ((k + logicalWidth) < dlim)
                    {
                        // TESTME: CreateBitmap - past dest edge
                        dlim = k + logicalWidth; // past dest edge
                    }
                    int sx = i * id.Size.Width; // start of line in source
                    while (dx < dlim)
                    {
                        // map color and insert in destination
                        int indexInColourTable = pixelIndices[sx++];
                        // Set this pixel's colour if its index isn't the 
                        // transparent colour index, or if this frame doesn't
                        // have a transparent colour.
                        int c;
                        if (hasTransparent && indexInColourTable == transparentColor)
                        {
                            c = 0; // transparent pixel
                        }
                        else if (indexInColourTable < numColors)
                        {
                            c = colorTableIndices[indexInColourTable];
                        }
                        else
                        {
                            // TESTME: CreateBitmap - BadColourIndex 
                            c = (255 << 24);
                            string message
                                = "Colour index: "
                                + indexInColourTable
                                + ", colour table length: "
                                + activeColourTable.Length
                                + " (" + dx + "," + pixelRowNumber + ")";
                            SetStatus(ErrorState.BadColourIndex, message);
                        }

                        pixelsForThisFrameInt[dx] = c;
                        dx++;
                    }
                }
            }
            return CreateBitmap(baseImage, pixelsForThisFrameInt);
        }

        /// <summary>
        /// Sets the pixels of the decoded image.
        /// </summary>
        /// <param name="imageData">
        /// Table based image data containing the indices within the active
        /// colour table of the colours of the pixels in this frame.
        /// </param>
        /// <param name="lsd">
        /// The logical screen descriptor for the GIF stream.
        /// </param>
        /// <param name="id">
        /// The image descriptor for this frame.
        /// </param>
        /// <param name="activeColourTable">
        /// The colour table to use with this frame - either the global colour
        /// table or a local colour table.
        /// </param>
        /// <param name="gce">
        /// The graphic control extension, if any, which precedes this image in
        /// the input stream.
        /// </param>
        /// <param name="previousFrame">
        /// The frame which precedes this one in the GIF stream, if present.
        /// </param>
        /// <param name="previousFrameBut1">
        /// The frame which precedes the frame before this one in the GIF stream,
        /// if present.
        /// </param>
        private Bitmap CreateBitmap(Bitmap bitmap, LogicalScreenDescriptor lsd, ImageDescriptor id, ColourTable activeColourTable, GraphicControlExtension gce, GifFrame previousFrame, GifFrame previousFrameBut1)
        {
            int[] pixelsForThisFrameInt = new int[lsd.LogicalScreenSize.Width * lsd.LogicalScreenSize.Height];
            Bitmap baseImage = GetBaseImage(previousFrame, previousFrameBut1, lsd, gce, activeColourTable);
            
            //return CreateBitmap(baseImage, pixelsForThisFrameInt);
            Graphics g = Graphics.FromImage(baseImage);
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            g.DrawImage(bitmap, Point.Empty);
            g.Flush();
            g.Dispose();

            return baseImage;
        }

        #endregion

        #region private static GetBaseImage method
        /// <summary>
        /// Gets the base image for this frame. This will be overpainted with
        /// the pixels for this frame, where they are not transparent.
        /// </summary>
        /// <param name="previousFrame">
        /// The frame which preceded this frame in the GIF stream.
        /// Null if this is the first frame in the stream.
        /// </param>
        /// <param name="previousFrameBut1">
        /// The frame which preceded the previous frame in the GIF stream.
        /// Null if this is the first or seond frame in the stream.
        /// </param>
        /// <param name="lsd">
        /// The logical screen descriptor for this GIF stream.
        /// </param>
        /// <param name="gce">
        /// The graphic control extension for this frame.
        /// </param>
        /// <param name="act">
        /// The active colour table for this frame.
        /// </param>
        /// <returns></returns>
        private Bitmap GetBaseImage(GifFrame previousFrame,
                                     GifFrame previousFrameBut1,
                                     LogicalScreenDescriptor lsd,
                                     GraphicControlExtension gce,
                                     ColourTable act)
        {
            RecurseGraphicControlExtension();

            #region Get the disposal method of the previous frame read from the GIF stream
            DisposalMethod previousDisposalMethod;
            if (previousFrame == null)
            {
                previousDisposalMethod = DisposalMethod.NotSpecified;
            }
            else
            {
                previousDisposalMethod = previousFrame.GraphicControlExtension.DisposalMethod;

                if (previousDisposalMethod == DisposalMethod.RestoreToPrevious && previousFrameBut1 == null)
                {
                    previousDisposalMethod = DisposalMethod.RestoreToBackgroundColour;
                }
            }
            #endregion

            Bitmap baseImage;
            int width = lsd.LogicalScreenSize.Width;
            int height = lsd.LogicalScreenSize.Height;
            int backgroundColorIndex = previousFrame == null ? lsd.BackgroundColourIndex : previousFrame.logicalScreenDescriptor.BackgroundColourIndex;
            int transparentColorIndex = previousFrame == null ? gce.TransparentColourIndex : previousFrame.graphicControlExtension.TransparentColourIndex;
            act = (previousFrame == null ? act : previousFrame._localColourTable == null ? globalColourTable : previousFrame._localColourTable);

            #region paint baseImage

            if (previousFrame == null || previousFrame._image == null)
            {
                baseImage = new Bitmap(width, height);
            }
            else
            {
                baseImage = new Bitmap(previousFrame._image);
            }

            switch (previousDisposalMethod)
            {
                case DisposalMethod.DoNotDispose:
                    // pre-populate image with previous frame
                    break;

                case DisposalMethod.RestoreToBackgroundColour:
                    // pre-populate image with background colour
                    int backgroundColour;
                    if (backgroundColorIndex == transparentColorIndex)
                    {
                        backgroundColour = 0;
                    }
                    else
                    {
                        if (backgroundColorIndex < act.Length)
                        {
                            backgroundColour = act[backgroundColorIndex];
                        }
                        else
                        {
                            backgroundColour = (255 << 24);
                            string message
                                = "Background colour index: "
                                + lsd.BackgroundColourIndex
                                + ", colour table length: "
                                + act.Length;
                            SetStatus(ErrorState.BadColourIndex, message);
                        }
                    }

                    // Adjust transparency
                    backgroundColour &= 0x00FFFFFF;

                    FastBitmap fastBaseImage = new FastBitmap(baseImage);
                    fastBaseImage.Lock();

                    // If the area to redraw is the whole image, utilize the fast image drawing method FastBitmap.Clear()
                    if (previousFrame._imageDescriptor.Position.X == 0 && previousFrame._imageDescriptor.Position.Y == 0 &&
                        previousFrame._imageDescriptor.Size.Width == logicalScreenDescriptor.LogicalScreenSize.Width &&
                        previousFrame._imageDescriptor.Size.Width == logicalScreenDescriptor.LogicalScreenSize.Width)
                    {
                        fastBaseImage.Clear(backgroundColour);
                    }
                    else
                    {
                        for (int y = previousFrame._imageDescriptor.Position.Y; y < previousFrame._imageDescriptor.Position.Y + previousFrame._imageDescriptor.Size.Height; y++)
                        {
                            for (int x = previousFrame._imageDescriptor.Position.X; x < previousFrame._imageDescriptor.Position.X + previousFrame._imageDescriptor.Size.Width; x++)
                            {
                                fastBaseImage.SetPixel(x, y, backgroundColour);
                            }
                        }
                    }

                    fastBaseImage.Unlock();
                    break;

                case DisposalMethod.RestoreToPrevious:
                    // pre-populate image with previous frame but 1
                    // TESTME: DisposalMethod.RestoreToPrevious
                    baseImage = new Bitmap((previousFrameBut1 == null ? previousFrame : previousFrameBut1).TheImage);
                    break;
            }
            #endregion

            return baseImage;
        }
        #endregion

        #region private static CreateBitmap( Bitmap, Color[] ) method
        /// <summary>
        /// Creates and returns a Bitmap of the supplied size composed of pixels
        /// of the supplied colours, working left to right and then top to 
        /// bottom.
        /// </summary>
        /// <param name="baseImage">
        /// The image to start with; this is overpainted with the supplied 
        /// pixels where they are not transparent.
        /// </param>
        /// <param name="pixels">
        /// An array of the colours of the pixels for the bitmap to be created.
        /// </param>
        private static Bitmap CreateBitmap(Bitmap baseImage, int[] pixels)
        {
            using (FastBitmap fastBitmap = baseImage.FastLock())
            {
                fastBitmap.CopyFromArray(pixels, true);
            }

            return baseImage;
        }
        #endregion

        #endregion
    }
}