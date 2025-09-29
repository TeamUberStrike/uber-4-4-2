using System.Collections.Generic;
using UnityEngine;

public class AudioLoader : Singleton<AudioLoader>
{
	private Dictionary<string, AudioClip> cachedAudioClips;

	public IEnumerable<KeyValuePair<string, AudioClip>> AllClips
	{
		get
		{
			return cachedAudioClips;
		}
	}

	private AudioLoader()
	{
		cachedAudioClips = new Dictionary<string, AudioClip>();
	}

	private void CreateStreamedAudioClip(string name)
	{
		StreamedAudioClip streamedAudioClip = new StreamedAudioClip(name);
		cachedAudioClips[name] = streamedAudioClip.Clip;
	}

	public AudioClip Get(string name)
	{
		Debug.LogWarning("Skipping streaming of Ogg file. Not supported on mobile.");
		return null;
	}
}
