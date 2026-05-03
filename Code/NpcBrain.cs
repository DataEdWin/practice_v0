using System;
using Sandbox.Citizen;
using Sandbox.Navigation;

public sealed class NpcBrain : Component
{
	public enum NpcState { Wandering, Idle, Fighting, Fleeing }

	public static Action<GameObject, GameObject> OnNpcDeath;

	[Property] public float WanderRadius { get; set; } = 300f;
	[Property] public float IdleMinTime { get; set; } = 2f;
	[Property] public float IdleMaxTime { get; set; } = 5f;
	[Property] public float AttackRange { get; set; } = 80f;
	[Property] public float FleeDistance { get; set; } = 400f;
	[Property] public float NormalSpeed { get; set; } = 80f;
	[Property] public float FleeSpeed { get; set; } = 200f;
	[Property] public float FleeDuration { get; set; } = 30f;
	[Property, Range( 0f, 1f )] public float Bravery { get; set; } = 0.5f;
	[Property] public float DetectionRadius { get; set; } = 400f;
	[Property] public CitizenAnimationHelper Animator { get; set; }

	public NpcState CurrentState;
	public GameObject Target => _target;

	private float _idleUntil;
	private float _lastAttackTime;
	private float _fleeUntil;
	private float _fleeUpdateTime;
	private float _animResetTime;
	private float _nextNearbyCheck;
	private NavMeshAgent _agent;
	private GameObject _target;
	private bool _targetDead;

	protected override void OnStart()
	{
		_agent = Components.Get<NavMeshAgent>();
		Animator ??= Components.GetInChildren<CitizenAnimationHelper>( true );
		CurrentState = NpcState.Wandering;
		try { PickNewWanderTarget(); } catch { }
		OnNpcDeath += HandleNpcDeath;
	}

	protected override void OnDestroy()
	{
		OnNpcDeath -= HandleNpcDeath;
	}

	protected override void OnUpdate()
	{
		if ( Animator != null && Animator.HoldType == CitizenAnimationHelper.HoldTypes.Punch && Time.Now > _animResetTime )
			Animator.HoldType = CitizenAnimationHelper.HoldTypes.None;

		if ( _agent is null ) return;

		if ( Animator != null )
		{
			Animator.WithVelocity( _agent.Velocity );
			Animator.WithWishVelocity( _agent.Velocity );
		}

		if ( (CurrentState == NpcState.Wandering || CurrentState == NpcState.Idle) && Time.Now > _nextNearbyCheck )
		{
			_nextNearbyCheck = Time.Now + 1f;
			CheckNearbyFighting();
		}

		if ( CurrentState == NpcState.Wandering && (_agent.TargetPosition.HasValue && (_agent.TargetPosition.Value - WorldPosition).Length < 20f) )
		{
			CurrentState = NpcState.Idle;
			_idleUntil = Time.Now + Game.Random.Float( IdleMinTime, IdleMaxTime );
		}
		else if ( CurrentState == NpcState.Idle && Time.Now > _idleUntil )
		{
			CurrentState = NpcState.Wandering;
			PickNewWanderTarget();
		}
		else if ( CurrentState == NpcState.Fleeing )
		{
			if ( Time.Now < _fleeUntil )
			{
				if ( Time.Now > _fleeUpdateTime + 0.5f )
				{
					_fleeUpdateTime = Time.Now;
					FleeTo();
				}
			}
			else
			{
				CurrentState = NpcState.Wandering;
				_agent.MaxSpeed = NormalSpeed;
				PickNewWanderTarget();
			}
		}
		else if ( CurrentState == NpcState.Fighting )
		{
			if ( _target is null || _targetDead || !_target.IsValid() )
			{
				_targetDead = false;
				CurrentState = NpcState.Fleeing;
				_agent.MaxSpeed = FleeSpeed;
				_fleeUntil = Time.Now + 3f;
				_target = null;
				FleeTo();
				return;
			}

			if ( Vector3.DistanceBetween( WorldPosition, _target.WorldPosition ) > AttackRange )
			{
				_agent.MoveTo( _target.WorldPosition );
			}
			else
			{
				_agent.Stop();
				WorldRotation = Rotation.LookAt( (_target.WorldPosition - WorldPosition).WithZ( 0 ).Normal, Vector3.Up );

				if ( _lastAttackTime + 1f < Time.Now )
				{
					_lastAttackTime = Time.Now;
					if ( Animator != null )
					{
						Animator.HoldType = CitizenAnimationHelper.HoldTypes.Punch;
						Animator.Target.Set( "b_attack", true );
						_animResetTime = Time.Now + 0.4f;
					}
					var origin = WorldPosition + Vector3.Up * 50f;
					var end = origin + ((_target.WorldPosition - WorldPosition).Normal * AttackRange);
					var hit = Scene.Trace.Sphere( 24f, origin, end ).IgnoreGameObjectHierarchy( GameObject ).Run();
					if ( hit.Hit && hit.GameObject is not null )
					{
						var health = hit.GameObject.Components.Get<HealthComponent>( FindMode.EverythingInSelfAndAncestors );
						health?.TakeDamage( 10f, GameObject );
					}
				}
			}
		}
	}

	public void ClearTarget()
	{
		_targetDead = true;
		var lastTargetPosition = _target?.WorldPosition;
		_target = null;
		if ( CurrentState == NpcState.Fighting )
		{
			CurrentState = NpcState.Fleeing;
			_agent.MaxSpeed = FleeSpeed;
			_fleeUntil = Time.Now + 3f;
			var randomDir = new Vector3( Game.Random.Float( -1f, 1f ), Game.Random.Float( -1f, 1f ), 0f ).Normal;
			var fleeTarget = WorldPosition + randomDir * FleeDistance;
			var closest = Scene.NavMesh?.GetClosestPoint( fleeTarget );
			if ( closest.HasValue ) _agent.MoveTo( closest.Value );
		}
	}

	public void OnDamaged( GameObject attacker )
	{
		if ( CurrentState == NpcState.Fighting || CurrentState == NpcState.Fleeing ) { _target = attacker; return; }

		_target = attacker;

		if ( Game.Random.Float( 0f, 1f ) < Bravery )
		{
			CurrentState = NpcState.Fighting;
			_agent.MaxSpeed = FleeSpeed;
		}
		else
		{
			CurrentState = NpcState.Fleeing;
			_agent.MaxSpeed = FleeSpeed;
			_fleeUntil = Time.Now + FleeDuration;
			FleeTo();
		}
	}

	private void HandleNpcDeath( GameObject attacker, GameObject victim )
	{
		if ( victim == GameObject ) return;
		if ( attacker == GameObject ) return;
		if ( attacker is null || !attacker.IsValid() ) return;
		if ( Vector3.DistanceBetween( WorldPosition, victim.WorldPosition ) > DetectionRadius ) return;

		if ( Game.Random.Float( 0f, 1f ) < Bravery )
		{
			_target = attacker;
			CurrentState = NpcState.Fighting;
			_agent.MaxSpeed = NormalSpeed;
		}
		else
		{
			_target = attacker;
			CurrentState = NpcState.Fleeing;
			_agent.MaxSpeed = FleeSpeed;
			_fleeUntil = Time.Now + FleeDuration;
			FleeTo();
		}
	}

	private void CheckNearbyFighting()
	{
		foreach ( var brain in Scene.GetAllComponents<NpcBrain>() )
		{
			if ( brain == this || brain.CurrentState != NpcState.Fighting ) continue;
			if ( Vector3.DistanceBetween( WorldPosition, brain.WorldPosition ) > DetectionRadius ) continue;
			if ( brain.Target is null ) continue;

			if ( Game.Random.Float( 0f, 1f ) < Bravery )
			{
				_target = brain.Target;
				CurrentState = NpcState.Fighting;
				_agent.MaxSpeed = FleeSpeed;
			}
			else
			{
				_target = brain.Target;
				CurrentState = NpcState.Fleeing;
				_agent.MaxSpeed = FleeSpeed;
				_fleeUntil = Time.Now + FleeDuration;
				FleeTo();
			}
			break;
		}

		foreach ( var melee in Scene.GetAllComponents<MeleeAttack>() )
		{
			if ( Time.Now - melee.LastAttackTime >= 2f ) continue;
			var player = melee.GameObject;
			if ( Vector3.DistanceBetween( WorldPosition, player.WorldPosition ) > DetectionRadius ) continue;

			if ( Game.Random.Float( 0f, 1f ) < Bravery )
			{
				_target = player;
				CurrentState = NpcState.Fighting;
				_agent.MaxSpeed = FleeSpeed;
			}
			else
			{
				_target = player;
				CurrentState = NpcState.Fleeing;
				_agent.MaxSpeed = FleeSpeed;
				_fleeUntil = Time.Now + FleeDuration;
				FleeTo();
			}
			break;
		}
	}

	private void FleeTo()
	{
		if ( _target is null || Scene.NavMesh is null ) return;

		var fleeDir = (WorldPosition - _target.WorldPosition).Normal;
		var fleeTarget = WorldPosition + fleeDir * FleeDistance;
		var closest = Scene.NavMesh.GetClosestPoint( fleeTarget );
		if ( closest.HasValue )
			_agent.MoveTo( closest.Value );
	}

	private void PickNewWanderTarget()
	{
		if ( Scene.NavMesh is not null )
		{
			var randomPoint = Scene.NavMesh.GetRandomPoint();
			if ( randomPoint.HasValue )
				_agent.MoveTo( randomPoint.Value );
		}
	}
}
