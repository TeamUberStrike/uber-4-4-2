using UnityEngine;

public class MobileDisableShadow : MonoBehaviour
{
	private void OnEnable()
	{
		base.gameObject.light.shadows = LightShadows.None;
	}
}
