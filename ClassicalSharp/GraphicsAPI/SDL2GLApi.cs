// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
#if !USE_DX && !ANDROID
using System;
using System.IO;
using System.Runtime.InteropServices;

using SDL2;

// TODO: get rid of these
using Matrix4 = OpenTK.Matrix4;
using Vector4 = OpenTK.Vector4;
using OpenTK.Graphics.OpenGL;  // for the enums only!
using System.Drawing;
using System.Drawing.Imaging;
using BmpPixelFormat = System.Drawing.Imaging.PixelFormat;
using GlPixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace ClassicalSharp.GraphicsAPI
{
	/// <summary> Alternate implementation of OpenGLApi that uses SDL2. </summary>
	/// <remarks> Any usage of this class assumes that SDL's video subsystem was initialized. </remarks>
	public unsafe class SDL2GLApi : IGraphicsApi
	{
		private unsafe static class GLFuncs {
			private delegate void GLAlphaFunc(int compareFunc, float value);
			private static GLAlphaFunc glAlphaFunc;
			public static void AlphaFunc(Compare func, float value) {
				glAlphaFunc((int)func, value);
			}
			
			private delegate void GLBegin(int BeginMode);
			private static GLBegin glBegin;
			public static void Begin(BeginMode mode) {
				glBegin((int)mode);
			}

			private delegate void GLBindBuffer(int bufferTarget, int buffer);
			private static GLBindBuffer glBindBuffer;
			public static void BindBuffer(BufferTarget target, int buffer) {
				glBindBuffer((int)target, buffer);
			}

			private delegate void GLBindTexture(int textureTarget, int texture);
			private static GLBindTexture glBindTexture;  // Could be the ARB version
			public static void BindTexture(TextureTarget target, int buffer) {
				glBindTexture((int)target, buffer);
			}

			private delegate void GLBlendFunc(int sfactor, int dfactor);
			private static GLBlendFunc glBlendFunc;
			public static void BlendFunc(BlendingFactor sfactor, BlendingFactor dfactor) {
				glBlendFunc((int)sfactor, (int)dfactor);
			}

			private delegate void GLBufferData(int bufferTarget, IntPtr size, IntPtr data, int usage);
			private static GLBufferData glBufferData;  // Could be the ARB version
			public static void BufferData(BufferTarget target, IntPtr size, IntPtr data, BufferUsage usage) {
				glBufferData((int)target, size, data, (int)usage);
			}

			public static void BufferData<T>(BufferTarget target, IntPtr size, T[] data, BufferUsage usage) where T : struct {
				throw new NotImplementedException();
			}

			private delegate void GLBufferSubData(int bufferTarget, IntPtr offset, IntPtr size, IntPtr data);
			private static GLBufferSubData glBufferSubData;  // Could be the ARB version
			public static void BufferSubData(BufferTarget target, IntPtr offset, IntPtr size, IntPtr data) {
				glBufferSubData((int)target, offset, size, data);
			}

			public static void BufferSubData<T>(BufferTarget target, IntPtr offset, IntPtr size, T[] data) where T : struct {
				throw new NotImplementedException();
			}
			
			private delegate void GLCallList(int list);
			private static GLCallList glCallList;
			public static void CallList(int list) {
				glCallList(list);
			}

			private delegate void GLClear(int mask);
			private static GLClear glClear;
			public static void Clear(ClearBufferMask mask) {
				glClear((int) mask);
			}

			private delegate void GLClearColor(float red, float green, float blue, float alpha);
			private static GLClearColor glClearColor;
			public static void ClearColor(float red, float green, float blue, float alpha) {
				glClearColor(red, green, blue, alpha);
			}

			private delegate void GLColor4ub(byte red, byte green, byte blue, byte alpha);
			private static GLColor4ub glColor4ub;
			public static void Color4ub(byte red, byte green, byte blue, byte alpha) {
				glColor4ub(red, green, blue, alpha);
			}
			
			private delegate void GLColorMask(byte red, byte green, byte blue, byte alpha);
			private static GLColorMask glColorMask;
			public static void ColorMask(bool red, bool green, bool blue, bool alpha) {
				glColorMask(red ? (byte)1 : (byte)0, green ? (byte)1 : (byte)0, blue ? (byte)1 : (byte)0, alpha ? (byte)1 : (byte)0);
			}

			private delegate void GLColorPointer(int size, int pointerType, int stride, IntPtr pointer);
			private static GLColorPointer glColorPointer;
			public static void ColorPointer(int size, PointerType type, int stride, IntPtr pointer) {
				glColorPointer(size, (int)type, stride, pointer);
			}

			private delegate void GLCullFace(int cullFaceMode);
			private static GLCullFace glCullFace;
			public static void CullFace(CullFaceMode mode) {
				glCullFace((int)mode);
			}

			private delegate void GLDeleteBuffers(int n, int* buffers);
			private static GLDeleteBuffers glDeleteBuffers;  // Could be the ARB version
			public static void DeleteBuffers(int n, int* buffers) {
				glDeleteBuffers(n, buffers);
			}

			private delegate void GLDeleteTextures(int n, int* textures);
			private static GLDeleteTextures glDeleteTextures;
			public static void DeleteTextures(int n, int* textures) {
				glDeleteTextures(n, textures);
			}

			private delegate void GLDepthFunc(int compareFunc);
			private static GLDepthFunc glDepthFunc;
			public static void DepthFunc(Compare func) {
				glDepthFunc((int)func);
			}

			private delegate void GLDepthMask(byte flag);
			private static GLDepthMask glDepthMask;
			public static void DepthMask(bool flag) {
				glDepthMask(flag ? (byte)1 : (byte)0);
			}

			private delegate void GLDisable(int enableCap);
			private static GLDisable glDisable;
			public static void Disable(EnableCap cap) {
				glDisable((int)cap);
			}

			private delegate void GLDisableClientState(int arrayCap);
			private static GLDisableClientState glDisableClientState;
			public static void DisableClientState(ArrayCap cap) {
				glDisableClientState((int)cap);
			}
			
			private delegate void GLDeleteLists(int list, int n);
			private static GLDeleteLists glDeleteLists;
			public static void DeleteLists(int list, int n) {
				glDeleteLists(list, n);
			}

			private delegate void GLDrawArrays(int beginMode, int first, int count);
			private static GLDrawArrays glDrawArrays;
			public static void DrawArrays(BeginMode mode, int first, int count) {
				glDrawArrays((int)mode, first, count);
			}

			private delegate void GLDrawElements(int beginMode, int count, int elementsType, IntPtr indices);
			private static GLDrawElements glDrawElements;
			public static void DrawElements(BeginMode mode, int count, DrawElementsType type, IntPtr indices) {
				glDrawElements((int)mode, count, (int)type, indices);
			}

			private delegate void GLEnable(int enableCap);
			private static GLEnable glEnable;
			public static void Enable(EnableCap cap) {
				glEnable((int)cap);
			}

			private delegate void GLEnableClientState(int arrayCap);
			private static GLEnableClientState glEnableClientState;
			public static void EnableClientState(ArrayCap cap) {
				glEnableClientState((int)cap);
			}
			
			private delegate void GLEnd();
			private static GLEnd glEnd;
			public static void End() {
				glEnd();
			}
			
			private delegate void GLEndList();
			private static GLEndList glEndList;
			public static void EndList() {
				glEndList();
			}

			private delegate void GLFogf(int pname, float param);
			private static GLFogf glFogf;
			public static void Fogf(FogParameter pname, float param) {
				glFogf((int)pname, param);
			}

			private delegate void GLFogfv(int pname, float* param);
			private static GLFogfv glFogfv;
			public static void Fogfv(FogParameter pname, float* param) {
				glFogfv((int)pname, param);
			}

			private delegate void GLFogi(int pname, int param);
			private static GLFogi glFogi;
			public static void Fogi(FogParameter pname, int param) {
				glFogi((int)pname, param);
			}

			private delegate void GLGenBuffers(int n, int* buffers);
			private static GLGenBuffers glGenBuffers;  // Could be the ARB version
			public static void GenBuffers(int n, int* buffers) {
				glGenBuffers(n, buffers);
			}

			private delegate int GLGenLists(int n);
			private static GLGenLists glGenLists;
			public static int GenLists(int n) {
				return glGenLists(n);
			}
			
			private delegate void GLGenTextures(int n, int* buffers);
			private static GLGenTextures glGenTextures;
			public static void GenTextures(int n, int* buffers) {
				glGenTextures(n, buffers);
			}

			private delegate ErrorCode GLGetError();
			private static GLGetError glGetError;
			public static ErrorCode GetError() {
				return (ErrorCode)glGetError();
			}

			private delegate void GLGetFloatv(int pname, float* @params);
			private static GLGetFloatv glGetFloatv;
			public static void GetFloatv(GetPName pname, float* @params) {
				glGetFloatv((int)pname, @params);
			}

			private delegate void GLGetIntegerv(int pname, int* @params);
			private static GLGetIntegerv glGetIntegerv;
			public static void GetIntegerv(GetPName pname, int* @params) {
				glGetIntegerv((int)pname,  @params);
			}

			private delegate IntPtr GLGetString(int stringName);
			private static GLGetString glGetString;
			public static IntPtr GetString(StringName name) {
				return (IntPtr)glGetString((int)name);
			}

			private delegate void GLHint(int hintTarget, int hintMode);
			private static GLHint glHint;
			public static void Hint(HintTarget target, HintMode mode) {
				glHint((int)target, (int)mode);
			}

			private delegate void GLLoadIdentity();
			private static GLLoadIdentity glLoadIdentity;
			public static void LoadIdentity() {
				glLoadIdentity();
			}

			private delegate void GLLoadMatrixf(float* m);
			private static GLLoadMatrixf glLoadMatrixf;
			public static void LoadMatrixf(float* m) {
				glLoadMatrixf(m);
			}

			private delegate void GLMatrixMode(int matrixMode);
			private static GLMatrixMode glMatrixMode;
			public static void MatrixMode(MatrixMode mode) {
				glMatrixMode((int)mode);
			}

			private delegate void GLMultMatrixf(float* m);
			private static GLMultMatrixf glMultMatrixf;
			public static void MultMatrixf(float* m) {
				glMultMatrixf(m);
			}
			
			private delegate void GLNewList(int list, int mode);
			private static GLNewList glNewList;
			public static void NewList(int list, int mode) {
				glNewList(list, mode);
			}

			private delegate void GLPopMatrix();
			private static GLPopMatrix glPopMatrix;
			public static void PopMatrix() {
				glPopMatrix();
			}

			private delegate void GLPushMatrix();
			private static GLPushMatrix glPushMatrix;
			public static void PushMatrix() {
				glPushMatrix();
			}

			private delegate void GLReadPixels(int x, int y, int width, int height, int pixelFormat, int pixelType, IntPtr pixels);
			private static GLReadPixels glReadPixels;
			public static void ReadPixels(int x, int y, int width, int height, GlPixelFormat format, PixelType type, IntPtr pixels) {
				glReadPixels(x, y, width, height, (int)format, (int)type, pixels);
			}

			private delegate void GLShadeModel(int shadeMode);
			private static GLShadeModel glShadeModel;
			public static void ShadeModel(ShadingModel mode) {
				glShadeModel((int)mode);
			}

			private delegate void GLTexCoord2f(float u, float v);
			private static GLTexCoord2f glTexCoord2f;
			public static void TexCoord2f(float u, float v) {
				glTexCoord2f(u, v);
			}
			
			private delegate void GLTexCoordPointer(int size, int pointerType, int stride, IntPtr pointer);
			private static GLTexCoordPointer glTexCoordPointer;
			public static void TexCoordPointer(int size, PointerType type, int stride, IntPtr pointer) {
				glTexCoordPointer(size, (int)type, stride, pointer);
			}

			private delegate void GLTexImage2D(int textureTarget, int level, int internalFormat,
			                              int width, int height, int border, int pixelFormat, int pixelType, IntPtr pixels);
			private static GLTexImage2D glTexImage2D;
			public static void TexImage2D(TextureTarget target, int level, PixelInternalFormat internalFormat,
			                              int width, int height, GlPixelFormat format, PixelType type, IntPtr pixels) {
				// border must be 0
				glTexImage2D((int)target, level, (int)internalFormat, width, height, 0, (int)format, (int)type, pixels);
			}

			private delegate void GLTexParameteri(int textureTarget, int pname, int param);
			private static GLTexParameteri glTexParameteri;
			public static void TexParameteri(TextureTarget target, TextureParameterName pname, int param) {
				glTexParameteri((int)target, (int)pname, param);
			}

			private delegate void GLTexSubImage2D(int textureTarget, int level, int xoffset, int yoffset,
			                                 int width, int height, int pixelFormat, int pixelType, IntPtr pixels);
			private static GLTexSubImage2D glTexSubImage2D;
			public static void TexSubImage2D(TextureTarget target, int level, int xoffset, int yoffset,
			                                 int width, int height, GlPixelFormat format, PixelType type, IntPtr pixels) {
				glTexSubImage2D((int)target, level, xoffset, yoffset, width, height, (int)format, (int)type, pixels);
			}
			
			private delegate void GLVertex3f(float x, float y, float z);
			private static GLVertex3f glVertex3f;
			public static void Vertex3f(float x, float y, float z) {
				glVertex3f(x, y, z);
			}

			private delegate void GLVertexPointer(int size, int pointerType, int stride, IntPtr pointer);
			private static GLVertexPointer glVertexPointer;
			public static void VertexPointer(int size, PointerType type, int stride, IntPtr pointer) {
				glVertexPointer(size, (int)type, stride, pointer);
			}

			private delegate void GLViewport(int x, int y, int width, int height);
			private static GLViewport glViewport;
			public static void Viewport(int x, int y, int width, int height) {
				glViewport(x, y, width, height);
			}

			private static Delegate GetFunc(string funcName, Type t) {
				return Marshal.GetDelegateForFunctionPointer(SDL.SDL_GL_GetProcAddress(funcName), t);
			}

			public static void LoadEntryPoints() {
				glAlphaFunc = (GLAlphaFunc) GetFunc("glAlphaFunc", typeof(GLAlphaFunc));
				glBegin = (GLBegin) GetFunc("glBegin", typeof(GLBegin));
				glBindBuffer = (GLBindBuffer) GetFunc("glBindBuffer", typeof(GLBindBuffer));
				glBindTexture = (GLBindTexture) GetFunc("glBindTexture", typeof(GLBindTexture));
				glBlendFunc = (GLBlendFunc) GetFunc("glBlendFunc", typeof(GLBlendFunc));
				glBufferData = (GLBufferData) GetFunc("glBufferData", typeof(GLBufferData));
				glBufferSubData = (GLBufferSubData) GetFunc("glBufferSubData", typeof(GLBufferSubData));
				glCallList = (GLCallList) GetFunc("glCallList", typeof(GLCallList));
				glClear = (GLClear) GetFunc("glClear", typeof(GLClear));
				glClearColor = (GLClearColor) GetFunc("glClearColor", typeof(GLClearColor));
				glColor4ub = (GLColor4ub) GetFunc("glColor4ub", typeof(GLColor4ub));
				glColorMask = (GLColorMask) GetFunc("glColorMask", typeof(GLColorMask));
				glColorPointer = (GLColorPointer) GetFunc("glColorPointer", typeof(GLColorPointer));
				glCullFace = (GLCullFace) GetFunc("glCullFace", typeof(GLCullFace));
				glDeleteBuffers = (GLDeleteBuffers) GetFunc("glDeleteBuffers", typeof(GLDeleteBuffers));
				glDeleteTextures = (GLDeleteTextures) GetFunc("glDeleteTextures", typeof(GLDeleteTextures));
				glDepthFunc = (GLDepthFunc) GetFunc("glDepthFunc", typeof(GLDepthFunc));
				glDepthMask = (GLDepthMask) GetFunc("glDepthMask", typeof(GLDepthMask));
				glDisable = (GLDisable) GetFunc("glDisable", typeof(GLDisable));
				glDisableClientState = (GLDisableClientState) GetFunc("glDisableClientState", typeof(GLDisableClientState));
				glDeleteLists = (GLDeleteLists) GetFunc("glDeleteLists", typeof(GLDeleteLists));
				glDrawArrays = (GLDrawArrays) GetFunc("glDrawArrays", typeof(GLDrawArrays));
				glDrawElements = (GLDrawElements) GetFunc("glDrawElements", typeof(GLDrawElements));
				glEnable = (GLEnable) GetFunc("glEnable", typeof(GLEnable));
				glEnableClientState = (GLEnableClientState) GetFunc("glEnableClientState", typeof(GLEnableClientState));
				glEnd = (GLEnd) GetFunc("glEnd", typeof(GLEnd));
				glEndList = (GLEndList) GetFunc("glEndList", typeof(GLEndList));
				glFogf = (GLFogf) GetFunc("glFogf", typeof(GLFogf));
				glFogfv = (GLFogfv) GetFunc("glFogfv", typeof(GLFogfv));
				glFogi = (GLFogi) GetFunc("glFogi", typeof(GLFogi));
				glGenBuffers = (GLGenBuffers) GetFunc("glGenBuffers", typeof(GLGenBuffers));
				glGenLists = (GLGenLists) GetFunc("glGenLists", typeof(GLGenLists));
				glGenTextures = (GLGenTextures) GetFunc("glGenTextures", typeof(GLGenTextures));
				glGetError = (GLGetError) GetFunc("glGetError", typeof(GLGetError));
				glGetFloatv = (GLGetFloatv) GetFunc("glGetFloatv", typeof(GLGetFloatv));
				glGetIntegerv = (GLGetIntegerv) GetFunc("glGetIntegerv", typeof(GLGetIntegerv));
				glGetString = (GLGetString) GetFunc("glGetString", typeof(GLGetString));
				glHint = (GLHint) GetFunc("glHint", typeof(GLHint));
				glLoadIdentity = (GLLoadIdentity) GetFunc("glLoadIdentity", typeof(GLLoadIdentity));
				glLoadMatrixf = (GLLoadMatrixf) GetFunc("glLoadMatrixf", typeof(GLLoadMatrixf));
				glMatrixMode = (GLMatrixMode) GetFunc("glMatrixMode", typeof(GLMatrixMode));
				glMultMatrixf = (GLMultMatrixf) GetFunc("glMultMatrixf", typeof(GLMultMatrixf));
				glNewList = (GLNewList) GetFunc("glNewList", typeof(GLNewList));
				glPopMatrix = (GLPopMatrix) GetFunc("glPopMatrix", typeof(GLPopMatrix));
				glPushMatrix = (GLPushMatrix) GetFunc("glPushMatrix", typeof(GLPushMatrix));
				glReadPixels = (GLReadPixels) GetFunc("glReadPixels", typeof(GLReadPixels));
				glShadeModel = (GLShadeModel) GetFunc("glShadeModel", typeof(GLShadeModel));
				glTexCoord2f = (GLTexCoord2f) GetFunc("glTexCoord2f", typeof(GLTexCoord2f));
				glTexCoordPointer = (GLTexCoordPointer) GetFunc("glTexCoordPointer", typeof(GLTexCoordPointer));
				glTexImage2D = (GLTexImage2D) GetFunc("glTexImage2D", typeof(GLTexImage2D));
				glTexParameteri = (GLTexParameteri) GetFunc("glTexParameteri", typeof(GLTexParameteri));
				glTexSubImage2D = (GLTexSubImage2D) GetFunc("glTexSubImage2D", typeof(GLTexSubImage2D));
				glVertex3f = (GLVertex3f) GetFunc("glVertex3f", typeof(GLVertex3f));
				glVertexPointer = (GLVertexPointer) GetFunc("glVertexPointer", typeof(GLVertexPointer));
				glViewport = (GLViewport) GetFunc("glViewport", typeof(GLViewport));
			}

			public static void UseArbVboAddresses() {
				glBindBuffer = (GLBindBuffer) GetFunc("glBindBufferARB", typeof(GLBindBuffer));
				glBufferData = (GLBufferData) GetFunc("glBufferDataARB", typeof(GLBufferData));
				glBufferSubData = (GLBufferSubData) GetFunc("glBufferSubDataARB", typeof(GLBufferSubData));
				glDeleteBuffers = (GLDeleteBuffers) GetFunc("glDeleteBuffersARB", typeof(GLDeleteBuffers));
				glGenBuffers = (GLGenBuffers) GetFunc("glGenBuffersARB", typeof(GLGenBuffers));
			}
		}

		bool glLists = false;
		int activeList = -1;
		const int dynamicListId = 1234567891;
		IntPtr dynamicListData;
		
		public SDL2GLApi() {
			GLFuncs.LoadEntryPoints();

			MinZNear = 0.1f;
			InitFields();
			int texDims;
			GLFuncs.GetIntegerv(GetPName.MaxTextureSize, &texDims);
			texDimensions = texDims;
			glLists = Options.GetBool(OptionsKey.ForceOldOpenGL, false);
			CustomMipmapsLevels = !glLists;
			CheckVboSupport();
			
			base.InitCommon();

			setupBatchFuncCol4b = SetupVbPos3fCol4b;
			setupBatchFuncTex2fCol4b = SetupVbPos3fTex2fCol4b;
			setupBatchFuncCol4b_Range = SetupVbPos3fCol4b_Range;
			setupBatchFuncTex2fCol4b_Range = SetupVbPos3fTex2fCol4b_Range;
			
			GLFuncs.EnableClientState(ArrayCap.VertexArray);
			GLFuncs.EnableClientState(ArrayCap.ColorArray);
		}

		private void CheckVboSupport() {
			string extensions = new String((sbyte*)GLFuncs.GetString(StringName.Extensions));
			string version = new String((sbyte*)GLFuncs.GetString(StringName.Version));
			int major = (int)(version[0] - '0'); // x.y. (and so forth)
			int minor = (int)(version[2] - '0');
			if ((major > 1) || (major == 1 && minor >= 5)) return; // Supported in core since 1.5

			Utils.LogDebug("Using ARB vertex buffer objects");
			if (extensions.Contains("GL_ARB_vertex_buffer_object")) {
				GLFuncs.UseArbVboAddresses();
			} else {
				glLists = true;
				CustomMipmapsLevels = false;
			}
		}

		public override bool AlphaTest {
			set { if (value) GLFuncs.Enable(EnableCap.AlphaTest);
				else GLFuncs.Disable(EnableCap.AlphaTest); }
		}

		public override bool AlphaBlending {
			set { if (value) GLFuncs.Enable(EnableCap.Blend);
				else GLFuncs.Disable(EnableCap.Blend); }
		}

		Compare[] compareFuncs;
		public override void AlphaTestFunc(CompareFunc func, float value) {
			GLFuncs.AlphaFunc(compareFuncs[(int)func], value);
		}

		BlendingFactor[] blendFuncs;
		public override void AlphaBlendFunc(BlendFunc srcFunc, BlendFunc dstFunc) {
			GLFuncs.BlendFunc(blendFuncs[(int)srcFunc], blendFuncs[(int)dstFunc]);
		}

		bool fogEnable;
		public override bool Fog {
			get { return fogEnable; }
			set { fogEnable = value;
				if (value) GLFuncs.Enable(EnableCap.Fog);
				else GLFuncs.Disable(EnableCap.Fog); }
		}

		FastColour lastFogCol = FastColour.Black;
		public override void SetFogColour(FastColour col) {
			if (col == lastFogCol) return;
			Vector4 colRGBA = new Vector4(col.R / 255f, col.G / 255f, col.B / 255f, col.A / 255f);
			GLFuncs.Fogfv(FogParameter.FogColor, &colRGBA.X);
			lastFogCol = col;
		}

		float lastFogEnd = -1, lastFogDensity = -1;
		public override void SetFogDensity(float value) {
			if (value == lastFogDensity) return;
			GLFuncs.Fogf(FogParameter.FogDensity, value);
			lastFogDensity = value;
		}

		public override void SetFogStart(float value) {
			GLFuncs.Fogf(FogParameter.FogStart, value);
		}

		public override void SetFogEnd(float value) {
			if (value == lastFogEnd) return;
			GLFuncs.Fogf(FogParameter.FogEnd, value);
			lastFogEnd = value;
		}

		Fog lastFogMode = (Fog)999;
		FogMode[] fogModes;
		public override void SetFogMode(Fog mode) {
			if (mode != lastFogMode) {
				GLFuncs.Fogi(FogParameter.FogMode, (int)fogModes[(int)mode]);
				lastFogMode = mode;
			}
		}

		public override bool FaceCulling {
			set {
				if (value) GLFuncs.Enable(EnableCap.CullFace);
				else GLFuncs.Disable(EnableCap.CullFace);
			}
		}

		public override void Clear() {
			GLFuncs.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}

		FastColour lastClearCol;
		public override void ClearColour(FastColour col) {
			if (col != lastClearCol) {
				GLFuncs.ClearColor(col.R / 255f, col.G / 255f, col.B / 255f, col.A / 255f);
				lastClearCol = col;
			}
		}

		public override void ColourWriteMask(bool r, bool g, bool b, bool a) {
			GLFuncs.ColorMask(r, g, b, a);
		}

		public override void DepthTestFunc(CompareFunc func) {
			GLFuncs.DepthFunc(compareFuncs[(int)func]);
		}

		public override bool DepthTest {
			set { if (value) GLFuncs.Enable(EnableCap.DepthTest);
				else GLFuncs.Disable(EnableCap.DepthTest);
			}
		}

		public override bool DepthWrite { set { GLFuncs.DepthMask(value); } }

		public override bool AlphaArgBlend { set { } }

		#region Texturing

		int texDimensions;
		public override int MaxTextureDimensions { get { return texDimensions; } }

		public override bool Texturing {
			set { if (value) GLFuncs.Enable(EnableCap.Texture2D);
				else GLFuncs.Disable(EnableCap.Texture2D);
			}
		}

		protected override int CreateTexture(int width, int height, IntPtr scan0, bool managedPool, bool mipmaps) {
			int texId = 0;
			GLFuncs.GenTextures(1, &texId);
			GLFuncs.BindTexture(TextureTarget.Texture2D, texId);
			GLFuncs.TexParameteri(TextureTarget.Texture2D, TextureParameterName.MagFilter, (int)TextureFilter.Nearest);

			if (mipmaps) {
				GLFuncs.TexParameteri(TextureTarget.Texture2D, TextureParameterName.MinFilter, (int)TextureFilter.NearestMipmapLinear);
				if (CustomMipmapsLevels) {
					GLFuncs.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, MipmapsLevels(width, height));
				}
			} else {
				GLFuncs.TexParameteri(TextureTarget.Texture2D, TextureParameterName.MinFilter, (int)TextureFilter.Nearest);
			}

			GLFuncs.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height,
				GlPixelFormat.Bgra, PixelType.UnsignedByte, scan0);

			if (mipmaps) DoMipmaps(texId, 0, 0, width, height, scan0, false);
			return texId;
		}

		unsafe void DoMipmaps(int texId, int x, int y, int width,
			int height, IntPtr scan0, bool partial) {
			IntPtr prev = scan0;
			int lvls = MipmapsLevels(width, height);

			for (int lvl = 1; lvl <= lvls; lvl++) {
				x /= 2; y /= 2;
				if (width > 1)   width /= 2;
				if (height > 1) height /= 2;
				int size = width * height * 4;

				IntPtr cur = Marshal.AllocHGlobal(size);
				GenMipmaps(width, height, cur, prev);

				if (partial) {
					GLFuncs.TexSubImage2D(TextureTarget.Texture2D, lvl, x, y, width, height,
						GlPixelFormat.Bgra, PixelType.UnsignedByte, cur);
				} else {
					GLFuncs.TexImage2D(TextureTarget.Texture2D, lvl, PixelInternalFormat.Rgba, width, height,
						GlPixelFormat.Bgra, PixelType.UnsignedByte, cur);
				}

				if (prev != scan0) Marshal.FreeHGlobal(prev);
				prev = cur;
			}
			if (prev != scan0) Marshal.FreeHGlobal(prev);
		}

		public override void BindTexture(int texture) {
			GLFuncs.BindTexture(TextureTarget.Texture2D, texture);
		}

		public override void UpdateTexturePart(int texId, int x, int y, FastBitmap part, bool mipmaps) {
			GLFuncs.BindTexture(TextureTarget.Texture2D, texId);
			GLFuncs.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, part.Width, part.Height,
				GlPixelFormat.Bgra, PixelType.UnsignedByte, part.Scan0);
			if (mipmaps) DoMipmaps(texId, x, y, part.Width, part.Height, part.Scan0, true);
		}

		public override void DeleteTexture(ref int texId) {
			if (texId <= 0) return;
			int id = texId; GLFuncs.DeleteTextures(1, &id);
			texId = -1;
		}

		public override void EnableMipmaps() { }

		public override void DisableMipmaps() { }
		#endregion

		#region Vertex/index buffers
		Action setupBatchFunc, setupBatchFuncCol4b, setupBatchFuncTex2fCol4b;
		Action<int> setupBatchFunc_Range, setupBatchFuncCol4b_Range, setupBatchFuncTex2fCol4b_Range;

		public override int CreateDynamicVb(VertexFormat format, int maxVertices) {
			if (glLists) return dynamicListId;
			int id = GenAndBind(BufferTarget.ArrayBuffer);
			int sizeInBytes = maxVertices * strideSizes[(int)format];
			GLFuncs.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeInBytes), IntPtr.Zero, BufferUsage.DynamicDraw);
			return id;
		}

		public override int CreateVb(IntPtr vertices, VertexFormat format, int count) {
			if (glLists) {
				VertexFormat curFormat = batchFormat;
				SetBatchFormat(format);
				int list = GLFuncs.GenLists(1);
				GLFuncs.NewList(list, 0x1300);
				count &= ~0x01; // Need to get rid of the 1 extra element, see comment in chunk mesh builder for why

				const int maxIndices = 65536 / 4 * 6;
				ushort* indicesPtr = stackalloc ushort[maxIndices];
				MakeIndices(indicesPtr, maxIndices);

				int stride = format == VertexFormat.P3fT2fC4b ? VertexP3fT2fC4b.Size : VertexP3fC4b.Size;
				GLFuncs.VertexPointer(3, PointerType.Float, stride, vertices);
				GLFuncs.ColorPointer(4, PointerType.UnsignedByte, stride, (IntPtr)((byte*)vertices + 12));
				if (format == VertexFormat.P3fT2fC4b) {
					GLFuncs.TexCoordPointer(2, PointerType.Float, stride, (IntPtr)((byte*)vertices + 16));
				}

				GLFuncs.DrawElements(BeginMode.Triangles, (count >> 2) * 6, DrawElementsType.UnsignedShort, (IntPtr)indicesPtr);
				GLFuncs.EndList();
				SetBatchFormat(curFormat);
				return list;
			}
			
			int id = GenAndBind(BufferTarget.ArrayBuffer);
			int sizeInBytes = count * strideSizes[(int)format];
			GLFuncs.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeInBytes), vertices, BufferUsage.StaticDraw);
			return id;
		}

		public override int CreateIb(IntPtr indices, int indicesCount) {
			if (glLists) return 0;
			int id = GenAndBind(BufferTarget.ElementArrayBuffer);
			int sizeInBytes = indicesCount * sizeof(ushort);
			GLFuncs.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(sizeInBytes), indices, BufferUsage.StaticDraw);
			return id;
		}

		static int GenAndBind(BufferTarget target) {
			int id = 0;
			GLFuncs.GenBuffers(1, &id);
			GLFuncs.BindBuffer(target, id);
			return id;
		}

		int batchStride;

		public override void SetDynamicVbData(int id, IntPtr vertices, int count) {
			if (glLists) {
				activeList = dynamicListId;
				dynamicListData = vertices;
				return;
			}

			GLFuncs.BindBuffer(BufferTarget.ArrayBuffer, id);
			GLFuncs.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero,
				new IntPtr(count * batchStride), vertices);
		}
		
		static void V(VertexP3fC4b v) {
			FastColour AAA = FastColour.Unpack(v.Colour);
			GLFuncs.Color4ub(AAA.R, AAA.G, AAA.B, AAA.A);
			GLFuncs.Vertex3f(v.X, v.Y, v.Z);
		}

		static void V(VertexP3fT2fC4b v) {
			FastColour AAA = FastColour.Unpack(v.Colour);
			GLFuncs.Color4ub(AAA.R, AAA.G, AAA.B, AAA.A);
			GLFuncs.TexCoord2f(v.U, v.V);
			GLFuncs.Vertex3f(v.X, v.Y, v.Z);
		}

		public override void DeleteVb(ref int vb) {
			if (vb <= 0) return;
			int id = vb; vb = -1;

			if (glLists) { if (id != dynamicListId) GLFuncs.DeleteLists(id, 1); }
			else { GLFuncs.DeleteBuffers(1, &id); }
		}

		public override void DeleteIb(ref int ib) {
			if (glLists || ib <= 0) return;
			int id = ib; ib = -1;
			GLFuncs.DeleteBuffers(1, &id);
		}

		VertexFormat batchFormat = (VertexFormat)999;
		public override void SetBatchFormat(VertexFormat format) {
			if (format == batchFormat) return;

			if (batchFormat == VertexFormat.P3fT2fC4b) {
				GLFuncs.DisableClientState(ArrayCap.TextureCoordArray);
			}

			batchFormat = format;
			if (format == VertexFormat.P3fT2fC4b) {
				GLFuncs.EnableClientState(ArrayCap.TextureCoordArray);
				setupBatchFunc = setupBatchFuncTex2fCol4b;
				setupBatchFunc_Range = setupBatchFuncTex2fCol4b_Range;
				batchStride = VertexP3fT2fC4b.Size;
			} else {
				setupBatchFunc = setupBatchFuncCol4b;
				setupBatchFunc_Range = setupBatchFuncCol4b_Range;
				batchStride = VertexP3fC4b.Size;
			}
		}

		public override void BindVb(int vb) {
			if (glLists) { activeList = vb; }
			else { GLFuncs.BindBuffer(BufferTarget.ArrayBuffer, vb); }
		}

		public override void BindIb(int ib) {
			if (glLists) return;
			GLFuncs.BindBuffer(BufferTarget.ElementArrayBuffer, ib);
		}

		const DrawElementsType indexType = DrawElementsType.UnsignedShort;
		public override void DrawVb_Lines(int verticesCount) {
			if (glLists) { DrawDynamicLines(verticesCount); return; }
			
			setupBatchFunc();
			GLFuncs.DrawArrays(BeginMode.Lines, 0, verticesCount);
		}

		public override void DrawVb_IndexedTris(int verticesCount, int startVertex) {
			if (glLists) {
				if (activeList != dynamicListId) { GLFuncs.CallList(activeList); }
				else { DrawDynamicTriangles(verticesCount, startVertex); }
				return;
			}

			setupBatchFunc_Range(startVertex);
			GLFuncs.DrawElements(BeginMode.Triangles, (verticesCount >> 2) * 6, indexType, IntPtr.Zero);
		}

		public override void DrawVb_IndexedTris(int verticesCount) {
			if (glLists) {
				if (activeList != dynamicListId) { GLFuncs.CallList(activeList); }
				else { DrawDynamicTriangles(verticesCount, 0); }
				return;
			}

			setupBatchFunc();
			GLFuncs.DrawElements(BeginMode.Triangles, (verticesCount >> 2) * 6, indexType, IntPtr.Zero);
		}
		
		unsafe void DrawDynamicLines(int verticesCount) {
			GLFuncs.Begin(BeginMode.Lines);
			if (batchFormat == VertexFormat.P3fT2fC4b) {
				VertexP3fT2fC4b* ptr = (VertexP3fT2fC4b*)dynamicListData;
				for (int i = 0; i < verticesCount; i += 2) {
					V(ptr[i + 0]); V(ptr[i + 1]);
				}
			} else {
				VertexP3fC4b* ptr = (VertexP3fC4b*)dynamicListData;
				for (int i = 0; i < verticesCount; i += 2) {
					V(ptr[i + 0]); V(ptr[i + 1]);
				}
			}
			GLFuncs.End();
		}

		unsafe void DrawDynamicTriangles(int verticesCount, int startVertex) {
			GLFuncs.Begin(BeginMode.Triangles);
			if (batchFormat == VertexFormat.P3fT2fC4b) {
				VertexP3fT2fC4b* ptr = (VertexP3fT2fC4b*)dynamicListData;
				for (int i = startVertex; i < startVertex + verticesCount; i += 4) {
					V(ptr[i + 0]); V(ptr[i + 1]); V(ptr[i + 2]);
					V(ptr[i + 2]); V(ptr[i + 3]); V(ptr[i + 0]);
				}
			} else {
				VertexP3fC4b* ptr = (VertexP3fC4b*)dynamicListData;
				for (int i = startVertex; i < startVertex + verticesCount; i += 4) {
					V(ptr[i + 0]); V(ptr[i + 1]); V(ptr[i + 2]);
					V(ptr[i + 2]); V(ptr[i + 3]); V(ptr[i + 0]);
				}
			}
			GLFuncs.End();
		}

		int lastPartialList = -1;
		internal override void DrawIndexedVb_TrisT2fC4b(int verticesCount, int startIndex) {
			// TODO: This renders the whole map, bad performance!! FIX FIX
			if (glLists) {
				if (activeList != lastPartialList) {
					GLFuncs.CallList(activeList); lastPartialList = activeList;
				}
				return;
			}

			int offset = startIndex * VertexP3fT2fC4b.Size;
			GLFuncs.VertexPointer(3, PointerType.Float, VertexP3fT2fC4b.Size, new IntPtr(offset));
			GLFuncs.ColorPointer(4, PointerType.UnsignedByte, VertexP3fT2fC4b.Size, new IntPtr(offset + 12));
			GLFuncs.TexCoordPointer(2, PointerType.Float, VertexP3fT2fC4b.Size, new IntPtr(offset + 16));
			GLFuncs.DrawElements(BeginMode.Triangles, (verticesCount >> 2) * 6, indexType, IntPtr.Zero);
		}

		IntPtr zero = new IntPtr(0), twelve = new IntPtr(12), sixteen = new IntPtr(16);

		void SetupVbPos3fCol4b() {
			GLFuncs.VertexPointer(3, PointerType.Float, VertexP3fC4b.Size, zero);
			GLFuncs.ColorPointer(4, PointerType.UnsignedByte, VertexP3fC4b.Size, twelve);
		}

		void SetupVbPos3fTex2fCol4b() {
			GLFuncs.VertexPointer(3, PointerType.Float, VertexP3fT2fC4b.Size, zero);
			GLFuncs.ColorPointer(4, PointerType.UnsignedByte, VertexP3fT2fC4b.Size, twelve);
			GLFuncs.TexCoordPointer(2, PointerType.Float, VertexP3fT2fC4b.Size, sixteen);
		}

		void SetupVbPos3fCol4b_Range(int startVertex) {
			int offset = startVertex * VertexP3fC4b.Size;
			GLFuncs.VertexPointer(3, PointerType.Float, VertexP3fC4b.Size, new IntPtr(offset));
			GLFuncs.ColorPointer(4, PointerType.UnsignedByte, VertexP3fC4b.Size, new IntPtr(offset + 12));
		}

		void SetupVbPos3fTex2fCol4b_Range(int startVertex) {
			int offset = startVertex * VertexP3fT2fC4b.Size;
			GLFuncs.VertexPointer(3, PointerType.Float, VertexP3fT2fC4b.Size, new IntPtr(offset));
			GLFuncs.ColorPointer(4, PointerType.UnsignedByte, VertexP3fT2fC4b.Size, new IntPtr(offset + 12));
			GLFuncs.TexCoordPointer(2, PointerType.Float, VertexP3fT2fC4b.Size, new IntPtr(offset + 16));
		}
		#endregion

		#region Matrix manipulation
		MatrixMode lastMode = 0;
		MatrixMode[] matrixModes;
		public override void SetMatrixMode(MatrixType mode) {
			MatrixMode glMode = matrixModes[(int)mode];
			if (glMode != lastMode) {
				GLFuncs.MatrixMode(glMode);
				lastMode = glMode;
			}
		}

		public override void LoadMatrix(ref Matrix4 matrix) {
			fixed (Single* ptr = &matrix.Row0.X)
				GLFuncs.LoadMatrixf(ptr);
		}

		public override void LoadIdentityMatrix() {
			GLFuncs.LoadIdentity();
		}

		public override void CalcOrthoMatrix(float width, float height, out Matrix4 matrix) {
			Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -10000, 10000, out matrix);
		}

		#endregion

		public override void BeginFrame(Game game) {
		}

		public override void EndFrame(Game game) {
			game.window.SwapBuffers();
		}

		public override void SetVSync(Game game, bool value) {
			game.VSync = value;
		}

		bool isIntelRenderer;
		internal override void MakeApiInfo() {
			string vendor = new String((sbyte*)GLFuncs.GetString(StringName.Vendor));
			string renderer = new String((sbyte*)GLFuncs.GetString(StringName.Renderer));
			string version = new String((sbyte*)GLFuncs.GetString(StringName.Version));
			int depthBits = 0;
			GLFuncs.GetIntegerv(GetPName.DepthBits, &depthBits);

			ApiInfo = new string[] {
				"--Using OpenGL api--",
				"Vendor: " + vendor,
				"Renderer: " + renderer,
				"GL version: " + version,
				"Max 2D texture dimensions: " + MaxTextureDimensions,
				"Depth buffer bits: " + depthBits,
			};
			isIntelRenderer = renderer.Contains("Intel");
		}

		public override bool WarnIfNecessary(Chat chat) {
			if (glLists) {
				chat.Add("&cYou are using the very outdated OpenGL backend.");
				chat.Add("&cAs such you may experience poor performance.");
				chat.Add("&cIt is likely you need to install video card drivers.");
			}
			
			if (!isIntelRenderer) return false;

			chat.Add("&cIntel graphics cards are known to have issues with the OpenGL build.");
			chat.Add("&cVSync may not work, and you may see disappearing clouds and map edges.");
			chat.Add("&cFor Windows, try downloading the Direct3D 9 build instead.");
			return true;
		}

		// Based on http://www.opentk.com/doc/graphics/save-opengl-rendering-to-disk
		public override void TakeScreenshot(Stream output, int width, int height) {
			using (Bitmap bmp = new Bitmap(width, height, BmpPixelFormat.Format32bppRgb)) { // ignore alpha component
				using (FastBitmap fastBmp = new FastBitmap(bmp, true, false)) {
					GLFuncs.ReadPixels(0, 0, width, height, GlPixelFormat.Bgra, PixelType.UnsignedByte, fastBmp.Scan0);
				}

				bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
				Platform.WriteBmp(bmp, output);
			}
		}

		public override void OnWindowResize(Game game) {
			GLFuncs.Viewport(0, 0, game.Width, game.Height);
		}

		void InitFields() {
			// See comment in Inventory() constructor for why this is necessary.
			blendFuncs = new BlendingFactor[6];
			blendFuncs[0] = BlendingFactor.Zero; blendFuncs[1] = BlendingFactor.One;
			blendFuncs[2] = BlendingFactor.SrcAlpha; blendFuncs[3] = BlendingFactor.OneMinusSrcAlpha;
			blendFuncs[4] = BlendingFactor.DstAlpha; blendFuncs[5] = BlendingFactor.OneMinusDstAlpha;
			compareFuncs = new Compare[8];
			compareFuncs[0] = Compare.Always; compareFuncs[1] = Compare.Notequal;
			compareFuncs[2] = Compare.Never; compareFuncs[3] = Compare.Less;
			compareFuncs[4] = Compare.Lequal; compareFuncs[5] = Compare.Equal;
			compareFuncs[6] = Compare.Gequal; compareFuncs[7] = Compare.Greater;

			fogModes = new FogMode[3];
			fogModes[0] = FogMode.Linear; fogModes[1] = FogMode.Exp;
			fogModes[2] = FogMode.Exp2;
			matrixModes = new MatrixMode[3];
			matrixModes[0] = MatrixMode.Projection; matrixModes[1] = MatrixMode.Modelview;
			matrixModes[2] = MatrixMode.Texture;
		}
	}
}
#endif
