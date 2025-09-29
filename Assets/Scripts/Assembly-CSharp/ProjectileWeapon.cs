using System;
using System.Collections;
using UberStrike.Core.Models.Views;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class ProjectileWeapon : BaseWeaponLogic
{
	private UberStrikeItemWeaponView _view;

	private ProjectileWeaponDecorator _decorator;

	public override BaseWeaponDecorator Decorator
	{
		get
		{
			return _decorator;
		}
	}

	public int MaxConcurrentProjectiles { get; private set; }

	public int MinProjectileDistance { get; private set; }

	public int ProjetileCountPerShoot { get; set; }

	public bool HasProjectileLimit
	{
		get
		{
			return MaxConcurrentProjectiles > 0;
		}
	}

	public ParticleConfigurationType ExplosionType { get; private set; }

	public event Action<ProjectileInfo> OnProjectileShoot;

	public ProjectileWeapon(WeaponItem item, ProjectileWeaponDecorator decorator, IWeaponController controller, UberStrikeItemWeaponView view)
		: base(item, controller)
	{
		_view = view;
		_decorator = decorator;
		MaxConcurrentProjectiles = item.Configuration.MaxConcurrentProjectiles;
		MinProjectileDistance = item.Configuration.MinProjectileDistance;
		ExplosionType = item.Configuration.ParticleEffect;
		ProjetileCountPerShoot = view.ProjectilesPerShot;
	}

	public override void Shoot(Ray ray, out CmunePairList<BaseGameProp, ShotPoint> hits)
	{
		hits = null;
		RaycastHit hitInfo;
		if (MinProjectileDistance > 0 && Physics.Raycast(ray.origin, ray.direction, out hitInfo, MinProjectileDistance, UberstrikeLayerMasks.LocalRocketMask))
		{
			int num = base.Controller.NextProjectileId();
			hits = new CmunePairList<BaseGameProp, ShotPoint>(1);
			hits.Add(null, new ShotPoint(hitInfo.point, num));
			ShowExplosionEffect(hitInfo.point, hitInfo.normal, ray.direction, num);
			if (this.OnProjectileShoot != null)
			{
				this.OnProjectileShoot(new ProjectileInfo(num, new Ray(hitInfo.point, -ray.direction)));
			}
		}
		else
		{
			if ((bool)_decorator)
			{
				_decorator.ShowShootEffect(new RaycastHit[0]);
			}
			MonoRoutine.Start(EmitProjectile(ray));
		}
	}

	public void ShowExplosionEffect(Vector3 position, Vector3 normal, Vector3 direction, int projectileId)
	{
		if ((bool)_decorator)
		{
			_decorator.ShowExplosionEffect(position, normal, ExplosionType);
		}
	}

	private IEnumerator EmitProjectile(Ray ray)
	{
		if (ProjetileCountPerShoot > 1)
		{
			float angle = 360 / ProjetileCountPerShoot;
			for (int i = 0; i < ProjetileCountPerShoot; i++)
			{
				if ((bool)_decorator)
				{
					int shotCount = base.Controller.NextProjectileId();
					ray.origin = _decorator.MuzzlePosition + Quaternion.AngleAxis(angle * (float)i, _decorator.transform.forward) * _decorator.transform.up * 0.2f;
					Projectile p = EmitProjectile(ray, shotCount, GameState.LocalCharacter.ActorId);
					if ((bool)p && this.OnProjectileShoot != null)
					{
						this.OnProjectileShoot(new ProjectileInfo(shotCount, ray)
						{
							Projectile = p
						});
					}
					yield return new WaitForSeconds(0.2f);
				}
			}
		}
		else
		{
			int shotCount2 = base.Controller.NextProjectileId();
			Projectile p2 = EmitProjectile(ray, shotCount2, GameState.LocalCharacter.ActorId);
			if ((bool)p2 && this.OnProjectileShoot != null)
			{
				this.OnProjectileShoot(new ProjectileInfo(shotCount2, ray)
				{
					Projectile = p2
				});
			}
		}
	}

	public Projectile EmitProjectile(Ray ray, int projectileID, int actorID)
	{
		if ((bool)_decorator && (bool)_decorator.Missle)
		{
			Vector3 muzzlePosition = _decorator.MuzzlePosition;
			Quaternion rotation = Quaternion.LookRotation(ray.direction);
			Projectile projectile = UnityEngine.Object.Instantiate(_decorator.Missle, muzzlePosition, rotation) as Projectile;
			if ((bool)projectile)
			{
				if (projectile is GrenadeProjectile)
				{
					GrenadeProjectile grenadeProjectile = projectile as GrenadeProjectile;
					grenadeProjectile.Sticky = base.Config.Sticky;
				}
				projectile.transform.parent = ProjectileManager.ProjectileContainer.transform;
				projectile.gameObject.tag = "Prop";
				projectile.ExplosionEffect = ExplosionType;
				projectile.TimeOut = _decorator.MissileTimeOut;
				projectile.SetExplosionSound(_decorator.ExplosionSound);
				projectile.transform.position = ray.origin + MinProjectileDistance * ray.direction;
				if (base.Controller.IsLocal)
				{
					projectile.gameObject.layer = 26;
				}
				else
				{
					projectile.gameObject.layer = 24;
				}
				CharacterConfig character;
				if (GameState.CurrentGame != null && GameState.CurrentGame.TryGetCharacter(actorID, out character) && (bool)character.Avatar.Decorator && projectile.gameObject.activeSelf)
				{
					CharacterHitArea[] hitAreas = character.Avatar.Decorator.HitAreas;
					foreach (CharacterHitArea characterHitArea in hitAreas)
					{
						if (characterHitArea.gameObject.activeInHierarchy)
						{
							Physics.IgnoreCollision(projectile.gameObject.collider, characterHitArea.collider);
						}
					}
				}
				projectile.MoveInDirection(ray.direction * WeaponConfigurationHelper.GetProjectileSpeed(_view));
				return projectile;
			}
		}
		else
		{
			Debug.LogError("Failed to create projectile!");
		}
		return null;
	}
}
