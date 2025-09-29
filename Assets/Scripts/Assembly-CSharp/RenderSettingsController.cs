using System;
using System.Reflection;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class RenderSettingsController : MonoBehaviour
{
	private const float UNDERWATER_FOG_START = 10f;

	private const float UNDERWATER_FOG_END = 50f;

	private const float FADE_SPEED = 4f;

	private static volatile RenderSettingsController _instance;

	private static object _lock = new object();

	private float lerpValue;

	private float fogStart;

	private float fogEnd;

	private Color fogColor;

	private FogMode fogMode;

	[SerializeField]
	private Color underwaterFogColor;

	[SerializeField]
	private GameObject advancedWater;

	[SerializeField]
	private GameObject simpleWater;

	[SerializeField]
	private MonoBehaviour[] simpleImageEffects;

	[SerializeField]
	private PostEffectsBase[] advancedImageEffects;

	public static RenderSettingsController Instance
	{
		get
		{
			if (_instance == null)
			{
				lock (_lock)
				{
					if (_instance == null)
					{
						ConstructorInfo constructor = typeof(RenderSettingsController).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);
						if (constructor == null || constructor.IsAssembly)
						{
							throw new Exception(string.Format("A private or protected constructor is missing for '{0}'.", typeof(RenderSettingsController).Name));
						}
						_instance = (RenderSettingsController)constructor.Invoke(null);
					}
				}
			}
			return _instance;
		}
	}

	private void OnEnable()
	{
		_instance = this;
		lerpValue = 0f;
		fogMode = RenderSettings.fogMode;
		fogColor = RenderSettings.fogColor;
		fogStart = RenderSettings.fogStartDistance;
		fogEnd = RenderSettings.fogEndDistance;
		if (simpleWater != null)
		{
			simpleWater.SetActive(ApplicationDataManager.IsMobile);
		}
		if (advancedWater != null)
		{
			advancedWater.SetActive(!ApplicationDataManager.IsMobile);
		}
		EnableImageEffects();
	}

	public void EnableImageEffects()
	{
		MonoBehaviour[] array = simpleImageEffects;
		foreach (MonoBehaviour monoBehaviour in array)
		{
			monoBehaviour.enabled = ApplicationDataManager.IsMobile;
		}
		PostEffectsBase[] array2 = advancedImageEffects;
		foreach (PostEffectsBase postEffectsBase in array2)
		{
			postEffectsBase.enabled = !ApplicationDataManager.IsMobile && ApplicationDataManager.ApplicationOptions.VideoPostProcessing;
		}
	}

	public void DisableImageEffects()
	{
		MonoBehaviour[] array = simpleImageEffects;
		foreach (MonoBehaviour monoBehaviour in array)
		{
			monoBehaviour.enabled = false;
		}
		PostEffectsBase[] array2 = advancedImageEffects;
		foreach (PostEffectsBase postEffectsBase in array2)
		{
			postEffectsBase.enabled = false;
		}
	}

	private void Update()
	{
		if (LevelCamera.Instance != null)
		{
			if (GameState.HasCurrentPlayer && GameState.LocalCharacter.Is(PlayerStates.DIVING) && !Singleton<PlayerSpectatorControl>.Instance.IsEnabled)
			{
				lerpValue += Time.deltaTime * 4f;
				RenderSettings.fogMode = FogMode.Linear;
			}
			else
			{
				lerpValue -= Time.deltaTime * 4f;
				RenderSettings.fogMode = fogMode;
			}
			lerpValue = Mathf.Clamp01(lerpValue);
			RenderSettings.fogColor = Color.Lerp(fogColor, underwaterFogColor, lerpValue);
			RenderSettings.fogStartDistance = Mathf.Lerp(fogStart, 10f, lerpValue);
			RenderSettings.fogEndDistance = Mathf.Lerp(fogEnd, 50f, lerpValue);
		}
	}
}
