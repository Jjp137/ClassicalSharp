using System;
using System.Collections.Generic;

using SDL2;

// TODO: remove these 'using' statements
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using OpenTK.Platform;

namespace ClassicalSharp
{
	public class SDL2Window : IPlatformWindow, IDisposable
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

		public Size ClientSize { 
			get {
				int w, h;
				SDL.SDL_GetWindowSize( this.window, out w, out h );
				return new Size( w, h );
			}
		}

		public bool VSync {
			get {
				int result = SDL.SDL_GL_GetSwapInterval();
				return result == 1;  // If it's -1, assume that we aren't using vsync
			}
			set {
				int arg = value ? 1 : 0;
				SDL.SDL_GL_SetSwapInterval( arg );
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

		public bool CursorVisible {
			get {
				int visible = SDL.SDL_ShowCursor( -1 );  // -1 = query
				return visible == SDL.SDL_ENABLE;
			}
			set {
				int arg = value ? SDL.SDL_ENABLE : SDL.SDL_DISABLE;
				SDL.SDL_ShowCursor( arg );
			}
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

		public MouseDevice Mouse { get; }

		public KeyboardDevice Keyboard { get; }

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

		public Point PointToScreen( Point coords ) {
			// FIXME: SDL 2.0.4 makes this easier, but Debian only has 2.0.2
			int win_x, win_y;
			SDL.SDL_GetWindowPosition( this.window, out win_x, out win_y );
			return new Point( coords.X + win_x, coords.Y + win_y );
		}

		public WindowState WindowState {
			get {
				throw new NotImplementedException( "WindowState.get" );
			}
			set {
				throw new NotImplementedException( "WindowState.set" );
			}
		}

		public IWindowInfo WindowInfo { get { return windowInfo; } }

		public event EventHandler<KeyPressEventArgs> KeyPress;

		private bool running = true;
		private bool disposed = false;

		private Game game;

		private IntPtr window;
		private IntPtr context;

		private SDL2WindowInfo windowInfo;

		public SDL2Window( Game game, string username, bool nullContext, int width, int height ) {
			int success = SDL.SDL_Init( SDL.SDL_INIT_TIMER | SDL.SDL_INIT_VIDEO );

			if( success != 0 ) {
				// TODO: inherit from Exception
				throw new Exception( SDL.SDL_GetError() );
			}

			this.window = SDL.SDL_CreateWindow(
				Program.AppName + " - SDL2 - (" + username + ")",
				SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED,
				width, height,
				SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
			);

			if( this.window == IntPtr.Zero ) {
				throw new Exception( SDL.SDL_GetError() );
			}

			this.windowInfo = new SDL2WindowInfo( this.window );

			this.context = SDL.SDL_GL_CreateContext( window );

			if( this.context == IntPtr.Zero ) {
				throw new Exception( SDL.SDL_GetError() );
			}

			// Try to enable VSync, but don't worry if it fails
			SDL.SDL_GL_SetSwapInterval( 1 );  // 1 = enable VSync

			this.game = game;

			// Temporary crap
			this.Keyboard = new KeyboardDevice();
			this.Mouse = new MouseDevice();

			// For text entry
			SDL.SDL_StartTextInput();

			this.exists = true;
			this.focused = true;
		}

		public void Run() {
			game.OnLoad();

			uint curTime = 0;
			uint prevTime = 0;
			double delta = 0;

			while( running ) {
				SDL.SDL_Event curEvent;

				while( SDL.SDL_PollEvent( out curEvent ) != 0 ) {
					switch( curEvent.type ) {
						case SDL.SDL_EventType.SDL_QUIT:
							running = false;
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

				curTime = SDL.SDL_GetTicks();  // returns ms
				delta = ( curTime - prevTime ) / 1000.0;  // convert to seconds
				prevTime = curTime;

				game.RenderFrame( delta );

				// TODO: should there be a delay?
			}

			this.exists = false;

			SDL.SDL_StopTextInput();

			SDL.SDL_GL_DeleteContext( context );
			SDL.SDL_DestroyWindow( window );
			SDL.SDL_Quit();
		}

		private void HandleWindowEvent( SDL.SDL_Event winEvent ) {
			switch ( winEvent.window.windowEvent ) {
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
					game.OnResize();
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
				this.Keyboard[tkKey] = true;
			}
			else {
				Utils.LogDebug( "No dict entry for: " + SDL.SDL_GetKeyName( sdlKey ) );
			}
		}

		private void HandleKeyUp( SDL.SDL_Event keyEvent ) {
			SDL.SDL_Keycode sdlKey = keyEvent.key.keysym.sym;

			if ( keyDict.ContainsKey(sdlKey) ) {
				Key tkKey = keyDict[sdlKey];
				this.Keyboard[tkKey] = false;
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

			this.Mouse.Position = new Point( x, y );
		}

		private void HandleMouseDown( SDL.SDL_Event mouseEvent ) {
			SDL.SDL_MouseButtonEvent down = mouseEvent.button;
			uint button = down.button;
			
			MouseButton tkButton = mouseDict[button];
			this.Mouse[tkButton] = true;
		}

		private void HandleMouseUp( SDL.SDL_Event mouseEvent ) {
			SDL.SDL_MouseButtonEvent up = mouseEvent.button;
			uint button = up.button;

			MouseButton tkButton = mouseDict[button];
			this.Mouse[tkButton] = false;
		}

		private void HandleMouseWheel( SDL.SDL_Event wheelEvent ) {
			SDL.SDL_MouseWheelEvent scroll = wheelEvent.wheel;
			// FIXME: doesn't take into account horizontal mouse wheels (hold Shift while scrolling on OS X, Linux)
			int y = scroll.y;
			
			this.Mouse.WheelPrecise += y; 
		}

		public void SwapBuffers() {
			SDL.SDL_GL_SwapWindow( window );
		}

		public void Exit() {
			SDL.SDL_Event newEvent = new SDL.SDL_Event();
			newEvent.type = SDL.SDL_EventType.SDL_QUIT;
			
			SDL.SDL_PushEvent( ref newEvent );
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
			// Doesn't do anything.
		}
	}
}
