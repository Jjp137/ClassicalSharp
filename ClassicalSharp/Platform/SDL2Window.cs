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
		};

		public int Width {
			get {
				int w, h;
				SDL.SDL_GetWindowSize(window, out w, out h);
				return w;
			}
		}

		public int Height { 
			get {
				int w, h;
				SDL.SDL_GetWindowSize(window, out w, out h);
				return h;
			}
		}

		public Size ClientSize { 
			get {
				int w, h;
				SDL.SDL_GetWindowSize(window, out w, out h);
				return new Size( w, h );
			}
		}

		public bool VSync { get; set; }

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
				int visible = SDL.SDL_ShowCursor( -1 );
				return visible == SDL.SDL_ENABLE;
			}
			set {
				int arg = value ? SDL.SDL_ENABLE : SDL.SDL_DISABLE;
				SDL.SDL_ShowCursor( arg );
			}
		}

		public Point DesktopCursorPos { get; set; }

		public MouseDevice Mouse { get; }

		public KeyboardDevice Keyboard { get; }

		public Icon Icon { get; set; }

		public Point PointToScreen( Point coords ) {
			return new Point( 0, 0 );
		}

		public WindowState WindowState {
			get {
				throw new NotImplementedException("WindowState.get");
			}
			set {
				throw new NotImplementedException("WindowState.set");
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
				Program.AppName + " - SDL2Window - (" + username + ")",
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

			// TODO: enable vsync

			this.game = game;

			// Temporary crap
			this.Keyboard = new KeyboardDevice();
			this.Mouse = new MouseDevice();

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
					if( curEvent.type == SDL.SDL_EventType.SDL_QUIT ) {
						running = false;
						break;
					}
					else if( curEvent.type == SDL.SDL_EventType.SDL_WINDOWEVENT ) {
						HandleWindowEvent( curEvent );
					}
					else if( curEvent.type == SDL.SDL_EventType.SDL_KEYDOWN ) {
						HandleKeyDown( curEvent );
					}
					else if( curEvent.type == SDL.SDL_EventType.SDL_KEYUP ) {
						HandleKeyUp( curEvent );
					}
					else if( curEvent.type == SDL.SDL_EventType.SDL_MOUSEMOTION ) {
						;
					}
					else if( curEvent.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN ) {
						;
					}
					else if( curEvent.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP ) {
						;
					}
				}

				curTime = SDL.SDL_GetTicks();  // returns ms
				delta = (curTime - prevTime) / 1000.0;  // convert to seconds
				prevTime = curTime;

				game.RenderFrame( delta );

				// TODO: should there be a delay?
			}

			this.exists = false;
			
			SDL.SDL_GL_DeleteContext( context );
			SDL.SDL_DestroyWindow( window );
			SDL.SDL_Quit();
		}

		private void HandleWindowEvent(SDL.SDL_Event winEvent) {
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

		private void HandleKeyDown(SDL.SDL_Event keyEvent) {
			SDL.SDL_Keycode sdlKey = keyEvent.key.keysym.sym;

			if( keyDict.ContainsKey(sdlKey) ) {
				Key tkKey = keyDict[sdlKey];
				this.Keyboard[tkKey] = true;

				EventHandler<KeyPressEventArgs> temp = this.KeyPress;
				if( temp != null ) {
					KeyPressEventArgs args = new KeyPressEventArgs();
					// TODO: not robust, support the high bit as well as more of Unicode
					args.KeyChar = (char)sdlKey;

					temp( this, args );
				}
			}
			else {
				Utils.LogDebug( "No dict entry for: " + SDL.SDL_GetKeyName( sdlKey ) );
			}
		}

		private void HandleKeyUp(SDL.SDL_Event keyEvent) {
			SDL.SDL_Keycode sdlKey = keyEvent.key.keysym.sym;

			if ( keyDict.ContainsKey(sdlKey) ) {
				Key tkKey = keyDict[sdlKey];
				this.Keyboard[tkKey] = false;
			}
			else {
				Utils.LogDebug( "No dict entry for: " + SDL.SDL_GetKeyName( sdlKey ) );
			}
		}

		private void HandleMouseMove(SDL.SDL_Event moveEvent) {
			
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
