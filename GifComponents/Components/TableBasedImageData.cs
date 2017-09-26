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

#region changes
/*
 Adapted from John Cristy's ImageMagick.
 Decodes LZW image data into pixel array and returns table-based 
 image data - see http://www.w3.org/Graphics/GIF/spec-gif89a.txt
 section 22.
 
 Amended by Simon Bridewell - July 2009 - January 2012
 	Extracted this logic from GifDecoder.cs
 	Added logical properties
 	Derive from GifComponent in order to make use of component status
 	Use stronger types than just byte where appropriate
*/
#endregion

using System;
using System.Collections.Generic;

using System.IO;
using System.Runtime.InteropServices;
using GIF_Viewer.GifComponents.Enums;

namespace GIF_Viewer.GifComponents.Components
{
	/// <summary>
	/// The image data for a table based image consists of a sequence of 
	/// sub-blocks, of size at most 255 bytes each, containing an index into 
	/// the active color table, for each pixel in the image.  
	/// Pixel indices are in order of left to right and from top to bottom.  
	/// Each index must be within the range of the size of the active color 
	/// table, starting at 0.
	/// See http://www.w3.org/Graphics/GIF/spec-gif89a.txt section 22
	/// </summary>
    public class TableBasedImageData : GifComponent
    {
        private const int MaxStackSize = 4096; // max decoder pixel stack size
        private const int NullCode = -1; // indicates no previous code has been read yet

        /// <summary>
        /// An array of indices to colours in the active colour table,
        /// representing the pixels of a frame in a GIF data stream.
        /// </summary>
        private readonly byte[] _pixelIndexes;

        /// <summary>
        /// Determines the initial number of bits used for LZW codes 
        /// in the image data.
        /// </summary>
        private readonly int _lzwMinimumCodeSize;

	    /// <summary>
	    /// Constructor.
	    /// </summary>
	    /// <param name="inputStream">
	    /// The stream from which the image data is to be read, starting with
	    /// the LZW minimum code size, and ending with a block terminator.
	    /// </param>
	    /// <param name="pixelCount">
	    /// Number of pixels in the image.
	    /// </param>
	    /// <remarks>
	    /// The input stream is read, first into the LZW minimum code size, then
	    /// into data blocks. Bytes are extracted from the data blocks into a
	    /// datum until the datum contains enough bits to form a code; this code
	    /// is then extracted from the datum and decoded into a pixel index.
	    /// Once all data has been read, or a block terminator, 
	    /// end-of-information code or error condition is encountered, any
	    /// remaining pixel indices not already populated default to zero.
	    /// </remarks>
	    public TableBasedImageData(Stream inputStream, int pixelCount)
	    {
            #region guard against silly image sizes

            if (pixelCount < 1)
            {
                string message
                    = "The pixel count must be greater than zero. "
                      + "Supplied value was " + pixelCount;
                throw new ArgumentOutOfRangeException(nameof(pixelCount), message);
            }

            #endregion

            #region declare / initialise local variables

            _pixelIndexes = new byte[pixelCount];
	        int code;
            int datum = 0; // temporary storage for codes read from the input stream
            int meaningfulBitsInDatum = 0; // number of bits of useful information held in the datum variable
            int firstCode = 0; // first code read from the stream since last clear code
            int indexInDataBlock = 0;
            int pixelIndex;

            // number of bytes still to be extracted from the current data block
            int bytesToExtract = 0;

            short[] prefix = new short[MaxStackSize];
            byte[] suffix = new byte[MaxStackSize];
            var pixelStack = new Stack<byte>();

            #endregion

            //  Initialize GIF data stream decoder.
            _lzwMinimumCodeSize = Read(inputStream); // number of bits initially used for LZW codes in image data
            int clearCode = ClearCode;
	        int endOfInformation = EndOfInformation;
            int nextAvailableCode = clearCode + 2;
	        int previousCode = NullCode;
	        int currentCodeSize = InitialCodeSize;

            #region guard against LZW code size being too large

            if (clearCode >= MaxStackSize)
            {
                string message
                    = "LZW minimum code size: " + _lzwMinimumCodeSize
                      + ". Clear code: " + clearCode
                      + ". Max stack size: " + MaxStackSize;
                SetStatus(ErrorState.LzwMinimumCodeSizeTooLarge, message);
                return;
            }

            #endregion

            // TODO: what are prefix and suffix and why are we initialising them like this?
            for (code = 0; code < clearCode; code++)
            {
                suffix[code] = (byte)code;
            }

            #region decode LZW image data

            // Initialise block to an empty data block. This will be overwritten
            // first time through the loop with a data block read from the input
            // stream.
            var block = new DataBlock(0, new byte[0]);

            for (pixelIndex = 0; pixelIndex < pixelCount;)
            {
                if (pixelStack.Count == 0)
                {
                    // There are no pixels in the stack at the moment, so...

                    #region get some pixels and put them on the stack

                    if (meaningfulBitsInDatum < currentCodeSize)
                    {
                        // Then we don't have enough bits in the datum to make
                        // a code; we need to get some more from the current
                        // data block, or we may need to read another data
                        // block from the input stream

                        #region get another byte from the current data block

                        if (bytesToExtract == 0)
                        {
                            // Then we've extracted all the bytes from the 
                            // current data block, so...

                            #region	read the next data block from the stream

                            block = ReadDataBlock(inputStream);
                            bytesToExtract = block.ActualBlockSize;

                            // Point to the first byte in the new data block
                            indexInDataBlock = 0;

                            if (block.TestState(ErrorState.DataBlockTooShort))
                            {
                                // then we've reached the end of the stream
                                // prematurely
                                break;
                            }

                            if (bytesToExtract == 0)
                            {
                                // then it's a block terminator, end of the
                                // image data (this is a data block other than
                                // the first one)
                                break;
                            }

                            #endregion
                        }
                        // Append the contents of the current byte in the data 
                        // block to the beginning of the datum
                        int newDatum = block[indexInDataBlock] << meaningfulBitsInDatum;
                        datum += newDatum;

                        // so we've now got 8 more bits of information in the
                        // datum.
                        meaningfulBitsInDatum += 8;

                        // Point to the next byte in the data block
                        indexInDataBlock++;

                        // We've one less byte still to read from the data block
                        // now.
                        bytesToExtract--;

                        // and carry on reading through the data block
                        continue;

                        #endregion
                    }

                    #region get the next code from the datum

                    // Get the least significant bits from the read datum, up
                    // to the maximum allowed by the current code size.
                    code = datum & GetMaximumPossibleCode(currentCodeSize);

                    // Drop the bits we've just extracted from the datum.
                    datum >>= currentCodeSize;

                    // Reduce the count of meaningful bits held in the datum
                    meaningfulBitsInDatum -= currentCodeSize;

                    #endregion

                    #region interpret the code

                    #region end of information?
                    if (code == endOfInformation)
                    {
                        // We've reached an explicit marker for the end of the
                        // image data.
                        break;
                    }

                    #endregion

                    #region code not in dictionary?

                    if (code > nextAvailableCode)
                    {
                        // We expect the code to be either one which is already
                        // in the dictionary, or the next available one to be
                        // added. If it's neither of these then abandon 
                        // processing of the image data.
                        string message
                            = "Next available code: " + nextAvailableCode
                              + ". Last code read from input stream: " + code;
                        SetStatus(ErrorState.CodeNotInDictionary, message);
                        break;
                    }

                    #endregion

                    #region clear code?

                    if (code == clearCode)
                    {
                        // We can get a clear code at any point in the image
                        // data, this is an instruction to reset the decoder
                        // and empty the dictionary of codes.
                        currentCodeSize = InitialCodeSize;
                        nextAvailableCode = ClearCode + 2;
                        previousCode = NullCode;

                        // Carry on reading from the input stream.
                        continue;
                    }

                    #endregion

                    #region first code since last clear code?

                    if (previousCode == NullCode)
                    {
                        // This is the first code read since the start of the
                        // image data or the most recent clear code.
                        // There's no previously read code in memory yet, so
                        // get the pixel index for the current code and add it
                        // to the stack.
                        pixelStack.Push(suffix[code]);
                        previousCode = code;
                        firstCode = code;

                        // and carry on to the next pixel
                        continue;
                    }

                    #endregion

                    var inCode = code;
                    if (code == nextAvailableCode)
                    {
                        pixelStack.Push((byte)firstCode);
                        code = previousCode;
                    }

                    while (code > clearCode)
                    {
                        pixelStack.Push(suffix[code]);
                        code = prefix[code];
                    }

                    #endregion

                    firstCode = (suffix[code]) & 0xff;

                    pixelStack.Push((byte)firstCode);

                    #region add a new string to the string table

                    // This fix is based off of ImageSharp's LzwDecoder.cs:
                    // https://github.com/SixLabors/ImageSharp/blob/8899f23c1ddf8044d4dea7d5055386f684120761/src/ImageSharp/Formats/Gif/LzwDecoder.cs

                    // Fix for Gifs that have "deferred clear code" as per here :
                    // https://bugzilla.mozilla.org/show_bug.cgi?id=55918
                    if (nextAvailableCode < MaxStackSize)
                    {
                        // TESTME: constructor - next available code >- _maxStackSize
                        prefix[nextAvailableCode] = (short)previousCode;
                        suffix[nextAvailableCode] = (byte)firstCode;
                        nextAvailableCode++;

                        #endregion

                        #region do we need to increase the code size?

                        if ((nextAvailableCode & GetMaximumPossibleCode(currentCodeSize)) == 0)
                        {
                            // We've reached the largest code possible for this size
                            if (nextAvailableCode < MaxStackSize)
                            {
                                // so increase the code size by 1
                                currentCodeSize++;
                            }
                        }

                        #endregion
                    }

                    previousCode = inCode;

                    #endregion
                }

                // Pop all the pixels currently on the stack off, and add them
                // to the return value.
                _pixelIndexes[pixelIndex] = pixelStack.Pop();
                pixelIndex++;
            }

            #endregion

            #region check input stream contains enough data to fill the image

            if (pixelIndex < pixelCount)
            {
                string message
                    = "Expected pixel count: " + pixelCount
                      + ". Actual pixel count: " + pixelIndex;
                SetStatus(ErrorState.TooFewPixelsInImageData, message);
            }

            #endregion

        }

        /// <summary>
        /// Gets an array of indices to colours in the active colour table,
        /// representing the pixels of a frame in a GIF data stream.
        /// </summary>
        public byte[] PixelIndexes => _pixelIndexes;

	    /// <summary>
        /// Determines the initial number of bits used for LZW codes in the 
        /// image data.
        /// This is read from the first available byte in the input stream.
        /// </summary>
        public int LzwMinimumCodeSize => _lzwMinimumCodeSize;

	    /// <summary>
        /// A special Clear code is defined which resets all compression / 
        /// decompression parameters and tables to a start-up state. 
        /// The value of this code is 2 ^ code size. 
        /// For example if the code size indicated was 4 (image was 4 bits/pixel)
        /// the Clear code value would be 16 (10000 binary). 
        /// The Clear code can appear at any point in the image data stream and 
        /// therefore requires the LZW algorithm to process succeeding codes as 
        /// if a new data stream was starting. 
        /// Encoders should output a Clear code as the first code of each image 
        /// data stream.
        /// </summary>
        public int ClearCode => 1 << _lzwMinimumCodeSize;

	    /// <summary>
        /// Gets the size in bits of the first code to add to the dictionary.
        /// </summary>
        public int InitialCodeSize => _lzwMinimumCodeSize + 1;

	    /// <summary>
        /// Gets the code which explicitly marks the end of the image data in
        /// the stream.
        /// </summary>
        public int EndOfInformation => ClearCode + 1;

	    /// <summary>
        /// Gets the highest possible code for the supplied code size - when
        /// all bits in the code are set to 1.
        /// This is used as a bitmask to extract the correct number of least 
        /// significant bits from the datum to form a code.
        /// </summary>
        /// <param name="currentCodeSize"></param>
        /// <returns></returns>
        private static int GetMaximumPossibleCode(int currentCodeSize)
        {
            return (1 << currentCodeSize) - 1;
        }

        private DataBlock ReadDataBlock(Stream inputStream)
        {
            DataBlock block = new DataBlock(inputStream);
            return block;
        }

        /// <summary>
        /// Returns the size of a table-based image data section on the given stream.
        /// This method keeps the current position of the stream unmodified after it finishes
        /// </summary>
        /// <param name="inputStream">The input stream</param>
        /// <returns>The length of the table-based image data on the stream, in bytes</returns>
        public static long GetDataSizeOnStream(Stream inputStream)
        {
            long startStreamPosition = inputStream.Position;

            //  Initialize GIF data stream decoder.
            Read(inputStream);
            //ReadDataBlockStatic(inputStream);
            SkipBlocksStatic(inputStream);

            var endStreamPosition = inputStream.Position;
            inputStream.Position = startStreamPosition;

            return endStreamPosition - startStreamPosition;
        }

        /// <summary>
        /// Skips a whole TableBasedImageData block on a given stream
        /// </summary>
        /// <param name="inputStream">The input stream</param>
        public static void SkipOnStream(Stream inputStream)
        {
            inputStream.Position += GetDataSizeOnStream(inputStream);
        }

        /// <summary>
        /// Writes this component to the supplied output stream.
        /// </summary>
        /// <param name="outputStream">
        /// The output stream to write to.
        /// </param>
        public override void WriteToStream(Stream outputStream)
        {
            throw new NotImplementedException();
        }
    }
}