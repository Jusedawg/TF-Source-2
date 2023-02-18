using Sandbox;
using System;
using System.Collections.Generic;

namespace TFS2;

partial class TFPlayer
{
	//General Taunt Vars

	[Net, Predicted]
	public TauntData ActiveTaunt { get; set; }

	[Net]
	public AnimatedEntity TauntPropModel { get; set; }

	/// <summary>
	/// Currently unused, for taunts with win-lose conditions instead of initiator-partner conditions, like RPS
	/// </summary>
	[Net, Predicted]
	public bool TauntWin { get; set; }

	[Net]
	public bool TauntEnableMove { get; set; }

	/// <summary>
	/// Timer for simple taunts
	/// </summary>
	[Net, Predicted]
	public TimeSince TimeSinceTaunt { get; set; }

	[Net, Predicted]
	public float TauntDuration { get; set; } = 1f;

	[Net]
	public bool TauntsReset { get; set; }

	//Weapon Taunt Vars

	///<summary>
	/// Timer for doubletap taunt function
	///</summary>
	[Net, Predicted]
	public TimeSince TimeSinceTauntMenuClose { get; set; }

	/// <summary>
	/// Timeframe for doubletap weapon taunts
	/// </summary>
	public const float WeaponTauntTimeframe = 0.5f;

	[Net, Predicted]
	public bool WeaponTauntAvailable { get; set; }

	//Partner Taunt Vars

	[Net, Predicted]
	public TFPlayer PartnerTarget { get; set; }

	/// <summary>
	/// How far to consider a hovered player for a valid partner, players bounds width multiplied by 2.5
	/// </summary>
	public const float PartnerDistance = 120f;

	[Net]
	public bool WaitingForPartner { get; set; }

	//Per-player list of taunts
	[Net]
	public IList<TauntData> TauntList { get; set; }
	public void CreateTauntList()
	{
		Log.Info($"creating tauntlist for {PlayerClass.Title.ToLower()}");

		//Reset our tauntlist
		TauntList.Clear();

		var classname = PlayerClass.Title.ToLower();
		TFPlayerClass classkey = TFPlayerClass.Undefined;

		foreach ( KeyValuePair<TFPlayerClass, string> pair in PlayerClass.Names )
		{
			if ( pair.Value == classname )
			{
				classkey = pair.Key;
			}
		}

		// Add taunt to Class' taunt list if Undefined (aka Allclass)
		foreach ( var taunt in TauntData.AllActive )
		{
			if ( taunt.Class == TFPlayerClass.Undefined )
			{
				TauntList.Add( taunt );
			}
		}

		// Separate so ALLCLASS taunts cycle first, this is important because we cannot dynamically match a playermodel's tauntlist enum, so it MUST match up
		// Sorting alphabetically in separated groups is the only way to do this

		// Add taunt to Class' taunt list if it belongs to this class
		foreach ( var taunt in TauntData.AllActive )
		{
			if ( taunt.Class == classkey )
			{
				TauntList.Add( taunt );
			}
		}
	}

	/// <summary>
	/// Taunt Logic check called under TFPlayer.Simulate
	/// </summary>
	public void SimulateTaunts()
	{
		if ( PlayerClass == null ) return;

		//When taunt menu is closed via release, set bool that allows doublepress taunt
		if ( Input.Released( InputButton.Drop ) && !WeaponTauntAvailable && !InCondition( TFCondition.Taunting ) )
		{
			TimeSinceTauntMenuClose = 0;
			WeaponTauntAvailable = true;
		}

		//Resets doubletap bool if time elapsed since taunt menu has closed
		if ( TimeSinceTauntMenuClose > WeaponTauntTimeframe )
		{
			WeaponTauntAvailable = false;
		}

		//I believe this code can be rewritten better, I just don't remember how
		if ( HoveredEntity != null )
		{
			if ( HoveredDistance < PartnerDistance ) //player bounds width * 2.5
				PartnerTarget = HoveredEntity as TFPlayer;
			else
				PartnerTarget = null;
		}

		//If taunt menu button is pressed before certain time elapses, check for Partner/Group taunts, if none play weapon taunt
		if ( Input.Pressed( InputButton.Drop ) && WeaponTauntAvailable && !InCondition( TFCondition.Taunting ) )
		{
			if ( TryDoubleTapTaunt() ) return;
		}

		if ( !InCondition( TFCondition.Taunting ) && !TauntsReset )
		{
			ActiveTaunt = null;
			Animator?.SetAnimParameter( "b_taunt", false );
			Animator?.SetAnimParameter( "b_taunt_partner", false );
			Animator?.SetAnimParameter( "b_taunt_initiator", false );
			TauntsReset = true;
		}

		//Check to see if we are somehow in taunt condition without a taunt set
		if ( ActiveTaunt == null ) return;

		if ( InCondition( TFCondition.Taunting ) )
		{
			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				Animator?.SetAnimParameter( "b_fire", true );
			}

			if ( Input.Pressed( InputButton.SecondaryAttack ) )
			{
				Animator?.SetAnimParameter( "b_fire_secondary", true );
			}

			//Call a fake partner accept
			if ( ActiveTaunt.TauntType == TauntType.Partner && Input.Pressed( InputButton.Use ) )
			{
				AcceptPartnerTaunt( true );
			}
			if ( ActiveTaunt.TauntType == TauntType.Partner && !WaitingForPartner )
			{
				if ( TimeSinceTaunt > TauntDuration )
				{
					StopTaunt();
				}
			}
			//Stop Taunt via duration
			if ( ActiveTaunt.TauntType == TauntType.Once && TimeSinceTaunt > TauntDuration )
			{
				StopTaunt();
			}
			
			//Stop Taunt via loss of grounded state
			if ( ActiveTaunt.TauntType != TauntType.Looping && GroundEntity == null )
			{
				StopTaunt();
			}

			//Stop Taunt via button press
			if ( Input.Pressed( InputButton.Drop ) && (ActiveTaunt.TauntType == TauntType.Looping || (ActiveTaunt.TauntType == TauntType.Partner && WaitingForPartner)) )
			{
				StopTaunt();
			}
		}
	}

	public bool CanTaunt()
	{
		if ( !IsGrounded ) return false;
		if ( InCondition( TFCondition.Taunting ) ) return false;

		return true;
	}

	public bool TryDoubleTapTaunt()
	{
		if ( TryJoinTaunt() ) return true;
		if ( TryWeaponTaunt() ) return true;
		return false;
	}

	/// <summary>
	/// Attempt to join a partner or group taunt
	/// </summary>
	/// <returns></returns>
	public bool TryJoinTaunt()
	{
		if ( PartnerTarget != null && PartnerTarget.InCondition( TFCondition.Taunting ) )
		{
			// Partner Taunt
			if ( PartnerTarget.ActiveTaunt.TauntType == TauntType.Partner && PartnerTarget.WaitingForPartner == true && IsPartnerTauntAngleValid( PartnerTarget ) )
			{
				WeaponTauntAvailable = false;
				ActiveTaunt = PartnerTarget.ActiveTaunt;
				PartnerSetLocation( PartnerTarget );
				AcceptPartnerTaunt( false );
				PartnerTarget.AcceptPartnerTaunt( true );
				return true;
			}
			// Group taunt
			else if ( PartnerTarget.ActiveTaunt.TauntType == TauntType.Looping && PartnerTarget.ActiveTaunt.TauntAllowJoin == true )
			{
				WeaponTauntAvailable = false;
				ActiveTaunt = PartnerTarget.ActiveTaunt;
				PlayTaunt( ActiveTaunt );
				return true;
			}
		}
		return false;
	}


	/// <summary>
	/// Attempt to start a weapon-specific taunt
	/// </summary>
	/// <returns></returns>
	public bool TryWeaponTaunt()
	{
		var weapon = ActiveWeapon as TFWeaponBase;
		var Tauntdata = TauntData.Get(weapon.Data.TauntData);
		var TauntName = weapon.Data.TauntString; 

		WeaponTauntAvailable = false;
		TimeSinceTaunt = 0;

		if ( Tauntdata != null )
		{
			PlayTaunt( Tauntdata );
			return true;
		}
		if ( !String.IsNullOrEmpty( TauntName ) )
		{
			PlayTaunt( TauntName );
			return true;
		}
		return false;
	}

	public void ApplyTauntConds()
	{
		(Animator as TFPlayerAnimator)?.SetAnimParameter( "b_taunt", true );

		Velocity = 0f;
		Rotation = Animator.GetIdealRotation();

		AddCondition( TFCondition.Taunting );
		TauntEnableMove = false;
		TauntsReset = false;
	}

	/// <summary>
	/// Play the selected taunt (by asset)
	/// </summary>
	public void PlayTaunt( TauntData taunt, bool initiator = true )
	{
		ActiveTaunt = taunt;
		Log.Info($"Active Taunt: {ActiveTaunt.ResourceName}");
		var animcontroller = Animator as TFPlayerAnimator;
		var TauntType = taunt.TauntType;
		var TauntIndex = TauntList.IndexOf( ActiveTaunt );  //Find way to dynamically assign, right now it MUST line up to animgraph

		if ( !CanTaunt() ) return;

		if ( TauntType == TauntType.Partner )
		{
			//If we are starting the partner taunt, we need to check for valid spacing
			if ( initiator )
			{
				if ( !CanInitiatePartnerTaunt() )
				{
					Log.Info( "Not enough space for a partner" );
					return;
				}
			}
		}

		if ( TauntType == TauntType.Once )
		{
			TimeSinceTaunt = 0;
			TauntDuration = GetSequenceDuration( ActiveTaunt.SequenceName );
		}

		if ( !string.IsNullOrEmpty( ActiveTaunt.TauntPropModel ) )
		{
			CreateTauntProp( ActiveTaunt, this );
		}

		if ( !string.IsNullOrEmpty( taunt.TauntMusic ) )
		{
			StartMusic();
		}

		animcontroller?.SetAnimParameter( "taunt_name", TauntIndex );
		animcontroller?.SetAnimParameter( "taunt_type", (int)TauntType );

		StayThirdperson = IsThirdperson;
		ThirdpersonSet( true );

		ApplyTauntConds();
	}

	/// <summary>
	/// Play the selected taunt (by string)
	/// </summary>
	/// <param name="taunt_name"></param>
	public void PlayTaunt( string taunt_name )
	{
		Log.Info($"Attempting {taunt_name} via string");
		TauntData taunt = null;

		//Searches through enabled taunts to find the appropriate taunt data and assigns it
		foreach ( TauntData data in TauntList )
		{
			if ( data.ResourceName == taunt_name )
				taunt = data;
		}

		//Solves different animations for shared weapons
		if ( taunt_name == "weapon_shared_pistol" )
		{
			taunt = TauntData.Get( $"weapon_{PlayerClass.Title}_secondary" );
		}
		else if ( taunt_name == "weapon_shared_shotgun" )
		{
			var slot = "secondary";

			//Special Case for engineer, since shotgun is his primary
			if ( PlayerClass.Title == "engineer" )
			{
				slot = "primary";
			}

			taunt = TauntData.Get( $"weapon_{PlayerClass.Title}_{slot}" );

		}
		else
		{
			taunt = TauntData.Get( taunt_name );
		}
		

		//Because the string-to-tauntdata assignment can possibly fail, we need to check before running taunt code
		if ( taunt != null )
		{
			ActiveTaunt = taunt;
			PlayTaunt( taunt );
		}
		else
			Log.Info( "Taunt returned null via string assignment" );
	}

	/// <summary>
	/// Stops and resets all taunt related variables
	/// </summary>
	public void StopTaunt()
	{
		var weapon = ActiveWeapon as TFWeaponBase;

		RemoveCondition( TFCondition.Taunting );

		Rotation = Animator.GetIdealRotation();
		Animator.SetAnimParameter( "b_taunt", false );
		Animator.SetAnimParameter( "b_taunt_partner", false );
		Animator.SetAnimParameter( "b_taunt_initiator", false );
		TauntEnableMove = false;
		WaitingForPartner = false;

		StopMusic();

		if ( TauntPropModel != null && Game.IsServer )
			TauntPropModel.Delete();
		if ( weapon != null && weapon.EnableDrawing == false )
			weapon.EnableDrawing = true;
		if ( !StayThirdperson )
			ThirdpersonSet(false);
	}

	#region Partner Taunt Logic

	/// <summary>
	/// Tells game if we're ready to accept a partner
	/// </summary>
	public bool CanInitiatePartnerTaunt()
	{
		if ( PartnerTauntIsSpaceValid() )
		{
			WaitingForPartner = true;
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Checks to see if we can move a player in front of initiator
	/// </summary>
	public bool PartnerTauntIsSpaceValid()
	{
		var positionShiftUp = this.Position;
		positionShiftUp.z += 42;
		var validateFrom = positionShiftUp + this.Rotation.Forward * 24;
		var validateTo = positionShiftUp + this.Rotation.Forward * 68;
		var tr = PartnerValidateTrace( validateFrom, validateTo ).Run();

		/*
		if ( tf_sv_debug_taunts )
		{
			DebugOverlay.Line( tr.StartPosition, tr.EndPosition, IsServer ? Color.Yellow : Color.Green, 15f, true );
			DebugOverlay.Box( tr.EndPosition, new Vector3( -24, -24, -41f ), new Vector3( 24, 24, 41 ), Color.Cyan, 15f, true );
			DebugOverlay.Sphere( tr.EndPosition, 2f, Color.Red, true, 15f );
			DebugOverlay.Sphere( tr.StartPosition, 2f, Color.Green, true, 15f );
			DebugOverlay.Text( tr.EndPosition, $"{tr.Distance}", 15f );
		}*/

		// Did we hit something?
		if ( tr.Hit )
		{
			return false;
		}
		var validateToDown = validateTo;
		validateToDown.z -= 10;
		var trDown = PartnerValidateTrace( validateTo, validateToDown ).Run();
		// If no ground or ground height is too low, no need to check if height is too high because of first trace
		if ( !trDown.Hit || (trDown.Hit && validateTo.Distance( trDown.EndPosition ) > 5f) )
		{
			return false;
		}
		else
		{
			return true;
		}
	}
	public virtual Trace PartnerValidateTrace( Vector3 Origin, Vector3 Target )
	{
		var collBox = new Vector3( 48, 48, 82 );
		var tr = Trace.Box( collBox, Origin, Target )
			.WorldOnly();

		return tr;
	}

	/// <summary>
	/// Checks to see if we are within a certain angle opposite of the target
	/// </summary>
	public bool IsPartnerTauntAngleValid( TFPlayer target )
	{
		var targetYaw = target.Rotation.Yaw();
		float idealYaw;
		if ( targetYaw >= 0 )
		{
			idealYaw = targetYaw - 180;
		}
		else
		{
			idealYaw = targetYaw + 180;
		}
		var angleDiff = Math.Abs( Rotation.Yaw() - idealYaw );
		var angleDiffMax = 50f;

		if ( angleDiff <= angleDiffMax )
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Sets player location for partner taunts
	/// </summary>
	public void PartnerSetLocation( TFPlayer target )
	{
		var distance = 68;
		Vector3 moveTo = target.Position + target.Rotation.Forward * distance;
		var rotateTo = Rotation.FromYaw( target.Rotation.Yaw() - 180f );

		Position = moveTo;
		Rotation = rotateTo;
	}

	/// <summary>
	/// Accepts partner taunt
	/// </summary>
	public void AcceptPartnerTaunt( bool isInitiator )
	{
		var animcontroller = Animator as TFPlayerAnimator;
		var player = this;

		if ( !isInitiator )
		{
			PlayTaunt( ActiveTaunt, false );
			animcontroller?.SetAnimParameter( "b_taunt_partner", true );
		}
		else
		{
			animcontroller?.SetAnimParameter( "b_taunt_initiator", true );
		}
		TimeSinceTaunt = 0;
		GetPartnerDuration( player, isInitiator );
		WaitingForPartner = false;
	}

	/* //Disabled, yandere-dev moment, need to find a better way to relay class-dependent durations or need to squash them
	/// <summary>
	/// Hardcoded references for partner exit durations, values vary too much per class and state, variables are too complex for simpler dynamic code
	/// </summary>
	public void GetPartnerDuration( TFPlayer player, bool initiator)
	{
		var taunt = ActiveTaunt.AnimName;
		var tauntDurationVar = 0f;
		if ( PlayerClass.Name == "scout" )
		{
			if (taunt == "taunt_highfive")
			{
				tauntDurationVar = 4.17f;
			}
			if ( taunt == "taunt_dosido" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if (taunt == "taunt_flip")
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_rps" )
			{
				if ( TauntWin == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
		}
		if ( PlayerClass.Name == "soldier" )
		{
			if ( taunt == "taunt_highfive" )
			{
				tauntDurationVar = 4.17f;
			}
			if ( taunt == "taunt_dosido" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_flip" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_rps" )
			{
				if ( TauntWin == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
		}
		if ( PlayerClass.Name == "pyro" )
		{
			if ( taunt == "taunt_highfive" )
			{
				tauntDurationVar = 4.17f;
			}
			if ( taunt == "taunt_dosido" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_flip" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_rps" )
			{
				if ( TauntWin == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
		}
		if ( PlayerClass.Name == "demoman" )
		{
			if ( taunt == "taunt_highfive" )
			{
				tauntDurationVar = 4.17f;
			}
			if ( taunt == "taunt_dosido" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_flip" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_rps" )
			{
				if ( TauntWin == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
		}
		if ( PlayerClass.Name == "heavy" )
		{
			if ( taunt == "taunt_highfive" )
			{
				tauntDurationVar = 4.17f;
			}
			if ( taunt == "taunt_dosido" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_flip" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_rps" )
			{
				if ( TauntWin == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
		}
		if ( PlayerClass.Name == "engineer" )
		{
			if ( taunt == "taunt_highfive" )
			{
				tauntDurationVar = 4.17f;
			}
			if ( taunt == "taunt_dosido" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_flip" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_rps" )
			{
				if ( TauntWin == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
		}
		if ( PlayerClass.Name == "medic" )
		{
			if ( taunt == "taunt_highfive" )
			{
				tauntDurationVar = 4.17f;
			}
			if ( taunt == "taunt_dosido" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_flip" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_rps" )
			{
				if ( TauntWin == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
		}
		if ( PlayerClass.Name == "sniper" )
		{
			if ( taunt == "taunt_highfive" )
			{
				tauntDurationVar = 4.17f;
			}
			if ( taunt == "taunt_dosido" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_flip" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_rps" )
			{
				if ( TauntWin == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
		}
		if ( PlayerClass.Name == "spy" )
		{
			if ( taunt == "taunt_highfive" )
			{
				tauntDurationVar = 4.17f;
			}
			if ( taunt == "taunt_dosido" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_flip" )
			{
				if ( initiator == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
			if ( taunt == "taunt_rps" )
			{
				if ( TauntWin == true )
				{
					tauntDurationVar = 1f;
				}
				else
				{
					tauntDurationVar = 1f;
				}
			}
		}
		tauntDurationVar += 0.1f; //Small cooldown window to prevent taunting before playermodel is ready to
		player.TauntDuration = tauntDurationVar;
	}
	*/

	/// <summary>
	/// Simplified version for now. Hardcoded references for partner exit durations, values vary too much per class and state, variables are too complex for simpler dynamic code
	/// </summary>
	public void GetPartnerDuration( TFPlayer player, bool initiator )
	{
		var taunt = ActiveTaunt.ResourceName;
		var tauntDurationVar = 0f;
		if ( taunt == "taunt_highfive" )
		{
			tauntDurationVar = 4.17f;
		}
		tauntDurationVar += 0.1f; //Small cooldown window to prevent taunting before playermodel is ready to
		player.TauntDuration = tauntDurationVar;
	}

	/// <summary>
	/// Unused, Generates random winner for duel taunts
	/// </summary>
	/// <returns></returns>
	public bool PartnerTauntGenerateWinner()
	{
		var random = new Random();
		var randomBool = random.Next( 2 ) == 1;
		return randomBool;
	}

	#endregion

	/// <summary>
	/// Creates a temporary prop model for taunts
	/// </summary>
	/// <param name="taunt"></param>
	/// <param name="player"></param>
	public void CreateTauntProp( TauntData taunt, TFPlayer player )
	{
		player.TauntPropModel = new AnimatedEntity
		{
			Position = Position,
			Owner = player,
			EnableHideInFirstPerson = false,
		};
		player.TauntPropModel.SetModel( taunt.TauntPropModel );
		player.TauntPropModel.SetParent( player, true );
	}

	#region music
	Sound TauntMusic { get; set; }
	public void StartMusic()
	{
		if ( Game.LocalClient?.Pawn == this )
		{
			Log.Info("local music");
			TauntMusic = Sound.FromScreen( To.Single( this ), ActiveTaunt.TauntMusic ); //FIX, not working
		}
		if ( Game.LocalClient?.Pawn != this )
		{
			Log.Info( "nonlocal music" );
			TauntMusic = Sound.FromEntity( ActiveTaunt.TauntMusic, this, "head" ); ; //INVESTIGATE, not playing from attachment
		}
	}
	public void StopMusic()
	{
		TauntMusic.Stop();
	}

	#endregion

	/// <summary>
	/// Console command for playing taunts by their animation name
	/// </summary>
	/// <param taunt_name="taunt_name"></param>
	[ConCmd.Server( "tf_playtaunt" )]
	public static void Command_PlayTauntName( string taunt_name )
	{
		if ( ConsoleSystem.Caller.Pawn is TFPlayer player )
		{
			TauntData taunt = null;

			if ( !player.CanTaunt() ) return;

			//Finds the appropriate taunt data and assigns it
			foreach ( TauntData data in player.TauntList )
			{
				if ( data.ResourceName == taunt_name )
					taunt = data;

			}

			if ( taunt != null )
			{
				if ( tf_disable_movement_taunts && (taunt.TauntMovespeed != 0) )
				{
					Log.Info( $"{taunt_name} is currently disabled by tf_disable_movement_taunts" );
				}
				else
				{
					player.PlayTaunt( taunt );
					Log.Info( $"{taunt_name} is a valid taunt name." );
				}
			}

			else
			{
				Log.Info( $"{taunt_name} is not a valid taunt name." );
			}
		}
	}

	/// <summary>
	/// Logic for re-implementing animation events in ModelDoc sequences (currently only on playermodels)
	/// </summary>
	public override void OnAnimEventGeneric( string name, int intData, float floatData, Vector3 vectorData, string stringData )
	{
		if ( name == "TF_TAUNT_ENABLE_MOVE" )
		{
			if ( intData == 0 )
			{
				TauntEnableMove = false;
			}
			if ( intData == 1 )
			{
				TauntEnableMove = true;
			}
		}

		if ( name == "TF_HIDE_WEAPON" )
		{
			var weapon = ActiveWeapon as TFWeaponBase;
			if ( weapon == null ) return;

			if ( intData == 0 )
			{
				weapon.EnableDrawing = true;
			}
			if ( intData == 1 )
			{
				weapon.EnableDrawing = false;
			}
		}

		if ( name == "TF_HIDE_TAUNTPROP" )
		{
			if ( intData == 0 && TauntPropModel.EnableDrawing == false )
			{
				TauntPropModel.EnableDrawing = true;
			}
			if ( intData == 1 && TauntPropModel.EnableDrawing == true )
			{
				TauntPropModel.EnableDrawing = false;
			}
		}

		if ( name == "TF_SET_BODYGROUP_PLAYER" )
		{
			SetBodyGroup( stringData, intData );
		}

		if ( name == "TF_SET_BODYGROUP_WEAPON" )
		{
			var weapon = ActiveWeapon as TFWeaponBase;
			if ( weapon == null ) return;

			weapon.SetBodyGroup( stringData, intData );
		}
	}

	[ConVar.Replicated] public static bool tf_sv_debug_taunts { get; set; } = false;
	[ConVar.Replicated] public static bool tf_disable_movement_taunts { get; set; } = false;
}
