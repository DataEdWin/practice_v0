using System;
using System.Threading.Tasks;

public abstract class WeaponBase : Component
{
	[Property] public string WeaponName { get; set; }
	[Property] public int MaxAmmo { get; set; } = 30;
	[Property] public int CurrentAmmo { get; set; }
	[Property] public int ReserveAmmo { get; set; } = 90;
	[Property] public float FireRate { get; set; } = 0.1f;
	[Property] public float Damage { get; set; } = 25f;
	[Property] public float Range { get; set; } = 5000f;
	[Property] public bool IsAutomatic { get; set; } = false;
	[Property] public GameObject WeaponModel { get; set; }

	private float _lastFireTime;
	private bool _isReloading;
	public bool IsEquipped;

	public bool IsReloading => _isReloading;

	protected abstract void OnFire( Vector3 origin, Vector3 direction );

	public virtual void TryFire( Vector3 origin, Vector3 direction )
	{
		if ( _isReloading ) return;
		if ( CurrentAmmo <= 0 ) return;
		if ( Time.Now - _lastFireTime < FireRate ) return;

		_lastFireTime = Time.Now;
		CurrentAmmo--;
		OnFire( origin, direction );
	}

	public virtual void StartReload()
	{
		if ( _isReloading ) return;
		if ( CurrentAmmo >= MaxAmmo ) return;
		if ( ReserveAmmo <= 0 ) return;

		_isReloading = true;
		_ = DoReload();
	}

	private async Task DoReload()
	{
		await Task.Delay( 2000 );
		CompleteReload();
	}

	public virtual void CompleteReload()
	{
		int ammoToAdd = Math.Min( MaxAmmo - CurrentAmmo, ReserveAmmo );
		CurrentAmmo += ammoToAdd;
		ReserveAmmo -= ammoToAdd;
		_isReloading = false;
	}
}
