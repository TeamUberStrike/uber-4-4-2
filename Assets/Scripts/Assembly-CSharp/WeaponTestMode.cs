using System.Collections;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

[NetworkClass(-1)]
public class WeaponTestMode : FpsGameMode
{
	public WeaponTestMode(RemoteMethodInterface rmi)
		: base(rmi, new GameMetaData(0, string.Empty, 120, 0, 0))
	{
	}

	protected override void OnInitialized()
	{
		base.OnInitialized();
		MonoRoutine.Run(StartDecreasingHealthAndArmor);
		MonoRoutine.Run(SimulateGameFrameUpdate);
		Singleton<ArmorHud>.Instance.Enabled = false;
	}

	protected override void OnCharacterLoaded()
	{
		Vector3 position;
		Quaternion rotation;
		Singleton<SpawnPointManager>.Instance.GetSpawnPointAt(0, GameMode.TryWeapon, TeamID.NONE, out position, out rotation);
		SpawnPlayerAt(position, rotation);
		if (LevelCamera.Instance != null)
		{
			LevelCamera.Instance.MainCamera.rect = new Rect(0f, 0f, 1f, 1f);
		}
	}

	protected override void OnModeInitialized()
	{
		OnPlayerJoined(SyncObjectBuilder.GetSyncData(GameState.LocalCharacter, true), Vector3.zero);
		base.IsMatchRunning = true;
		GameState.LocalPlayer.SetWeaponControlState(PlayerHudState.Playing);
	}

	public override void PlayerHit(int targetPlayer, short damage, BodyPart part, Vector3 force, int shotCount, int weaponID, UberstrikeItemClass weaponClass, DamageEffectType damageEffectFlag, float damageEffectValue)
	{
		GameState.LocalPlayer.MoveController.ApplyForce(force, CharacterMoveController.ForceType.Additive);
	}

	protected override void ApplyCurrentGameFrameUpdates(SyncObject delta)
	{
		base.ApplyCurrentGameFrameUpdates(delta);
		if (delta.Contains(2097152) && !GameState.LocalCharacter.IsAlive)
		{
			OnSetNextSpawnPoint(Random.Range(0, Singleton<SpawnPointManager>.Instance.GetSpawnPointCount(GameMode.TryWeapon, TeamID.NONE)), 3);
		}
	}

	public override void RequestRespawn()
	{
		OnSetNextSpawnPoint(Random.Range(0, Singleton<SpawnPointManager>.Instance.GetSpawnPointCount(GameMode.TryWeapon, TeamID.NONE)), 3);
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
			GameState.LocalCharacter.Health += (short)value;
			break;
		}
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
			yield return new WaitForSeconds(0.1f);
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
}
