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

namespace SharpDX.Direct3D9
{
	public unsafe class Device : ComObject
	{
		// FIXME: all of this assumes that the comPointer field will never change
		private delegate int DXTestCooperativeLevel(IntPtr comPointer);
		private DXTestCooperativeLevel TestCooperativeLevelFunc;
		
		private delegate int DXAvailableTextureMemory(IntPtr comPointer);
		private DXAvailableTextureMemory AvailableTextureMemoryFunc;
		
		private delegate int DXEvictManagedResources(IntPtr comPointer);
		private DXEvictManagedResources EvictManagedResourcesFunc;
		
		private delegate int DXCapabilities(IntPtr comPointer, IntPtr caps);
		private DXCapabilities CapabilitiesFunc;
		
		private delegate int DXGetDisplayMode(IntPtr comPointer, int iSwapChain, IntPtr modeOut);
		private DXGetDisplayMode GetDisplayModeFunc;
		
		private delegate int DXReset(IntPtr comPointer, IntPtr presentParameters);
		private DXReset ResetFunc;
		
		private delegate int DXPresent(IntPtr comPointer, IntPtr sourceRect, IntPtr destRect,IntPtr destWindowOverride,
		                               IntPtr dirtyRegion);
		private DXPresent PresentFunc;
		
		private delegate int DXGetBackBuffer(IntPtr comPointer, int iSwapChain, int iBackBuffer, int type, IntPtr backBufferOut);
		private DXGetBackBuffer GetBackBufferFunc;
		
		private delegate int DXCreateTexture(IntPtr comPointer, int width, int height, int levels, int usage,
		                                     int format, int pool, IntPtr textureOut, IntPtr sharedHandle);
		private DXCreateTexture CreateTextureFunc;
		
		private delegate int DXCreateVertexBuffer(IntPtr comPointer, int length, int usage, int vertexFormat,
		                                          int pool, IntPtr vertexBufferOut, IntPtr sharedHandle);
		private DXCreateVertexBuffer CreateVertexBufferFunc;
		
		private delegate int DXCreateIndexBuffer(IntPtr comPointer, int length, int usage, int format, int pool,
		                                         IntPtr indexBufferOut, IntPtr sharedHandle);
		private DXCreateIndexBuffer CreateIndexBufferFunc;
		
		private delegate int DXUpdateTexture(IntPtr comPointer, IntPtr srcTex, IntPtr dstTex);
		private DXUpdateTexture UpdateTextureFunc;
		
		private delegate int DXGetRenderTargetData(IntPtr comPointer, IntPtr renderTarget, IntPtr destSurface);
		private DXGetRenderTargetData GetRenderTargetDataFunc;
		
		private delegate int DXCreateOffscreenPlainSurface(IntPtr comPointer, int width, int height, int format,
		                                                   int pool, IntPtr surfaceOut, IntPtr sharedHandle);
		private DXCreateOffscreenPlainSurface CreateOffscreenPlainSurfaceFunc;
		
		private delegate int DXBeginScene(IntPtr comPointer);
		private DXBeginScene BeginSceneFunc;
		
		private delegate int DXEndScene(IntPtr comPointer);
		private DXEndScene EndSceneFunc;
		
		private delegate int DXClear(IntPtr comPointer, int count, IntPtr rects, int flags, int colorBGRA, float z, int stencil);
		private DXClear ClearFunc;
		
		private delegate int DXSetTransform(IntPtr comPointer, int state, IntPtr matrix);
		private DXSetTransform SetTransformFunc;
		
		private delegate int DXSetRenderState(IntPtr comPointer, int state, int value);
		private DXSetRenderState SetRenderStateFunc;
		
		private delegate int DXSetTexture(IntPtr comPointer, int stage, IntPtr texture);
		private DXSetTexture SetTextureFunc;
		
		private delegate int DXSetTextureStageState(IntPtr comPointer, int stage, int type, int value);
		private DXSetTextureStageState SetTextureStageStateFunc;
		
		private delegate int DXDrawPrimitives(IntPtr comPointer, int type, int startVertex, int primitiveCount);
		private DXDrawPrimitives DrawPrimitivesFunc;
		
		private delegate int DXDrawIndexedPrimitives(IntPtr comPointer, int type, int baseVertexIndex, int minVertexIndex,
		                                             int numVertices, int startIndex, int primCount);
		private DXDrawIndexedPrimitives DrawIndexedPrimitivesFunc;
		
		private delegate int DXSetVertexFormat(IntPtr comPointer, int vertexFormat);
		private DXSetVertexFormat SetVertexFormatFunc;
		
		private delegate int DXSetStreamSource(IntPtr comPointer, int streamNumber, IntPtr streamData, int offsetInBytes, int stride);
		private DXSetStreamSource SetStreamSourceFunc;
		
		private delegate int DXSetIndices(IntPtr comPointer, IntPtr indexData);
		private DXSetIndices SetIndicesFunc;
		
		private void GetFuncPointers(IntPtr comPtr) {
			TestCooperativeLevelFunc = (DXTestCooperativeLevel) GetFunc(comPtr, 3, typeof(DXTestCooperativeLevel));
			AvailableTextureMemoryFunc = (DXAvailableTextureMemory) GetFunc(comPtr, 4, typeof(DXAvailableTextureMemory));
			EvictManagedResourcesFunc = (DXEvictManagedResources) GetFunc(comPtr, 5, typeof(DXEvictManagedResources));
			CapabilitiesFunc = (DXCapabilities) GetFunc(comPtr, 7, typeof(DXCapabilities));
			GetDisplayModeFunc = (DXGetDisplayMode) GetFunc(comPtr, 8, typeof(DXGetDisplayMode));
			ResetFunc = (DXReset) GetFunc(comPtr, 16, typeof(DXReset));
			PresentFunc = (DXPresent) GetFunc(comPtr, 17, typeof(DXPresent));
			GetBackBufferFunc = (DXGetBackBuffer) GetFunc(comPtr, 18, typeof(DXGetBackBuffer));
			CreateTextureFunc = (DXCreateTexture) GetFunc(comPtr, 23, typeof(DXCreateTexture));
			CreateVertexBufferFunc = (DXCreateVertexBuffer) GetFunc(comPtr, 26, typeof(DXCreateVertexBuffer));
			CreateIndexBufferFunc = (DXCreateIndexBuffer) GetFunc(comPtr, 27, typeof(DXCreateIndexBuffer));
			UpdateTextureFunc = (DXUpdateTexture) GetFunc(comPtr, 31, typeof(DXUpdateTexture));
			GetRenderTargetDataFunc = (DXGetRenderTargetData) GetFunc(comPtr, 32, typeof(DXGetRenderTargetData));
			CreateOffscreenPlainSurfaceFunc = (DXCreateOffscreenPlainSurface) GetFunc(comPtr, 36, typeof(DXCreateOffscreenPlainSurface));
			BeginSceneFunc = (DXBeginScene) GetFunc(comPtr, 41, typeof(DXBeginScene));
			EndSceneFunc = (DXEndScene) GetFunc(comPtr, 42, typeof(DXEndScene));
			ClearFunc = (DXClear) GetFunc(comPtr, 43, typeof(DXClear));
			SetTransformFunc = (DXSetTransform) GetFunc(comPtr, 44, typeof(DXSetTransform));
			SetRenderStateFunc = (DXSetRenderState) GetFunc(comPtr, 57, typeof(DXSetRenderState));
			SetTextureFunc = (DXSetTexture) GetFunc(comPtr, 65, typeof(DXSetTexture));
			SetTextureStageStateFunc = (DXSetTextureStageState) GetFunc(comPtr, 67, typeof(DXSetTextureStageState));
			DrawPrimitivesFunc = (DXDrawPrimitives) GetFunc(comPtr, 81, typeof(DXDrawPrimitives));
			DrawIndexedPrimitivesFunc = (DXDrawIndexedPrimitives) GetFunc(comPtr, 82, typeof(DXDrawIndexedPrimitives));
			SetVertexFormatFunc = (DXSetVertexFormat) GetFunc(comPtr, 89, typeof(DXSetVertexFormat));
			SetStreamSourceFunc = (DXSetStreamSource) GetFunc(comPtr, 100, typeof(DXSetStreamSource));
			SetIndicesFunc = (DXSetIndices) GetFunc(comPtr, 104, typeof(DXSetIndices));
		}
		
		public Device(IntPtr nativePtr) : base(nativePtr) {
			GetFuncPointers(nativePtr);
		}

		public int TestCooperativeLevel() {
			return TestCooperativeLevelFunc(comPointer);
		}
		
		public uint AvailableTextureMemory {
			get { return (uint)AvailableTextureMemoryFunc(comPointer); }
		}
		
		public void EvictManagedResources() {
			int res = EvictManagedResourcesFunc(comPointer);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}
		
		public Capabilities Capabilities {
			get {
				Capabilities caps = new Capabilities();
				int res = CapabilitiesFunc(comPointer, (IntPtr)(void*)&caps);
				if( res < 0 ) { throw new SharpDXException( res ); }
				return caps;
			}
		}
		
		public DisplayMode GetDisplayMode(int iSwapChain) {
			DisplayMode modeRef = new DisplayMode();
			int res = GetDisplayModeFunc(comPointer, iSwapChain, (IntPtr)(void*)&modeRef);
			if( res < 0 ) { throw new SharpDXException( res ); }
			return modeRef;
		}
		
		public int Reset( PresentParameters presentParams ) {
			return ResetFunc(comPointer, (IntPtr)(void*)&presentParams);
		}
		
		public int Present() {
			return PresentFunc(comPointer, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
		}
		
		public Surface GetBackBuffer(int iSwapChain, int iBackBuffer, BackBufferType type) {
			IntPtr backBufferOut = IntPtr.Zero;
			int res = GetBackBufferFunc(comPointer, iSwapChain, iBackBuffer, (int)type, (IntPtr)(void*)&backBufferOut);
			if( res < 0 ) { throw new SharpDXException( res ); }
			return ( backBufferOut == IntPtr.Zero ) ? null : new Surface( backBufferOut );
		}
		
		public Texture CreateTexture(int width, int height, int levels, Usage usage, Format format, Pool pool) {
			IntPtr pOut = IntPtr.Zero;
			int res = CreateTextureFunc(comPointer, width, height, levels, (int)usage, (int)format, (int)pool,
			                                    (IntPtr)(void*)&pOut, IntPtr.Zero);
			if( res < 0 ) { throw new SharpDXException( res ); }
			return new Texture( pOut );
		}
		
		public DataBuffer CreateVertexBuffer(int length, Usage usage, VertexFormat vertexFormat, Pool pool) {
			IntPtr pOut = IntPtr.Zero;
			int res = CreateVertexBufferFunc(comPointer, length, (int)usage, (int)vertexFormat, (int)pool,
			                                         (IntPtr)(void*)&pOut, IntPtr.Zero);
			if( res < 0 ) { throw new SharpDXException( res ); }
			return new DataBuffer( pOut );
		}
		
		public DataBuffer CreateIndexBuffer(int length, Usage usage, Format format, Pool pool) {
			IntPtr pOut = IntPtr.Zero;
			int res = CreateIndexBufferFunc(comPointer, length, (int)usage, (int)format, (int)pool,
			                                        (IntPtr)(void*)&pOut, IntPtr.Zero);
			if( res < 0 ) { throw new SharpDXException( res ); }
			return new DataBuffer( pOut );
		}
		
		public void UpdateTexture(Texture srcTex, Texture dstTex) {
			int res = UpdateTextureFunc(comPointer, srcTex.comPointer, dstTex.comPointer);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}
		
		public void GetRenderTargetData(Surface renderTarget, Surface destSurface) {
			int res = GetRenderTargetDataFunc(comPointer, renderTarget.comPointer, destSurface.comPointer);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}
		
		public Surface CreateOffscreenPlainSurface(int width, int height, Format format, Pool pool) {
			IntPtr pOut = IntPtr.Zero;
			int res = CreateOffscreenPlainSurfaceFunc(comPointer, width, height, (int)format, (int)pool,
			                                                  (IntPtr)(void*)&pOut, IntPtr.Zero);
			if( res < 0 ) { throw new SharpDXException( res ); }
			return new Surface(pOut);
		}

		public void BeginScene() {
			int res = BeginSceneFunc(comPointer);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}

		public void EndScene() {
			int res = EndSceneFunc(comPointer);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}
		
		public void Clear(ClearFlags flags, int colorBGRA, float z, int stencil) {
			int res = ClearFunc(comPointer, 0, IntPtr.Zero, (int)flags, colorBGRA, z, stencil);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}

		public void SetTransform(TransformState state, ref Matrix4 matrixRef) {
			int res;
			fixed (void* matrixRef_ = &matrixRef)
				res = SetTransformFunc(comPointer, (int)state, (IntPtr)matrixRef_);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}
		
		public void SetRenderState(RenderState renderState, bool enable) {
			SetRenderState(renderState, enable ? 1 : 0);
		}

		public void SetRenderState(RenderState renderState, float value) {
			SetRenderState(renderState, *(int*)&value);
		}

		public void SetRenderState(RenderState state, int value) {
			int res = SetRenderStateFunc(comPointer, (int)state, value);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}
		
		public void SetTexture(int stage, Texture texture) {
			int res = SetTextureFunc(comPointer, stage, (texture == null) ? IntPtr.Zero : texture.comPointer);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}
		
		public void SetTextureStageState(int stage, TextureStage type, int value) {
			int res = SetTextureStageStateFunc(comPointer, stage, (int)type, value);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}
		
		public void DrawPrimitives(PrimitiveType type, int startVertex, int primitiveCount) {
			int res = DrawPrimitivesFunc(comPointer, (int)type, startVertex, primitiveCount);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}
		
		public void DrawIndexedPrimitives(PrimitiveType type, int baseVertexIndex, int minVertexIndex, int numVertices, int startIndex, int primCount) {
			int res = DrawIndexedPrimitivesFunc(comPointer, (int)type, baseVertexIndex, minVertexIndex, numVertices,
			                                      startIndex, primCount);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}

		public void SetVertexFormat(VertexFormat vertexFormat) {
			int res = SetVertexFormatFunc(comPointer, (int)vertexFormat);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}
		
		public void SetStreamSource(int streamNumber, DataBuffer streamData, int offsetInBytes, int stride) {
			int res = SetStreamSourceFunc(comPointer, streamNumber, (streamData == null) ? IntPtr.Zero : streamData.comPointer,
			                                offsetInBytes, stride);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}
		
		public void SetIndices(DataBuffer indexData) {
			int res = SetIndicesFunc(comPointer, (indexData == null) ? IntPtr.Zero : indexData.comPointer);
			if( res < 0 ) { throw new SharpDXException( res ); }
		}
	}
}
