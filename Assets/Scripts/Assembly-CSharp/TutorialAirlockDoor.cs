using System.Collections;
using UnityEngine;

public class TutorialAirlockDoor : MonoBehaviour
{
	public enum AnimPlayMode
	{
		Forward = 0,
		Backward = 1
	}

	public AnimPlayMode PlayMode;

	public Collider BlockCollider;

	private bool _entered;

	public void Reset()
	{
		if (_entered)
		{
			if (PlayMode == AnimPlayMode.Backward)
			{
				base.transform.rotation = Quaternion.Euler(0f, 180f + base.transform.rotation.eulerAngles.y, 0f);
			}
			else
			{
				BlockCollider.enabled = true;
			}
		}
		_entered = false;
	}

	private void OnTriggerEnter(Collider c)
	{
		if (!LevelTutorial.Instance.AirlockDoorAnim || _entered)
		{
			return;
		}
		AnimationState animationState = LevelTutorial.Instance.AirlockDoorAnim["DoorOpen"];
		_entered = true;
		if (PlayMode == AnimPlayMode.Backward)
		{
			if ((bool)animationState)
			{
				animationState.weight = 1f;
				animationState.speed = -1f;
				animationState.normalizedTime = 1f;
				animationState.enabled = true;
			}
			else
			{
				Debug.LogError("Failed to get door animation state!");
			}
			base.transform.rotation = Quaternion.Euler(0f, 180f + base.transform.rotation.eulerAngles.y, 0f);
			SfxManager.Play2dAudioClip(LevelTutorial.Instance.BigDoorClose);
		}
		else
		{
			animationState.enabled = false;
			animationState.weight = 0f;
			animationState.speed = 1f;
			animationState.normalizedTime = 0f;
			LevelTutorial.Instance.AirlockDoorAnim.Play();
			StartCoroutine(StartHideMe(animationState.length));
		}
	}

	private IEnumerator StartHideMe(float time)
	{
		yield return new WaitForSeconds(time);
		BlockCollider.enabled = false;
	}
}
