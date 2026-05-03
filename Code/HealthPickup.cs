public sealed class HealthPickup : RotatingPickup
{
	[Property] public float HealAmount { get; set; } = 25f;

	protected override void OnPickedUp()
	{
		foreach ( var health in Scene.GetAllComponents<HealthComponent>() )
		{
			if ( health.GameObject.Tags.Has( "player" ) )
			{
				health.Heal( HealAmount );
				return;
			}
		}
	}
}
