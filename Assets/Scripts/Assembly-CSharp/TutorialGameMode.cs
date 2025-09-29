using System;
using System.Collections;
using System.Collections.Generic;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UberStrike.WebService.Unity;
using UnityEngine;

[NetworkClass(-1)]
public class TutorialGameMode : FpsGameMode, ITutorialCinematicSequenceListener
{
	private TutorialCinematicSequence _sequence;

	private TutorialShootingTargetController _shootingRangeController;

	private float _fadeInAlpha = 1f;

	private string _subtitle = string.Empty;

	private Vector2 _scale = Vector2.one;

	private MeshGUIText _txtObjectives;

	private MeshGUIText _txtObjUnderscore;

	private MeshGUIText _txtMouseLook;

	private MeshGUIText _txtWasdWalk;

	private MeshGUIText _txtToArmory;

	private MeshGUIText _txtPickupMG;

	private MeshGUIText _txtShoot3;

	private MeshGUIText _txtShoot6;

	private MeshGUIText _txtComplete;

	private ObjectiveTick _objMouseMove;

	private ObjectiveTick _objWasdWalk;

	private ObjectiveTick _objGotoArmory;

	private ObjectiveTick _objPickupWeapon;

	private ObjectiveTick _objShootTarget3;

	private ObjectiveTick _objShootTarget6;

	private bool _showObjective;

	private bool _showObjMouse;

	private bool _showObjWasdWalk;

	private bool _showGotoArmory;

	private float _blackBarHeight;

	private Rect _mousePos;

	private float _mouseXOffset;

	private KeyState[] _keys = new KeyState[4]
	{
		KeyState.Forward,
		KeyState.Left,
		KeyState.Backward,
		KeyState.Right
	};

	public TutorialCinematicSequence Sequence
	{
		get
		{
			return _sequence;
		}
	}

	public ObjectiveTick ObjShootTarget3
	{
		get
		{
			return _objShootTarget3;
		}
	}

	public ObjectiveTick ObjShootTarget6
	{
		get
		{
			return _objShootTarget6;
		}
	}

	public TutorialGameMode(RemoteMethodInterface rmi)
		: base(rmi, new GameMetaData(0, string.Empty, 120, 0, 108))
	{
		_sequence = new TutorialCinematicSequence(this);
		Singleton<LoadoutManager>.Instance.ResetSlot(LoadoutSlotType.WeaponMelee);
		Singleton<LoadoutManager>.Instance.ResetSlot(LoadoutSlotType.WeaponPickup);
		Singleton<LoadoutManager>.Instance.ResetSlot(LoadoutSlotType.WeaponPrimary);
		Singleton<LoadoutManager>.Instance.ResetSlot(LoadoutSlotType.WeaponSecondary);
		Singleton<LoadoutManager>.Instance.ResetSlot(LoadoutSlotType.WeaponTertiary);
		MonoRoutine.Start(StartTutorialMode());
	}

	public void DrawGui()
	{
		GUI.depth = 100;
		DrawFadeInRect();
		DrawSubtitle();
		if (LevelTutorial.Instance.ShowObjectives)
		{
			DrawObjectives();
		}
		if (LevelTutorial.Instance.ShowObjComplete)
		{
			_txtComplete.Draw(((float)Screen.width - _txtComplete.Size.x) / 2f, ((float)Screen.height - _txtComplete.Size.y) / 2f);
		}
		PlayAvatarAnimation();
		if (LevelTutorial.Instance.IsCinematic)
		{
			DrawBlackBars();
		}
		if (GUI.Button(new Rect(Screen.width - 100, 60f, 90f, 40f), "SKIP", BlueStonez.button_white))
		{
			ApplicationDataManager.EventsSystem.SendSkipTutorial(PlayerDataManager.CmidSecure);
			if (LevelTutorial.Exists)
			{
				LevelTutorial.Instance.StartCoroutine(StartTerminateTutorial());
			}
		}
	}

	private void DrawFadeInRect()
	{
		if (_fadeInAlpha > 0f)
		{
			_fadeInAlpha = Mathf.Lerp(_fadeInAlpha, 0f, Time.deltaTime);
			if (Mathf.Approximately(0f, _fadeInAlpha))
			{
				_fadeInAlpha = 0f;
			}
			GUI.color = new Color(1f, 1f, 1f, _fadeInAlpha);
			GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none, BlueStonez.box_black);
			GUI.color = Color.white;
		}
	}

	private void DrawSubtitle()
	{
		GUI.color = Color.black;
		GUI.Label(new Rect(1f, Screen.height - 149, Screen.width, 80f), _subtitle, BlueStonez.label_interparkbold_18pt);
		GUI.color = Color.white;
		GUI.Label(new Rect(0f, Screen.height - 150, Screen.width, 80f), _subtitle, BlueStonez.label_interparkbold_18pt);
	}

	private void PlayAvatarAnimation()
	{
		switch (_sequence.State)
		{
		case TutorialCinematicSequence.TutorialState.AirlockCameraZoomIn:
		case TutorialCinematicSequence.TutorialState.AirlockCameraReady:
		case TutorialCinematicSequence.TutorialState.AirlockWelcome:
		case TutorialCinematicSequence.TutorialState.AirlockMouseLookSubtitle:
		case TutorialCinematicSequence.TutorialState.AirlockMouseLook:
		case TutorialCinematicSequence.TutorialState.AirlockWasdSubtitle:
		case TutorialCinematicSequence.TutorialState.AirlockWasdWalk:
			GameState.LocalAvatar.Decorator.AnimationController.PlayAnimation(AnimationIndex.HomeNoWeaponIdle);
			break;
		case TutorialCinematicSequence.TutorialState.ArmoryPickupMG:
		case TutorialCinematicSequence.TutorialState.TutorialEnd:
			GameState.LocalAvatar.Decorator.AnimationController.PlayAnimation(AnimationIndex.HomeNoWeaponnLookAround);
			break;
		case TutorialCinematicSequence.TutorialState.AirlockDoorOpen:
		case TutorialCinematicSequence.TutorialState.ArmoryEnter:
		case TutorialCinematicSequence.TutorialState.ShootingRangeGameOver:
			break;
		}
	}

	private void DrawObjectives()
	{
		float num = Mathf.Clamp((float)Screen.height * 0.5f / 1000f, 0.35f, 0.4f);
		float scale = (float)Screen.height / 1000f;
		float a = (float)Screen.width / 4f;
		float num2 = (float)Screen.height / 4f;
		float num3 = Mathf.Clamp(Mathf.Min(a, num2) / 2f, 20f, 68f);
		float num4 = num3 / 5f;
		Rect rect = new Rect(Mathf.Clamp(Screen.width / 6 - 70, 0f, float.PositiveInfinity), ((float)Screen.height - num2) / 2f - 40f, num3 * 3f + num4 * 2f, num3 * 2f + num4);
		Rect rect2 = rect;
		rect2.x = (float)Screen.width - rect2.x - rect2.width - 140f;
		rect2.height = rect2.width * 0.8f;
		_scale.x = num;
		_scale.y = num;
		GUI.BeginGroup(new Rect(70f, 40f, Screen.width - 70, Screen.height - 40));
		if (_showObjective)
		{
			_txtObjectives.Scale = _scale * 1.5f;
			_txtObjectives.Position = new Vector2(70f, 36f);
			_txtObjUnderscore.Scale = _scale * 1.5f;
			_txtObjUnderscore.Position = new Vector2(73f + _txtObjectives.Size.x, 36f);
			_txtObjUnderscore.Alpha = ((Mathf.Sin(Time.time * 4f) > 0f) ? 1 : 0);
			_txtObjectives.Draw();
		}
		if (_showObjMouse)
		{
			_txtMouseLook.Scale = _scale;
			_txtMouseLook.Position = new Vector2(70f, 40f);
			_txtMouseLook.Draw(0f, 58f);
			_objMouseMove.Draw(new Vector2(_txtMouseLook.Size.x + 5f, 38f), scale);
		}
		if (_showObjWasdWalk && GameState.HasCurrentPlayer)
		{
			_txtWasdWalk.Scale = _scale;
			_txtWasdWalk.Position = new Vector2(70f, 40f);
			_txtWasdWalk.Draw(0f, 64f);
			_objWasdWalk.Draw(new Vector2(_txtWasdWalk.Size.x + 5f, 38f), scale);
		}
		if (_showGotoArmory)
		{
			_txtToArmory.Scale = _scale;
			_txtToArmory.Position = new Vector2(70f, 40f);
			_txtToArmory.Draw(0f, 58f);
			_objGotoArmory.Draw(new Vector2(_txtToArmory.Size.x + 5f, 38f), scale);
		}
		if (LevelTutorial.Instance.ShowObjPickupMG)
		{
			_txtPickupMG.Scale = _scale;
			_txtPickupMG.Position = new Vector2(70f, 40f);
			_txtPickupMG.Draw(0f, 57f);
			_objPickupWeapon.Draw(new Vector2(_txtPickupMG.Size.x + 5f, 38f), scale);
		}
		if (LevelTutorial.Instance.ShowObjShoot3)
		{
			_txtShoot3.Scale = _scale;
			_txtShoot3.Position = new Vector2(70f, 40f);
			_txtShoot3.Draw(0f, 57f);
			_objShootTarget3.Draw(new Vector2(_txtShoot3.Size.x + 5f, 38f), scale);
		}
		if (LevelTutorial.Instance.ShowObjShoot6)
		{
			_txtShoot6.Scale = _scale;
			_txtShoot6.Position = new Vector2(70f, 40f);
			_txtShoot6.Draw(0f, 57f);
			_objShootTarget6.Draw(new Vector2(_txtShoot6.Size.x + 5f, 38f), scale);
		}
		GUI.EndGroup();
	}

	private void DrawWaypoints()
	{
		if (_sequence.State <= TutorialCinematicSequence.TutorialState.AirlockWasdWalk)
		{
		}
	}

	private void DrawBlackBars()
	{
		if (Event.current.type == UnityEngine.EventType.Repaint)
		{
			_blackBarHeight = Mathf.Lerp(_blackBarHeight, (float)(Screen.height * 1) / 8f, Time.deltaTime * 3f);
		}
		GUI.DrawTexture(new Rect(0f, 0f, Screen.width, _blackBarHeight), BlueStonez.box_black.normal.background);
		GUI.DrawTexture(new Rect(0f, (float)Screen.height - _blackBarHeight, Screen.width, _blackBarHeight), BlueStonez.box_black.normal.background);
	}

	protected override void OnInitialized()
	{
		base.OnInitialized();
		MonoRoutine.Run(StartDecreasingHealthAndArmor);
		MonoRoutine.Run(SimulateGameFrameUpdate);
	}

	protected override void OnCharacterLoaded()
	{
		CreateAirlockNPC();
		PlaceAvatarInAirlock();
		GameState.LocalAvatar.Decorator.HideWeapons();
		MonoRoutine.Start(_sequence.StartSequences());
	}

	protected override void OnModeInitialized()
	{
		if (Application.isEditor)
		{
			GlobalUIRibbon.Instance.Hide();
		}
		GameState.LocalPlayer.SetWeaponControlState(PlayerHudState.None);
		GameState.LocalPlayer.SetEnabled(true);
		OnPlayerJoined(SyncObjectBuilder.GetSyncData(GameState.LocalCharacter, true), Vector3.zero);
		TutorialCinematicSequence.TutorialState state = _sequence.State;
		if (state == TutorialCinematicSequence.TutorialState.None || state == TutorialCinematicSequence.TutorialState.AirlockCameraZoomIn)
		{
			AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled = false;
		}
		base.IsMatchRunning = true;
		Screen.lockCursor = true;
		Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag = HudDrawFlags.XpPoints;
	}

	public override void PlayerHit(int targetPlayer, short damage, BodyPart part, Vector3 force, int shotCount, int weaponID, UberstrikeItemClass weaponClass, DamageEffectType damageEffectFlag, float damageEffectValue)
	{
		if (base.MyCharacterState.Info.IsAlive)
		{
			byte angle = Conversion.Angle2Byte(Vector3.Angle(Vector3.forward, force));
			base.MyCharacterState.Info.Health -= base.MyCharacterState.Info.Armor.AbsorbDamage(damage, part);
			Singleton<DamageFeedbackHud>.Instance.AddDamageMark(Mathf.Clamp01((float)damage / 50f), Conversion.Byte2Angle(angle));
			Singleton<HpApHud>.Instance.HP = GameState.LocalCharacter.Health;
			Singleton<HpApHud>.Instance.AP = GameState.LocalCharacter.Armor.ArmorPoints;
			GameState.LocalPlayer.MoveController.ApplyForce(force, CharacterMoveController.ForceType.Additive);
		}
	}

	protected override void ApplyCurrentGameFrameUpdates(SyncObject delta)
	{
		base.ApplyCurrentGameFrameUpdates(delta);
		if (delta.Contains(2097152) && !GameState.LocalCharacter.IsAlive)
		{
			OnSetNextSpawnPoint(UnityEngine.Random.Range(0, Singleton<SpawnPointManager>.Instance.GetSpawnPointCount(GameMode.Tutorial, TeamID.NONE)), 3);
		}
	}

	public override void RequestRespawn()
	{
		OnSetNextSpawnPoint(UnityEngine.Random.Range(0, Singleton<SpawnPointManager>.Instance.GetSpawnPointCount(GameMode.Tutorial, TeamID.NONE)), 3);
	}

	public override void IncreaseHealthAndArmor(int health, int armor)
	{
		UberStrike.Realtime.UnitySdk.CharacterInfo localCharacter = GameState.LocalCharacter;
		if (health > 0 && localCharacter.Health < 200)
		{
			localCharacter.Health = (short)Mathf.Clamp(localCharacter.Health + health, 0, 200);
		}
		if (armor > 0 && localCharacter.Armor.ArmorPoints < 200)
		{
			localCharacter.Armor.ArmorPoints = Mathf.Clamp(localCharacter.Armor.ArmorPoints + armor, 0, 200);
		}
	}

	public override void PickupPowerup(int pickupID, PickupItemType type, int value)
	{
		switch (type)
		{
		case PickupItemType.Armor:
			GameState.LocalCharacter.Armor.ArmorPoints += value;
			break;
		case PickupItemType.Health:
			switch (value)
			{
			case 5:
			case 100:
				if (GameState.LocalCharacter.Health < 200)
				{
					GameState.LocalCharacter.Health = (short)Mathf.Clamp(GameState.LocalCharacter.Health + value, 0, 200);
				}
				break;
			case 25:
			case 50:
				if (GameState.LocalCharacter.Health < 100)
				{
					GameState.LocalCharacter.Health = (short)Mathf.Clamp(GameState.LocalCharacter.Health + value, 0, 100);
				}
				break;
			}
			break;
		}
	}

	public void ResetBlackBar()
	{
		_blackBarHeight = 0f;
	}

	private void PlaceAvatarInAirlock()
	{
		Vector3 position = new Vector3(58f, -6.437f, 64.4f);
		Quaternion horizontalRotation = Quaternion.Euler(0f, 224f, 0f);
		GameState.LocalCharacter.Position = position;
		GameState.LocalCharacter.HorizontalRotation = horizontalRotation;
		GameState.LocalAvatar.Decorator.HideWeapons();
		GameState.LocalPlayer.transform.position = position;
		if ((bool)GameState.LocalPlayer.Character && (bool)GameState.LocalPlayer.Decorator && GameState.LocalPlayer.Decorator.AnimationController != null)
		{
			GameState.LocalPlayer.Character.StateController.IsCinematic = true;
			GameState.LocalPlayer.Decorator.AnimationController.ResetAllAnimations();
			GameState.LocalPlayer.Decorator.AnimationController.PlayAnimation(AnimationIndex.HomeNoWeaponIdle);
		}
	}

	private void CreateAirlockNPC()
	{
		Vector3 finalPosition = new Vector3(56.97f, -7.4f, 63.18f);
		if (GameState.HasCurrentSpace)
		{
			if (LevelTutorial.Exists)
			{
				LevelTutorial instance = LevelTutorial.Instance;
				List<int> list = new List<int>();
				list.Add(0);
				list.Add(instance.GearHead);
				list.Add(0);
				list.Add(instance.GearGloves);
				list.Add(instance.GearUB);
				list.Add(instance.GearLB);
				list.Add(instance.GearBoots);
				List<int> gearItemIds = list;
				list = new List<int>();
				list.Add(0);
				list.Add(0);
				list.Add(0);
				list.Add(0);
				list.Add(0);
				List<int> weaponItemIds = list;
				Loadout loadout = Loadout.Create(gearItemIds, weaponItemIds);
				LevelTutorial.Instance.NPC = Singleton<AvatarBuilder>.Instance.CreateRemoteAvatar(loadout.GetAvatarGear(), Color.white);
				if ((bool)LevelTutorial.Instance.NPC)
				{
					LevelTutorial.Instance.NPC.gameObject.layer = 21;
					LevelTutorial.Instance.NPC.transform.position = LevelTutorial.Instance.NpcStartPos.position;
					LevelTutorial.Instance.NPC.transform.rotation = LevelTutorial.Instance.NpcStartPos.rotation;
					BaseWeaponDecorator baseWeaponDecorator = LevelTutorial.Instance.Weapon.Clone();
					if ((bool)baseWeaponDecorator)
					{
						baseWeaponDecorator.transform.parent = LevelTutorial.Instance.NPC.WeaponAttachPoint;
						baseWeaponDecorator.transform.localPosition = Vector3.zero;
						baseWeaponDecorator.transform.localRotation = Quaternion.identity;
						LayerUtil.SetLayerRecursively(baseWeaponDecorator.transform, UberstrikeLayer.Props);
					}
					LevelTutorial.Instance.NPC.GetComponent<Animation>().enabled = true;
					LevelTutorial.Instance.NPC.GetComponent<Animation>().Play(AnimationIndex.HomeSmallGunIdle.ToString());
					LevelTutorial.Instance.NPC.GetComponent<Animation>().Stop();
					CapsuleCollider capsuleCollider = LevelTutorial.Instance.NPC.gameObject.AddComponent<CapsuleCollider>();
					if ((bool)capsuleCollider)
					{
						capsuleCollider.radius = 0.4f;
					}
					TutorialAirlockNPC tutorialAirlockNPC = LevelTutorial.Instance.NPC.gameObject.AddComponent<TutorialAirlockNPC>();
					if ((bool)tutorialAirlockNPC)
					{
						MonoRoutine.Start(StartNPCAirlockVoiceOver());
						tutorialAirlockNPC.SetFinalPosition(finalPosition);
					}
				}
				return;
			}
			throw new Exception("LevelTutorial is not initialized!");
		}
		throw new Exception("GameState doesn't have current space!");
	}

	private IEnumerator StartNPCAirlockVoiceOver()
	{
		yield return new WaitForSeconds(3f);
		_subtitle = "Come on private, let's have a look at you.";
		SfxManager.Play2dAudioClip(LevelTutorial.Instance.VoiceWelcome);
	}

	private IEnumerator StartTutorialMode()
	{
		GlobalUIRibbon.Instance.Hide();
		while (!LevelTutorial.Exists)
		{
			yield return new WaitForEndOfFrame();
		}
		LevelTutorial.Instance.gameObject.SetActive(true);
		_objWasdWalk = new ObjectiveTick(LevelTutorial.Instance.ImgObjBk, LevelTutorial.Instance.ImgObjTick);
		_objMouseMove = new ObjectiveTick(LevelTutorial.Instance.ImgObjBk, LevelTutorial.Instance.ImgObjTick);
		_objGotoArmory = new ObjectiveTick(LevelTutorial.Instance.ImgObjBk, LevelTutorial.Instance.ImgObjTick);
		_objPickupWeapon = new ObjectiveTick(LevelTutorial.Instance.ImgObjBk, LevelTutorial.Instance.ImgObjTick);
		_objShootTarget3 = new ObjectiveTick(LevelTutorial.Instance.ImgObjBk, LevelTutorial.Instance.ImgObjTick);
		_objShootTarget6 = new ObjectiveTick(LevelTutorial.Instance.ImgObjBk, LevelTutorial.Instance.ImgObjTick);
		_txtObjectives = new MeshGUIText("OBJECTIVES", LevelTutorial.Instance.Font);
		_txtObjUnderscore = new MeshGUIText("_", LevelTutorial.Instance.Font);
		_txtMouseLook = new MeshGUIText("> Use your right thumb to look around", LevelTutorial.Instance.Font);
		_txtWasdWalk = new MeshGUIText("> Use your left thumb to walk", LevelTutorial.Instance.Font);
		_txtToArmory = new MeshGUIText("> Navigate to the Armory", LevelTutorial.Instance.Font);
		_txtPickupMG = new MeshGUIText("> Pick up the Machine Gun", LevelTutorial.Instance.Font);
		_txtShoot3 = new MeshGUIText("> Tap the Fire button and Shoot the 3 Targets", LevelTutorial.Instance.Font);
		_txtShoot6 = new MeshGUIText("> Shoot the 6 targets", LevelTutorial.Instance.Font);
		_txtComplete = new MeshGUIText("Objectives Complete", LevelTutorial.Instance.Font);
		_txtObjectives.Hide();
		_txtObjUnderscore.Hide();
		_txtMouseLook.Hide();
		_txtWasdWalk.Hide();
		_txtToArmory.Hide();
		_txtPickupMG.Hide();
		_txtShoot3.Hide();
		_txtShoot6.Hide();
		_txtComplete.Hide();
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_txtObjectives);
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_txtObjUnderscore);
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_txtMouseLook);
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_txtWasdWalk);
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_txtToArmory);
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_txtPickupMG);
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_txtShoot3);
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_txtShoot6);
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_txtComplete);
		LevelTutorial.Instance.ShowObjectives = true;
		LevelTutorial.Instance.BackgroundMusic.Play();
		LevelTutorial.Instance.BridgeAudio.Play();
		LevelTutorial.Instance.AirlockBridgeAnim.Play();
		LevelTutorial.Instance.AirlockSplineController.FollowSpline();
		LevelTutorial.Instance.AirlockBackDoor.Reset();
		LevelTutorial.Instance.ArmoryTrigger.Reset();
		InitializeMode();
	}

	private IEnumerator StartDecreasingHealthAndArmor()
	{
		while (IsInitialized)
		{
			yield return new WaitForSeconds(1f);
			if (GameState.LocalCharacter.Health > 100)
			{
				GameState.LocalCharacter.Health--;
			}
			if (GameState.LocalCharacter.Armor.ArmorPoints > GameState.LocalCharacter.Armor.ArmorPointCapacity)
			{
				GameState.LocalCharacter.Armor.ArmorPoints--;
			}
		}
	}

	private IEnumerator SimulateGameFrameUpdate()
	{
		while (IsInitialized)
		{
			yield return new WaitForEndOfFrame();
			if (_sequence.State < TutorialCinematicSequence.TutorialState.AirlockMouseLookSubtitle && AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled)
			{
				if (GameState.HasCurrentPlayer)
				{
					GameState.LocalCharacter.Position = new Vector3(58f, -6.437f, 64.4f);
					GameState.LocalCharacter.HorizontalRotation = Quaternion.Euler(0f, 224f, 0f);
				}
				AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled = false;
			}
			if (GameState.LocalPlayer.Character != null)
			{
				SyncObject delta = SyncObjectBuilder.GetSyncData(GameState.LocalCharacter, false);
				if (!delta.IsEmpty)
				{
					ApplyCurrentGameFrameUpdates(delta);
					GameState.LocalPlayer.Character.OnCharacterStateUpdated(delta);
				}
			}
		}
	}

	private IEnumerator StartAirlockCameraSmoothIn()
	{
		Vector3 splineEnd = new Vector3(57.659f, -5.366f, 68.957f);
		Vector3 camVelocity = Vector3.zero;
		while (Vector3.SqrMagnitude(LevelCamera.Instance.TransformCache.position - splineEnd) > 0.2f)
		{
			yield return new WaitForEndOfFrame();
		}
		LevelTutorial.Instance.AirlockSplineController.Stop();
		camVelocity = LevelCamera.Instance.MainCamera.velocity;
		while (camVelocity.magnitude > 0.05f)
		{
			camVelocity = Vector3.Lerp(camVelocity, Vector3.zero, Time.deltaTime * 3.5f);
			LevelCamera.Instance.TransformCache.position += camVelocity * Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		_sequence.OnAirlockCameraReady();
	}

	private IEnumerator StartShowObjective()
	{
		_subtitle = string.Empty;
		yield return new WaitForSeconds(1f);
		_showObjective = true;
		ShowDirective(_txtObjectives);
		ShowDirective(_txtObjUnderscore);
		SfxManager.Play2dAudioClip(GameAudio.Objective);
	}

	private IEnumerator StartAirlockMouseLook()
	{
		AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled = false;
		AutoMonoBehaviour<InputManager>.Instance.Reset();
		UserInput.SetRotation(180f, 0f);
		GameState.LocalPlayer.SetWeaponControlState(PlayerHudState.Playing);
		base.IsWaitingForSpawn = false;
		Vector3 pos = new Vector3(58f, -6.437f, 64.4f);
		Quaternion rot = Quaternion.Euler(0f, 224f, 0f);
		SpawnPlayerAt(pos, rot);
		LevelCamera.Instance.CanDip = false;
		SfxManager.Play2dAudioClip(GameAudio.SubObjective);
		_showObjMouse = true;
		_mousePos.width = LevelTutorial.Instance.ImgMouse.width;
		_mousePos.height = LevelTutorial.Instance.ImgMouse.height;
		ShowDirective(_txtMouseLook);
		AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled = true;
		float mouseValue = 0f;
		Vector2 prevMouse = UserInput.Mouse;
		bool mouseIconMoving = false;
		float time = 0f;
		float frequency = 10f;
		float magnitude = -20f;
		while (mouseValue < 90f)
		{
			if (mouseIconMoving)
			{
				if (time < (float)Math.PI * 2f / frequency)
				{
					_mouseXOffset = Mathf.Sin(frequency * time) * Mathf.Cos(frequency * time / 2.5f) * magnitude;
					time += Time.deltaTime;
				}
				else
				{
					time = 0f;
					mouseIconMoving = false;
				}
			}
			else if (time < 1f)
			{
				time += Time.deltaTime;
			}
			else
			{
				time = 0f;
				mouseIconMoving = true;
			}
			mouseValue += Mathf.Abs(prevMouse.x - UserInput.Mouse.x) + Mathf.Abs(prevMouse.y - UserInput.Mouse.y);
			prevMouse = UserInput.Mouse;
			yield return new WaitForEndOfFrame();
		}
		LevelCamera.Instance.CanDip = true;
		_objMouseMove.Complete();
		_sequence.OnAirlockMouseLook();
		SfxManager.Play2dAudioClip(GameAudio.ObjectiveTick);
		yield return new WaitForSeconds(0.5f);
		HudController.Instance.XpPtsHud.GainXp(5);
		MonoRoutine.Start(StartHideDirective(_txtMouseLook));
		yield return new WaitForSeconds(0.5f);
		_showObjMouse = false;
		_txtMouseLook.Hide();
		_txtMouseLook.FreeObject();
		EnableObjectives(false);
		yield return new WaitForSeconds(0.5f);
		if ((bool)LevelTutorial.Instance.AirlockDoorAnim)
		{
			LevelTutorial.Instance.AirlockDoorAnim.Play();
		}
	}

	private IEnumerator StartAirlockWasdWalk()
	{
		_showObjWasdWalk = true;
		ShowDirective(_txtWasdWalk);
		LevelTutorial.Instance.AirlockFrontDoor.Waypoint.CanShow = true;
		GameState.LocalPlayer.IsWalkingEnabled = true;
		SfxManager.Play2dAudioClip(GameAudio.SubObjective);
		yield return new WaitForSeconds(0.5f);
		SfxManager.Play2dAudioClip(LevelTutorial.Instance.VoiceToArmory);
		_subtitle = "Through the door, up on the right and\nwe'll get you started.";
		while (!LevelTutorial.Instance.AirlockFrontDoor.PlayerEntered)
		{
			yield return new WaitForEndOfFrame();
		}
		_subtitle = string.Empty;
		_objWasdWalk.Complete();
		SfxManager.Play2dAudioClip(GameAudio.ObjectiveTick);
		yield return new WaitForSeconds(0.5f);
		HudController.Instance.XpPtsHud.GainXp(5);
		MonoRoutine.Start(StartHideDirective(_txtWasdWalk));
		yield return new WaitForSeconds(0.5f);
		_showObjWasdWalk = false;
		yield return new WaitForSeconds(1f);
		_sequence.OnAirlockWasdWalk();
		yield return new WaitForSeconds(1f);
		if ((bool)LevelTutorial.Instance.ArmoryWaypoint)
		{
			LevelTutorial.Instance.ArmoryWaypoint.CanShow = true;
		}
		SfxManager.Play2dAudioClip(LevelTutorial.Instance.WaypointAppear);
	}

	private IEnumerator StartNavigateToArmory()
	{
		yield return new WaitForSeconds(1f);
		SfxManager.Play2dAudioClip(GameAudio.SubObjective);
		_showGotoArmory = true;
		_txtToArmory.Alpha = 0f;
		_txtToArmory.Flicker(0.5f);
		_txtToArmory.FadeAlphaTo(1f, 0.5f);
		_txtToArmory.Show();
		EnableObjectives(true);
	}

	private IEnumerator StartHideObjectives()
	{
		_txtObjectives.Flicker(0.5f);
		_txtObjectives.FadeAlphaTo(0f, 0.5f);
		_txtToArmory.Flicker(0.5f);
		_txtToArmory.FadeAlphaTo(0f, 0.5f);
		yield return new WaitForSeconds(0.5f);
		_txtObjectives.Hide();
		_txtObjUnderscore.Hide();
		_txtToArmory.Hide();
		_txtToArmory.FreeObject();
		_showGotoArmory = false;
		EnableObjectives(false);
		LevelTutorial.Instance.ShowObjectives = false;
	}

	private IEnumerator StartArmoryObjective()
	{
		_txtObjectives.Flicker(0.5f);
		_txtObjectives.FadeAlphaTo(1f, 0.5f);
		_txtObjectives.Show();
		_txtObjUnderscore.Show();
		_txtPickupMG.Alpha = 0f;
		_txtPickupMG.Flicker(0.5f);
		_txtPickupMG.FadeAlphaTo(1f, 0.5f);
		_txtPickupMG.Show();
		EnableObjectives(true);
		yield return new WaitForSeconds(7f);
		_subtitle = string.Empty;
	}

	private IEnumerator StartPickupWeapon()
	{
		_txtPickupMG.Flicker(0.5f);
		_txtPickupMG.FadeAlphaTo(0f, 0.5f);
		yield return new WaitForSeconds(0.5f);
		_txtPickupMG.Hide();
		_txtPickupMG.FreeObject();
		EnableObjectives(false);
		LevelTutorial.Instance.ShowObjPickupMG = false;
		HudController.Instance.XpPtsHud.GainXp(5);
		yield return new WaitForSeconds(8f);
		_subtitle = string.Empty;
	}

	private void ShowDirective(MeshGUIText txt, bool showObjective = true)
	{
		EnableObjectives(showObjective);
		txt.Alpha = 0f;
		txt.Flicker(0.5f);
		txt.FadeAlphaTo(1f, 0.5f);
		txt.Show();
	}

	private IEnumerator StartHideDirective(MeshGUIText txt, bool delete = true)
	{
		txt.Flicker(0.5f);
		txt.FadeAlphaTo(0f, 0.5f);
		yield return new WaitForSeconds(0.5f);
		txt.Hide();
		if (delete)
		{
			txt.FreeObject();
		}
		EnableObjectives(false);
	}

	private IEnumerator StartTerminateTutorial()
	{
		yield return new WaitForEndOfFrame();
		Singleton<PlayerDataManager>.Instance.AttributeXp(80);
		UserWebServiceClient.FlagTutorialAsCompleted(PlayerDataManager.AuthToken, delegate
		{
		}, delegate(Exception ex)
		{
			DebugConsoleManager.SendExceptionReport(ex);
		});
		_txtComplete.IsEnabled = false;
		_txtMouseLook.IsEnabled = false;
		_txtObjectives.IsEnabled = false;
		_txtObjUnderscore.IsEnabled = false;
		_txtPickupMG.IsEnabled = false;
		_txtShoot3.IsEnabled = false;
		_txtShoot6.IsEnabled = false;
		_txtToArmory.IsEnabled = false;
		_txtWasdWalk.IsEnabled = false;
		LevelTutorial.Instance.AirlockFrontDoor.Waypoint.CanShow = false;
		LevelTutorial.Instance.AmmoWaypoint.CanShow = false;
		LevelTutorial.Instance.ArmoryWaypoint.CanShow = false;
		LevelTutorial.Instance.AmmoWaypoint.CanShow = false;
		GameState.LocalPlayer.SetWeaponControlState(PlayerHudState.None);
		GameState.LocalPlayer.IsShootingEnabled = false;
		ResetBlackBar();
		LevelTutorial.Instance.NPC.gameObject.SetActive(false);
		GlobalUIRibbon.Instance.Hide();
		PanelManager.Instance.OpenPanel(PanelType.CompleteAccount);
		LevelTutorial.Instance.gameObject.SetActive(false);
		LevelTutorial.Instance.StopAllCoroutines();
	}

	public void ShowShoot3()
	{
		ShowDirective(_txtShoot3);
	}

	public void ShowShoot6()
	{
		_subtitle = string.Empty;
		ShowDirective(_txtShoot6);
	}

	public void HideShoot3()
	{
		SfxManager.Play2dAudioClip(LevelTutorial.Instance.VoiceShootMore);
		_subtitle = "Hah, not bad at all! Try some more!";
		MonoRoutine.Start(StartHideDirective(_txtShoot3));
	}

	public void HideShoot6()
	{
		_subtitle = "Who would have thought you had it in you.\nNow let's see how you do in some real combat.";
		MonoRoutine.Start(StartHideDirective(_txtShoot6));
	}

	public void DestroyObjectives()
	{
		MonoRoutine.Start(StartHideDirective(_txtObjectives));
		MonoRoutine.Start(StartHideDirective(_txtObjUnderscore));
	}

	public void ShowObjComplete()
	{
		ShowDirective(_txtComplete, false);
	}

	public void HideObjComplete(bool destroyAfter = true)
	{
		_subtitle = string.Empty;
		MonoRoutine.Start(StartHideDirective(_txtComplete, destroyAfter));
	}

	public void ShowTutorialComplete()
	{
		_txtComplete.Text = "TUTORIAL COMPLETE";
		_txtComplete.BitmapMeshText.ShadowColor = new Color(64f / 85f, 28f / 51f, 0f);
		ShowDirective(_txtComplete, false);
	}

	public void OnAirlockCameraZoomIn()
	{
		if ((bool)GameState.LocalAvatar.Decorator)
		{
			GameState.LocalAvatar.Decorator.MeshRenderer.enabled = true;
			GameState.LocalAvatar.Decorator.HudInformation.enabled = true;
		}
		MonoRoutine.Start(StartAirlockCameraSmoothIn());
	}

	public void OnAirlockWelcome()
	{
		MonoRoutine.Start(StartShowObjective());
	}

	public void OnAirlockMouseLookSubtitle()
	{
		_subtitle = string.Empty;
		MonoRoutine.Start(StartAirlockMouseLook());
	}

	public void OnAirlockWasdSubtitle()
	{
		MonoRoutine.Start(StartAirlockWasdWalk());
	}

	public void OnAirlockDoorOpen()
	{
		_subtitle = string.Empty;
		MonoRoutine.Start(StartNavigateToArmory());
	}

	public void ReachArmoryWaypoint()
	{
		_objGotoArmory.Complete();
		SfxManager.Play2dAudioClip(GameAudio.ObjectiveTick);
		GameState.LocalCharacter.Keys = KeyState.Still;
		HudController.Instance.XpPtsHud.GainXp(5);
	}

	public void HideObjectives()
	{
		MonoRoutine.Start(StartHideObjectives());
	}

	public void EnterArmory(SplineController splineController)
	{
		_sequence.OnArmoryEnter();
		_shootingRangeController = new TutorialShootingTargetController(this);
	}

	public void OnArmoryEnterSubtitle()
	{
		_subtitle = "Right, go to the counter, pick up your weapon and lets see what you're made of.";
		SfxManager.Play2dAudioClip(LevelTutorial.Instance.VoicePickupWeapon);
		LevelTutorial.Instance.ArmoryDoor.Close();
		LevelTutorial.Instance.IsCinematic = false;
		LevelTutorial.Instance.ShowObjectives = true;
		SfxManager.Play2dAudioClip(GameAudio.SubObjective);
		MonoRoutine.Start(StartArmoryObjective());
	}

	public void OnArmoryPickupMG()
	{
		_subtitle = "Alright my soldier. Let's see if you can shoot straight.\nFeed these targets some lead.";
		_sequence.OnArmoryPickupMG();
		GameState.LocalPlayer.SetWeaponControlState(PlayerHudState.Playing);
		GameState.LocalPlayer.UnPausePlayer();
		Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag = HudDrawFlags.Ammo | HudDrawFlags.Reticle | HudDrawFlags.XpPoints;
		_objPickupWeapon.Complete();
		SfxManager.Play2dAudioClip(GameAudio.ObjectiveTick);
		MonoRoutine.Start(StartPickupWeapon());
		LevelTutorial.Instance.WeaponWaypoint.CanShow = false;
		MonoRoutine.Start(_shootingRangeController.StartShootingRange());
	}

	public void OnTutorialEnd()
	{
		ApplicationDataManager.EventsSystem.SendFinishTutorial(PlayerDataManager.CmidSecure);
		if (LevelTutorial.Exists)
		{
			LevelTutorial.Instance.StartCoroutine(StartTerminateTutorial());
		}
	}

	private void EnableObjectives(bool enabled)
	{
		if (enabled)
		{
			_txtObjectives.Show();
			_txtObjUnderscore.Show();
		}
		else
		{
			_txtObjectives.Hide();
			_txtObjUnderscore.Hide();
		}
	}
}
