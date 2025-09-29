using System.Collections;
using UnityEngine;

public class TutorialCinematicSequence
{
	public enum TutorialState
	{
		None = 0,
		AirlockCameraZoomIn = 1,
		AirlockCameraReady = 2,
		AirlockWelcome = 3,
		AirlockMouseLookSubtitle = 4,
		AirlockMouseLook = 5,
		AirlockWasdSubtitle = 6,
		AirlockWasdWalk = 7,
		AirlockDoorOpen = 8,
		ArmoryEnter = 9,
		ArmoryPickupMG = 10,
		ShootingRangeGameOver = 11,
		TutorialEnd = 12
	}

	private TutorialState state;

	private ITutorialCinematicSequenceListener listener;

	public TutorialState State
	{
		get
		{
			return state;
		}
	}

	public TutorialCinematicSequence(ITutorialCinematicSequenceListener l)
	{
		listener = l;
	}

	public IEnumerator StartSequences()
	{
		yield return new WaitForSeconds(1f);
		state = TutorialState.AirlockCameraZoomIn;
		if (listener != null)
		{
			listener.OnAirlockCameraZoomIn();
		}
		while (state != TutorialState.AirlockCameraReady)
		{
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds(1f);
		state = TutorialState.AirlockWelcome;
		if (listener != null)
		{
			listener.OnAirlockWelcome();
		}
		yield return new WaitForSeconds(1f);
		state = TutorialState.AirlockMouseLookSubtitle;
		if (listener != null)
		{
			listener.OnAirlockMouseLookSubtitle();
		}
		while (state != TutorialState.AirlockMouseLook)
		{
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds(3f);
		state = TutorialState.AirlockWasdSubtitle;
		if (listener != null)
		{
			listener.OnAirlockWasdSubtitle();
		}
		while (state != TutorialState.AirlockWasdWalk)
		{
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds(1f);
		state = TutorialState.AirlockDoorOpen;
		if (listener != null)
		{
			listener.OnAirlockDoorOpen();
		}
		while (state != TutorialState.ArmoryEnter)
		{
			yield return new WaitForEndOfFrame();
		}
		while (state != TutorialState.ArmoryPickupMG)
		{
			yield return new WaitForEndOfFrame();
		}
		while (state != TutorialState.ShootingRangeGameOver)
		{
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds(2f);
		state = TutorialState.TutorialEnd;
		if (listener != null)
		{
			listener.OnTutorialEnd();
		}
	}

	public void OnAirlockCameraReady()
	{
		state = TutorialState.AirlockCameraReady;
	}

	public void OnAirlockMouseLook()
	{
		state = TutorialState.AirlockMouseLook;
	}

	public void OnAirlockWasdWalk()
	{
		state = TutorialState.AirlockWasdWalk;
	}

	public void OnArmoryEnter()
	{
		state = TutorialState.ArmoryEnter;
	}

	public void OnArmoryPickupMG()
	{
		state = TutorialState.ArmoryPickupMG;
	}

	public void OnShootingRangeGameOver()
	{
		state = TutorialState.ShootingRangeGameOver;
	}
}
