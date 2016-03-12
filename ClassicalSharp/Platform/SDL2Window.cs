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
		
		public IWindowInfo WindowInfo { get; }
		
		public event EventHandler<KeyPressEventArgs> KeyPress;
		
		private bool running = true;
		private bool disposed = false;
		
		private IntPtr window;
		
		public SDL2Window( Game game, string username, bool nullContext, int width, int height ) {
			int success = SDL.SDL_Init( SDL.SDL_INIT_TIMER | SDL.SDL_INIT_VIDEO );
			
			if ( success != 0 ) {
				string error = SDL.SDL_GetError();
				
				throw new Exception( error );
			}
			
			IntPtr window = SDL.SDL_CreateWindow( "ClassicalSharp - SDL", SDL.SDL_WINDOWPOS_UNDEFINED, 
			                                      SDL.SDL_WINDOWPOS_UNDEFINED,
			                                      800, 600, 
			                                      SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL );
			if ( window == IntPtr.Zero ) {
				string error = SDL.SDL_GetError();
				
				throw new Exception( error );
			}
		}
		
		public void Run() {
			while ( running ) {
				SDL.SDL_Event curEvent;
				
				while (SDL.SDL_PollEvent( out curEvent ) != 0) {
					if ( curEvent.type == SDL.SDL_EventType.SDL_QUIT ) {
						running = false;
					}
				}
			}
			
			SDL.SDL_DestroyWindow( window );
			SDL.SDL_Quit();
		}
		
		public void SwapBuffers() {
			SDL.SDL_UpdateWindowSurface(window);
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
			if ( disposed ) {
				return;
			}
		}
	}
}

