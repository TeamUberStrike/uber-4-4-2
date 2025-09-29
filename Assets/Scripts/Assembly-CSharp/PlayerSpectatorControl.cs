using System;
using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class PlayerSpectatorControl : Singleton<PlayerSpectatorControl>
{
	private int _currentFollowActorId;

	private int _currentFollowIndex;

	private bool _isEnabled;

	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (value)
			{
				EnterFreeMoveMode();
				if (!_isEnabled)
				{
					CmuneEventHandler.AddListener<InputChangeEvent>(OnInputChanged);
					CmuneEventHandler.Route(new OnPlayerSpectatingEvent());
				}
			}
			else
			{
				ReleaseLastTarget();
				if (_isEnabled)
				{
					if ((bool)GameState.LocalPlayer && (bool)GameState.LocalAvatar.Decorator)
					{
						GameState.LocalPlayer.SetPlayerControlState(LocalPlayer.PlayerState.FirstPerson, GameState.LocalAvatar.Decorator.Configuration);
					}
					CmuneEventHandler.RemoveListener<InputChangeEvent>(OnInputChanged);
					CmuneEventHandler.Route(new OnPlayerUnspecatingEvent());
				}
			}
			_isEnabled = value;
		}
	}

	public int CurrentActorId
	{
		get
		{
			return _currentFollowActorId;
		}
	}

	public bool IsFollowingPlayer
	{
		get
		{
			return _currentFollowActorId > 0;
		}
	}

	private PlayerSpectatorControl()
	{
	}

	public void OnInputChanged(InputChangeEvent ev)
	{
		if (!Singleton<InGameChatHud>.Instance.CanInput && Screen.lockCursor)
		{
			if (ev.Key == GameInputKey.PrimaryFire && ev.IsDown)
			{
				FollowPrevPlayer();
			}
			else if (ev.Key == GameInputKey.SecondaryFire && ev.IsDown)
			{
				FollowNextPlayer();
			}
			else if (ev.Key == GameInputKey.Jump && ev.IsDown)
			{
				EnterFreeMoveMode();
			}
		}
	}

	public void FollowNextPlayer()
	{
		try
		{
			if (!GameState.HasCurrentGame || GameState.CurrentGame.Players.Count <= 0)
			{
				return;
			}
			UberStrike.Realtime.UnitySdk.CharacterInfo[] array = GameState.CurrentGame.Players.ValueArray();
			_currentFollowIndex = (_currentFollowIndex + 1) % array.Length;
			int currentFollowIndex = _currentFollowIndex;
			while (array[_currentFollowIndex].ActorId == GameState.CurrentPlayerID || !array[_currentFollowIndex].IsAlive || !GameState.CurrentGame.HasAvatarLoaded(array[_currentFollowIndex].ActorId))
			{
				_currentFollowIndex = (_currentFollowIndex + 1) % array.Length;
				if (_currentFollowIndex == currentFollowIndex)
				{
					EnterFreeMoveMode();
					return;
				}
			}
			if (array[_currentFollowIndex] != null)
			{
				ChangeTarget(array[_currentFollowIndex].ActorId);
			}
			else
			{
				EnterFreeMoveMode();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to follow next player: " + ex.Message);
		}
	}

	public void FollowPrevPlayer()
	{
		try
		{
			if (!GameState.HasCurrentGame || GameState.CurrentGame.Players.Count <= 0)
			{
				return;
			}
			List<UberStrike.Realtime.UnitySdk.CharacterInfo> list = new List<UberStrike.Realtime.UnitySdk.CharacterInfo>(GameState.CurrentGame.Players.Values);
			_currentFollowIndex = (_currentFollowIndex + list.Count - 1) % list.Count;
			int currentFollowIndex = _currentFollowIndex;
			while (list[_currentFollowIndex].ActorId == GameState.CurrentPlayerID || !list[_currentFollowIndex].IsAlive || !GameState.CurrentGame.HasAvatarLoaded(list[_currentFollowIndex].ActorId))
			{
				_currentFollowIndex = (_currentFollowIndex + list.Count - 1) % list.Count;
				if (_currentFollowIndex == currentFollowIndex)
				{
					EnterFreeMoveMode();
					return;
				}
			}
			if (list[_currentFollowIndex] != null)
			{
				ChangeTarget(list[_currentFollowIndex].ActorId);
			}
			else
			{
				EnterFreeMoveMode();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to follow prev player: " + ex.Message);
		}
	}

	public void ReleaseLastTarget()
	{
		CharacterConfig character;
		if (GameState.HasCurrentGame && _currentFollowActorId > 0 && GameState.CurrentGame.TryGetCharacter(_currentFollowActorId, out character))
		{
			character.RemoveFollowCamera();
		}
		_currentFollowActorId = 0;
	}

	private void ChangeTarget(int actorId)
	{
		if (!GameState.HasCurrentGame || _currentFollowActorId == actorId)
		{
			return;
		}
		ReleaseLastTarget();
		CharacterConfig character;
		if (GameState.CurrentGame.TryGetCharacter(actorId, out character) && (bool)character.Avatar.Decorator)
		{
			_currentFollowActorId = actorId;
			LevelCamera.Instance.SetTarget(character.Avatar.Decorator.transform);
			LevelCamera.Instance.SetMode(LevelCamera.CameraMode.SmoothFollow);
			if (!character.State.IsAlive)
			{
				LevelCamera.Instance.transform.position = character.State.Position;
			}
			LevelCamera.Instance.SetLookAtHeight(1.3f);
			character.AddFollowCamera();
			character.Avatar.Decorator.HudInformation.ForceShowInformation = true;
		}
	}

	public void EnterFreeMoveMode()
	{
		ReleaseLastTarget();
		Screen.lockCursor = true;
		LevelCamera.Instance.SetLookAtHeight(0f);
		LevelCamera.Instance.SetMode(LevelCamera.CameraMode.Spectator);
	}
}
