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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;

namespace GifComponents.Components
{
	/// <summary>
	/// A global or local colour table which forms part of a GIF data stream.
	/// </summary>
	public class ColourTable : GifComponent
	{
		#region declarations
        /// <summary>
        /// The colours in the colour table in INT form
        /// </summary>
        private Collection<int> _intColours;
		#endregion
		
		#region constructors

		#region default constructor
		/// <summary>
		/// Constructor.
		/// </summary>
		public ColourTable()
		{
            _intColours = new Collection<int>();
		}
		#endregion
		
		#region constructor( Stream, int )
		/// <summary>
		/// Reads and returns a colour table from the supplied input stream.
		/// </summary>
		/// <param name="inputStream">
		/// The input stream to read.
		/// </param>
		/// <param name="numberOfColours">
		/// The number of colours the colour table is expected to contain.
		/// </param>
		public ColourTable( Stream inputStream, int numberOfColours )
			: this( inputStream, numberOfColours, false )
		{}
		#endregion

		#region constructor( Stream, int, bool )
		/// <summary>
		/// Reads and returns a colour table from the supplied input stream.
		/// </summary>
		/// <param name="inputStream">
		/// The input stream to read.
		/// </param>
		/// <param name="numberOfColours">
		/// The number of colours the colour table is expected to contain.
		/// </param>
		/// <param name="xmlDebugging">
		/// Whether or not to create debug XML
		/// </param>
		public ColourTable( Stream inputStream, 
		                    int numberOfColours, 
		                    bool xmlDebugging )
			: base( xmlDebugging )
		{
			string message 
				= "The number of colours must be between 0 and 256. "
				+ "Number supplied: " + numberOfColours;
			
			if( numberOfColours < 0 )
			{
				throw new ArgumentOutOfRangeException( "numberOfColours",
				                                       message );
			}
			
			if( numberOfColours > 256 )
			{
				throw new ArgumentOutOfRangeException( "numberOfColours", 
				                                       message );
			}
			
			int bytesExpected = numberOfColours * 3; // expected length of Colour table
			byte[] buffer = new byte[bytesExpected];
			int bytesRead = 0;
			bytesRead = inputStream.Read( buffer, 0, buffer.Length );
			int coloursRead = bytesRead / 3;

			int i = 0;
			int j = 0;
            _intColours = new Collection<int>();
			while( i < coloursRead )
			{
				int r = ((int) buffer[j++]) & 0xff;
				int g = ((int) buffer[j++]) & 0xff;
				int b = ((int) buffer[j++]) & 0xff;
                _intColours.Add(((255) << 24) | (r << 16) | (g << 8) | b);
				i++;
			}
		}
		#endregion

		#endregion

		#region properties

		#region Colours property
        public int[] IntColours
        {
            get
            {
                int[] colours = new int[Length];
                this._intColours.CopyTo(colours, 0);
                return colours;
            }
        }
		#endregion
		
		#region Length property
		/// <summary>
		/// Gets the number of colours in the colour table.
		/// </summary>
		public int Length
		{
			get { return _intColours.Count; }
		}
		#endregion
		
		#region SizeBits property
		/// <summary>
		/// Gets the number of bits required to hold the length of the colour
		/// table, minus 1.
		/// </summary>
		public int SizeBits
		{
			get
			{
				switch( Length )
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
						string message
							= "The colour table size is not an exact power of "
							+ "2. Did you forget to call the Pad() method?";
						throw new InvalidOperationException( message );
				}
			}
		}
		#endregion
		
		#endregion

		#region indexer
		/// <summary>
		/// Gets or sets the colour at the specified index in the colour table.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The supplied index is outside the bounds of the array.
		/// </exception>
		public int this[int index]
		{
			get
			{
				ValidateIndex( index );
				return _intColours[index];
			}
			set
			{
				ValidateIndex( index );
                _intColours[index] = value;
			}
		}
		#endregion
		
		#region methods

		#region public Add method
        public void Add(int colourToAdd)
        {
            _intColours.Add(colourToAdd);
        }
		#endregion
		
		#region public WriteToStream method
		/// <summary>
		/// Writes this component to the supplied output stream.
		/// </summary>
		/// <param name="outputStream">
		/// The output stream to write to.
		/// </param>
		public override void WriteToStream( Stream outputStream )
		{
			foreach( int c in _intColours )
			{
				WriteByte( c, outputStream );
			}
		}
		#endregion

		#region private ValidateIndex method
		private void ValidateIndex( int index )
		{
			if( index >= _intColours.Count || index < 0 )
			{
				string message
                    = "Colour table size: " + _intColours.Count
					+ ". Index: " + index;
				throw new ArgumentOutOfRangeException( "index", message );
			}
		}
		#endregion
		
		#endregion

	}
}
