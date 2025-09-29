using System.Collections.Generic;
using UberStrike.Core.Types;

public class WeaponShotCounter
{
	private Dictionary<UberstrikeItemClass, int> _shotCountPerWeaponClass;

	private UberstrikeItemClass[] _allWeaponClasses = new UberstrikeItemClass[7]
	{
		UberstrikeItemClass.WeaponCannon,
		UberstrikeItemClass.WeaponLauncher,
		UberstrikeItemClass.WeaponMachinegun,
		UberstrikeItemClass.WeaponMelee,
		UberstrikeItemClass.WeaponShotgun,
		UberstrikeItemClass.WeaponSniperRifle,
		UberstrikeItemClass.WeaponSplattergun
	};

	public WeaponShotCounter()
	{
		_shotCountPerWeaponClass = new Dictionary<UberstrikeItemClass, int>();
		Reset();
	}

	public void Reset()
	{
		UberstrikeItemClass[] allWeaponClasses = _allWeaponClasses;
		foreach (UberstrikeItemClass key in allWeaponClasses)
		{
			_shotCountPerWeaponClass[key] = 0;
		}
	}

	public int GetShotCount(UberstrikeItemClass weaponClass)
	{
		return _shotCountPerWeaponClass[weaponClass];
	}

	public void IncreaseShotCount(UberstrikeItemClass weaponClass)
	{
		Dictionary<UberstrikeItemClass, int> shotCountPerWeaponClass;
		Dictionary<UberstrikeItemClass, int> dictionary = (shotCountPerWeaponClass = _shotCountPerWeaponClass);
		UberstrikeItemClass key2;
		UberstrikeItemClass key = (key2 = weaponClass);
		int num = shotCountPerWeaponClass[key2];
		dictionary[key] = num + 1;
	}
}
