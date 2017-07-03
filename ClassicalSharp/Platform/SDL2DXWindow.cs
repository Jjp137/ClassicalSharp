#if USE_DX && !ANDROID
using System;
using System.Drawing;

using SDL2;

namespace ClassicalSharp
{
	// FIXME: there's some code copied from SDL2GLWindow
	public class SDL2DXWindow : SDL2Window, IPlatformWindow, IDisposable
	{
		// SDL2DXApi (and Direct3D9Api) handle VSync already, so just store the current value.
		bool vsync = true;
		public bool VSync {
			get {
				return vsync;
			}
			set {
				vsync = value;
			}
		}
		
		public bool CursorVisible {
			get {
				int visible = SDL.SDL_ShowCursor(-1);  // -1 = query
				return visible == SDL.SDL_ENABLE;
			}
			set {
				int arg = value ? SDL.SDL_ENABLE : SDL.SDL_DISABLE;
				SDL.SDL_ShowCursor(arg);
			}
		}
		
		private Game game;
		
		public SDL2DXWindow(Game game, string username, bool nullContext, int width, int height) :
			base(width, height, Program.AppName + " - (" + username + " - DirectX)", 
			     SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE) {
			this.game = game;
			
			this.Resize += this.OnResize;
		}
		
		public void Run() {
			game.OnLoad();
			// Emulate OpenTK raising the resize event; fixes block in hand not showing up initially
			game.OnResize();

			uint curTime = 0;
			uint prevTime = 0;
			double delta = 0;

			while (true) {
				ProcessEvents();

				if (!this.exists) {
					break;
				}

				curTime = SDL.SDL_GetTicks();  // returns ms
				delta = (curTime - prevTime) / 1000.0;  // convert to seconds
				prevTime = curTime;

				game.RenderFrame(delta);
			}
		}
		
		private void OnResize(object sender, EventArgs e) {
			game.OnResize();
		}
		
		public void SwapBuffers() {
			// Do nothing; SDL2DXApi already handles that.
		}

		protected override void UpdateSurfacePointer() {
			// Do nothing; it's best not to combine SDL_GetWindowSurface with DirectX.
		}

		public override void Draw(Bitmap framebuffer) {
			throw new InvalidOperationException("You can't use SDL drawing functions when DirectX is being used directly.");
		}

		public override void Draw(Bitmap framebuffer, Rectangle rec) {
			throw new InvalidOperationException("You can't use SDL drawing functions when DirectX is being used directly.");
		}

		public void Exit() {
			SDL.SDL_Event newEvent = new SDL.SDL_Event();
			newEvent.type = SDL.SDL_EventType.SDL_QUIT;

			SDL.SDL_PushEvent(ref newEvent);
		}
	}
}
#endif
