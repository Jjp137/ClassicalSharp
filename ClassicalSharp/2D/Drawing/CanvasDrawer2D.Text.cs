﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
#if ANDROID
using System;
using ClassicalSharp.GraphicsAPI;
using Android.Graphics;
using Android.Graphics.Drawables;
using System.Drawing;

namespace ClassicalSharp {

	public sealed partial class CanvasDrawer2D {
		
		Bitmap measuringBmp;
		Canvas measuringC;
		
		public CanvasDrawer2D( IGraphicsApi graphics ) {
			this.graphics = graphics;		
			measuringBmp = Bitmap.CreateBitmap( 1, 1, Bitmap.Config.Argb8888 );
			measuringC = new Canvas( measuringBmp );
		}

		public override void DrawText( ref DrawTextArgs args, int x, int y ) {
			if( !args.SkipPartsCheck )
				GetTextParts( args.Text );
			
			Paint shadowBrush = GetOrCreateBrush( Color.Black );
			float textX = x;
			for( int i = 0; i < parts.Count; i++ ) {
				TextPart part = parts[i];
				Paint textBrush = GetOrCreateBrush( part.TextColour );
				if( args.UseShadow )
					c.DrawText( part.Text, textX + Offset, y + Offset, shadowBrush );
				
				c.DrawText( part.Text, textX, y, textBrush );
				textX += textBrush.MeasureText( part.Text );
			}
		}
		
		public override void DrawClippedText( ref DrawTextArgs args, int x, int y, float maxWidth, float maxHeight ) {
			throw new NotImplementedException();
		}
		
		FastBitmap bitmapWrapper = new FastBitmap();
		public override void DrawBitmappedText( ref DrawTextArgs args, int x, int y ) {
			using( bitmapWrapper ) {
				bitmapWrapper.SetData( curBmp, true, false );
				DrawBitmappedTextImpl( bitmapWrapper, ref args, x, y );
			}
		}
		
		public override Size MeasureSize( ref DrawTextArgs args ) {
			GetTextParts( args.Text );
			if( parts.Count == 0 )
				return Size.Empty;
			
			SizeF total = SizeF.Empty;
			for( int i = 0; i < parts.Count; i++ ) {
				TextPart part = parts[i];
				Paint textBrush = GetOrCreateBrush( part.TextColour );
				total.Width += textBrush.MeasureText( part.Text );
			}
			total.Height = PtToPx( args.Font.Size );
			if( args.UseShadow ) {
				total.Width += Offset; total.Height += Offset;
			}
			return Size.Ceiling( total );
		}
		
		public override Size MeasureBitmappedSize( ref DrawTextArgs args ) {
			return MeasureBitmappedSizeImpl( ref args );
		}
		
		void DisposeText() {
			measuringC.Dispose();
			measuringBmp.Dispose();
		}
	}
}
#endif