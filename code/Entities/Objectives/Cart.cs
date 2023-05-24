using Sandbox;
using Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amper.FPS;

namespace TFS2
{
	/// <summary>
	/// A payload cart.
	/// </summary>
	[Library("tf_cart", Title = "Payload Cart" ,Group = "Objectives")]
	[Model(Model = "models/props_trainyard/bomb_cart.vmdl" )]
	[HammerEntity]
	public partial class Cart : AnimatedEntity, ITeam, IResettable
	{
		[ConVar.Replicated] public static bool tf_debug_cart { get; set; } = false;
		public static new IEnumerable<Cart> All => Entity.All.OfType<Cart>();

		#region Properties
		public int TeamNumber => (int)Team;
		[Property( "Team" )]
		public HammerTFTeamOption TeamOption { get; set; } = HammerTFTeamOption.Blue;

		[Property("Path", Title = "Starting Path"), FGDType("target_destination")]
		public string LinkedCartPath { get; set; }
		
		[Category( "Speed" ), Property( "Level1Speed", Title = "x1 Speed" )]
		public float Level1Speed { get; set; } = 50f;
		[Category( "Speed" ), Property( "Level2Speed", Title = "x2 Speed" )]
		public float Level2Speed { get; set; } = 70f;
		[Category( "Speed" ), Property( "Level3Speed", Title = "x3 Speed" )]
		public float Level3Speed { get; set; } = 90f;
		/// <summary>
		/// How much the cart can change its speed in HU/s
		/// </summary>
		[Category("Speed"), Property("Accelleration", Title = "Cart Accelleration")]
		public float Acceleration { get; set; } = 70f;

		/// <summary>
		/// After how many seconds the carts starts rolling back automatically.
		/// </summary>
		[Category("Speed/Rollback"), Property("IdleTime", Title = "Automatic Rollback Time"), Net]
		public float IdleTime { get; set; } = 30f;
		[Category( "Speed/Rollback" ), Property( "BackwardsSpeed", Title = "Rollback Speed" )]
		public float BackwardsSpeed { get; set; } = 9f;

		[Category( "Sound" ), Property( "CartMoveSound", Title = "Move Sound" ), FGDType( "sound" )]
		public string MoveSound { get; set; }
		[Category("Sound"), Property("CartStartMoveSound", Title = "Start Move Sound"), FGDType( "sound" )]
		public string StartMoveSound { get; set; }
		[Category( "Sound" ), Property("CartStopMoveSound", Title = "Stop Move Sound" ), FGDType( "sound" )]
		public string StopMoveSound { get; set; }
		/// <summary>
		/// Sound which plays when the cart rolls in a rollback/forward zone.
		/// </summary>
		[Category( "Sound" ), Property("CartGrindSound", Title = "Rollback Sound" ), FGDType( "sound" )]
		public string RollbackSound { get; set; }
		#endregion

		[Net] public TFTeam Team { get; set; }
		[Net] public CartPath Path { get; set; }
		private CartPath startingPath;
		public bool IsLoaded => Path != null;
		public IReadOnlyList<TFPlayer> Pushers => pushers.AsReadOnly();
		public IReadOnlyList<TFPlayer> Blockers => blockers.AsReadOnly();
		[Net]
		protected IList<TFPlayer> pushers { get; set; }
		[Net]
		protected IList<TFPlayer> blockers { get; set; }


		/// <summary>
		/// What node we are currently moving away from
		/// </summary>
		public CartPathNode CurrentNode => Path.PathNodes.ElementAtOrDefault( CurrentIndex );
		public CartPathNode PreviousNode => Path.PathNodes.ElementAtOrDefault( CurrentIndex -1 );
		public CartPathNode NextNode => Path.PathNodes.ElementAtOrDefault( CurrentIndex + 1 );
		[Net] public int CurrentIndex { get; set; }
		/// <summary>
		/// How far away we are from the current node (in percentage from 0 to 1)
		/// </summary>
		[Net]
		public float CurrentFraction { get; set; }

		/// <summary>
		/// The time since this cart was last pushed.
		/// </summary>
		[Net]
		public TimeSince TimeSincePush { get; protected set; } = 0;

		/// <summary>
		/// Distance from the current node to the next node
		/// </summary>
		protected float NodeDistance => nodeDistances.ElementAtOrDefault( CurrentIndex );
		/// <summary>
		/// The speed the cart is currently travelling at.
		/// </summary>
		protected float CurrentSpeed;
		protected bool IsAtEnd = false;

		// Util list of distances to next node
		[Net]
		private IList<float> nodeDistances { get; set; }

		public List<CartPath> GetPaths()
		{
			if ( Path == null ) return null;

			List<CartPath> paths = new() { Path };

			var currentPath = Path;
			var nextPath = currentPath.PathNodes?.LastOrDefault()?.GetNextPath();

			while ( nextPath != null)
			{
				paths.Add( nextPath );
				currentPath = nextPath;
				nextPath = currentPath.PathNodes.Last().GetNextPath();
			}

			return paths;
		}
		public override void Spawn()
		{
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
			EnableAllCollisions = true;

			Team = TeamOption.ToTFTeam();

			Tags.Add( CollisionTags.Solid );
			Tags.Add( "cart" );
			Tags.Add( $"cart_{Team.GetTag()}" );
		}

		[GameEvent.Entity.PostSpawn]
		public void PostLevelSetup()
		{
			startingPath = FindByName( LinkedCartPath ) as CartPath;

			if ( startingPath == null )
				throw new ArgumentNullException( "Cart HAS to have a cart path set!" );

			Reset();
		}

		public void Reset(bool fullRoundReset = true)
		{
			Path = startingPath;
			ResetPath();

			pushers.Clear();
			blockers.Clear();

			TimeSincePush = 0;
			CurrentSpeed = 0;
		}

		public void ResetPath()
		{
			CurrentIndex = 0;
			CurrentFraction = 0;
			IsAtEnd = false;

			nodeDistances.Clear();
			for ( int i = 0; i < Path.PathNodes.Count-1; i++ )
			{
				float distance = Path.GetCurveLength( Path.PathNodes.ElementAt( i ), Path.PathNodes.ElementAt( i+1 ), CartPath.PATH_DETAIL );
				nodeDistances.Add( distance);
			}
			nodeDistances.Add( 0 ); // last node technically has a distance of 0, makes index out of bounds less likely

			Position = CurrentNode.WorldPosition;
			var dir = Position - Path.GetPointBetweenNodes( CurrentNode, NextNode, 0.05f );
			Rotation = Rotation.LookAt( dir ).Angles().WithRoll( 0 ).ToRotation();
		}

		[GameEvent.Tick.Server]
		public void Tick()
		{
			if ( Path == null )
			{
				Log.Error( $"Cart {this} has no path set!" );
			}

			DoMovement();

			if ( tf_debug_cart )
			{
				Path.DrawPath( 10 );
				var textpos = Position + Vector3.Up * 32f;

				DebugOverlay.Sphere( GetPathPosition(), 8f, Color.Orange );
				
				DebugOverlay.Text( $"[CART]", textpos, 0, Color.Blue );
				DebugOverlay.Text( $"Team: {Team}", textpos, 1, Color.Cyan );
				DebugOverlay.Text( $"Pushers: {pushers.Count}", textpos, 2, Color.Cyan );
				DebugOverlay.Text( $"Blockers: {blockers.Count}", textpos, 3, Color.Cyan );
				DebugOverlay.Text( $"CanPush: {CanPush()}", textpos, 4, CanPush() ? Color.Green : Color.Red );

				DebugOverlay.Text( $"CurrentNode: {CurrentNode} ({CurrentIndex + 1}/{Path.PathNodes.Count}, {NextNode})", textpos, 6, !IsAtEnd ? Color.Green : Color.Red );
				DebugOverlay.Text( $"CurrentFraction: {CurrentFraction}", textpos, 7, Color.Cyan );
				DebugOverlay.Text( $"NodeDistance: {NodeDistance}", textpos, 8, Color.Cyan );
				DebugOverlay.Text( $"CurrentSpeed: {CurrentSpeed} ({GetSpeedFraction(CurrentSpeed * Time.Delta)})", textpos, 9, Color.Cyan );

				DebugOverlay.Text( $"Time Since Push: {TimeSincePush}", textpos, 11, Color.Cyan );
				DebugOverlay.Text( $"CanMove: {CanMove()}", textpos, 12, CanMove() ? Color.Green : Color.Red );
				DebugOverlay.Text( $"CanRollforward: {CanRollforward()}", textpos, 13, CanRollforward() ? Color.Green : Color.Red );
				DebugOverlay.Text( $"CanRollback: {CanRollback()}", textpos, 14, CanRollback() ? Color.Green : Color.Red );
			}
		}

		bool wasMoving = false;
		protected virtual void DoMovement()
		{
			if ( !CanMove() )
				return;

			bool isRolling = false;
			if(CanPush())
			{
				float maxSpeed = 0;
				switch (GetCapRate())
				{
					case 0:
						break;
					case 1:
						maxSpeed = Level1Speed;
						break;
					case 2:
						maxSpeed = Level2Speed;
						break;
					default:
						maxSpeed = Level3Speed;
						break;
				}

				CurrentSpeed += Acceleration * Time.Delta;
				CurrentSpeed = MathF.Min( maxSpeed, CurrentSpeed );

				TimeSincePush = 0;
			}
			else if(CanRollforward())
			{
				isRolling = true;
				float maxSpeed = Level3Speed;

				CurrentSpeed += Acceleration * Time.Delta;
				CurrentSpeed = MathF.Min( maxSpeed, CurrentSpeed );
			}
			else if(CanRollback())
			{
				isRolling = true;
				float minSpeed = -BackwardsSpeed;
				CurrentSpeed -= Acceleration * Time.Delta;
				CurrentSpeed = MathF.Max( minSpeed, CurrentSpeed );
			}
			else
			{
				float minSpeed = 0f;
				CurrentSpeed -= Acceleration * Time.Delta;
				CurrentSpeed = MathF.Max( minSpeed, CurrentSpeed );
			}

			if ( CurrentSpeed < 0 && CurrentIndex == 0 && CurrentFraction <= 0 )
			{
				CurrentSpeed = 0;
				return;
			}

			if ( CurrentSpeed == 0 )
			{
				if(wasMoving)
				{
					StopMoveSounds();
					wasMoving = false;
				}

				return;
			}

			bool isMovingReverse = CurrentSpeed < 0;

			if ( isMovingReverse )
			{
				float remainingDistance = MathF.Abs(CurrentSpeed) * Time.Delta;
				while ( remainingDistance > 0.01f )
				{
					remainingDistance = DoMoveBackwards( remainingDistance );
				}
			} 
			else
			{
				float remainingDistance = CurrentSpeed * Time.Delta;
				while ( remainingDistance > 0.01f )
				{
					remainingDistance = DoMoveForwards( remainingDistance );
				}
			}
			var newpos = GetPathPosition();

			Vector3 dir = isMovingReverse ? newpos - Position : Position - newpos;
			Rotation = Rotation.LookAt( dir );
				
			Position = newpos;

			if ( isRolling )
				RollingSounds();

			if ( !wasMoving )
			{
				StartMoveSounds();
				wasMoving = true;
			}
			else
				MoveSounds();
		}
		
		protected virtual float DoMoveForwards(float distance)
		{
			float usedDistance = MathF.Min( distance * Time.Delta, distance );
			CurrentFraction += GetSpeedFraction( usedDistance );

			if ( CurrentFraction >= 1 )
			{
				CurrentIndex++;
				CurrentFraction -= 1;
				OnNodeChanged( CurrentNode );

				if ( NextNode == null )
				{
					var nextPath = CurrentNode.GetNextPath();
					if(nextPath != null)
					{
						Path = nextPath;
						ResetPath();
						return -2;
					}

					IsAtEnd = true;
					StopMoveSounds();
					OnReachEnd.Fire( this );
					Log.Info( $"Reached end at: {CurrentIndex + 1}" );
					return -1;
				}
			}

			return distance - usedDistance;
		}

		protected virtual float DoMoveBackwards(float distance)
		{
			distance = MathF.Abs( distance );
			float usedDistance = MathF.Min( distance * Time.Delta, distance );
			CurrentFraction -= MathF.Abs(GetSpeedFraction( usedDistance ));

			if ( CurrentFraction < 0 )
			{
				if ( PreviousNode == null )
				{
					StopMoveSounds();
					return 0;
				}

				CurrentIndex--;
				CurrentFraction += 1;
				OnNodeChanged( CurrentNode );
			}

			return distance - usedDistance;
		}

		/// <summary>
		/// Gets the position of the cart along the current path.
		/// </summary>
		/// <returns>Position of the cart from the start of the Path in HU</returns>
		public float GetCurrentDistance()
		{
			if ( Path == null || CurrentNode == null ) return -1;

			float prevLength = 0;
			if ( PreviousNode != null )
				prevLength = nodeDistances.Take( CurrentIndex ).Sum();
			float currentLength = 0;
			if(NextNode != null)
				currentLength = nodeDistances.ElementAt(CurrentIndex) * CurrentFraction;

			if ( Cart.tf_debug_cart )
				DebugOverlay.ScreenText( $"prevLength + currentLength => {prevLength} + {currentLength}", -2 );

			return currentLength + prevLength;
		}

		/// <summary>
		/// Returns the position according to the current node and fraction
		/// </summary>
		/// <returns></returns>
		protected Vector3 GetPathPosition(bool reverse = false)
		{
			if ( IsAtEnd )
				return CurrentNode.WorldPosition;

			return Path.GetPointBetweenNodes( CurrentNode, NextNode, CurrentFraction, reverse );
		}
		protected float GetSpeedFraction(float speed)
		{
			return speed / NodeDistance;
		}
		
		protected virtual void OnNodeChanged( CartPathNode node )
		{
			//node.OnPass.Fire( this );
			var cp = node.GetControlPoint();
			if ( cp != null )
			{
				// Let gamerules know about this.
				TFGameRules.Current.ControlPointCaptured( cp, cp.OwnerTeam, Team, pushers.Select(ply => ply.Client).ToArray() );
				cp.SetOwnerTeam( Team );
			}
		}

		public int GetCapRate()
		{
			return pushers.Sum( TFGameRules.Current.GetCaptureValueForPlayer );
		}

		public bool CanMove()
		{
			if ( IsAtEnd )
				return false;

			return TFGameRules.Current.PointsMayBeCaptured();
		}
		public bool CanPush()
		{
			if ( blockers.Any() )
				return false;

			return pushers.Any();
		}
		public bool CanRollforward()
		{
			if ( CurrentNode.Mode == PathNodeMode.RollForward )
				return true;
			return false;
		}
		public bool CanRollback()
		{
			if ( CurrentNode.GetControlPoint() != null && CurrentFraction <= 0.05 )
				return false;

			if ( CurrentNode.Mode == PathNodeMode.RollBack )
				return true;
			
			if(TimeSincePush >= IdleTime)
			{
				if ( CurrentNode.GetControlPoint() != null && CurrentFraction <= GetSpeedFraction(CurrentSpeed+1) )
				{
					return false;
				}

				return true;
			}

			return false;
		}

		protected virtual void StartPush(TFPlayer ply)
		{
			if ( !pushers.Any() )
			{
				OnStartPush.Fire( ply );

				if ( blockers.Any() )
					OnStartBlock.Fire( ply );
			}

			pushers.Add( ply );
		}

		protected virtual void StopPush(TFPlayer ply)
		{
			pushers.Remove( ply );

			if ( !CanPush() )
			{
				OnStopPush.Fire( ply );
			}
		}

		protected virtual void StartBlock(TFPlayer ply)
		{
			if ( pushers.Any() )
			{
				OnStartBlock.Fire( ply );
			}

			blockers.Add( ply );
		}

		protected virtual void StopBlock(TFPlayer ply)
		{
			blockers.Remove( ply );

			if ( !blockers.Any() )
			{
				OnStopBlock.Fire( ply );
			}
		}

		public override void StartTouch( Entity other )
		{
			//Log.Info( "StartTouch" );
			if (other is TFPlayer ply)
			{
				if ( ply.Team == Team )
				{
					StartPush( ply );
				}
				else
				{
					StartBlock( ply );
				}
			}
		}

		public override void EndTouch( Entity other )
		{
			if(other is TFPlayer ply)
			{
				// Remove from both collections in case of team change
				if ( pushers.Contains( ply ) )
					StopPush( ply );
				if ( blockers.Contains( ply ) )
					StopBlock( ply );
			}
		}

		public Output OnStartPush { get; set; }
		public Output OnStopPush { get; set; }

		public Output OnStartBlock { get; set; }
		public Output OnStopBlock { get; set; }
		public Output OnReachEnd { get; set; }
	}
}
