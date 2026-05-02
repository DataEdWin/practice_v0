using Sandbox.Citizen;
using Sandbox.Navigation;

public sealed class NpcBrain : Component
{
	public enum NpcState { Wandering, Idle }

	[Property] public float WanderRadius { get; set; } = 300f;
	[Property] public float IdleMinTime { get; set; } = 2f;
	[Property] public float IdleMaxTime { get; set; } = 5f;
	[Property] public CitizenAnimationHelper Animator { get; set; }

	public NpcState CurrentState;
	private float _idleUntil;
	private NavMeshAgent _agent;

	protected override void OnStart()
	{
		_agent = Components.Get<NavMeshAgent>();
		Animator ??= Components.GetInChildren<CitizenAnimationHelper>( true );
		CurrentState = NpcState.Wandering;
		try { PickNewWanderTarget(); } catch { }
	}

	protected override void OnUpdate()
	{
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
