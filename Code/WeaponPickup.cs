public sealed class WeaponPickup : RotatingPickup
{
	[Property] public WeaponBase WeaponPrefab { get; set; }

	protected override void OnPickedUp()
	{
		var manager = Scene.GetAllComponents<WeaponManager>().FirstOrDefault();
		if ( manager is null ) return;

		var clone = WeaponPrefab.GameObject.Clone();
		clone.Parent = manager.GameObject;

		var weapon = clone.Components.Get<WeaponBase>();
		if ( weapon is not null )
			manager.Weapons.Add( weapon );

		clone.Components.Get<RotatingPickup>()?.Destroy();
	}
}
