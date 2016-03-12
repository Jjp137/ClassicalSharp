#region License
/* SDL2# - C# Wrapper for SDL2
 *
 * Copyright (c) 2013-2016 Ethan Lee.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 * Ethan "flibitijibibo" Lee <flibitijibibo@flibitijibibo.com>
 *
 */
#endregion

#region Using Statements
using System;
using System.Runtime.InteropServices;
#endregion

namespace SDL2
{
	/// <summary>
	/// Entry point for all SDL-related (non-extension) types and methods
	/// </summary>
	public static class SDL
	{
		#region SDL2# Variables

		/// <summary>
		/// Used by DllImport to load the native library.
		/// </summary>
		private const string nativeLibName = "SDL2.dll";

		#endregion

		#region SDL_stdinc.h

		public static uint SDL_FOURCC(byte A, byte B, byte C, byte D)
		{
			return (uint) (A | (B << 8) | (C << 16) | (D << 24));
		}

		public enum SDL_bool
		{
			SDL_FALSE = 0,
			SDL_TRUE = 1
		}

		/* malloc/free are used by the marshaler! -flibit */

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr SDL_malloc(IntPtr size);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SDL_free(IntPtr memblock);

		#endregion

		#region SDL_rwops.h

		/* Note about SDL2# and Internal RWops:
		 * These functions are currently not supported for public use.
		 * They are only meant to be used internally in functions marked with
		 * the phrase "THIS IS AN RWops FUNCTION!"
		 */

		/// <summary>
		/// Use this function to create a new SDL_RWops structure for reading from and/or writing to a named file.
		/// </summary>
		/// <param name="file">a UTF-8 string representing the filename to open</param>
		/// <param name="mode">an ASCII string representing the mode to be used for opening the file; see Remarks for details</param>
		/// <returns>Returns a pointer to the SDL_RWops structure that is created, or NULL on failure; call SDL_GetError() for more information.</returns>
		[DllImport(nativeLibName, EntryPoint = "SDL_RWFromFile", CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr INTERNAL_SDL_RWFromFile(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string file,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string mode
		);

		/* These are the public RWops functions. They should be used by
		 * functions marked with the phrase "THIS IS A PUBLIC RWops FUNCTION!"
		 */

		/* IntPtr refers to an SDL_RWops */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_RWFromMem(byte[] mem, int size);

		#endregion

		#region SDL_main.h

		/// <summary>
		/// Use this function to circumvent failure of SDL_Init() when not using SDL_main() as an entry point.
		/// </summary>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetMainReady();

		#endregion

		#region SDL.h

		public const uint SDL_INIT_TIMER =		0x00000001;
		public const uint SDL_INIT_AUDIO =		0x00000010;
		public const uint SDL_INIT_VIDEO =		0x00000020;
		public const uint SDL_INIT_JOYSTICK =		0x00000200;
		public const uint SDL_INIT_HAPTIC =		0x00001000;
		public const uint SDL_INIT_GAMECONTROLLER =	0x00002000;
		public const uint SDL_INIT_NOPARACHUTE =	0x00100000;
		public const uint SDL_INIT_EVERYTHING = (
			SDL_INIT_TIMER | SDL_INIT_AUDIO | SDL_INIT_VIDEO |
			SDL_INIT_JOYSTICK | SDL_INIT_HAPTIC |
			SDL_INIT_GAMECONTROLLER
		);

		/// <summary>
		/// Use this function to initialize the SDL library.
		/// This must be called before using any other SDL function.
		/// </summary>
		/// <param name="flags">subsystem initialization flags; see Remarks for details</param>
		/// <returns>Returns 0 on success or a negative error code on failure.
		/// Call <see cref="SDL_GetError()"/> for more information.</returns>
		/// <remarks>The Event Handling, File I/O, and Threading subsystems are initialized by default.
		/// You must specifically initialize other subsystems if you use them in your application.</remarks>
		/// <remarks>Unless the SDL_INIT_NOPARACHUTE flag is set, it will install cleanup signal handlers
		/// for some commonly ignored fatal signals (like SIGSEGV). </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_Init(uint flags);

		/// <summary>
		/// Use this function to initialize specific SDL subsystems.
		/// </summary>
		/// <param name="flags">any of the flags used by SDL_Init(); see Remarks for details</param>
		/// <returns>Returns 0 on success or a negative error code on failure.
		/// Call <see cref="SDL_GetError()"/> for more information.</returns>
		/// <remarks>After SDL has been initialized with <see cref="SDL_Init()"/> you may initialize
		/// uninitialized subsystems with <see cref="SDL_InitSubSystem()"/>.</remarks>
		/// <remarks>If you want to initialize subsystems separately you would call <see cref="SDL_Init(0)"/>
		/// followed by <see cref="SDL_InitSubSystem()"/> with the desired subsystem flag. </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_InitSubSystem(uint flags);

		/// <summary>
		/// Use this function to clean up all initialized subsystems.
		/// You should call it upon all exit conditions.
		/// </summary>
		/// <remarks>You should call this function even if you have already shutdown each initialized
		/// subsystem with <see cref="SDL_QuitSubSystem()"/>.</remarks>
		/// <remarks>If you start a subsystem using a call to that subsystem's init function (for example
		/// <see cref="SDL_VideoInit()"/>) instead of <see cref="SDL_Init()"/> or <see cref="SDL_InitSubSystem()"/>,
		/// then you must use that subsystem's quit function (<see cref="SDL_VideoQuit()"/>) to shut it down
		/// before calling <see cref="SDL_Quit()"/>.</remarks>
		/// <remarks>You can use this function with atexit() to ensure that it is run when your application is
		/// shutdown, but it is not wise to do this from a library or other dynamically loaded code. </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_Quit();

		/// <summary>
		/// Use this function to shut down specific SDL subsystems.
		/// </summary>
		/// <param name="flags">any of the flags used by <see cref="SDL_Init()"/>; see Remarks for details</param>
		/// <remarks>If you start a subsystem using a call to that subsystem's init function (for example
		/// <see cref="SDL_VideoInit()"/>) instead of <see cref="SDL_Init()"/> or <see cref="SDL_InitSubSystem()"/>,
		/// then you must use that subsystem's quit function (<see cref="SDL_VideoQuit()"/>) to shut it down
		/// before calling <see cref="SDL_Quit()"/>.</remarks>
		/// <remarks>You can use this function with atexit() to en
		/// <remarks>You still need to call <see cref="SDL_Quit()"/> even if you close all open subsystems with SDL_QuitSubSystem(). </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_QuitSubSystem(uint flags);

		/// <summary>
		/// Use this function to return a mask of the specified subsystems which have previously been initialized.
		/// </summary>
		/// <param name="flags">any of the flags used by <see cref="SDL_Init()"/>; see Remarks for details</param>
		/// <returns>If flags is 0 it returns a mask of all initialized subsystems, otherwise it returns the
		/// initialization status of the specified subsystems. The return value does not include SDL_INIT_NOPARACHUTE.</returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_WasInit(uint flags);

		#endregion

		#region SDL_platform.h

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GetPlatform();

		#endregion

		#region SDL_hints.h

		public const string SDL_HINT_FRAMEBUFFER_ACCELERATION =
			"SDL_FRAMEBUFFER_ACCELERATION";
		public const string SDL_HINT_RENDER_DRIVER =
			"SDL_RENDER_DRIVER";
		public const string SDL_HINT_RENDER_OPENGL_SHADERS =
			"SDL_RENDER_OPENGL_SHADERS";
		public const string SDL_HINT_RENDER_DIRECT3D_THREADSAFE =
			"SDL_RENDER_DIRECT3D_THREADSAFE";
		public const string SDL_HINT_RENDER_VSYNC =
			"SDL_RENDER_VSYNC";
		public const string SDL_HINT_VIDEO_X11_XVIDMODE =
			"SDL_VIDEO_X11_XVIDMODE";
		public const string SDL_HINT_VIDEO_X11_XINERAMA =
			"SDL_VIDEO_X11_XINERAMA";
		public const string SDL_HINT_VIDEO_X11_XRANDR =
			"SDL_VIDEO_X11_XRANDR";
		public const string SDL_HINT_GRAB_KEYBOARD =
			"SDL_GRAB_KEYBOARD";
		public const string SDL_HINT_VIDEO_MINIMIZE_ON_FOCUS_LOSS =
			"SDL_VIDEO_MINIMIZE_ON_FOCUS_LOSS";
		public const string SDL_HINT_IDLE_TIMER_DISABLED =
			"SDL_IOS_IDLE_TIMER_DISABLED";
		public const string SDL_HINT_ORIENTATIONS =
			"SDL_IOS_ORIENTATIONS";
		public const string SDL_HINT_XINPUT_ENABLED =
			"SDL_XINPUT_ENABLED";
		public const string SDL_HINT_GAMECONTROLLERCONFIG =
			"SDL_GAMECONTROLLERCONFIG";
		public const string SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS =
			"SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS";
		public const string SDL_HINT_ALLOW_TOPMOST =
			"SDL_ALLOW_TOPMOST";
		public const string SDL_HINT_TIMER_RESOLUTION =
			"SDL_TIMER_RESOLUTION";
		public const string SDL_HINT_RENDER_SCALE_QUALITY =
			"SDL_RENDER_SCALE_QUALITY";

		/* Only available in SDL 2.0.1 or higher */
		public const string SDL_HINT_VIDEO_HIGHDPI_DISABLED =
			"SDL_VIDEO_HIGHDPI_DISABLED";

		/* Only available in SDL 2.0.2 or higher */
		public const string SDL_HINT_CTRL_CLICK_EMULATE_RIGHT_CLICK =
			"SDL_CTRL_CLICK_EMULATE_RIGHT_CLICK";
		public const string SDL_HINT_VIDEO_WIN_D3DCOMPILER =
			"SDL_VIDEO_WIN_D3DCOMPILER";
		public const string SDL_HINT_MOUSE_RELATIVE_MODE_WARP =
			"SDL_MOUSE_RELATIVE_MODE_WARP";
		public const string SDL_HINT_VIDEO_WINDOW_SHARE_PIXEL_FORMAT =
			"SDL_VIDEO_WINDOW_SHARE_PIXEL_FORMAT";
		public const string SDL_HINT_VIDEO_ALLOW_SCREENSAVER =
			"SDL_VIDEO_ALLOW_SCREENSAVER";
		public const string SDL_HINT_ACCELEROMETER_AS_JOYSTICK =
			"SDL_ACCELEROMETER_AS_JOYSTICK";
		public const string SDL_HINT_VIDEO_MAC_FULLSCREEN_SPACES =
			"SDL_VIDEO_MAC_FULLSCREEN_SPACES";

		/* Only available in SDL 2.0.4 or higher */
		public const string SDL_HINT_NO_SIGNAL_HANDLERS =
			"SDL_NO_SIGNAL_HANDLERS";
		public const string SDL_HINT_IME_INTERNAL_EDITING =
			"SDL_IME_INTERNAL_EDITING";
		public const string SDL_HINT_ANDROID_SEPARATE_MOUSE_AND_TOUCH =
			"SDL_ANDROID_SEPARATE_MOUSE_AND_TOUCH";
		public const string SDL_HINT_EMSCRIPTEN_KEYBOARD_ELEMENT =
			"SDL_EMSCRIPTEN_KEYBOARD_ELEMENT";
		public const string SDL_HINT_THREAD_STACK_SIZE =
			"SDL_THREAD_STACK_SIZE";
		public const string SDL_HINT_WINDOW_FRAME_USABLE_WHILE_CURSOR_HIDDEN =
			"SDL_WINDOW_FRAME_USABLE_WHILE_CURSOR_HIDDEN";
		public const string SDL_HINT_WINDOWS_ENABLE_MESSAGELOOP =
			"SDL_WINDOWS_ENABLE_MESSAGELOOP";
		public const string SDL_HINT_WINDOWS_NO_CLOSE_ON_ALT_F4 =
			"SDL_WINDOWS_NO_CLOSE_ON_ALT_F4";
		public const string SDL_HINT_XINPUT_USE_OLD_JOYSTICK_MAPPING =
			"SDL_XINPUT_USE_OLD_JOYSTICK_MAPPING";
		public const string SDL_HINT_MAC_BACKGROUND_APP =
			"SDL_MAC_BACKGROUND_APP";
		public const string SDL_HINT_VIDEO_X11_NET_WM_PING =
			"SDL_VIDEO_X11_NET_WM_PING";
		public const string SDL_HINT_ANDROID_APK_EXPANSION_MAIN_FILE_VERSION =
			"SDL_ANDROID_APK_EXPANSION_MAIN_FILE_VERSION";
		public const string SDL_HINT_ANDROID_APK_EXPANSION_PATCH_FILE_VERSION =
			"SDL_ANDROID_APK_EXPANSION_PATCH_FILE_VERSION";

		public enum SDL_HintPriority
		{
			SDL_HINT_DEFAULT,
			SDL_HINT_NORMAL,
			SDL_HINT_OVERRIDE
		}

		/// <summary>
		/// Use this function to clear all hints.
		/// </summary>
		/// <remarks>This function is automatically called during <see cref="SDL_Quit()"/>. </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_ClearHints();

		/// <summary>
		/// Use this function to get the value of a hint.
		/// </summary>
		/// <param name="name">the hint to query; see the list of hints on
		/// <a href="http://wiki.libsdl.org/moin.cgi/CategoryHints#Hints">CategoryHints</a> for details</param>
		/// <returns>Returns the string value of a hint or NULL if the hint isn't set.</returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GetHint(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string name
		);

		/// <summary>
		/// Use this function to set a hint with normal priority.
		/// </summary>
		/// <param name="name">the hint to query; see the list of hints on
		/// <a href="http://wiki.libsdl.org/moin.cgi/CategoryHints#Hints">CategoryHints</a> for details</param>
		/// <param name="value">the value of the hint variable</param>
		/// <returns>Returns SDL_TRUE if the hint was set, SDL_FALSE otherwise.</returns>
		/// <remarks>Hints will not be set if there is an existing override hint or environment
		/// variable that takes precedence. You can use <see cref="SDL_SetHintWithPriority()"/> to set the hint with
		/// override priority instead.</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_SetHint(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string name,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string value
		);

		/// <summary>
		/// Use this function to set a hint with a specific priority.
		/// </summary>
		/// <param name="name">the hint to query; see the list of hints on
		/// <a href="http://wiki.libsdl.org/moin.cgi/CategoryHints#Hints">CategoryHints</a> for details</param>
		/// <param name="value">the value of the hint variable</param>
		/// <param name="priority">the <see cref="SDL_HintPriority"/> level for the hint</param>
		/// <returns>Returns SDL_TRUE if the hint was set, SDL_FALSE otherwise.</returns>
		/// <remarks>The priority controls the behavior when setting a hint that already has a value.
		/// Hints will replace existing hints of their priority and lower. Environment variables are
		/// considered to have override priority. </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_SetHintWithPriority(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string name,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string value,
			SDL_HintPriority priority
		);

		#endregion

		#region SDL_error.h

		/// <summary>
		/// Use this function to clear any previous error message.
		/// </summary>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_ClearError();

		/// <summary>
		/// Use this function to retrieve a message about the last error that occurred.
		/// </summary>
		/// <returns>Returns a message with information about the specific error that occurred,
		/// or an empty string if there hasn't been an error since the last call to <see cref="SDL_ClearError()"/>.
		/// Without calling <see cref="SDL_ClearError()"/>, the message is only applicable when an SDL function
		/// has signaled an error. You must check the return values of SDL function calls to determine
		/// when to appropriately call <see cref="SDL_GetError()"/>.
		/// This string is statically allocated and must not be freed by the application.</returns>
		/// <remarks>It is possible for multiple errors to occur before calling SDL_GetError(). Only the last error is returned. </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GetError();

		/// <summary>
		/// Use this function to set the SDL error string.
		/// </summary>
		/// <param name="fmt">a printf() style message format string </param>
		/// <param name="...">additional parameters matching % tokens in the fmt string, if any</param>
		/// <remarks>Calling this function will replace any previous error message that was set.</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetError(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string fmt,
			__arglist
		);

		#endregion

		#region SDL_log.h

		/* Begin nameless enum SDL_LOG_CATEGORY */
		public const int SDL_LOG_CATEGORY_APPLICATION = 0;
		public const int SDL_LOG_CATEGORY_ERROR = 1;
		public const int SDL_LOG_CATEGORY_ASSERT = 2;
		public const int SDL_LOG_CATEGORY_SYSTEM = 3;
		public const int SDL_LOG_CATEGORY_AUDIO = 4;
		public const int SDL_LOG_CATEGORY_VIDEO = 5;
		public const int SDL_LOG_CATEGORY_RENDER = 6;
		public const int SDL_LOG_CATEGORY_INPUT = 7;
		public const int SDL_LOG_CATEGORY_TEST = 8;

		/* Reserved for future SDL library use */
		public const int SDL_LOG_CATEGORY_RESERVED1 = 9;
		public const int SDL_LOG_CATEGORY_RESERVED2 = 10;
		public const int SDL_LOG_CATEGORY_RESERVED3 = 11;
		public const int SDL_LOG_CATEGORY_RESERVED4 = 12;
		public const int SDL_LOG_CATEGORY_RESERVED5 = 13;
		public const int SDL_LOG_CATEGORY_RESERVED6 = 14;
		public const int SDL_LOG_CATEGORY_RESERVED7 = 15;
		public const int SDL_LOG_CATEGORY_RESERVED8 = 16;
		public const int SDL_LOG_CATEGORY_RESERVED9 = 17;
		public const int SDL_LOG_CATEGORY_RESERVED10 = 18;

		/* Beyond this point is reserved for application use, e.g.
			enum {
				LOG_CATEGORY_AWESOME1 = SDL_LOG_CATEGORY_CUSTOM,
				LOG_CATEGORY_AWESOME2,
				LOG_CATEGORY_AWESOME3,
				...
			};
		*/
		public const int SDL_LOG_CATEGORY_CUSTOM = 19;
		/* End nameless enum SDL_LOG_CATEGORY */

		/// <summary>
		/// An enumeration of the predefined log priorities.
		/// </summary>
		public enum SDL_LogPriority
		{
			SDL_LOG_PRIORITY_VERBOSE = 1,
			SDL_LOG_PRIORITY_DEBUG,
			SDL_LOG_PRIORITY_INFO,
			SDL_LOG_PRIORITY_WARN,
			SDL_LOG_PRIORITY_ERROR,
			SDL_LOG_PRIORITY_CRITICAL,
			SDL_NUM_LOG_PRIORITIES
		}

		/// <summary>
		/// Used as a callback for <see cref="SDL_LogGetOutputFunction()"/> and <see cref="SDL_LogSetOutputFunction()"/>
		/// </summary>
		/// <param name="userdata">what was passed as userdata to <see cref="SDL_LogSetOutputFunction()"/></param>
		/// <param name="category">the category of the message; see Remarks for details</param>
		/// <param name="priority">the priority of the message; see Remarks for details</param>
		/// <param name="message">the message being output</param>
		/// <remarks>The category can be one of SDL_LOG_CATEGORY*</remarks>
		/// <remarks>The priority can be one of SDL_LOG_PRIORITY*</remarks>
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void SDL_LogOutputFunction(
			IntPtr userdata, // void*
			int category,
			SDL_LogPriority priority,
			IntPtr message // const char*
		);

		/// <summary>
		/// Use this function to log a message with SDL_LOG_CATEGORY_APPLICATION and SDL_LOG_PRIORITY_INFO.
		/// </summary>
		/// <param name="fmt">a printf() style message format string</param>
		/// <param name="...">additional parameters matching % tokens in the fmt string, if any</param>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_Log(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string fmt,
			__arglist
		);

		/// <summary>
		/// Use this function to log a message with SDL_LOG_PRIORITY_VERBOSE.
		/// </summary>
		/// <param name="category">the category of the message; see Remarks for details</param>
		/// <param name="fmt">a printf() style message format string</param>
		/// <param name="...">additional parameters matching % tokens in the fmt string, if any</param>
		/// <remarks>The category can be one of SDL_LOG_CATEGORY*</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_LogVerbose(
			int category,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string fmt,
			__arglist
		);

		/// <summary>
		/// Use this function to log a message with SDL_LOG_PRIORITY_DEBUG.
		/// </summary>
		/// <param name="category">the category of the message; see Remarks for details</param>
		/// <param name="fmt">a printf() style message format string</param>
		/// <param name="...">additional parameters matching % tokens in the fmt string, if any</param>
		/// <remarks>The category can be one of SDL_LOG_CATEGORY*</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_LogDebug(
			int category,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string fmt,
			__arglist
		);

		/// <summary>
		/// Use this function to log a message with SDL_LOG_PRIORITY_INFO.
		/// </summary>
		/// <param name="category">the category of the message; see Remarks for details</param>
		/// <param name="fmt">a printf() style message format string</param>
		/// <param name="...">additional parameters matching % tokens in the fmt string, if any</param>
		/// <remarks>The category can be one of SDL_LOG_CATEGORY*</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_LogInfo(
			int category,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string fmt,
			__arglist
		);

		/// <summary>
		/// Use this function to log a message with SDL_LOG_PRIORITY_WARN.
		/// </summary>
		/// <param name="category">the category of the message; see Remarks for details</param>
		/// <param name="fmt">a printf() style message format string</param>
		/// <param name="...">additional parameters matching % tokens in the fmt string, if any</param>
		/// <remarks>The category can be one of SDL_LOG_CATEGORY*</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_LogWarn(
			int category,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string fmt,
			__arglist
		);

		/// <summary>
		/// Use this function to log a message with SDL_LOG_PRIORITY_ERROR.
		/// </summary>
		/// <param name="category">the category of the message; see Remarks for details</param>
		/// <param name="fmt">a printf() style message format string</param>
		/// <param name="...">additional parameters matching % tokens in the fmt string, if any</param>
		/// <remarks>The category can be one of SDL_LOG_CATEGORY*</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_LogError(
			int category,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string fmt,
			__arglist
		);

		/// <summary>
		/// Use this function to log a message with SDL_LOG_PRIORITY_CRITICAL.
		/// </summary>
		/// <param name="category">the category of the message; see Remarks for details</param>
		/// <param name="fmt">a printf() style message format string</param>
		/// <param name="...">additional parameters matching % tokens in the fmt string, if any</param>
		/// <remarks>The category can be one of SDL_LOG_CATEGORY*</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_LogCritical(
			int category,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string fmt,
			__arglist
		);

		/// <summary>
		/// Use this function to log a message with the specified category and priority.
		/// </summary>
		/// <param name="category">the category of the message; see Remarks for details</param>
		/// <param name="priority">the priority of the message; see Remarks for details</param>
		/// <param name="fmt">a printf() style message format string</param>
		/// <param name="...">additional parameters matching % tokens in the fmt string, if any</param>
		/// <remarks>The category can be one of SDL_LOG_CATEGORY*</remarks>
		/// <remarks>The priority can be one of SDL_LOG_PRIORITY*</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_LogMessage(
			int category,
			SDL_LogPriority priority,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string fmt,
			__arglist
		);

		/// <summary>
		/// Use this function to log a message with the specified category and priority.
		/// This version of <see cref="SDL_LogMessage"/> uses a stdarg variadic argument list.
		/// </summary>
		/// <param name="category">the category of the message; see Remarks for details</param>
		/// <param name="priority">the priority of the message; see Remarks for details</param>
		/// <param name="fmt">a printf() style message format string</param>
		/// <param name="...">additional parameters matching % tokens in the fmt string, if any</param>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_LogMessageV(
			int category,
			SDL_LogPriority priority,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string fmt,
			__arglist
		);

		/// <summary>
		/// Use this function to get the priority of a particular log category.
		/// </summary>
		/// <param name="category">the category to query; see Remarks for details</param>
		/// <returns>Returns the <see cref="SDL_LogPriority"/> for the requested category; see Remarks for details. </returns>
		/// <remarks>The category can be one of SDL_LOG_CATEGORY*</remarks>
		/// <remarks>The returned priority will be one of SDL_LOG_PRIORITY*</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_LogPriority SDL_LogGetPriority(
			int category
		);

		/// <summary>
		/// Use this function to set the priority of a particular log category.
		/// </summary>
		/// <param name="category">the category to query; see Remarks for details</param>
		/// <param name="priority">the <see cref="SDL_LogPriority"/> of the message; see Remarks for details</param>
		/// <remarks>The category can be one of SDL_LOG_CATEGORY*</remarks>
		/// <remarks>The priority can be one of SDL_LOG_PRIORITY*</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_LogSetPriority(
			int category,
			SDL_LogPriority priority
		);

		/// <summary>
		/// Use this function to set the priority of all log categories.
		/// </summary>
		/// <param name="priority">the <see cref="SDL_LogPriority"/> of the message; see Remarks for details</param>
		/// <remarks>The priority can be one of SDL_LOG_PRIORITY*</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_LogSetAllPriority(
			SDL_LogPriority priority
		);

		/// <summary>
		/// Use this function to reset all priorities to default.
		/// </summary>
		/// <remarks>This is called in <see cref="SDL_Quit()"/>. </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_LogResetPriorities();

		/// <summary>
		/// Use this function to get the current log output function.
		/// </summary>
		/// <param name="callback">a pointer filled in with the current log callback; see Remarks for details</param>
		/// <param name="userdata">a pointer filled in with the pointer that is passed to callback (refers to void*)</param>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_LogGetOutputFunction(
			out SDL_LogOutputFunction callback,
			out IntPtr userdata
		);

		/* userdata refers to a void* */
		/// <summary>
		/// Use this function to replace the default log output function with one of your own.
		/// </summary>
		/// <param name="callback">the function to call instead of the default; see Remarks for details</param>
		/// <param name="userdata">a pointer that is passed to callback (refers to void*)</param>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_LogSetOutputFunction(
			SDL_LogOutputFunction callback,
			IntPtr userdata
		);

		#endregion

		#region SDL_messagebox.h

		[Flags]
		public enum SDL_MessageBoxFlags : uint
		{
			SDL_MESSAGEBOX_ERROR =		0x00000010,
			SDL_MESSAGEBOX_WARNING =	0x00000020,
			SDL_MESSAGEBOX_INFORMATION =	0x00000040
		}

		[Flags]
		public enum SDL_MessageBoxButtonFlags : uint
		{
			SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT = 0x00000001,
			SDL_MESSAGEBOX_BUTTON_ESCAPEKEY_DEFAULT = 0x00000002
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct INTERNAL_SDL_MessageBoxButtonData
		{
			public SDL_MessageBoxButtonFlags flags;
			public int buttonid;
			public IntPtr text; /* The UTF-8 button text */
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MessageBoxButtonData
		{
			public SDL_MessageBoxButtonFlags flags;
			public int buttonid;
			public string text; /* The UTF-8 button text */
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MessageBoxColor
		{
			public byte r, g, b;
		}

		public enum SDL_MessageBoxColorType
		{
			SDL_MESSAGEBOX_COLOR_BACKGROUND,
			SDL_MESSAGEBOX_COLOR_TEXT,
			SDL_MESSAGEBOX_COLOR_BUTTON_BORDER,
			SDL_MESSAGEBOX_COLOR_BUTTON_BACKGROUND,
			SDL_MESSAGEBOX_COLOR_BUTTON_SELECTED,
			SDL_MESSAGEBOX_COLOR_MAX
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MessageBoxColorScheme
		{
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = (int)SDL_MessageBoxColorType.SDL_MESSAGEBOX_COLOR_MAX)]
				public SDL_MessageBoxColor[] colors;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct INTERNAL_SDL_MessageBoxData
		{
			public SDL_MessageBoxFlags flags;
			public IntPtr window;				/* Parent window, can be NULL */
			public IntPtr title;				/* UTF-8 title */
			public IntPtr message;				/* UTF-8 message text */
			public int numbuttons;
			public IntPtr buttons;
			public IntPtr colorScheme;			/* Can be NULL to use system settings */
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MessageBoxData
		{
			public SDL_MessageBoxFlags flags;
			public IntPtr window;				/* Parent window, can be NULL */
			public string title;				/* UTF-8 title */
			public string message;				/* UTF-8 message text */
			public int numbuttons;
			public SDL_MessageBoxButtonData[] buttons;
			public SDL_MessageBoxColorScheme? colorScheme;	/* Can be NULL to use system settings */
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="messageboxdata"></param>
		/// <param name="buttonid"></param>
		/// <returns></returns>
		[DllImport(nativeLibName, EntryPoint = "SDL_ShowMessageBox", CallingConvention = CallingConvention.Cdecl)]
		private static extern int INTERNAL_SDL_ShowMessageBox([In()] ref INTERNAL_SDL_MessageBoxData messageboxdata, out int buttonid);

		/// <summary>
		///
		/// </summary>
		/// <param name="messageboxdata"></param>
		/// <param name="buttonid"></param>
		/// <returns></returns>
		public static unsafe int SDL_ShowMessageBox([In()] ref SDL_MessageBoxData messageboxdata, out int buttonid)
		{
			var utf8 = LPUtf8StrMarshaler.GetInstance(null);

			var data = new INTERNAL_SDL_MessageBoxData()
			{
				flags = messageboxdata.flags,
				window = messageboxdata.window,
				title = utf8.MarshalManagedToNative(messageboxdata.title),
				message = utf8.MarshalManagedToNative(messageboxdata.message),
				numbuttons = messageboxdata.numbuttons,
			};

			var buttons = new INTERNAL_SDL_MessageBoxButtonData[messageboxdata.numbuttons];
			for (int i = 0; i < messageboxdata.numbuttons; i++)
			{
				buttons[i] = new INTERNAL_SDL_MessageBoxButtonData()
				{
					flags = messageboxdata.buttons[i].flags,
					buttonid = messageboxdata.buttons[i].buttonid,
					text = utf8.MarshalManagedToNative(messageboxdata.buttons[i].text),
				};
			}

			if (messageboxdata.colorScheme != null)
			{
				data.colorScheme = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDL_MessageBoxColorScheme)));
				Marshal.StructureToPtr(messageboxdata.colorScheme.Value, data.colorScheme, false);
			}

			int result;
			fixed (INTERNAL_SDL_MessageBoxButtonData* buttonsPtr = &buttons[0])
			{
				data.buttons = (IntPtr)buttonsPtr;
				result = INTERNAL_SDL_ShowMessageBox(ref data, out buttonid);
			}

			Marshal.FreeHGlobal(data.colorScheme);
			for (int i = 0; i < messageboxdata.numbuttons; i++)
			{
				utf8.CleanUpNativeData(buttons[i].text);
			}
			utf8.CleanUpNativeData(data.message);
			utf8.CleanUpNativeData(data.title);

			return result;
		}

		/// <summary>
		/// Use this function to display a simple message box.
		/// </summary>
		/// <param name="flags">An <see cref="SDL_MessageBoxFlag"/>; see Remarks for details;</param>
		/// <param name="title">UTF-8 title text</param>
		/// <param name="message">UTF-8 message text</param>
		/// <param name="window">the parent window, or NULL for no parent (refers to a <see cref="SDL_Window"/></param>
		/// <returns>0 on success or a negative error code on failure; call SDL_GetError() for more information. </returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_ShowSimpleMessageBox(
			SDL_MessageBoxFlags flags,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string title,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string message,
			IntPtr window
		);

		#endregion

		#region SDL_version.h, SDL_revision.h

		/* Similar to the headers, this is the version we're expecting to be
		 * running with. You will likely want to check this somewhere in your
		 * program!
		 */
		public const int SDL_MAJOR_VERSION =	2;
		public const int SDL_MINOR_VERSION =	0;
		public const int SDL_PATCHLEVEL =	4;

		public static readonly int SDL_COMPILEDVERSION = SDL_VERSIONNUM(
			SDL_MAJOR_VERSION,
			SDL_MINOR_VERSION,
			SDL_PATCHLEVEL
		);

		/// <summary>
		/// A structure that contains information about the version of SDL in use.
		/// </summary>
		/// <remarks>Represents the library's version as three levels: </remarks>
		/// <remarks>major revision (increments with massive changes, additions, and enhancements) </remarks>
		/// <remarks>minor revision (increments with backwards-compatible changes to the major revision), and </remarks>
		/// <remarks>patchlevel (increments with fixes to the minor revision)</remarks>
		/// <remarks><see cref="SDL_VERSION"/> can be used to populate this structure with information</remarks>
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_version
		{
			public byte major;
			public byte minor;
			public byte patch;
		}

		/// <summary>
		/// Use this macro to determine the SDL version your program was compiled against.
		/// </summary>
		/// <param name="x">an <see cref="SDL_version"/> structure to initialize</param>
		public static void SDL_VERSION(out SDL_version x)
		{
			x.major = SDL_MAJOR_VERSION;
			x.minor = SDL_MINOR_VERSION;
			x.patch = SDL_PATCHLEVEL;
		}

		/// <summary>
		/// Use this macro to convert separate version components into a single numeric value.
		/// </summary>
		/// <param name="X">major version; reported in thousands place</param>
		/// <param name="Y">minor version; reported in hundreds place</param>
		/// <param name="Z">update version (patchlevel); reported in tens and ones places</param>
		/// <returns></returns>
		/// <remarks>This assumes that there will never be more than 100 patchlevels.</remarks>
		/// <remarks>Example: SDL_VERSIONNUM(1,2,3) -> (1203)</remarks>
		public static int SDL_VERSIONNUM(int X, int Y, int Z)
		{
			return (X * 1000) + (Y * 100) + Z;
		}

		/// <summary>
		/// Use this macro to determine whether the SDL version compiled against is at least as new as the specified version.
		/// </summary>
		/// <param name="X">major version</param>
		/// <param name="Y">minor version</param>
		/// <param name="Z">update version (patchlevel)</param>
		/// <returns>This macro will evaluate to true if compiled with SDL version at least X.Y.Z. </returns>
		public static bool SDL_VERSION_ATLEAST(int X, int Y, int Z)
		{
			return (SDL_COMPILEDVERSION >= SDL_VERSIONNUM(X, Y, Z));
		}

		/// <summary>
		/// Use this function to get the version of SDL that is linked against your program.
		/// </summary>
		/// <param name="ver">the <see cref="SDL_version"/> structure that contains the version information</param>
		/// <remarks>This function may be called safely at any time, even before SDL_Init(). </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GetVersion(out SDL_version ver);

		/// <summary>
		/// Use this function to get the code revision of SDL that is linked against your program.
		/// </summary>
		/// <returns>Returns an arbitrary string, uniquely identifying the exact revision
		/// of the SDL library in use. </returns>
		/// <remarks>The revision is a string including sequential revision number that is
		/// incremented with each commit, and a hash of the last code change.</remarks>
		/// <remarks>Example: hg-5344:94189aa89b54</remarks>
		/// <remarks>This value is the revision of the code you are linked with and may be
		/// different from the code you are compiling with, which is found in the constant SDL_REVISION.</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GetRevision();

		/// <summary>
		/// Use this function to get the revision number of SDL that is linked against your program.
		/// </summary>
		/// <returns>Returns a number uniquely identifying the exact revision of the SDL library in use.</returns>
		/// <remarks>This is an incrementing number based on commits to hg.libsdl.org.</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetRevisionNumber();

		#endregion

		#region SDL_video.h

		/* Actually, this is from SDL_blendmode.h */
		/// <summary>
		/// An enumeration of blend modes used in SDL_RenderCopy() and drawing operations.
		/// </summary>
		[Flags]
		public enum SDL_BlendMode
		{
			SDL_BLENDMODE_NONE =	0x00000000,
			SDL_BLENDMODE_BLEND =	0x00000001,
			SDL_BLENDMODE_ADD =	0x00000002,
			SDL_BLENDMODE_MOD =	0x00000004
		}

		/// <summary>
		/// An enumeration of OpenGL configuration attributes.
		/// </summary>
		public enum SDL_GLattr
		{
			SDL_GL_RED_SIZE,
			SDL_GL_GREEN_SIZE,
			SDL_GL_BLUE_SIZE,
			SDL_GL_ALPHA_SIZE,
			SDL_GL_BUFFER_SIZE,
			SDL_GL_DOUBLEBUFFER,
			SDL_GL_DEPTH_SIZE,
			SDL_GL_STENCIL_SIZE,
			SDL_GL_ACCUM_RED_SIZE,
			SDL_GL_ACCUM_GREEN_SIZE,
			SDL_GL_ACCUM_BLUE_SIZE,
			SDL_GL_ACCUM_ALPHA_SIZE,
			SDL_GL_STEREO,
			SDL_GL_MULTISAMPLEBUFFERS,
			SDL_GL_MULTISAMPLESAMPLES,
			SDL_GL_ACCELERATED_VISUAL,
			SDL_GL_RETAINED_BACKING,
			SDL_GL_CONTEXT_MAJOR_VERSION,
			SDL_GL_CONTEXT_MINOR_VERSION,
			SDL_GL_CONTEXT_EGL,
			SDL_GL_CONTEXT_FLAGS,
			SDL_GL_CONTEXT_PROFILE_MASK,
			SDL_GL_SHARE_WITH_CURRENT_CONTEXT,
			SDL_GL_FRAMEBUFFER_SRGB_CAPABLE,
			SDL_GL_CONTEXT_RELEASE_BEHAVIOR
		}

		/// <summary>
		/// An enumeration of OpenGL profiles.
		/// </summary>
		[Flags]
		public enum SDL_GLprofile
		{
			SDL_GL_CONTEXT_PROFILE_CORE				= 0x0001,
			SDL_GL_CONTEXT_PROFILE_COMPATIBILITY	= 0x0002,
			SDL_GL_CONTEXT_PROFILE_ES				= 0x0004
		}

		/// <summary>
		/// This enumeration is used in conjunction with SDL_GL_SetAttribute
		/// and SDL_GL_CONTEXT_FLAGS. Multiple flags can be OR'd together.
		/// </summary>
		[Flags]
		public enum SDL_GLcontext
		{
			SDL_GL_CONTEXT_DEBUG_FLAG				= 0x0001,
			SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG	= 0x0002,
			SDL_GL_CONTEXT_ROBUST_ACCESS_FLAG		= 0x0004,
			SDL_GL_CONTEXT_RESET_ISOLATION_FLAG		= 0x0008
		}

		/// <summary>
		/// An enumeration of window events.
		/// </summary>
		public enum SDL_WindowEventID : byte
		{
			SDL_WINDOWEVENT_NONE,
			SDL_WINDOWEVENT_SHOWN,
			SDL_WINDOWEVENT_HIDDEN,
			SDL_WINDOWEVENT_EXPOSED,
			SDL_WINDOWEVENT_MOVED,
			SDL_WINDOWEVENT_RESIZED,
			SDL_WINDOWEVENT_SIZE_CHANGED,
			SDL_WINDOWEVENT_MINIMIZED,
			SDL_WINDOWEVENT_MAXIMIZED,
			SDL_WINDOWEVENT_RESTORED,
			SDL_WINDOWEVENT_ENTER,
			SDL_WINDOWEVENT_LEAVE,
			SDL_WINDOWEVENT_FOCUS_GAINED,
			SDL_WINDOWEVENT_FOCUS_LOST,
			SDL_WINDOWEVENT_CLOSE,
		}

		/// <summary>
		/// An enumeration of window states.
		/// </summary>
		[Flags]
		public enum SDL_WindowFlags : uint
		{
			SDL_WINDOW_FULLSCREEN =		0x00000001,
			SDL_WINDOW_OPENGL =		0x00000002,
			SDL_WINDOW_SHOWN =		0x00000004,
			SDL_WINDOW_HIDDEN =		0x00000008,
			SDL_WINDOW_BORDERLESS =		0x00000010,
			SDL_WINDOW_RESIZABLE =		0x00000020,
			SDL_WINDOW_MINIMIZED =		0x00000040,
			SDL_WINDOW_MAXIMIZED =		0x00000080,
			SDL_WINDOW_INPUT_GRABBED =	0x00000100,
			SDL_WINDOW_INPUT_FOCUS =	0x00000200,
			SDL_WINDOW_MOUSE_FOCUS =	0x00000400,
			SDL_WINDOW_FULLSCREEN_DESKTOP =
				(SDL_WINDOW_FULLSCREEN | 0x00001000),
			SDL_WINDOW_FOREIGN =		0x00000800,
			SDL_WINDOW_ALLOW_HIGHDPI =	0x00002000,	/* Only available in 2.0.1 */
			SDL_WINDOW_MOUSE_CAPTURE =	0x00004000,	/* Only available in 2.0.4 */
		}

		/// <summary>
		/// Possible return values from the SDL_HitTest callback.
		/// This is only available in 2.0.4.
		/// </summary>
		public enum SDL_HitTestResult
		{
			SDL_HITTEST_NORMAL,		/* Region is normal. No special properties. */
			SDL_HITTEST_DRAGGABLE,		/* Region can drag entire window. */
			SDL_HITTEST_RESIZE_TOPLEFT,
			SDL_HITTEST_RESIZE_TOP,
			SDL_HITTEST_RESIZE_TOPRIGHT,
			SDL_HITTEST_RESIZE_RIGHT,
			SDL_HITTEST_RESIZE_BOTTOMRIGHT,
			SDL_HITTEST_RESIZE_BOTTOM,
			SDL_HITTEST_RESIZE_BOTTOMLEFT,
			SDL_HITTEST_RESIZE_LEFT
		}

		public const int SDL_WINDOWPOS_UNDEFINED_MASK =	0x1FFF0000;
		public const int SDL_WINDOWPOS_CENTERED_MASK =	0x2FFF0000;
		public const int SDL_WINDOWPOS_UNDEFINED =		0x1FFF0000;
		public const int SDL_WINDOWPOS_CENTERED =		0x2FFF0000;

		public static int SDL_WINDOWPOS_UNDEFINED_DISPLAY(int X)
		{
			return (SDL_WINDOWPOS_UNDEFINED_MASK | X);
		}

		public static bool SDL_WINDOWPOS_ISUNDEFINED(int X)
		{
			return (X & 0xFFFF0000) == SDL_WINDOWPOS_UNDEFINED_MASK;
		}

		public static int SDL_WINDOWPOS_CENTERED_DISPLAY(int X)
		{
			return (SDL_WINDOWPOS_CENTERED_MASK | X);
		}

		public static bool SDL_WINDOWPOS_ISCENTERED(int X)
		{
			return (X & 0xFFFF0000) == SDL_WINDOWPOS_CENTERED_MASK;
		}

		/// <summary>
		/// A structure that describes a display mode.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_DisplayMode
		{
			public uint format;
			public int w;
			public int h;
			public int refresh_rate;
			public IntPtr driverdata; // void*
		}

		/* win refers to an SDL_Window*, area to a cosnt SDL_Point*, data to a void* */
		/* Only available in 2.0.4 */
		public delegate SDL_HitTestResult SDL_HitTest(IntPtr win, IntPtr area, IntPtr data);

		/// <summary>
		/// Use this function to create a window with the specified position, dimensions, and flags.
		/// </summary>
		/// <param name="title">the title of the window, in UTF-8 encoding</param>
		/// <param name="x">the x position of the window, SDL_WINDOWPOS_CENTERED, or SDL_WINDOWPOS_UNDEFINED</param>
		/// <param name="y">the y position of the window, SDL_WINDOWPOS_CENTERED, or SDL_WINDOWPOS_UNDEFINED</param>
		/// <param name="w">the width of the window</param>
		/// <param name="h">the height of the window</param>
		/// <param name="flags">0, or one or more <see cref="SDL_WindowFlags"/> OR'd together;
		/// see Remarks for details</param>
		/// <returns>Returns the window that was created or NULL on failure; call <see cref="SDL_GetError()"/>
		/// for more information. (refers to an <see cref="SDL_Window"/>)</returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_CreateWindow(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string title,
			int x,
			int y,
			int w,
			int h,
			SDL_WindowFlags flags
		);

		/// <summary>
		/// Use this function to create a window and default renderer.
		/// </summary>
		/// <param name="width">The width of the window</param>
		/// <param name="height">The height of the window</param>
		/// <param name="window_flags">The flags used to create the window (see <see cref="SDL_CreateWindow()"/>)</param>
		/// <param name="window">A pointer filled with the window, or NULL on error (<see cref="SDL_Window*"/>)</param>
		/// <param name="renderer">A pointer filled with the renderer, or NULL on error <see cref="(SDL_Renderer*)"/></param>
		/// <returns>Returns 0 on success, or -1 on error; call <see cref="SDL_GetError()"/> for more information. </returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_CreateWindowAndRenderer(
			int width,
			int height,
			SDL_WindowFlags window_flags,
			out IntPtr window,
			out IntPtr renderer
		);

		/// <summary>
		/// Use this function to create an SDL window from an existing native window.
		/// </summary>
		/// <param name="data">a pointer to driver-dependent window creation data, typically your native window cast to a void*</param>
		/// <returns>Returns the window (<see cref="SDL_Window"/>) that was created or NULL on failure;
		/// call <see cref="SDL_GetError()"/> for more information. </returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_CreateWindowFrom(IntPtr data);

		/// <summary>
		/// Use this function to destroy a window.
		/// </summary>
		/// <param name="window">the window to destroy (<see cref="SDL_Window"/>)</param>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_DestroyWindow(IntPtr window);

		/// <summary>
		/// Use this function to prevent the screen from being blanked by a screen saver.
		/// </summary>
		/// <remarks>If you disable the screensaver, it is automatically re-enabled when SDL quits. </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_DisableScreenSaver();

		/// <summary>
		/// Use this function to allow the screen to be blanked by a screen saver.
		/// </summary>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_EnableScreenSaver();

		/* IntPtr refers to an SDL_DisplayMode. Just use closest. */
		/// <summary>
		/// Use this function to get the closest match to the requested display mode.
		/// </summary>
		/// <param name="displayIndex">the index of the display to query</param>
		/// <param name="mode">an <see cref="SDL_DisplayMode"/> structure containing the desired display mode </param>
		/// <param name="closest">an <see cref="SDL_DisplayMode"/> structure filled in with
		/// the closest match of the available display modes </param>
		/// <returns>Returns the passed in value closest or NULL if no matching video mode was available;
		/// (refers to a <see cref="SDL_DisplayMode"/>)
		/// call <see cref="SDL_GetError()"/> for more information. </returns>
		/// <remarks>The available display modes are scanned and closest is filled in with the closest mode
		/// matching the requested mode and returned. The mode format and refresh rate default to the desktop
		/// mode if they are set to 0. The modes are scanned with size being first priority, format being
		/// second priority, and finally checking the refresh rate. If all the available modes are too small,
		/// then NULL is returned. </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GetClosestDisplayMode(
			int displayIndex,
			ref SDL_DisplayMode mode,
			out SDL_DisplayMode closest
		);

		/// <summary>
		/// Use this function to get information about the current display mode.
		/// </summary>
		/// <param name="displayIndex">the index of the display to query</param>
		/// <param name="mode">an <see cref="SDL_DisplayMode"/> structure filled in with the current display mode</param>
		/// <returns>Returns 0 on success or a negative error code on failure;
		/// call <see cref="SDL_GetError()"/> for more information. </returns>
		/// <remarks>There's a difference between this function and <see cref="SDL_GetDesktopDisplayMode"/> when SDL
		/// runs fullscreen and has changed the resolution. In that case this function will return the
		/// current display mode, and not the previous native display mode. </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetCurrentDisplayMode(
			int displayIndex,
			out SDL_DisplayMode mode
		);

		/// <summary>
		/// Use this function to return the name of the currently initialized video driver.
		/// </summary>
		/// <returns>Returns the name of the current video driver or NULL if no driver has been initialized. </returns>
		/// <remarks>There's a difference between this function and <see cref="SDL_GetCurrentDisplayMode"/> when SDL
		/// runs fullscreen and has changed the resolution. In that case this function will return the
		/// previous native display mode, and not the current display mode. </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GetCurrentVideoDriver();

		/// <summary>
		/// Use this function to get information about the desktop display mode.
		/// </summary>
		/// <param name="displayIndex">the index of the display to query</param>
		/// <param name="mode">an <see cref="SDL_DisplayMode"/> structure filled in with the current display mode</param>
		/// <returns>Returns 0 on success or a negative error code on failure;
		/// call <see cref="SDL_GetError()"/> for more information. </returns>
		/// <remarks>There's a difference between this function and <see cref="SDL_GetCurrentDisplayMode"/> when SDL
		/// runs fullscreen and has changed the resolution. In that case this function will return the
		/// previous native display mode, and not the current display mode. </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetDesktopDisplayMode(
			int displayIndex,
			out SDL_DisplayMode mode
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GetDisplayName(int index);

		/// <summary>
		/// Use this function to get the desktop area represented by a display, with the primary display located at 0,0.
		/// </summary>
		/// <param name="displayIndex">the index of the display to query</param>
		/// <param name="rect">the <see cref="SDL_Rect"/> structure filled in with the display bounds</param>
		/// <returns>Returns 0 on success or a negative error code on failure;
		/// call <see cref="SDL_GetError()"/> for more information. </returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetDisplayBounds(
			int displayIndex,
			out SDL_Rect rect
		);

		/* This function is only available in 2.0.4 or higher */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetDisplayDPI(
			int displayIndex,
			out float ddpi,
			out float hdpi,
			out float vdpi
		);

		/// <summary>
		/// Use this function to get information about a specific display mode.
		/// </summary>
		/// <param name="displayIndex">the index of the display to query</param>
		/// <param name="modeIndex">the index of the display mode to query</param>
		/// <param name="mode">an <see cref="SDL_DisplayMode"/> structure filled in with the mode at modeIndex</param>
		/// <returns>Returns 0 on success or a negative error code on failure;
		/// call <see cref="SDL_GetError()"/> for more information. </returns>
		/// <remarks>The display modes are sorted in this priority:
		/// <remarks>bits per pixel -> more colors to fewer colors</remarks>
		/// <remarks>width -> largest to smallest</remarks>
		/// <remarks>height -> largest to smallest</remarks>
		/// <remarks>refresh rate -> highest to lowest</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetDisplayMode(
			int displayIndex,
			int modeIndex,
			out SDL_DisplayMode mode
		);

		/// <summary>
		/// Use this function to return the number of available display modes.
		/// </summary>
		/// <param name="displayIndex">the index of the display to query</param>
		/// <returns>Returns a number >= 1 on success or a negative error code on failure;
		/// call <see cref="SDL_GetError()"/> for more information. </returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetNumDisplayModes(
			int displayIndex
		);

		/// <summary>
		/// Use this function to return the number of available video displays.
		/// </summary>
		/// <returns>Returns a number >= 1 or a negative error code on failure;
		/// call <see cref="SDL_GetError()"/> for more information. </returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetNumVideoDisplays();

		/// <summary>
		/// Use this function to get the number of video drivers compiled into SDL.
		/// </summary>
		/// <returns>Returns a number >= 1 on success or a negative error code on failure;
		/// call <see cref="SDL_GetError()"/> for more information. </returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetNumVideoDrivers();

		/// <summary>
		/// Use this function to get the name of a built in video driver.
		/// </summary>
		/// <param name="index">the index of a video driver</param>
		/// <returns>Returns the name of the video driver with the given index. </returns>
		/// <remarks>The video drivers are presented in the order in which they are normally checked during initialization. </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GetVideoDriver(
			int index
		);

		/// <summary>
		/// Use this function to get the brightness (gamma correction) for a window.
		/// </summary>
		/// <param name="window">the window to query (<see cref="SDL_Window"/>)</param>
		/// <returns>Returns the brightness for the window where 0.0 is completely dark and 1.0 is normal brightness. </returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern float SDL_GetWindowBrightness(
			IntPtr window
		);

		/// <summary>
		/// Use this function to retrieve the data pointer associated with a window.
		/// </summary>
		/// <param name="window">the window to query (<see cref="SDL_Window"/>)</param>
		/// <param name="name">the name of the pointer</param>
		/// <returns>Returns the value associated with name. (void*)</returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GetWindowData(
			IntPtr window,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string name
		);

		/// <summary>
		/// Use this function to get the index of the display associated with a window.
		/// </summary>
		/// <param name="window">the window to query (<see cref="SDL_Window"/>)</param>
		/// <returns>Returns the index of the display containing the center of the window
		/// on success or a negative error code on failure;
		/// call <see cref="SDL_GetError()"/> for more information. </returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetWindowDisplayIndex(
			IntPtr window
		);

		/// <summary>
		/// Use this function to fill in information about the display mode to use when a window is visible at fullscreen.
		/// </summary>
		/// <param name="window">the window to query (<see cref="SDL_Window"/>)</param>
		/// <param name="mode">an <see cref="SDL_DisplayMode"/> structure filled in with the fullscreen display mode</param>
		/// <returns>Returns 0 on success or a negative error code on failure;
		/// call <see cref="SDL_GetError()"/> for more information. </returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetWindowDisplayMode(
			IntPtr window,
			out SDL_DisplayMode mode
		);

		/// <summary>
		/// Use this function to get the window flags.
		/// </summary>
		/// <param name="window">the window to query (<see cref="SDL_Window"/>)</param>
		/// <returns>Returns a mask of the <see cref="SDL_WindowFlags"/> associated with window; see Remarks for details.</returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_GetWindowFlags(IntPtr window);

		/// <summary>
		/// Use this function to get a window from a stored ID.
		/// </summary>
		/// <param name="id">the ID of the window</param>
		/// <returns>Returns the window associated with id or NULL if it doesn't exist (<see cref="SDL_Window"/>);
		/// call <see cref="SDL_GetError()"/> for more information. </returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GetWindowFromID(uint id);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetWindowGammaRamp(
			IntPtr window,
			[Out()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeConst = 256)]
				ushort[] red,
			[Out()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeConst = 256)]
				ushort[] green,
			[Out()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeConst = 256)]
				ushort[] blue
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_GetWindowGrab(IntPtr window);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_GetWindowID(IntPtr window);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_GetWindowPixelFormat(
			IntPtr window
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GetWindowMaximumSize(
			IntPtr window,
			out int max_w,
			out int max_h
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GetWindowMinimumSize(
			IntPtr window,
			out int min_w,
			out int min_h
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GetWindowPosition(
			IntPtr window,
			out int x,
			out int y
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GetWindowSize(
			IntPtr window,
			out int w,
			out int h
		);

		/* IntPtr refers to an SDL_Surface*, window to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GetWindowSurface(IntPtr window);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GetWindowTitle(
			IntPtr window
		);

		/* texture refers to an SDL_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_BindTexture(
			IntPtr texture,
			out float texw,
			out float texh
		);

		/* IntPtr and window refer to an SDL_GLContext and SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GL_CreateContext(IntPtr window);

		/* context refers to an SDL_GLContext */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GL_DeleteContext(IntPtr context);

		/* IntPtr refers to a function pointer */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GL_GetProcAddress(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string proc
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_GL_ExtensionSupported(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string extension
		);

		/* Only available in SDL 2.0.2 or higher */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GL_ResetAttributes();

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_GetAttribute(
			SDL_GLattr attr,
			out int value
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_GetSwapInterval();

		/* window and context refer to an SDL_Window* and SDL_GLContext */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_MakeCurrent(
			IntPtr window,
			IntPtr context
		);

		/* IntPtr refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GL_GetCurrentWindow();

		/* IntPtr refers to an SDL_Context */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GL_GetCurrentContext();

		/* window refers to an SDL_Window*, This function is only available in SDL 2.0.1 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GL_GetDrawableSize(
			IntPtr window,
			out int w,
			out int h
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_SetAttribute(
			SDL_GLattr attr,
			int value
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_SetSwapInterval(int interval);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GL_SwapWindow(IntPtr window);

		/* texture refers to an SDL_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_UnbindTexture(IntPtr texture);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_HideWindow(IntPtr window);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_IsScreenSaverEnabled();

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_MaximizeWindow(IntPtr window);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_MinimizeWindow(IntPtr window);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_RaiseWindow(IntPtr window);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_RestoreWindow(IntPtr window);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetWindowBrightness(
			IntPtr window,
			float brightness
		);

		/* IntPtr and userdata are void*, window is an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_SetWindowData(
			IntPtr window,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string name,
			IntPtr userdata
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetWindowDisplayMode(
			IntPtr window,
			ref SDL_DisplayMode mode
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetWindowFullscreen(
			IntPtr window,
			uint flags
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetWindowGammaRamp(
			IntPtr window,
			[In()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeConst = 256)]
				ushort[] red,
			[In()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeConst = 256)]
				ushort[] green,
			[In()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeConst = 256)]
				ushort[] blue
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowGrab(
			IntPtr window,
			SDL_bool grabbed
		);

		/* window refers to an SDL_Window*, icon to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowIcon(
			IntPtr window,
			IntPtr icon
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowMaximumSize(
			IntPtr window,
			int max_w,
			int max_h
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowMinimumSize(
			IntPtr window,
			int min_w,
			int min_h
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowPosition(
			IntPtr window,
			int x,
			int y
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowSize(
			IntPtr window,
			int w,
			int h
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowBordered(
			IntPtr window,
			SDL_bool bordered
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowTitle(
			IntPtr window,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string title
		);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_ShowWindow(IntPtr window);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_UpdateWindowSurface(IntPtr window);

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_UpdateWindowSurfaceRects(
			IntPtr window,
			[In()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 2)]
				SDL_Rect[] rects,
			int numrects
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_VideoInit(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string driver_name
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_VideoQuit();

		/* window refers to an SDL_Window*, callback_data to a void* */
		/* Only available in 2.0.4 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetWindowHitTest(
			IntPtr window,
			SDL_HitTest callback,
			IntPtr callback_data
		);

		/* IntPtr refers to an SDL_Window* */
		/* Only available in 2.0.4 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GetGrabbedWindow();

		#endregion

		#region SDL_render.h

		[Flags]
		public enum SDL_RendererFlags : uint
		{
			SDL_RENDERER_SOFTWARE =		0x00000001,
			SDL_RENDERER_ACCELERATED =	0x00000002,
			SDL_RENDERER_PRESENTVSYNC =	0x00000004,
			SDL_RENDERER_TARGETTEXTURE =	0x00000008
		}

		[Flags]
		public enum SDL_RendererFlip
		{
			SDL_FLIP_NONE =		0x00000000,
			SDL_FLIP_HORIZONTAL =	0x00000001,
			SDL_FLIP_VERTICAL =	0x00000002
		}

		public enum SDL_TextureAccess
		{
			SDL_TEXTUREACCESS_STATIC,
			SDL_TEXTUREACCESS_STREAMING,
			SDL_TEXTUREACCESS_TARGET
		}

		[Flags]
		public enum SDL_TextureModulate
		{
			SDL_TEXTUREMODULATE_NONE =		0x00000000,
			SDL_TEXTUREMODULATE_HORIZONTAL =	0x00000001,
			SDL_TEXTUREMODULATE_VERTICAL =		0x00000002
		}

		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct SDL_RendererInfo
		{
			public IntPtr name; // const char*
			public uint flags;
			public uint num_texture_formats;
			public fixed uint texture_formats[16];
			public int max_texture_width;
			public int max_texture_height;
		}

		/* IntPtr refers to an SDL_Renderer*, window to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_CreateRenderer(
			IntPtr window,
			int index,
			SDL_RendererFlags flags
		);

		/* IntPtr refers to an SDL_Renderer*, surface to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_CreateSoftwareRenderer(IntPtr surface);

		/* IntPtr refers to an SDL_Texture*, renderer to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_CreateTexture(
			IntPtr renderer,
			uint format,
			int access,
			int w,
			int h
		);

		/* IntPtr refers to an SDL_Texture*
		 * renderer refers to an SDL_Renderer*
		 * surface refers to an SDL_Surface*
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_CreateTextureFromSurface(
			IntPtr renderer,
			IntPtr surface
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_DestroyRenderer(IntPtr renderer);

		/* texture refers to an SDL_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_DestroyTexture(IntPtr texture);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetNumRenderDrivers();

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetRenderDrawBlendMode(
			IntPtr renderer,
			out SDL_BlendMode blendMode
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetRenderDrawColor(
			IntPtr renderer,
			out byte r,
			out byte g,
			out byte b,
			out byte a
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetRenderDriverInfo(
			int index,
			out SDL_RendererInfo info
		);

		/* IntPtr refers to an SDL_Renderer*, window to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GetRenderer(IntPtr window);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetRendererInfo(
			IntPtr renderer,
			out SDL_RendererInfo info
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetRendererOutputSize(
			IntPtr renderer,
			out int w,
			out int h
		);

		/* texture refers to an SDL_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetTextureAlphaMod(
			IntPtr texture,
			out byte alpha
		);

		/* texture refers to an SDL_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetTextureBlendMode(
			IntPtr texture,
			out SDL_BlendMode blendMode
		);

		/* texture refers to an SDL_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetTextureColorMod(
			IntPtr texture,
			out byte r,
			out byte g,
			out byte b
		);

		/// <summary>
		/// Use this function to lock a portion of the texture for write-only pixel access.
		/// </summary>
		/// <param name="texture">the texture to lock for access, which was created with
		/// SDL_TEXTUREACCESS_STREAMING (refers to a SDL_Texture*)</param>
		/// <param name="rect">an SDL_Rect structure representing the area to lock for access;
		/// NULL to lock the entire texture </param>
		/// <param name="pixels">this is filled in with a pointer to the locked pixels, appropriately
		/// offset by the locked area (refers to a void*)</param>
		/// <param name="pitch">this is filled in with the pitch of the locked pixels </param>
		/// <returns>Returns 0 on success or a negative error code if the texture is not valid or
		/// was not created with SDL_TEXTUREACCESS_STREAMING; call <see cref="SDL_GetError()"/> for more information. </returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_LockTexture(
			IntPtr texture,
			ref SDL_Rect rect,
			out IntPtr pixels,
			out int pitch
		);

		/// <summary>
		/// Use this function to lock a portion of the texture for write-only pixel access. This overload
		/// allows for passing an IntPtr.Zero (null) rect value to lock the entire texture.
		/// </summary>
		/// <param name="texture">the texture to lock for access, which was created with
		/// SDL_TEXTUREACCESS_STREAMING (refers to a SDL_Texture*)</param>
		/// <param name="rect">an SDL_Rect structure representing the area to lock for access;
		/// NULL to lock the entire texture </param>
		/// <param name="pixels">this is filled in with a pointer to the locked pixels, appropriately
		/// offset by the locked area (refers to a void*)</param>
		/// <param name="pitch">this is filled in with the pitch of the locked pixels </param>
		/// <returns>Returns 0 on success or a negative error code if the texture is not valid or
		/// was not created with SDL_TEXTUREACCESS_STREAMING; call <see cref="SDL_GetError()"/> for more information. </returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_LockTexture(
			IntPtr texture,
			IntPtr rect,
			out IntPtr pixels,
			out int pitch
		);

		/* texture refers to an SDL_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_QueryTexture(
			IntPtr texture,
			out uint format,
			out int access,
			out int w,
			out int h
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderClear(IntPtr renderer);

		/* renderer refers to an SDL_Renderer*, texture to an SDL_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderCopy(
			IntPtr renderer,
			IntPtr texture,
			ref SDL_Rect srcrect,
			ref SDL_Rect dstrect
		);

		/* renderer refers to an SDL_Renderer*, texture to an SDL_Texture*.
		 * Internally, this function contains logic to use default values when
		 * source and destination rectangles are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for srcrect.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderCopy(
			IntPtr renderer,
			IntPtr texture,
			IntPtr srcrect,
			ref SDL_Rect dstrect
		);

		/* renderer refers to an SDL_Renderer*, texture to an SDL_Texture*.
		 * Internally, this function contains logic to use default values when
		 * source and destination rectangles are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for dstrect.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderCopy(
			IntPtr renderer,
			IntPtr texture,
			ref SDL_Rect srcrect,
			IntPtr dstrect
		);

		/* renderer refers to an SDL_Renderer*, texture to an SDL_Texture*.
		 * Internally, this function contains logic to use default values when
		 * source and destination rectangles are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for both SDL_Rects.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderCopy(
			IntPtr renderer,
			IntPtr texture,
			IntPtr srcrect,
			IntPtr dstrect
		);

		/* renderer refers to an SDL_Renderer*, texture to an SDL_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderCopyEx(
			IntPtr renderer,
			IntPtr texture,
			ref SDL_Rect srcrect,
			ref SDL_Rect dstrect,
			double angle,
			ref SDL_Point center,
			SDL_RendererFlip flip
		);

		/* renderer refers to an SDL_Renderer*, texture to an SDL_Texture*.
		 * Internally, this function contains logic to use default values when
		 * source, destination, and/or center are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for srcrect.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderCopyEx(
			IntPtr renderer,
			IntPtr texture,
			IntPtr srcrect,
			ref SDL_Rect dstrect,
			double angle,
			ref SDL_Point center,
			SDL_RendererFlip flip
		);

		/* renderer refers to an SDL_Renderer*, texture to an SDL_Texture*.
		 * Internally, this function contains logic to use default values when
		 * source, destination, and/or center are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for dstrect.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderCopyEx(
			IntPtr renderer,
			IntPtr texture,
			ref SDL_Rect srcrect,
			IntPtr dstrect,
			double angle,
			ref SDL_Point center,
			SDL_RendererFlip flip
		);

		/* renderer refers to an SDL_Renderer*, texture to an SDL_Texture*.
		 * Internally, this function contains logic to use default values when
		 * source, destination, and/or center are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for center.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderCopyEx(
			IntPtr renderer,
			IntPtr texture,
			ref SDL_Rect srcrect,
			ref SDL_Rect dstrect,
			double angle,
			IntPtr center,
			SDL_RendererFlip flip
		);

		/* renderer refers to an SDL_Renderer*, texture to an SDL_Texture*.
		 * Internally, this function contains logic to use default values when
		 * source, destination, and/or center are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for both
		 * srcrect and dstrect.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderCopyEx(
			IntPtr renderer,
			IntPtr texture,
			IntPtr srcrect,
			IntPtr dstrect,
			double angle,
			ref SDL_Point center,
			SDL_RendererFlip flip
		);

		/* renderer refers to an SDL_Renderer*, texture to an SDL_Texture*.
		 * Internally, this function contains logic to use default values when
		 * source, destination, and/or center are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for both
		 * srcrect and center.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderCopyEx(
			IntPtr renderer,
			IntPtr texture,
			IntPtr srcrect,
			ref SDL_Rect dstrect,
			double angle,
			IntPtr center,
			SDL_RendererFlip flip
		);

		/* renderer refers to an SDL_Renderer*, texture to an SDL_Texture*.
		 * Internally, this function contains logic to use default values when
		 * source, destination, and/or center are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for both
		 * dstrect and center.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderCopyEx(
			IntPtr renderer,
			IntPtr texture,
			ref SDL_Rect srcrect,
			IntPtr dstrect,
			double angle,
			IntPtr center,
			SDL_RendererFlip flip
		);

		/* renderer refers to an SDL_Renderer*, texture to an SDL_Texture*.
		 * Internally, this function contains logic to use default values when
		 * source, destination, and/or center are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for all
		 * three parameters.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderCopyEx(
			IntPtr renderer,
			IntPtr texture,
			IntPtr srcrect,
			IntPtr dstrect,
			double angle,
			IntPtr center,
			SDL_RendererFlip flip
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderDrawLine(
			IntPtr renderer,
			int x1,
			int y1,
			int x2,
			int y2
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderDrawLines(
			IntPtr renderer,
			[In()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 2)]
				SDL_Point[] points,
			int count
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderDrawPoint(
			IntPtr renderer,
			int x,
			int y
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderDrawPoints(
			IntPtr renderer,
			[In()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 2)]
				SDL_Point[] points,
			int count
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderDrawRect(
			IntPtr renderer,
			ref SDL_Rect rect
		);

		/* renderer refers to an SDL_Renderer*, rect to an SDL_Rect*.
		 * This overload allows for IntPtr.Zero (null) to be passed for rect.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderDrawRect(
			IntPtr renderer,
			IntPtr rect
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderDrawRects(
			IntPtr renderer,
			[In()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 2)]
				SDL_Rect[] rects,
			int count
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderFillRect(
			IntPtr renderer,
			ref SDL_Rect rect
		);

		/* renderer refers to an SDL_Renderer*, rect to an SDL_Rect*.
		 * This overload allows for IntPtr.Zero (null) to be passed for rect.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderFillRect(
			IntPtr renderer,
			IntPtr rect
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderFillRects(
			IntPtr renderer,
			[In()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 2)]
				SDL_Rect[] rects,
			int count
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_RenderGetClipRect(
			IntPtr renderer,
			out SDL_Rect rect
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_RenderGetLogicalSize(
			IntPtr renderer,
			out int w,
			out int h
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_RenderGetScale(
			IntPtr renderer,
			out float scaleX,
			out float scaleY
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderGetViewport(
			IntPtr renderer,
			out SDL_Rect rect
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_RenderPresent(IntPtr renderer);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderReadPixels(
			IntPtr renderer,
			ref SDL_Rect rect,
			uint format,
			IntPtr pixels,
			int pitch
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderSetClipRect(
			IntPtr renderer,
			ref SDL_Rect rect
		);

		/* renderer refers to an SDL_Renderer*
		 * This overload allows for IntPtr.Zero (null) to be passed for rect.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderSetClipRect(
			IntPtr renderer,
			IntPtr rect
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderSetLogicalSize(
			IntPtr renderer,
			int w,
			int h
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderSetScale(
			IntPtr renderer,
			float scaleX,
			float scaleY
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RenderSetViewport(
			IntPtr renderer,
			ref SDL_Rect rect
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetRenderDrawBlendMode(
			IntPtr renderer,
			SDL_BlendMode blendMode
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetRenderDrawColor(
			IntPtr renderer,
			byte r,
			byte g,
			byte b,
			byte a
		);

		/* renderer refers to an SDL_Renderer*, texture to an SDL_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetRenderTarget(
			IntPtr renderer,
			IntPtr texture
		);

		/* texture refers to an SDL_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetTextureAlphaMod(
			IntPtr texture,
			byte alpha
		);

		/* texture refers to an SDL_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetTextureBlendMode(
			IntPtr texture,
			SDL_BlendMode blendMode
		);

		/* texture refers to an SDL_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetTextureColorMod(
			IntPtr texture,
			byte r,
			byte g,
			byte b
		);

		/* texture refers to an SDL_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_UnlockTexture(IntPtr texture);

		/* texture refers to an SDL_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_UpdateTexture(
			IntPtr texture,
			ref SDL_Rect rect,
			IntPtr pixels,
			int pitch
		);

		/* renderer refers to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_RenderTargetSupported(
			IntPtr renderer
		);

		/* IntPtr refers to an SDL_Texture*, renderer to an SDL_Renderer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GetRenderTarget(IntPtr renderer);

		/* renderer refers to an SDL_Renderer* */
		/* Only available in 2.0.4 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_RenderIsClipEnabled(IntPtr renderer);

		#endregion

		#region SDL_pixels.h

		public static uint SDL_DEFINE_PIXELFOURCC(byte A, byte B, byte C, byte D)
		{
			return SDL_FOURCC(A, B, C, D);
		}

		public static uint SDL_DEFINE_PIXELFORMAT(
			SDL_PIXELTYPE_ENUM type,
			SDL_PIXELORDER_ENUM order,
			SDL_PACKEDLAYOUT_ENUM layout,
			byte bits,
			byte bytes
		) {
			return (uint) (
				(1 << 28) |
				(((byte) type) << 24) |
				(((byte) order) << 20) |
				(((byte) layout) << 16) |
				(bits << 8) |
				(bytes)
			);
		}

		public static byte SDL_PIXELFLAG(uint X)
		{
			return (byte) ((X >> 28) & 0x0F);
		}

		public static byte SDL_PIXELTYPE(uint X)
		{
			return (byte) ((X >> 24) & 0x0F);
		}

		public static byte SDL_PIXELORDER(uint X)
		{
			return (byte) ((X >> 20) & 0x0F);
		}

		public static byte SDL_PIXELLAYOUT(uint X)
		{
			return (byte) ((X >> 16) & 0x0F);
		}

		public static byte SDL_BITSPERPIXEL(uint X)
		{
			return (byte) ((X >> 8) & 0x0F);
		}

		public static byte SDL_BYTESPERPIXEL(uint X)
		{
			if (SDL_ISPIXELFORMAT_FOURCC(X))
			{
				if (	(X == SDL_PIXELFORMAT_YUY2) ||
						(X == SDL_PIXELFORMAT_UYVY) ||
						(X == SDL_PIXELFORMAT_YVYU)	)
				{
					return 2;
				}
				return 1;
			}
			return (byte) (X & 0xFF);
		}

		public static bool SDL_ISPIXELFORMAT_INDEXED(uint format)
		{
			if (SDL_ISPIXELFORMAT_FOURCC(format))
			{
				return false;
			}
			SDL_PIXELTYPE_ENUM pType =
					(SDL_PIXELTYPE_ENUM) SDL_PIXELTYPE(format);
			return (
				pType == SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_INDEX1 ||
				pType == SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_INDEX4 ||
				pType == SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_INDEX8
			);
		}

		public static bool SDL_ISPIXELFORMAT_ALPHA(uint format)
		{
			if (SDL_ISPIXELFORMAT_FOURCC(format))
			{
				return false;
			}
			SDL_PIXELORDER_ENUM pOrder =
					(SDL_PIXELORDER_ENUM) SDL_PIXELORDER(format);
			return (
				pOrder == SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_ARGB ||
				pOrder == SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_RGBA ||
				pOrder == SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_ABGR ||
				pOrder == SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_BGRA
			);
		}

		public static bool SDL_ISPIXELFORMAT_FOURCC(uint format)
		{
			return (format == 0) && (SDL_PIXELFLAG(format) != 1);
		}

		public enum SDL_PIXELTYPE_ENUM
		{
			SDL_PIXELTYPE_UNKNOWN,
			SDL_PIXELTYPE_INDEX1,
			SDL_PIXELTYPE_INDEX4,
			SDL_PIXELTYPE_INDEX8,
			SDL_PIXELTYPE_PACKED8,
			SDL_PIXELTYPE_PACKED16,
			SDL_PIXELTYPE_PACKED32,
			SDL_PIXELTYPE_ARRAYU8,
			SDL_PIXELTYPE_ARRAYU16,
			SDL_PIXELTYPE_ARRAYU32,
			SDL_PIXELTYPE_ARRAYF16,
			SDL_PIXELTYPE_ARRAYF32
		}

		public enum SDL_PIXELORDER_ENUM
		{
			/* BITMAPORDER */
			SDL_BITMAPORDER_NONE,
			SDL_BITMAPORDER_4321,
			SDL_BITMAPORDER_1234,
			/* PACKEDORDER */
			SDL_PACKEDORDER_NONE = 0,
			SDL_PACKEDORDER_XRGB,
			SDL_PACKEDORDER_RGBX,
			SDL_PACKEDORDER_ARGB,
			SDL_PACKEDORDER_RGBA,
			SDL_PACKEDORDER_XBGR,
			SDL_PACKEDORDER_BGRX,
			SDL_PACKEDORDER_ABGR,
			SDL_PACKEDORDER_BGRA,
			/* ARRAYORDER */
			SDL_ARRAYORDER_NONE = 0,
			SDL_ARRAYORDER_RGB,
			SDL_ARRAYORDER_RGBA,
			SDL_ARRAYORDER_ARGB,
			SDL_ARRAYORDER_BGR,
			SDL_ARRAYORDER_BGRA,
			SDL_ARRAYORDER_ABGR
		}

		public enum SDL_PACKEDLAYOUT_ENUM
		{
			SDL_PACKEDLAYOUT_NONE,
			SDL_PACKEDLAYOUT_332,
			SDL_PACKEDLAYOUT_4444,
			SDL_PACKEDLAYOUT_1555,
			SDL_PACKEDLAYOUT_5551,
			SDL_PACKEDLAYOUT_565,
			SDL_PACKEDLAYOUT_8888,
			SDL_PACKEDLAYOUT_2101010,
			SDL_PACKEDLAYOUT_1010102
		}

		public static readonly uint SDL_PIXELFORMAT_UNKNOWN = 0;
		public static readonly uint SDL_PIXELFORMAT_INDEX1LSB =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_INDEX1,
				SDL_PIXELORDER_ENUM.SDL_BITMAPORDER_4321,
				0,
				1, 0
			);
		public static readonly uint SDL_PIXELFORMAT_INDEX1MSB =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_INDEX1,
				SDL_PIXELORDER_ENUM.SDL_BITMAPORDER_1234,
				0,
				1, 0
			);
		public static readonly uint SDL_PIXELFORMAT_INDEX4LSB =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_INDEX4,
				SDL_PIXELORDER_ENUM.SDL_BITMAPORDER_4321,
				0,
				4, 0
			);
		public static readonly uint SDL_PIXELFORMAT_INDEX4MSB =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_INDEX4,
				SDL_PIXELORDER_ENUM.SDL_BITMAPORDER_1234,
				0,
				4, 0
			);
		public static readonly uint SDL_PIXELFORMAT_INDEX8 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_INDEX8,
				0,
				0,
				8, 1
			);
		public static readonly uint SDL_PIXELFORMAT_RGB332 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED8,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_XRGB,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_332,
				8, 1
			);
		public static readonly uint SDL_PIXELFORMAT_RGB444 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED16,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_XRGB,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_4444,
				12, 2
			);
		public static readonly uint SDL_PIXELFORMAT_RGB555 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED16,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_XRGB,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_1555,
				15, 2
			);
		public static readonly uint SDL_PIXELFORMAT_BGR555 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_INDEX1,
				SDL_PIXELORDER_ENUM.SDL_BITMAPORDER_4321,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_1555,
				15, 2
			);
		public static readonly uint SDL_PIXELFORMAT_ARGB4444 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED16,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_ARGB,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_4444,
				16, 2
			);
		public static readonly uint SDL_PIXELFORMAT_RGBA4444 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED16,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_RGBA,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_4444,
				16, 2
			);
		public static readonly uint SDL_PIXELFORMAT_ABGR4444 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED16,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_ABGR,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_4444,
				16, 2
			);
		public static readonly uint SDL_PIXELFORMAT_BGRA4444 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED16,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_BGRA,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_4444,
				16, 2
			);
		public static readonly uint SDL_PIXELFORMAT_ARGB1555 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED16,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_ARGB,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_1555,
				16, 2
			);
		public static readonly uint SDL_PIXELFORMAT_RGBA5551 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED16,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_RGBA,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_5551,
				16, 2
			);
		public static readonly uint SDL_PIXELFORMAT_ABGR1555 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED16,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_ABGR,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_1555,
				16, 2
			);
		public static readonly uint SDL_PIXELFORMAT_BGRA5551 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED16,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_BGRA,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_5551,
				16, 2
			);
		public static readonly uint SDL_PIXELFORMAT_RGB565 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED16,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_XRGB,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_565,
				16, 2
			);
		public static readonly uint SDL_PIXELFORMAT_BGR565 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED16,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_XBGR,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_565,
				16, 2
			);
		public static readonly uint SDL_PIXELFORMAT_RGB24 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_ARRAYU8,
				SDL_PIXELORDER_ENUM.SDL_ARRAYORDER_RGB,
				0,
				24, 3
			);
		public static readonly uint SDL_PIXELFORMAT_BGR24 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_ARRAYU8,
				SDL_PIXELORDER_ENUM.SDL_ARRAYORDER_BGR,
				0,
				24, 3
			);
		public static readonly uint SDL_PIXELFORMAT_RGB888 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED32,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_XRGB,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_8888,
				24, 4
			);
		public static readonly uint SDL_PIXELFORMAT_RGBX8888 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED32,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_RGBX,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_8888,
				24, 4
			);
		public static readonly uint SDL_PIXELFORMAT_BGR888 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED32,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_XBGR,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_8888,
				24, 4
			);
		public static readonly uint SDL_PIXELFORMAT_BGRX8888 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED32,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_BGRX,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_8888,
				24, 4
			);
		public static readonly uint SDL_PIXELFORMAT_ARGB8888 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED32,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_ARGB,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_8888,
				32, 4
			);
		public static readonly uint SDL_PIXELFORMAT_RGBA8888 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED32,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_RGBA,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_8888,
				32, 4
			);
		public static readonly uint SDL_PIXELFORMAT_ABGR8888 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED32,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_ABGR,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_8888,
				32, 4
			);
		public static readonly uint SDL_PIXELFORMAT_BGRA8888 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED32,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_BGRA,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_8888,
				32, 4
			);
		public static readonly uint SDL_PIXELFORMAT_ARGB2101010 =
			SDL_DEFINE_PIXELFORMAT(
				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED32,
				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_ARGB,
				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_2101010,
				32, 4
			);
		public static readonly uint SDL_PIXELFORMAT_YV12 =
			SDL_DEFINE_PIXELFOURCC(
				(byte) 'Y', (byte) 'V', (byte) '1', (byte) '2'
			);
		public static readonly uint SDL_PIXELFORMAT_IYUV =
			SDL_DEFINE_PIXELFOURCC(
				(byte) 'I', (byte) 'Y', (byte) 'U', (byte) 'V'
			);
		public static readonly uint SDL_PIXELFORMAT_YUY2 =
			SDL_DEFINE_PIXELFOURCC(
				(byte) 'Y', (byte) 'U', (byte) 'Y', (byte) '2'
			);
		public static readonly uint SDL_PIXELFORMAT_UYVY =
			SDL_DEFINE_PIXELFOURCC(
				(byte) 'U', (byte) 'Y', (byte) 'V', (byte) 'Y'
			);
		public static readonly uint SDL_PIXELFORMAT_YVYU =
			SDL_DEFINE_PIXELFOURCC(
				(byte) 'Y', (byte) 'V', (byte) 'Y', (byte) 'U'
			);

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_Color
		{
			public byte r;
			public byte g;
			public byte b;
			public byte a;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_Palette
		{
			public int ncolors;
			public IntPtr colors;
			public int version;
			public int refcount;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_PixelFormat
		{
			public uint format;
			public IntPtr palette; // SDL_Palette*
			public byte BitsPerPixel;
			public byte BytesPerPixel;
			public uint Rmask;
			public uint Gmask;
			public uint Bmask;
			public uint Amask;
			public byte Rloss;
			public byte Gloss;
			public byte Bloss;
			public byte Aloss;
			public byte Rshift;
			public byte Gshift;
			public byte Bshift;
			public byte Ashift;
			public int refcount;
			public IntPtr next; // SDL_PixelFormat*
		}

		/* IntPtr refers to an SDL_PixelFormat* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_AllocFormat(uint pixel_format);

		/* IntPtr refers to an SDL_Palette* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_AllocPalette(int ncolors);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_CalculateGammaRamp(
			float gamma,
			[Out()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeConst = 256)]
				ushort[] ramp
		);

		/* format refers to an SDL_PixelFormat* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_FreeFormat(IntPtr format);

		/* palette refers to an SDL_Palette* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_FreePalette(IntPtr palette);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GetPixelFormatName(
			uint format
		);

		/* format refers to an SDL_PixelFormat* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GetRGB(
			uint pixel,
			IntPtr format,
			out byte r,
			out byte g,
			out byte b
		);

		/* format refers to an SDL_PixelFormat* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GetRGBA(
			uint pixel,
			IntPtr format,
			out byte r,
			out byte g,
			out byte b,
			out byte a
		);

		/* format refers to an SDL_PixelFormat* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_MapRGB(
			IntPtr format,
			byte r,
			byte g,
			byte b
		);

		/* format refers to an SDL_PixelFormat* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_MapRGBA(
			IntPtr format,
			byte r,
			byte g,
			byte b,
			byte a
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_MasksToPixelFormatEnum(
			int bpp,
			uint Rmask,
			uint Gmask,
			uint Bmask,
			uint Amask
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_PixelFormatEnumToMasks(
			uint format,
			out int bpp,
			out uint Rmask,
			out uint Gmask,
			out uint Bmask,
			out uint Amask
		);

		/* palette refers to an SDL_Palette* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetPaletteColors(
			IntPtr palette,
			[In()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct)]
				SDL_Color[] colors,
			int firstcolor,
			int ncolors
		);

		/* format and palette refer to an SDL_PixelFormat* and SDL_Palette* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetPixelFormatPalette(
			IntPtr format,
			IntPtr palette
		);

		#endregion

		#region SDL_rect.h

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_Point
		{
			public int x;
			public int y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_Rect
		{
			public int x;
			public int y;
			public int w;
			public int h;
		}

		/* Only available in 2.0.4 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_PointInRect(ref SDL_Point p, ref SDL_Rect r);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_EnclosePoints(
			[In()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 1)]
				SDL_Point[] points,
			int count,
			ref SDL_Rect clip,
			out SDL_Rect result
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_HasIntersection(
			ref SDL_Rect A,
			ref SDL_Rect B
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_IntersectRect(
			ref SDL_Rect A,
			ref SDL_Rect B,
			out SDL_Rect result
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_IntersectRectAndLine(
			ref SDL_Rect rect,
			ref int X1,
			ref int Y1,
			ref int X2,
			ref int Y2
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_RectEmpty(ref SDL_Rect rect);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_RectEquals(
			ref SDL_Rect A,
			ref SDL_Rect B
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_UnionRect(
			ref SDL_Rect A,
			ref SDL_Rect B,
			out SDL_Rect result
		);

		#endregion

		#region SDL_surface.h

		public const uint SDL_SWSURFACE =	0x00000000;
		public const uint SDL_PREALLOC =	0x00000001;
		public const uint SDL_RLEACCEL =	0x00000002;
		public const uint SDL_DONTFREE =	0x00000004;

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_Surface
		{
			public uint flags;
			public IntPtr format; // SDL_PixelFormat*
			public int w;
			public int h;
			public int pitch;
			public IntPtr pixels; // void*
			public IntPtr userdata; // void*
			public int locked;
			public IntPtr lock_data; // void*
			public SDL_Rect clip_rect;
			public IntPtr map; // SDL_BlitMap*
			public int refcount;
		}

		/* surface refers to an SDL_Surface* */
		public static bool SDL_MUSTLOCK(IntPtr surface)
		{
			SDL_Surface sur;
			sur = (SDL_Surface) Marshal.PtrToStructure(
				surface,
				typeof(SDL_Surface)
			);
			return (sur.flags & SDL_RLEACCEL) != 0;
		}

		/* src and dst refer to an SDL_Surface* */
		[DllImport(nativeLibName, EntryPoint = "SDL_UpperBlit", CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_BlitSurface(
			IntPtr src,
			ref SDL_Rect srcrect,
			IntPtr dst,
			ref SDL_Rect dstrect
		);

		/* src and dst refer to an SDL_Surface*
		 * Internally, this function contains logic to use default values when
		 * source and destination rectangles are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for srcrect.
		 */
		[DllImport(nativeLibName, EntryPoint = "SDL_UpperBlit", CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_BlitSurface(
			IntPtr src,
			IntPtr srcrect,
			IntPtr dst,
			ref SDL_Rect dstrect
		);

		/* src and dst refer to an SDL_Surface*
		 * Internally, this function contains logic to use default values when
		 * source and destination rectangles are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for dstrect.
		 */
		[DllImport(nativeLibName, EntryPoint = "SDL_UpperBlit", CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_BlitSurface(
			IntPtr src,
			ref SDL_Rect srcrect,
			IntPtr dst,
			IntPtr dstrect
		);

		/* src and dst refer to an SDL_Surface*
		 * Internally, this function contains logic to use default values when
		 * source and destination rectangles are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for both SDL_Rects.
		 */
		[DllImport(nativeLibName, EntryPoint = "SDL_UpperBlit", CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_BlitSurface(
			IntPtr src,
			IntPtr srcrect,
			IntPtr dst,
			IntPtr dstrect
		);

		/* src and dst refer to an SDL_Surface* */
		[DllImport(nativeLibName, EntryPoint = "SDL_UpperBlitScaled", CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_BlitScaled(
			IntPtr src,
			ref SDL_Rect srcrect,
			IntPtr dst,
			ref SDL_Rect dstrect
		);

		/* src and dst refer to an SDL_Surface*
		 * Internally, this function contains logic to use default values when
		 * source and destination rectangles are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for srcrect.
		 */
		[DllImport(nativeLibName, EntryPoint = "SDL_UpperBlitScaled", CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_BlitScaled(
			IntPtr src,
			IntPtr srcrect,
			IntPtr dst,
			ref SDL_Rect dstrect
		);

		/* src and dst refer to an SDL_Surface*
		 * Internally, this function contains logic to use default values when
		 * source and destination rectangles are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for dstrect.
		 */
		[DllImport(nativeLibName, EntryPoint = "SDL_UpperBlitScaled", CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_BlitScaled(
			IntPtr src,
			ref SDL_Rect srcrect,
			IntPtr dst,
			IntPtr dstrect
		);

		/* src and dst refer to an SDL_Surface*
		 * Internally, this function contains logic to use default values when
		 * source and destination rectangles are passed as NULL.
		 * This overload allows for IntPtr.Zero (null) to be passed for both SDL_Rects.
		 */
		[DllImport(nativeLibName, EntryPoint = "SDL_UpperBlitScaled", CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_BlitScaled(
			IntPtr src,
			IntPtr srcrect,
			IntPtr dst,
			IntPtr dstrect
		);

		/* src and dst are void* pointers */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_ConvertPixels(
			int width,
			int height,
			uint src_format,
			IntPtr src,
			int src_pitch,
			uint dst_format,
			IntPtr dst,
			int dst_pitch
		);

		/* IntPtr refers to an SDL_Surface*
		 * src refers to an SDL_Surface*
		 * fmt refers to an SDL_PixelFormat*
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_ConvertSurface(
			IntPtr src,
			IntPtr fmt,
			uint flags
		);

		/* IntPtr refers to an SDL_Surface*, src to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_ConvertSurfaceFormat(
			IntPtr src,
			uint pixel_format,
			uint flags
		);

		/* IntPtr refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_CreateRGBSurface(
			uint flags,
			int width,
			int height,
			int depth,
			uint Rmask,
			uint Gmask,
			uint Bmask,
			uint Amask
		);

		/* IntPtr refers to an SDL_Surface*, pixels to a void* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_CreateRGBSurfaceFrom(
			IntPtr pixels,
			int width,
			int height,
			int depth,
			int pitch,
			uint Rmask,
			uint Gmask,
			uint Bmask,
			uint Amask
		);

		/* dst refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_FillRect(
			IntPtr dst,
			ref SDL_Rect rect,
			uint color
		);

		/* dst refers to an SDL_Surface*.
		 * This overload allows passing NULL to rect.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_FillRect(
			IntPtr dst,
			IntPtr rect,
			uint color
		);

		/* dst refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_FillRects(
			IntPtr dst,
			[In()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 2)]
				SDL_Rect[] rects,
			int count,
			uint color
		);

		/* surface refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_FreeSurface(IntPtr surface);

		/* surface refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GetClipRect(
			IntPtr surface,
			out SDL_Rect rect
		);

		/* surface refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetColorKey(
			IntPtr surface,
			out uint key
		);

		/* surface refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetSurfaceAlphaMod(
			IntPtr surface,
			out byte alpha
		);

		/* surface refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetSurfaceBlendMode(
			IntPtr surface,
			out SDL_BlendMode blendMode
		);

		/* surface refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetSurfaceColorMod(
			IntPtr surface,
			out byte r,
			out byte g,
			out byte b
		);

		/* These are for SDL_LoadBMP, which is a macro in the SDL headers. */
		/* IntPtr refers to an SDL_Surface* */
		/* THIS IS AN RWops FUNCTION! */
		[DllImport(nativeLibName, EntryPoint = "SDL_LoadBMP_RW", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr INTERNAL_SDL_LoadBMP_RW(
			IntPtr src,
			int freesrc
		);
		public static IntPtr SDL_LoadBMP(string file)
		{
			IntPtr rwops = INTERNAL_SDL_RWFromFile(file, "rb");
			return INTERNAL_SDL_LoadBMP_RW(rwops, 1);
		}

		/* surface refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_LockSurface(IntPtr surface);

		/* src and dst refer to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_LowerBlit(
			IntPtr src,
			ref SDL_Rect srcrect,
			IntPtr dst,
			ref SDL_Rect dstrect
		);

		/* src and dst refer to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_LowerBlitScaled(
			IntPtr src,
			ref SDL_Rect srcrect,
			IntPtr dst,
			ref SDL_Rect dstrect
		);

		/* These are for SDL_SaveBMP, which is a macro in the SDL headers. */
		/* IntPtr refers to an SDL_Surface* */
		/* THIS IS AN RWops FUNCTION! */
		[DllImport(nativeLibName, EntryPoint = "SDL_SaveBMP_RW", CallingConvention = CallingConvention.Cdecl)]
		private static extern int INTERNAL_SDL_SaveBMP_RW(
			IntPtr surface,
			IntPtr src,
			int freesrc
		);
		public static int SDL_SaveBMP(IntPtr surface, string file)
		{
			IntPtr rwops = INTERNAL_SDL_RWFromFile(file, "wb");
			return INTERNAL_SDL_SaveBMP_RW(surface, rwops, 1);
		}

		/* surface refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_SetClipRect(
			IntPtr surface,
			ref SDL_Rect rect
		);

		/* surface refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetColorKey(
			IntPtr surface,
			int flag,
			uint key
		);

		/* surface refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetSurfaceAlphaMod(
			IntPtr surface,
			byte alpha
		);

		/* surface refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetSurfaceBlendMode(
			IntPtr surface,
			SDL_BlendMode blendMode
		);

		/* surface refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetSurfaceColorMod(
			IntPtr surface,
			byte r,
			byte g,
			byte b
		);

		/* surface refers to an SDL_Surface*, palette to an SDL_Palette* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetSurfacePalette(
			IntPtr surface,
			IntPtr palette
		);

		/* surface refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetSurfaceRLE(
			IntPtr surface,
			int flag
		);

		/* src and dst refer to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SoftStretch(
			IntPtr src,
			ref SDL_Rect srcrect,
			IntPtr dst,
			ref SDL_Rect dstrect
		);

		/* surface refers to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_UnlockSurface(IntPtr surface);

		/* src and dst refer to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_UpperBlit(
			IntPtr src,
			ref SDL_Rect srcrect,
			IntPtr dst,
			ref SDL_Rect dstrect
		);

		/* src and dst refer to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_UpperBlitScaled(
			IntPtr src,
			ref SDL_Rect srcrect,
			IntPtr dst,
			ref SDL_Rect dstrect
		);

		#endregion

		#region SDL_clipboard.h

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_HasClipboardText();

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GetClipboardText();

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetClipboardText(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string text
		);

		#endregion

		#region SDL_events.h

		/* General keyboard/mouse state definitions. */
		public const byte SDL_PRESSED =		1;
		public const byte SDL_RELEASED =	0;

		/* Default size is according to SDL2 default. */
		public const int SDL_TEXTEDITINGEVENT_TEXT_SIZE = 32;
		public const int SDL_TEXTINPUTEVENT_TEXT_SIZE = 32;

		/* The types of events that can be delivered. */
		public enum SDL_EventType : uint
		{
			SDL_FIRSTEVENT =		0,

			/* Application events */
			SDL_QUIT = 			0x100,

			/* Window events */
			SDL_WINDOWEVENT = 		0x200,
			SDL_SYSWMEVENT,

			/* Keyboard events */
			SDL_KEYDOWN = 			0x300,
			SDL_KEYUP,
			SDL_TEXTEDITING,
			SDL_TEXTINPUT,

			/* Mouse events */
			SDL_MOUSEMOTION = 		0x400,
			SDL_MOUSEBUTTONDOWN,
			SDL_MOUSEBUTTONUP,
			SDL_MOUSEWHEEL,

			/* Joystick events */
			SDL_JOYAXISMOTION =		0x600,
			SDL_JOYBALLMOTION,
			SDL_JOYHATMOTION,
			SDL_JOYBUTTONDOWN,
			SDL_JOYBUTTONUP,
			SDL_JOYDEVICEADDED,
			SDL_JOYDEVICEREMOVED,

			/* Game controller events */
			SDL_CONTROLLERAXISMOTION = 	0x650,
			SDL_CONTROLLERBUTTONDOWN,
			SDL_CONTROLLERBUTTONUP,
			SDL_CONTROLLERDEVICEADDED,
			SDL_CONTROLLERDEVICEREMOVED,
			SDL_CONTROLLERDEVICEREMAPPED,

			/* Touch events */
			SDL_FINGERDOWN = 		0x700,
			SDL_FINGERUP,
			SDL_FINGERMOTION,

			/* Gesture events */
			SDL_DOLLARGESTURE =		0x800,
			SDL_DOLLARRECORD,
			SDL_MULTIGESTURE,

			/* Clipboard events */
			SDL_CLIPBOARDUPDATE =		0x900,

			/* Drag and drop events */
			SDL_DROPFILE =			0x1000,

			/* Audio hotplug events */
			/* Only available in SDL 2.0.4 or higher */
			SDL_AUDIODEVICEADDED =		0x1100,
			SDL_AUDIODEVICEREMOVED,

			/* Render events */
			/* Only available in SDL 2.0.2 or higher */
			SDL_RENDER_TARGETS_RESET =	0x2000,
			/* Only available in SDL 2.0.4 or higher */
			SDL_RENDER_DEVICE_RESET,

			/* Events SDL_USEREVENT through SDL_LASTEVENT are for
			 * your use, and should be allocated with
			 * SDL_RegisterEvents()
			 */
			SDL_USEREVENT =			0x8000,

			/* The last event, used for bouding arrays. */
			SDL_LASTEVENT =			0xFFFF
		}

		/* Only available in 2.0.4 or higher */
		public enum SDL_MouseWheelDirection : uint
		{
			SDL_MOUSEHWEEL_NORMAL,
			SDL_MOUSEWHEEL_FLIPPED
		}

		/* Fields shared by every event */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_GenericEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
		}

// Ignore private members used for padding in this struct
#pragma warning disable 0169
		/* Window state change event data (event.window.*) */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_WindowEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public UInt32 windowID;
			public SDL_WindowEventID windowEvent; // event, lolC#
			private byte padding1;
			private byte padding2;
			private byte padding3;
			public Int32 data1;
			public Int32 data2;
		}
#pragma warning restore 0169

// Ignore private members used for padding in this struct
#pragma warning disable 0169
		/* Keyboard button event structure (event.key.*) */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_KeyboardEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public UInt32 windowID;
			public byte state;
			public byte repeat; /* non-zero if this is a repeat */
			private byte padding2;
			private byte padding3;
			public SDL_Keysym keysym;
		}
#pragma warning restore 0169

		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct SDL_TextEditingEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public UInt32 windowID;
			public fixed byte text[SDL_TEXTEDITINGEVENT_TEXT_SIZE];
			public Int32 start;
			public Int32 length;
		}

		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct SDL_TextInputEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public UInt32 windowID;
			public fixed byte text[SDL_TEXTINPUTEVENT_TEXT_SIZE];
		}

// Ignore private members used for padding in this struct
#pragma warning disable 0169
		/* Mouse motion event structure (event.motion.*) */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MouseMotionEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public UInt32 windowID;
			public UInt32 which;
			public byte state; /* bitmask of buttons */
			private byte padding1;
			private byte padding2;
			private byte padding3;
			public Int32 x;
			public Int32 y;
			public Int32 xrel;
			public Int32 yrel;
		}
#pragma warning restore 0169

// Ignore private members used for padding in this struct
#pragma warning disable 0169
		/* Mouse button event structure (event.button.*) */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MouseButtonEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public UInt32 windowID;
			public UInt32 which;
			public byte button; /* button id */
			public byte state; /* SDL_PRESSED or SDL_RELEASED */
			public byte clicks; /* 1 for single-click, 2 for double-click, etc. */
			private byte padding1;
			public Int32 x;
			public Int32 y;
		}
#pragma warning restore 0169

		/* Mouse wheel event structure (event.wheel.*) */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MouseWheelEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public UInt32 windowID;
			public UInt32 which;
			public Int32 x; /* amount scrolled horizontally */
			public Int32 y; /* amount scrolled vertically */
			public UInt32 direction; /* Set to one of the SDL_MOUSEWHEEL_* defines */
		}

// Ignore private members used for padding in this struct
#pragma warning disable 0169
		/* Joystick axis motion event structure (event.jaxis.*) */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_JoyAxisEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public Int32 which; /* SDL_JoystickID */
			public byte axis;
			private byte padding1;
			private byte padding2;
			private byte padding3;
			public Int16 axisValue; /* value, lolC# */
			public UInt16 padding4;
		}
#pragma warning restore 0169

// Ignore private members used for padding in this struct
#pragma warning disable 0169
		/* Joystick trackball motion event structure (event.jball.*) */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_JoyBallEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public Int32 which; /* SDL_JoystickID */
			public byte ball;
			private byte padding1;
			private byte padding2;
			private byte padding3;
			public Int16 xrel;
			public Int16 yrel;
		}
#pragma warning restore 0169

// Ignore private members used for padding in this struct
#pragma warning disable 0169
		/* Joystick hat position change event struct (event.jhat.*) */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_JoyHatEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public Int32 which; /* SDL_JoystickID */
			public byte hat; /* index of the hat */
			public byte hatValue; /* value, lolC# */
			private byte padding1;
			private byte padding2;
		}
#pragma warning restore 0169

// Ignore private members used for padding in this struct
#pragma warning disable 0169
		/* Joystick button event structure (event.jbutton.*) */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_JoyButtonEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public Int32 which; /* SDL_JoystickID */
			public byte button;
			public byte state; /* SDL_PRESSED or SDL_RELEASED */
			private byte padding1;
			private byte padding2;
		}
#pragma warning restore 0169

		/* Joystick device event structure (event.jdevice.*) */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_JoyDeviceEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public Int32 which; /* SDL_JoystickID */
		}

// Ignore private members used for padding in this struct
#pragma warning disable 0169
		/* Game controller axis motion event (event.caxis.*) */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_ControllerAxisEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public Int32 which; /* SDL_JoystickID */
			public byte axis;
			private byte padding1;
			private byte padding2;
			private byte padding3;
			public Int16 axisValue; /* value, lolC# */
			private UInt16 padding4;
		}
#pragma warning restore 0169

// Ignore private members used for padding in this struct
#pragma warning disable 0169
		/* Game controller button event (event.cbutton.*) */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_ControllerButtonEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public Int32 which; /* SDL_JoystickID */
			public byte button;
			public byte state;
			private byte padding1;
			private byte padding2;
		}
#pragma warning restore 0169

		/* Game controller device event (event.cdevice.*) */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_ControllerDeviceEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public Int32 which;	/* joystick id for ADDED,
						 * else instance id
						 */
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_TouchFingerEvent
		{
			public UInt32 type;
			public UInt32 timestamp;
			public Int64 touchId; // SDL_TouchID
			public Int64 fingerId; // SDL_GestureID
			public float x;
			public float y;
			public float dx;
			public float dy;
			public float pressure;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MultiGestureEvent
		{
			public UInt32 type;
			public UInt32 timestamp;
			public Int64 touchId; // SDL_TouchID
			public float dTheta;
			public float dDist;
			public float x;
			public float y;
			public UInt16 numFingers;
			public UInt16 padding;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_DollarGestureEvent
		{
			public UInt32 type;
			public UInt32 timestamp;
			public Int64 touchId; // SDL_TouchID
			public Int64 gestureId; // SDL_GestureID
			public UInt32 numFingers;
			public float error;
			public float x;
			public float y;
		}

		/* File open request by system (event.drop.*), disabled by
		 * default
		 */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_DropEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public IntPtr file; /* char* filename, to be freed */
		}

		/* The "quit requested" event */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_QuitEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
		}

		/* A user defined event (event.user.*) */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_UserEvent
		{
			public UInt32 type;
			public UInt32 timestamp;
			public UInt32 windowID;
			public Int32 code;
			public IntPtr data1; /* user-defined */
			public IntPtr data2; /* user-defined */
		}

		/* A video driver dependent event (event.syswm.*), disabled */
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_SysWMEvent
		{
			public SDL_EventType type;
			public UInt32 timestamp;
			public IntPtr msg; /* SDL_SysWMmsg*, system-dependent*/
		}

		/* General event structure */
		// C# doesn't do unions, so we do this ugly thing. */
		[StructLayout(LayoutKind.Explicit)]
		public struct SDL_Event
		{
			[FieldOffset(0)]
			public SDL_EventType type;
			[FieldOffset(0)]
			public SDL_WindowEvent window;
			[FieldOffset(0)]
			public SDL_KeyboardEvent key;
			[FieldOffset(0)]
			public SDL_TextEditingEvent edit;
			[FieldOffset(0)]
			public SDL_TextInputEvent text;
			[FieldOffset(0)]
			public SDL_MouseMotionEvent motion;
			[FieldOffset(0)]
			public SDL_MouseButtonEvent button;
			[FieldOffset(0)]
			public SDL_MouseWheelEvent wheel;
			[FieldOffset(0)]
			public SDL_JoyAxisEvent jaxis;
			[FieldOffset(0)]
			public SDL_JoyBallEvent jball;
			[FieldOffset(0)]
			public SDL_JoyHatEvent jhat;
			[FieldOffset(0)]
			public SDL_JoyButtonEvent jbutton;
			[FieldOffset(0)]
			public SDL_JoyDeviceEvent jdevice;
			[FieldOffset(0)]
			public SDL_ControllerAxisEvent caxis;
			[FieldOffset(0)]
			public SDL_ControllerButtonEvent cbutton;
			[FieldOffset(0)]
			public SDL_ControllerDeviceEvent cdevice;
			[FieldOffset(0)]
			public SDL_QuitEvent quit;
			[FieldOffset(0)]
			public SDL_UserEvent user;
			[FieldOffset(0)]
			public SDL_SysWMEvent syswm;
			[FieldOffset(0)]
			public SDL_TouchFingerEvent tfinger;
			[FieldOffset(0)]
			public SDL_MultiGestureEvent mgesture;
			[FieldOffset(0)]
			public SDL_DollarGestureEvent dgesture;
			[FieldOffset(0)]
			public SDL_DropEvent drop;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int SDL_EventFilter(
			IntPtr userdata, // void*
			IntPtr sdlevent // SDL_Event* event, lolC#
		);

		/* Pump the event loop, getting events from the input devices*/
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_PumpEvents();

		public enum SDL_eventaction
		{
			SDL_ADDEVENT,
			SDL_PEEKEVENT,
			SDL_GETEVENT
		}

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_PeepEvents(
			[Out()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 1)]
				SDL_Event[] events,
			int numevents,
			SDL_eventaction action,
			SDL_EventType minType,
			SDL_EventType maxType
		);

		/* Checks to see if certain events are in the event queue */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_HasEvent(SDL_EventType type);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_HasEvents(
			SDL_EventType minType,
			SDL_EventType maxType
		);

		/* Clears events from the event queue */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_FlushEvent(SDL_EventType type);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_FlushEvents(
			SDL_EventType min,
			SDL_EventType max
		);

		/* Polls for currently pending events */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_PollEvent(out SDL_Event _event);

		/* Waits indefinitely for the next event */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_WaitEvent(out SDL_Event _event);

		/* Waits until the specified timeout (in ms) for the next event
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_WaitEventTimeout(out SDL_Event _event, int timeout);

		/* Add an event to the event queue */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_PushEvent(ref SDL_Event _event);

		/* userdata refers to a void* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetEventFilter(
			SDL_EventFilter filter,
			IntPtr userdata
		);

		/* userdata refers to a void* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_GetEventFilter(
			out SDL_EventFilter filter,
			out IntPtr userdata
		);

		/* userdata refers to a void* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_AddEventWatch(
			SDL_EventFilter filter,
			IntPtr userdata
		);

		/* userdata refers to a void* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_DelEventWatch(
			SDL_EventFilter filter,
			IntPtr userdata
		);

		/* userdata refers to a void* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_FilterEvents(
			SDL_EventFilter filter,
			IntPtr userdata
		);

		/* These are for SDL_EventState() */
		public const int SDL_QUERY = 		-1;
		public const int SDL_IGNORE = 		0;
		public const int SDL_DISABLE =		0;
		public const int SDL_ENABLE = 		1;

		/* This function allows you to enable/disable certain events */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte SDL_EventState(SDL_EventType type, int state);

		/* Get the state of an event */
		public static byte SDL_GetEventState(SDL_EventType type)
		{
			return SDL_EventState(type, SDL_QUERY);
		}

		/* Allocate a set of user-defined events */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern UInt32 SDL_RegisterEvents(int numevents);
		#endregion

		#region SDL_scancode.h

		/* Scancodes based off USB keyboard page (0x07) */
		public enum SDL_Scancode
		{
			SDL_SCANCODE_UNKNOWN = 0,

			SDL_SCANCODE_A = 4,
			SDL_SCANCODE_B = 5,
			SDL_SCANCODE_C = 6,
			SDL_SCANCODE_D = 7,
			SDL_SCANCODE_E = 8,
			SDL_SCANCODE_F = 9,
			SDL_SCANCODE_G = 10,
			SDL_SCANCODE_H = 11,
			SDL_SCANCODE_I = 12,
			SDL_SCANCODE_J = 13,
			SDL_SCANCODE_K = 14,
			SDL_SCANCODE_L = 15,
			SDL_SCANCODE_M = 16,
			SDL_SCANCODE_N = 17,
			SDL_SCANCODE_O = 18,
			SDL_SCANCODE_P = 19,
			SDL_SCANCODE_Q = 20,
			SDL_SCANCODE_R = 21,
			SDL_SCANCODE_S = 22,
			SDL_SCANCODE_T = 23,
			SDL_SCANCODE_U = 24,
			SDL_SCANCODE_V = 25,
			SDL_SCANCODE_W = 26,
			SDL_SCANCODE_X = 27,
			SDL_SCANCODE_Y = 28,
			SDL_SCANCODE_Z = 29,

			SDL_SCANCODE_1 = 30,
			SDL_SCANCODE_2 = 31,
			SDL_SCANCODE_3 = 32,
			SDL_SCANCODE_4 = 33,
			SDL_SCANCODE_5 = 34,
			SDL_SCANCODE_6 = 35,
			SDL_SCANCODE_7 = 36,
			SDL_SCANCODE_8 = 37,
			SDL_SCANCODE_9 = 38,
			SDL_SCANCODE_0 = 39,

			SDL_SCANCODE_RETURN = 40,
			SDL_SCANCODE_ESCAPE = 41,
			SDL_SCANCODE_BACKSPACE = 42,
			SDL_SCANCODE_TAB = 43,
			SDL_SCANCODE_SPACE = 44,

			SDL_SCANCODE_MINUS = 45,
			SDL_SCANCODE_EQUALS = 46,
			SDL_SCANCODE_LEFTBRACKET = 47,
			SDL_SCANCODE_RIGHTBRACKET = 48,
			SDL_SCANCODE_BACKSLASH = 49,
			SDL_SCANCODE_NONUSHASH = 50,
			SDL_SCANCODE_SEMICOLON = 51,
			SDL_SCANCODE_APOSTROPHE = 52,
			SDL_SCANCODE_GRAVE = 53,
			SDL_SCANCODE_COMMA = 54,
			SDL_SCANCODE_PERIOD = 55,
			SDL_SCANCODE_SLASH = 56,

			SDL_SCANCODE_CAPSLOCK = 57,

			SDL_SCANCODE_F1 = 58,
			SDL_SCANCODE_F2 = 59,
			SDL_SCANCODE_F3 = 60,
			SDL_SCANCODE_F4 = 61,
			SDL_SCANCODE_F5 = 62,
			SDL_SCANCODE_F6 = 63,
			SDL_SCANCODE_F7 = 64,
			SDL_SCANCODE_F8 = 65,
			SDL_SCANCODE_F9 = 66,
			SDL_SCANCODE_F10 = 67,
			SDL_SCANCODE_F11 = 68,
			SDL_SCANCODE_F12 = 69,

			SDL_SCANCODE_PRINTSCREEN = 70,
			SDL_SCANCODE_SCROLLLOCK = 71,
			SDL_SCANCODE_PAUSE = 72,
			SDL_SCANCODE_INSERT = 73,
			SDL_SCANCODE_HOME = 74,
			SDL_SCANCODE_PAGEUP = 75,
			SDL_SCANCODE_DELETE = 76,
			SDL_SCANCODE_END = 77,
			SDL_SCANCODE_PAGEDOWN = 78,
			SDL_SCANCODE_RIGHT = 79,
			SDL_SCANCODE_LEFT = 80,
			SDL_SCANCODE_DOWN = 81,
			SDL_SCANCODE_UP = 82,

			SDL_SCANCODE_NUMLOCKCLEAR = 83,
			SDL_SCANCODE_KP_DIVIDE = 84,
			SDL_SCANCODE_KP_MULTIPLY = 85,
			SDL_SCANCODE_KP_MINUS = 86,
			SDL_SCANCODE_KP_PLUS = 87,
			SDL_SCANCODE_KP_ENTER = 88,
			SDL_SCANCODE_KP_1 = 89,
			SDL_SCANCODE_KP_2 = 90,
			SDL_SCANCODE_KP_3 = 91,
			SDL_SCANCODE_KP_4 = 92,
			SDL_SCANCODE_KP_5 = 93,
			SDL_SCANCODE_KP_6 = 94,
			SDL_SCANCODE_KP_7 = 95,
			SDL_SCANCODE_KP_8 = 96,
			SDL_SCANCODE_KP_9 = 97,
			SDL_SCANCODE_KP_0 = 98,
			SDL_SCANCODE_KP_PERIOD = 99,

			SDL_SCANCODE_NONUSBACKSLASH = 100,
			SDL_SCANCODE_APPLICATION = 101,
			SDL_SCANCODE_POWER = 102,
			SDL_SCANCODE_KP_EQUALS = 103,
			SDL_SCANCODE_F13 = 104,
			SDL_SCANCODE_F14 = 105,
			SDL_SCANCODE_F15 = 106,
			SDL_SCANCODE_F16 = 107,
			SDL_SCANCODE_F17 = 108,
			SDL_SCANCODE_F18 = 109,
			SDL_SCANCODE_F19 = 110,
			SDL_SCANCODE_F20 = 111,
			SDL_SCANCODE_F21 = 112,
			SDL_SCANCODE_F22 = 113,
			SDL_SCANCODE_F23 = 114,
			SDL_SCANCODE_F24 = 115,
			SDL_SCANCODE_EXECUTE = 116,
			SDL_SCANCODE_HELP = 117,
			SDL_SCANCODE_MENU = 118,
			SDL_SCANCODE_SELECT = 119,
			SDL_SCANCODE_STOP = 120,
			SDL_SCANCODE_AGAIN = 121,
			SDL_SCANCODE_UNDO = 122,
			SDL_SCANCODE_CUT = 123,
			SDL_SCANCODE_COPY = 124,
			SDL_SCANCODE_PASTE = 125,
			SDL_SCANCODE_FIND = 126,
			SDL_SCANCODE_MUTE = 127,
			SDL_SCANCODE_VOLUMEUP = 128,
			SDL_SCANCODE_VOLUMEDOWN = 129,
			/* not sure whether there's a reason to enable these */
			/*	SDL_SCANCODE_LOCKINGCAPSLOCK = 130, */
			/*	SDL_SCANCODE_LOCKINGNUMLOCK = 131, */
			/*	SDL_SCANCODE_LOCKINGSCROLLLOCK = 132, */
			SDL_SCANCODE_KP_COMMA = 133,
			SDL_SCANCODE_KP_EQUALSAS400 = 134,

			SDL_SCANCODE_INTERNATIONAL1 = 135,
			SDL_SCANCODE_INTERNATIONAL2 = 136,
			SDL_SCANCODE_INTERNATIONAL3 = 137,
			SDL_SCANCODE_INTERNATIONAL4 = 138,
			SDL_SCANCODE_INTERNATIONAL5 = 139,
			SDL_SCANCODE_INTERNATIONAL6 = 140,
			SDL_SCANCODE_INTERNATIONAL7 = 141,
			SDL_SCANCODE_INTERNATIONAL8 = 142,
			SDL_SCANCODE_INTERNATIONAL9 = 143,
			SDL_SCANCODE_LANG1 = 144,
			SDL_SCANCODE_LANG2 = 145,
			SDL_SCANCODE_LANG3 = 146,
			SDL_SCANCODE_LANG4 = 147,
			SDL_SCANCODE_LANG5 = 148,
			SDL_SCANCODE_LANG6 = 149,
			SDL_SCANCODE_LANG7 = 150,
			SDL_SCANCODE_LANG8 = 151,
			SDL_SCANCODE_LANG9 = 152,

			SDL_SCANCODE_ALTERASE = 153,
			SDL_SCANCODE_SYSREQ = 154,
			SDL_SCANCODE_CANCEL = 155,
			SDL_SCANCODE_CLEAR = 156,
			SDL_SCANCODE_PRIOR = 157,
			SDL_SCANCODE_RETURN2 = 158,
			SDL_SCANCODE_SEPARATOR = 159,
			SDL_SCANCODE_OUT = 160,
			SDL_SCANCODE_OPER = 161,
			SDL_SCANCODE_CLEARAGAIN = 162,
			SDL_SCANCODE_CRSEL = 163,
			SDL_SCANCODE_EXSEL = 164,

			SDL_SCANCODE_KP_00 = 176,
			SDL_SCANCODE_KP_000 = 177,
			SDL_SCANCODE_THOUSANDSSEPARATOR = 178,
			SDL_SCANCODE_DECIMALSEPARATOR = 179,
			SDL_SCANCODE_CURRENCYUNIT = 180,
			SDL_SCANCODE_CURRENCYSUBUNIT = 181,
			SDL_SCANCODE_KP_LEFTPAREN = 182,
			SDL_SCANCODE_KP_RIGHTPAREN = 183,
			SDL_SCANCODE_KP_LEFTBRACE = 184,
			SDL_SCANCODE_KP_RIGHTBRACE = 185,
			SDL_SCANCODE_KP_TAB = 186,
			SDL_SCANCODE_KP_BACKSPACE = 187,
			SDL_SCANCODE_KP_A = 188,
			SDL_SCANCODE_KP_B = 189,
			SDL_SCANCODE_KP_C = 190,
			SDL_SCANCODE_KP_D = 191,
			SDL_SCANCODE_KP_E = 192,
			SDL_SCANCODE_KP_F = 193,
			SDL_SCANCODE_KP_XOR = 194,
			SDL_SCANCODE_KP_POWER = 195,
			SDL_SCANCODE_KP_PERCENT = 196,
			SDL_SCANCODE_KP_LESS = 197,
			SDL_SCANCODE_KP_GREATER = 198,
			SDL_SCANCODE_KP_AMPERSAND = 199,
			SDL_SCANCODE_KP_DBLAMPERSAND = 200,
			SDL_SCANCODE_KP_VERTICALBAR = 201,
			SDL_SCANCODE_KP_DBLVERTICALBAR = 202,
			SDL_SCANCODE_KP_COLON = 203,
			SDL_SCANCODE_KP_HASH = 204,
			SDL_SCANCODE_KP_SPACE = 205,
			SDL_SCANCODE_KP_AT = 206,
			SDL_SCANCODE_KP_EXCLAM = 207,
			SDL_SCANCODE_KP_MEMSTORE = 208,
			SDL_SCANCODE_KP_MEMRECALL = 209,
			SDL_SCANCODE_KP_MEMCLEAR = 210,
			SDL_SCANCODE_KP_MEMADD = 211,
			SDL_SCANCODE_KP_MEMSUBTRACT = 212,
			SDL_SCANCODE_KP_MEMMULTIPLY = 213,
			SDL_SCANCODE_KP_MEMDIVIDE = 214,
			SDL_SCANCODE_KP_PLUSMINUS = 215,
			SDL_SCANCODE_KP_CLEAR = 216,
			SDL_SCANCODE_KP_CLEARENTRY = 217,
			SDL_SCANCODE_KP_BINARY = 218,
			SDL_SCANCODE_KP_OCTAL = 219,
			SDL_SCANCODE_KP_DECIMAL = 220,
			SDL_SCANCODE_KP_HEXADECIMAL = 221,

			SDL_SCANCODE_LCTRL = 224,
			SDL_SCANCODE_LSHIFT = 225,
			SDL_SCANCODE_LALT = 226,
			SDL_SCANCODE_LGUI = 227,
			SDL_SCANCODE_RCTRL = 228,
			SDL_SCANCODE_RSHIFT = 229,
			SDL_SCANCODE_RALT = 230,
			SDL_SCANCODE_RGUI = 231,

			SDL_SCANCODE_MODE = 257,

			/* These come from the USB consumer page (0x0C) */
			SDL_SCANCODE_AUDIONEXT = 258,
			SDL_SCANCODE_AUDIOPREV = 259,
			SDL_SCANCODE_AUDIOSTOP = 260,
			SDL_SCANCODE_AUDIOPLAY = 261,
			SDL_SCANCODE_AUDIOMUTE = 262,
			SDL_SCANCODE_MEDIASELECT = 263,
			SDL_SCANCODE_WWW = 264,
			SDL_SCANCODE_MAIL = 265,
			SDL_SCANCODE_CALCULATOR = 266,
			SDL_SCANCODE_COMPUTER = 267,
			SDL_SCANCODE_AC_SEARCH = 268,
			SDL_SCANCODE_AC_HOME = 269,
			SDL_SCANCODE_AC_BACK = 270,
			SDL_SCANCODE_AC_FORWARD = 271,
			SDL_SCANCODE_AC_STOP = 272,
			SDL_SCANCODE_AC_REFRESH = 273,
			SDL_SCANCODE_AC_BOOKMARKS = 274,

			/* These come from other sources, and are mostly mac related */
			SDL_SCANCODE_BRIGHTNESSDOWN = 275,
			SDL_SCANCODE_BRIGHTNESSUP = 276,
			SDL_SCANCODE_DISPLAYSWITCH = 277,
			SDL_SCANCODE_KBDILLUMTOGGLE = 278,
			SDL_SCANCODE_KBDILLUMDOWN = 279,
			SDL_SCANCODE_KBDILLUMUP = 280,
			SDL_SCANCODE_EJECT = 281,
			SDL_SCANCODE_SLEEP = 282,

			SDL_SCANCODE_APP1 = 283,
			SDL_SCANCODE_APP2 = 284,

			/* This is not a key, simply marks the number of scancodes
			 * so that you know how big to make your arrays. */
			SDL_NUM_SCANCODES = 512
		}

		#endregion

		#region SDL_keycode.h

		public const int SDLK_SCANCODE_MASK = (1 << 30);
		public static SDL_Keycode SDL_SCANCODE_TO_KEYCODE(SDL_Scancode X)
		{
			return (SDL_Keycode)((int)X | SDLK_SCANCODE_MASK);
		}

		/* So, in the C headers, SDL_Keycode is a typedef of Sint32
		 * and all of the names are in an anonymous enum. Yeah...
		 * that's not going to cut it for C#. We'll just put them in an
		 * enum for now? */
		public enum SDL_Keycode
		{
			SDLK_UNKNOWN = 0,

			SDLK_RETURN = '\r',
			SDLK_ESCAPE = 27, // '\033'
			SDLK_BACKSPACE = '\b',
			SDLK_TAB = '\t',
			SDLK_SPACE = ' ',
			SDLK_EXCLAIM = '!',
			SDLK_QUOTEDBL = '"',
			SDLK_HASH = '#',
			SDLK_PERCENT = '%',
			SDLK_DOLLAR = '$',
			SDLK_AMPERSAND = '&',
			SDLK_QUOTE = '\'',
			SDLK_LEFTPAREN = '(',
			SDLK_RIGHTPAREN = ')',
			SDLK_ASTERISK = '*',
			SDLK_PLUS = '+',
			SDLK_COMMA = ',',
			SDLK_MINUS = '-',
			SDLK_PERIOD = '.',
			SDLK_SLASH = '/',
			SDLK_0 = '0',
			SDLK_1 = '1',
			SDLK_2 = '2',
			SDLK_3 = '3',
			SDLK_4 = '4',
			SDLK_5 = '5',
			SDLK_6 = '6',
			SDLK_7 = '7',
			SDLK_8 = '8',
			SDLK_9 = '9',
			SDLK_COLON = ':',
			SDLK_SEMICOLON = ';',
			SDLK_LESS = '<',
			SDLK_EQUALS = '=',
			SDLK_GREATER = '>',
			SDLK_QUESTION = '?',
			SDLK_AT = '@',
			/*
			Skip uppercase letters
			*/
			SDLK_LEFTBRACKET = '[',
			SDLK_BACKSLASH = '\\',
			SDLK_RIGHTBRACKET = ']',
			SDLK_CARET = '^',
			SDLK_UNDERSCORE = '_',
			SDLK_BACKQUOTE = '`',
			SDLK_a = 'a',
			SDLK_b = 'b',
			SDLK_c = 'c',
			SDLK_d = 'd',
			SDLK_e = 'e',
			SDLK_f = 'f',
			SDLK_g = 'g',
			SDLK_h = 'h',
			SDLK_i = 'i',
			SDLK_j = 'j',
			SDLK_k = 'k',
			SDLK_l = 'l',
			SDLK_m = 'm',
			SDLK_n = 'n',
			SDLK_o = 'o',
			SDLK_p = 'p',
			SDLK_q = 'q',
			SDLK_r = 'r',
			SDLK_s = 's',
			SDLK_t = 't',
			SDLK_u = 'u',
			SDLK_v = 'v',
			SDLK_w = 'w',
			SDLK_x = 'x',
			SDLK_y = 'y',
			SDLK_z = 'z',

			SDLK_CAPSLOCK = (int)SDL_Scancode.SDL_SCANCODE_CAPSLOCK | SDLK_SCANCODE_MASK,

			SDLK_F1 = (int)SDL_Scancode.SDL_SCANCODE_F1 | SDLK_SCANCODE_MASK,
			SDLK_F2 = (int)SDL_Scancode.SDL_SCANCODE_F2 | SDLK_SCANCODE_MASK,
			SDLK_F3 = (int)SDL_Scancode.SDL_SCANCODE_F3 | SDLK_SCANCODE_MASK,
			SDLK_F4 = (int)SDL_Scancode.SDL_SCANCODE_F4 | SDLK_SCANCODE_MASK,
			SDLK_F5 = (int)SDL_Scancode.SDL_SCANCODE_F5 | SDLK_SCANCODE_MASK,
			SDLK_F6 = (int)SDL_Scancode.SDL_SCANCODE_F6 | SDLK_SCANCODE_MASK,
			SDLK_F7 = (int)SDL_Scancode.SDL_SCANCODE_F7 | SDLK_SCANCODE_MASK,
			SDLK_F8 = (int)SDL_Scancode.SDL_SCANCODE_F8 | SDLK_SCANCODE_MASK,
			SDLK_F9 = (int)SDL_Scancode.SDL_SCANCODE_F9 | SDLK_SCANCODE_MASK,
			SDLK_F10 = (int)SDL_Scancode.SDL_SCANCODE_F10 | SDLK_SCANCODE_MASK,
			SDLK_F11 = (int)SDL_Scancode.SDL_SCANCODE_F11 | SDLK_SCANCODE_MASK,
			SDLK_F12 = (int)SDL_Scancode.SDL_SCANCODE_F12 | SDLK_SCANCODE_MASK,

			SDLK_PRINTSCREEN = (int)SDL_Scancode.SDL_SCANCODE_PRINTSCREEN | SDLK_SCANCODE_MASK,
			SDLK_SCROLLLOCK = (int)SDL_Scancode.SDL_SCANCODE_SCROLLLOCK | SDLK_SCANCODE_MASK,
			SDLK_PAUSE = (int)SDL_Scancode.SDL_SCANCODE_PAUSE | SDLK_SCANCODE_MASK,
			SDLK_INSERT = (int)SDL_Scancode.SDL_SCANCODE_INSERT | SDLK_SCANCODE_MASK,
			SDLK_HOME = (int)SDL_Scancode.SDL_SCANCODE_HOME | SDLK_SCANCODE_MASK,
			SDLK_PAGEUP = (int)SDL_Scancode.SDL_SCANCODE_PAGEUP | SDLK_SCANCODE_MASK,
			SDLK_DELETE = 127,
			SDLK_END = (int)SDL_Scancode.SDL_SCANCODE_END | SDLK_SCANCODE_MASK,
			SDLK_PAGEDOWN = (int)SDL_Scancode.SDL_SCANCODE_PAGEDOWN | SDLK_SCANCODE_MASK,
			SDLK_RIGHT = (int)SDL_Scancode.SDL_SCANCODE_RIGHT | SDLK_SCANCODE_MASK,
			SDLK_LEFT = (int)SDL_Scancode.SDL_SCANCODE_LEFT | SDLK_SCANCODE_MASK,
			SDLK_DOWN = (int)SDL_Scancode.SDL_SCANCODE_DOWN | SDLK_SCANCODE_MASK,
			SDLK_UP = (int)SDL_Scancode.SDL_SCANCODE_UP | SDLK_SCANCODE_MASK,

			SDLK_NUMLOCKCLEAR = (int)SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR | SDLK_SCANCODE_MASK,
			SDLK_KP_DIVIDE = (int)SDL_Scancode.SDL_SCANCODE_KP_DIVIDE | SDLK_SCANCODE_MASK,
			SDLK_KP_MULTIPLY = (int)SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY | SDLK_SCANCODE_MASK,
			SDLK_KP_MINUS = (int)SDL_Scancode.SDL_SCANCODE_KP_MINUS | SDLK_SCANCODE_MASK,
			SDLK_KP_PLUS = (int)SDL_Scancode.SDL_SCANCODE_KP_PLUS | SDLK_SCANCODE_MASK,
			SDLK_KP_ENTER = (int)SDL_Scancode.SDL_SCANCODE_KP_ENTER | SDLK_SCANCODE_MASK,
			SDLK_KP_1 = (int)SDL_Scancode.SDL_SCANCODE_KP_1 | SDLK_SCANCODE_MASK,
			SDLK_KP_2 = (int)SDL_Scancode.SDL_SCANCODE_KP_2 | SDLK_SCANCODE_MASK,
			SDLK_KP_3 = (int)SDL_Scancode.SDL_SCANCODE_KP_3 | SDLK_SCANCODE_MASK,
			SDLK_KP_4 = (int)SDL_Scancode.SDL_SCANCODE_KP_4 | SDLK_SCANCODE_MASK,
			SDLK_KP_5 = (int)SDL_Scancode.SDL_SCANCODE_KP_5 | SDLK_SCANCODE_MASK,
			SDLK_KP_6 = (int)SDL_Scancode.SDL_SCANCODE_KP_6 | SDLK_SCANCODE_MASK,
			SDLK_KP_7 = (int)SDL_Scancode.SDL_SCANCODE_KP_7 | SDLK_SCANCODE_MASK,
			SDLK_KP_8 = (int)SDL_Scancode.SDL_SCANCODE_KP_8 | SDLK_SCANCODE_MASK,
			SDLK_KP_9 = (int)SDL_Scancode.SDL_SCANCODE_KP_9 | SDLK_SCANCODE_MASK,
			SDLK_KP_0 = (int)SDL_Scancode.SDL_SCANCODE_KP_0 | SDLK_SCANCODE_MASK,
			SDLK_KP_PERIOD = (int)SDL_Scancode.SDL_SCANCODE_KP_PERIOD | SDLK_SCANCODE_MASK,

			SDLK_APPLICATION = (int)SDL_Scancode.SDL_SCANCODE_APPLICATION | SDLK_SCANCODE_MASK,
			SDLK_POWER = (int)SDL_Scancode.SDL_SCANCODE_POWER | SDLK_SCANCODE_MASK,
			SDLK_KP_EQUALS = (int)SDL_Scancode.SDL_SCANCODE_KP_EQUALS | SDLK_SCANCODE_MASK,
			SDLK_F13 = (int)SDL_Scancode.SDL_SCANCODE_F13 | SDLK_SCANCODE_MASK,
			SDLK_F14 = (int)SDL_Scancode.SDL_SCANCODE_F14 | SDLK_SCANCODE_MASK,
			SDLK_F15 = (int)SDL_Scancode.SDL_SCANCODE_F15 | SDLK_SCANCODE_MASK,
			SDLK_F16 = (int)SDL_Scancode.SDL_SCANCODE_F16 | SDLK_SCANCODE_MASK,
			SDLK_F17 = (int)SDL_Scancode.SDL_SCANCODE_F17 | SDLK_SCANCODE_MASK,
			SDLK_F18 = (int)SDL_Scancode.SDL_SCANCODE_F18 | SDLK_SCANCODE_MASK,
			SDLK_F19 = (int)SDL_Scancode.SDL_SCANCODE_F19 | SDLK_SCANCODE_MASK,
			SDLK_F20 = (int)SDL_Scancode.SDL_SCANCODE_F20 | SDLK_SCANCODE_MASK,
			SDLK_F21 = (int)SDL_Scancode.SDL_SCANCODE_F21 | SDLK_SCANCODE_MASK,
			SDLK_F22 = (int)SDL_Scancode.SDL_SCANCODE_F22 | SDLK_SCANCODE_MASK,
			SDLK_F23 = (int)SDL_Scancode.SDL_SCANCODE_F23 | SDLK_SCANCODE_MASK,
			SDLK_F24 = (int)SDL_Scancode.SDL_SCANCODE_F24 | SDLK_SCANCODE_MASK,
			SDLK_EXECUTE = (int)SDL_Scancode.SDL_SCANCODE_EXECUTE | SDLK_SCANCODE_MASK,
			SDLK_HELP = (int)SDL_Scancode.SDL_SCANCODE_HELP | SDLK_SCANCODE_MASK,
			SDLK_MENU = (int)SDL_Scancode.SDL_SCANCODE_MENU | SDLK_SCANCODE_MASK,
			SDLK_SELECT = (int)SDL_Scancode.SDL_SCANCODE_SELECT | SDLK_SCANCODE_MASK,
			SDLK_STOP = (int)SDL_Scancode.SDL_SCANCODE_STOP | SDLK_SCANCODE_MASK,
			SDLK_AGAIN = (int)SDL_Scancode.SDL_SCANCODE_AGAIN | SDLK_SCANCODE_MASK,
			SDLK_UNDO = (int)SDL_Scancode.SDL_SCANCODE_UNDO | SDLK_SCANCODE_MASK,
			SDLK_CUT = (int)SDL_Scancode.SDL_SCANCODE_CUT | SDLK_SCANCODE_MASK,
			SDLK_COPY = (int)SDL_Scancode.SDL_SCANCODE_COPY | SDLK_SCANCODE_MASK,
			SDLK_PASTE = (int)SDL_Scancode.SDL_SCANCODE_PASTE | SDLK_SCANCODE_MASK,
			SDLK_FIND = (int)SDL_Scancode.SDL_SCANCODE_FIND | SDLK_SCANCODE_MASK,
			SDLK_MUTE = (int)SDL_Scancode.SDL_SCANCODE_MUTE | SDLK_SCANCODE_MASK,
			SDLK_VOLUMEUP = (int)SDL_Scancode.SDL_SCANCODE_VOLUMEUP | SDLK_SCANCODE_MASK,
			SDLK_VOLUMEDOWN = (int)SDL_Scancode.SDL_SCANCODE_VOLUMEDOWN | SDLK_SCANCODE_MASK,
			SDLK_KP_COMMA = (int)SDL_Scancode.SDL_SCANCODE_KP_COMMA | SDLK_SCANCODE_MASK,
			SDLK_KP_EQUALSAS400 =
			(int)SDL_Scancode.SDL_SCANCODE_KP_EQUALSAS400 | SDLK_SCANCODE_MASK,

			SDLK_ALTERASE = (int)SDL_Scancode.SDL_SCANCODE_ALTERASE | SDLK_SCANCODE_MASK,
			SDLK_SYSREQ = (int)SDL_Scancode.SDL_SCANCODE_SYSREQ | SDLK_SCANCODE_MASK,
			SDLK_CANCEL = (int)SDL_Scancode.SDL_SCANCODE_CANCEL | SDLK_SCANCODE_MASK,
			SDLK_CLEAR = (int)SDL_Scancode.SDL_SCANCODE_CLEAR | SDLK_SCANCODE_MASK,
			SDLK_PRIOR = (int)SDL_Scancode.SDL_SCANCODE_PRIOR | SDLK_SCANCODE_MASK,
			SDLK_RETURN2 = (int)SDL_Scancode.SDL_SCANCODE_RETURN2 | SDLK_SCANCODE_MASK,
			SDLK_SEPARATOR = (int)SDL_Scancode.SDL_SCANCODE_SEPARATOR | SDLK_SCANCODE_MASK,
			SDLK_OUT = (int)SDL_Scancode.SDL_SCANCODE_OUT | SDLK_SCANCODE_MASK,
			SDLK_OPER = (int)SDL_Scancode.SDL_SCANCODE_OPER | SDLK_SCANCODE_MASK,
			SDLK_CLEARAGAIN = (int)SDL_Scancode.SDL_SCANCODE_CLEARAGAIN | SDLK_SCANCODE_MASK,
			SDLK_CRSEL = (int)SDL_Scancode.SDL_SCANCODE_CRSEL | SDLK_SCANCODE_MASK,
			SDLK_EXSEL = (int)SDL_Scancode.SDL_SCANCODE_EXSEL | SDLK_SCANCODE_MASK,

			SDLK_KP_00 = (int)SDL_Scancode.SDL_SCANCODE_KP_00 | SDLK_SCANCODE_MASK,
			SDLK_KP_000 = (int)SDL_Scancode.SDL_SCANCODE_KP_000 | SDLK_SCANCODE_MASK,
			SDLK_THOUSANDSSEPARATOR =
			(int)SDL_Scancode.SDL_SCANCODE_THOUSANDSSEPARATOR | SDLK_SCANCODE_MASK,
			SDLK_DECIMALSEPARATOR =
			(int)SDL_Scancode.SDL_SCANCODE_DECIMALSEPARATOR | SDLK_SCANCODE_MASK,
			SDLK_CURRENCYUNIT = (int)SDL_Scancode.SDL_SCANCODE_CURRENCYUNIT | SDLK_SCANCODE_MASK,
			SDLK_CURRENCYSUBUNIT =
			(int)SDL_Scancode.SDL_SCANCODE_CURRENCYSUBUNIT | SDLK_SCANCODE_MASK,
			SDLK_KP_LEFTPAREN = (int)SDL_Scancode.SDL_SCANCODE_KP_LEFTPAREN | SDLK_SCANCODE_MASK,
			SDLK_KP_RIGHTPAREN = (int)SDL_Scancode.SDL_SCANCODE_KP_RIGHTPAREN | SDLK_SCANCODE_MASK,
			SDLK_KP_LEFTBRACE = (int)SDL_Scancode.SDL_SCANCODE_KP_LEFTBRACE | SDLK_SCANCODE_MASK,
			SDLK_KP_RIGHTBRACE = (int)SDL_Scancode.SDL_SCANCODE_KP_RIGHTBRACE | SDLK_SCANCODE_MASK,
			SDLK_KP_TAB = (int)SDL_Scancode.SDL_SCANCODE_KP_TAB | SDLK_SCANCODE_MASK,
			SDLK_KP_BACKSPACE = (int)SDL_Scancode.SDL_SCANCODE_KP_BACKSPACE | SDLK_SCANCODE_MASK,
			SDLK_KP_A = (int)SDL_Scancode.SDL_SCANCODE_KP_A | SDLK_SCANCODE_MASK,
			SDLK_KP_B = (int)SDL_Scancode.SDL_SCANCODE_KP_B | SDLK_SCANCODE_MASK,
			SDLK_KP_C = (int)SDL_Scancode.SDL_SCANCODE_KP_C | SDLK_SCANCODE_MASK,
			SDLK_KP_D = (int)SDL_Scancode.SDL_SCANCODE_KP_D | SDLK_SCANCODE_MASK,
			SDLK_KP_E = (int)SDL_Scancode.SDL_SCANCODE_KP_E | SDLK_SCANCODE_MASK,
			SDLK_KP_F = (int)SDL_Scancode.SDL_SCANCODE_KP_F | SDLK_SCANCODE_MASK,
			SDLK_KP_XOR = (int)SDL_Scancode.SDL_SCANCODE_KP_XOR | SDLK_SCANCODE_MASK,
			SDLK_KP_POWER = (int)SDL_Scancode.SDL_SCANCODE_KP_POWER | SDLK_SCANCODE_MASK,
			SDLK_KP_PERCENT = (int)SDL_Scancode.SDL_SCANCODE_KP_PERCENT | SDLK_SCANCODE_MASK,
			SDLK_KP_LESS = (int)SDL_Scancode.SDL_SCANCODE_KP_LESS | SDLK_SCANCODE_MASK,
			SDLK_KP_GREATER = (int)SDL_Scancode.SDL_SCANCODE_KP_GREATER | SDLK_SCANCODE_MASK,
			SDLK_KP_AMPERSAND = (int)SDL_Scancode.SDL_SCANCODE_KP_AMPERSAND | SDLK_SCANCODE_MASK,
			SDLK_KP_DBLAMPERSAND =
			(int)SDL_Scancode.SDL_SCANCODE_KP_DBLAMPERSAND | SDLK_SCANCODE_MASK,
			SDLK_KP_VERTICALBAR =
			(int)SDL_Scancode.SDL_SCANCODE_KP_VERTICALBAR | SDLK_SCANCODE_MASK,
			SDLK_KP_DBLVERTICALBAR =
			(int)SDL_Scancode.SDL_SCANCODE_KP_DBLVERTICALBAR | SDLK_SCANCODE_MASK,
			SDLK_KP_COLON = (int)SDL_Scancode.SDL_SCANCODE_KP_COLON | SDLK_SCANCODE_MASK,
			SDLK_KP_HASH = (int)SDL_Scancode.SDL_SCANCODE_KP_HASH | SDLK_SCANCODE_MASK,
			SDLK_KP_SPACE = (int)SDL_Scancode.SDL_SCANCODE_KP_SPACE | SDLK_SCANCODE_MASK,
			SDLK_KP_AT = (int)SDL_Scancode.SDL_SCANCODE_KP_AT | SDLK_SCANCODE_MASK,
			SDLK_KP_EXCLAM = (int)SDL_Scancode.SDL_SCANCODE_KP_EXCLAM | SDLK_SCANCODE_MASK,
			SDLK_KP_MEMSTORE = (int)SDL_Scancode.SDL_SCANCODE_KP_MEMSTORE | SDLK_SCANCODE_MASK,
			SDLK_KP_MEMRECALL = (int)SDL_Scancode.SDL_SCANCODE_KP_MEMRECALL | SDLK_SCANCODE_MASK,
			SDLK_KP_MEMCLEAR = (int)SDL_Scancode.SDL_SCANCODE_KP_MEMCLEAR | SDLK_SCANCODE_MASK,
			SDLK_KP_MEMADD = (int)SDL_Scancode.SDL_SCANCODE_KP_MEMADD | SDLK_SCANCODE_MASK,
			SDLK_KP_MEMSUBTRACT =
			(int)SDL_Scancode.SDL_SCANCODE_KP_MEMSUBTRACT | SDLK_SCANCODE_MASK,
			SDLK_KP_MEMMULTIPLY =
			(int)SDL_Scancode.SDL_SCANCODE_KP_MEMMULTIPLY | SDLK_SCANCODE_MASK,
			SDLK_KP_MEMDIVIDE = (int)SDL_Scancode.SDL_SCANCODE_KP_MEMDIVIDE | SDLK_SCANCODE_MASK,
			SDLK_KP_PLUSMINUS = (int)SDL_Scancode.SDL_SCANCODE_KP_PLUSMINUS | SDLK_SCANCODE_MASK,
			SDLK_KP_CLEAR = (int)SDL_Scancode.SDL_SCANCODE_KP_CLEAR | SDLK_SCANCODE_MASK,
			SDLK_KP_CLEARENTRY = (int)SDL_Scancode.SDL_SCANCODE_KP_CLEARENTRY | SDLK_SCANCODE_MASK,
			SDLK_KP_BINARY = (int)SDL_Scancode.SDL_SCANCODE_KP_BINARY | SDLK_SCANCODE_MASK,
			SDLK_KP_OCTAL = (int)SDL_Scancode.SDL_SCANCODE_KP_OCTAL | SDLK_SCANCODE_MASK,
			SDLK_KP_DECIMAL = (int)SDL_Scancode.SDL_SCANCODE_KP_DECIMAL | SDLK_SCANCODE_MASK,
			SDLK_KP_HEXADECIMAL =
			(int)SDL_Scancode.SDL_SCANCODE_KP_HEXADECIMAL | SDLK_SCANCODE_MASK,

			SDLK_LCTRL = (int)SDL_Scancode.SDL_SCANCODE_LCTRL | SDLK_SCANCODE_MASK,
			SDLK_LSHIFT = (int)SDL_Scancode.SDL_SCANCODE_LSHIFT | SDLK_SCANCODE_MASK,
			SDLK_LALT = (int)SDL_Scancode.SDL_SCANCODE_LALT | SDLK_SCANCODE_MASK,
			SDLK_LGUI = (int)SDL_Scancode.SDL_SCANCODE_LGUI | SDLK_SCANCODE_MASK,
			SDLK_RCTRL = (int)SDL_Scancode.SDL_SCANCODE_RCTRL | SDLK_SCANCODE_MASK,
			SDLK_RSHIFT = (int)SDL_Scancode.SDL_SCANCODE_RSHIFT | SDLK_SCANCODE_MASK,
			SDLK_RALT = (int)SDL_Scancode.SDL_SCANCODE_RALT | SDLK_SCANCODE_MASK,
			SDLK_RGUI = (int)SDL_Scancode.SDL_SCANCODE_RGUI | SDLK_SCANCODE_MASK,

			SDLK_MODE = (int)SDL_Scancode.SDL_SCANCODE_MODE | SDLK_SCANCODE_MASK,

			SDLK_AUDIONEXT = (int)SDL_Scancode.SDL_SCANCODE_AUDIONEXT | SDLK_SCANCODE_MASK,
			SDLK_AUDIOPREV = (int)SDL_Scancode.SDL_SCANCODE_AUDIOPREV | SDLK_SCANCODE_MASK,
			SDLK_AUDIOSTOP = (int)SDL_Scancode.SDL_SCANCODE_AUDIOSTOP | SDLK_SCANCODE_MASK,
			SDLK_AUDIOPLAY = (int)SDL_Scancode.SDL_SCANCODE_AUDIOPLAY | SDLK_SCANCODE_MASK,
			SDLK_AUDIOMUTE = (int)SDL_Scancode.SDL_SCANCODE_AUDIOMUTE | SDLK_SCANCODE_MASK,
			SDLK_MEDIASELECT = (int)SDL_Scancode.SDL_SCANCODE_MEDIASELECT | SDLK_SCANCODE_MASK,
			SDLK_WWW = (int)SDL_Scancode.SDL_SCANCODE_WWW | SDLK_SCANCODE_MASK,
			SDLK_MAIL = (int)SDL_Scancode.SDL_SCANCODE_MAIL | SDLK_SCANCODE_MASK,
			SDLK_CALCULATOR = (int)SDL_Scancode.SDL_SCANCODE_CALCULATOR | SDLK_SCANCODE_MASK,
			SDLK_COMPUTER = (int)SDL_Scancode.SDL_SCANCODE_COMPUTER | SDLK_SCANCODE_MASK,
			SDLK_AC_SEARCH = (int)SDL_Scancode.SDL_SCANCODE_AC_SEARCH | SDLK_SCANCODE_MASK,
			SDLK_AC_HOME = (int)SDL_Scancode.SDL_SCANCODE_AC_HOME | SDLK_SCANCODE_MASK,
			SDLK_AC_BACK = (int)SDL_Scancode.SDL_SCANCODE_AC_BACK | SDLK_SCANCODE_MASK,
			SDLK_AC_FORWARD = (int)SDL_Scancode.SDL_SCANCODE_AC_FORWARD | SDLK_SCANCODE_MASK,
			SDLK_AC_STOP = (int)SDL_Scancode.SDL_SCANCODE_AC_STOP | SDLK_SCANCODE_MASK,
			SDLK_AC_REFRESH = (int)SDL_Scancode.SDL_SCANCODE_AC_REFRESH | SDLK_SCANCODE_MASK,
			SDLK_AC_BOOKMARKS = (int)SDL_Scancode.SDL_SCANCODE_AC_BOOKMARKS | SDLK_SCANCODE_MASK,

			SDLK_BRIGHTNESSDOWN =
			(int)SDL_Scancode.SDL_SCANCODE_BRIGHTNESSDOWN | SDLK_SCANCODE_MASK,
			SDLK_BRIGHTNESSUP = (int)SDL_Scancode.SDL_SCANCODE_BRIGHTNESSUP | SDLK_SCANCODE_MASK,
			SDLK_DISPLAYSWITCH = (int)SDL_Scancode.SDL_SCANCODE_DISPLAYSWITCH | SDLK_SCANCODE_MASK,
			SDLK_KBDILLUMTOGGLE =
			(int)SDL_Scancode.SDL_SCANCODE_KBDILLUMTOGGLE | SDLK_SCANCODE_MASK,
			SDLK_KBDILLUMDOWN = (int)SDL_Scancode.SDL_SCANCODE_KBDILLUMDOWN | SDLK_SCANCODE_MASK,
			SDLK_KBDILLUMUP = (int)SDL_Scancode.SDL_SCANCODE_KBDILLUMUP | SDLK_SCANCODE_MASK,
			SDLK_EJECT = (int)SDL_Scancode.SDL_SCANCODE_EJECT | SDLK_SCANCODE_MASK,
			SDLK_SLEEP = (int)SDL_Scancode.SDL_SCANCODE_SLEEP | SDLK_SCANCODE_MASK
		}

		/* Key modifiers (bitfield) */
		[Flags]
		public enum SDL_Keymod : ushort
		{
			KMOD_NONE = 0x0000,
			KMOD_LSHIFT = 0x0001,
			KMOD_RSHIFT = 0x0002,
			KMOD_LCTRL = 0x0040,
			KMOD_RCTRL = 0x0080,
			KMOD_LALT = 0x0100,
			KMOD_RALT = 0x0200,
			KMOD_LGUI = 0x0400,
			KMOD_RGUI = 0x0800,
			KMOD_NUM = 0x1000,
			KMOD_CAPS = 0x2000,
			KMOD_MODE = 0x4000,
			KMOD_RESERVED = 0x8000,

			/* These are defines in the SDL headers */
			KMOD_CTRL = (KMOD_LCTRL | KMOD_RCTRL),
			KMOD_SHIFT = (KMOD_LSHIFT | KMOD_RSHIFT),
			KMOD_ALT = (KMOD_LALT | KMOD_RALT),
			KMOD_GUI = (KMOD_LGUI | KMOD_RGUI)
		}

		#endregion

		#region SDL_keyboard.h

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_Keysym
		{
			public SDL_Scancode scancode;
			public SDL_Keycode sym;
			public SDL_Keymod mod; /* UInt16 */
			public UInt32 unicode; /* Deprecated */
		}

		/* Get the window which has kbd focus */
		/* Return type is an SDL_Window pointer */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GetKeyboardFocus();

		/* Get a snapshot of the keyboard state. */
		/* Return value is a pointer to a UInt8 array */
		/* Numkeys returns the size of the array if non-null */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GetKeyboardState(out int numkeys);

		/* Get the current key modifier state for the keyboard. */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_Keymod SDL_GetModState();

		/* Set the current key modifier state */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetModState(SDL_Keymod modstate);

		/* Get the key code corresponding to the given scancode
		 * with the current keyboard layout.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_Keycode SDL_GetKeyFromScancode(SDL_Scancode scancode);

		/* Get the scancode for the given keycode */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_Scancode SDL_GetScancodeFromKey(SDL_Keycode key);

		/* Wrapper for SDL_GetScancodeName */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GetScancodeName(SDL_Scancode scancode);

		/* Get a scancode from a human-readable name */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_Scancode SDL_GetScancodeFromName(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))] string name
		);

		/* Wrapper for SDL_GetKeyName */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GetKeyName(SDL_Keycode key);

		/* Get a key code from a human-readable name */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_Keycode SDL_GetKeyFromName(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))] string name
		);

		/* Start accepting Unicode text input events, show keyboard */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_StartTextInput();

		/* Check if unicode input events are enabled */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_IsTextInputActive();

		/* Stop receiving any text input events, hide onscreen kbd */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_StopTextInput();

		/* Set the rectangle used for text input, hint for IME */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetTextInputRect(ref SDL_Rect rect);

		/* Does the platform support an on-screen keyboard? */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_HasScreenKeyboardSupport();

		/* Is the on-screen keyboard shown for a given window? */
		/* window is an SDL_Window pointer */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_IsScreenKeyboardShown(IntPtr window);

		#endregion

		#region SDL_mouse.c

		/* Note: SDL_Cursor is a typedef normally. We'll treat it as
		 * an IntPtr, because C# doesn't do typedefs. Yay!
		 */

		/* System cursor types */
		public enum SDL_SystemCursor
		{
			SDL_SYSTEM_CURSOR_ARROW,	// Arrow
			SDL_SYSTEM_CURSOR_IBEAM,	// I-beam
			SDL_SYSTEM_CURSOR_WAIT,		// Wait
			SDL_SYSTEM_CURSOR_CROSSHAIR,	// Crosshair
			SDL_SYSTEM_CURSOR_WAITARROW,	// Small wait cursor (or Wait if not available)
			SDL_SYSTEM_CURSOR_SIZENWSE,	// Double arrow pointing northwest and southeast
			SDL_SYSTEM_CURSOR_SIZENESW,	// Double arrow pointing northeast and southwest
			SDL_SYSTEM_CURSOR_SIZEWE,	// Double arrow pointing west and east
			SDL_SYSTEM_CURSOR_SIZENS,	// Double arrow pointing north and south
			SDL_SYSTEM_CURSOR_SIZEALL,	// Four pointed arrow pointing north, south, east, and west
			SDL_SYSTEM_CURSOR_NO,		// Slashed circle or crossbones
			SDL_SYSTEM_CURSOR_HAND,		// Hand
			SDL_NUM_SYSTEM_CURSORS
		}

		/* Get the window which currently has mouse focus */
		/* Return value is an SDL_Window pointer */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GetMouseFocus();

		/* Get the current state of the mouse */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern UInt32 SDL_GetMouseState(out int x, out int y);

		/* Get the current state of the mouse */
		/* This overload allows for passing NULL to x */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern UInt32 SDL_GetMouseState(IntPtr x, out int y);

		/* Get the current state of the mouse */
		/* This overload allows for passing NULL to y */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern UInt32 SDL_GetMouseState(out int x, IntPtr y);

		/* Get the current state of the mouse */
		/* This overload allows for passing NULL to both x and y */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern UInt32 SDL_GetMouseState(IntPtr x, IntPtr y);

		/* Get the current state of the mouse, in relation to the desktop */
		/* Only available in 2.0.4 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern UInt32 SDL_GetGlobalMouseState(out int x, out int y);

		/* Get the current state of the mouse, in relation to the desktop */
		/* Only available in 2.0.4 */
		/* This overload allows for passing NULL to x */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern UInt32 SDL_GetGlobalMouseState(IntPtr x, out int y);

		/* Get the current state of the mouse, in relation to the desktop */
		/* Only available in 2.0.4 */
		/* This overload allows for passing NULL to y */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern UInt32 SDL_GetGlobalMouseState(out int x, IntPtr y);

		/* Get the current state of the mouse, in relation to the desktop */
		/* Only available in 2.0.4 */
		/* This overload allows for passing NULL to both x and y */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern UInt32 SDL_GetGlobalMouseState(IntPtr x, IntPtr y);

		/* Get the mouse state with relative coords*/
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern UInt32 SDL_GetRelativeMouseState(out int x, out int y);

		/* Set the mouse cursor's position (within a window) */
		/* window is an SDL_Window pointer */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_WarpMouseInWindow(IntPtr window, int x, int y);

		/* Set the mouse cursor's position in global screen space */
		/* Only available in 2.0.4 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_WarpMouseGlobal(int x, int y);

		/* Enable/Disable relative mouse mode (grabs mouse, rel coords) */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetRelativeMouseMode(SDL_bool enabled);

		/* Capture the mouse, to track input outside an SDL window */
		/* Only available in 2.0.4 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_CaptureMouse(SDL_bool enabled);

		/* Query if the relative mouse mode is enabled */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_GetRelativeMouseMode();

		/* Create a cursor from bitmap data (amd mask) in MSB format */
		/* data and mask are byte arrays, and w must be a multiple of 8 */
		/* return value is an SDL_Cursor pointer */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_CreateCursor(
			IntPtr data,
			IntPtr mask,
			int w,
			int h,
			int hot_x,
			int hot_y
		);

		/* Create a cursor from an SDL_Surface */
		/* IntPtr refers to an SDL_Cursor*, surface to an SDL_Surface* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_CreateColorCursor(
			IntPtr surface,
			int hot_x,
			int hot_y
		);

		/* Create a cursor from a system cursor id */
		/* return value is an SDL_Cursor pointer */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_CreateSystemCursor(SDL_SystemCursor id);

		/* Set the active cursor */
		/* cursor is an SDL_Cursor pointer */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetCursor(IntPtr cursor);

		/* Return the active cursor */
		/* return value is an SDL_Cursor pointer */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GetCursor();

		/* Frees a cursor created with one of the CreateCursor functions */
		/* cursor in an SDL_Cursor pointer */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_FreeCursor(IntPtr cursor);

		/* Toggle whether or not the cursor is shown */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_ShowCursor(int toggle);

		public static uint SDL_BUTTON(uint X)
		{
			// If only there were a better way of doing this in C#
			return (uint) (1 << ((int) X - 1));
		}

		public const uint SDL_BUTTON_LEFT =	1;
		public const uint SDL_BUTTON_MIDDLE =	2;
		public const uint SDL_BUTTON_RIGHT =	3;
		public const uint SDL_BUTTON_X1 =	4;
		public const uint SDL_BUTTON_X2 =	5;
		public static readonly UInt32 SDL_BUTTON_LMASK =	SDL_BUTTON(SDL_BUTTON_LEFT);
		public static readonly UInt32 SDL_BUTTON_MMASK =	SDL_BUTTON(SDL_BUTTON_MIDDLE);
		public static readonly UInt32 SDL_BUTTON_RMASK =	SDL_BUTTON(SDL_BUTTON_RIGHT);
		public static readonly UInt32 SDL_BUTTON_X1MASK =	SDL_BUTTON(SDL_BUTTON_X1);
		public static readonly UInt32 SDL_BUTTON_X2MASK =	SDL_BUTTON(SDL_BUTTON_X2);

		#endregion

		#region SDL_touch.h

		public const uint SDL_TOUCH_MOUSEID = uint.MaxValue;

		public struct SDL_Finger
		{
			public long id; // SDL_FingerID
			public float x;
			public float y;
			public float pressure;
		}

		/**
		 *  \brief Get the number of registered touch devices.
 		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetNumTouchDevices();

		/**
		 *  \brief Get the touch ID with the given index, or 0 if the index is invalid.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern long SDL_GetTouchDevice(int index);

		/**
		 *  \brief Get the number of active fingers for a given touch device.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetNumTouchFingers(long touchID);

		/**
		 *  \brief Get the finger object of the given touch, with the given index.
		 *  Returns pointer to SDL_Finger.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GetTouchFinger(long touchID, int index);

		#endregion

		#region SDL_joystick.h

		public const byte SDL_HAT_CENTERED =	0x00;
		public const byte SDL_HAT_UP =		0x01;
		public const byte SDL_HAT_RIGHT =	0x02;
		public const byte SDL_HAT_DOWN =	0x04;
		public const byte SDL_HAT_LEFT =	0x08;
		public const byte SDL_HAT_RIGHTUP =	SDL_HAT_RIGHT | SDL_HAT_UP;
		public const byte SDL_HAT_RIGHTDOWN =	SDL_HAT_RIGHT | SDL_HAT_DOWN;
		public const byte SDL_HAT_LEFTUP =	SDL_HAT_LEFT | SDL_HAT_UP;
		public const byte SDL_HAT_LEFTDOWN =	SDL_HAT_LEFT | SDL_HAT_DOWN;

		public enum SDL_JoystickPowerLevel
		{
			SDL_JOYSTICK_POWER_UNKNOWN = -1,
			SDL_JOYSTICK_POWER_EMPTY,
			SDL_JOYSTICK_POWER_LOW,
			SDL_JOYSTICK_POWER_MEDIUM,
			SDL_JOYSTICK_POWER_FULL,
			SDL_JOYSTICK_POWER_WIRED,
			SDL_JOYSTICK_POWER_MAX
		}

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_JoystickClose(IntPtr joystick);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickEventState(int state);

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern short SDL_JoystickGetAxis(
			IntPtr joystick,
			int axis
		);

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickGetBall(
			IntPtr joystick,
			int ball,
			out int dx,
			out int dy
		);

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte SDL_JoystickGetButton(
			IntPtr joystick,
			int button
		);

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte SDL_JoystickGetHat(
			IntPtr joystick,
			int hat
		);

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_JoystickName(
			IntPtr joystick
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_JoystickNameForIndex(
			int device_index
		);

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickNumAxes(IntPtr joystick);

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickNumBalls(IntPtr joystick);

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickNumButtons(IntPtr joystick);

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickNumHats(IntPtr joystick);

		/* IntPtr refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_JoystickOpen(int device_index);

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickOpened(int device_index);

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_JoystickUpdate();

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_NumJoysticks();

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern Guid SDL_JoystickGetDeviceGUID(
			int device_index
		);

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern Guid SDL_JoystickGetGUID(
			IntPtr joystick
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_JoystickGetGUIDString(
			Guid guid,
			byte[] pszGUID,
			int cbGUID
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern Guid SDL_JoystickGetGUIDFromString(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string pchGUID
		);

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_JoystickGetAttached(IntPtr joystick);

		/* int refers to an SDL_JoystickID, joystick to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickInstanceID(IntPtr joystick);

		/* joystick refers to an SDL_Joystick*.
		 * This function is only available in 2.0.4 or higher.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_JoystickPowerLevel SDL_JoystickCurrentPowerLevel(
			IntPtr joystick
		);

		/* int refers to an SDL_JoystickID, IntPtr to an SDL_Joystick*.
		 * This function is only available in 2.0.4 or higher.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_JoystickFromInstanceID(int joyid);

		#endregion

		#region SDL_gamecontroller.h

		public enum SDL_GameControllerBindType
		{
			SDL_CONTROLLER_BINDTYPE_NONE,
			SDL_CONTROLLER_BINDTYPE_BUTTON,
			SDL_CONTROLLER_BINDTYPE_AXIS,
			SDL_CONTROLLER_BINDTYPE_HAT
		}

		public enum SDL_GameControllerAxis
		{
			SDL_CONTROLLER_AXIS_INVALID = -1,
			SDL_CONTROLLER_AXIS_LEFTX,
			SDL_CONTROLLER_AXIS_LEFTY,
			SDL_CONTROLLER_AXIS_RIGHTX,
			SDL_CONTROLLER_AXIS_RIGHTY,
			SDL_CONTROLLER_AXIS_TRIGGERLEFT,
			SDL_CONTROLLER_AXIS_TRIGGERRIGHT,
			SDL_CONTROLLER_AXIS_MAX
		}

		public enum SDL_GameControllerButton
		{
			SDL_CONTROLLER_BUTTON_INVALID = -1,
			SDL_CONTROLLER_BUTTON_A,
			SDL_CONTROLLER_BUTTON_B,
			SDL_CONTROLLER_BUTTON_X,
			SDL_CONTROLLER_BUTTON_Y,
			SDL_CONTROLLER_BUTTON_BACK,
			SDL_CONTROLLER_BUTTON_GUIDE,
			SDL_CONTROLLER_BUTTON_START,
			SDL_CONTROLLER_BUTTON_LEFTSTICK,
			SDL_CONTROLLER_BUTTON_RIGHTSTICK,
			SDL_CONTROLLER_BUTTON_LEFTSHOULDER,
			SDL_CONTROLLER_BUTTON_RIGHTSHOULDER,
			SDL_CONTROLLER_BUTTON_DPAD_UP,
			SDL_CONTROLLER_BUTTON_DPAD_DOWN,
			SDL_CONTROLLER_BUTTON_DPAD_LEFT,
			SDL_CONTROLLER_BUTTON_DPAD_RIGHT,
			SDL_CONTROLLER_BUTTON_MAX,
		}

		// FIXME: I'd rather this somehow be private...
		[StructLayout(LayoutKind.Sequential)]
		public struct INTERNAL_GameControllerButtonBind_hat
		{
			public int hat;
			public int hat_mask;
		}

		/* This struct has a union in it, hence the Explicit layout. */
		[StructLayout(LayoutKind.Explicit)]
		public struct SDL_GameControllerButtonBind
		{
			/* Note: enum size is 4 bytes. */
			[FieldOffset(0)]
			public SDL_GameControllerBindType bindType;
			[FieldOffset(4)]
			public int button;
			[FieldOffset(4)]
			public int axis;
			[FieldOffset(4)]
			public INTERNAL_GameControllerButtonBind_hat hat;
		}

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GameControllerAddMapping(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string mappingString
		);

		/* THIS IS AN RWops FUNCTION! */
		[DllImport(nativeLibName, EntryPoint = "SDL_GameControllerAddMappingsFromRW", CallingConvention = CallingConvention.Cdecl)]
		private static extern int INTERNAL_SDL_GameControllerAddMappingsFromRW(
			IntPtr rw,
			int freerw
		);
		public static int SDL_GameControllerAddMappingsFromFile(string file)
		{
			IntPtr rwops = INTERNAL_SDL_RWFromFile(file, "rb");
			return INTERNAL_SDL_GameControllerAddMappingsFromRW(rwops, 1);
		}

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GameControllerMappingForGUID(
			Guid guid
		);

		/* gamecontroller refers to an SDL_GameController* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GameControllerMapping(
			IntPtr gamecontroller
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_IsGameController(int joystick_index);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GameControllerNameForIndex(
			int joystick_index
		);

		/* IntPtr refers to an SDL_GameController* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GameControllerOpen(int joystick_index);

		/* gamecontroller refers to an SDL_GameController* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GameControllerName(
			IntPtr gamecontroller
		);

		/* gamecontroller refers to an SDL_GameController* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_GameControllerGetAttached(
			IntPtr gamecontroller
		);

		/* IntPtr refers to an SDL_Joystick*
		 * gamecontroller refers to an SDL_GameController*
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GameControllerGetJoystick(
			IntPtr gamecontroller
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GameControllerEventState(int state);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GameControllerUpdate();

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_GameControllerAxis SDL_GameControllerGetAxisFromString(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string pchString
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GameControllerGetStringForAxis(
			SDL_GameControllerAxis axis
		);

		/* gamecontroller refers to an SDL_GameController* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_GameControllerButtonBind SDL_GameControllerGetBindForAxis(
			IntPtr gamecontroller,
			SDL_GameControllerAxis axis
		);

		/* gamecontroller refers to an SDL_GameController* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern short SDL_GameControllerGetAxis(
			IntPtr gamecontroller,
			SDL_GameControllerAxis axis
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_GameControllerButton SDL_GameControllerGetButtonFromString(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string pchString
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GameControllerGetStringForButton(
			SDL_GameControllerButton button
		);

		/* gamecontroller refers to an SDL_GameController* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_GameControllerButtonBind SDL_GameControllerGetBindForButton(
			IntPtr gamecontroller,
			SDL_GameControllerButton button
		);

		/* gamecontroller refers to an SDL_GameController* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte SDL_GameControllerGetButton(
			IntPtr gamecontroller,
			SDL_GameControllerButton button
		);

		/* gamecontroller refers to an SDL_GameController* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GameControllerClose(
			IntPtr gamecontroller
		);

		/* int refers to an SDL_JoystickID, IntPtr to an SDL_GameController*.
		 * This function is only available in 2.0.4 or higher.
		 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_GameControllerFromInstanceID(int joyid);

		#endregion

		#region SDL_haptic.h

		/* SDL_HapticEffect type */
		public const ushort SDL_HAPTIC_CONSTANT =	(1 << 0);
		public const ushort SDL_HAPTIC_SINE =		(1 << 1);
		public const ushort SDL_HAPTIC_LEFTRIGHT =	(1 << 2);
		public const ushort SDL_HAPTIC_TRIANGLE =	(1 << 3);
		public const ushort SDL_HAPTIC_SAWTOOTHUP =	(1 << 4);
		public const ushort SDL_HAPTIC_SAWTOOTHDOWN =	(1 << 5);
		public const ushort SDL_HAPTIC_SPRING =		(1 << 7);
		public const ushort SDL_HAPTIC_DAMPER =		(1 << 8);
		public const ushort SDL_HAPTIC_INERTIA =	(1 << 9);
		public const ushort SDL_HAPTIC_FRICTION =	(1 << 10);
		public const ushort SDL_HAPTIC_CUSTOM =		(1 << 11);
		public const ushort SDL_HAPTIC_GAIN =		(1 << 12);
		public const ushort SDL_HAPTIC_AUTOCENTER =	(1 << 13);
		public const ushort SDL_HAPTIC_STATUS =		(1 << 14);
		public const ushort SDL_HAPTIC_PAUSE =		(1 << 15);

		/* SDL_HapticDirection type */
		public const byte SDL_HAPTIC_POLAR =		0;
		public const byte SDL_HAPTIC_CARTESIAN =	1;
		public const byte SDL_HAPTIC_SPHERICAL =	2;

		/* SDL_HapticRunEffect */
		public const uint SDL_HAPTIC_INFINITY = 4292967295U;

		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct SDL_HapticDirection
		{
			public byte type;
			public fixed int dir[3];
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_HapticConstant
		{
			// Header
			public ushort type;
			public SDL_HapticDirection direction;
			// Replay
			public uint length;
			public ushort delay;
			// Trigger
			public ushort button;
			public ushort interval;
			// Constant
			public short level;
			// Envelope
			public ushort attack_length;
			public ushort attack_level;
			public ushort fade_length;
			public ushort fade_level;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_HapticPeriodic
		{
			// Header
			public ushort type;
			public SDL_HapticDirection direction;
			// Replay
			public uint length;
			public ushort delay;
			// Trigger
			public ushort button;
			public ushort interval;
			// Periodic
			public ushort period;
			public short magnitude;
			public short offset;
			public ushort phase;
			// Envelope
			public ushort attack_length;
			public ushort attack_level;
			public ushort fade_length;
			public ushort fade_level;
		}

		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct SDL_HapticCondition
		{
			// Header
			public ushort type;
			public SDL_HapticDirection direction;
			// Replay
			public uint length;
			public ushort delay;
			// Trigger
			public ushort button;
			public ushort interval;
			// Condition
			public fixed ushort right_sat[3];
			public fixed ushort left_sat[3];
			public fixed short right_coeff[3];
			public fixed short left_coeff[3];
			public fixed ushort deadband[3];
			public fixed short center[3];
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_HapticRamp
		{
			// Header
			public ushort type;
			public SDL_HapticDirection direction;
			// Replay
			public uint length;
			public ushort delay;
			// Trigger
			public ushort button;
			public ushort interval;
			// Ramp
			public short start;
			public short end;
			// Envelope
			public ushort attack_length;
			public ushort attack_level;
			public ushort fade_length;
			public ushort fade_level;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_HapticLeftRight
		{
			// Header
			public ushort type;
			// Replay
			public uint length;
			// Rumble
			public ushort large_magnitude;
			public ushort small_magnitude;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_HapticCustom
		{
			// Header
			public ushort type;
			public SDL_HapticDirection direction;
			// Replay
			public uint length;
			public ushort delay;
			// Trigger
			public ushort button;
			public ushort interval;
			// Custom
			public byte channels;
			public ushort period;
			public ushort samples;
			public IntPtr data; // Uint16*
			// Envelope
			public ushort attack_length;
			public ushort attack_level;
			public ushort fade_length;
			public ushort fade_level;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct SDL_HapticEffect
		{
			[FieldOffset(0)]
			public ushort type;
			[FieldOffset(0)]
			public SDL_HapticConstant constant;
			[FieldOffset(0)]
			public SDL_HapticPeriodic periodic;
			[FieldOffset(0)]
			public SDL_HapticCondition condition;
			[FieldOffset(0)]
			public SDL_HapticRamp ramp;
			[FieldOffset(0)]
			public SDL_HapticLeftRight leftright;
			[FieldOffset(0)]
			public SDL_HapticCustom custom;
		}

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_HapticClose(IntPtr haptic);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_HapticDestroyEffect(
			IntPtr haptic,
			int effect
		);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticEffectSupported(
			IntPtr haptic,
			ref SDL_HapticEffect effect
		);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticGetEffectStatus(
			IntPtr haptic,
			int effect
		);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticIndex(IntPtr haptic);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_HapticName(int device_index);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticNewEffect(
			IntPtr haptic,
			ref SDL_HapticEffect effect
		);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticNumAxes(IntPtr haptic);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticNumEffects(IntPtr haptic);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticNumEffectsPlaying(IntPtr haptic);

		/* IntPtr refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_HapticOpen(int device_index);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticOpened(int device_index);

		/* IntPtr refers to an SDL_Haptic*, joystick to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_HapticOpenFromJoystick(
			IntPtr joystick
		);

		/* IntPtr refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr SDL_HapticOpenFromMouse();

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticPause(IntPtr haptic);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_HapticQuery(IntPtr haptic);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticRumbleInit(IntPtr haptic);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticRumblePlay(
			IntPtr haptic,
			float strength,
			uint length
		);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticRumbleStop(IntPtr haptic);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticRumbleSupported(IntPtr haptic);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticRunEffect(
			IntPtr haptic,
			int effect,
			uint iterations
		);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticSetAutocenter(
			IntPtr haptic,
			int autocenter
		);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticSetGain(
			IntPtr haptic,
			int gain
		);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticStopAll(IntPtr haptic);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticStopEffect(
			IntPtr haptic,
			int effect
		);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticUnpause(IntPtr haptic);

		/* haptic refers to an SDL_Haptic* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticUpdateEffect(
			IntPtr haptic,
			int effect,
			ref SDL_HapticEffect data
		);

		/* joystick refers to an SDL_Joystick* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickIsHaptic(IntPtr joystick);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_MouseIsHaptic();

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_NumHaptics();

		#endregion

		#region SDL_audio.h

		public const ushort SDL_AUDIO_MASK_BITSIZE =	0xFF;
		public const ushort SDL_AUDIO_MASK_DATATYPE =	(1 << 8);
		public const ushort SDL_AUDIO_MASK_ENDIAN =	(1 << 12);
		public const ushort SDL_AUDIO_MASK_SIGNED =	(1 << 15);

		public static ushort SDL_AUDIO_BITSIZE(ushort x)
		{
			return (ushort) (x & SDL_AUDIO_MASK_BITSIZE);
		}

		public static bool SDL_AUDIO_ISFLOAT(ushort x)
		{
			return (x & SDL_AUDIO_MASK_DATATYPE) != 0;
		}

		public static bool SDL_AUDIO_ISBIGENDIAN(ushort x)
		{
			return (x & SDL_AUDIO_MASK_ENDIAN) != 0;
		}

		public static bool SDL_AUDIO_ISSIGNED(ushort x)
		{
			return (x & SDL_AUDIO_MASK_SIGNED) != 0;
		}

		public static bool SDL_AUDIO_ISINT(ushort x)
		{
			return (x & SDL_AUDIO_MASK_DATATYPE) == 0;
		}

		public static bool SDL_AUDIO_ISLITTLEENDIAN(ushort x)
		{
			return (x & SDL_AUDIO_MASK_ENDIAN) == 0;
		}

		public static bool SDL_AUDIO_ISUNSIGNED(ushort x)
		{
			return (x & SDL_AUDIO_MASK_SIGNED) == 0;
		}

		public const ushort AUDIO_U8 =		0x0008;
		public const ushort AUDIO_S8 =		0x8008;
		public const ushort AUDIO_U16LSB =	0x0010;
		public const ushort AUDIO_S16LSB =	0x8010;
		public const ushort AUDIO_U16MSB =	0x1010;
		public const ushort AUDIO_S16MSB =	0x9010;
		public const ushort AUDIO_U16 =		AUDIO_U16LSB;
		public const ushort AUDIO_S16 =		AUDIO_S16LSB;
		public const ushort AUDIO_S32LSB =	0x8020;
		public const ushort AUDIO_S32MSB =	0x9020;
		public const ushort AUDIO_S32 =		AUDIO_S32LSB;
		public const ushort AUDIO_F32LSB =	0x8120;
		public const ushort AUDIO_F32MSB =	0x9120;
		public const ushort AUDIO_F32 =		AUDIO_F32LSB;

		public static readonly ushort AUDIO_U16SYS =
			BitConverter.IsLittleEndian ? AUDIO_U16LSB : AUDIO_U16MSB;
		public static readonly ushort AUDIO_S16SYS =
			BitConverter.IsLittleEndian ? AUDIO_S16LSB : AUDIO_S16MSB;
		public static readonly ushort AUDIO_S32SYS =
			BitConverter.IsLittleEndian ? AUDIO_S32LSB : AUDIO_S32MSB;
		public static readonly ushort AUDIO_F32SYS =
			BitConverter.IsLittleEndian ? AUDIO_F32LSB : AUDIO_F32MSB;

		public const uint SDL_AUDIO_ALLOW_FREQUENCY_CHANGE =	0x00000001;
		public const uint SDL_AUDIO_ALLOW_FORMAT_CHANGE =	0x00000001;
		public const uint SDL_AUDIO_ALLOW_CHANNELS_CHANGE =	0x00000001;
		public const uint SDL_AUDIO_ALLOW_ANY_CHANGE = (
			SDL_AUDIO_ALLOW_FREQUENCY_CHANGE |
			SDL_AUDIO_ALLOW_FORMAT_CHANGE |
			SDL_AUDIO_ALLOW_CHANNELS_CHANGE
		);

		public const int SDL_MIX_MAXVOLUME = 128;

		public enum SDL_AudioStatus
		{
			SDL_AUDIO_STOPPED,
			SDL_AUDIO_PLAYING,
			SDL_AUDIO_PAUSED
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_AudioSpec
		{
			public int freq;
			public ushort format; // SDL_AudioFormat
			public byte channels;
			public byte silence;
			public ushort samples;
			public uint size;
			public SDL_AudioCallback callback;
			public IntPtr userdata; // void*
		}

		/* userdata refers to a void*, stream to a Uint8 */
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void SDL_AudioCallback(
			IntPtr userdata,
			IntPtr stream,
			int len
		);

		/* dev refers to an SDL_AudioDeviceID */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_AudioDeviceConnected(uint dev);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_AudioInit(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string driver_name
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_AudioQuit();

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_CloseAudio();

		/* dev refers to an SDL_AudioDeviceID */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_CloseAudioDevice(uint dev);

		/* audio_buf refers to a malloc()'d buffer from SDL_LoadWAV */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_FreeWAV(IntPtr audio_buf);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GetAudioDeviceName(
			int index,
			int iscapture
		);

		/* dev refers to an SDL_AudioDeviceID */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_AudioStatus SDL_GetAudioDeviceStatus(
			uint dev
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GetAudioDriver(int index);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_AudioStatus SDL_GetAudioStatus();

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
		public static extern string SDL_GetCurrentAudioDriver();

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetNumAudioDevices(int iscapture);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetNumAudioDrivers();

		/* audio_buf will refer to a malloc()'d byte buffer */
		/* THIS IS AN RWops FUNCTION! */
		[DllImport(nativeLibName, EntryPoint = "SDL_LoadWAV_RW", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr INTERNAL_SDL_LoadWAV_RW(
			IntPtr src,
			int freesrc,
			ref SDL_AudioSpec spec,
			out IntPtr audio_buf,
			out uint audio_len
		);
		public static SDL_AudioSpec SDL_LoadWAV(
			string file,
			ref SDL_AudioSpec spec,
			out IntPtr audio_buf,
			out uint audio_len
		) {
			SDL_AudioSpec result;
			IntPtr rwops = INTERNAL_SDL_RWFromFile(file, "rb");
			IntPtr result_ptr = INTERNAL_SDL_LoadWAV_RW(
				rwops,
				1,
				ref spec,
				out audio_buf,
				out audio_len
			);
			result = (SDL_AudioSpec) Marshal.PtrToStructure(
				result_ptr,
				typeof(SDL_AudioSpec)
			);
			return result;
		}

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_LockAudio();

		/* dev refers to an SDL_AudioDeviceID */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_LockAudioDevice(uint dev);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_MixAudio(
			[Out()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 2)]
				byte[] dst,
			[In()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 2)]
				byte[] src,
			uint len,
			int volume
		);

		/* format refers to an SDL_AudioFormat */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_MixAudioFormat(
			[Out()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 3)]
				byte[] dst,
			[In()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 3)]
				byte[] src,
			ushort format,
			uint len,
			int volume
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_OpenAudio(
			ref SDL_AudioSpec desired,
			out SDL_AudioSpec obtained
		);

		/* uint refers to an SDL_AudioDeviceID */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_OpenAudioDevice(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
				string device,
			int iscapture,
			ref SDL_AudioSpec desired,
			out SDL_AudioSpec obtained,
			int allowed_changes
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_PauseAudio(int pause_on);

		/* dev refers to an SDL_AudioDeviceID */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_PauseAudioDevice(
			uint dev,
			int pause_on
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_UnlockAudio();

		/* dev refers to an SDL_AudioDeviceID */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_UnlockAudioDevice(uint dev);

		/* dev refers to an SDL_AudioDeviceID, data to a void* */
		/* Only available in 2.0.4 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_QueueAudio(
			uint dev,
			IntPtr data,
			UInt32 len
		);

		/* dev refers to an SDL_AudioDeviceID */
		/* Only available in 2.0.4 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern UInt32 SDL_GetQueuedAudioSize(uint dev);

		/* dev refers to an SDL_AudioDeviceID */
		/* Only available in 2.0.4 */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_ClearQueuedAudio(uint dev);

		#endregion

		#region SDL_timer.h

		/* System timers rely on different OS mechanisms depending on
		 * which operating system SDL2 is compiled against.
		 */

		/* Compare tick values, return true if A has passed B. Introduced in SDL 2.0.1,
		 * but does not require it (it was a macro).
		 */
		public static bool SDL_TICKS_PASSED(UInt32 A, UInt32 B)
		{
			return ((Int32)(B - A) <= 0);
		}

		/* Delays the thread's processing based on the milliseconds parameter */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_Delay(UInt32 ms);

		/* Returns the milliseconds that have passed since SDL was initialized */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern UInt32 SDL_GetTicks();

		/* Get the current value of the high resolution counter */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern UInt64 SDL_GetPerformanceCounter();

		/* Get the count per second of the high resolution counter */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern UInt64 SDL_GetPerformanceFrequency();

		/* param refers to a void* */
		public delegate UInt32 SDL_TimerCallback(UInt32 interval, IntPtr param);

		/* int refers to an SDL_TimerID, param to a void* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_AddTimer(
			UInt32 interval,
			SDL_TimerCallback callback,
			IntPtr param
		);

		/* id refers to an SDL_TimerID */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_RemoveTimer(int id);

		#endregion

		#region SDL_syswm.h

		public enum SDL_SYSWM_TYPE
		{
			SDL_SYSWM_UNKNOWN,
			SDL_SYSWM_WINDOWS,
			SDL_SYSWM_X11,
			SDL_SYSWM_DIRECTFB,
			SDL_SYSWM_COCOA,
			SDL_SYSWM_UIKIT,
			SDL_SYSWM_WAYLAND,
			SDL_SYSWM_MIR,
			SDL_SYSWM_WINRT,
			SDL_SYSWM_ANDROID
		}

		// FIXME: I wish these weren't public...
		[StructLayout(LayoutKind.Sequential)]
		public struct INTERNAL_windows_wminfo
		{
			public IntPtr window; // Refers to an HWND
			public IntPtr hdc; // Refers to an HDC
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct INTERNAL_winrt_wminfo
		{
			public IntPtr window; // Refers to an IInspectable*
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct INTERNAL_x11_wminfo
		{
			public IntPtr display; // Refers to a Display*
			public IntPtr window; // Refers to a Window (XID, use ToInt64!)
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct INTERNAL_directfb_wminfo
		{
			public IntPtr dfb; // Refers to an IDirectFB*
			public IntPtr window; // Refers to an IDirectFBWindow*
			public IntPtr surface; // Refers to an IDirectFBSurface*
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct INTERNAL_cocoa_wminfo
		{
			public IntPtr window; // Refers to an NSWindow*
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct INTERNAL_uikit_wminfo
		{
			public IntPtr window; // Refers to a UIWindow*
			public uint framebuffer;
			public uint colorbuffer;
			public uint resolveFramebuffer;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct INTERNAL_wayland_wminfo
		{
			public IntPtr display; // Refers to a wl_display*
			public IntPtr surface; // Refers to a wl_surface*
			public IntPtr shell_surface; // Refers to a wl_shell_surface*
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct INTERNAL_mir_wminfo
		{
			public IntPtr connection; // Refers to a MirConnection*
			public IntPtr surface; // Refers to a MirSurface*
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct INTERNAL_android_wminfo
		{
			public IntPtr window; // Refers to an ANativeWindow
			public IntPtr surface; // Refers to an EGLSurface
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct INTERNAL_SysWMDriverUnion
		{
			[FieldOffset(0)]
			public INTERNAL_windows_wminfo win;
			[FieldOffset(0)]
			public INTERNAL_winrt_wminfo winrt;
			[FieldOffset(0)]
			public INTERNAL_x11_wminfo x11;
			[FieldOffset(0)]
			public INTERNAL_directfb_wminfo dfb;
			[FieldOffset(0)]
			public INTERNAL_cocoa_wminfo cocoa;
			[FieldOffset(0)]
			public INTERNAL_uikit_wminfo uikit;
			[FieldOffset(0)]
			public INTERNAL_wayland_wminfo wl;
			[FieldOffset(0)]
			public INTERNAL_mir_wminfo mir;
			[FieldOffset(0)]
			public INTERNAL_android_wminfo android;
			// private int dummy;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_SysWMinfo
		{
			public SDL_version version;
			public SDL_SYSWM_TYPE subsystem;
			public INTERNAL_SysWMDriverUnion info;
		}

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_GetWindowWMInfo(
			IntPtr window,
			ref SDL_SysWMinfo info
		);

		#endregion

		#region SDL_filesystem.h

		/// <summary>
		/// Get the path where the application resides.
		///
		/// Get the "base path". This is the directory where the application was run
		/// from, which is probably the installation directory, and may or may not
		/// be the process's current working directory.
		///
		/// This returns an absolute path in UTF-8 encoding, and is garunteed to
		/// end with a path separator ('\\' on Windows, '/' most other places).
		/// </summary>
		/// <returns>string of base dir in UTF-8 encoding</returns>
		/// <remarks>The underlying C string is owned by the application,
		/// and can be NULL on some platforms.
		///
		/// This function is not necessarily fast, so you should only
		/// call it once and save the string if you need it.
		///
		/// This function is only available in SDL 2.0.1 and later.</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
		public static extern string SDL_GetBasePath();

		/// <summary>
		/// Get the user-and-app-specific path where files can be written.
		///
		/// Get the "pref dir". This is meant to be where users can write personal
		/// files (preferences and save games, etc) that are specific to your
		/// application. This directory is unique per user, per application.
		///
		/// This function will decide the appropriate location in the native filesystem¸
		/// create the directory if necessary, and return a string of the absolute
		/// path to the directory in UTF-8 encoding.
		/// </summary>
		/// <param name="org">The name of your organization.</param>
		/// <param name="app">The name of your application.</param>
		/// <returns>UTF-8 string of user dir in platform-dependent notation. NULL
		/// if there's a problem (creating directory failed, etc).</returns>
		/// <remarks>The underlying C string is owned by the application,
		/// and can be NULL on some platforms. .NET provides some similar functions.
		///
		/// This function is not necessarily fast, so you should only
		/// call it once and save the string if you need it.
		///
		/// This function is only available in SDL 2.0.1 and later.</remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		[return : MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
		public static extern string SDL_GetPrefPath(
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
			string org,
			[In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
			string app
		);

		#endregion

		#region SDL_power.h

		/// <summary>
		/// The basic state for the system's power supply.
		/// </summary>
		public enum SDL_PowerState
		{
			SDL_POWERSTATE_UNKNOWN = 0,
			SDL_POWERSTATE_ON_BATTERY,
			SDL_POWERSTATE_NO_BATTERY,
			SDL_POWERSTATE_CHARGING,
			SDL_POWERSTATE_CHARGED
		}

		/// <summary>
		/// Get the current power supply details.
		/// </summary>
		/// <param name="secs">Seconds of battery life left. You can pass a NULL here if
		/// you don't care. Will return -1 if we can't determine a
		/// value, or we're not running on a battery.</param>
		/// <param name="pct">Percentage of battery life left, between 0 and 100. You can
		/// pass a NULL here if you don't care. Will return -1 if we
		/// can't determine a value, or we're not running on a battery.</param>
		/// <returns>The state of the battery (if any).</returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_PowerState SDL_GetPowerInfo(
			out int secs,
			out int pct
		);

		#endregion

		#region SDL_cpuinfo.h

		/// <summary>
		/// This function returns the number of CPU cores available.
		/// </summary>
		/// <returns>The number of CPU cores available.</returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetCPUCount();

		/// <summary>
		/// This function returns the amount of RAM configured in the system, in MB.
		/// </summary>
		/// <returns>The amount of RAM configured in the system, in MB.</returns>
		/// <remarks>
		/// This function is only available in SDL 2.0.1 and later.
		/// </remarks>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetSystemRAM();

		#endregion
	}
}
