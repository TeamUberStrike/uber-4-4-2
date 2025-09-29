using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class GamePageUtil
{
	public static void SpawnLocalAvatar()
	{
		if ((bool)GameState.LocalAvatar.Decorator)
		{
			Vector3 pos;
			Quaternion rot;
			GetAvatarSpawnPoint(out pos, out rot);
			AutoMonoBehaviour<AvatarAnimationManager>.Instance.ResetAnimationState(PageType.Shop);
			GameState.LocalAvatar.Decorator.HideWeapons();
			GameState.LocalAvatar.Decorator.SetPosition(pos, rot);
			GameState.LocalAvatar.Decorator.MeshRenderer.enabled = true;
			GameState.LocalAvatar.Decorator.SetLayers(UberstrikeLayer.RemotePlayer);
			GameState.LocalPlayer.transform.position = pos + Vector3.up * 2f;
		}
	}

	private static void GetAvatarSpawnPoint(out Vector3 pos, out Quaternion rot)
	{
		Singleton<SpawnPointManager>.Instance.GetSpawnPointAt(GameState.CurrentSpace.DefaultSpawnPoint, GameMode.DeathMatch, TeamID.NONE, out pos, out rot);
		RaycastHit hitInfo;
		if (Physics.Raycast(pos, Vector3.down, out hitInfo, 5f))
		{
			pos = hitInfo.point;
		}
		rot = GetCollisionFreeRotation(pos);
	}

	private static Quaternion GetCollisionFreeRotation(Vector3 pos)
	{
		float num = Random.Range(0f, 45f);
		float num2 = 0f;
		Quaternion result = Quaternion.identity;
		for (int i = 0; i < 8; i++)
		{
			Quaternion quaternion = Quaternion.Euler(0f, (float)(i * 45) + num, 0f);
			Vector3 origin = pos + Vector3.up * 1.5f;
			Vector3 direction = quaternion * LevelCamera.OverviewState.ViewDirection;
			RaycastHit hitInfo;
			if (Physics.Raycast(origin, direction, out hitInfo, 7f, UberstrikeLayerMasks.ShootMask))
			{
				if (hitInfo.distance > num2)
				{
					num2 = hitInfo.distance;
					result = quaternion;
				}
				continue;
			}
			result = quaternion;
			break;
		}
		return result;
	}
}
