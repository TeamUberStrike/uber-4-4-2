using UnityEngine;

[RequireComponent(typeof(Animation))]
public class AnimationSynchronizer : MonoBehaviour
{
	private AnimationState animationState;

	private void Start()
	{
		animationState = base.GetComponent<Animation>()[base.GetComponent<Animation>().clip.name];
	}

	private void Update()
	{
		if (GameState.HasCurrentGame && GameState.CurrentGame.IsMatchRunning)
		{
			animationState.time = GameState.CurrentGame.GameTime;
		}
		else
		{
			animationState.time += Time.deltaTime;
		}
	}
}
