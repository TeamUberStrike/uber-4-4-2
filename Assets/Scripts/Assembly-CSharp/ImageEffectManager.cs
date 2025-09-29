using System.Collections.Generic;
using UnityEngine;

public class ImageEffectManager
{
	public enum ImageEffectType
	{
		None = 0,
		ColorCorrectionCurves = 1,
		BloomAndLensFlares = 2,
		MotionBlur = 3
	}

	private class ImageEffectParameters
	{
		private bool _permanentEnable;

		private bool _timedEnable;

		private float _baseIntencity;

		private float _totalTime;

		private float _activeTime;

		public bool PermanentEnable
		{
			get
			{
				return _permanentEnable;
			}
		}

		public bool TimedEnable
		{
			get
			{
				return _timedEnable;
			}
		}

		public bool EffectEnable
		{
			get
			{
				return _permanentEnable || _timedEnable;
			}
		}

		public float BaseIntencity
		{
			get
			{
				return _baseIntencity;
			}
		}

		public float TotalTime
		{
			get
			{
				return _totalTime;
			}
		}

		public float ActiveTime
		{
			get
			{
				return _activeTime;
			}
		}

		public void SetPermanentEnable(bool value)
		{
			_permanentEnable = value;
		}

		public void SetTimedEnable(bool value)
		{
			_timedEnable = value;
		}

		public void SetBaseIntensity(float value)
		{
			_baseIntencity = value;
		}

		public void SetTotalTime(float value)
		{
			_totalTime = value;
		}

		public void SetActiveTime(float value)
		{
			_activeTime = value;
		}

		public void SetTotalAndActiveTime(float time)
		{
			SetActiveTime(time);
			SetTotalTime(time);
		}

		public void ChangeActiveTime(float change)
		{
			_activeTime += change;
		}
	}

	private const float _motionBlurMaxValue = 0.5f;

	private ImageEffectType _currentEffect;

	private Dictionary<ImageEffectType, MonoBehaviour> _effects = new Dictionary<ImageEffectType, MonoBehaviour>();

	private Dictionary<ImageEffectType, ImageEffectParameters> _effectsParameters = new Dictionary<ImageEffectType, ImageEffectParameters>();

	public ImageEffectType CurrentEffect
	{
		get
		{
			return _currentEffect;
		}
	}

	public void ApplyMotionBlur(float time)
	{
		if (_effects.ContainsKey(ImageEffectType.MotionBlur))
		{
			EnableEffect(ImageEffectType.MotionBlur, time);
		}
	}

	public void ApplyMotionBlur(float time, float intensity)
	{
		if (_effects.ContainsKey(ImageEffectType.MotionBlur))
		{
			EnableEffect(ImageEffectType.MotionBlur, time, intensity);
		}
	}

	public void ApplyWhiteout(float time)
	{
		if (_effects.ContainsKey(ImageEffectType.BloomAndLensFlares))
		{
			EnableEffect(ImageEffectType.BloomAndLensFlares, time);
		}
	}

	public void AddEffect(ImageEffectType imageEffectType, MonoBehaviour monoBehaviour)
	{
		_effects[imageEffectType] = monoBehaviour;
		_effectsParameters[imageEffectType] = new ImageEffectParameters();
	}

	public void Clear()
	{
		_effects.Clear();
	}

	public void Update()
	{
		if (ApplicationDataManager.ApplicationOptions.VideoMotionBlur)
		{
			ImageEffectParameters value;
			if (_effectsParameters.TryGetValue(ImageEffectType.MotionBlur, out value) && value != null && value.EffectEnable)
			{
				if (value.ActiveTime > 0f)
				{
					value.ChangeActiveTime(0f - Time.deltaTime);
					if (value.ActiveTime < 0f)
					{
						value.SetTimedEnable(false);
					}
				}
				if (value.PermanentEnable)
				{
					((MotionBlur)_effects[ImageEffectType.MotionBlur]).blurAmount = 0.5f;
				}
				else if (value.TimedEnable)
				{
					float baseIntencity = _effectsParameters[ImageEffectType.MotionBlur].BaseIntencity;
					baseIntencity = ((!(baseIntencity > 0f)) ? 0.5f : baseIntencity);
					((MotionBlur)_effects[ImageEffectType.MotionBlur]).blurAmount = _effectsParameters[ImageEffectType.MotionBlur].ActiveTime / _effectsParameters[ImageEffectType.MotionBlur].TotalTime * baseIntencity;
				}
			}
		}
		else if (_effects.ContainsKey(ImageEffectType.MotionBlur))
		{
			_effects[ImageEffectType.MotionBlur].enabled = false;
		}
		if (_effects.ContainsKey(ImageEffectType.ColorCorrectionCurves))
		{
			_effects[ImageEffectType.ColorCorrectionCurves].enabled = ApplicationDataManager.ApplicationOptions.VideoVignetting;
		}
		if (_effects.ContainsKey(ImageEffectType.BloomAndLensFlares))
		{
			_effects[ImageEffectType.BloomAndLensFlares].enabled = ApplicationDataManager.ApplicationOptions.VideoBloomAndFlares;
		}
	}

	public void EnableEffect(ImageEffectType imageEffectType)
	{
		EnableEffect(imageEffectType, -1f, -1f);
	}

	public void EnableEffect(ImageEffectType imageEffectType, float time)
	{
		EnableEffect(imageEffectType, time, -1f);
	}

	public void EnableEffect(ImageEffectType imageEffectType, float duration, float intensity)
	{
		if (_effects.ContainsKey(imageEffectType) && _effectsParameters.ContainsKey(imageEffectType))
		{
			_effects[imageEffectType].enabled = true;
			if (imageEffectType == ImageEffectType.BloomAndLensFlares)
			{
				_effectsParameters[imageEffectType].SetBaseIntensity(((BloomAndLensFlares)_effects[ImageEffectType.BloomAndLensFlares]).bloomIntensity);
			}
			if (intensity > 0f)
			{
				_effectsParameters[imageEffectType].SetBaseIntensity(intensity);
			}
			if (duration > 0f)
			{
				_effectsParameters[imageEffectType].SetTotalAndActiveTime(duration);
				_effectsParameters[imageEffectType].SetTimedEnable(true);
			}
			else
			{
				_effectsParameters[imageEffectType].SetPermanentEnable(true);
			}
			_currentEffect = imageEffectType;
		}
		else
		{
			Debug.LogError("You're trying to enable an effect that hasn't been initialized. Check the components on MainCamera in the level.");
		}
	}

	public void DisableEffect(ImageEffectType imageEffectType)
	{
		if (_effects.ContainsKey(imageEffectType) && _effectsParameters.ContainsKey(imageEffectType))
		{
			_effects[imageEffectType].enabled = false;
			_effectsParameters[imageEffectType].SetPermanentEnable(false);
			_currentEffect = ImageEffectType.None;
		}
	}

	public void DisableEffectInstant(ImageEffectType imageEffectType)
	{
		if (_effects.ContainsKey(imageEffectType) && _effectsParameters.ContainsKey(imageEffectType))
		{
			_effects[imageEffectType].enabled = false;
			_effectsParameters[imageEffectType].SetPermanentEnable(false);
			_effectsParameters[imageEffectType].SetTimedEnable(false);
			_currentEffect = ImageEffectType.None;
		}
	}

	public void DisableAllEffects()
	{
		foreach (ImageEffectType key in _effectsParameters.Keys)
		{
			DisableEffectInstant(key);
		}
	}
}
