public sealed class ShieldPickup : RotatingPickup
{
	[Property] public float ShieldAmount { get; set; } = 50f;

	protected override void OnPickedUp()
	{
		var hud = Scene.GetAllComponents<PlayerHud>().FirstOrDefault();
		if ( hud is null ) return;

		hud.CurrentShield = System.Math.Clamp( hud.CurrentShield + ShieldAmount, 0f, hud.MaxShield );
	}
}
