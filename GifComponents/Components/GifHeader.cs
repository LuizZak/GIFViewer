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

using System.ComponentModel;
using System.IO;
using System.Text;
using GIF_Viewer.GifComponents.Enums;

namespace GIF_Viewer.GifComponents.Components
{
    /// <summary>
    /// The header section of a Graphics Interchange Format stream.
    /// See http://www.w3.org/Graphics/GIF/spec-gif89a.txt section 17.
    /// </summary>
    /// <remarks>
    /// The Header identifies the GIF Data Stream in context. The Signature 
    /// field marks the beginning of the Data Stream, and the Version field 
    /// identifies the set of capabilities required of a decoder to fully 
    /// process the Data Stream.
    /// This block is REQUIRED; exactly one Header must be present per Data 
    /// Stream.
    /// </remarks>
    public class GifHeader : GifComponent
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="signature">
        /// The GIF signature which identifies a GIF stream.
        /// Should contain the fixed value "GIF".
        /// </param>
        /// <param name="gifVersion">
        /// The version of the GIF standard used by this stream.
        /// </param>
        public GifHeader(string signature, string gifVersion)
        {
            Signature = signature;
            Version = gifVersion;

            if (Signature != "GIF")
            {
                string errorInfo = "Bad signature: " + Signature;
                const ErrorState status = ErrorState.BadSignature;
                SetStatus(status, errorInfo);
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inputStream">
        /// A <see cref="System.IO.Stream"/> containing the data to create the
        /// GifHeader.
        /// </param>
        public GifHeader(Stream inputStream)
        {

            var sb = new StringBuilder();
            // Read 6 bytes from the GIF stream
            // These should contain the signature and GIF version.
            bool endOfFile = false;
            for (int i = 0; i < 6; i++)
            {
                int nextByte = Read(inputStream);
                if (nextByte == -1)
                {
                    if (endOfFile == false)
                    {
                        SetStatus(ErrorState.EndOfInputStream,
                                   "Bytes read: " + i);
                        endOfFile = true;
                    }
                    nextByte = 0;
                }
                sb.Append((char)nextByte);
            }

            string headerString = sb.ToString();

            Signature = headerString.Substring(0, 3);
            Version = headerString.Substring(3, 3);
            if (Signature != "GIF")
            {
                string errorInfo = "Bad signature: " + Signature;
                const ErrorState status = ErrorState.BadSignature;
                SetStatus(status, errorInfo);
            }
        }

        /// <summary>
        /// Gets the signature which introduces the GIF stream.
        /// This should contain the fixed value "GIF".
        /// </summary>
        [Description("The signature which introduces the GIF stream. " +
                     "This should contain the fixed value \"GIF\".")]
        public string Signature { get; }

        /// <summary>
        /// Gets the version of the Graphics Interchange Format used by the GIF 
        /// stream which contains this header.
        /// </summary>
        [Description("The version of the Graphics Interchange Format used " +
                     "by the GIF stream which contains this header.")]
        public string Version { get; }
    }
}