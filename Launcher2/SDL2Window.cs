using System;
using System.Collections.Generic;

using SDL2;

using ClassicalSharp;

// TODO: remove these
using OpenTK;
using OpenTK.Input;
using OpenTK.Platform;
using System.Drawing;

namespace Launcher
{
	// TODO: perhaps use inheritance once it's clear what's actually shared between the two
	public class SDL2Window
	{
		// Temporary until we can rip out OpenTK stuff and just use SDL2 stuff
		private static Dictionary<SDL.SDL_Keycode, Key> keyDict = new Dictionary<SDL.SDL_Keycode, Key>() {
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
			{ SDL.SDL_Keycode.SDLK_DOWN, Key.Down }
		};
		private static Dictionary<uint, MouseButton> mouseDict = new Dictionary<uint, MouseButton>() {
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
		
		private MouseDevice mouse;
		public MouseDevice Mouse {
			get {
				return mouse;
			}
		}
		
		private bool exists;
		public bool Exists {
			get { return exists; }
		}

		private bool focused;
		public bool Focused {
			get {
				return focused;
			}
		}
		
		public bool Visible {
			get { return true; }
			set { }
		}

		public Point DesktopCursorPos {
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
		
		private KeyboardDevice keyboard;
		public KeyboardDevice Keyboard {
			get {
				return keyboard;
			}
		}
		
		public Point PointToScreen( Point coords ) {
			// FIXME: SDL 2.0.4 makes this easier, but Debian only has 2.0.2
			int win_x, win_y;
			SDL.SDL_GetWindowPosition( this.window, out win_x, out win_y );
			return new Point( coords.X + win_x, coords.Y + win_y );
		}
		
		public WindowState WindowState {
			get {
				return WindowState.Normal;
				//throw new NotImplementedException( "WindowState.get" );
			}
			set {
				//throw new NotImplementedException( "WindowState.set" );
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
		
		private IntPtr window;
		private SDL2WindowInfo windowInfo;
		
		public SDL2Window ( int width, int height, string title )
		{
			int success = SDL.SDL_Init( SDL.SDL_INIT_TIMER | SDL.SDL_INIT_VIDEO );

			if( success != 0 ) {
				// TODO: inherit from Exception
				throw new Exception( SDL.SDL_GetError() );
			}

			this.window = SDL.SDL_CreateWindow(
				title + " - SDL2 ",
				SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED,
				width, height,
				SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
				);

			if( this.window == IntPtr.Zero ) {
				throw new Exception( SDL.SDL_GetError() );
			}

			this.windowInfo = new SDL2WindowInfo( this.window );

			// Temporary crap
			this.keyboard = new KeyboardDevice();
			this.mouse = new MouseDevice();

			// For text entry
			SDL.SDL_StartTextInput();

			this.exists = true;
			this.focused = true;
		}
		
		public void ProcessEvents() {
			SDL.SDL_Event curEvent;

			while( SDL.SDL_PollEvent( out curEvent ) != 0 ) {
				switch( curEvent.type ) {
					case SDL.SDL_EventType.SDL_QUIT:
						// TODO: figure out what to do here
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
			}
		}
		
		private void HandleWindowEvent( SDL.SDL_Event winEvent ) {
			switch ( winEvent.window.windowEvent ) {
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
					break;
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
					this.focused = true;
					break;
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
					this.focused = false;
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
			EventHandler<KeyPressEventArgs> temp = this.KeyPress;
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
				temp( this, args );
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
		
		public void Close() {
			this.exists = false;
			
			SDL.SDL_DestroyWindow( window );
			SDL.SDL_Quit();
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
			// Doesn't do anything.
		}
	}
}

