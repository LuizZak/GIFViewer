/* Copyright Visual C# Kicks, Simon Bridewell
 * This file is open source software and is subject to the Visual C# Kicks
 * Open License, which can be downloaded from http://www.vcskicks.com/license.php
 */

#region changes
/*
 Based on a class downloaded from http://www.vcskicks.com/fast-image-processing.php
 Modified by Simon Bridewell May 2010:
 	* XML and inline comments added and whitespace added to code.
 	* Private members renamed to begin with an underscore.
 	* CheckImageIsLocked and ValidateCoordinates methods added.
*/
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;

namespace GifComponents
{
    /// <summary>
    /// A class which provides faster GetPixel and SetPixel methods than those
    /// provided by the System.Drawing.Bitmap class, using unsafe code and 
    /// pointers to access the pixel data directly.
    /// Project properties must be set to allow unsafe code.
    /// </summary>
    unsafe public class FastBitmap
    {
        #region private PixelData structure
        /// <summary>
        /// Structure defining the way colour information for one pixel of a
        /// bitmap is held in memory.
        /// </summary>
        private struct PixelData
        {
            public byte blue;
            public byte green;
            public byte red;
            public byte alpha;
        }
        #endregion

        #region declarations
        /// <summary>
        /// The bitmap we are working on.
        /// </summary>
        private Bitmap _workingBitmap;

        /// <summary>
        /// Number of bytes of data which make up one line of pixels in the 
        /// bitmap.
        /// </summary>
        private int _width;

        /// <summary>
        /// Width of the bitmap in pixels.
        /// </summary>
        private int _imageWidth;

        /// <summary>
        /// Height of the bitmap in pixels.
        /// </summary>
        private int _imageHeight;

        /// <summary>
        /// Holds information about the lock operation which we perform on the
        /// working bitmap.
        /// </summary>
        private BitmapData _bitmapData;

        /// <summary>
        /// Pointer to the pixel we're working with.
        /// </summary>
        private int* _pBase;

        private int _strideWidth;

        /// <summary>
        /// Pointer to the colour information for the pixel we're working with.
        /// </summary>
        private PixelData* _pixelData;
        #endregion

        #region constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inputBitmap">The bitmap to work with</param>
        public FastBitmap(Bitmap inputBitmap)
        {
            if (inputBitmap == null)
            {
                throw new ArgumentNullException("inputBitmap");
            }
            _workingBitmap = inputBitmap;
        }
        #endregion

        #region public LockImage method
        /// <summary>
        /// Locks the image data into memory.
        /// Call this before calling the GetPixel or SetPixel methods.
        /// </summary>
        public void LockImage()
        {
            Rectangle bounds = new Rectangle(Point.Empty, _workingBitmap.Size);

            _width = (int)(bounds.Width * sizeof(PixelData));
            // This will never happen because sizeof(PixelData) is 4
            // if (_width % 4 != 0) _width = 4 * (_width / 4 + 1);

            _imageWidth = _workingBitmap.Width;
            _imageHeight = _workingBitmap.Height;

            // Lock image
            _bitmapData = _workingBitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            // Get a pointer to the first pixel in the bitmap.
            _pBase = (int*)_bitmapData.Scan0;

            _strideWidth = _bitmapData.Stride / 4;

            // Set the pixel data pointer to just before the first pixel in the image,
            // so that if GetPixelNext is called without calling GetPixel first, it
            // will return the first pixel in the image.
            _pixelData = (PixelData*)_pBase - 1;
        }
        #endregion

        #region GetPixel method
        /// <summary>
        /// Gets the colour of the pixel at the specified co-ordinate in the
        /// bitmap.
        /// Be sure to call the LockImage method before calling GetPixel or
        /// SetPixel for the first time.
        /// </summary>
        /// <param name="x">The horizontal co-ordinate</param>
        /// <param name="y">The vertical co-ordinate</param>
        /// <returns>The colour of the pixel</returns>
        [SuppressMessage("Microsoft.Naming",
                         "CA1704:IdentifiersShouldBeSpelledCorrectly",
                         MessageId = "1#y")]
        [SuppressMessage("Microsoft.Naming",
                         "CA1704:IdentifiersShouldBeSpelledCorrectly",
                         MessageId = "0#x")]
        public Color GetPixel(int x, int y)
        {
            // The calls to CheckImageIsLocked and ValidateCoordinates slow
            // processing slightly but ensure that meaningful error messages are
            // returned to the caller in the event that the image isn't locked
            // into memory or the x or y co-ordinates are outside the bounds of
            // the image.
            if (_pBase == null)
            {
                string message
                    = "The LockImage method must be called before the GetPixel method.";
                throw new InvalidOperationException(message);
            }
            //ValidateCoordinates(x, y);
            if (x < 0 || x >= _imageWidth)
            {
                throw new ArgumentOutOfRangeException("x", "Something bad happened!");
            }
            if (y < 0 || y >= _imageHeight)
            {
                throw new ArgumentOutOfRangeException("y", "Something bad happened!");
            }
            // Calculate a pointer to the memory address which holds the colour
            // information for the pixel we want.
            _pixelData = (PixelData*)(_pBase + y * _width + x * sizeof(PixelData));
            // Create a Color instance from the colour information.
            return Color.FromArgb(_pixelData->alpha,
                                   _pixelData->red,
                                   _pixelData->green,
                                   _pixelData->blue);
        }
        #endregion

        #region GetPixelNext method
        /// <summary>
        /// Gets the colour of the next pixel in the image, or of the first 
        /// pixel in the image if this is the first GetPixel or GetPixelNext 
        /// call.
        /// Be sure to call the LockImage method before calling GetPixel or
        /// SetPixel for the first time.
        /// </summary>
        /// <returns>The colour of the pixel</returns>
        [SuppressMessage("Microsoft.Design",
                         "CA1024:UsePropertiesWhereAppropriate")]
        public Color GetPixelNext()
        {
            if (_pBase == null)
            {
                string message
                    = "The LockImage method must be called before the GetPixel method.";
                throw new InvalidOperationException(message);
            }
            _pixelData++;
            return Color.FromArgb(_pixelData->alpha,
                                   _pixelData->red,
                                   _pixelData->green,
                                   _pixelData->blue);
        }
        #endregion

        #region SetPixel method
        /// <summary>
        /// Sets the colour of the pixel at the specified co-ordinates.
        /// </summary>
        /// <param name="x">The horizontal co-ordinate</param>
        /// <param name="y">The vertical co-ordinate</param>
        /// <param name="colour">The colour to set the pixel to</param>
        [SuppressMessage("Microsoft.Naming",
                         "CA1704:IdentifiersShouldBeSpelledCorrectly",
                         MessageId = "1#y")]
        [SuppressMessage("Microsoft.Naming",
                         "CA1704:IdentifiersShouldBeSpelledCorrectly",
                         MessageId = "0#x")]
        public void SetPixel(int x, int y, int colour)
        {
            // The calls to CheckImageIsLocked and ValidateCoordinates slow
            // processing slightly but ensure that meaningful error messages are
            // returned to the caller in the event that the image isn't locked
            // into memory or the x or y co-ordinates are outside the bounds of
            // the image.
            if (_pBase == null)
            {
                string message
                    = "The LockImage method must be called before the SetPixel method.";
                throw new InvalidOperationException(message);
            }
            //ValidateCoordinates(x, y);
            if (x < 0 || x >= _imageWidth)
            {
                throw new ArgumentOutOfRangeException("x", "Something bad happened!");
            }
            if (y < 0 || y >= _imageHeight)
            {
                throw new ArgumentOutOfRangeException("y", "Something bad happened!");
            }
            // Get a pointer to the memory address holding the colour information
            // for the pixel.
            //PixelData* data = (PixelData*)(_pBase + y * _width + x * sizeof(PixelData));
            //*(_pBase + y * _width + x * sizeof(int)) = colour;
            int reverse = ((colour >> 24) & 0xFF) | (((colour >> 16) & 0xFF) << 8) | (((colour >> 8) & 0xFF) << 16) | ((colour & 0xFF) << 24);
            *(_pBase + x + y * _strideWidth) = colour;
            //PixelData* data = (PixelData*)(_pBase + x + y * sizeof(int));
            // Update the colour information at that address.
            //data->alpha = (byte)((colour >> 24) & 0xFF);
            //data->red = (byte)((colour >> 16) & 0xFF);
            //data->green = (byte)((colour >> 8) & 0xFF);
            //data->blue = (byte)((colour) & 0xFF);
        }
        #endregion

        #region UnlockImage method
        /// <summary>
        /// Unlocks the area of memory where the bitmap is held.
        /// Call this method when you have finished calling GetPixel and 
        /// SetPixel.
        /// </summary>
        public void UnlockImage()
        {
            _workingBitmap.UnlockBits(_bitmapData);
            _bitmapData = null;
            _pBase = null;
        }
        #endregion
    }
}
