using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialShootingTargetController
{
	private TutorialGameMode _game;

	private List<TutorialShootingTarget> _targets = new List<TutorialShootingTarget>(6);

	public TutorialShootingTargetController(TutorialGameMode mode)
	{
		_game = mode;
		List<Transform> list = new List<Transform>();
		list.AddRange(LevelTutorial.Instance.NearRangeTargetPos);
		list.AddRange(LevelTutorial.Instance.FarRangeTargetPos);
		foreach (Transform item in list)
		{
			GameObject gameObject = Object.Instantiate(LevelTutorial.Instance.ShootingTargetPrefab, item.position, item.rotation) as GameObject;
			if ((bool)gameObject)
			{
				TutorialShootingTarget component = gameObject.GetComponent<TutorialShootingTarget>();
				if ((bool)component)
				{
					_targets.Add(component);
					component.OnHitCallback = OnTargetHit;
				}
			}
		}
	}

	public IEnumerator StartShootingRange()
	{
		SfxManager.Play2dAudioClip(LevelTutorial.Instance.VoiceShootingRange);
		yield return new WaitForSeconds(3f);
		_game.ShowShoot3();
		LevelTutorial.Instance.ShowObjShoot3 = true;
		SfxManager.Play2dAudioClip(GameAudio.SubObjective);
		for (int i = 0; i < LevelTutorial.Instance.NearRangeTargetPos.Length; i++)
		{
			_targets[i].Reset();
		}
		SfxManager.Play2dAudioClip(GameAudio.TargetPopup);
		float timeBeforeShoot = Time.time;
		bool allHit;
		do
		{
			allHit = true;
			for (int j = 0; j < LevelTutorial.Instance.NearRangeTargetPos.Length; j++)
			{
				allHit &= _targets[j].IsHit;
			}
			if (!AmmoDepot.HasAmmoOfType(AmmoType.Machinegun) && !LevelTutorial.Instance.AmmoWaypoint.CanShow)
			{
				LevelTutorial.Instance.AmmoWaypoint.CanShow = true;
			}
			else if (AmmoDepot.HasAmmoOfType(AmmoType.Machinegun) && LevelTutorial.Instance.AmmoWaypoint.CanShow)
			{
				LevelTutorial.Instance.AmmoWaypoint.CanShow = false;
			}
			yield return new WaitForSeconds(0.3f);
		}
		while (!allHit);
		float timeAfterShoot = Time.time - timeBeforeShoot;
		if (timeAfterShoot < LevelTutorial.Instance.VoiceShootingRange.length - 3f)
		{
			yield return new WaitForSeconds(LevelTutorial.Instance.VoiceShootingRange.length - 3f);
		}
		HudController.Instance.XpPtsHud.GainXp(5);
		_game.ObjShootTarget3.Complete();
		SfxManager.Play2dAudioClip(GameAudio.ObjectiveTick);
		_game.HideShoot3();
		yield return new WaitForSeconds(0.5f);
		LevelTutorial.Instance.ShowObjShoot3 = false;
		yield return new WaitForSeconds(4.5f);
		LevelTutorial.Instance.ShowObjShoot6 = true;
		SfxManager.Play2dAudioClip(GameAudio.SubObjective);
		_game.ShowShoot6();
		foreach (TutorialShootingTarget t in _targets)
		{
			t.Reset();
		}
		SfxManager.Play2dAudioClip(GameAudio.TargetPopup);
		do
		{
			allHit = true;
			foreach (TutorialShootingTarget t2 in _targets)
			{
				allHit &= t2.IsHit;
			}
			if (!AmmoDepot.HasAmmoOfType(AmmoType.Machinegun) && !LevelTutorial.Instance.AmmoWaypoint.CanShow)
			{
				LevelTutorial.Instance.AmmoWaypoint.CanShow = true;
			}
			else if (AmmoDepot.HasAmmoOfType(AmmoType.Machinegun) && LevelTutorial.Instance.AmmoWaypoint.CanShow)
			{
				LevelTutorial.Instance.AmmoWaypoint.CanShow = false;
			}
			yield return new WaitForSeconds(0.5f);
		}
		while (!allHit);
		Singleton<AmmoHud>.Instance.Enabled = false;
		Singleton<WeaponController>.Instance.ResetPickupSlot();
		GameState.LocalPlayer.SetPlayerControlState(LocalPlayer.PlayerState.None);
		AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled = false;
		GameState.LocalAvatar.Decorator.SetPosition(LevelTutorial.Instance.FinalPlayerPos.position, LevelTutorial.Instance.FinalPlayerPos.rotation);
		AutoMonoBehaviour<AvatarAnimationManager>.Instance.ResetAnimationState(PageType.Shop);
		HudController.Instance.XpPtsHud.GainXp(5);
		yield return new WaitForSeconds(0.5f);
		_game.ObjShootTarget6.Complete();
		SfxManager.Play2dAudioClip(GameAudio.ObjectiveTick);
		yield return new WaitForSeconds(0.5f);
		_game.HideShoot6();
		_game.DestroyObjectives();
		SfxManager.Play2dAudioClip(LevelTutorial.Instance.VoiceArena);
		yield return new WaitForSeconds(0.5f);
		LevelTutorial.Instance.ShowObjShoot6 = false;
		LevelTutorial.Instance.ShowObjectives = false;
		yield return new WaitForSeconds(0.5f);
		HudController.Instance.XpPtsHud.GainXp(30);
		yield return new WaitForSeconds(1f);
		LevelTutorial.Instance.ShowObjComplete = true;
		yield return new WaitForSeconds(2f);
		LevelTutorial.Instance.ShowObjComplete = false;
		yield return new WaitForSeconds(3f);
		_game.HideObjComplete();
		LevelTutorial.Instance.BackgroundMusic.Stop();
		Screen.lockCursor = false;
		Singleton<HpApHud>.Instance.Enabled = false;
		PopupSystem.Show(new LevelUpPopup(2, EndLevelup));
		HudController.Instance.enabled = false;
		foreach (TutorialShootingTarget i2 in _targets)
		{
			Object.Destroy(i2.gameObject);
		}
	}

	private void OnTargetHit()
	{
		HudController.Instance.XpPtsHud.GainXp(1);
	}

	private void EndLevelup()
	{
		_game.OnTutorialEnd();
	}
}
