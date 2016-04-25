using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SDL2;

using ClassicalSharp;

// TODO: remove these
using OpenTK;
using OpenTK.Input;
using OpenTK.Platform;
using System.Drawing;

namespace ClassicalSharp
{
	public class SDL2Window : IDisposable
	{
		public int Width {
			get {
				int w, h;
				SDL.SDL_GetWindowSize( this.window, out w, out h );
				return w;
			}
		}

		public int Height { 
			get {
				int w, h;
				SDL.SDL_GetWindowSize( this.window, out w, out h );
				return h;
			}
		}

		public Size ClientSize {
			get {
				int w, h;
				SDL.SDL_GetWindowSize( this.window, out w, out h );
				return new Size( w, h );
			}
		}

		protected bool exists;
		public bool Exists {
			get { return this.exists; }
		}

		private bool focused;
		public bool Focused {
			get {
				return this.focused;
			}
		}

		private bool visible;
		public bool Visible {
			get {
				return visible;
			}
			set {
				this.visible = value;
				if ( value ) {
					SDL.SDL_ShowWindow( this.window );
				}
				else {
					SDL.SDL_HideWindow( this.window );
				}
			}
		}

		public Point DesktopCursorPos {
			// FIXME: SDL 2.0.4 makes this easier, but Debian only has 2.0.2
			get {
				int win_x, win_y, mouse_x, mouse_y;
				SDL.SDL_GetWindowPosition( this.window, out win_x, out win_y );
				SDL.SDL_GetMouseState( out mouse_x, out mouse_y );

				return new Point( win_x + mouse_x, win_y + mouse_y );
			}
			set {
				int win_x, win_y;

				SDL.SDL_GetWindowPosition( this.window, out win_x, out win_y );
				SDL.SDL_WarpMouseInWindow( this.window, value.X - win_x, value.Y - win_y );
				// Force the mouse pointer to move since otherwise it won't update its position until the next
				// call to SDL.SDL_PollEvent(); this is needed because the Camera uses its own mouse grabbing
				// mechanism that depends on the change to mouse position immediately being applied
				SDL.SDL_PumpEvents();
			}
		}

		protected MouseDevice mouse;
		public MouseDevice Mouse {
			get { return mouse; }
		}

		protected KeyboardDevice keyboard;
		public KeyboardDevice Keyboard {
			get { return keyboard; }
		}
		
		public Point PointToScreen( Point coords ) {
			// FIXME: SDL 2.0.4 makes this easier, but Debian only has 2.0.2
			int win_x, win_y;
			SDL.SDL_GetWindowPosition( this.window, out win_x, out win_y );
			return new Point( coords.X + win_x, coords.Y + win_y );
		}
		
		public WindowState WindowState {
			get {
				uint flags = SDL.SDL_GetWindowFlags( this.window );

				if( ( flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_MINIMIZED ) != 0 ) {
					return WindowState.Minimized;
				}
				else if( ( flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED ) != 0 ) {
					return WindowState.Maximized;
				}
				else if( ( flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP ) != 0 ) {
					return WindowState.Fullscreen;
				}
				else {
					return WindowState.Normal;
				}
			}
			set {
				WindowState current = this.WindowState;

				if( value == WindowState.Minimized && current != WindowState.Minimized ) {
					SDL.SDL_MinimizeWindow( this.window );
				}
				else if( value == WindowState.Maximized && current != WindowState.Maximized ) {
					SDL.SDL_MaximizeWindow( this.window );
				}
				else if( value == WindowState.Fullscreen && current != WindowState.Fullscreen ) {
					// I guess desktop fullscreen is desired since that's what it does with OpenTK on Linux
					SDL.SDL_SetWindowFullscreen( this.window, (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP );

					// There is no "changed from/to fullscreen" specific SDL event, so handle it here
					UpdateSurfacePointer();
					if( WindowStateChanged != null ) {
						WindowStateChanged ( this, new EventArgs() );
					}
				}
				else {  // WindowState.Normal
					if( current == WindowState.Minimized || current == WindowState.Maximized ) {
						SDL.SDL_RestoreWindow( this.window );
					}
					else if( current == WindowState.Fullscreen ) {
						SDL.SDL_SetWindowFullscreen( this.window, 0 );

						// Same reasoning here
						UpdateSurfacePointer();
						if( WindowStateChanged != null ) {
							WindowStateChanged ( this, new EventArgs() );
						}
					}
				}
			}
		}
		
		Icon currentIcon = null;
		public Icon Icon {
			get {
				return currentIcon;
			}
			set {
				currentIcon = value;

				// TODO: actually implement it; currently it's a no-op
				//Bitmap bitmap = currentIcon.ToBitmap();

				//SDL.SDL_SetWindowIcon( this.window, IntPtr.Zero );
			}
		}

		public IWindowInfo WindowInfo { get { return windowInfo; } }
		
		public string ClipboardText { 
			get {
				string text = String.Empty;
				if( SDL.SDL_HasClipboardText() == SDL.SDL_bool.SDL_TRUE ) {
					// FIXME: How do we call SDL_free() from this?
					text = SDL.SDL_GetClipboardText();
					if( text == null ) {
						throw new InvalidOperationException( "SDL_GetClipboardText failed: " + SDL.SDL_GetError() );
					}
				}
				return text;
			}
			set {
				if( SDL.SDL_SetClipboardText( value ) < 0 ) {
					throw new InvalidOperationException( "SDL_SetClipboardText failed: " + SDL.SDL_GetError() );
				}
			}
		}

		public event EventHandler<EventArgs> Resize;
		public event EventHandler<EventArgs> WindowStateChanged;
		public event EventHandler<EventArgs> FocusedChanged;
		public event EventHandler<KeyPressEventArgs> KeyPress;

		protected IntPtr window;
		private IntPtr surface;
		protected SDL2WindowInfo windowInfo;

		protected bool disposed;

		public SDL2Window( int width, int height, string title, SDL.SDL_WindowFlags flags )
		{
			SDL.SDL_version version;
			SDL.SDL_GetVersion( out version );
			Utils.LogDebug( "Using SDL version: " + version.major + "." + version.minor + "." + version.patch );

			if( SDL.SDL_COMPILEDVERSION > SDL.SDL_VERSIONNUM( version.major, version.minor, version.patch ) ) {
				throw new InvalidOperationException( "SDL version must be at least 2.0.4" );
			}

			int success = SDL.SDL_Init( SDL.SDL_INIT_TIMER | SDL.SDL_INIT_VIDEO );

			if( success != 0 ) {
				throw new InvalidOperationException( "SDL_Init failed: " + SDL.SDL_GetError() );
			}

			this.window = SDL.SDL_CreateWindow(
				title + " - SDL2 ",
				// Let the operating system's window manager decide where to put the window
				SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED,
				width, height, flags
				);

			if( this.window == IntPtr.Zero ) {
				throw new InvalidOperationException( "SDL_CreateWindow failed: " + SDL.SDL_GetError() );
			}

			this.UpdateSurfacePointer();
			
			this.exists = true;
			this.visible = true;
			this.focused = true;

			this.windowInfo = new SDL2WindowInfo( this.window );

			this.keyboard = new KeyboardDevice();
			this.mouse = new MouseDevice();

			// TODO: Having text input enabled adds overhead, so it should be enabled when necessary (such as
			// when entering text in a chat box) and disabled otherwise
			SDL.SDL_StartTextInput();
		}
		
		public void ProcessEvents() {
			SDL.SDL_Event curEvent;

			while( SDL.SDL_PollEvent( out curEvent ) != 0 ) {
				switch( curEvent.type ) {
					case SDL.SDL_EventType.SDL_QUIT:
						DestroyWindow();
						break;
					case SDL.SDL_EventType.SDL_WINDOWEVENT:
						HandleWindowEvent( curEvent );
						break;
					case SDL.SDL_EventType.SDL_KEYDOWN:
						HandleKeyDown( curEvent );
						break;
					case SDL.SDL_EventType.SDL_KEYUP:
						HandleKeyUp( curEvent );
						break;
					case SDL.SDL_EventType.SDL_TEXTINPUT:
						HandleTextInput( curEvent );
						break;
					case SDL.SDL_EventType.SDL_MOUSEMOTION:
						HandleMouseMove( curEvent );
						break;
					case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
						HandleMouseDown( curEvent );
						break;
					case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
						HandleMouseUp( curEvent );
						break;
					case SDL.SDL_EventType.SDL_MOUSEWHEEL:
						HandleMouseWheel( curEvent );
						break;
				}

				// Don't process any more events if the window has been destroyed
				if ( !this.exists ) {
					break;
				}
			}
		}
		
		/// <summary> Updates the pointer to the window's surface. </summary>
		/// <remarks> This method must be called when the window is resized. Otherwise, it will be pointing to an
		/// invalid SDL_Surface. Do not free this pointer, as it will be automatically freed when the window is
		/// destroyed. Override this method with one that does nothing if using OpenGL or DirectX, as 
		/// SDL_GetWindowSurface is not intended to be used with those APIs. </remarks>
		protected virtual void UpdateSurfacePointer() {
			this.surface = SDL.SDL_GetWindowSurface( window );
			if( this.window == IntPtr.Zero ) {
				throw new InvalidOperationException( "SDL_GetWindowSurface failed: " + SDL.SDL_GetError() );
			}
		}
		
		public virtual void Draw( Bitmap framebuffer ) {
			using( FastBitmap fastBmp = new FastBitmap( framebuffer, true, true ) ) {
				IntPtr image = SDL.SDL_CreateRGBSurfaceFrom( fastBmp.Scan0, fastBmp.Width, fastBmp.Height, 32,
				                                             fastBmp.Stride, 0x00FF0000, 0x0000FF00, 0x000000FF,
				                                             0xFF000000 );
				SDL.SDL_BlitSurface( image, IntPtr.Zero, this.surface, IntPtr.Zero );
				SDL.SDL_FreeSurface( image );
			}

			SDL.SDL_UpdateWindowSurface( window );
		}

		private void HandleWindowEvent( SDL.SDL_Event winEvent ) {
			switch ( winEvent.window.windowEvent ) {
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
					UpdateSurfacePointer();
					if( Resize != null ) {
						Resize( this, new EventArgs() );
					}
					break;
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
					this.focused = true;
					if( FocusedChanged != null ) {
						FocusedChanged( this, new EventArgs() );
					}
					break;
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
					this.focused = false;
					if( FocusedChanged != null ) {
						FocusedChanged( this, new EventArgs() );
					}
					break;
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED:
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
					if( WindowStateChanged != null ) {
						WindowStateChanged( this, new EventArgs() );
					}
					break;
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SHOWN:
					this.visible = true;
					break;
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_HIDDEN:
					this.visible = false;
					break;
				default:
					break;
			}
		}

		private void HandleKeyDown( SDL.SDL_Event keyEvent ) {
			SDL.SDL_Keycode sdlKey = keyEvent.key.keysym.sym;

			if( SDL2InputMapping.keyDict.ContainsKey( sdlKey ) ) {
				Key tkKey = SDL2InputMapping.keyDict[sdlKey];
				this.keyboard[tkKey] = true;
			}
			else {
				Utils.LogDebug( "No key dict entry for: " + SDL.SDL_GetKeyName( sdlKey ) );
			}
		}

		private void HandleKeyUp( SDL.SDL_Event keyEvent ) {
			SDL.SDL_Keycode sdlKey = keyEvent.key.keysym.sym;

			if( SDL2InputMapping.keyDict.ContainsKey( sdlKey ) ) {
				Key tkKey = SDL2InputMapping.keyDict[sdlKey];
				this.keyboard[tkKey] = false;
			}
			else {
				Utils.LogDebug( "No key dict entry for: " + SDL.SDL_GetKeyName( sdlKey ) );
			}
		}

		private void HandleTextInput( SDL.SDL_Event textEvent ) {
			KeyPressEventArgs args = new KeyPressEventArgs();

			for( int i = 0; i < SDL.SDL_TEXTINPUTEVENT_TEXT_SIZE; i++ ) {
				char c;
				unsafe {
					c = (char)textEvent.text.text[i];
				}
				if( c == (char)0 ) {  // Reached a null
					break;
				}

				args.KeyChar = c;
				if( KeyPress != null ) {
					KeyPress( this, args );
				}
			}
		}

		private void HandleMouseMove( SDL.SDL_Event moveEvent ) {
			SDL.SDL_MouseMotionEvent motion = moveEvent.motion;
			int x = motion.x;
			int y = motion.y;

			this.mouse.Position = new Point( x, y );
		}

		// TODO: fix mouse button limitation
		private void HandleMouseDown( SDL.SDL_Event mouseEvent ) {
			SDL.SDL_MouseButtonEvent down = mouseEvent.button;
			uint button = down.button;

			if( SDL2InputMapping.mouseDict.ContainsKey( button ) ) {
				MouseButton tkButton = SDL2InputMapping.mouseDict[button];
				this.mouse[tkButton] = true;
			}
			else {
				Utils.LogDebug( "No mouse dict entry for: " + button );
			}
		}

		private void HandleMouseUp( SDL.SDL_Event mouseEvent ) {
			SDL.SDL_MouseButtonEvent up = mouseEvent.button;
			uint button = up.button;

			if( SDL2InputMapping.mouseDict.ContainsKey( button ) ) {
				MouseButton tkButton = SDL2InputMapping.mouseDict[button];
				this.mouse[tkButton] = false;
			}
			else {
				Utils.LogDebug( "No mouse dict entry for: " + button );
			}
		}

		private void HandleMouseWheel( SDL.SDL_Event wheelEvent ) {
			SDL.SDL_MouseWheelEvent scroll = wheelEvent.wheel;
			// FIXME: doesn't take into account horizontal mouse wheels (hold Shift while scrolling on OS X, Linux)
			int y = scroll.y;

			this.mouse.WheelPrecise += y;
		}
		
		protected virtual void DestroyWindow() {
			// TODO: like its counterpart, turn text input into a toggle
			SDL.SDL_StopTextInput();

			SDL.SDL_DestroyWindow( window );
			this.exists = false;
		}

		public virtual void Close() {
			if( this.exists ) {
				DestroyWindow();
			}

			SDL.SDL_Quit();
		}
		
		public void Dispose() {
			Dispose( true );
		}

		protected void Dispose( bool disposing ) {
			if( disposed ) {
				return;
			}
		}
	}
	
	public class SDL2WindowInfo : IWindowInfo {
		IntPtr window;

		public IntPtr WinHandle {
			get { return window; }
		}

		public SDL2WindowInfo( IntPtr window ) {
			this.window = window;
		}

		public void Dispose() {
			// Doesn't do anything; is just here to implement IDisposable
		}
	}
}

