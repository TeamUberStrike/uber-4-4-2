using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Draggable Panel Extended")]
public class UIDraggablePanelExtended : UIDraggablePanel
{
	protected GameObject selectedItem;

	public GameObject SelectedItem
	{
		get
		{
			return selectedItem;
		}
		set
		{
			selectedItem = value;
		}
	}

	public void SpringToSelection(GameObject selectedObject, float springStrength)
	{
		SelectedItem = selectedObject;
		SpringToPosition(selectedObject.transform.position, springStrength);
	}

	public void SpringToSelection(Vector3 selectedPosition, float springStrength)
	{
		SpringToPosition(selectedPosition, springStrength);
	}

	public void PlayTweenOnSelected(bool forward)
	{
		if (!(selectedItem != null))
		{
			return;
		}
		UITweener[] componentsInChildren = selectedItem.GetComponentsInChildren<UITweener>();
		if (componentsInChildren == null)
		{
			return;
		}
		UITweener[] array = componentsInChildren;
		foreach (UITweener uITweener in array)
		{
			if (uITweener != null)
			{
				uITweener.Reset();
				uITweener.Play(forward);
			}
		}
	}

	private void SpringToPosition(Vector3 positionToSpring, float springStrength)
	{
		Vector4 clipRange = base.panel.clipRange;
		Transform cachedTransform = base.panel.cachedTransform;
		Vector3 localPosition = cachedTransform.localPosition;
		localPosition.x += clipRange.x;
		localPosition.y += clipRange.y;
		localPosition = cachedTransform.parent.TransformPoint(localPosition);
		base.currentMomentum = Vector3.zero;
		Vector3 vector = cachedTransform.InverseTransformPoint(positionToSpring);
		Vector3 vector2 = cachedTransform.InverseTransformPoint(localPosition);
		Vector3 vector3 = vector - vector2;
		if (scale.x == 0f)
		{
			vector3.x = 0f;
		}
		if (scale.y == 0f)
		{
			vector3.y = 0f;
		}
		if (scale.z == 0f)
		{
			vector3.z = 0f;
		}
		SpringPanel.Begin(base.gameObject, cachedTransform.localPosition - vector3, 20f);
	}
}
