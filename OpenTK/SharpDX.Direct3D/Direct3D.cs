// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace SharpDX.Direct3D9 {
	
	public unsafe class Direct3D : ComObject {
		
		private delegate int DXGetAdapterCount(IntPtr comPointer);
		private DXGetAdapterCount GetAdapterCountFunc;
		
		private delegate int DXGetAdapterIdentifier(IntPtr comPointer, int adapter, int flags, IntPtr identifier);
		private DXGetAdapterIdentifier GetAdapterIdentifierFunc;
		
		private delegate int DXGetAdapterModeCount(IntPtr comPointer, int adapter, int format);
		private DXGetAdapterModeCount GetAdapterModeCountFunc;
		
		private delegate int DXEnumAdapterModes(IntPtr comPointer, int adapter, int format, int mode, IntPtr modeRef);
		private DXEnumAdapterModes EnumAdapterModesFunc;
		
		private delegate int DXGetAdapterDisplayMode(IntPtr comPointer, int adapter, IntPtr modeRef);
		private DXGetAdapterDisplayMode GetAdapterDisplayModeFunc;
		
		private delegate int DXCheckDeviceType(IntPtr comPointer, int adapter, int deviceType, int adapterFormat,
		                                       int backBufferFormat, int windowed);
		private DXCheckDeviceType CheckDeviceTypeFunc;
		
		private delegate int DXCheckDepthStencilMatch(IntPtr comPointer, int adapter, int deviceType, int adapterFormat,
		                                              int renderTargetFormat, int depthStencilFormat);
		private DXCheckDepthStencilMatch CheckDepthStencilMatchFunc;
		
		private delegate int DXGetDeviceCaps(IntPtr comPionter, int adapter, int deviceType, IntPtr capsRef);
		private DXGetDeviceCaps GetDeviceCapsFunc;
		
		private delegate IntPtr DXGetAdapterMonitor(IntPtr comPointer, int adapter);
		private DXGetAdapterMonitor GetAdapterMonitorFunc;
		
		private delegate int DXCreateDevice(IntPtr comPointer, int adapter, int deviceType, IntPtr hFocusWindow,
		                                    int behaviorFlags, IntPtr presentParams, IntPtr devicePtr);
		private DXCreateDevice CreateDeviceFunc;
		
		private void GetFuncPointers(IntPtr comPtr) {
			GetAdapterCountFunc = (DXGetAdapterCount) GetFunc(comPtr, 4, typeof(DXGetAdapterCount));
			GetAdapterIdentifierFunc = (DXGetAdapterIdentifier) GetFunc(comPtr, 5, typeof(DXGetAdapterIdentifier));
			GetAdapterModeCountFunc = (DXGetAdapterModeCount) GetFunc(comPtr, 6, typeof(DXGetAdapterModeCount));
			EnumAdapterModesFunc = (DXEnumAdapterModes) GetFunc(comPtr, 7, typeof(DXEnumAdapterModes));
			GetAdapterDisplayModeFunc = (DXGetAdapterDisplayMode) GetFunc(comPtr, 8, typeof(DXGetAdapterDisplayMode));
			CheckDeviceTypeFunc = (DXCheckDeviceType) GetFunc(comPtr, 9, typeof(DXCheckDeviceType));
			CheckDepthStencilMatchFunc = (DXCheckDepthStencilMatch) GetFunc(comPtr, 12, typeof(DXCheckDepthStencilMatch));
			GetDeviceCapsFunc = (DXGetDeviceCaps) GetFunc(comPtr, 14, typeof(DXGetDeviceCaps));
			GetAdapterMonitorFunc = (DXGetAdapterMonitor) GetFunc(comPtr, 15, typeof(DXGetAdapterMonitor));
			CreateDeviceFunc = (DXCreateDevice) GetFunc(comPtr, 16, typeof(DXCreateDevice));
		}
		
		public Direct3D() {
			comPointer = Direct3DCreate9( SdkVersion );
			
			GetFuncPointers(comPointer);
			
			int count = GetAdapterCount();
			Adapters = new AdapterInformation[count];
			for( int i = 0; i < count; i++ ) {
				Adapters[i] = new AdapterInformation( this, i );
			}
		}

		public AdapterInformation[] Adapters;
		
		const int SdkVersion = 32;
		[DllImport( "d3d9.dll" )]
		static extern IntPtr Direct3DCreate9( int sdkVersion );
		
		public int GetAdapterCount() {
			return GetAdapterCountFunc(comPointer);
		}
		
		public AdapterDetails GetAdapterIdentifier( int adapter ) {
			AdapterDetails.Native identifierNative = new AdapterDetails.Native();
			int res = GetAdapterIdentifierFunc(comPointer, adapter, 0, (IntPtr)(void*)&identifierNative);
			if( res < 0 ) { throw new SharpDXException( res ); }
			
			AdapterDetails identifier = new AdapterDetails();
			identifier.MarshalFrom(ref identifierNative);			
			return identifier;
		}
		
		public bool CheckDeviceType(int adapter, DeviceType devType, Format adapterFormat, Format backBufferFormat, bool bWindowed) {
			return CheckDeviceTypeFunc(comPointer, adapter, (int)devType, (int)adapterFormat,
			                           (int)backBufferFormat, bWindowed ? 1 : 0) == 0;
		}
		
		public bool CheckDepthStencilMatch(int adapter, DeviceType deviceType, Format adapterFormat, Format renderTargetFormat, Format depthStencilFormat) {
			return CheckDepthStencilMatchFunc(comPointer, adapter, (int)deviceType, (int)adapterFormat, 
			                                  (int)renderTargetFormat, (int)depthStencilFormat) == 0;
		}
		
		public Device CreateDevice(int adapter, DeviceType deviceType, IntPtr hFocusWindow, CreateFlags behaviorFlags,  PresentParameters presentParams) {
			IntPtr devicePtr = IntPtr.Zero;
			int res = CreateDeviceFunc(comPointer, adapter, (int)deviceType, hFocusWindow, (int)behaviorFlags,
			                           (IntPtr)(void*)&presentParams, (IntPtr)(void*)&devicePtr);
			
			if( res < 0 ) { throw new SharpDXException( res ); }
			return new Device( devicePtr );
		}
	}
}
