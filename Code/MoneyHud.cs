public sealed class MoneyHud : Component
{
	protected override void OnUpdate()
	{
		if ( Scene.Camera is null ) return;

		var hud = Scene.Camera.Hud;

		hud.DrawRect( new Rect( 20, 20, 220, 50 ), new Color( 0, 0, 0, 0.5f ) );

		hud.DrawText( new TextRendering.Scope( "💵 $" + ((int)(MoneyManager.Instance?.TotalMoney ?? 0)).ToString(), Color.Green, 28, "Poppins" ), new Vector2( 30, 30 ) );
	}
}
