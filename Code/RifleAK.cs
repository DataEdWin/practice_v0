using System;

public sealed class RifleAK : WeaponBase
{
	protected override void OnStart()
	{
		WeaponName = "AK47";
		MaxAmmo = 30;
		ReserveAmmo = 90;
		FireRate = 0.1f;
		Damage = 25f;
		IsAutomatic = true;
	}

	protected override void OnFire( Vector3 origin, Vector3 direction )
	{
		var spreadDir = ApplySpread( direction, 1f );
		var tr = Scene.Trace.Ray( origin, origin + spreadDir * Range )
			.WithoutTags( "player" )
			.Run();

		if ( !tr.Hit ) return;

		var health = tr.GameObject.Components.Get<HealthComponent>( FindMode.EverythingInSelfAndAncestors );
		health?.TakeDamage( Damage, GameObject );
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
