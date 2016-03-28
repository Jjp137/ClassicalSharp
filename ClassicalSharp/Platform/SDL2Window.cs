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
		// Temporary until we can rip out OpenTK stuff and just use SDL2 stuff
		protected static Dictionary<SDL.SDL_Keycode, Key> keyDict = new Dictionary<SDL.SDL_Keycode, Key>() {
			{ SDL.SDL_Keycode.SDLK_a, Key.A },
			{ SDL.SDL_Keycode.SDLK_b, Key.B },
			{ SDL.SDL_Keycode.SDLK_c, Key.C },
			{ SDL.SDL_Keycode.SDLK_d, Key.D },
			{ SDL.SDL_Keycode.SDLK_e, Key.E },
			{ SDL.SDL_Keycode.SDLK_f, Key.F },
			{ SDL.SDL_Keycode.SDLK_g, Key.G },
			{ SDL.SDL_Keycode.SDLK_h, Key.H },
			{ SDL.SDL_Keycode.SDLK_i, Key.I },
			{ SDL.SDL_Keycode.SDLK_j, Key.J },
			{ SDL.SDL_Keycode.SDLK_k, Key.K },
			{ SDL.SDL_Keycode.SDLK_l, Key.L },
			{ SDL.SDL_Keycode.SDLK_m, Key.M },
			{ SDL.SDL_Keycode.SDLK_n, Key.N },
			{ SDL.SDL_Keycode.SDLK_o, Key.O },
			{ SDL.SDL_Keycode.SDLK_p, Key.P },
			{ SDL.SDL_Keycode.SDLK_q, Key.Q },
			{ SDL.SDL_Keycode.SDLK_r, Key.R },
			{ SDL.SDL_Keycode.SDLK_s, Key.S },
			{ SDL.SDL_Keycode.SDLK_t, Key.T },
			{ SDL.SDL_Keycode.SDLK_u, Key.U },
			{ SDL.SDL_Keycode.SDLK_v, Key.V },
			{ SDL.SDL_Keycode.SDLK_w, Key.W },
			{ SDL.SDL_Keycode.SDLK_x, Key.X },
			{ SDL.SDL_Keycode.SDLK_y, Key.Y },
			{ SDL.SDL_Keycode.SDLK_z, Key.Z },
			{ SDL.SDL_Keycode.SDLK_0, Key.Number0 },
			{ SDL.SDL_Keycode.SDLK_1, Key.Number1 },
			{ SDL.SDL_Keycode.SDLK_2, Key.Number2 },
			{ SDL.SDL_Keycode.SDLK_3, Key.Number3 },
			{ SDL.SDL_Keycode.SDLK_4, Key.Number4 },
			{ SDL.SDL_Keycode.SDLK_5, Key.Number5 },
			{ SDL.SDL_Keycode.SDLK_6, Key.Number6 },
			{ SDL.SDL_Keycode.SDLK_7, Key.Number7 },
			{ SDL.SDL_Keycode.SDLK_8, Key.Number8 },
			{ SDL.SDL_Keycode.SDLK_9, Key.Number9 },
			{ SDL.SDL_Keycode.SDLK_SPACE, Key.Space },
			{ SDL.SDL_Keycode.SDLK_BACKSPACE, Key.BackSpace },
			{ SDL.SDL_Keycode.SDLK_RETURN, Key.Enter },
			{ SDL.SDL_Keycode.SDLK_ESCAPE, Key.Escape },
			{ SDL.SDL_Keycode.SDLK_LSHIFT, Key.ShiftLeft },
			{ SDL.SDL_Keycode.SDLK_RSHIFT, Key.ShiftRight },
			{ SDL.SDL_Keycode.SDLK_LCTRL, Key.ControlLeft },
			{ SDL.SDL_Keycode.SDLK_RCTRL, Key.ControlRight },
			{ SDL.SDL_Keycode.SDLK_LALT, Key.AltLeft },
			{ SDL.SDL_Keycode.SDLK_RALT, Key.AltRight },
			{ SDL.SDL_Keycode.SDLK_LEFT, Key.Left },
			{ SDL.SDL_Keycode.SDLK_RIGHT, Key.Right },
			{ SDL.SDL_Keycode.SDLK_UP, Key.Up },
			{ SDL.SDL_Keycode.SDLK_DOWN, Key.Down },
			{ SDL.SDL_Keycode.SDLK_TAB, Key.Tab },
			{ SDL.SDL_Keycode.SDLK_F1, Key.F1 },
			{ SDL.SDL_Keycode.SDLK_F2, Key.F2 },
			{ SDL.SDL_Keycode.SDLK_F3, Key.F3 },
			{ SDL.SDL_Keycode.SDLK_F4, Key.F4 },
			{ SDL.SDL_Keycode.SDLK_F5, Key.F5 },
			{ SDL.SDL_Keycode.SDLK_F6, Key.F6 },
			{ SDL.SDL_Keycode.SDLK_F7, Key.F7 },
			{ SDL.SDL_Keycode.SDLK_F8, Key.F8 },
			{ SDL.SDL_Keycode.SDLK_F9, Key.F9 },
			{ SDL.SDL_Keycode.SDLK_F10, Key.F10 },
			{ SDL.SDL_Keycode.SDLK_F11, Key.F11 },
			{ SDL.SDL_Keycode.SDLK_F12, Key.F12 },
		};

		protected static Dictionary<uint, MouseButton> mouseDict = new Dictionary<uint, MouseButton>() {
			{ SDL.SDL_BUTTON_LEFT, MouseButton.Left },
			{ SDL.SDL_BUTTON_MIDDLE, MouseButton.Middle },
			{ SDL.SDL_BUTTON_RIGHT, MouseButton.Right },
			{ SDL.SDL_BUTTON_X1, MouseButton.Button1 },
			{ SDL.SDL_BUTTON_X2, MouseButton.Button2 },
		};

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

		public bool Focused {
			get {
				uint flags = SDL.SDL_GetWindowFlags( this.window );
				return ( flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS ) != 0;
			}
		}

		// TODO: implement
		public bool Visible {
			get { return true; }
			set { }
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

		protected bool exists;
		public bool Exists {
			get { return exists; }
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
				else if( ( flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN ) != 0 ) {
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
					if ( WindowStateChanged != null ) {
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

		public event EventHandler<EventArgs> Resize;
		public event EventHandler<EventArgs> WindowStateChanged;
		public event EventHandler<EventArgs> FocusedChanged;
		public event EventHandler<KeyPressEventArgs> KeyPress;

		protected IntPtr window;
		protected SDL2WindowInfo windowInfo;

		protected bool disposed;

		public SDL2Window( int width, int height, string title, SDL.SDL_WindowFlags flags )
		{
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

			this.exists = true;

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
		
		public virtual void Draw( Bitmap framebuffer ) {
			// TODO: store a pointer to the window surface and update it when resizing happens
			IntPtr winSurf = SDL.SDL_GetWindowSurface( window );

			using( FastBitmap fastBmp = new FastBitmap( framebuffer, true, true ) ) {
				IntPtr image = SDL.SDL_CreateRGBSurfaceFrom( fastBmp.Scan0, fastBmp.Width, fastBmp.Height, 32,
				                                             fastBmp.Stride, 0x00FF0000, 0x0000FF00, 0x000000FF,
				                                             0xFF000000 );
				SDL.SDL_BlitSurface( image, IntPtr.Zero, winSurf, IntPtr.Zero );
				SDL.SDL_FreeSurface( image );
			}

			SDL.SDL_UpdateWindowSurface( window );
		}

		private void HandleWindowEvent( SDL.SDL_Event winEvent ) {
			Console.WriteLine( winEvent.window.windowEvent );

			switch ( winEvent.window.windowEvent ) {
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
					if ( Resize != null ) {
						Resize( this, new EventArgs() );
					}
					break;
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
					if ( FocusedChanged != null ) {
						FocusedChanged( this, new EventArgs() );
					}
					break;
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
					if ( FocusedChanged != null ) {
						FocusedChanged( this, new EventArgs() );
					}
					break;
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED:
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
					if ( WindowStateChanged != null ) {
						WindowStateChanged ( this, new EventArgs() );
					}
					break;
				default:
					break;
			}
		}

		private void HandleKeyDown( SDL.SDL_Event keyEvent ) {
			SDL.SDL_Keycode sdlKey = keyEvent.key.keysym.sym;

			if( keyDict.ContainsKey(sdlKey) ) {
				Key tkKey = keyDict[sdlKey];
				this.keyboard[tkKey] = true;
			}
			else {
				Utils.LogDebug( "No dict entry for: " + SDL.SDL_GetKeyName( sdlKey ) );
			}
		}

		private void HandleKeyUp( SDL.SDL_Event keyEvent ) {
			SDL.SDL_Keycode sdlKey = keyEvent.key.keysym.sym;

			if ( keyDict.ContainsKey(sdlKey) ) {
				Key tkKey = keyDict[sdlKey];
				this.keyboard[tkKey] = false;
			}
			else {
				Utils.LogDebug( "No dict entry for: " + SDL.SDL_GetKeyName( sdlKey ) );
			}
		}

		private void HandleTextInput( SDL.SDL_Event textEvent ) {
			KeyPressEventArgs args = new KeyPressEventArgs();

			for( int i = 0; i < SDL.SDL_TEXTINPUTEVENT_TEXT_SIZE; i++ ) {
				char c;
				unsafe {
					c = (char)textEvent.text.text[i];
				}
				if ( c == (char)0 ) {  // Reached a null
					break;
				}

				args.KeyChar = c;
				if (KeyPress != null) {
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

		private void HandleMouseDown( SDL.SDL_Event mouseEvent ) {
			SDL.SDL_MouseButtonEvent down = mouseEvent.button;
			uint button = down.button;

			MouseButton tkButton = mouseDict[button];
			this.mouse[tkButton] = true;
		}

		private void HandleMouseUp( SDL.SDL_Event mouseEvent ) {
			SDL.SDL_MouseButtonEvent up = mouseEvent.button;
			uint button = up.button;

			MouseButton tkButton = mouseDict[button];
			this.mouse[tkButton] = false;
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
			// Doesn't do anything; is just here to implement IDisposable, heh :(
		}
	}
}

