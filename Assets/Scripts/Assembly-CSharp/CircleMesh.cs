using System.Collections.Generic;
using UnityEngine;

public class CircleMesh : CustomMesh
{
	private static Vector3 Normal = new Vector3(0f, 0f, -1f);

	private static Vector3 TexShift = new Vector3(0.5f, 0.5f, 0.5f);

	private Vector3 _center;

	private float _radius;

	private float _startAngle;

	private float _fillAngle;

	public float Radius
	{
		get
		{
			return _radius;
		}
	}

	public float StartAngle
	{
		get
		{
			return _startAngle;
		}
		set
		{
			_startAngle = value;
			Reset();
		}
	}

	public float FillAngle
	{
		get
		{
			return _fillAngle;
		}
		set
		{
			_fillAngle = value;
			Reset();
		}
	}

	protected override Mesh GenerateMesh()
	{
		Quaternion quaternion = Quaternion.Euler(0f, 0f, 0f - _startAngle);
		Vector3 vector = quaternion * Vector3.up;
		int num = (int)Mathf.Clamp(Mathf.Abs(_fillAngle) * 0.1f, 5f, 30f);
		float num2 = 1f / (float)(num - 1);
		Quaternion quaternion2 = Quaternion.AngleAxis(_fillAngle * num2, Normal);
		Vector3 vector2 = vector * _radius;
		float num3 = 1f / (2f * _radius);
		Vector3 b = new Vector3(num3, num3, num3);
		List<int> list = new List<int>();
		List<Vector3> list2 = new List<Vector3>();
		List<Vector3> list3 = new List<Vector3>();
		List<Vector2> list4 = new List<Vector2>();
		for (int i = 0; i < num - 1; i++)
		{
			Vector3 vector3 = vector2;
			vector2 = quaternion2 * vector2;
			list4.Add(TexShift);
			list2.Add(_center);
			list3.Add(Normal);
			list.Add(i * 3);
			list4.Add(TexShift + Vector3.Scale(vector3, b));
			list2.Add(_center + vector3);
			list3.Add(Normal);
			list.Add(i * 3 + 1);
			list4.Add(TexShift + Vector3.Scale(vector2, b));
			list2.Add(_center + vector2);
			list3.Add(Normal);
			list.Add(i * 3 + 2);
		}
		Mesh mesh = new Mesh();
		mesh.vertices = list2.ToArray();
		mesh.normals = list3.ToArray();
		mesh.uv = list4.ToArray();
		mesh.subMeshCount = 1;
		mesh.SetTriangles(list.ToArray(), 0);
		return mesh;
	}

	private void Awake()
	{
		_radius = 0.5f;
		_center = Vector3.zero;
		_startAngle = 0f;
		_fillAngle = 360f;
		Reset();
	}
}
