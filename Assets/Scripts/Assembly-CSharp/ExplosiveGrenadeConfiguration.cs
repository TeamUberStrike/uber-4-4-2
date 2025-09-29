using System;
using UnityEngine;

[Serializable]
public class ExplosiveGrenadeConfiguration : QuickItemConfiguration
{
	[SerializeField]
	[CustomProperty("Damage")]
	private int _damage = 100;

	[SerializeField]
	[CustomProperty("SplashRadius")]
	private int _splash = 2;

	[CustomProperty("LifeTime")]
	[SerializeField]
	private int _lifeTime = 15;

	[SerializeField]
	[CustomProperty("Bounciness")]
	private int _bounciness = 3;

	[SerializeField]
	[CustomProperty("Sticky")]
	private bool _isSticky = true;

	[SerializeField]
	private int _speed = 15;

	public int Damage
	{
		get
		{
			return _damage;
		}
	}

	public int SplashRadius
	{
		get
		{
			return _splash;
		}
	}

	public int LifeTime
	{
		get
		{
			return _lifeTime;
		}
	}

	public float Bounciness
	{
		get
		{
			return (float)_bounciness * 0.1f;
		}
	}

	public bool IsSticky
	{
		get
		{
			return _isSticky;
		}
	}

	public int Speed
	{
		get
		{
			return _speed;
		}
	}
}
