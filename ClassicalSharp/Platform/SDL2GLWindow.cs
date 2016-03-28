using System;
using System.Drawing;

using SDL2;

namespace ClassicalSharp
{
	public class SDL2GLWindow : SDL2Window, IPlatformWindow, IDisposable
	{
		public bool VSync {
			get {
				int result = SDL.SDL_GL_GetSwapInterval();
				return result == 1;  // If the return value is -1 (query not supported), assume that vsync is off
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

		private IntPtr glContext;

		public SDL2GLWindow( Game game, string username, bool nullContext, int width, int height ) :
			base( width, height, Program.AppName + " - (" + username + ")", 
			      SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE ) {

			this.glContext = SDL.SDL_GL_CreateContext( window );

			if( this.glContext == IntPtr.Zero ) {
				throw new InvalidOperationException( "SDL_GL_CreateContext: " + SDL.SDL_GetError() );
			}

			// Try to enable VSync, but don't worry if it fails
			SDL.SDL_GL_SetSwapInterval( 1 );  // 1 = enable VSync

			this.game = game;

			this.Resize += this.OnResize;
		}

		public void Run() {
			game.OnLoad();

			uint curTime = 0;
			uint prevTime = 0;
			double delta = 0;

			while( true ) {
				ProcessEvents();

				if( !this.exists ) {
					break;
				}

				curTime = SDL.SDL_GetTicks();  // returns ms
				delta = ( curTime - prevTime ) / 1000.0;  // convert to seconds
				prevTime = curTime;

				game.RenderFrame( delta );
			}
		}

		private void OnResize( object sender, EventArgs e ) {
			game.OnResize();
		}

		public void SwapBuffers() {
			SDL.SDL_GL_SwapWindow( window );
		}

		public override void Draw( Bitmap framebuffer ) {
			throw new InvalidOperationException( "You can't use SDL drawing functions when OpenGL is being used." );
		}

		protected override void DestroyWindow() {
			SDL.SDL_GL_DeleteContext( glContext );

			base.DestroyWindow();
		}

		public void Exit() {
			SDL.SDL_Event newEvent = new SDL.SDL_Event();
			newEvent.type = SDL.SDL_EventType.SDL_QUIT;

			SDL.SDL_PushEvent( ref newEvent );
		}
	}
}
