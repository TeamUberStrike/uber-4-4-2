using System.Collections;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

[NetworkClass(-1)]
public class TrainingFpsMode : FpsGameMode
{
	public override float GameTime
	{
		get
		{
			return Time.realtimeSinceStartup;
		}
	}

	public TrainingFpsMode(RemoteMethodInterface rmi, int mapId)
		: base(rmi, new GameMetaData(mapId, string.Empty, 120, 0, 109))
	{
	}

	protected override void OnInitialized()
	{
		base.OnInitialized();
		MonoRoutine.Run(StartDecreasingHealthAndArmor);
		MonoRoutine.Run(SimulateGameFrameUpdate);
	}

	protected override void OnCharacterLoaded()
	{
		OnSetNextSpawnPoint(Random.Range(0, Singleton<SpawnPointManager>.Instance.GetSpawnPointCount(GameMode.Training, TeamID.NONE)), 0);
	}

	protected override void OnModeInitialized()
	{
		OnPlayerJoined(SyncObjectBuilder.GetSyncData(GameState.LocalCharacter, true), Vector3.zero);
		base.IsMatchRunning = true;
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
			OnSetNextSpawnPoint(Random.Range(0, Singleton<SpawnPointManager>.Instance.GetSpawnPointCount(GameMode.Training, TeamID.NONE)), 3);
		}
	}

	public override void RequestRespawn()
	{
		OnSetNextSpawnPoint(Random.Range(0, Singleton<SpawnPointManager>.Instance.GetSpawnPointCount(GameMode.Training, TeamID.NONE)), 3);
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
