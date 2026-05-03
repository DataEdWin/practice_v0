public class HealthComponent : Component
{
	[Property] public float MaxHealth { get; set; } = 100f;

	public float CurrentHealth;

	public bool IsDead => CurrentHealth <= 0;

	private GameObject _lastAttacker;

	protected override void OnStart()
	{
		CurrentHealth = MaxHealth;
	}

	public void Heal( float amount )
	{
		CurrentHealth = System.Math.Clamp( CurrentHealth + amount, 0f, MaxHealth );
	}

	public void TakeDamage( float amount, GameObject attacker = null )
	{
		CurrentHealth = System.Math.Max( CurrentHealth - amount, 0f );
		_lastAttacker = attacker;
		Log.Info( $"Took damage: {amount}, remaining: {CurrentHealth}" );

		var brain = Components.Get<NpcBrain>();
		if ( brain is not null )
			brain.OnDamaged( attacker );

		if ( CurrentHealth <= 0 )
			OnDeath();
	}

	protected virtual void OnDeath()
	{
		Log.Info( "Entity died!" );

		var lootTable = Components.Get<LootTableComponent>();
		if ( lootTable is not null )
			lootTable.DropLoot( WorldPosition );

		foreach ( var brain in Scene.GetAllComponents<NpcBrain>() )
		{
			if ( brain.Target == GameObject )
				brain.ClearTarget();
		}

		var npcBrain = Components.Get<NpcBrain>();
		if ( npcBrain is not null )
			NpcBrain.OnNpcDeath?.Invoke( _lastAttacker, GameObject );

		GameObject.Destroy();
	}
}
