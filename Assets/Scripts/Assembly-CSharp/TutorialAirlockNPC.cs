using System.Collections;
using UnityEngine;

public class TutorialAirlockNPC : MonoBehaviour
{
	private enum State
	{
		Moving = 0,
		Talking = 1
	}

	private Transform _transform;

	private Vector3 _finalPos = Vector3.zero;

	private State _state;

	private void Awake()
	{
		AnimationState animationState = base.GetComponent<Animation>()[AnimationIndex.TutorialGuideWalk.ToString()];
		animationState.enabled = true;
		animationState.weight = 1f;
		animationState.speed = 1f;
		_transform = base.transform;
	}

	private void Update()
	{
		if ((bool)_transform && _state == State.Moving)
		{
			if (Vector3.SqrMagnitude(_transform.position - _finalPos) < 0.1f)
			{
				_state = State.Talking;
				base.GetComponent<Animation>().Stop(AnimationIndex.heavyGunUpDown.ToString());
				base.GetComponent<Animation>().Blend(AnimationIndex.TutorialGuideWalk.ToString(), 0f);
				base.GetComponent<Animation>().Blend(AnimationIndex.TutorialGuideAirlock.ToString(), 1f);
				StartCoroutine(StartIdleAnimation());
			}
			else
			{
				_transform.position += _transform.forward * Time.deltaTime * 0.7f;
			}
		}
	}

	private IEnumerator StartIdleAnimation()
	{
		yield return new WaitForSeconds(5.2f);
		base.GetComponent<Animation>().Blend(AnimationIndex.TutorialGuideAirlock.ToString(), 0f);
		base.GetComponent<Animation>().Blend(AnimationIndex.TutorialGuideIdle.ToString(), 1f);
	}

	public void SetFinalPosition(Vector3 pos)
	{
		_finalPos = pos;
	}
}
