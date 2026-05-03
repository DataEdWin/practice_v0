using System;

public sealed class Shotgun : WeaponBase
{
	protected override void OnStart()
	{
		WeaponName = "Shotgun";
		MaxAmmo = 6;
		ReserveAmmo = 30;
		FireRate = 0.8f;
		Damage = 15f;
		IsAutomatic = false;
	}

	protected override void OnFire( Vector3 origin, Vector3 direction )
	{
		for ( int i = 0; i < 8; i++ )
		{
			var pelletDir = ApplySpread( direction, 3f );
			var tr = Scene.Trace.Ray( origin, origin + pelletDir * Range )
				.WithoutTags( "player" )
				.Run();

			if ( !tr.Hit ) continue;

			var health = tr.GameObject.Components.Get<HealthComponent>( FindMode.EverythingInSelfAndAncestors );
			health?.TakeDamage( Damage, GameObject );
		}
	}

	private Vector3 ApplySpread( Vector3 dir, float degrees )
	{
		float spread = (float)Math.Tan( degrees * Math.PI / 180.0 );
		var right = Vector3.Cross( dir, Vector3.Up ).Normal;
		var up = Vector3.Cross( right, dir ).Normal;
		float x = (Random.Shared.NextSingle() * 2f - 1f) * spread;
		float y = (Random.Shared.NextSingle() * 2f - 1f) * spread;
		return (dir + right * x + up * y).Normal;
	}
}
