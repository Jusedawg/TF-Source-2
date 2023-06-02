using Amper.FPS;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TFS2;

partial class TFPlayer
{
	//
	//General Taunt Vars
	//

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
	/// Whether or not the player can manually cancel their taunt. Set via AnimGraphTag
	/// </summary>
	[Net]
	public bool TauntCanCancel { get; set; }

	[Net]
	public bool TauntsReset { get; set; }

	//
	//Weapon Taunt Vars
	//

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

	//
	//Partner Taunt Vars
	//

	/// <summary>
	/// Currently hovered TFPlayer within PartnerAcceptDistance
	/// </summary>
	public TFPlayer PartnerTarget => GetPartnerTarget();

	[Net]
	public Vector3 PartnerValidLocation { get; set; }

	[Net]
	public bool PartnerSpaceAvailable { get; set; }

	/// <summary>
	/// How far to consider a hovered player for a valid partner
	/// </summary>
	public const float PartnerAcceptDistance = 150f;

	/// <summary>
	/// How far to consider a hovered player for a valid partner
	/// </summary>
	public const float PartnerPlacementDistance = 96f;

	/// <summary>
	/// How far in height can two players be from eachother?
	/// </summary>
	public const float PartnerMaxHeighDiff = 12f;

	/// <summary>
	/// Whether or not this player is ready to accept a partner, set via AnimGraphTag
	/// </summary>
	[Net]
	public bool WaitingForPartner { get; set; }

	[Net]
	public IList<TauntData> TauntList { get; set; }

	public void CreateTauntList()
	{
		// Reset our tauntlist, incase we have one
		TauntList.Clear();

		TFPlayerClass classkey = GetTFPlayerClass();

		// Add taunt to Class' taunt list if an entry for our current class exists
		foreach ( var taunt in TauntData.StockTaunts )
		{
			if ( taunt.AnimationModelEntries == null ) continue;
			foreach ( var tauntModelEntry in taunt.AnimationModelEntries )
			{
				if ( tauntModelEntry.playerClass == classkey )
				{
					TauntList.Add( taunt );
				}
			}
		}

		//Custom taunts, currently not implemented
		foreach ( var taunt in TauntData.CustomTaunts )
		{
			if ( taunt.AnimationModelEntries == null ) break;
			foreach ( var tauntModelEntry in taunt.AnimationModelEntries )
			{
				if ( tauntModelEntry.playerClass == classkey )
				{
					TauntList.Add( taunt );
				}
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
		if ( Input.Released( "Taunt" ) && !WeaponTauntAvailable && !InCondition( TFCondition.Taunting ) )
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
		/*
		if ( HoveredEntity != null )
		{
			if ( HoveredEntity is TFPlayer player  )
			{
				if ( HoveredDistance < PartnerDistance ) //player bounds width * 2.5
					PartnerTarget = player;
				else
					PartnerTarget = null;
			}
			else
				PartnerTarget = null;
		}
		*/

		//If taunt menu button is pressed before certain time elapses, check for Partner/Group taunts, if none play weapon taunt
		if ( Input.Pressed( "Taunt" ) && WeaponTauntAvailable && !InCondition( TFCondition.Taunting ) )
		{
			if ( TryDoubleTapTaunt() ) return;
		}

		//If we have somehow exited taunt condition without reseting our parameters, do so now
		if ( !InCondition( TFCondition.Taunting ) && !TauntsReset )
		{
			ResetTauntParams();
		}

		//Failsafe, check to see if we are somehow in taunt condition without a taunt set
		if ( ActiveTaunt == null ) return;

		if ( InCondition( TFCondition.Taunting ) )
		{
			if ( !IsLocalPawn )
			{
				UpdateMusicPosition();
			}

			if ( Input.Pressed( "Attack1" ) )
			{
				Animator?.SetAnimParameter( "b_fire", true );
			}

			if ( Input.Pressed( "Attack2" ) )
			{
				Animator?.SetAnimParameter( "b_fire_secondary", true );
			}

			//Debug, call a fake partner accept
			if ( WaitingForPartner && Input.Pressed( "Ready" ) )
			{
				AcceptPartnerTaunt( true );
			}

			// Stop single taunts via loss of grounded state
			if ( ActiveTaunt.RequireGround && GroundEntity == null )
			{
				StopTaunt();
				return;
			}

			// Stop looping/partner taunts via key press
			if ( TauntCanCancel && (Input.Pressed( "Jump" ) || Input.Pressed( "Taunt" )) )
			{
				//TauntAnimationMaster?.SetAnimParameter( "b_taunt_cancel", true ); //TAM
				Animator?.SetAnimParameter( "b_taunt_cancel", true );
				return;
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
	public TFPlayer GetPartnerTarget()
	{
		if ( HoveredEntity != null )
		{
			if ( HoveredEntity is TFPlayer player )
			{
				if ( HoveredDistance < PartnerAcceptDistance )
					return player;
				else
					return null;
			}
			else
				return null;
		}
		else
			return null;
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
			if (  PartnerTarget.WaitingForPartner == true && IsPartnerTauntAngleValid( PartnerTarget ) && PartnerTarget.PartnerSpaceAvailable )
			{
				WeaponTauntAvailable = false;
				ActiveTaunt = PartnerTarget.ActiveTaunt;
				PartnerSetLocation( PartnerTarget );
				AcceptPartnerTaunt( false );
				PartnerTarget.AcceptPartnerTaunt( true );
				return true;
			}
			// Group taunt
			else if ( PartnerTarget.ActiveTaunt.TauntAllowJoin == true )
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

		WeaponTauntAvailable = false;

		//If we have a tauntdata set, use that
		var Tauntdata = ResourceLibrary.Get<TauntData>( weapon.Data.TauntData );
		if ( Tauntdata != null )
		{
			PlayTaunt( Tauntdata );
			return true;
		}

		//attempt via string instead
		var TauntName = weapon.Data.TauntString;
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

		var animcontroller = Animator as TFPlayerAnimator;
		var TauntIndex = TauntList.IndexOf( ActiveTaunt );  //Find way to dynamically assign, right now it MUST line up to animgraph

		if ( !CanTaunt() ) return;

		if ( taunt.IsPartnerTaunt )
		{
			//If we are starting the partner taunt, we need to check for valid spacing
			if ( initiator )
			{
				if ( !CanInitiatePartnerTaunt() && !tf_sv_taunt_ignore_partner_space_requirements )
				{
					Log.Info( "Not enough space for a partner" );
					return;
				}
				else if ( tf_sv_taunt_ignore_partner_space_requirements )
				{
					var state = PartnerSpaceAvailable ? "passed" : "failed";
					Log.Warning( $"tf_taunt_ignore_partner_space_requirements is enabled; Partner Taunt {state} the space check");
				}
			}
		}

		// sets player facing the direction of their camera, rather than their model rotation.
		if ( initiator )
		{
			Rotation = Animator.GetIdealRotation();
		}


		/*
		//TauntDuration deprecated by OnAnimGraphTag
		if ( TauntType == TauntType.Simple )
		{
			if (!string.IsNullOrEmpty( ActiveTaunt.SequenceName ) ) TauntDuration = GetSequenceDuration( ActiveTaunt.SequenceName );
			if ( taunt.IsCustomTaunt ) TauntDuration = tauntPuppetMaster.GetSequenceDuration( ActiveTaunt.SequenceName );
			if ( tauntPuppetMaster != null ) TauntDuration = tauntPuppetMaster.CurrentSequence.Duration;
			Log.Info($"Duration {TauntDuration}");
		} 
		*/

		CreateTauntProp( ActiveTaunt );

		if ( !string.IsNullOrEmpty( taunt.TauntMusic ) )
		{
			StopMusic();
			StartMusic();
		}

		if ( !taunt.UseTAM )
		{
			animcontroller?.SetAnimParameter( "taunt_name", TauntIndex );
		}

		StayThirdperson = IsThirdperson;
		ThirdpersonSet( true );

		ApplyTauntConds();
	}

	/// <summary>
	/// Play the selected taunt (by string, matching file name of taunt asset)
	/// </summary>
	/// <param name="taunt_name"></param>
	public void PlayTaunt( string taunt_name )
	{
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
			var playerClass = PlayerClass.Title.ToLower();

			//Special Case for engineer, since shotgun is his primary
			if ( playerClass == "engineer" )
			{
				slot = "primary";
			}

			taunt = TauntData.Get( $"weapon_{playerClass}_{slot}" );

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

		ResetTauntParams();

		StopMusic();

		//DeleteAnimationMaster(); TAM

		if ( TauntPropModel != null && Game.IsServer )
			TauntPropModel.Delete();
		if ( weapon != null )
			weapon.EnableDrawing = true;

		ThirdpersonSet( StayThirdperson );

	}

	/// <summary>
	/// Failsafe function that makes sure our parameters are set back to default when a taunt ends
	/// </summary>
	public void ResetTauntParams()
	{
		Animator?.SetAnimParameter( "b_taunt", false );
		Animator?.SetAnimParameter( "b_taunt_associate", false );
		Animator?.SetAnimParameter( "b_taunt_initiator", false );
		Animator?.SetAnimParameter( "b_taunt_cancel", false );
		TauntEnableMove = false;
		TauntCanCancel = false;
		WaitingForPartner = false;

		ActiveTaunt = null;

		TauntsReset = true;
	}

	#region Partner Taunt Logic

	/// <summary>
	/// Tells game if we're ready to accept a partner
	/// </summary>
	public bool CanInitiatePartnerTaunt()
	{
		if ( PartnerTauntIsSpaceValid() )
		{
			PartnerSpaceAvailable = true;
			return true;
		}
		else
		{
			PartnerSpaceAvailable = false;
			return false;
		}
	}

	/// <summary>
	/// Checks to see if we can move a player in front of initiator
	/// </summary>
	public bool PartnerTauntIsSpaceValid()
	{
		var positionShiftUp = Position;
		//positionShiftUp.z += PartnerMaxHeighDiff;
		var validateFrom = positionShiftUp;
		var validateTo = positionShiftUp + Rotation.Forward * PartnerPlacementDistance;
		var tr = PartnerValidateTrace( validateFrom, validateTo ).Run();

		if ( tf_sv_debug_taunts )
		{
			var BBox = new BBox { Mins = ViewVectors.HullMin, Maxs = ViewVectors.HullMax };

			DebugOverlay.Line( tr.StartPosition, tr.EndPosition, Game.IsServer ? Color.Yellow : Color.Green, 15f, true );
			DebugOverlay.Box( tr.EndPosition, BBox.Mins, BBox.Maxs, Color.Cyan, 15f, true );
			DebugOverlay.Sphere( tr.EndPosition, 2f, Color.Red, 15f );
			DebugOverlay.Sphere( tr.StartPosition, 2f, Color.Green, 15f );
			DebugOverlay.Text( $"{tr.Distance}", tr.EndPosition, 15f );
		}

		// This split allows us to taunt in the edge case of ceilings between max height difference
		// Did we hit something? If so, check from max height for possible higher ground
		if ( tr.Hit )
		{
			positionShiftUp.z += PartnerMaxHeighDiff;
			validateFrom = positionShiftUp;
			validateTo = positionShiftUp + Rotation.Forward * PartnerPlacementDistance;
			var trHigh = PartnerValidateTrace( validateFrom, validateTo ).Run();
			if ( trHigh.Hit )
			{
				Log.Info( "Partner Taunt Failed: Obstacle in way" );
				return false;
			}
		}

		var validateToDown = validateTo;
		var heightMult = tr.Hit ? 2 : 1; //If we hit low, but passed high, multiply height check to compensate
		validateToDown.z -= PartnerMaxHeighDiff * heightMult;
		var trDown = PartnerValidateTrace( validateTo, validateToDown ).Run();

		if ( tf_sv_debug_taunts )
		{
			DebugOverlay.Line( trDown.StartPosition, trDown.EndPosition, Game.IsServer ? Color.Yellow : Color.Green, 15f, true );
		}

		// If no ground or ground height is too low, no need to check if height is too high because of first trace
		if ( !trDown.Hit )
		{
			Log.Info( "Partner Taunt Failed: No Ground Found" );
			return false;
		}
		else
		{
			PartnerValidLocation = trDown.HitPosition;
			return true;
		}
	}

	public virtual Trace PartnerValidateTrace( Vector3 Origin, Vector3 Target )
	{
		//var collBox = new Vector3( 48, 48, 82 );
		var BBox = new BBox { Mins = ViewVectors.HullMin, Maxs = ViewVectors.HullMax };
		var tr = Trace.Box( BBox, Origin, Target )
			.WorldOnly();

		return tr;
	}

	/// <summary>
	/// Checks to see if we are within a certain angle opposite of the target
	/// </summary>
	public bool IsPartnerTauntAngleValid( TFPlayer target )
	{
		// Get a vector from owner origin to target origin
		var vecToTarget = (target.WorldSpaceBounds.Center - Owner.WorldSpaceBounds.Center).WithZ( 0 ).Normal;

		// Get owner forward view vector
		var vecOwnerForward = Owner.GetEyeRotation().Forward.WithZ( 0 ).Normal;

		// Get target forward view vector
		var vecTargetForward = target.GetEyeRotation().Forward.WithZ( 0 ).Normal;

		// Make sure owner is behind, facing and aiming at target's back
		float flPosVsTargetViewDot = vecToTarget.Dot( vecTargetForward ); // Behind?
		float flPosVsOwnerViewDot = vecToTarget.Dot( vecOwnerForward );   // Facing?
		float flViewAnglesDot = vecTargetForward.Dot( vecOwnerForward );  // Facestab?

		//Log.Info( $"{flPosVsTargetViewDot < 0}" );
		//Log.Info( $"{flPosVsOwnerViewDot > 0.5f}" );
		return flPosVsTargetViewDot < 0 && flPosVsOwnerViewDot > 0.5f;
	}

	/// <summary>
	/// Sets our transform to the proper spot across from target
	/// </summary>
	public void PartnerSetLocation( TFPlayer target )
	{
		if ( Game.IsClient ) return;
		//var distance = PartnerPlacementDistance;
		//Vector3 moveTo = target.Position + target.Rotation.Forward * distance;
		var rotateTo = Rotation.FromYaw( target.Rotation.Yaw() - 180f );

		Position = target.PartnerValidLocation;
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
			animcontroller?.SetAnimParameter( "b_taunt_associate", true );
		}
		else
		{
			animcontroller?.SetAnimParameter( "b_taunt_initiator", true );
		}
	}

	/// <summary>
	/// Unused, Generates random winner for duel taunts
	/// </summary>
	/// <returns></returns>
	public bool PartnerTauntGenerateWinner()
	{
		var random = new Random();
		var randomBool = random.Next( 1 ) == 1;
		return randomBool;
	}

	#endregion

	/// <summary>
	/// Creates a temporary prop model for taunts
	/// </summary>
	/// <param name="taunt"></param>
	public void CreateTauntProp( TauntData taunt )
	{
		var propPath = taunt.GetPropModel( GetTFPlayerClass() );

		//Didn't get a class-specific entry, try all class
		if ( string.IsNullOrEmpty( propPath ) )
		{
			propPath = taunt.GetPropModel( TFPlayerClass.Undefined );
		}

		//didn't get any matching entries, do nothing
		if ( string.IsNullOrEmpty( propPath ) ) return;

		TauntPropModel = new AnimatedEntity
		{
			Position = Position,
			Owner = this,
			EnableHideInFirstPerson = false,
		};
		TauntPropModel.SetModel( propPath );

		//TauntPropModel.SetParent( PlayerModel, true ); TAM
		TauntPropModel.SetParent( this, true );
	}

	#region Music

	Sound TauntMusic { get; set; }

	[ClientRpc]
	public void StartMusic()
	{
		var tauntMusic = ActiveTaunt.TauntMusic;

		//Lets us assign a UI variant without having to manually do so
		var tauntMusicLength = ActiveTaunt.TauntMusic.Length;
		var format = ".sound";
		var tauntMusicNoFormat = tauntMusic.Remove( tauntMusicLength - format.Length );

		if ( IsLocalPawn )
		{
			//Log.Info( "local music" );
			TauntMusic = Sound.FromScreen( To.Single( this ), $"{tauntMusicNoFormat}.ui{format}" );
			SetOtherMusicVolume( 0.1f, this ); //Figure out how to muffle incoming taunt music from other players ONLY for this player
		}
		else
		{
			//Log.Info( "nonlocal music" );
			//var attachment = PlayerModel.GetAttachment( "head" ); TAM
			var attachment = GetAttachment( "head" );
			//TauntMusic = Sound.FromEntity( ActiveTaunt.TauntMusic, PlayerModel, "head" ); //Doesn't play from attachment, using hacky workaround
			TauntMusic = Sound.FromWorld( ActiveTaunt.TauntMusic, attachment.Value.Position );
		}
	}

	[ClientRpc]
	public void UpdateMusicPosition()
	{
		//var attachment = PlayerModel.GetAttachment( "head" ); TAM
		var attachment = GetAttachment( "head" );
		TauntMusic.SetPosition( attachment.Value.Position );
	}

	[ClientRpc]
	public void SetOtherMusicVolume( float volume, TFPlayer caller )
	{
		foreach ( TFPlayer player in Entity.All.Where( x => x != caller ).OfType<TFPlayer>() )
		{
			player.TauntMusic.SetVolume( volume );
		}
	}

	[ClientRpc]
	public void StopMusic()
	{
		TauntMusic.Stop();
		SetOtherMusicVolume( 1f, this );
	}

	public bool IsLocalTFPlayer( TFPlayer pawn )
	{
		var steamId = Game.SteamId;
		foreach ( var client in Game.Clients )
		{
			if ( client.SteamId == steamId && client.Pawn == pawn ) return true;
		}

		return false;
	}

	public IEnumerable<IClient> GetNonLocalTFPlayers( TFPlayer pawn )
	{
		List<IClient> clients = new List<IClient>();
		var steamId = Game.SteamId;
		foreach ( var client in Game.Clients )
		{
			if ( client.SteamId == steamId && client.Pawn == pawn ) continue;
			clients.Add( client );
		}

		return clients;
	}

	#endregion

	#region Taunt Kills

	public virtual void Tauntkill_HeavyHighNoon()
	{
		if ( !Game.IsServer ) return;

		var damage = 500f;
		var hurtbox = 5f;
		var range = 500f;
		var forceAngle = new QAngle( -45, Rotation.Yaw(), 0 );
		var force = 500;
		List<string> tags = new() { TFDamageTags.Bullet };

		Tauntkill_BoxTrace( range, hurtbox, damage, forceAngle.Forward * force, tags );
	}

	public virtual void Tauntkill_PyroHadouken()
	{
		if ( !Game.IsServer ) return;

		var damage = 500f;
		var hurtbox = 24f;
		var range = 64f;
		var forceAngle = new QAngle( -45, Rotation.Yaw(), 0 );
		var force = 350f;
		List<string> tags = new() { TFDamageTags.Burn, TFDamageTags.Ignite };

		Tauntkill_BoxTrace( range, hurtbox, damage, forceAngle.Forward * force, tags, false );
	}

	public virtual void Tauntkill_SpyFencing( int phase )
	{
		if ( !Game.IsServer ) return;

		var damage = 500f;
		var hurtbox = 24f;
		var range = 64f;
		var forceAngle = new QAngle( -45, Rotation.Yaw(), 0 );
		var force = 350f;
		List<string> tags = new() { TFDamageTags.Slash };

		if ( phase == 1 || phase == 2 )
		{
			damage = 25f;
			tags.Add( TFDamageTags.PreventPhysicsForce );
		}

		Tauntkill_BoxTrace( range, hurtbox, damage, forceAngle.Forward * force, tags );
	}

	public virtual void Tauntkill_BoxTrace( float range, float extents, float damage, Vector3 force, IEnumerable<string> tags, bool singleTarget = true )
	{
		var startPoint = WorldSpaceBounds.Center;
		var endPoint = ((Rotation.Forward * range) + Position).WithZ( startPoint.z );

		var tr = Trace.Box( new Vector3( extents ), startPoint, endPoint )
			.Ignore( this )
			.WithTag( "player" )
			.RunAll();



		if ( tf_sv_debug_taunts )
		{
			//Draws approximate corners of box trace, not exact because these respect rotation while the box trace does not
			var RU = (Rotation.Right + Rotation.Up) * (extents);
			var RL = (Rotation.Right - Rotation.Up) * (extents);
			var LU = (-Rotation.Right + Rotation.Up) * (extents);
			var LL = (-Rotation.Right - Rotation.Up) * (extents);
			DebugOverlay.Line( startPoint + RU, endPoint + RU, 5f );
			DebugOverlay.Line( startPoint + RL, endPoint + RL, 5f );
			DebugOverlay.Line( startPoint + LU, endPoint + LU, 5f );
			DebugOverlay.Line( startPoint + LL, endPoint + LL, 5f );

			DebugOverlay.Box( endPoint, new Vector3( extents ), new Vector3( -extents ), Color.Cyan, 5f );
			DebugOverlay.Axis( endPoint, Rotation, 10, 5f );
			DebugOverlay.Text( "End Point", endPoint, 5f );
		}

		if ( tr != null )
		{
			foreach ( var trHit in tr )
			{
				var ent = trHit.Entity;
				if ( ent is TFPlayer )
				{
					var damageInf = ExtendedDamageInfo.Create( damage )
						.UsingTraceResult( trHit )
						.WithInflictor( this )
						.WithAttacker( this )
						.WithWeapon( ActiveWeapon )
						.WithForce( force )
						.WithTags( tags );


					if ( (ent as TFPlayer).CanTakeDamage( this, damageInf ) )
					{
						(ent as TFPlayer).TakeDamage( damageInf );
						if ( singleTarget )
						{
							break;
						}
					}
				}
			}
		}
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
			TFWeaponBase weapon = (TFWeaponBase)ActiveWeapon;
			if ( weapon != null )
			{
				if ( intData == 0 )
				{
					weapon.EnableDrawing = true;
				}
				if ( intData == 1 )
				{
					weapon.EnableDrawing = false;
				}
			}
		}

		if ( name == "TF_HIDE_TAUNTPROP" )
		{
			if ( TauntPropModel != null )
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
		}

		if ( name == "TF_SET_BODYGROUP_PLAYER" )
		{
			SetBodyGroup( stringData, intData );
		}

		if ( name == "TF_SET_BODYGROUP_WEAPON" )
		{
			TFWeaponBase weapon = (TFWeaponBase)ActiveWeapon;
			if ( weapon != null )
			{
				weapon.SetBodyGroup( stringData, intData );
			}
		}

		if ( name == "TF_TAUNTKILL_HEAVYHIGHNOON" )
		{
			Tauntkill_HeavyHighNoon();
		}

		if ( name == "TF_TAUNTKILL_PYROHADOUKEN" )
		{
			Tauntkill_PyroHadouken();
		}

		if ( name == "TF_TAUNTKILL_SPYFENCING" )
		{
			Tauntkill_SpyFencing( intData );
		}

		base.OnAnimEventGeneric( name, intData, floatData, vectorData, stringData );
	}

	public virtual void RecieveAnimGraphTag( string tag, AnimGraphTagEvent fireMode )
	{
		OnAnimGraphTag( tag, fireMode );
	}

	protected override void OnAnimGraphTag( string tag, AnimGraphTagEvent fireMode )
	{
		if ( !Game.IsServer ) return;

		if ( tag == "TF_Taunt_CanCancel" )
		{
			if ( fireMode == AnimGraphTagEvent.Start )
			{
				TauntCanCancel = true;
			}
			if ( fireMode == AnimGraphTagEvent.End )
			{
				TauntCanCancel = false;
			}
			if ( fireMode == AnimGraphTagEvent.Fired )
			{
				TauntCanCancel = !TauntCanCancel;
			}
		}

		if ( tag == "TF_Taunt_PartnerTauntReady" )
		{
			if ( fireMode == AnimGraphTagEvent.Start )
			{
				WaitingForPartner = true;
			}
			if ( fireMode == AnimGraphTagEvent.End )
			{
				WaitingForPartner = false;
			}
			if ( fireMode == AnimGraphTagEvent.Fired )
			{
				WaitingForPartner = !WaitingForPartner;
			}
		}

		if ( tag == "TF_Taunt_End" )
		{
			if ( fireMode == AnimGraphTagEvent.Start || fireMode == AnimGraphTagEvent.Fired )
			{
				StopTaunt();
			}
		}

		base.OnAnimGraphTag( tag, fireMode );
	}
	public TFPlayerClass GetTFPlayerClass()
	{
		var classname = PlayerClass.Title.ToLower();
		TFPlayerClass classkey = TFPlayerClass.Undefined;

		foreach ( KeyValuePair<TFPlayerClass, string> pair in PlayerClass.Names )
		{
			if ( pair.Value == classname )
			{
				classkey = pair.Key;
			}
		}
		return classkey;
	}

	[ConVar.Replicated] public static bool tf_sv_debug_taunts { get; set; } = false;
	[ConVar.Replicated] public static bool tf_sv_taunt_ignore_partner_space_requirements { get; set; } = false;
	[ConVar.Replicated] public static bool tf_disable_movement_taunts { get; set; } = false;
}
