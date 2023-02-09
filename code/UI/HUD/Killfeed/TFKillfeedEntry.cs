using Amper.FPS;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TFS2.UI;

partial class TFKillFeedEntry : Panel
{
	public IClient Attacker { get; }
	public IClient Victim { get; }
	public IClient Assister { get; }
	public WeaponData Weapon { get; }
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
	TimeSince TimeSinceCreated { get; set; } = 0;

	/// <summary>
	/// Create death entry
	/// </summary>
	/// <param name="attacker"></param>
	/// <param name="victim"></param>
	/// <param name="assister"></param>
	/// <param name="weapon"></param>
	/// <param name="tags"></param>
	/// <param name="icon"></param>
	public TFKillFeedEntry( IClient attacker, IClient victim, IClient assister, WeaponData weapon, string[] tags, string icon )
	{
		Attacker = attacker;
		Victim = victim;
		Assister = assister;
		Weapon = weapon;
		Tags = tags;
		Icon = icon;
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
		var local = Sandbox.Game.LocalClient;
		bool is_crit = false; 
		bool is_mini_crit = false;

		if(Tags != default)
		{
			Tags.Contains( TFDamageTags.Critical );
			Tags.Contains( TFDamageTags.MiniCritical );
		}
		
		var killIcon = Icon;

		// Local Player Involved?
		var localPlayerInvolved = local == Attacker || local == Victim || local == Assister;
		if ( localPlayerInvolved )
		{
			SetClass( "is_local_player_involved", true );
			LifeTimeMultiplier = 2;
		}

		//
		// Victim
		//
		VictimName.Text = Victim.Name;
		var victimTeam = Victim.GetTeam();
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
		else if ( Attacker.IsValid() )
		{
			// someone else killed us.
			AttackerName.Text = Attacker.Name;

			var attackerTeam = Attacker.GetTeam();
			AttackerName.SetClass( "red", attackerTeam == TFTeam.Red );
			AttackerName.SetClass( "blue", attackerTeam == TFTeam.Blue );
			SetClass( "has_attacker", true );
		}

		//
		// Weapon Icon
		//
		if ( Weapon != null )
		{
			// if we have a weapon, put it's kill icon in the feed.
			var normalIcon = Weapon.KillFeedIcon;
			var specialIcon = Weapon.KillFeedIconSpecial;
			var weaponIcon = normalIcon;

			if ( is_crit && !string.IsNullOrEmpty( specialIcon ) )
				weaponIcon = specialIcon;

			if ( !string.IsNullOrEmpty( weaponIcon ) )
				killIcon = weaponIcon;
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
