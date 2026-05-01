public sealed class MeleeAttack : Component
{
	[Property] public float Damage { get; set; } = 25f;
	[Property] public float Range { get; set; } = 100f;
	[Property] public float AttackCooldown { get; set; } = 0.5f;

	private float _lastAttackTime;

	protected override void OnUpdate()
	{
		if ( !Input.Pressed( "attack1" ) || Time.Now < _lastAttackTime + AttackCooldown )
			return;

		_lastAttackTime = Time.Now;

		var origin = WorldPosition + Vector3.Up * 50f;
		var result = Scene.Trace
			.Ray( origin, origin + WorldRotation.Forward * Range )
			.WithoutTags( "player" )
			.Run();

		if ( result.Hit && result.GameObject is not null )
		{
			result.GameObject.Components.Get<HealthComponent>()?.TakeDamage( Damage );
			Log.Info( "Hit: " + result.GameObject.Name );
		}
	}
}
