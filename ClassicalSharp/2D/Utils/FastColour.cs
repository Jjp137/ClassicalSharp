﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using System.Drawing;
#if ANDROID
using AndroidColor = Android.Graphics.Color;
#endif

namespace ClassicalSharp {
	
	/// <summary> Structure that can be used for quick manipulations of A/R/G/B colours. </summary>
	/// <remarks> This structure is **not** suitable for interop with OpenGL or Direct3D. </remarks>
	public struct FastColour : IEquatable<FastColour> {
		
		public byte A, R, G, B;
		
		public FastColour( byte r, byte g, byte b, byte a ) {
			A = a; R = r; G = g; B = b;
		}
		
		public FastColour( int r, int g, int b, int a ) {
			A = (byte)a; R = (byte)r; G = (byte)g; B = (byte)b;
		}
		
		public FastColour( byte r, byte g, byte b ) {
			A = 255; R = r; G = g; B = b;
		}
		
		public FastColour( int r, int g, int b ) {
			A = 255; R = (byte)r; G = (byte)g; B = (byte)b;
		}
		
		public FastColour( int argb ) {
			A = (byte)(argb >> 24);
			R = (byte)(argb >> 16);
			G = (byte)(argb >> 8);
			B = (byte)argb;
		}

		public FastColour( Color c ) {
			A = c.A; R = c.R; G = c.G; B = c.B;
		}
		
		/// <summary> Multiplies the RGB components of this instance by the
		/// specified t parameter, where 0 ≤ t ≤ 1 </summary>
		public static FastColour Scale( FastColour value, float t ) {
			value.R = (byte)(value.R * t);
			value.G = (byte)(value.G * t);
			value.B = (byte)(value.B * t);
			return value;
		}
		
		/// <summary> Linearly interpolates the RGB components of the two colours
		/// by the specified t parameter, where 0 ≤ t ≤ 1 </summary>
		public static FastColour Lerp( FastColour a, FastColour b, float t ) {
			a.R = (byte)Utils.Lerp( a.R, b.R, t );
			a.G = (byte)Utils.Lerp( a.G, b.G, t );
			a.B = (byte)Utils.Lerp( a.B, b.B, t );
			return a;
		}
		
		public static FastColour GetHexEncodedCol( int hex, int lo, int hi ) {
			return new FastColour(
				lo * ((hex >> 2) & 1) + hi * (hex >> 3),
				lo * ((hex >> 1) & 1) + hi * (hex >> 3),
				lo * ((hex >> 0) & 1) + hi * (hex >> 3) );
		}
		
		public const float ShadeX = 0.6f, ShadeZ = 0.8f, ShadeYBottom = 0.5f;
		public static void GetShaded( FastColour normal, out int xSide, out int zSide, out int yBottom ) {
			xSide = FastColour.Scale( normal, ShadeX ).Pack();
			zSide = FastColour.Scale( normal, ShadeZ ).Pack();
			yBottom = FastColour.Scale( normal, ShadeYBottom ).Pack();
		}
		
		public Color ToColor() {
			return Color.FromArgb( A, R, G, B );
		}
		
		/// <summary> Packs this instance into a 32 bit integer, where A occupies
		/// the highest 8 bits and B occupies the lowest 8 bits. </summary>
		public int ToArgb() {
			return A << 24 | R << 16 | G << 8 | B;
		}
		
		public int Pack() {
			#if !USE_DX
			return A << 24 | B << 16 | G << 8 | R;
			#else
			return A << 24 | R << 16 | G << 8 | B;
			#endif
		}
		
		public override bool Equals( object obj ) {
			return (obj is FastColour) && Equals( (FastColour)obj );
		}
		
		/// <summary> Returns whether all of the colour components of this instance
		/// equal that of the other instance. </summary>
		public bool Equals( FastColour other ) {
			return A == other.A && R == other.R && G == other.G && B == other.B;
		}
		
		public override int GetHashCode() {
			return A << 24 | R << 16 | G << 8 | B;
		}
		
		/// <summary> Convers this instance into a hex colour code of the form RRGGBB. </summary>
		public string ToRGBHexString() {
			return Utils.ToHexString( new byte[] { R, G, B } );
		}
		
		public override string ToString() {
			return R + ", " + G + ", " + B + " : " + A;
		}
		

		public static bool operator == ( FastColour left, FastColour right ) {
			return left.Equals( right );
		}
		
		public static bool operator != ( FastColour left, FastColour right ) {
			return !left.Equals( right );
		}
		
		public static implicit operator FastColour( Color col ) {
			return new FastColour( col );
		}
		
		public static implicit operator Color( FastColour col ) {
			return Color.FromArgb( col.A, col.R, col.G, col.B );
		}

		#if ANDROID
		public static implicit operator AndroidColor( FastColour col ) {
			return AndroidColor.Argb( col.A, col.R, col.G, col.B );
		}
		#endif
		
		public static FastColour Red = new FastColour( 255, 0, 0 );
		public static FastColour Green = new FastColour( 0, 255, 0 );
		public static FastColour Blue = new FastColour( 0, 0, 255 );
		
		public static FastColour White = new FastColour( 255, 255, 255 );
		public static FastColour Black = new FastColour( 0, 0, 0 );
		public const int WhitePacked = unchecked((int)0xFFFFFFFF);
		public const int BlackPacked = unchecked((int)0xFF000000);

		public static FastColour Yellow = new FastColour( 255, 255, 0 );
		public static FastColour Magenta = new FastColour( 255, 0, 255 );
		public static FastColour Cyan = new FastColour( 0, 255, 255 );
		
		public static bool TryParse( string input, out FastColour value ) {
			value = default( FastColour );
			if( input == null || input.Length < 6 ) return false;
			
			try {
				int i = input.Length > 6 ? 1 : 0;
				if( input.Length > 6 && (input[0] != '#' || input.Length > 7) )
					return false;
				
				int r = Utils.ParseHex( input[i + 0] ) * 16 + Utils.ParseHex( input[i + 1] );
				int g = Utils.ParseHex( input[i + 2] ) * 16 + Utils.ParseHex( input[i + 3] );
				int b = Utils.ParseHex( input[i + 4] ) * 16 + Utils.ParseHex( input[i + 5] );
				value = new FastColour( r, g, b );
				return true;
			} catch( FormatException ) {
				return false;
			}
		}
		
		public static FastColour Parse( string input ) {
			int i = input.Length > 6 ? 1 : 0;
			
			int r = Utils.ParseHex( input[i + 0] ) * 16 + Utils.ParseHex( input[i + 1] );
			int g = Utils.ParseHex( input[i + 2] ) * 16 + Utils.ParseHex( input[i + 3] );
			int b = Utils.ParseHex( input[i + 4] ) * 16 + Utils.ParseHex( input[i + 5] );
			return new FastColour( r, g, b );
		}
	}
}
