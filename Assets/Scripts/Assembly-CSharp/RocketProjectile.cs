using UnityEngine;

public class RocketProjectile : Projectile
{
	[SerializeField]
	private Renderer _smokeRenderer;

	[SerializeField]
	private ParticleSystem _smokeEmitter;

	[SerializeField]
	private Color _smokeColor = Color.white;

	[SerializeField]
	private float _smokeAmount = 1f;

	[SerializeField]
	private Light _light;

	public Color SmokeColor
	{
		get
		{
			return _smokeColor;
		}
		set
		{
			_smokeColor = value;
			if ((bool)_smokeRenderer)
			{
				_smokeRenderer.material.SetColor("_TintColor", _smokeColor);
			}
		}
	}

	public float SmokeAmount
	{
		get
		{
			return _smokeAmount;
		}
		set
		{
			_smokeAmount = value;
			if ((bool)_smokeEmitter)
			{
				var emission = _smokeEmitter.emission;
				emission.rateOverTime = _smokeAmount * 15f; // Average of old min/max values
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		SmokeColor = _smokeColor;
		SmokeAmount = _smokeAmount;
		if (_light != null)
		{
			_light.enabled = ApplicationDataManager.ApplicationOptions.VideoQualityLevel == 2;
		}
	}

	protected override void OnTriggerEnter(Collider c)
	{
		if (!base.IsProjectileExploded && LayerUtil.IsLayerInMask(base.CollisionMask, c.gameObject.layer))
		{
			Singleton<ProjectileManager>.Instance.RemoveProjectile(ID);
			GameState.CurrentGame.RemoveProjectile(ID, true);
		}
	}

	protected override void OnCollisionEnter(Collision c)
	{
		if (!base.IsProjectileExploded && (bool)c.gameObject && LayerUtil.IsLayerInMask(base.CollisionMask, c.gameObject.layer))
		{
			Singleton<ProjectileManager>.Instance.RemoveProjectile(ID);
			GameState.CurrentGame.RemoveProjectile(ID, true);
		}
	}
}
