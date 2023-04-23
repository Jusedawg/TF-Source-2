using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace TFS2;

public partial class VoiceMenu : Panel
{
	[ConVar.Client] public static float hud_voicemenu_dismiss_time { get; set; } = 20;
	public static VoiceMenu Current { get; set; }

	public static readonly List<List<(TFResponseConcept, string)>> PageDefinition = new()
	{
		// Voice Menu 1
		new()
		{
			( TFResponseConcept.VoiceMedic,             "MEDIC!" ),
			( TFResponseConcept.VoiceThanks,            "Thanks!" ),
			( TFResponseConcept.VoiceGo,                "Go Go Go!" ),
			( TFResponseConcept.VoiceMoveUp,            "Move Up!" ),
			( TFResponseConcept.VoiceGoLeft,            "Go Left" ),
			( TFResponseConcept.VoiceGoRight,           "Go Right" ),
			( TFResponseConcept.VoiceYes,               "Yes" ),
			( TFResponseConcept.VoiceNo,                "No" )
		},
		// Voice Menu 2
		new()
		{
			( TFResponseConcept.VoiceIncoming,          "Incoming" ),
			( TFResponseConcept.VoiceSpy,               "Spy!" ),
			( TFResponseConcept.VoiceSentryAhead,       "Sentry Ahead!" ),
			( TFResponseConcept.VoiceTeleporterHere,    "Teleporter Here" ),
			( TFResponseConcept.VoiceDispenserHere,     "Dispenser Here" ),
			( TFResponseConcept.VoiceSentryHere,        "Sentry Here" ),
			( TFResponseConcept.VoiceActivateUbercharge,"Activate ÜberCharge!" ),
			( TFResponseConcept.VoiceUberchargeReady,   "MEDIC: ÜberCharge Ready" )
		},
		// Voice Menu 3
		new()
		{
			( TFResponseConcept.VoiceHelp,              "Help!" ),
			( TFResponseConcept.VoiceBattleCry,         "Battle Cry" ),
			( TFResponseConcept.VoiceCheers,            "Cheers" ),
			( TFResponseConcept.VoiceJeers,             "Jeers" ),
			( TFResponseConcept.VoicePositive,          "Positive" ),
			( TFResponseConcept.VoiceNegative,          "Negative" ),
			( TFResponseConcept.VoiceNiceShot,          "Nice Shot" ),
			( TFResponseConcept.VoiceGoodJob,           "Good Job" )
		}
	};

	readonly Dictionary<string, int> SlotButtons = new()
	{
		{ "Slot1", 0 },
		{ "Slot2", 1 },
		{ "Slot3", 2 },
		{ "Slot4", 3 },
		{ "Slot5", 4 },
		{ "Slot6", 5 },
		{ "Slot7", 6 },
		{ "Slot8", 7 },
		{ "Slot9", 8 }
	};

	
	bool Shown;
	int ActivePage;
	public float? AutoDismissTime;
	List<VoiceMenuPage> Pages = new();

	public VoiceMenu()
	{
		Current = this;
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		var i = 0;
		foreach ( var pageDef in PageDefinition )
		{
			i++;
			var pagePanel = AddPage( $"Voice Menu {i}" );

			var j = 0;
			foreach ( var pair in pageDef )
			{
				j++;
				var concept = pair.Item1;
				var name = pair.Item2;
				pagePanel.Add.Label( $"{j}. {name}", "option" );
			}

			Pages.Add( pagePanel );
		}
	}

	public VoiceMenuPage AddPage( string name )
	{
		if(PageContainer == null)
			return null;

		var page = PageContainer.AddChild<VoiceMenuPage>();
		page.Add.Label( name, "header" );
		return page;
	}

	public override void Tick()
	{
		if ( !Shown )
			return;

		if ( AutoDismissTime.HasValue && Time.Now >= AutoDismissTime )
			Close();
	}

	public void SwitchToPage( int index )
	{
		if ( Pages.Count == 0 )
			return;

		index = Math.Clamp( index, 0, Pages.Count - 1 );

		for ( var i = 0; i < Pages.Count; i++ )
		{
			var page = Pages[i];

			if ( index == i )
				page.Show();
			else
				page.Hide();
		}

		ActivePage = index;
		ExtendAutoDismissTime();
	}

	public void Show( int menu = 0 )
	{
		//InfoContainer.SetClass( "visible", Input.GetButtonOrigin( InputButton.View ) != null );
		Shown = true;
		AddClass( "visible" );
		SwitchToPage( menu );
	}

	public void Toggle( int menu = 0 )
	{
		// If we're trying to open the currently active page...
		if ( Shown && ActivePage == menu )
			Close(); // just close the menu.
		else
			Show( menu );
	}

	public void NextPage()
	{
		if ( !Shown )
		{
			Show();
			return;
		}

		var nextPage = ActivePage + 1;
		if ( nextPage >= Pages.Count )
			nextPage = 0;

		SwitchToPage( nextPage );
	}

	public void Close()
	{
		if ( !Shown )
			return;

		RemoveClass( "visible" );
		Shown = false;
		AutoDismissTime = null;
	}

	public void ExtendAutoDismissTime()
	{
		AutoDismissTime = null;

		if ( hud_voicemenu_dismiss_time <= 0 )
			return;

		AutoDismissTime = Time.Now + hud_voicemenu_dismiss_time;
	}

	[Event.Client.BuildInput]
	public void ProcessClientInput()
	{
		if ( Input.Pressed( "VoiceMenu1" ) )
			Toggle(0);
		else if ( Input.Pressed( "VoiceMenu2" ) )
			Toggle(1);
		else if ( Input.Pressed( "VoiceMenu3" ) )
			Toggle( 2 );

		if ( !Shown )
			return;

		// If we've pressed Slot0 close the menu right away, this button means cancel.
		if ( Input.Pressed( "Slot10" ) )
		{
			Input.Clear( "Slot10" );
			Close();
			return;
		}

		foreach ( var pair in SlotButtons )
		{
			if ( Input.Pressed( pair.Key ) )
			{
				ButtonPressed( pair.Key );
				Input.Clear( pair.Key );
			}
		}
	}

	public void ButtonPressed( string button )
	{
		if ( !Shown )
			return;

		if ( !SlotButtons.TryGetValue( button, out var index ) )
			return;

		ConsoleSystem.Run( "voicemenu", ActivePage, index );
		Close();
	}

	[ConCmd.Client( "voice_menu" )]
	public static void Command_VoiceMenu( int menu )
	{
		Current?.Toggle( menu );
	}
}
