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
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;

using GIF_Viewer.GifComponents.Enums;
using GIF_Viewer.GifComponents.Types;

namespace GIF_Viewer.GifComponents.Components
{
    /// <summary>
    /// The base class for a component of a Graphics Interchange File data 
    /// stream.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class GifComponent : IDisposable
    {
        /// <summary>
        /// Plain text label - identifies the current block as a plain text
        /// extension.
        /// Value 0x01.
        /// TODO: add see cref once PlainTextExtension class implemented
        /// </summary>
        public const byte CodePlaintextLabel = 0x01;

        /// <summary>
        /// Extension introducer - identifies the start of an extension block.
        /// Value 0x21.
        /// </summary>
        public const byte CodeExtensionIntroducer = 0x21;

        /// <summary>
        /// Image separator - identifies the start of an 
        /// <see cref="ImageDescriptor"/>.
        /// Value 0x2C.
        /// </summary>
        public const byte CodeImageSeparator = 0x2C;

        /// <summary>
        /// Trailer - This is a single-field block indicating the end of the GIF
        /// data stream.
        /// Value 0x3B.
        /// </summary>
        public const byte CodeTrailer = 0x3B;

        /// <summary>
        /// Graphic control label - identifies the current block as a
        /// <see cref="GraphicControlExtension"/>.
        /// Value 0xF9.
        /// </summary>
        public const byte CodeGraphicControlLabel = 0xF9;

        /// <summary>
        /// Comment label - identifies the current block as a comment extension.
        /// Value 0xFE.
        /// TODO: add see cref once CommentExtension class is implemented.
        /// </summary>
        public const byte CodeCommentLabel = 0xFE;

        /// <summary>
        /// Application extension label - identifies the current block as a
        /// <see cref="ApplicationExtension"/>.
        /// Value 0xFF.
        /// </summary>
        public const byte CodeApplicationExtensionLabel = 0xFF;

        /// <summary>
        /// Constructor.
        /// This is implicitly called by constructors of derived types.
        /// </summary>
        protected GifComponent()
        {
            ComponentStatus = new GifComponentStatus(ErrorState.Ok, "");
        }

        #region public properties

        /// <summary>
        /// Gets the status of this component, consisting of its error state
        /// and any associated error message.
        /// </summary>
        [Category("Status")]
        [Description("Gets the status of this component, consisting of its " +
                     "error state and any associated error message.")]
        public GifComponentStatus ComponentStatus { get; private set; }

        /// <summary>
        /// Gets the member of the Gif.Components.ErrorState enumeration held 
        /// within the ComponentStatus property.
        /// </summary>
        [Browsable(false)]
        [Category("Status")]
        [Description("Gets the member of the Gif.Components.ErrorState " +
                     "enumeration held within the ComponentStatus property.")]
        public ErrorState ErrorState => ComponentStatus.ErrorState;

        /// <summary>
        /// Gets the combined error states of this component and all its child
        /// components.
        /// </summary>
        /// <remarks>
        /// This property uses reflection to inspect the runtime type of the
        /// current instance and performs a bitwise or of the ErrorStates of
        /// the current instance and of any GifComponents within it.
        /// </remarks>
        [Category("Status")]
        [Description("Gets the combined error states of this component and all its child components.")]
        public virtual ErrorState ConsolidatedState
        {
            get
            {
                var state = ErrorState;
                var properties = GetType().GetProperties();
                foreach (var property in properties)
                {
                    // We don't want to inspect the ConsolidatedState property
                    // else we get a StackOverflowException.
                    if (property.Name == "ConsolidatedState")
                    {
                        continue;
                    }

                    // Is this property an array?
                    if (property.PropertyType.IsArray)
                    {
                        // Is this property an array of GifComponents?
                        var componentArray = property.GetValue(this, null) as GifComponent[];
                        if (componentArray != null)
                        {
                            // It's an array of GifComponents, so inspect
                            // their ConsolidatedState properties.
                            foreach (var c in componentArray)
                            {
                                state |= c.ConsolidatedState;
                            }
                        }
                        continue;
                    }

                    // Is this property an indexer?
                    if (property.GetIndexParameters().Length > 0)
                    {
                        // it's probably an indexer, so ignore it
                        continue;
                    }

                    // Is this property of a type derived from GifComponent?
                    if (property.PropertyType.IsSubclassOf(typeof(GifComponent)))
                    {
                        // Yes, so it also has a ConsolidatedState property
                        var component = property.GetValue(this, null) as GifComponent;
                        if (component != null)
                        {
                            state |= component.ConsolidatedState;
                        }
                        continue;
                    }

                    // Is this property a generic type?
                    if (property.PropertyType.IsGenericType)
                    {
                        var objectCollection = property.GetValue(this, null) as IEnumerable;
                        if (objectCollection != null)
                        {
                            // Yes, it's IEnumerable, so iterate through its members
                            foreach (object o in objectCollection)
                            {
                                var c = o as GifComponent;
                                if (c != null)
                                {
                                    state |= c.ConsolidatedState;
                                }
                            }
                        }
                    }

                }
                return state;
            }
        }

        /// <summary>
        /// Gets any error message associated with the component's error state.
        /// </summary>
        [Browsable(false)]
        [Category("Status")]
        [Description("Gets any error message associated with the component's " +
                     "error state.")]
        public string ErrorMessage => ComponentStatus.ErrorMessage;

        #endregion

        /// <summary>
        /// Gets a string representation of the error status of this component
        /// and its subcomponents.
        /// </summary>
        /// <returns>
        /// A string representation of the error status of this component and
        /// its subcomponents.
        /// </returns>
        public override string ToString()
        {
            return ConsolidatedState.ToString();
        }

        /// <summary>
        /// Tests whether the error state of this component or any of its member
        /// components contains the supplied member of the ErrorState 
        /// enumeration.
        /// </summary>
        /// <param name="state">
        /// The error state to look for in the current instance's state.
        /// </param>
        /// <returns>
        /// True if the current instance's error state includes the supplied
        /// error state, otherwise false.
        /// </returns>
        public bool TestState(ErrorState state)
        {
            return (ConsolidatedState & state) == state;
        }

        /// <summary>
        /// Sets the ComponentStatus property of thie GifComponent.
        /// </summary>
        /// <param name="errorState">
        /// A member of the Gif.Components.ErrorState enumeration.
        /// </param>
        /// <param name="errorMessage">
        /// An error message associated with the error state.
        /// </param>
        protected void SetStatus(ErrorState errorState, string errorMessage)
        {
            var newState = ComponentStatus.ErrorState | errorState;
            string newMessage = ComponentStatus.ErrorMessage;
            if (!string.IsNullOrEmpty(newMessage))
            {
                newMessage += Environment.NewLine;
            }
            newMessage += errorMessage;
            ComponentStatus = new GifComponentStatus(newState, newMessage);
        }

        /// <summary>
        /// Converts the supplied integer to a 2-character hexadecimal value.
        /// </summary>
        /// <param name="value">The integer to convert</param>
        /// <returns>A 2-character hexadecimal value</returns>
        protected static string ToHex(int value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:X2}", value);
        }

        // TODO: consider moving methods for interacting with the GIF stream into their own class.

        /// <summary>
        /// Reads a single byte from the input stream and advances the position
        /// within the stream by one byte, or returns -1 if at the end of the
        /// stream.
        /// </summary>
        /// <param name="inputStream">
        /// The input stream to read.
        /// </param>
        /// <returns>
        /// The unsigned byte, cast to an Int32, or -1 if at the end of the 
        /// stream.
        /// </returns>
        protected static int Read(Stream inputStream)
        {
            return inputStream.ReadByte();
        }

        /// <summary>
        /// Reads next 16-bit value, least significant byte first, and advances 
        /// the position within the stream by two bytes.
        /// </summary>
        /// <param name="inputStream">
        /// The input stream to read.
        /// </param>
        /// <returns>
        /// The next two bytes in the stream, cast to an Int32, or -1 if at the 
        /// end of the stream.
        /// </returns>
        protected static int ReadShort(Stream inputStream)
        {
            // Least significant byte is first in the stream
            int leastSignificant = Read(inputStream);

            // Most significant byte is next - shift its value left by 8 bits
            int mostSignificant = Read(inputStream) << 8;

            // Use bitwise or to combine them to a short return value
            int returnValue = leastSignificant | mostSignificant;

            // Ensure the return value is -1 if the end of stream has been 
            // reached (if the first byte wasn't the end of stream then we'd
            // get a different negative number instead).
            if (returnValue < 0)
            {
                returnValue = -1;
            }

            return returnValue;
        }
        
        /// <summary>
        /// Skips variable length blocks up to and including next zero length 
        /// block (block terminator).
        /// </summary>
        /// <param name="inputStream">
        /// The input stream to read.
        /// </param>
        protected static void SkipBlocks(Stream inputStream)
        {
            while (DataBlock.SkipStream(inputStream) > 0) { }
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~GifComponent()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes resources used by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes resources used by this class.
        /// </summary>
        /// <param name="disposing">
        /// Indicates whether this method is being called by the class's Dispose
        /// method (true) or by the garbage collector (false).
        /// </param>
        protected virtual void Dispose(bool disposing)
        {

        }
    }
}
