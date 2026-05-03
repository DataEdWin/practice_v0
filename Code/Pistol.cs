public sealed class Pistol : WeaponBase
{
	protected override void OnStart()
	{
		WeaponName = "Pistol";
		MaxAmmo = 12;
		ReserveAmmo = 60;
		FireRate = 0.3f;
		Damage = 35f;
		IsAutomatic = false;
	}

	protected override void OnFire( Vector3 origin, Vector3 direction )
	{
		var tr = Scene.Trace.Ray( origin, origin + direction * Range )
			.WithoutTags( "player" )
			.Run();

		if ( !tr.Hit ) return;

		var health = tr.GameObject.Components.Get<HealthComponent>( FindMode.EverythingInSelfAndAncestors );
		health?.TakeDamage( Damage, GameObject );
	}
}
