﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using System.Drawing;
using ClassicalSharp.GraphicsAPI;
using OpenTK.Input;

namespace ClassicalSharp.Gui {
	
	public class HudScreen : Screen, IGameComponent {
		
		public HudScreen( Game game ) : base( game ) { }
		
		ChatScreen chat;
		internal BlockHotbarWidget hotbar;
		PlayerListWidget playerList;
		Font playerFont;
		
		public void Init( Game game ) { }
		public void Ready( Game game) { Init(); }
		public void Reset( Game game ) { }
		public void OnNewMap( Game game ) { }
		public void OnNewMapLoaded( Game game ) { }
		
		public override void Render( double delta ) {
			if( game.HideGui ) return;
			
			bool showMinimal = game.ActiveScreen.BlocksWorld;
			if( chat.HandlesAllInput && !game.PureClassic )
				chat.RenderBackground();
			if( !showMinimal )
				RenderHotbar( delta );
			api.Texturing = true;
			chat.Render( delta );
			
			//graphicsApi.BeginVbBatch( VertexFormat.Pos3fTex2fCol4b );
			//graphicsApi.BindTexture( game.TerrainAtlas.TexId );
			//IsometricBlockDrawer.Draw( game, (byte)Block.Brick, 30, game.Width - 50, game.Height - 20 );
			
			if( playerList != null && game.ActiveScreen == this ) {
				playerList.Render( delta );
				// NOTE: Should usually be caught by KeyUp, but just in case.
				if( !game.IsKeyDown( KeyBinding.PlayerList ) ) {
					playerList.Dispose();
					playerList = null;
				}
			}
			
			api.Texturing = false;
			if( playerList == null && !showMinimal )
				DrawCrosshairs();
		}
		
		public void RenderHotbar( double delta ) { hotbar.Render( delta ); }
		
		const int chExtent = 16, chWeight = 2;
		static TextureRec chRec = new TextureRec( 0, 0, 16/256f, 16/256f );
		void DrawCrosshairs() {
			int cenX = game.Width / 2, cenY = game.Height / 2;
			if( game.IconsTex > 0 ) {
				api.Texturing = true;
				int extent = (int)(chExtent * game.Scale( game.Height / 480f ) );
				Texture chTex = new Texture( game.IconsTex, cenX - extent, 
				                            cenY - extent, extent * 2, extent * 2, chRec );
				chTex.Render( api );
				api.Texturing = false;
				return;
			}
			
			int curCol = 150 + (int)(50 * Math.Abs( Math.Sin( game.accumulator ) ));
			FastColour col = new FastColour( curCol, curCol, curCol );
			api.Draw2DQuad( cenX - chExtent, cenY - chWeight, chExtent * 2, chWeight * 2, col );
			api.Draw2DQuad( cenX - chWeight, cenY - chExtent, chWeight * 2, chExtent * 2, col );
		}
		
		public override void Dispose() {
			playerFont.Dispose();
			chat.Dispose();
			hotbar.Dispose();
			if( playerList != null )
				playerList.Dispose();
		}
		
		public void GainFocus() {
			game.CursorVisible = false;
			game.Camera.RegrabMouse();
		}
		
		public void LoseFocus() {
			game.CursorVisible = true;
		}
		
		public override void OnResize( int oldWidth, int oldHeight, int width, int height ) {
			PlayerListWidget widget = playerList;		
			game.RefreshHud();
			if( widget != null )
				CreatePlayerListWidget();
		}
		
		public override void Init() {
			int size = game.Drawer2D.UseBitmappedChat ? 16 : 11;
			playerFont = new Font( game.FontName, size );
			chat = new ChatScreen( game );
			chat.Init();
			hotbar = new BlockHotbarWidget( game );
			hotbar.Init();
			game.WorldEvents.OnNewMap += OnNewMap;
		}

		void OnNewMap( object sender, EventArgs e ) {
			if( playerList != null )
				playerList.Dispose();
			playerList = null;
		}
		
		public override bool HandlesAllInput { get { return chat.HandlesAllInput; } }

		public override bool HandlesKeyPress( char key ) {
			return chat.HandlesKeyPress( key );
		}
		
		public override bool HandlesKeyDown( Key key ) {
			Key playerListKey = game.Mapping( KeyBinding.PlayerList );
			bool handles = playerListKey != Key.Tab || !game.TabAutocomplete || !chat.HandlesAllInput;
			if( key == playerListKey && handles ) {
				if( playerList == null && !game.Network.IsSinglePlayer )
					CreatePlayerListWidget();
				return true;
			}
			
			if( chat.HandlesKeyDown( key ) )
				return true;
			return hotbar.HandlesKeyDown( key );
		}
		
		void CreatePlayerListWidget() {
			if( game.UseClassicTabList ) {
				playerList = new ClassicPlayerListWidget( game, playerFont );
			} else if( game.Network.UsingExtPlayerList ) {
				playerList = new ExtPlayerListWidget( game, playerFont );
			} else {
				playerList = new NormalPlayerListWidget( game, playerFont );
			}
			playerList.Init();
			playerList.MoveTo( playerList.X, game.Height / 4 );
		}
		
		public override bool HandlesKeyUp( Key key ) {
			if( key == game.Mapping( KeyBinding.PlayerList ) ) {
				if( playerList != null ) {
					playerList.Dispose();
					playerList = null;
					return true;
				}
			}
			return false;
		}
		
		public void OpenTextInputBar( string text ) {
			chat.OpenTextInputBar( text );
		}
		
		public override bool HandlesMouseScroll( int delta ) {
			return chat.HandlesMouseScroll( delta );
		}
		
		public override bool HandlesMouseClick( int mouseX, int mouseY, MouseButton button ) {
			if( button != MouseButton.Left || !HandlesAllInput ) return false;
			
			string name;
			if( playerList == null || (name = playerList.GetNameUnder( mouseX, mouseY )) == null )
				return chat.HandlesMouseClick( mouseX, mouseY, button );
			chat.AppendTextToInput( name + " " );
			return true;
		}
	}
}
