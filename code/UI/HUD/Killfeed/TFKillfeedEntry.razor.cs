using Amper.FPS;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TFS2.UI;

partial class TFKillFeedEntry : Panel
{
	public Entity Attacker { get; }
	public Entity Victim { get; }
	public Entity Assister { get; }
	public Entity Weapon { get; }
	public string[] Tags { get; }
	public string Icon { get; set; }
	public string IconRed { get; set; }
	public string IconBlue
	{
		get
		{
			return Icon;
		}
		set
		{
			Icon = value;
		}
	}

	protected ControlPointCapturedEvent CaptureArgs { get; set; }
	public float LifeTimeMultiplier { get; set; } = 1;
	public float LifeTime => hud_deathnotice_time * LifeTimeMultiplier;
	bool showAsInvolved = false;
	TimeSince TimeSinceCreated { get; set; } = 0;
	Label AttackerName;
	Label PostAttackerMessage;
	Label VictimName;
	Label PostVictimMessage;
	Label StreakCounter;
	Image IconElement { get; set; }

	/// <summary>
	/// Create death entry
	/// </summary>
	/// <param name="attacker"></param>
	/// <param name="victim"></param>
	/// <param name="assister"></param>
	/// <param name="weapon"></param>
	/// <param name="tags"></param>
	/// <param name="icon"></param>
	public TFKillFeedEntry( Entity attacker, Entity victim, Entity assister, Entity weapon, string[] tags, string icon, bool involved = false )
	{
		Attacker = attacker;
		Victim = victim;
		Assister = assister;
		Weapon = weapon;
		Tags = tags;
		Icon = icon;
		showAsInvolved = involved;
	}

	/// <summary>
	/// Create capture entry
	/// </summary>
	/// <param name="args"></param>
	/// <param name="iconBlue"></param>
	/// <param name="iconRed"></param>
	public TFKillFeedEntry( ControlPointCapturedEvent args, string iconBlue, string iconRed)
	{
		CaptureArgs = args;
		Icon = iconBlue;
		IconRed = iconRed;
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );
		if(CaptureArgs != null)
		{
			InitCapture();
		}
		else 
		{
			InitKill();
		}
	}

	protected virtual void InitKill()
	{
		var local = Game.LocalClient;
		bool is_crit = false; 
		bool is_mini_crit = false;

		if(Tags != default)
		{
			//Checking if the attack was a critical hit.
			if ( Tags.Contains( TFDamageTags.Critical ) )
			{
				is_crit = true;
			}

			//Checking if the attack was a mini-critical hit.
			if ( Tags.Contains( TFDamageTags.MiniCritical ) )
			{
				is_mini_crit = true;
			}
		}	

		var killIcon = Icon;

		// Local Player Involved?
		var localPlayerInvolved = local == Attacker?.Client || local == Victim.Client || local == Assister?.Client || local.Pawn == Victim.Owner || showAsInvolved;
		if ( localPlayerInvolved )
		{
			SetClass( "is_local_player_involved", true );
			LifeTimeMultiplier = 2;
		}

		//
		// Victim
		//
		// someone else killed us.
		if ( Victim is IKillfeedName killfeedVictim )
			VictimName.Text = killfeedVictim.Name;
		else
			VictimName.Text = Victim.Name;

		var victimTeam = TFTeam.Unassigned;
		if ( Victim is ITeam teamVictim )
			victimTeam = (TFTeam)teamVictim.TeamNumber;
		VictimName.SetClass( "red", victimTeam == TFTeam.Red );
		VictimName.SetClass( "blue", victimTeam == TFTeam.Blue );
		SetClass( "has_victim", true );

		//
		// Attacker
		//
		if ( Tags?.Contains( DamageTags.Fall ) == true )
			// If there is no attacker that means the world has killed us.
			PostVictimMessage.Text = " fell to a clumsy, painful death";
		else if ( Victim == Attacker && Weapon == null )
			// attacker has dealt damage to themselves with no weapon, they must have suicided.
			PostVictimMessage.Text = " bid farewell, cruel world";
		else if ( Victim == Attacker )
			// Attacker blew himself up or etc.
			AttackerName = null;
		else if ( Attacker.IsValid() )
		{
			// someone else killed us.
			if(Attacker is IKillfeedName killfeedAttacker)
				AttackerName.Text = killfeedAttacker.Name;
			else
				AttackerName.Text = Attacker.Name;

			var attackerTeam = TFTeam.Unassigned;
			if ( Attacker is ITeam teamAttacker )
				attackerTeam = (TFTeam)teamAttacker.TeamNumber;

			AttackerName.SetClass( "red", attackerTeam == TFTeam.Red );
			AttackerName.SetClass( "blue", attackerTeam == TFTeam.Blue );
				
			SetClass( "has_attacker", true );
		}

		//
		// Weapon Icon
		//
		if(Weapon is IKillfeedIcon p)
		{
			string inflictorIcon = p.GetIcon( is_crit, Tags );
			if ( !string.IsNullOrEmpty( inflictorIcon ) )
				killIcon = inflictorIcon;
		}

		// If local player is involved, we need to switch the image to an inverted variation.
		if ( localPlayerInvolved )
		{
			var noExtension = Path.Combine( Path.GetDirectoryName( killIcon ), Path.GetFileNameWithoutExtension( killIcon ) );
			var extension = Path.GetExtension( killIcon );

			killIcon = $"{noExtension}_neg{extension}";
		}

		IconElement.SetTexture( Util.JPGToPNG( killIcon ) );

		//
		// Backdrop
		//
		SetClass( "is_crit", is_crit );
		SetClass( "is_minicrit", is_mini_crit );
	}
	protected virtual void InitCapture()
	{
		var cappers = CaptureArgs.Cappers;
		var newTeam = CaptureArgs.NewTeam;

		if ( cappers.Contains( Sandbox.Game.LocalClient ) )
		{
			SetClass( "is_local_player_involved", true );
			LifeTimeMultiplier = 2;
		}
		
		// Set the control point name.
		VictimName.Text = $"Captured the {CaptureArgs.Point.PrintName}";
		SetClass( "has_victim", true );

		// Set the cappers.
		AttackerName.SetClass( "red", newTeam == TFTeam.Red );
		AttackerName.SetClass( "blue", newTeam == TFTeam.Blue );
		AttackerName.Text = string.Join( ", ", cappers.Select( x => x.Name ) );
		SetClass( "has_attacker", true );

		// Set the icon. This will fallback to red icon in case spectators somehow end up being put as point owners.
		IconElement.SetTexture( newTeam == TFTeam.Blue ? IconBlue : IconRed );
	}


	public override void Tick()
	{
		var texture = IconElement.Texture;

		if ( texture != null )
		{
			float width = texture.Width;
			float height = texture.Height;
			var aspectRatio = width / height;

			IconElement.Style.AspectRatio = aspectRatio;
		}

		if ( TimeSinceCreated > LifeTime )
			Delete();
	}
	[ConVar.Client] public static float hud_deathnotice_time { get; set; } = 6;
}
