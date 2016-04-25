using System;
using System.Collections.Generic;

using SDL2;

// TODO: maybe remove these
using OpenTK;
using OpenTK.Input;

namespace ClassicalSharp
{
	public static class SDL2InputMapping
	{
		// Temporary until we can rip out OpenTK stuff and just use SDL2 stuff
		public static Dictionary<SDL.SDL_Keycode, Key> keyDict = new Dictionary<SDL.SDL_Keycode, Key>() {
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

		public static Dictionary<uint, MouseButton> mouseDict = new Dictionary<uint, MouseButton>() {
			{ SDL.SDL_BUTTON_LEFT, MouseButton.Left },
			{ SDL.SDL_BUTTON_MIDDLE, MouseButton.Middle },
			{ SDL.SDL_BUTTON_RIGHT, MouseButton.Right },
			{ SDL.SDL_BUTTON_X1, MouseButton.Button1 },
			{ SDL.SDL_BUTTON_X2, MouseButton.Button2 },
		};
	}
}

