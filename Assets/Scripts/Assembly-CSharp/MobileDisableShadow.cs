using UnityEngine;

public class MobileDisableShadow : MonoBehaviour
{
	private void OnEnable()
	{
		base.gameObject.GetComponent<Light>().shadows = LightShadows.None;
	}
}
