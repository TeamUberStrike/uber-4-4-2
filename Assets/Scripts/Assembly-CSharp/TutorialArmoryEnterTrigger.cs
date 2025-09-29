using System.Collections;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class TutorialArmoryEnterTrigger : MonoBehaviour
{
	public Transform ArmoryDoor;

	public Transform ArmoryDesk;

	public SplineController ArmoryCameraPath;

	private bool _entered;

	private Vector3 _velocity;

	private void OnTriggerEnter()
	{
		if (!_entered)
		{
			_entered = true;
			GameState.LocalPlayer.IsWalkingEnabled = false;
			AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled = false;
			LevelCamera.SetBobMode(BobMode.None);
			LevelCamera.Instance.SetMode(LevelCamera.CameraMode.None);
			LevelTutorial.Instance.ArmoryWaypoint.CanShow = false;
			Object.Destroy(LevelTutorial.Instance.ArmoryWaypoint);
			StartCoroutine(StartEnterArmory());
		}
	}

	private IEnumerator StartEnterArmory()
	{
		TutorialGameMode mode = null;
		if (GameState.HasCurrentGame && GameState.CurrentGame is TutorialGameMode)
		{
			mode = GameState.CurrentGame as TutorialGameMode;
		}
		if (mode != null)
		{
			mode.ReachArmoryWaypoint();
		}
		while (Vector3.SqrMagnitude(LevelCamera.Instance.MainCamera.velocity) > 0.1f)
		{
			Vector3 v = Vector3.Lerp(LevelCamera.Instance.MainCamera.velocity, Vector3.zero, Time.deltaTime * 5f);
			LevelCamera.Instance.TransformCache.position += v * Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		LevelTutorial.Instance.ShowObjComplete = true;
		if (mode != null)
		{
			mode.ShowObjComplete();
			SfxManager.Play2dAudioClip(LevelTutorial.Instance.BigObjComplete);
		}
		yield return new WaitForSeconds(2.5f);
		LevelTutorial.Instance.IsCinematic = true;
		if (mode != null)
		{
			mode.HideObjectives();
			mode.ResetBlackBar();
			mode.HideObjComplete(false);
		}
		yield return new WaitForSeconds(0.5f);
		LevelTutorial.Instance.PickupWeapon.Show(false);
		LevelTutorial.Instance.ShowObjComplete = false;
		_velocity = Vector3.Normalize(ArmoryDoor.position - LevelCamera.Instance.TransformCache.position);
		while (true)
		{
			float speed = Vector3.Magnitude(LevelCamera.Instance.TransformCache.position - ArmoryDoor.position);
			LevelCamera.Instance.TransformCache.position += _velocity * Time.deltaTime * Mathf.Clamp(speed, 2f, 12f);
			LevelCamera.Instance.TransformCache.rotation = Quaternion.Lerp(LevelCamera.Instance.TransformCache.rotation, ArmoryDoor.rotation, Time.deltaTime * 2f);
			if (Vector3.SqrMagnitude(LevelCamera.Instance.TransformCache.position - ArmoryDoor.position) < 0.3f)
			{
				break;
			}
			yield return new WaitForEndOfFrame();
		}
		LevelTutorial.Instance.ArmoryDoor.Open();
		OnEndCallback callback = null;
		if (mode != null)
		{
			mode.EnterArmory(ArmoryCameraPath);
		}
		else
		{
			Debug.LogError("Failed to get TutorialGameMode");
		}
		_velocity = Vector3.Normalize(ArmoryDesk.position - LevelCamera.Instance.TransformCache.position);
		LevelTutorial.Instance.ShowObjPickupMG = true;
		LevelTutorial.Instance.PickupWeapon.SetItemAvailable(true);
		LevelTutorial.Instance.WeaponWaypoint.CanShow = true;
		SfxManager.Play2dAudioClip(LevelTutorial.Instance.WaypointAppear);
		ArmoryCameraPath.FollowSpline(callback);
		if ((bool)LevelTutorial.Instance.NPC)
		{
			Transform npcTransform = LevelTutorial.Instance.NPC.transform;
			npcTransform.position = new Vector3(-15.18789f, -2.342683f, -4.393633f);
			npcTransform.rotation = Quaternion.Euler(0f, 55.204f, 0f);
		}
		float time = 0f;
		bool played = false;
		while (Vector3.Dot(LevelCamera.Instance.TransformCache.position - LevelTutorial.Instance.ArmoryCameraPathEnd.position, Vector3.right) > 0f && Vector3.Distance(LevelCamera.Instance.TransformCache.position, LevelTutorial.Instance.ArmoryCameraPathEnd.position) > 0.1f)
		{
			yield return new WaitForEndOfFrame();
			time += Time.deltaTime;
			if (time > 1f && !played)
			{
				played = true;
				LevelTutorial.Instance.NPC.animation.Blend(AnimationIndex.TutorialGuideIdle.ToString(), 0f);
				LevelTutorial.Instance.NPC.animation.Blend(AnimationIndex.TutorialGuideArmory.ToString(), 1f);
			}
		}
		ArmoryCameraPath.Stop();
		Vector3 pos = LevelTutorial.Instance.ArmoryCameraPathEnd.position;
		pos.y = -1.355259f;
		Vector3 rotAngles = new Vector3(0f, 230.1176f, 0f);
		GameState.LocalPlayer.SpawnPlayerAt(pos, Quaternion.Euler(rotAngles));
		GameState.LocalCharacter.Keys = KeyState.Still;
		LevelCamera.Instance.CanDip = false;
		if (mode != null)
		{
			mode.OnArmoryEnterSubtitle();
		}
		else
		{
			Debug.LogError("No TutorialGameMode!");
		}
		yield return new WaitForSeconds(2f);
		LevelTutorial.Instance.NPC.animation.Blend(AnimationIndex.TutorialGuideArmory.ToString(), 0f);
		LevelTutorial.Instance.NPC.animation.Blend(AnimationIndex.TutorialGuideTalk.ToString(), 1f);
		yield return new WaitForSeconds(LevelTutorial.Instance.VoicePickupWeapon.length);
		LevelTutorial.Instance.NPC.animation.Blend(AnimationIndex.TutorialGuideTalk.ToString(), 0f);
		LevelTutorial.Instance.NPC.animation.Blend(AnimationIndex.TutorialGuideIdle.ToString(), 1f);
		AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled = true;
		GameState.LocalPlayer.IsWalkingEnabled = true;
		GameState.LocalPlayer.SetPlayerControlState(LocalPlayer.PlayerState.FirstPerson);
		yield return new WaitForSeconds(0.5f);
		LevelCamera.Instance.CanDip = true;
	}

	public void Reset()
	{
		_entered = false;
	}
}
