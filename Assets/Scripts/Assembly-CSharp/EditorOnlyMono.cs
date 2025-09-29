using UnityEngine;

public class EditorOnlyMono : MonoBehaviour
{
	protected virtual void Awake()
	{
		base.enabled &= Application.isEditor;
	}
}
