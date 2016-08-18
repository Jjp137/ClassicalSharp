﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using ClassicalSharp.Audio;
using ClassicalSharp.Commands;
using ClassicalSharp.Entities;
using ClassicalSharp.GraphicsAPI;
using ClassicalSharp.Gui;
using ClassicalSharp.Map;
using ClassicalSharp.Model;
using ClassicalSharp.Network;
using ClassicalSharp.Particles;
using ClassicalSharp.Renderers;
using ClassicalSharp.Selections;
using ClassicalSharp.TexturePack;
using OpenTK;
using OpenTK.Input;
#if ANDROID
using Android.Graphics;
#endif
using PathIO = System.IO.Path; // Android.Graphics.Path clash otherwise

namespace ClassicalSharp {

	public partial class Game : IDisposable {
		
		void LoadAtlas( Bitmap bmp ) {
			TerrainAtlas1D.Dispose();
			TerrainAtlas.Dispose();
			TerrainAtlas.UpdateState( BlockInfo, bmp );
			TerrainAtlas1D.UpdateState( TerrainAtlas );
		}
		
		public bool ChangeTerrainAtlas( Bitmap atlas ) {
			bool pow2 = Utils.IsPowerOf2( atlas.Width ) && Utils.IsPowerOf2( atlas.Height );
			if( !pow2 || atlas.Width != atlas.Height ) {
				Chat.Add( "&cCurrent texture pack has an invalid terrain.png" );
				Chat.Add( "&cWidth and length must be the same, and also powers of two." );
				return false;
			}
			LoadAtlas( atlas );
			Events.RaiseTerrainAtlasChanged();
			return true;
		}
		
		public void Run() { window.Run(); }
		
		public void Exit() { window.Exit(); }
		
		void OnNewMapCore( object sender, EventArgs e ) {
			for( int i = 0; i < Components.Count; i++ )
				Components[i].OnNewMap( this );
		}
		
		void OnNewMapLoadedCore( object sender, EventArgs e ) {
			for( int i = 0; i < Components.Count; i++ )
				Components[i].OnNewMapLoaded( this );
		}
		
		public T AddComponent<T>( T obj ) where T : IGameComponent {
			Components.Add( obj );
			return obj;
		}
		
		public bool ReplaceComponent<T>( ref T old, T obj ) where T : IGameComponent {
			for( int i = 0; i < Components.Count; i++ ) {
				if( !object.ReferenceEquals( Components[i], old ) ) continue;
				old.Dispose();
				
				Components[i] = obj;
				old = obj;
				obj.Init( this );
				return true;
			}
			
			Components.Add( obj );
			obj.Init( this );
			return false;
		}
		
		public void SetViewDistance( int distance, bool save ) {
			ViewDistance = distance;
			if( ViewDistance > MaxViewDistance )
				ViewDistance = MaxViewDistance;
			Utils.LogDebug( "setting view distance to: {0} ({1})", distance, ViewDistance );
			
			if( save ) {
				UserViewDistance = distance;
				Options.Set( OptionsKey.ViewDist, distance );
			}
			Events.RaiseViewDistanceChanged();
			UpdateProjection();
		}
		
		Stopwatch frameTimer = new Stopwatch();
		internal void RenderFrame( double delta ) {
			frameTimer.Reset();
			frameTimer.Start();
			
			Graphics.BeginFrame( this );
			Graphics.BindIb( defaultIb );
			accumulator += delta;
			Vertices = 0;
			if( !Focused && !Gui.ActiveScreen.HandlesAllInput )
				Gui.SetNewScreen( new PauseScreen( this ) );
			CheckZoomFov();
			
			DoScheduledTasks( delta );
			float t = (float)(entTask.Accumulator / entTask.Interval);
			LocalPlayer.SetInterpPosition( t );
			
			if( !SkipClear || SkyboxRenderer.ShouldRender )
				Graphics.Clear();
			UpdateViewMatrix( delta, t );
			
			bool visible = Gui.activeScreen == null || !Gui.activeScreen.BlocksWorld;
			if( World.IsNotLoaded ) visible = false;
			if( visible )
				Render3D( delta, t );
			else
				SelectedPos.SetAsInvalid();
			
			Gui.Render( delta );
			if( screenshotRequested )
				TakeScreenshot();
			Graphics.EndFrame( this );
			LimitFPS();
		}
		
		void CheckZoomFov() {
			bool allowZoom = Gui.activeScreen == null && !Gui.hudScreen.HandlesAllInput;
			if( allowZoom && IsKeyDown( KeyBind.ZoomScrolling ) )
				InputHandler.SetFOV( ZoomFov, false );
		}
		
		void UpdateViewMatrix( double delta, float t ) {
			Graphics.SetMatrixMode( MatrixType.Modelview );
			Matrix4 modelView = Camera.GetView( delta, t );
			View = modelView;
			Graphics.LoadMatrix( ref modelView );
			Culling.CalcFrustumEquations( ref Projection, ref modelView );
		}
		
		void Render3D( double delta, float t ) {
			CurrentCameraPos = Camera.GetCameraPos( LocalPlayer.EyePosition );
			if( SkyboxRenderer.ShouldRender )
				SkyboxRenderer.Render( delta );
			AxisLinesRenderer.Render( delta );
			Entities.RenderModels( Graphics, delta, t );
			Entities.RenderNames( Graphics, delta );
			
			ParticleManager.Render( delta, t );
			Camera.GetPickedBlock( SelectedPos ); // TODO: only pick when necessary
			EnvRenderer.Render( delta );
			MapRenderer.Render( delta );
			MapBordersRenderer.RenderSides( delta );
			
			if( SelectedPos.Valid && !HideGui ) {
				Picking.UpdateState( SelectedPos );
				Picking.Render( delta );
			}
			
			// Render water over translucent blocks when underwater for proper alpha blending
			Vector3 pos = LocalPlayer.Position;
			if( CurrentCameraPos.Y < World.Env.EdgeHeight
			   && (pos.X < 0 || pos.Z < 0 || pos.X > World.Width || pos.Z > World.Length) ) {
				MapRenderer.RenderTranslucent( delta );
				MapBordersRenderer.RenderEdges( delta );
			} else {
				MapBordersRenderer.RenderEdges( delta );
				MapRenderer.RenderTranslucent( delta );
			}
			
			// Need to render again over top of translucent block, as the selection outline
			// is drawn without writing to the depth buffer
			if( SelectedPos.Valid && !HideGui && BlockInfo.IsTranslucent[SelectedPos.Block] )
				Picking.Render( delta );
			
			Entities.DrawShadows();
			SelectionManager.Render( delta );
			Entities.RenderHoveredNames( Graphics, delta );
			
			bool left = IsMousePressed( MouseButton.Left );
			bool middle = IsMousePressed( MouseButton.Middle );
			bool right = IsMousePressed( MouseButton.Right );
			InputHandler.PickBlocks( true, left, middle, right );
			if( !HideGui )
				HeldBlockRenderer.Render( delta );
		}
		
		void DoScheduledTasks( double time ) {
			for( int i = 0; i < Tasks.Count; i++ ) {
				ScheduledTask task = Tasks[i];
				task.Accumulator += time;
				
				while( task.Accumulator >= task.Interval ) {
					task.Callback( task );
					task.Accumulator -= task.Interval;					
				}
			}
		}
		
		public ScheduledTask AddScheduledTask( double interval, 
		                                      Action<ScheduledTask> callback ) {
			ScheduledTask task = new ScheduledTask();
			task.Interval = interval; task.Callback = callback;
			Tasks.Add( task );
			return task;
		}
		
		void TakeScreenshot() {
			string path = PathIO.Combine( Program.AppDirectory, "screenshots" );
			if( !Directory.Exists( path ) )
				Directory.CreateDirectory( path );
			
			string timestamp = DateTime.Now.ToString( "dd-MM-yyyy-HH-mm-ss" );
			string file = "screenshot_" + timestamp + ".png";
			path = PathIO.Combine( path, file );
			Graphics.TakeScreenshot( path, Width, Height );
			Chat.Add( "&eTaken screenshot as: " + file );
			screenshotRequested = false;
		}
		
		public void UpdateProjection() {
			DefaultFov = Options.GetInt( OptionsKey.FieldOfView, 1, 150, 70 );
			Matrix4 projection = Camera.GetProjection();
			Projection = projection;
			
			Graphics.SetMatrixMode( MatrixType.Projection );
			Graphics.LoadMatrix( ref projection );
			Graphics.SetMatrixMode( MatrixType.Modelview );
			Events.RaiseProjectionChanged();
		}
		
		internal void OnResize() {
			Width = window.Width; Height = window.Height;
			Graphics.OnWindowResize( this );
			UpdateProjection();
			Gui.OnResize();
		}
		
		public void Disconnect( string title, string reason ) {
			Gui.Reset( this );
			World.Reset();
			World.blocks = null;
			Drawer2D.InitColours();
			
			for( int block = BlockInfo.CpeCount; block < BlockInfo.BlocksCount; block++ )
				BlockInfo.ResetBlockInfo( (byte)block, false );
			BlockInfo.SetupCullingCache();
			BlockInfo.InitLightOffsets();
			
			Network.ExtractDefault();
			Gui.SetNewScreen( new ErrorScreen( this, title, reason ) );
			GC.Collect();
		}		
		
		public void CycleCamera() {
			if( ClassicMode ) return;
			PerspectiveCamera oldCam = (PerspectiveCamera)Camera;
			if( Camera == firstPersonCam ) Camera = thirdPersonCam;
			else if( Camera == thirdPersonCam ) Camera = forwardThirdPersonCam;
			else Camera = firstPersonCam;

			if( !LocalPlayer.Hacks.CanUseThirdPersonCamera || !LocalPlayer.Hacks.Enabled )
				Camera = firstPersonCam;
			PerspectiveCamera newCam = (PerspectiveCamera)Camera;
			newCam.delta = oldCam.delta;
			newCam.previous = oldCam.previous;
			UpdateProjection();
		}
		
		public void UpdateBlock( int x, int y, int z, byte block ) {
			int oldHeight = World.GetLightHeight( x, z ) + 1;
			World.SetBlock( x, y, z, block );
			int newHeight = World.GetLightHeight( x, z ) + 1;
			MapRenderer.RedrawBlock( x, y, z, block, oldHeight, newHeight );
		}
		
		float limitMilliseconds;
		public void SetFpsLimitMethod( FpsLimitMethod method ) {
			FpsLimit = method;
			limitMilliseconds = 0;
			Graphics.SetVSync( this, method == FpsLimitMethod.LimitVSync );
			
			if( method == FpsLimitMethod.Limit120FPS )
				limitMilliseconds = 1000f / 120;
			if( method == FpsLimitMethod.Limit60FPS )
				limitMilliseconds = 1000f / 60;
			if( method == FpsLimitMethod.Limit30FPS )
				limitMilliseconds = 1000f / 30;
		}
		
		void LimitFPS() {
			if( FpsLimit == FpsLimitMethod.LimitVSync ) return;
			
			double elapsed = frameTimer.Elapsed.TotalMilliseconds;
			double leftOver = limitMilliseconds - elapsed;
			if( leftOver > 0.001 ) // going faster than limit
				Thread.Sleep( (int)Math.Round( leftOver, MidpointRounding.AwayFromZero ) );
		}
		
		public bool IsKeyDown( Key key ) { return InputHandler.IsKeyDown( key ); }
		
		public bool IsKeyDown( KeyBind binding ) { return InputHandler.IsKeyDown( binding ); }
		
		public bool IsMousePressed( MouseButton button ) { return InputHandler.IsMousePressed( button ); }
		
		public Key Mapping( KeyBind mapping ) { return InputHandler.Keys[mapping]; }
		
		public void Dispose() {
			MapRenderer.Dispose();
			TerrainAtlas.Dispose();
			TerrainAtlas1D.Dispose();
			ModelCache.Dispose();
			Entities.Dispose();
			WorldEvents.OnNewMap -= OnNewMapCore;
			WorldEvents.OnNewMapLoaded -= OnNewMapLoadedCore;
			
			for( int i = 0; i < Components.Count; i++ )
				Components[i].Dispose();
			
			Graphics.DeleteIb( defaultIb );
			Drawer2D.DisposeInstance();
			Graphics.DeleteTexture( ref CloudsTex );
			Graphics.Dispose();
			
			if( Options.HasChanged ) {
				Options.Load();
				Options.Save();
			}
		}
		
		internal bool CanPick( byte block ) {
			if( BlockInfo.IsAir[block] ) return false;
			if( BlockInfo.IsSprite[block] ) return true;
			if( BlockInfo.Collide[block] != CollideType.SwimThrough ) return true;
			
			return !ModifiableLiquids ? false :
				Inventory.CanPlace[block] && Inventory.CanDelete[block];
		}
		
		
		/// <summary> Reads a bitmap from the stream (converting it to 32 bits per pixel if necessary),
		/// and updates the native texture for it. </summary>
		public bool UpdateTexture( ref int texId, string file, byte[] data, bool setSkinType ) {
			MemoryStream stream = new MemoryStream( data );
			int maxSize = Graphics.MaxTextureDimensions;
			using( Bitmap bmp = Platform.ReadBmp( stream ) ) {
				if( bmp.Width > maxSize || bmp.Height > maxSize ) {
					Chat.Add( "&cUnable to use " + file + " from the texture pack." );
					Chat.Add( "&c Its size is (" + bmp.Width + "," + bmp.Height
					         + "), your GPU supports (" + maxSize + "," + maxSize + ") at most." );
					return false;
				}
				
				Graphics.DeleteTexture( ref texId );
				if( setSkinType )
					DefaultPlayerSkinType = Utils.GetSkinType( bmp );
				
				if( !Platform.Is32Bpp( bmp ) ) {
					using( Bitmap bmp32 = Drawer2D.ConvertTo32Bpp( bmp ) )
						texId = Graphics.CreateTexture( bmp32 );
				} else {
					texId = Graphics.CreateTexture( bmp );
				}
				return true;
			}
		}
		
		public bool SetRenderType( string type ) {
			if( Utils.CaselessEquals( type, "legacyfast" ) ) {
				SetNewRenderType( true, true );
			} else if( Utils.CaselessEquals( type, "legacy" ) ) {
				SetNewRenderType( true, false );
			} else if( Utils.CaselessEquals( type, "normal" ) ) {
				SetNewRenderType( false, false );
			} else if( Utils.CaselessEquals( type, "normalfast" ) ) {
				SetNewRenderType( false, true );
			} else {
				return false;
			}
			Options.Set( OptionsKey.RenderType, type );
			return true;
		}
		
		void SetNewRenderType( bool legacy, bool minimal ) {
			if( MapBordersRenderer == null ) {
				MapBordersRenderer = AddComponent( new MapBordersRenderer() );
				MapBordersRenderer.legacy = legacy;
			} else {
				MapBordersRenderer.UseLegacyMode( legacy );
			}
			
			if( minimal ) {
				if( EnvRenderer == null )
					EnvRenderer = AddComponent( new MinimalEnvRenderer() );
				else
					ReplaceComponent( ref EnvRenderer, new MinimalEnvRenderer() );
			} else if( EnvRenderer == null ) {
				EnvRenderer = AddComponent( new StandardEnvRenderer() );
				((StandardEnvRenderer)EnvRenderer).legacy = legacy;
			} else {
				if( !(EnvRenderer is StandardEnvRenderer) )
					ReplaceComponent( ref EnvRenderer, new StandardEnvRenderer() );
				((StandardEnvRenderer)EnvRenderer).UseLegacyMode( legacy );
			}
		}
		
		public Game( string username, string mppass, string skinServer,
		            bool nullContext, int width, int height ) {
			#if USE_DX
			// TODO: implement SDL2 + DirectX and replace this
			window = new SDL2DXWindow( this, username, nullContext, width, height );
			#else
			window = new SDL2GLWindow( this, username, nullContext, width, height );
			#endif
			Username = username;
			Mppass = mppass;
			this.skinServer = skinServer;
		}
	}
}