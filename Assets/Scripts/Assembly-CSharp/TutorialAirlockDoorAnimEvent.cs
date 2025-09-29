using System.Collections.Generic;
using UnityEngine;

public class TutorialAirlockDoorAnimEvent : MonoBehaviour
{
	private Dictionary<string, AudioSource> _doorAudioSources;

	private Dictionary<string, float> _doorLockTiming;

	private void Awake()
	{
		_doorLockTiming = new Dictionary<string, float>();
		_doorAudioSources = new Dictionary<string, AudioSource>();
		_doorLockTiming.Add("AirlockDoor", 0f);
		_doorLockTiming.Add("Gear4", 0.5f);
		_doorLockTiming.Add("Gear3", 2f / 3f);
		_doorLockTiming.Add("Gear2", 5f / 6f);
		_doorLockTiming.Add("Gear1", 1f);
		_doorLockTiming.Add("Gear10", 1.1666666f);
		_doorLockTiming.Add("Gear9", 1.3333334f);
		_doorLockTiming.Add("Gear8", 1.5f);
		AudioSource[] componentsInChildren = GetComponentsInChildren<AudioSource>(true);
		AudioSource[] array = componentsInChildren;
		foreach (AudioSource audioSource in array)
		{
			AnimationEvent animationEvent = new AnimationEvent();
			animationEvent.functionName = "OnDoorUnlock";
			animationEvent.stringParameter = audioSource.gameObject.name;
			float value;
			if (_doorLockTiming.TryGetValue(animationEvent.stringParameter, out value))
			{
				animationEvent.time = value;
				base.animation.clip.AddEvent(animationEvent);
				_doorAudioSources.Add(animationEvent.stringParameter, audioSource);
			}
			else
			{
				Debug.LogError("Failed to get door lock: " + animationEvent.stringParameter);
			}
		}
	}

	private void OnDoorUnlock(string lockName)
	{
		AudioSource value;
		if (_doorAudioSources.TryGetValue(lockName, out value))
		{
			value.Play();
			_doorAudioSources.Remove(lockName);
		}
	}
}
