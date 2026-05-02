public class HealthComponent : Component
{
	[Property] public float MaxHealth { get; set; } = 100f;

	public float CurrentHealth;

	public bool IsDead => CurrentHealth <= 0;

	protected override void OnStart()
	{
		CurrentHealth = MaxHealth;
	}

	public void TakeDamage( float amount )
	{
		CurrentHealth = System.Math.Max( CurrentHealth - amount, 0f );
		Log.Info( $"Took damage: {amount}, remaining: {CurrentHealth}" );

		if ( CurrentHealth <= 0 )
			OnDeath();
	}

	protected virtual void OnDeath()
	{
		Log.Info( "Entity died!" );

		var lootTable = Components.Get<LootTableComponent>();
		if ( lootTable is not null )
			lootTable.DropLoot( WorldPosition );

		GameObject.Destroy();
	}
}
