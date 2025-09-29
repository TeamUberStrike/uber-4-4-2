using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxManager : AutoMonoBehaviour<SfxManager>
{
	[Serializable]
	public class SoundValuePair
	{
		[SerializeField]
		private string _name;

		public AudioClip ID;

		public AudioClip Audio;

		public SoundValuePair(AudioClip id, AudioClip clip)
		{
			_name = id.ToString();
			ID = id;
			Audio = clip;
		}
	}

	private AudioSource uiAudioSource;

	private AudioSource musicAudioSource;

	private AudioClip _lastFootStep;

	private AudioClip[] _footStepDirt;

	private AudioClip[] _footStepGrass;

	private AudioClip[] _footStepMetal;

	private AudioClip[] _footStepHeavyMetal;

	private AudioClip[] _footStepRock;

	private AudioClip[] _footStepSand;

	private AudioClip[] _footStepWater;

	private AudioClip[] _footStepWood;

	private AudioClip[] _swimAboveWater;

	private AudioClip[] _swimUnderWater;

	private AudioClip[] _footStepSnow;

	private AudioClip[] _footStepGlass;

	private Dictionary<string, AudioClip[]> _surfaceImpactSoundMap;

	public static float EffectsAudioVolume
	{
		get
		{
			return ApplicationDataManager.ApplicationOptions.AudioEffectsVolume;
		}
	}

	public static float MusicAudioVolume
	{
		get
		{
			return ApplicationDataManager.ApplicationOptions.AudioMusicVolume;
		}
	}

	public static float MasterAudioVolume
	{
		get
		{
			return ApplicationDataManager.ApplicationOptions.AudioMasterVolume;
		}
	}

	private void Awake()
	{
		uiAudioSource = base.gameObject.AddComponent<AudioSource>();
		uiAudioSource.playOnAwake = false;
		uiAudioSource.rolloffMode = AudioRolloffMode.Linear;
		musicAudioSource = base.gameObject.AddComponent<AudioSource>();
		musicAudioSource.loop = true;
		musicAudioSource.playOnAwake = false;
		_footStepDirt = new AudioClip[4]
		{
			GameAudio.FootStepDirt1,
			GameAudio.FootStepDirt2,
			GameAudio.FootStepDirt3,
			GameAudio.FootStepDirt4
		};
		_footStepGrass = new AudioClip[4]
		{
			GameAudio.FootStepGrass1,
			GameAudio.FootStepGrass2,
			GameAudio.FootStepGrass3,
			GameAudio.FootStepGrass4
		};
		_footStepMetal = new AudioClip[4]
		{
			GameAudio.FootStepMetal1,
			GameAudio.FootStepMetal2,
			GameAudio.FootStepMetal3,
			GameAudio.FootStepMetal4
		};
		_footStepHeavyMetal = new AudioClip[4]
		{
			GameAudio.FootStepHeavyMetal1,
			GameAudio.FootStepHeavyMetal2,
			GameAudio.FootStepHeavyMetal3,
			GameAudio.FootStepHeavyMetal4
		};
		_footStepRock = new AudioClip[4]
		{
			GameAudio.FootStepRock1,
			GameAudio.FootStepRock2,
			GameAudio.FootStepRock3,
			GameAudio.FootStepRock4
		};
		_footStepSand = new AudioClip[4]
		{
			GameAudio.FootStepSand1,
			GameAudio.FootStepSand2,
			GameAudio.FootStepSand3,
			GameAudio.FootStepSand4
		};
		_footStepWater = new AudioClip[3]
		{
			GameAudio.FootStepWater1,
			GameAudio.FootStepWater2,
			GameAudio.FootStepWater3
		};
		_footStepWood = new AudioClip[4]
		{
			GameAudio.FootStepWood1,
			GameAudio.FootStepWood2,
			GameAudio.FootStepWood3,
			GameAudio.FootStepWood4
		};
		_swimAboveWater = new AudioClip[4]
		{
			GameAudio.SwimAboveWater1,
			GameAudio.SwimAboveWater2,
			GameAudio.SwimAboveWater3,
			GameAudio.SwimAboveWater4
		};
		_swimUnderWater = new AudioClip[1] { GameAudio.SwimUnderWater };
		_footStepSnow = new AudioClip[4]
		{
			GameAudio.FootStepSnow1,
			GameAudio.FootStepSnow2,
			GameAudio.FootStepSnow3,
			GameAudio.FootStepSnow4
		};
		_footStepGlass = new AudioClip[4]
		{
			GameAudio.FootStepGlass1,
			GameAudio.FootStepGlass2,
			GameAudio.FootStepGlass3,
			GameAudio.FootStepGlass4
		};
		AudioClip[] value = new AudioClip[4]
		{
			GameAudio.ImpactCement1,
			GameAudio.ImpactCement2,
			GameAudio.ImpactCement3,
			GameAudio.ImpactCement4
		};
		AudioClip[] value2 = new AudioClip[5]
		{
			GameAudio.ImpactGlass1,
			GameAudio.ImpactGlass2,
			GameAudio.ImpactGlass3,
			GameAudio.ImpactGlass4,
			GameAudio.ImpactGlass5
		};
		AudioClip[] value3 = new AudioClip[4]
		{
			GameAudio.ImpactGrass1,
			GameAudio.ImpactGrass2,
			GameAudio.ImpactGrass3,
			GameAudio.ImpactGrass4
		};
		AudioClip[] value4 = new AudioClip[5]
		{
			GameAudio.ImpactMetal1,
			GameAudio.ImpactMetal2,
			GameAudio.ImpactMetal3,
			GameAudio.ImpactMetal4,
			GameAudio.ImpactMetal5
		};
		AudioClip[] value5 = new AudioClip[5]
		{
			GameAudio.ImpactSand1,
			GameAudio.ImpactSand2,
			GameAudio.ImpactSand3,
			GameAudio.ImpactSand4,
			GameAudio.ImpactSand5
		};
		AudioClip[] value6 = new AudioClip[5]
		{
			GameAudio.ImpactStone1,
			GameAudio.ImpactStone2,
			GameAudio.ImpactStone3,
			GameAudio.ImpactStone4,
			GameAudio.ImpactStone5
		};
		AudioClip[] value7 = new AudioClip[5]
		{
			GameAudio.ImpactWater1,
			GameAudio.ImpactWater2,
			GameAudio.ImpactWater3,
			GameAudio.ImpactWater4,
			GameAudio.ImpactWater5
		};
		AudioClip[] value8 = new AudioClip[5]
		{
			GameAudio.ImpactWood1,
			GameAudio.ImpactWood2,
			GameAudio.ImpactWood3,
			GameAudio.ImpactWood4,
			GameAudio.ImpactWood5
		};
		_surfaceImpactSoundMap = new Dictionary<string, AudioClip[]>
		{
			{ "Wood", value8 },
			{ "SolidWood", value8 },
			{ "Stone", value6 },
			{ "Metal", value4 },
			{ "Sand", value5 },
			{ "Grass", value3 },
			{ "Glass", value2 },
			{ "Cement", value },
			{ "Water", value7 }
		};
	}

	public static void StopAll2dAudio()
	{
		AutoMonoBehaviour<SfxManager>.Instance.uiAudioSource.Stop();
	}

	public static void Play2dAudioClip(AudioClip soundEffect, float delay)
	{
		MonoRoutine.Start(Play2dAudioClipInSeconds(soundEffect, delay));
	}

	private static IEnumerator Play2dAudioClipInSeconds(AudioClip soundEffect, float delay)
	{
		yield return new WaitForSeconds(delay);
		Play2dAudioClip(soundEffect);
	}

	public static void Play2dAudioClip(AudioClip audioClip)
	{
		try
		{
			AutoMonoBehaviour<SfxManager>.Instance.uiAudioSource.PlayOneShot(audioClip);
		}
		catch
		{
			Debug.LogError("Play2dAudioClip: failed.");
		}
	}

	public static void Play3dAudioClip(AudioClip audioClip, Vector3 position)
	{
		try
		{
			AudioSource.PlayClipAtPoint(audioClip, position, AutoMonoBehaviour<SfxManager>.Instance.uiAudioSource.volume);
		}
		catch
		{
			Debug.LogError("Play3dAudioClip: failed.");
		}
	}

	public static void Play3dAudioClip(AudioClip soundEffect, float volume, float minDistance, float maxDistance, AudioRolloffMode rolloffMode, Vector3 position)
	{
		if (minDistance <= 0f)
		{
			return;
		}
		GameObject gameObject = new GameObject("One Shot Audio", typeof(AudioSource));
		float t = 0f;
		try
		{
			gameObject.transform.position = position;
			gameObject.audio.clip = soundEffect;
			t = gameObject.audio.clip.length;
			gameObject.audio.volume = volume;
			gameObject.audio.rolloffMode = rolloffMode;
			gameObject.audio.minDistance = minDistance;
			gameObject.audio.maxDistance = maxDistance;
			gameObject.audio.Play();
		}
		catch
		{
			Debug.LogError(string.Concat("Play3dAudioClip: ", soundEffect, " failed."));
		}
		finally
		{
			UnityEngine.Object.Destroy(gameObject, t);
		}
	}

	public void PlayMusicMobile(AudioClip clip, float volume)
	{
		if (clip != null)
		{
			musicAudioSource.volume = MusicAudioVolume * volume;
			musicAudioSource.clip = clip;
			musicAudioSource.loop = true;
			musicAudioSource.Play();
		}
	}

	public void PlayFootStepAudioClip(FootStepSoundType footStep, Vector3 position)
	{
		AudioClip[] array = null;
		switch (footStep)
		{
		case FootStepSoundType.Dirt:
			array = _footStepDirt;
			break;
		case FootStepSoundType.Grass:
			array = _footStepGrass;
			break;
		case FootStepSoundType.Metal:
			array = _footStepMetal;
			break;
		case FootStepSoundType.HeavyMetal:
			array = _footStepHeavyMetal;
			break;
		case FootStepSoundType.Rock:
			array = _footStepRock;
			break;
		case FootStepSoundType.Sand:
			array = _footStepSand;
			break;
		case FootStepSoundType.Water:
			array = _footStepWater;
			break;
		case FootStepSoundType.Wood:
			array = _footStepWood;
			break;
		case FootStepSoundType.Swim:
			array = _swimAboveWater;
			break;
		case FootStepSoundType.Dive:
			array = _swimUnderWater;
			break;
		case FootStepSoundType.Snow:
			array = _footStepSnow;
			break;
		case FootStepSoundType.Glass:
			array = _footStepGlass;
			break;
		}
		if (array != null && array.Length > 0)
		{
			AudioClip audioClip = null;
			if (array.Length > 1)
			{
				do
				{
					audioClip = array[UnityEngine.Random.Range(0, array.Length)];
				}
				while (audioClip == _lastFootStep);
			}
			else if (array.Length > 0)
			{
				audioClip = array[0];
			}
			if (audioClip != null)
			{
				_lastFootStep = audioClip;
				Play3dAudioClip(audioClip, position);
			}
		}
		else
		{
			Debug.LogWarning("FootStep type not supported: " + footStep);
		}
	}

	public void PlayImpactSound(string surfaceType, Vector3 position)
	{
		AudioClip[] value = null;
		if (_surfaceImpactSoundMap.TryGetValue(surfaceType, out value))
		{
			Play3dAudioClip(value[UnityEngine.Random.Range(0, value.Length)], position);
		}
	}

	public void EnableAudio(bool enabled)
	{
		AudioListener.volume = ((!enabled) ? 0f : ApplicationDataManager.ApplicationOptions.AudioMasterVolume);
	}

	public void UpdateMasterVolume()
	{
		if (ApplicationDataManager.ApplicationOptions.AudioEnabled)
		{
			AudioListener.volume = ApplicationDataManager.ApplicationOptions.AudioMasterVolume;
		}
	}

	public void UpdateMusicVolume()
	{
		if (ApplicationDataManager.ApplicationOptions.AudioEnabled)
		{
			AutoMonoBehaviour<SfxManager>.Instance.musicAudioSource.volume = ApplicationDataManager.ApplicationOptions.AudioMusicVolume;
			AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Volume = ApplicationDataManager.ApplicationOptions.AudioMusicVolume;
		}
	}

	public void UpdateEffectsVolume()
	{
		if (ApplicationDataManager.ApplicationOptions.AudioEnabled)
		{
			AutoMonoBehaviour<SfxManager>.Instance.uiAudioSource.volume = ApplicationDataManager.ApplicationOptions.AudioEffectsVolume;
		}
		else
		{
			AutoMonoBehaviour<SfxManager>.Instance.uiAudioSource.volume = 0f;
		}
	}

	public float GetSoundLength(AudioClip AudioClip)
	{
		try
		{
			return AudioClip.length;
		}
		catch
		{
			Debug.LogError("GetSoundLength: Failed to get AudioClip");
			return 0f;
		}
	}
}
