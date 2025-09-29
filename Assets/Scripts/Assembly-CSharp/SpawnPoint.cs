using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	[SerializeField]
	private bool DrawGizmos = true;

	[SerializeField]
	private float Radius = 1f;

	[SerializeField]
	public TeamID TeamPoint;

	[SerializeField]
	public GameMode GameMode;

	public Vector3 Position
	{
		get
		{
			return base.transform.position;
		}
	}

	public Vector2 Rotation
	{
		get
		{
			return new Vector2(base.transform.rotation.eulerAngles.y, base.transform.rotation.eulerAngles.x);
		}
	}

	public TeamID TeamId
	{
		get
		{
			return TeamPoint;
		}
	}

	public float SpawnRadius
	{
		get
		{
			return Radius;
		}
	}

	private void OnDrawGizmos()
	{
		if (DrawGizmos)
		{
			switch (TeamPoint)
			{
			case TeamID.NONE:
				Gizmos.color = Color.green;
				break;
			case TeamID.RED:
				Gizmos.color = Color.red;
				break;
			case TeamID.BLUE:
				Gizmos.color = Color.blue;
				break;
			}
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, Quaternion.identity, new Vector3(1f, 0.1f, 1f));
			Gizmos.DrawSphere(Vector3.zero, Radius);
			switch (GameMode)
			{
			case GameMode.DeathMatch:
				Gizmos.color = Color.yellow;
				break;
			case GameMode.TeamDeathMatch:
				Gizmos.color = Color.white;
				break;
			}
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.DrawLine(base.transform.position + base.transform.forward * Radius, base.transform.position + base.transform.forward * 2f * Radius);
		}
	}
}
