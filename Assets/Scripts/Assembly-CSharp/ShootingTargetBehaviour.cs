using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingTargetBehaviour : MonoBehaviour
{
	private List<TutorialShootingTarget> _targets = new List<TutorialShootingTarget>(6);

	[SerializeField]
	private Transform[] _targetPositons;

	[SerializeField]
	private TutorialShootingTarget _targetPrefab;

	private IEnumerator StartShootingRange()
	{
		while (_targets.Count > 0)
		{
			yield return new WaitForSeconds(1f);
			_targets.ForEach(delegate(TutorialShootingTarget t)
			{
				t.Reset();
			});
			SfxManager.Play2dAudioClip(GameAudio.TargetPopup);
			while (!_targets.TrueForAll((TutorialShootingTarget t) => t.IsHit))
			{
				yield return new WaitForSeconds(1f);
			}
		}
	}

	public void StartGame()
	{
		if (_targets.Count > 0)
		{
			StopGame();
		}
		Transform[] targetPositons = _targetPositons;
		foreach (Transform transform in targetPositons)
		{
			TutorialShootingTarget tutorialShootingTarget = Object.Instantiate(_targetPrefab, transform.position, transform.rotation) as TutorialShootingTarget;
			tutorialShootingTarget.transform.parent = base.transform;
			_targets.Add(tutorialShootingTarget);
		}
		StartCoroutine(StartShootingRange());
	}

	public void StopGame()
	{
		for (int i = 0; i < _targets.Count; i++)
		{
			Object.Destroy(_targets[i].gameObject);
		}
		_targets.Clear();
	}
}
