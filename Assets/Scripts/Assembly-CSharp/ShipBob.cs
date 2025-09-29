using UnityEngine;

public class ShipBob : MonoBehaviour
{
	[SerializeField]
	private float rotateAmount = 1f;

	[SerializeField]
	private float moveAmount = 0.005f;

	private Transform _transform;

	private Vector3 shipRotation;

	private void Awake()
	{
		_transform = base.transform;
		shipRotation = _transform.localRotation.eulerAngles;
	}

	private void Update()
	{
		_transform.position = new Vector3(_transform.position.x, _transform.position.y + Mathf.Sin(Time.time) * moveAmount, _transform.position.z);
		float num = Mathf.Sin(Time.time) * rotateAmount;
		_transform.localRotation = Quaternion.Euler(shipRotation + new Vector3(num, num, num));
	}
}
