public sealed class MoneyPickup : RotatingPickup
{
	[Property] public float Value { get; set; } = 100f;

	protected override void OnPickedUp()
	{
		MoneyManager.Instance.AddMoney( Value );
	}
}
