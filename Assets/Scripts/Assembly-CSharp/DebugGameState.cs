using UnityEngine;

public class DebugGameState : IDebugPage
{
	private Vector2 v1;

	public string Title
	{
		get
		{
			return "Game";
		}
	}

	public void Draw()
	{
		if (GameState.CurrentGame != null)
		{
			v1 = GUILayout.BeginScrollView(v1);
			GUILayout.Label("Type:" + GameState.CurrentGame.GetType().ToString());
			GUILayout.Label("Room:" + GameState.CurrentGame.GameData.RoomID.ToString());
			GUILayout.Label("IsGameStarted:" + GameState.CurrentGame.IsGameStarted);
			GUILayout.Label("IsRoundRunning:" + GameState.CurrentGame.IsMatchRunning);
			GUILayout.Label("GameTime:" + GameState.CurrentGame.GameTime.ToString("N2"));
			GUILayout.Label("Latency:" + GameConnectionManager.Client.Latency.ToString("N0"));
			GUILayout.Label("Ping:" + GameConnectionManager.Client.PeerListener.Ping.ToString("N0"));
			GUILayout.Label("CameraState:" + GameState.LocalPlayer.CurrentCameraControl);
			GUILayout.Label("IsHudEnabled:" + HudController.Instance.enabled);
			GUILayout.Label("HudDrawFlags:" + HudController.Instance.DrawFlagString);
			GUILayout.Label("IsGamePaused:" + GameState.LocalPlayer.IsGamePaused);
			GUILayout.Label("IsInputEnabled:" + AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled);
			GUILayout.Label("PlayerSpectator:" + Singleton<PlayerSpectatorControl>.Instance.IsEnabled);
			GUILayout.Label("MyPlayerID:" + GameState.CurrentGame.MyActorId);
			GUILayout.Label("lockCursor:" + Screen.lockCursor);
			GUILayout.Label("IsMouseLockStateConsistent: " + GameState.LocalPlayer.IsMouseLockStateConsistent);
			GUILayout.Label("IsShootingEnabled: " + GameState.LocalPlayer.IsShootingEnabled);
			GUILayout.Label("IsWalkingEnabled: " + GameState.LocalPlayer.IsWalkingEnabled);
			GUILayout.Label("IsWeaponControlEnabled: " + Singleton<WeaponController>.Instance.IsEnabled);
			GUILayout.Label("Players: " + (GameState.HasCurrentGame ? GameState.CurrentGame.Players.Count : 0));
			GUILayout.EndScrollView();
		}
	}
}
