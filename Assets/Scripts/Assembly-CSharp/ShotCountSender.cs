using System.Collections.Generic;
using UberStrike.Core.Types;
using UnityEngine;

public class ShotCountSender
{
	private const float SendDuration = 10f;

	private UberstrikeItemClass[] _weaponClasses = new UberstrikeItemClass[7]
	{
		UberstrikeItemClass.WeaponCannon,
		UberstrikeItemClass.WeaponLauncher,
		UberstrikeItemClass.WeaponMachinegun,
		UberstrikeItemClass.WeaponMelee,
		UberstrikeItemClass.WeaponShotgun,
		UberstrikeItemClass.WeaponSniperRifle,
		UberstrikeItemClass.WeaponSplattergun
	};

	private FpsGameMode _game;

	private float _nextSendTime;

	public ShotCountSender(FpsGameMode game)
	{
		_game = game;
		_nextSendTime = Time.time + 10f;
	}

	public void Send()
	{
		List<int> list = new List<int>(_weaponClasses.Length);
		UberstrikeItemClass[] weaponClasses = _weaponClasses;
		foreach (UberstrikeItemClass weaponClass in weaponClasses)
		{
			int shotCount = Singleton<WeaponController>.Instance.ShotCounter.GetShotCount(weaponClass);
			list.Add(shotCount);
		}
		_game.SendShotCounts(list);
	}

	public void UpdateEveryTenSeconds()
	{
		if (Time.time >= _nextSendTime)
		{
			_nextSendTime += 10f;
			Send();
		}
	}

	public void UpdateEverySecond()
	{
		if (_nextSendTime - Time.time > 1f)
		{
			_nextSendTime = Time.time + 1f;
		}
		if (Time.time >= _nextSendTime)
		{
			_nextSendTime = Time.time + 1f;
			Send();
		}
	}
}
