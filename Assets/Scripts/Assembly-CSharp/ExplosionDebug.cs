using System.Collections.Generic;
using UnityEngine;

public class ExplosionDebug : MonoBehaviour
{
	public struct Line
	{
		public Vector3 Start;

		public Vector3 End;

		public Line(Vector3 start, Vector3 end)
		{
			Start = start;
			End = end;
		}
	}

	public static Vector3 ImpactPoint;

	public static Vector3 TestPoint;

	public static float Radius;

	public static List<Vector3> Hits = new List<Vector3>();

	public static List<Line> Protections = new List<Line>();

	public static ExplosionDebug Instance { get; private set; }

	public static bool Exists
	{
		get
		{
			return Instance != null;
		}
	}

	private void Awake()
	{
		Instance = this;
	}

	public static void Reset()
	{
		Hits.Clear();
		Protections.Clear();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(ImpactPoint, Radius);
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(TestPoint, 0.1f);
		for (int i = 0; i < Hits.Count; i++)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(Hits[i], 0.1f);
		}
		for (int j = 0; j < Protections.Count; j++)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(Protections[j].Start, Protections[j].End);
		}
	}
}
