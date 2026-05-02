using Sandbox.Citizen;
using Sandbox.Navigation;

public sealed class NpcBrain : Component
{
	public enum NpcState { Wandering, Idle, Fighting, Fleeing }

	[Property] public float WanderRadius { get; set; } = 300f;
	[Property] public float IdleMinTime { get; set; } = 2f;
	[Property] public float IdleMaxTime { get; set; } = 5f;
	[Property] public float AttackRange { get; set; } = 80f;
	[Property] public float FleeDistance { get; set; } = 400f;
	[Property] public float NormalSpeed { get; set; } = 80f;
	[Property] public float FleeSpeed { get; set; } = 200f;
	[Property] public float FleeDuration { get; set; } = 30f;
	[Property] public CitizenAnimationHelper Animator { get; set; }

	public NpcState CurrentState;
	private float _idleUntil;
	private float _lastAttackTime;
	private float _fleeUntil;
	private float _fleeUpdateTime;
	private float _animResetTime;
	private NavMeshAgent _agent;
	private GameObject _target;

	protected override void OnStart()
	{
		_agent = Components.Get<NavMeshAgent>();
		Animator ??= Components.GetInChildren<CitizenAnimationHelper>( true );
		CurrentState = NpcState.Wandering;
		try { PickNewWanderTarget(); } catch { }
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
		else if ( CurrentState == NpcState.Fighting && _target is not null )
		{
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

	public void OnDamaged( GameObject attacker )
	{
		_target = attacker;

		if ( Game.Random.Float( 0f, 1f ) < 0.5f )
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
