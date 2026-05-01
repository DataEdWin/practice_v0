public sealed class MoneyManager : Component
{
	public static MoneyManager Instance { get; private set; }

	[Property] public float TotalMoney { get; private set; } = 0f;

	protected override void OnStart()
	{
		Instance = this;
	}

	public void AddMoney( float amount )
	{
		TotalMoney += amount;
		Log.Info( $"Total: ${TotalMoney}" );
	}
}
