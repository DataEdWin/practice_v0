public sealed class LootTableComponent : Component
{
	[Property] public GameObject MoneyPrefab { get; set; }
	[Property, Range( 0f, 1f )] public float BonusLootChance { get; set; } = 0.5f;
	[Property] public int BonusLootMin { get; set; } = 1;
	[Property] public int BonusLootMax { get; set; } = 3;

	public void DropLoot( Vector3 position )
	{
		if ( MoneyPrefab is null )
			return;

		SpawnAt( position + RandomOffset() );

		if ( Game.Random.Float( 0f, 1f ) < BonusLootChance )
		{
			int count = Game.Random.Int( BonusLootMin, BonusLootMax );
			for ( int i = 0; i < count; i++ )
				SpawnAt( position + RandomOffset() );
		}
	}

	private void SpawnAt( Vector3 pos )
	{
		var go = MoneyPrefab.Clone( pos );
		go.SetParent( null );
		go.Enabled = true;
	}

	private Vector3 RandomOffset()
	{
		float x = Game.Random.Float( -20f, 20f );
		float y = Game.Random.Float( -20f, 20f );
		return new Vector3( x, y, 0f );
	}
}
