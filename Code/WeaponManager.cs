using System.Collections.Generic;

public sealed class WeaponManager : Component
{
	[Property] public List<WeaponBase> Weapons { get; set; } = new();
	[Property] public SkinnedModelRenderer PlayerBody { get; set; }
	public WeaponBase CurrentWeapon { get; set; }
	private int _currentIndex;
	private GameObject _weaponModelInstance;

	protected override void OnStart()
	{
		if ( Weapons.Count > 0 )
		{
			CurrentWeapon = Weapons[0];
			CurrentWeapon.IsEquipped = true;
			AttachWeaponModel( CurrentWeapon );
		}
	}

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "slot1" ) ) EquipWeapon( 0 );
		if ( Input.Pressed( "slot2" ) ) EquipWeapon( 1 );
		if ( Input.Pressed( "slot3" ) ) EquipWeapon( 2 );

		if ( CurrentWeapon is not null )
		{
			var direction = Scene.Camera.Transform.Rotation.Forward;
			var origin = Transform.Position + Vector3.Up * 64f;

			bool shouldFire = CurrentWeapon.IsAutomatic
				? Input.Down( "attack1" )
				: Input.Pressed( "attack1" );

			if ( shouldFire )
				CurrentWeapon.TryFire( origin, direction );

			if ( Input.Pressed( "reload" ) )
				CurrentWeapon.StartReload();

			// ADS — handle later
			if ( Input.Down( "attack2" ) ) { }
		}
	}

	private void EquipWeapon( int index )
	{
		if ( index < 0 || index >= Weapons.Count ) return;

		if ( CurrentWeapon is not null )
			CurrentWeapon.IsEquipped = false;

		_currentIndex = index;
		CurrentWeapon = Weapons[index];
		CurrentWeapon.IsEquipped = true;
		AttachWeaponModel( CurrentWeapon );
	}

	private void AttachWeaponModel( WeaponBase weapon )
	{
		if ( _weaponModelInstance is not null )
		{
			_weaponModelInstance.Destroy();
			_weaponModelInstance = null;
		}

		if ( weapon?.WeaponModel is null ) return;

		var boneGo = PlayerBody?.GetBoneObject( "hand_R" );
		if ( boneGo is null ) return;

		_weaponModelInstance = weapon.WeaponModel.Clone( boneGo.WorldPosition, boneGo.WorldRotation );
		_weaponModelInstance.Parent = boneGo;
		_weaponModelInstance.LocalPosition = Vector3.Zero;
		_weaponModelInstance.LocalRotation = Rotation.Identity;
	}
}
