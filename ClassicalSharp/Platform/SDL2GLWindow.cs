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
	public class SDL2GLWindow : SDL2Window, IPlatformWindow, IDisposable
	{
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

		private Game game;

		private IntPtr context;

		public SDL2GLWindow( Game game, string username, bool nullContext, int width, int height ) :
			base( width, height, Program.AppName + " - (" + username + ")", 
			      SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE ) {

			this.context = SDL.SDL_GL_CreateContext( window );

			if( this.context == IntPtr.Zero ) {
				throw new InvalidOperationException( "SDL_GL_CreateContext: " + SDL.SDL_GetError() );
			}

			// Try to enable VSync, but don't worry if it fails
			SDL.SDL_GL_SetSwapInterval( 1 );  // 1 = enable VSync

			this.game = game;

			// TODO: Attach resized event and other events
		}

		public void Run() {
			game.OnLoad();

			uint curTime = 0;
			uint prevTime = 0;
			double delta = 0;

			while( true ) {
				ProcessEvents();

				if ( !this.exists ) {
					break;
				}

				curTime = SDL.SDL_GetTicks();  // returns ms
				delta = ( curTime - prevTime ) / 1000.0;  // convert to seconds
				prevTime = curTime;

				game.RenderFrame( delta );
			}
		}

		public void SwapBuffers() {
			SDL.SDL_GL_SwapWindow( window );
		}

		public override void Draw( Bitmap framebuffer ) {
			throw new InvalidOperationException( "You can't use SDL drawing functions when OpenGL is being used." );
		}

		public override void Close() {
			SDL.SDL_GL_DeleteContext( context );
			
			base.Close();
		}
		
		public void Exit() {
			SDL.SDL_Event newEvent = new SDL.SDL_Event();
			newEvent.type = SDL.SDL_EventType.SDL_QUIT;
			
			SDL.SDL_PushEvent( ref newEvent );
		}
	}
}
