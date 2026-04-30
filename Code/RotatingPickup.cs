using System;

public sealed class RotatingPickup : Component, Component.ITriggerListener
{
	[Property] public float RotateSpeed { get; set; } = 90f;
	[Property] public float BobHeight { get; set; } = 10f;
	[Property] public float BobSpeed { get; set; } = 2f;

	private Vector3 _startPosition;

	protected override void OnStart()
	{
		_startPosition = LocalPosition;
	}

	protected override void OnUpdate()
	{
		LocalRotation *= Rotation.FromYaw( RotateSpeed * Time.Delta );

		float bob = (float)System.Math.Sin( Time.Now * BobSpeed ) * BobHeight;
		LocalPosition = _startPosition + Vector3.Up * bob;
	}

	void Component.ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( !other.GameObject.Tags.Has( "player" ) )
			return;

		Log.Info( "Pickup collected!" );
		GameObject.Destroy();
	}
}
