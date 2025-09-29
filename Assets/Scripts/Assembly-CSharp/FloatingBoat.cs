using System.Collections;
using UnityEngine;

public class FloatingBoat : MonoBehaviour
{
	public float Offset = 1f;

	public float Force = 400f;

	public Transform bug;

	public Transform heck1;

	public Transform heck2;

	private Rigidbody rb;

	private Transform tf;

	private float torque;

	private void Start()
	{
		rb = base.GetComponent<Rigidbody>();
		tf = base.transform;
	}

	private void OnEnable()
	{
		StopCoroutine("startKeepBoatUpright");
		StartCoroutine("startKeepBoatUpright");
	}

	private IEnumerator startKeepBoatUpright()
	{
		while (true)
		{
			float tor = tf.localRotation.eulerAngles.z - 180f;
			if (Mathf.Abs(tor) < 90f)
			{
				torque = Mathf.Lerp(torque, 5f * (90f - tor), 10f * Time.deltaTime);
			}
			else
			{
				torque = 0f;
			}
			rb.AddRelativeTorque(new Vector3(0f, 0f, torque));
			yield return new WaitForFixedUpdate();
		}
	}
}
