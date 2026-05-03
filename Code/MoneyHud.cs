public sealed class PlayerHud : Component
{
	[Property] public HealthComponent PlayerHealth { get; set; }
	[Property] public float MaxShield { get; set; } = 100f;

	public float CurrentShield;

	protected override void OnUpdate()
	{
		if ( Scene.Camera is null ) return;

		var hud = Scene.Camera.Hud;
		var screenWidth = Screen.Width;
		var anchor = new Vector2( screenWidth - 250, 20 );

		hud.DrawRect( new Rect( anchor.x, anchor.y, 250, 120 ), new Color( 0f, 0f, 0f, 0.6f ) );

		hud.DrawText( new TextRendering.Scope( "👊", Color.White, 36 ), anchor + new Vector2( 8, 8 ) );

		var shieldFill = MaxShield > 0f ? System.Math.Clamp( CurrentShield / MaxShield, 0f, 1f ) : 0f;
		hud.DrawText( new TextRendering.Scope( "🛡️", Color.White, 16 ), anchor + new Vector2( 52, 28 ) );
		hud.DrawRect( new Rect( anchor.x + 80, anchor.y + 28, 160, 14 ), new Color( 0.1f, 0.1f, 0.2f, 0.8f ) );
		hud.DrawRect( new Rect( anchor.x + 80, anchor.y + 28, 160 * shieldFill, 14 ), new Color( 0.2f, 0.5f, 1f, 1f ) );

		var healthFill = PlayerHealth is not null && PlayerHealth.MaxHealth > 0f
			? System.Math.Clamp( PlayerHealth.CurrentHealth / PlayerHealth.MaxHealth, 0f, 1f )
			: 0f;
		hud.DrawText( new TextRendering.Scope( "❤️", Color.White, 16 ), anchor + new Vector2( 52, 52 ) );
		hud.DrawRect( new Rect( anchor.x + 80, anchor.y + 52, 160, 14 ), new Color( 0.2f, 0.05f, 0.05f, 0.8f ) );
		hud.DrawRect( new Rect( anchor.x + 80, anchor.y + 52, 160 * healthFill, 14 ), new Color( 0.9f, 0.15f, 0.15f, 1f ) );

		var money = (int)(MoneyManager.Instance?.TotalMoney ?? 0f);
		hud.DrawText( new TextRendering.Scope( "$" + money, Color.Green, 22, "Poppins" ), anchor + new Vector2( 52, 82 ) );
	}
}
