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
using OpenTK;

namespace SharpDX.Direct3D9 {

	public unsafe class Resource : ComObject {
		
		private delegate int DXSetPriority(IntPtr comPointer, int priorityNew);
		private DXSetPriority SetPriorityFunc;
		
		private delegate int DXGetPriority(IntPtr comPointer);
		private DXGetPriority GetPriorityFunc;
		
		private delegate void DXPreLoad(IntPtr comPointer);
		private DXPreLoad PreLoadFunc;
		
		public Resource( IntPtr nativePtr ) : base( nativePtr ) {
			SetPriorityFunc = (DXSetPriority) GetFunc(nativePtr, 7, typeof(DXSetPriority));
			GetPriorityFunc = (DXGetPriority) GetFunc(nativePtr, 8, typeof(DXGetPriority));
			PreLoadFunc = (DXPreLoad) GetFunc(nativePtr, 9, typeof(DXPreLoad));
		}

		public int SetPriority(int priorityNew) {
			return SetPriorityFunc(comPointer, priorityNew);
		}
		
		public int GetPriority() {
			return GetPriorityFunc(comPointer);
		}
		
		public void PreLoad() {
			PreLoadFunc(comPointer);
		}
	}
	
	public unsafe class DataBuffer : Resource { // Either 'VertexBuffer' or 'IndexBuffer
		
		private delegate int DXLock(IntPtr comPointer, int offsetToLock, int sizeToLock, IntPtr pOut, int flags);
		private DXLock LockFunc;
		
		private delegate int DXUnlock(IntPtr comPointer);
		private DXUnlock UnlockFunc;
		
		public DataBuffer(IntPtr nativePtr) : base(nativePtr) {
			LockFunc = (DXLock) GetFunc(nativePtr, 11, typeof(DXLock));
			UnlockFunc = (DXUnlock) GetFunc(nativePtr, 12, typeof(DXUnlock));
		}
		
		public IntPtr Lock( int offsetToLock, int sizeToLock, LockFlags flags ) {
			IntPtr pOut;
			int res = LockFunc(comPointer, offsetToLock, sizeToLock, (IntPtr)(void*)&pOut, (int)flags);
			if( res < 0 ) { throw new SharpDXException( res ); }
			return pOut;
		}
		
		public void SetData( IntPtr data, int bytes, LockFlags flags ) {
			IntPtr dst = Lock( 0, bytes, flags );
			MemUtils.memcpy( data, dst, bytes );
			Unlock();
		}
		
		public void SetData<T>( T[] data, int bytes, LockFlags flags ) where T : struct {
			throw new NotImplementedException();
		}
		
		public void Unlock() {
			int res = UnlockFunc(comPointer);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}
	}
	
	public unsafe class Surface : Resource {
		
		private delegate int DXLockRectangle(IntPtr comPointer, IntPtr lockedRect, IntPtr rect, int flags);
		private DXLockRectangle LockRectangleFunc;
		
		private delegate int DXUnlockRectangle(IntPtr comPointer);
		private DXUnlockRectangle UnlockRectangleFunc;
		
		public Surface(IntPtr nativePtr) : base(nativePtr) {
			LockRectangleFunc = (DXLockRectangle) GetFunc(nativePtr, 13, typeof(DXLockRectangle));
			UnlockRectangleFunc = (DXUnlockRectangle) GetFunc(nativePtr, 14, typeof(DXUnlockRectangle));
		}
		
		public LockedRectangle LockRectangle(LockFlags flags) {
			LockedRectangle lockedRect = new LockedRectangle();
			int res = LockRectangleFunc(comPointer, (IntPtr)(void*)&lockedRect, IntPtr.Zero, (int)flags);
			if( res < 0 ) { throw new SharpDXException( res ); }
			return lockedRect;
		}
		
		public void UnlockRectangle() {
			int res = UnlockRectangleFunc(comPointer);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}
	}
	
	public unsafe class Texture : Resource {
		
		private delegate int DXLockRectangle(IntPtr comPointer, int level, IntPtr lockedRect, IntPtr rect, int flags);
		private DXLockRectangle LockRectangleFunc;
		
		private delegate int DXUnlockRectangle(IntPtr comPointer, int level);
		private DXUnlockRectangle UnlockRectangleFunc;
		
		public Texture(IntPtr nativePtr) : base(nativePtr) {
			LockRectangleFunc = (DXLockRectangle) GetFunc(nativePtr, 19, typeof(DXLockRectangle));
			UnlockRectangleFunc = (DXUnlockRectangle) GetFunc(nativePtr, 20, typeof(DXUnlockRectangle));
		}

		public LockedRectangle LockRectangle(int level, LockFlags flags) {
			LockedRectangle lockedRect = new LockedRectangle();
			int res = LockRectangleFunc(comPointer, level, (IntPtr)(void*)&lockedRect, IntPtr.Zero, (int)flags);
			if( res < 0 ) { throw new SharpDXException( res ); }
			return lockedRect;
		}
		
		public LockedRectangle LockRectangle(int level, D3DRect rect, LockFlags flags) {
			LockedRectangle lockedRect = new LockedRectangle();
			int res = LockRectangleFunc(comPointer, level, (IntPtr)(void*)&lockedRect, (IntPtr)(void*)&rect, (int)flags);
			if( res < 0 ) { throw new SharpDXException( res ); }
			return lockedRect;
		}
		
		public void SetData( int level, LockFlags flags, IntPtr data, int bytes ) {
			LockedRectangle rect = LockRectangle( level, flags );
			MemUtils.memcpy( data, rect.DataPointer, bytes );
			UnlockRectangle( level );
		}
		
		public void SetPartData( int level, LockFlags flags, IntPtr data, int x, int y, int width, int height ) {
			D3DRect partRect;
			partRect.Left = x; partRect.Top = y;
			partRect.Right = x + width; partRect.Bottom = y + height;
			LockedRectangle rect = LockRectangle( level, partRect, flags );
			
			// We need to copy scanline by scanline, as generally rect.stride != data.stride
			byte* src = (byte*)data, dst = (byte*)rect.DataPointer;
			for( int yy = 0; yy < height; yy++ ) {
				MemUtils.memcpy( (IntPtr)src, (IntPtr)dst, width * 4 );
				src += width * 4;
				dst += rect.Pitch;			
			}
			UnlockRectangle( level );
		}
		
		public void UnlockRectangle(int level) {
			int res = UnlockRectangleFunc(comPointer, level);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}
	}
}
