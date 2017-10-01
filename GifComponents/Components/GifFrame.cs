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

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using GIF_Viewer.GifComponents.Enums;
using GIF_Viewer.Utils;

namespace GIF_Viewer.GifComponents.Components
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
        private bool _expectsUserInput;
        private Point _position;
        private ColorTable _localColorTable;
        private GraphicControlExtension _extension;
        private readonly Stream _inputStream;
        private readonly LogicalScreenDescriptor _logicalScreenDescriptor;
        private readonly ColorTable _globalColorTable;
        private GraphicControlExtension _graphicControlExtension;
        /// <summary>
        /// Whether this GifFrame is loaded on memory
        /// </summary>
        private bool _isLoaded;
        /// <summary>
        /// Whether this frame requires redraw, since it lacks all the meaningful information from previous frames
        /// </summary>
        private bool _requiresRedraw;
        /// <summary>
        /// Field used by keyframes to signal that their drawing state is fully ready.
        /// When all previous frames before a keyframe are drawn, the keyframe is said to be 'ready' to be used as a proper keyframe.
        /// </summary>
        private bool _keyframeReady;
        /// <summary>
        /// Whether the image for this frame was created with partial information (lacking information from all the previous frames)
        /// </summary>
        private bool _isImagePartial;
        private readonly GifFrame _previousFrame;
        private readonly GifFrame _previousFrameBut1;

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
        /// <param name="globalColorTable">
        /// The global color table for the GIF stream.
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
        /// <param name="index">The index of this frame on the owning animation</param>
        public GifFrame(Stream inputStream, LogicalScreenDescriptor logicalScreenDescriptor,
            ColorTable globalColorTable, GraphicControlExtension graphicControlExtension, GifFrame previousFrame,
            GifFrame previousFrameBut1, int index)
        {
            if (graphicControlExtension == null)
            {
                SetStatus(ErrorState.NoGraphicControlExtension, "");
                // use a default GCE
                graphicControlExtension = new GraphicControlExtension();
            }

            Index = index;
            _logicalScreenDescriptor = logicalScreenDescriptor ?? throw new ArgumentNullException(nameof(logicalScreenDescriptor));
            _globalColorTable = globalColorTable;
            _graphicControlExtension = graphicControlExtension;
            _isLoaded = false;
            _requiresRedraw = true;
            StreamOffset = inputStream.Position;
            _inputStream = inputStream;
            _isImagePartial = true;
            _previousFrame = previousFrame;
            _previousFrameBut1 = previousFrameBut1;

            // Read image descriptor and skip this frame
            ImageDescriptor = new ImageDescriptor(_inputStream);
            _inputStream.Position = StreamOffset;
            
            Skip();
        }

        /// <summary>
        /// Gets and sets the delay in hundredths of a second before showing 
        /// the next frame.
        /// </summary>
        [Description("The delay in hundredths of a second before showing " +
                      "the next frame in the animation")]
        public int Delay { get; set; }

        /// <summary>
        /// Gets and sets the background color of the current frame
        /// </summary>
        [Description("The background color for this frame.")]
        public Color BackgroundColor { get; set; }

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
            get => _extension?.ExpectsUserInput ?? _expectsUserInput;
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
            get => ImageDescriptor?.Position ?? _position;
            set
            {
                if (ImageDescriptor == null)
                {
                    _position = value;
                }
                else
                {
                    const string message = "This GifFrame was returned by a GifDecoder so this property is read-only";
                    throw new InvalidOperationException(message);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this frame is a keyword
        /// </summary>
        public bool Keyframe { get; set; }

        /// <summary>
        /// The index of this frame on the animation
        /// </summary>
        public int Index { get; }
        
        /// <summary>
        /// The offset of the stream for this frame
        /// </summary>
        public long StreamOffset { get; }

        /// <summary>
        /// Gets the image held in this frame.
        /// </summary>
        [Description("The image held in this frame")]
        [Category("Set by decoder")]
        public Bitmap TheImage { get; private set; }

        /// <summary>
        /// Gets the local color table for this frame.
        /// </summary>
        [Description("The local color table for this frame")]
        [Category("Set by decoder")]
        public ColorTable LocalColorTable => _localColorTable;

        /// <summary>
        /// Gets the graphic control extension which precedes this image.
        /// </summary>
        [Description("The graphic control extension which precedes this image.")]
        [Category("Set by decoder")]
        public GraphicControlExtension GraphicControlExtension => _extension;

        /// <summary>
        /// Gets the image descriptor for this frame.
        /// </summary>
        [Category("Set by decoder")]
        [Description("The image descriptor for this frame. This contains the " +
                      "size and position of the image, and flags indicating " +
                      "whether the color table is global or local, whether " +
                      "it is sorted, and whether the image is interlaced.")]
        public ImageDescriptor ImageDescriptor { get; private set; }

        /// <summary>
        /// Recursively checks whether the current sequence of frames requires redrawing
        /// </summary>
        public void CheckRequiresRedraw()
        {
            // Base case: This frame is the first, and has an image attached to it
            if (Index == 0 && TheImage != null)
            {
                _requiresRedraw = false;
                return;
            }

            // If the disposal mode is set to restore to the background, and the frame image is set, it is said to be valid
            if (_graphicControlExtension.DisposalMethod == DisposalMethod.RestoreToBackgroundColor)
            {

            }

            // Check if the previous frame requires redraw, if either this frame or the previous require redraw, mark this one as requiring a redraw
            if (_previousFrame != null)
            {
                _requiresRedraw = TheImage == null || _previousFrame._requiresRedraw;
            }

            if (Keyframe && !_keyframeReady)
            {
                _keyframeReady = !_requiresRedraw;
            }
        }

        /// <summary>
        /// Recurses the drawing until a keyframe is hit
        /// </summary>
        public bool DecodeRecursingToKeyframe(int maxDepth)
        {
            if (maxDepth <= 0)
                return false;

            if (!_requiresRedraw || _keyframeReady)
                return true;

            bool redraw = false;

            if (_previousFrame != null)
                redraw = _previousFrame.DecodeRecursingToKeyframe(maxDepth - 1);

            if (redraw)
                Decode();

            return redraw;
        }

        /// <summary>
        /// Propagates the graphics control extension through all the frames
        /// </summary>
        public void RecurseGraphicControlExtension()
        {
            if (_extension == null)
                _previousFrame?.RecurseGraphicControlExtension();

            if (_graphicControlExtension == null)
            {
                SetStatus(ErrorState.NoGraphicControlExtension, "");
                // use a default GCE
                _graphicControlExtension = new GraphicControlExtension();
            }

            _extension = _graphicControlExtension;
        }

        /// <summary>
        /// Unloads the data from this frame from memory and marks this frame as not loaded
        /// </summary>
        public void Unload()
        {
            if (!_isLoaded)
                return;

            TheImage.Dispose();

            TheImage = null;

            _requiresRedraw = true;
            _isLoaded = false;
        }

        /// <summary>
        /// Decodes the contents of the GifFrame from the binded stream
        /// </summary>
        /// <param name="force">Whether to force redraw, even if the frame is already drawn</param>
        public void Decode(bool force = false)
        {
            // Image preparation and reutilization checks
            if (_isLoaded)
            {
                if (!_requiresRedraw && !force)
                    return;
                
                // Unload before re-drawing
                Unload();
            }

            // Prepare the stream
            _inputStream.Position = StreamOffset;

            _extension = _graphicControlExtension;

            int transparentColorIndex = _graphicControlExtension.TransparentColorIndex;

            var imageDescriptor = new ImageDescriptor(_inputStream);

            var backgroundColor = Color.FromArgb(0); // TODO: is this the right background color?
            // TODO: use backgroundColorIndex from the logical screen descriptor?
            ColorTable activeColorTable;
            if (imageDescriptor.HasLocalColorTable)
            {
                _localColorTable = new ColorTable(_inputStream, imageDescriptor.LocalColorTableSize);
                activeColorTable = _localColorTable; // make local table active
            }
            else
            {
                if (_globalColorTable == null)
                {
                    // We have neither local nor global color table, so we
                    // won't be able to decode this frame.
                    var emptyBitmap = new Bitmap(_logicalScreenDescriptor.LogicalScreenSize.Width, _logicalScreenDescriptor.LogicalScreenSize.Height);
                    TheImage = emptyBitmap;
                    Delay = _graphicControlExtension.DelayTime;
                    SetStatus(ErrorState.FrameHasNoColorTable, "");
                    return;
                }
                activeColorTable = _globalColorTable; // make global table active
                if (_logicalScreenDescriptor.BackgroundColorIndex
                    == transparentColorIndex)
                {
                    backgroundColor = Color.FromArgb(0);
                }
            }

            // decode pixel data
            int pixelCount = imageDescriptor.Size.Width * imageDescriptor.Size.Height;
            var tbid = new TableBasedImageData(_inputStream, pixelCount);
            if (tbid.PixelIndexes.Length == 0)
            {
                // TESTME: constructor - PixelIndexes.Length == 0
                // TODO: probably not possible as TBID constructor rejects 0 pixels
                var emptyBitmap = new Bitmap(_logicalScreenDescriptor.LogicalScreenSize.Width, _logicalScreenDescriptor.LogicalScreenSize.Height);
                TheImage = emptyBitmap;
                Delay = _graphicControlExtension.DelayTime;
                SetStatus(ErrorState.FrameHasNoImageData, "");
                return;
            }

            // Skip any remaining blocks up to the next block terminator (in
            // case there is any surplus data before the next frame)
            SkipBlocks(_inputStream);

            if (_graphicControlExtension != null)
            {
                Delay = _graphicControlExtension.DelayTime;
            }
            ImageDescriptor = imageDescriptor;
            BackgroundColor = backgroundColor;
            TheImage = CreateBitmap(tbid, _logicalScreenDescriptor, imageDescriptor, activeColorTable, _graphicControlExtension, _previousFrame, _previousFrameBut1);
            
            CheckRequiresRedraw();

            if (TheImage != null && _previousFrame != null)
            {
                _isImagePartial = _previousFrame._isImagePartial;
            }

            _isLoaded = true;
        }

        /// <summary>
        /// Skips the stream past the frame
        /// </summary>
        private void Skip()
        {
            _inputStream.Position = StreamOffset;

            if (_logicalScreenDescriptor == null)
            {
                throw new Exception(@"Logical screen descriptor is null");
            }

            var imageDescriptor = new ImageDescriptor(_inputStream);

            if (imageDescriptor.HasLocalColorTable)
            {
                ColorTable.SkipOnStream(_inputStream, imageDescriptor.LocalColorTableSize);
            }

            TableBasedImageData.SkipOnStream(_inputStream);
        }

        /// <summary>
        /// Sets the pixels of the decoded image.
        /// </summary>
        /// <param name="imageData">
        /// Table based image data containing the indices within the active
        /// color table of the colors of the pixels in this frame.
        /// </param>
        /// <param name="lsd">
        /// The logical screen descriptor for the GIF stream.
        /// </param>
        /// <param name="id">
        /// The image descriptor for this frame.
        /// </param>
        /// <param name="activeColorTable">
        /// The color table to use with this frame - either the global color
        /// table or a local color table.
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
        private unsafe Bitmap CreateBitmap(TableBasedImageData imageData, LogicalScreenDescriptor lsd, ImageDescriptor id, ColorTable activeColorTable, GraphicControlExtension gce, GifFrame previousFrame, GifFrame previousFrameBut1)
        {
            var baseImage = GetBaseImage(previousFrame, previousFrameBut1, lsd, gce, activeColorTable);

            // copy each source line to the appropriate place in the destination
            int pass = 1;
            int interlaceRowIncrement = 8;
            int interlaceRowNumber = 0; // the row of pixels we're currently 
            // setting in an interlaced image.
            bool hasTransparent = gce.HasTransparentColor;
            int transparentColor = gce.TransparentColorIndex;
            int logicalWidth = lsd.LogicalScreenSize.Width;
            int logicalHeight = lsd.LogicalScreenSize.Height;

            int imageX = id.Position.X;
            int imageY = id.Position.Y;
            int imageWidth = id.Size.Width;
            int imageHeight = id.Size.Height;

            byte[] pixelIndices = imageData.PixelIndexes;

            var fastImageBase = baseImage.FastLock();
            int* pointerImage = (int*)fastImageBase.Scan0;

            int[] colorTableIndices = activeColorTable.IntColors;
            int numColors = activeColorTable.Length;

            for (int i = 0; i < imageHeight; i++)
            {
                int pixelRowNumber = i;
                if (id.IsInterlaced)
                {
                    if (interlaceRowNumber >= imageHeight)
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
                    pixelRowNumber = interlaceRowNumber;
                    interlaceRowNumber += interlaceRowIncrement;
                }

                // Color in the pixels for this row
                pixelRowNumber += imageY;
                if (pixelRowNumber >= logicalHeight)
                    continue;

                int k = pixelRowNumber * logicalWidth;
                int dx = k + imageX; // start of line in dest
                int dlim = dx + imageWidth; // end of dest line
                if (k + logicalWidth < dlim)
                {
                    // TESTME: CreateBitmap - past dest edge
                    dlim = k + logicalWidth; // past dest edge
                }
                int sx = i * imageWidth; // start of line in source
                while (dx < dlim)
                {
                    // map color and insert in destination
                    int indexInColorTable = pixelIndices[sx++];
                    // Set this pixel's color if its index isn't the transparent color index, or if this frame doesn't have a transparent color.
                    if (!hasTransparent || indexInColorTable != transparentColor)
                    {
                        if (indexInColorTable < numColors)
                        {
                            pointerImage[dx] = colorTableIndices[indexInColorTable];
                        }
                        else
                        {
                            // TESTME: CreateBitmap - BadColorIndex 
                            pointerImage[dx] = 255 << 24;
                            string message = "Color index: " + indexInColorTable + ", color table length: " + activeColorTable.Length + " (" + dx + "," + pixelRowNumber + ")";
                            SetStatus(ErrorState.BadColorIndex, message);
                        }
                    }

                    dx++;
                }
            }

            fastImageBase.Unlock();

            return baseImage;
        }

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
        /// The active color table for this frame.
        /// </param>
        /// <returns></returns>
        private Bitmap GetBaseImage(GifFrame previousFrame, GifFrame previousFrameBut1, LogicalScreenDescriptor lsd, GraphicControlExtension gce, ColorTable act)
        {
            RecurseGraphicControlExtension();

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
                    previousDisposalMethod = DisposalMethod.RestoreToBackgroundColor;
                }
            }

            Bitmap baseImage;
            int width = lsd.LogicalScreenSize.Width;
            int height = lsd.LogicalScreenSize.Height;
            int backgroundColorIndex = previousFrame?._logicalScreenDescriptor.BackgroundColorIndex ?? lsd.BackgroundColorIndex;
            int transparentColorIndex = previousFrame?._graphicControlExtension.TransparentColorIndex ?? gce.TransparentColorIndex;
            act = previousFrame == null ? act : previousFrame._localColorTable ?? _globalColorTable;

            if (previousDisposalMethod == DisposalMethod.RestoreToPrevious || previousFrame?.TheImage == null)
            {
                baseImage = new Bitmap(width, height);
            }
            else
            {
                baseImage = new Bitmap(width, height);
                FastBitmap.CopyPixels(previousFrame.TheImage, baseImage);
            }

            switch (previousDisposalMethod)
            {
                case DisposalMethod.DoNotDispose:
                    // pre-populate image with previous frame
                    break;

                case DisposalMethod.RestoreToBackgroundColor:
                    // pre-populate image with background color
                    int backgroundColor;
                    if (backgroundColorIndex == transparentColorIndex)
                    {
                        backgroundColor = 0;
                    }
                    else
                    {
                        if (backgroundColorIndex < act.Length)
                        {
                            backgroundColor = act[backgroundColorIndex];
                        }
                        else
                        {
                            backgroundColor = 255 << 24;
                            string message = "Background color index: " + lsd.BackgroundColorIndex + ", color table length: " + act.Length;
                            SetStatus(ErrorState.BadColorIndex, message);
                        }
                    }

                    // Adjust transparency
                    backgroundColor &= 0x00FFFFFF;

                    if (previousFrame?.ImageDescriptor == null)
                        break;

                    using (var fastBaseImage = baseImage.FastLock())
                    {
                        fastBaseImage.ClearRegion(previousFrame.ImageDescriptor.Region, backgroundColor);
                    }

                    break;

                case DisposalMethod.RestoreToPrevious:
                    // pre-populate image with previous frame but 1
                    // TESTME: DisposalMethod.RestoreToPrevious
                    var prevImage = 
                        (previousFrameBut1 ?? previousFrame)?.TheImage;

                    if (prevImage != null)
                    {
                        FastBitmap.CopyPixels(prevImage, baseImage);
                    }

                    break;
            }

            return baseImage;
        }
    }
}