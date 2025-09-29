using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraComponents : MonoBehaviour
{
	public Camera Camera { get; private set; }

	public MouseOrbit MouseOrbit { get; private set; }

	private void Awake()
	{
		Camera = GetComponent<Camera>();
		MouseOrbit = GetComponent<MouseOrbit>();
	}
}
