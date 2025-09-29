using UnityEngine;

public class SoundArea : MonoBehaviour
{
	[SerializeField]
	private FootStepSoundType _footStep;

	private void OnTriggerEnter(Collider other)
	{
		SetFootStep(other);
	}

	private void OnTriggerStay(Collider other)
	{
		SetFootStep(other);
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Avatar")
		{
			CharacterTrigger component = other.GetComponent<CharacterTrigger>();
			if ((bool)component && (bool)component.Character && (bool)component.Character.Avatar.Decorator && GameState.HasCurrentSpace)
			{
				component.Character.Avatar.Decorator.SetFootStep(GameState.CurrentSpace.DefaultFootStep);
			}
		}
	}

	private void SetFootStep(Collider other)
	{
		if (other.tag == "Avatar")
		{
			CharacterTrigger component = other.GetComponent<CharacterTrigger>();
			if ((bool)component && (bool)component.Character && (bool)component.Character.Avatar.Decorator)
			{
				component.Character.Avatar.Decorator.SetFootStep(_footStep);
			}
		}
	}
}
