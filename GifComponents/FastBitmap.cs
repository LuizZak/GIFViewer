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
        /// <summary>
        /// The bitmap we are working on.
        /// </summary>
        private Bitmap _workingBitmap;

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

        /// <summary>
        /// The width of the stride, in pixel units (Int32)
        /// </summary>
        private int _strideWidth;

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

        /// <summary>
        /// Locks the image data into memory.
        /// Call this before calling the GetPixel or SetPixel methods.
        /// </summary>
        public void LockImage()
        {
            Rectangle bounds = new Rectangle(Point.Empty, _workingBitmap.Size);

            _imageWidth = _workingBitmap.Width;
            _imageHeight = _workingBitmap.Height;

            // Lock image
            _bitmapData = _workingBitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            // Get a pointer to the first pixel in the bitmap.
            _pBase = (int*)_bitmapData.Scan0;

            // Calculate stride width
            _strideWidth = _bitmapData.Stride / 4;
        }

        /// <summary>
        /// Sets the colour of the pixel at the specified co-ordinates.
        /// </summary>
        /// <param name="x">The horizontal co-ordinate</param>
        /// <param name="y">The vertical co-ordinate</param>
        /// <param name="colour">The colour to set the pixel to</param>
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
            // Put the color pixel on the image data
            *(_pBase + x + y * _strideWidth) = colour;
        }

        /// <summary>
        /// Clears the bitmap with the given color
        /// </summary>
        /// <param name="color">The color to clear the bitmap with</param>
        public void Clear(int color)
        {
            // Clear all the pixels
            int count = _imageWidth * _imageHeight;
            int* curScan = _pBase;

            int rem = count % 8;

            count /= 8;

            while (count-- > 0)
            {
                *(curScan++) = color;
                *(curScan++) = color;
                *(curScan++) = color;
                *(curScan++) = color;

                *(curScan++) = color;
                *(curScan++) = color;
                *(curScan++) = color;
                *(curScan++) = color;
            }
            while (rem-- > 0)
            {
                *(curScan++) = color;
            }
        }
        
        /// <summary>
        /// Copies the contents of the given array of colors into this FastBitmap.
        /// Throws an ArgumentException if the count of colors on the array mismatches the pixel count from this FastBitmap
        /// </summary>
        /// <param name="colors">The array of colors to copy</param>
        /// <param name="ignoreZeroes">Whether to ignore zeroes when copying the data</param>
        public void CopyFromArray(int[] colors, bool ignoreZeroes)
        {
            if (colors.Length != _imageWidth * _imageHeight)
            {
                throw new ArgumentException("The number of colors of the given array mismatch the pixel count of the bitmap", "colors");
            }

            // Simply copy the argb values array
            int* s0t = _pBase;

            fixed (int *source = colors)
            {
                int* s0s = source;
                int bpp = 1; // Bytes per pixel

                int count = _imageWidth * _imageHeight * bpp;

                if (!ignoreZeroes)
                {
                    // Unfold the loop
                    const int sizeBlock = 8;
                    int rem = count % sizeBlock;

                    count /= sizeBlock;

                    while (count-- > 0)
                    {
                        *(s0t++) = *(s0s++);
                        *(s0t++) = *(s0s++);
                        *(s0t++) = *(s0s++);
                        *(s0t++) = *(s0s++);

                        *(s0t++) = *(s0s++);
                        *(s0t++) = *(s0s++);
                        *(s0t++) = *(s0s++);
                        *(s0t++) = *(s0s++);
                    }

                    while (rem-- > 0)
                    {
                        *(s0t++) = *(s0s++);
                    }
                }
                else
                {
                    // Unfold the loop
                    const int sizeBlock = 4;
                    int rem = count % sizeBlock;

                    count /= sizeBlock;

                    while (count-- > 0)
                    {
                        if (*(s0s) == 0) { s0t++; s0s++; continue; }
                        *(s0t++) = *(s0s++);

                        if (*(s0s) == 0) { s0t++; s0s++; continue; }
                        *(s0t++) = *(s0s++);

                        if (*(s0s) == 0) { s0t++; s0s++; continue; }
                        *(s0t++) = *(s0s++);

                        if (*(s0s) == 0) { s0t++; s0s++; continue; }
                        *(s0t++) = *(s0s++);
                    }

                    while (rem-- > 0)
                    {
                        *(s0t++) = *(s0s++);
                    }
                }
            }
        }

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
    }
}
