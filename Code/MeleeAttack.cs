using Sandbox.Citizen;

public sealed class MeleeAttack : Component
{
	[Property] public float Damage { get; set; } = 25f;
	[Property] public float Range { get; set; } = 100f;
	[Property] public float AttackCooldown { get; set; } = 0.5f;
	[Property] public float AnimationResetDelay { get; set; } = 0.4f;

	public float LastAttackTime { get; private set; } = -999f;

	private float _lastAttackTime;

	protected override void OnUpdate()
	{
		var helper = Components.Get<CitizenAnimationHelper>( true );

		if ( helper is not null && helper.HoldType == CitizenAnimationHelper.HoldTypes.Punch && Time.Now > _lastAttackTime + AnimationResetDelay )
			helper.HoldType = CitizenAnimationHelper.HoldTypes.None;

		if ( !Input.Pressed( "attack1" ) || Time.Now < _lastAttackTime + AttackCooldown )
			return;

		_lastAttackTime = Time.Now;
		LastAttackTime = Time.Now;

		if ( helper is not null )
		{
			helper.HoldType = CitizenAnimationHelper.HoldTypes.Punch;
			helper.Target.Set( "b_attack", true );
		}

		var origin = WorldPosition + Vector3.Up * 50f;
		var end = origin + WorldRotation.Forward * Range;

		var result = Scene.Trace
			.Sphere( 24f, origin, end )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( result.Hit && result.GameObject is not null )
		{
			var health = result.GameObject.Components.Get<HealthComponent>( FindMode.EverythingInSelfAndAncestors );
			Log.Info( "Hit: " + result.GameObject.Name + " | Has Health: " + (health != null) );
			health?.TakeDamage( Damage, GameObject );
		}
	}
}
