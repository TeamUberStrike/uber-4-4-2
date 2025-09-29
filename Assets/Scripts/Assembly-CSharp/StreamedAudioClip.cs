using System.Collections;
using UnityEngine;

public class StreamedAudioClip
{
	private AudioClip _clip;

	private AudioClip _streamedAudioClip;

	private int _lastPosition;

	private bool _downloadingFinished;

	private float[] _samples;

	public AudioClip Clip
	{
		get
		{
			return _clip;
		}
	}

	public bool IsDownloaded
	{
		get
		{
			return _downloadingFinished;
		}
	}

	public StreamedAudioClip(string name)
	{
		WWW wWW = new WWW(ApplicationDataManager.BaseAudioURL + name);
		MonoRoutine.Start(StartDownloadingAudioClip(wWW));
		_streamedAudioClip = wWW.GetAudioClip(false, true, AudioType.OGGVORBIS);
		_samples = new float[_streamedAudioClip.samples * _streamedAudioClip.channels];
		_clip = AudioClip.Create(name, _streamedAudioClip.samples, _streamedAudioClip.channels, _streamedAudioClip.frequency, false, true, OnPCMRead, OnPCMSetPosition);
	}

	public StreamedAudioClip(string name, AudioClip source)
	{
		_samples = new float[source.samples * source.channels];
		source.GetData(_samples, 0);
		_clip = AudioClip.Create(name, source.samples, source.channels, source.frequency, false, true, OnPCMRead, OnPCMSetPosition);
		_downloadingFinished = true;
	}

	private void OnPCMRead(float[] data)
	{
		int num = 0;
		while (_downloadingFinished && num < data.Length)
		{
			int num2 = _lastPosition + num;
			if (num2 < _samples.Length)
			{
				data[num] = _samples[num2];
			}
			else
			{
				data[num] = 0f;
			}
			num++;
		}
		_lastPosition += data.Length;
	}

	private void OnPCMSetPosition(int position)
	{
		_lastPosition = position;
	}

	private IEnumerator StartDownloadingAudioClip(WWW www)
	{
		while (!www.isDone)
		{
			yield return new WaitForEndOfFrame();
		}
		if (!string.IsNullOrEmpty(www.error))
		{
			Debug.LogError(www.error + "\n" + www.url);
		}
		if (_samples != null)
		{
			AudioClip clip = www.GetAudioClip(false, false, AudioType.OGGVORBIS);
			if (!clip)
			{
				Debug.LogError("Failed to GetAudioClip from WWW");
			}
		}
		else
		{
			Debug.LogError("Failed to GetData from " + _streamedAudioClip.name);
		}
	}
}
