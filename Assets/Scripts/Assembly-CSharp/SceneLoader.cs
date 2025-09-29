using System;
using System.Collections;
using UnityEngine;

public class SceneLoader : Singleton<SceneLoader>
{
	private const float FadeTime = 1f;

	private const float MinLoadingTime = 1f;

	private Texture2D _blackTexture;

	private Color _color;

	public string CurrentScene { get; private set; }

	public bool IsLoading { get; private set; }

	private SceneLoader()
	{
		_blackTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
	}

	public Coroutine LoadLevel(string level, Action<string> onError = null, Action<float> progress = null)
	{
		if (GameState.LocalAvatar != null && GameState.LocalAvatar.Decorator != null)
		{
			GameState.LocalAvatar.Decorator.transform.parent = null;
		}
		return MonoRoutine.Start(LoadLevelAsync(level, onError, progress));
	}

	private IEnumerator LoadLevelAsync(string level, Action<string> onError = null, Action<float> progress = null)
	{
		IsLoading = true;
		CurrentScene = level;
		AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Stop();
		AutoMonoBehaviour<UnityRuntime>.Instance.OnGui += OnGUI;
		for (float f = _color.a * 1f; f < 1f; f += Time.deltaTime)
		{
			yield return new WaitForEndOfFrame();
			_color.a = f / 1f;
		}
		_color.a = 1f;
		float startTime = Time.realtimeSinceStartup;
		AsyncOperation async = Application.LoadLevelAsync(level);
		if (async != null)
		{
			while (!async.isDone)
			{
				if (progress != null)
				{
					progress(async.progress);
				}
				yield return new WaitForEndOfFrame();
			}
		}
		else
		{
			Debug.LogError("Loading scene with name '" + level + "' failed.");
		}
		if (onError != null && Application.loadedLevelName != level)
		{
			onError("There is no scene with name: " + level);
		}
		else
		{
			yield return Resources.UnloadUnusedAssets();
			if (progress != null)
			{
				progress(1f);
			}
			yield return new WaitForSeconds(Math.Max(startTime + 1f - Time.realtimeSinceStartup, 0f));
			if (level == "Menu")
			{
				AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Play(GameAudio.SeletronRadioShort);
			}
			for (float f2 = 0f; f2 < 1f; f2 += Time.deltaTime)
			{
				yield return new WaitForEndOfFrame();
				_color.a = 1f - f2 / 1f;
			}
		}
		AutoMonoBehaviour<UnityRuntime>.Instance.OnGui -= OnGUI;
		IsLoading = false;
	}

	private void OnGUI()
	{
		GUI.depth = 8;
		GUI.color = _color;
		GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _blackTexture);
		GUI.color = Color.white;
	}
}
