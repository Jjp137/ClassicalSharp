// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Platform;
using Clipboard = System.Windows.Forms.Clipboard;

namespace ClassicalSharp {
	
	/// <summary> Abstracts away a platform specific window, and input handler mechanism. </summary>
	public interface IPlatformWindow {
		
		int Width { get; }
		
		int Height { get; }
		
		Size ClientSize { get; }
		
		bool VSync { get; set; }
		
		bool Exists { get; }
		
		bool Focused { get; }
		
		bool CursorVisible { get; set; }
		
		Point DesktopCursorPos { get; set; }
		
		MouseDevice Mouse { get; }
		
		KeyboardDevice Keyboard { get; }
		
		Icon Icon { get; set; }
		
		Point PointToScreen( Point coords );
		
		WindowState WindowState { get; set; }
		
		IWindowInfo WindowInfo { get; }
		
		string ClipboardText { get; set; }
		
		void Run();
		
		void SwapBuffers();
		
		void Exit();
		
		event EventHandler<KeyPressEventArgs> KeyPress;
	}
}
