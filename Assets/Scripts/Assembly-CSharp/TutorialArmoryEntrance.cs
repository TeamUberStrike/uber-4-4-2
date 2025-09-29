using System.Collections;
using UnityEngine;

public class TutorialArmoryEntrance : MonoBehaviour
{
	public Collider Block;

	private bool _entered;

	private void OnTriggerEnter(Collider c)
	{
		if (!_entered && c.tag == "Player")
		{
			_entered = true;
			if (GameState.HasCurrentGame && GameState.CurrentGame is TutorialGameMode)
			{
				TutorialGameMode tutorialGameMode = GameState.CurrentGame as TutorialGameMode;
				tutorialGameMode.Sequence.OnArmoryEnter();
				Block.isTrigger = false;
				StartCoroutine(StartDeleteMe());
			}
		}
	}

	private IEnumerator StartDeleteMe()
	{
		yield return new WaitForEndOfFrame();
		Object.Destroy(base.gameObject);
	}
}
