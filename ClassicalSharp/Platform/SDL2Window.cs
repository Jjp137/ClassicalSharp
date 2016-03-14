using System;
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
		public int Width {
			get { return 800; }
		}

		public int Height { 
			get { return 600; }
		}

		public Size ClientSize { 
			get { return new Size( 800, 600 ); }
		}

		public bool VSync { get; set; }

		public bool Exists { get; }

		public bool Focused { get; }

		public bool CursorVisible { get; set; }

		public Point DesktopCursorPos { get; set; }

		public MouseDevice Mouse { get; }

		public KeyboardDevice Keyboard { get; }

		public Icon Icon { get; set; }

		public Point PointToScreen( Point coords ) {
			return new Point( 0, 0 );
		}

		public WindowState WindowState { get; set; }

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
				SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL
			);

			if( this.window == IntPtr.Zero ) {
				throw new Exception( SDL.SDL_GetError() );
			}
			
			this.windowInfo = new SDL2WindowInfo( this.window );

			this.context = SDL.SDL_GL_CreateContext( window );
			
			if( this.context == IntPtr.Zero ) {
				throw new Exception( SDL.SDL_GetError() );
			}

			this.game = game;
			
			// Temporary crap
			this.Keyboard = new KeyboardDevice();
			this.Mouse = new MouseDevice();
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
				}
				
				curTime = SDL.SDL_GetTicks();  // returns ms
				delta = (curTime - prevTime) / 1000.0;  // convert to seconds
				prevTime = curTime;
				
				game.RenderFrame( delta );
			}

			SDL.SDL_GL_DeleteContext( context );
			SDL.SDL_DestroyWindow( window );
			SDL.SDL_Quit();
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
