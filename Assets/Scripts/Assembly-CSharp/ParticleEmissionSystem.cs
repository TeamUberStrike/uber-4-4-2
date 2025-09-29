using UnityEngine;

public static class ParticleEmissionSystem
{
	public static void TrailParticles(Vector3 emitPoint, Vector3 direction, TrailParticleConfiguration particleConfiguration, Vector3 muzzlePosition, float distance)
	{
		if (particleConfiguration.ParticleSystem != null)
		{
			float num = 200f;
			Vector3 velocity = direction * num;
			float energy = distance / num * 0.9f;
			if (distance > 3f)
			{
				// Use modern ParticleSystem.EmitParams instead of obsolete Emit method
				var emitParams = new ParticleSystem.EmitParams();
				emitParams.position = muzzlePosition + direction * 3f;
				emitParams.velocity = velocity;
				emitParams.startSize = Random.Range(particleConfiguration.ParticleMinSize, particleConfiguration.ParticleMaxSize);
				emitParams.startLifetime = energy;
				emitParams.startColor = particleConfiguration.ParticleColor;
				particleConfiguration.ParticleSystem.Emit(emitParams, 1);
			}
		}
	}

	public static void FireParticles(Vector3 hitPoint, Vector3 hitNormal, FireParticleConfiguration particleConfiguration)
	{
		if (particleConfiguration.ParticleSystem != null)
		{
			Vector3 vector = Vector3.zero;
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, hitNormal);
			Vector3 zero = Vector3.zero;
			ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
			for (int i = 0; i < particleConfiguration.ParticleCount; i++)
			{
				vector.x = 0f + Random.Range(0f, 0.001f);
				vector.y = 2f + Random.Range(0f, 0.4f);
				vector.z = 0f + Random.Range(0f, 0.001f);
				vector = quaternion * vector;
				zero = hitPoint;
				zero.x += Random.Range(0f, 0.2f);
				zero.z += Random.Range(0f, 0.4f) * -1f;
				emitParams.position = zero;
				emitParams.velocity = vector;
				emitParams.startSize = Random.Range(particleConfiguration.ParticleMinSize, particleConfiguration.ParticleMaxSize);
				emitParams.startLifetime = Random.Range(particleConfiguration.ParticleMinLiveTime, particleConfiguration.ParticleMaxLiveTime);
				emitParams.startColor = particleConfiguration.ParticleColor;
				particleConfiguration.ParticleSystem.Emit(emitParams, 1);
			}
		}
	}

	public static void WaterCircleParticles(Vector3 hitPoint, Vector3 hitNormal, FireParticleConfiguration particleConfiguration)
	{
		if (particleConfiguration.ParticleSystem != null)
		{
			Vector3 zero = Vector3.zero;
			ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
			for (int i = 0; i < particleConfiguration.ParticleCount; i++)
			{
				zero.x = Random.Range(0f, 0.3f);
				zero.z = Random.Range(0f, 0.3f);
				emitParams.position = hitPoint;
				emitParams.velocity = zero;
				emitParams.startSize = Random.Range(particleConfiguration.ParticleMinSize, particleConfiguration.ParticleMaxSize);
				emitParams.startLifetime = Random.Range(particleConfiguration.ParticleMinLiveTime, particleConfiguration.ParticleMaxLiveTime);
				emitParams.startColor = particleConfiguration.ParticleColor;
				particleConfiguration.ParticleSystem.Emit(emitParams, 1);
			}
		}
	}

	public static void WaterSplashParticles(Vector3 hitPoint, Vector3 hitNormal, FireParticleConfiguration particleConfiguration)
	{
		if (particleConfiguration.ParticleSystem != null)
		{
			Vector3 zero = Vector3.zero;
			ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
			for (int i = 0; i < particleConfiguration.ParticleCount; i++)
			{
				zero.x = Random.Range(0f, 0.3f);
				zero.y = 2f + Random.Range(0f, 0.3f);
				zero.z = Random.Range(0f, 0.3f);
				emitParams.position = hitPoint;
				emitParams.velocity = zero;
				emitParams.startSize = Random.Range(particleConfiguration.ParticleMinSize, particleConfiguration.ParticleMaxSize);
				emitParams.startLifetime = Random.Range(particleConfiguration.ParticleMinLiveTime, particleConfiguration.ParticleMaxLiveTime);
				emitParams.startColor = particleConfiguration.ParticleColor;
				particleConfiguration.ParticleSystem.Emit(emitParams, 1);
			}
		}
	}

	public static void HitMaterialParticles(Vector3 hitPoint, Vector3 hitNormal, ParticleConfiguration particleConfiguration)
	{
		if (particleConfiguration.ParticleSystem != null)
		{
			Vector3 vector = Vector3.zero;
			Quaternion quaternion = default(Quaternion);
			quaternion = Quaternion.FromToRotation(Vector3.back, hitNormal);
			for (int i = 0; i < particleConfiguration.ParticleCount; i++)
			{
				Vector2 vector2 = Random.insideUnitCircle * Random.Range(particleConfiguration.ParticleMinSpeed, particleConfiguration.ParticleMaxSpeed);
				vector.x = vector2.x;
				vector.y = vector2.y;
				vector.z = Random.Range(particleConfiguration.ParticleMinZVelocity, particleConfiguration.ParticleMaxZVelocity);
				vector = quaternion * vector;
				
				// Use modern ParticleSystem.EmitParams instead of obsolete Emit method
				var emitParams = new ParticleSystem.EmitParams();
				emitParams.position = hitPoint;
				emitParams.velocity = vector;
				emitParams.startSize = Random.Range(particleConfiguration.ParticleMinSize, particleConfiguration.ParticleMaxSize);
				emitParams.startLifetime = Random.Range(particleConfiguration.ParticleMinLiveTime, particleConfiguration.ParticleMaxLiveTime);
				emitParams.startColor = particleConfiguration.ParticleColor;
				particleConfiguration.ParticleSystem.Emit(emitParams, 1);
			}
		}
	}

	public static void HitMaterialRotatingParticles(Vector3 hitPoint, Vector3 hitNormal, ParticleConfiguration particleConfiguration)
	{
		if (particleConfiguration.ParticleSystem != null)
		{
			Vector3 vector = Vector3.zero;
			Quaternion quaternion = default(Quaternion);
			quaternion = Quaternion.FromToRotation(Vector3.back, hitNormal);
			for (int i = 0; i < particleConfiguration.ParticleCount; i++)
			{
				Vector2 vector2 = Random.insideUnitCircle * Random.Range(particleConfiguration.ParticleMinSpeed, particleConfiguration.ParticleMaxSpeed);
				vector.x = vector2.x;
				vector.y = vector2.y;
				vector.z = Random.Range(particleConfiguration.ParticleMinZVelocity, particleConfiguration.ParticleMaxZVelocity) * -1f;
				vector = quaternion * vector;
				
				// Use modern ParticleSystem.EmitParams instead of obsolete Emit method
				var emitParams = new ParticleSystem.EmitParams();
				emitParams.position = hitPoint;
				emitParams.velocity = vector;
				emitParams.startSize = Random.Range(particleConfiguration.ParticleMinSize, particleConfiguration.ParticleMaxSize);
				emitParams.startLifetime = Random.Range(particleConfiguration.ParticleMinLiveTime, particleConfiguration.ParticleMaxLiveTime);
				emitParams.startColor = particleConfiguration.ParticleColor;
				emitParams.rotation = Random.Range(0f, 360f);
				particleConfiguration.ParticleSystem.Emit(emitParams, 1);
			}
		}
	}

	public static void HitMateriaHalfSphericParticles(Vector3 hitPoint, Vector3 hitNormal, ParticleConfiguration particleConfiguration)
	{
		if (!(particleConfiguration.ParticleSystem != null))
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		Quaternion quaternion = default(Quaternion);
		quaternion = Quaternion.FromToRotation(Vector3.back, hitNormal);
		for (int i = 0; i < particleConfiguration.ParticleCount; i++)
		{
			zero = Random.insideUnitSphere * Random.Range(particleConfiguration.ParticleMinSpeed, particleConfiguration.ParticleMaxSpeed);
			if (zero.z > 0f)
			{
				zero.z *= -1f;
			}
			zero = quaternion * zero;
			
			// Use modern ParticleSystem.EmitParams instead of obsolete Emit method
			var emitParams = new ParticleSystem.EmitParams();
			emitParams.position = hitPoint;
			emitParams.velocity = zero;
			emitParams.startSize = Random.Range(particleConfiguration.ParticleMinSize, particleConfiguration.ParticleMaxSize);
			emitParams.startLifetime = Random.Range(particleConfiguration.ParticleMinLiveTime, particleConfiguration.ParticleMaxLiveTime);
			emitParams.startColor = particleConfiguration.ParticleColor;
			particleConfiguration.ParticleSystem.Emit(emitParams, 1);
		}
	}

	public static void HitMateriaFullSphericParticles(Vector3 hitPoint, Vector3 hitNormal, ParticleConfiguration particleConfiguration)
	{
		if (particleConfiguration.ParticleSystem != null)
		{
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < particleConfiguration.ParticleCount; i++)
			{
				zero = Random.insideUnitSphere * Random.Range(particleConfiguration.ParticleMinSpeed, particleConfiguration.ParticleMaxSpeed);
				
				// Use modern ParticleSystem.EmitParams instead of obsolete Emit method
				var emitParams = new ParticleSystem.EmitParams();
				emitParams.position = hitPoint;
				emitParams.velocity = zero;
				emitParams.startSize = Random.Range(particleConfiguration.ParticleMinSize, particleConfiguration.ParticleMaxSize);
				emitParams.startLifetime = Random.Range(particleConfiguration.ParticleMinLiveTime, particleConfiguration.ParticleMaxLiveTime);
				emitParams.startColor = particleConfiguration.ParticleColor;
				particleConfiguration.ParticleSystem.Emit(emitParams, 1);
			}
		}
	}
}
