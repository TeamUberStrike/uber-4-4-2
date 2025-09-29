using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
	private Vector3 _lastPosition;

	private Vector3 _lastMovement;

	public Vector3 LastMovement
	{
		get
		{
			return _lastMovement;
		}
	}

	private void OnTriggerEnter(Collider c)
	{
		if (c.tag == "Player")
		{
			_lastPosition = base.transform.position;
			GameState.LocalPlayer.MoveController.Platform = this;
		}
	}

	private void OnTriggerExit(Collider c)
	{
		if (c.tag == "Player")
		{
			GameState.LocalPlayer.MoveController.Platform = null;
		}
	}

	public Vector3 GetMovementDelta()
	{
		_lastMovement = base.transform.position - _lastPosition;
		_lastPosition = base.transform.position;
		return _lastMovement;
	}
}
