#region Copyright (C) Simon Bridewell
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

using System;
using System.IO;

namespace GIF_Viewer.GifComponents.Components
{
    /// <summary>
    /// A global or local color table which forms part of a GIF data stream.
    /// </summary>
    public class ColorTable : GifComponent
    {
        /// <summary>
        /// The colors in the color table in INT form
        /// </summary>
        public int[] IntColors { get; }

        /// <summary>
        /// Creates an empty color table
        /// </summary>
        public ColorTable(int colorCount)
        {
            IntColors = new int[colorCount];
        }

        /// <summary>
        /// Reads and returns a color table from the supplied input stream.
        /// </summary>
        /// <param name="inputStream">
        /// The input stream to read.
        /// </param>
        /// <param name="numberOfColors">
        /// The number of colors the color table is expected to contain.
        /// </param>
        public unsafe ColorTable(Stream inputStream, int numberOfColors)
        {
            if (numberOfColors < 0 || numberOfColors > 256)
            {
                string message
                    = "The number of colors must be between 0 and 256. "
                      + "Number supplied: " + numberOfColors;

                throw new ArgumentOutOfRangeException(nameof(numberOfColors),
                                                       message);
            }

            int bytesExpected = numberOfColors * 3; // expected length of Color table
            byte[] buffer = new byte[bytesExpected];
            int bytesRead = inputStream.Read(buffer, 0, buffer.Length);
            int colorsRead = bytesRead / 3;
            
            int j = 0;
            IntColors = new int[colorsRead];

            fixed (byte* pBuffer = buffer)
            fixed (int* pIntColors = IntColors)
            {
                for (int i = 0; i < colorsRead; i++)
                {
                    byte r = pBuffer[j++];
                    byte g = pBuffer[j++];
                    byte b = pBuffer[j++];
                    pIntColors[i] = (255 << 24) | (r << 16) | (g << 8) | b;
                }
            }
        }

        /// <summary>
        /// Gets the number of colors in the color table.
        /// </summary>
        public int Length => IntColors.Length;

        /// <summary>
        /// Gets the number of bits required to hold the length of the color
        /// table, minus 1.
        /// </summary>
        public int SizeBits
        {
            get
            {
                switch (Length)
                {
                    case 256:
                        return 7;

                    case 128:
                        return 6;

                    case 64:
                        return 5;

                    case 32:
                        return 4;

                    case 16:
                        return 3;

                    case 8:
                        return 2;

                    case 4:
                        return 1;

                    default:
                        const string message = "The color table size is not an exact power of 2. Did you forget to call the Pad() method?";
                        throw new InvalidOperationException(message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the color at the specified index in the color table.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The supplied index is outside the bounds of the array.
        /// </exception>
        public int this[int index]
        {
            get
            {
                ValidateIndex(index);
                return IntColors[index];
            }
            set
            {
                ValidateIndex(index);
                IntColors[index] = value;
            }
        }
        
        private void ValidateIndex(int index)
        {
            if (index >= IntColors.Length || index < 0)
            {
                string message
                    = "Color table size: " + IntColors.Length
                    + ". Index: " + index;
                throw new ArgumentOutOfRangeException(nameof(index), message);
            }
        }
        
        /// <summary>
        /// Skips a whole color table block on a given stream
        /// </summary>
        /// <param name="inputStream">The input stream</param>
        /// <param name="colorCount">The number of colors to skip</param>
        public static void SkipOnStream(Stream inputStream, int colorCount)
        {
            int bytesExpected = colorCount * 3; // expected length of Color table
            inputStream.Position += bytesExpected;
        }
    }
}