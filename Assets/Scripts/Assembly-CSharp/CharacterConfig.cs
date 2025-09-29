using System;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class CharacterConfig : MonoBehaviour, IShootable
{
	private ICharacterState _state;

	private PlayerSound _sound;

	private DamageInfo _lastShotInfo;

	private float _graceTimeAfterSpawn;

	private bool _graceTimeOut = true;

	[SerializeField]
	private bool _isLocalPlayer;

	[SerializeField]
	private PlayerDamageEffect _damageFeedback;

	[SerializeField]
	private CharacterTrigger _aimTrigger;

	[SerializeField]
	private float _walkingSoundSpeed = 0.38f;

	public bool IsDead { get; private set; }

	public float WalkingSoundSpeed
	{
		get
		{
			return _walkingSoundSpeed;
		}
	}

	public bool IsLocal
	{
		get
		{
			return _isLocalPlayer;
		}
	}

	public bool IsVulnerable
	{
		get
		{
			return _graceTimeAfterSpawn <= 0f;
		}
	}

	public bool IsAnimationEnabled { get; set; }

	public float TimeLastGrounded { get; private set; }

	public Avatar Avatar { get; private set; }

	public CharacterMoveSimulator MoveSimulator { get; private set; }

	public WeaponSimulator WeaponSimulator { get; private set; }

	public CharacterStateAnimationController StateController { get; private set; }

	public int ActorID
	{
		get
		{
			return (State != null) ? State.ActorId : 0;
		}
	}

	public TeamID Team
	{
		get
		{
			return (State != null) ? State.TeamID : TeamID.NONE;
		}
	}

	public UberStrike.Realtime.UnitySdk.CharacterInfo State
	{
		get
		{
			return (_state == null) ? null : _state.Info;
		}
	}

	public CharacterTrigger AimTrigger
	{
		get
		{
			return _aimTrigger;
		}
	}

	private void Awake()
	{
		IsAnimationEnabled = true;
		MoveSimulator = new CharacterMoveSimulator(base.transform);
		WeaponSimulator = new WeaponSimulator(this);
		StateController = new CharacterStateAnimationController();
	}

	private void LateUpdate()
	{
		if (_state != null)
		{
			if ((bool)Avatar.Decorator && Avatar.Decorator.AnimationController != null)
			{
				StateController.Update(_state.Info, Avatar.Decorator.AnimationController);
			}
			WeaponSimulator.Update(_state.Info, IsLocal);
			MoveSimulator.Update(_state.Info);
		}
		if (_sound != null)
		{
			_sound.Update();
		}
		if (_graceTimeAfterSpawn > 0f)
		{
			_graceTimeAfterSpawn -= Time.deltaTime;
		}
		if (!_graceTimeOut && _graceTimeAfterSpawn <= 0f)
		{
			_graceTimeOut = true;
		}
	}

	public void Initialize(ICharacterState state, Avatar avatar)
	{
		_state = state;
		SetAvatarDecorator(avatar);
		_sound = new PlayerSound(state.Info);
		_sound.SetCharacter(this);
		_state.SubscribeToEvents(this);
		base.transform.position = _state.LastPosition;
		OnCharacterStateUpdated(SyncObjectBuilder.GetSyncData(state.Info, true));
	}

	private void SetAvatarDecorator(Avatar avatar)
	{
		Avatar = avatar;
		Avatar.Decorator.GetComponent<Renderer>().receiveShadows = false;
		Avatar.Decorator.GetComponent<Renderer>().castShadows = true;
		Avatar.Decorator.transform.parent = base.transform;
		Avatar.Decorator.SetPosition(new Vector3(0f, -0.98f, 0f), Quaternion.identity);
		Avatar.Decorator.HudInformation.SetCharacterInfo(State);
		Avatar.Decorator.HudInformation.SetHealthBarValue((float)State.Health / 100f);
		Avatar.Decorator.SetFootStep((!GameState.HasCurrentSpace) ? FootStepSoundType.Rock : GameState.CurrentSpace.DefaultFootStep);
		Avatar.Decorator.SetSkinColor(State.SkinColor);
		Avatar.Decorator.SetLayers((!IsLocal) ? UberstrikeLayer.RemotePlayer : UberstrikeLayer.LocalPlayer);
		WeaponSimulator.SetAvatarDecorator(avatar.Decorator);
		WeaponSimulator.UpdateWeapons(State.CurrentWeaponSlot, State.Weapons.ItemIDs, State.QuickItems);
		WeaponSimulator.UpdateWeaponSlot(State.CurrentWeaponSlot, _isLocalPlayer);
		base.gameObject.name = string.Format("Player{0}_{1}", State.Cmid, State.PlayerName);
		CharacterHitArea[] hitAreas = Avatar.Decorator.HitAreas;
		foreach (CharacterHitArea characterHitArea in hitAreas)
		{
			if ((bool)characterHitArea)
			{
				characterHitArea.Shootable = this;
			}
		}
	}

	public void OnCharacterStateUpdated(SyncObject delta)
	{
		try
		{
			if (delta.Contains(8388608))
			{
				WeaponSimulator.UpdateWeapons(_state.Info.CurrentWeaponSlot, _state.Info.Weapons.ItemIDs, _state.Info.QuickItems);
				WeaponSimulator.UpdateWeaponSlot(_state.Info.CurrentWeaponSlot, _isLocalPlayer);
			}
			else if (delta.Contains(16777216))
			{
				WeaponSimulator.UpdateWeaponSlot(_state.Info.CurrentWeaponSlot, _isLocalPlayer);
			}
			if (delta.Contains(134217728) && !IsLocal)
			{
				Singleton<AvatarBuilder>.Instance.UpdateRemoteAvatar(Avatar.Decorator, Loadout.Create(_state.Info.Gear, _state.Info.Weapons.ItemIDs).GetAvatarGear(), _state.Info.SkinColor);
			}
			if (delta.Contains(256) && _state != null && _state.Info.Is(PlayerStates.GROUNDED) && TimeLastGrounded + 0.5f < Time.time && !_state.Info.Is(PlayerStates.DIVING))
			{
				TimeLastGrounded = Time.time;
				if ((bool)Avatar.Decorator)
				{
					Avatar.Decorator.PlayFootSound(WalkingSoundSpeed);
				}
			}
			if (!delta.Contains(2097152))
			{
				return;
			}
			int num = (short)delta.Data[2097152];
			if (IsDead && num > 0)
			{
				Avatar.EnableDecorator();
				IsDead = false;
				_graceTimeOut = false;
				_graceTimeAfterSpawn = 2f;
			}
			else if (!IsDead && num <= 0)
			{
				IsDead = true;
				Singleton<QuickItemSfxController>.Instance.DestroytSfxFromPlayer(State.PlayerNumber);
				Avatar.Decorator.HudInformation.Hide();
				Avatar.SpawnRagdoll((_lastShotInfo == null) ? Vector3.zero : _lastShotInfo.Force);
				if (IsLocal)
				{
					GameState.LocalPlayer.SetPlayerControlState(LocalPlayer.PlayerState.Death, Avatar.Ragdoll);
				}
				if (GameState.CurrentGame.IsLocalAvatarLoaded)
				{
					Avatar.Decorator.PlayDieSound();
				}
				if (!_isLocalPlayer)
				{
					Avatar.Decorator.HideWeapons();
				}
			}
			Avatar.Decorator.HudInformation.SetHealthBarValue((float)_state.Info.Health / 100f);
		}
		catch (Exception ex)
		{
			ex.Data.Add("OnCharacterStateUpdated", delta);
			throw;
		}
	}

	public void ApplyDamage(DamageInfo d)
	{
		_lastShotInfo = d;
		if (_state == null || !GameState.HasCurrentGame)
		{
			return;
		}
		if (_state.Info.Health > 0)
		{
			GameState.CurrentGame.PlayerHit(_state.Info.ActorId, d.Damage, d.BodyPart, d.Force, d.ShotCount, d.WeaponID, d.WeaponClass, d.DamageEffectFlag, d.DamageEffectValue);
		}
		if (!IsLocal && GameState.LocalCharacter != null)
		{
			if (_state.Info.TeamID == TeamID.NONE || _state.Info.TeamID != GameState.LocalCharacter.TeamID)
			{
				Singleton<ReticleHud>.Instance.EnableEnemyReticle();
			}
			if (_state.Info.TeamID == TeamID.NONE || _state.Info.TeamID != GameState.LocalCharacter.TeamID)
			{
				ShowDamageFeedback(d);
			}
		}
		PlayDamageSound();
	}

	public virtual void ApplyForce(Vector3 position, Vector3 force)
	{
		if (IsLocal)
		{
			GameState.LocalPlayer.MoveController.ApplyForce(force, CharacterMoveController.ForceType.Additive);
		}
		else
		{
			GameState.CurrentGame.SendPlayerHitFeedback(ActorID, force);
		}
	}

	public void AddFollowCamera()
	{
		MoveSimulator.AddPositionObserver(LevelCamera.Instance);
	}

	public void RemoveFollowCamera()
	{
		MoveSimulator.RemovePositionObserver();
	}

	internal void Destroy()
	{
		try
		{
			Singleton<ProjectileManager>.Instance.RemoveAllProjectilesFromPlayer(State.PlayerNumber);
			Singleton<QuickItemSfxController>.Instance.DestroytSfxFromPlayer(State.PlayerNumber);
			_state.UnSubscribeAll();
			Avatar.EnableDecorator();
			if ((bool)Avatar.Decorator && IsLocal)
			{
				Avatar.Decorator.transform.parent = null;
			}
			AvatarBuilder.Destroy(base.gameObject);
		}
		catch
		{
			Debug.LogWarning("Character already destroyed");
		}
	}

	private void PlayDamageSound()
	{
		if (IsLocal)
		{
			if (_state.Info.Armor.HasArmor && _state.Info.Armor.HasArmorPoints)
			{
				SfxManager.Play2dAudioClip(GameAudio.LocalPlayerHitArmorRemaining);
			}
			else if (_state.Info.Health < 25)
			{
				SfxManager.Play2dAudioClip(GameAudio.LocalPlayerHitNoArmorLowHealth);
			}
			else
			{
				SfxManager.Play2dAudioClip(GameAudio.LocalPlayerHitNoArmor);
			}
		}
	}

	private void ShowDamageFeedback(DamageInfo shot)
	{
		PlayerDamageEffect playerDamageEffect = UnityEngine.Object.Instantiate(_damageFeedback, shot.Hitpoint, (!(shot.Force.magnitude > 0f)) ? Quaternion.identity : Quaternion.LookRotation(shot.Force)) as PlayerDamageEffect;
		if ((bool)playerDamageEffect)
		{
			playerDamageEffect.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
			playerDamageEffect.Show(shot);
		}
	}
}
