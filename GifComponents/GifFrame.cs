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
using System.Drawing;
using System.IO;

using GifComponents.Components;

namespace GifComponents
{
	/// <summary>
	/// A single image frame from a GIF file.
	/// </summary>
	public class GifFrame : GifComponents.Components.GifFrame
	{
		#region Constructor( Image )
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="theImage">
		/// The image held in this frame of the GIF file
		/// </param>
		public GifFrame( Image theImage ) : base( theImage )
		{}
		#endregion
		
		#region constructor( Stream, , , , ,  )
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
		public GifFrame( Stream inputStream,
		                 LogicalScreenDescriptor logicalScreenDescriptor,
		                 ColourTable globalColourTable,
		                 GraphicControlExtension graphicControlExtension,
		                 GifFrame previousFrame,
		                 GifFrame previousFrameBut1 )
			: base( inputStream, 
			        logicalScreenDescriptor, 
			        globalColourTable, 
			        graphicControlExtension, 
			        previousFrame, 
			        previousFrameBut1 )
		{
			// Just a public wrapper for the base constructor
		}
		#endregion
		
		#region constructor( Stream, , , , , bool )
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
		/// <param name="xmlDebugging">Whether or not to create debug XML</param>
		public GifFrame( Stream inputStream,
		                 LogicalScreenDescriptor logicalScreenDescriptor,
		                 ColourTable globalColourTable,
		                 GraphicControlExtension graphicControlExtension,
		                 GifFrame previousFrame,
		                 GifFrame previousFrameBut1, 
		                 bool xmlDebugging )
			: base( inputStream, 
			        logicalScreenDescriptor, 
			        globalColourTable, 
			        graphicControlExtension, 
			        previousFrame, 
			        previousFrameBut1,
			        xmlDebugging )
		{}
		#endregion
	}
}
