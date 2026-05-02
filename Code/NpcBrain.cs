using Sandbox.Navigation;

public sealed class NpcBrain : Component
{
	public enum NpcState { Wandering, Idle }

	[Property] public float WanderRadius { get; set; } = 300f;
	[Property] public float IdleMinTime { get; set; } = 2f;
	[Property] public float IdleMaxTime { get; set; } = 5f;

	public NpcState CurrentState;
	private float _idleUntil;
	private NavMeshAgent _agent;

	protected override void OnStart()
	{
		_agent = Components.Get<NavMeshAgent>();
		CurrentState = NpcState.Wandering;
		try { PickNewWanderTarget(); } catch { }
	}

	protected override void OnUpdate()
	{
		if ( _agent is null ) return;

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
		float x = Game.Random.Float( -WanderRadius, WanderRadius );
		float y = Game.Random.Float( -WanderRadius, WanderRadius );
		Log.Info( "NavMesh: " + (Scene.NavMesh != null) );
		if ( Scene.NavMesh is not null )
		{
			_agent.MoveTo( WorldPosition + new Vector3( x, y, 0f ) );
			Log.Info( "Moving to target" );
		}
	}
}
