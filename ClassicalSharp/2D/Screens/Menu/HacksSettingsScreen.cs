﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using System.Drawing;
using ClassicalSharp.Entities;
using ClassicalSharp.Singleplayer;

namespace ClassicalSharp.Gui {
	
	public class HacksSettingsScreen : MenuOptionsScreen {
		
		public HacksSettingsScreen( Game game ) : base( game ) {
		}
		
		public override void Init() {
			base.Init();
			
			widgets = new Widget[] {
				// Column 1
				MakeBool( -1, -150, "Hacks enabled", OptionsKey.HacksEnabled,
				     OnWidgetClick, g => g.LocalPlayer.Hacks.Enabled,
				     (g, v) => { g.LocalPlayer.Hacks.Enabled = v;
				     	g.LocalPlayer.CheckHacksConsistency(); } ),
				
				MakeOpt( -1, -100, "Speed multiplier", OnWidgetClick,
				     g => g.LocalPlayer.Hacks.SpeedMultiplier.ToString( "F2" ),
				     (g, v) => { g.LocalPlayer.Hacks.SpeedMultiplier = Single.Parse( v );
				     	Options.Set( OptionsKey.Speed, v ); } ),
				
				MakeBool( -1, -50, "Camera clipping", OptionsKey.CameraClipping,
				     OnWidgetClick, g => g.CameraClipping, (g, v) => g.CameraClipping = v ),
				
				MakeOpt( -1, 0, "Jump height", OnWidgetClick,
				     g => g.LocalPlayer.JumpHeight.ToString( "F3" ),
				     (g, v) => g.LocalPlayer.physics.CalculateJumpVelocity( true, Single.Parse( v ) ) ),
				
				MakeBool( -1, 50, "Double jump", OptionsKey.DoubleJump,
				     OnWidgetClick, g => g.LocalPlayer.Hacks.DoubleJump,
				     (g, v) => g.LocalPlayer.Hacks.DoubleJump = v ),
				
				// Column 2
				MakeBool( 1, -150, "Full block stepping", OptionsKey.FullBlockStep,
				     OnWidgetClick, g => g.LocalPlayer.Hacks.FullBlockStep,
				     (g, v) => g.LocalPlayer.Hacks.FullBlockStep = v ),
				
				MakeBool( 1, -100, "Modifiable liquids", OptionsKey.ModifiableLiquids,
				         OnWidgetClick, g => g.ModifiableLiquids, (g, v) => g.ModifiableLiquids = v ),
				
				MakeBool( 1, -50, "Pushback placing", OptionsKey.PushbackPlacing,
				     OnWidgetClick, g => g.LocalPlayer.Hacks.PushbackPlacing,
				     (g, v) => g.LocalPlayer.Hacks.PushbackPlacing = v ),
				
				MakeBool( 1, 0, "Noclip slide", OptionsKey.NoclipSlide,
				     OnWidgetClick, g => g.LocalPlayer.Hacks.NoclipSlide,
				     (g, v) => g.LocalPlayer.Hacks.NoclipSlide = v ),
				
				MakeOpt( 1, 50, "Field of view", OnWidgetClick,
				     g => g.Fov.ToString(),
				     (g, v) => { g.Fov = Int32.Parse( v );
				     	Options.Set( OptionsKey.FieldOfView, v );
				     	g.UpdateProjection();
				     } ),
				
				MakeBack( false, titleFont,
				     (g, w) => g.Gui.SetNewScreen( new OptionsGroupScreen( g ) ) ),
				null, null,
			};
			
			MakeValidators();
			MakeDescriptions();
			game.Events.HackPermissionsChanged += CheckHacksAllowed;
			CheckHacksAllowed( null, null );
		}
		
		void CheckHacksAllowed( object sender, EventArgs e ) { 
			for( int i = 0; i < widgets.Length; i++ ) {
				if( widgets[i] == null ) continue;
				widgets[i].Disabled = false;
			}
			
			LocalPlayer p = game.LocalPlayer;
			bool noGlobalHacks = !p.Hacks.CanAnyHacks || !p.Hacks.Enabled;
			widgets[3].Disabled = noGlobalHacks || !p.Hacks.CanSpeed;
			widgets[4].Disabled = noGlobalHacks || !p.Hacks.CanSpeed;
			widgets[5].Disabled = noGlobalHacks || !p.Hacks.CanSpeed;
			widgets[7].Disabled = noGlobalHacks || !p.Hacks.CanPushbackBlocks;
		}
		
		public override void Dispose() {
			base.Dispose();
			game.Events.HackPermissionsChanged -= CheckHacksAllowed;
		}
		
		void MakeValidators() {
			validators = new MenuInputValidator[] {
				new BooleanValidator(),
				new RealValidator( 0.1f, 50 ),
				new BooleanValidator(),			
				new RealValidator( 0.1f, 1024f ),
				new BooleanValidator(),
				
				new BooleanValidator(),
				new BooleanValidator(),
				new BooleanValidator(),
				new BooleanValidator(),
				new IntegerValidator( 1, 150 ),				
			};
		}
		
		void MakeDescriptions() {
			descriptions = new string[widgets.Length][];
			descriptions[2] = new [] {
				"&eIf camera clipping is set to true, then the third person",
				"&ecameras will limit their zoom distance if they hit a solid block.",
			};
			descriptions[3] = new [] {
				"&eSets how many blocks high you can jump up.",
				"&eNote: You jump much higher when holding down the speed key binding.",
			};
			descriptions[6] = new [] {
				"&eIf modifiable liquids is set to true, then water/lava can",
				"&ebe placed and deleted the same way as any other block.",
			};
			descriptions[7] = new [] {
				"&eWhen this is active, placing blocks that intersect your own position",
				"&ecause the block to be placed, and you to be moved out of the way.",
				"&fThis is mainly useful for quick pillaring/towering.",
			};
			descriptions[8] = new [] {
				"&eIf noclip sliding isn't used, you will immediately stop when",
				"&eyou are in noclip mode and no movement keys are held down.",
			};
		}
	}
}