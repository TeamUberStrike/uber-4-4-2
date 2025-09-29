using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class PerformanceTester : MonoBehaviour
{
	private const float ITEM_HEIGHT = 30f;

	private Texture2D loadingBarBackground;

	private MapConfiguration mapConfig;

	[SerializeField]
	private MouseOrbit orbit;

	private string mapToLoad;

	private string fileToLoad;

	private int playersToSpawn;

	[SerializeField]
	private string[] gearToSpawnPerPlayer;

	[SerializeField]
	private string[] weaponsToSpawnPerPlayer;

	private Vector2 barSize = new Vector2(300f, 20f);

	private Vector2 scrollPosition = Vector2.zero;

	private bool ShowUI = true;

	private bool IsTesting;

	private bool FireWeapons = true;

	private Dictionary<string, BaseUnityItem> Prefabs = new Dictionary<string, BaseUnityItem>();

	private List<IUnityItem> spawnedItems;

	private int currentItem = -1;

	public static string ErrorMessage { get; private set; }

	public static bool IsError
	{
		get
		{
			return !string.IsNullOrEmpty(ErrorMessage);
		}
	}

	public static bool IsInitialised { get; private set; }

	public static bool IsAssetBundleLoaded { get; private set; }

	public static float ItemAssetBundleProgress { get; private set; }

	private void Awake()
	{
		loadingBarBackground = new Texture2D(1, 1, TextureFormat.RGB24, false);
		loadingBarBackground.SetPixels(new Color[1] { Color.white });
		loadingBarBackground.Apply(false);
		spawnedItems = new List<IUnityItem>();
		AutoMonoBehaviour<TouchInput>.Instance.enabled = true;
	}

	private IEnumerator Start()
	{
		IsInitialised = false;
		IsAssetBundleLoaded = false;
		yield return StartCoroutine(LoadConfig());
		if (IsError)
		{
			yield break;
		}
		IsInitialised = true;
		Debug.Log("starting asset bundle");
		yield return StartCoroutine(LoadMapConfig());
		if (!string.IsNullOrEmpty(mapToLoad) && !string.IsNullOrEmpty(fileToLoad))
		{
			yield return AssetBundleLoader.LoadMapAssetBundle(fileToLoad, null, delegate(AssetBundle bundle)
			{
				if (bundle != null)
				{
					Application.LoadLevelAdditive(mapToLoad);
				}
				else
				{
					Debug.LogError("Couldn't download map!");
				}
			});
			yield return new WaitForEndOfFrame();
			LevelCamera.Instance.gameObject.SetActive(false);
			GameObject levelCamera = GameObject.Find("MainCamera");
			UnityEngine.Object.Destroy(levelCamera);
			MapConfiguration config = UnityEngine.Object.FindObjectsOfType(typeof(MapConfiguration))[0] as MapConfiguration;
			if (config != null)
			{
				mapConfig = config;
			}
			AutoMonoBehaviour<TouchInput>.Instance.EnablePerformanceChecker();
		}
		IsAssetBundleLoaded = true;
	}

	private IEnumerator LoadMapConfig()
	{
		string path = ApplicationDataManager.BaseMapsURL + "Tester.xml";
		WWW www = new WWW(path);
		yield return www;
		Debug.Log("Downloading map config: " + path);
		try
		{
			using (XmlReader xmlReader = XmlReader.Create(new StringReader(www.text)))
			{
				while (xmlReader.Read())
				{
					if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "MapTester")
					{
						if (xmlReader.MoveToAttribute("Map"))
						{
							mapToLoad = xmlReader.Value;
						}
						if (xmlReader.MoveToAttribute("File"))
						{
							fileToLoad = xmlReader.Value;
						}
						if (xmlReader.MoveToAttribute("SpawnCount"))
						{
							playersToSpawn = int.Parse(xmlReader.Value);
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			ErrorMessage = "The Maps XML was malformed.\n" + ex2.Message;
			Debug.LogError(ErrorMessage);
		}
	}

	private IEnumerator LoadConfig()
	{
		string path = string.Empty;
		if (Application.isEditor)
		{
			path = "file://" + Application.dataPath + "/../EditorConfiguration.xml";
		}
		else
		{
			switch (Application.platform)
			{
			case RuntimePlatform.OSXWebPlayer:
			case RuntimePlatform.WindowsWebPlayer:
				path = Application.absoluteURL.Replace(".unity3d", ".xml") + "?arg=" + DateTime.Now.Ticks;
				break;
			case RuntimePlatform.WindowsPlayer:
				path = "file://" + Application.dataPath + "/UberStrike.xml";
				break;
			case RuntimePlatform.OSXPlayer:
				path = "file://" + Application.dataPath + "/Data/UberStrike.xml";
				break;
			case RuntimePlatform.Android:
				path = "jar:file://" + Application.dataPath + "!/assets/UberStrike.xml";
				break;
			case RuntimePlatform.IPhonePlayer:
				path = "file://" + Application.dataPath + "/Raw/UberStrike.xml";
				break;
			default:
				ErrorMessage = string.Concat("Cannot load Configuration Xml, ", Application.platform, " Platform is not supported.");
				Debug.LogError(ErrorMessage);
				yield break;
			}
		}
		Debug.Log(path);
		WWW www = new WWW(path);
		yield return www;
		ClientConfiguration config = new ClientConfiguration();
		try
		{
			using (XmlReader xmlReader = XmlReader.Create(new StringReader(www.text)))
			{
				while (xmlReader.Read())
				{
					if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "Application")
					{
						if (xmlReader.MoveToAttribute("DebugLevel"))
						{
							config.SetDebugLevel(xmlReader.Value);
						}
						if (xmlReader.MoveToAttribute("BuildType"))
						{
							config.SetBuildType(xmlReader.Value);
						}
						if (xmlReader.MoveToAttribute("WebServiceBaseUrl"))
						{
							config.WebServiceBaseUrl = xmlReader.Value;
						}
						if (xmlReader.MoveToAttribute("ContentBaseUrl"))
						{
							config.ContentBaseUrl = xmlReader.Value;
						}
						if (xmlReader.MoveToAttribute("ChannelType"))
						{
							config.SetChannelType(xmlReader.Value);
						}
					}
				}
			}
			if (!config.IsValid())
			{
				ErrorMessage = "Missing critical elements and/or attributes.";
				Debug.LogError(ErrorMessage);
				yield break;
			}
		}
		catch (Exception ex)
		{
			ErrorMessage = "The Configuration XML was malformed.\n" + ex.Message;
			Debug.LogError(ErrorMessage);
			yield break;
		}
		ApplicationDataManager.Config = config;
	}

	private void OnGUI()
	{
		if (!IsError)
		{
			if (!IsInitialised)
			{
				GUI.Label(new Rect(0f, Screen.height - 150, Screen.width, 50f), "Loading ...");
				return;
			}
			if (!IsAssetBundleLoaded)
			{
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
				GUI.DrawTexture(new Rect((float)Screen.width * 0.5f - barSize.x * 0.5f, (float)(Screen.height - 150) + barSize.y + 8f, barSize.x, 8f), loadingBarBackground);
				GUI.color = Color.white;
				GUI.DrawTexture(new Rect((float)Screen.width * 0.5f - barSize.x * 0.5f, (float)(Screen.height - 150) + barSize.y + 8f, Mathf.RoundToInt(ItemAssetBundleProgress * barSize.x), 8f), loadingBarBackground);
				return;
			}
			float num = Screen.width / 4;
			if (ShowUI && !IsTesting)
			{
				scrollPosition = GUITools.BeginScrollView(new Rect(num * 3f, 0f, num, Screen.height), scrollPosition, new Rect(0f, 0f, num, (float)Prefabs.Count * 30f));
				float num2 = 0f;
				foreach (string key in Prefabs.Keys)
				{
					if (GUI.Button(new Rect(0f, num2, num, 30f), new GUIContent(key)))
					{
						SpawnItem(key);
					}
					num2 += 30f;
				}
				GUI.EndScrollView();
				if (GUI.Button(new Rect(0f, Screen.height - 60, 60f, 60f), new GUIContent("<<")))
				{
					OrbitNextItem();
				}
				if (GUI.Button(new Rect(num * 3f - 60f, Screen.height - 60, 60f, 60f), new GUIContent(">>")))
				{
					OrbitPrevItem();
				}
				if (GUI.Button(new Rect(num * 2f + 5f, Screen.height - 60, 60f, 60f), new GUIContent("DEL")))
				{
					RemoveAllItems();
				}
				if (mapConfig != null && GUI.Button(new Rect(num * 3f - 130f, Screen.height - 60, 60f, 60f), new GUIContent("Test")))
				{
					StartCoroutine(DoTest());
				}
				string text = ((!FireWeapons) ? "Fire: OFF" : "Fire: ON");
				if (GUI.Button(new Rect(num * 2f - 150f, Screen.height - 60, 80f, 60f), new GUIContent(text)))
				{
					FireWeapons = !FireWeapons;
				}
			}
			if (IsTesting)
			{
				return;
			}
			if (GUI.Button(new Rect(num * 2f - 65f, Screen.height - 60, 60f, 60f), new GUIContent("UI")))
			{
				ShowUI = !ShowUI;
				if (ShowUI)
				{
					AutoMonoBehaviour<TouchInput>.Instance.EnablePerformanceChecker();
				}
				else
				{
					AutoMonoBehaviour<TouchInput>.Instance.DisablePerformanceChecker();
				}
			}
			string text2 = string.Format("Render time: {0}\nFPS: {1}\nObjects: {2}", Time.smoothDeltaTime, 1f / Time.smoothDeltaTime, spawnedItems.Count);
			GUI.Label(new Rect(0f, 0f, 300f, 300f), new GUIContent(text2));
		}
		else
		{
			GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "There was a problem loading " + ErrorMessage);
		}
	}

	private IEnumerator DoTest()
	{
		IsTesting = true;
		MouseOrbit.Disable = true;
		yield return new WaitForEndOfFrame();
		int totalFrames = 0;
		float totalTime = 0f;
		int totalPoints = 0;
		float minTime = 1E+09f;
		float maxTime = 0f;
		string maxPoint = string.Empty;
		string minPoint = string.Empty;
		SpawnPoint[] componentsInChildren = mapConfig.SpawnPoints.GetComponentsInChildren<SpawnPoint>();
		foreach (SpawnPoint obj in componentsInChildren)
		{
			base.transform.position = obj.transform.position;
			int thisFrame = Time.frameCount;
			float startTime = Time.time;
			while (Time.frameCount < thisFrame + 100)
			{
				yield return new WaitForEndOfFrame();
				totalFrames++;
			}
			float localTime = Time.time - startTime;
			Debug.Log("Spawn point: " + obj.name + " took " + localTime + " to render 100 frames. FPS: " + 1f / (localTime / 100f));
			totalTime += localTime;
			totalPoints++;
			if (localTime < minTime)
			{
				minTime = localTime;
				minPoint = obj.name;
			}
			if (localTime > maxTime)
			{
				maxTime = localTime;
				maxPoint = obj.name;
			}
		}
		float avgTime = totalTime / (float)totalPoints;
		Debug.Log("Testing complete. Avg: " + avgTime + " FPS: " + 1f / (avgTime / 100f) + ". Checked " + totalPoints + " points.");
		Debug.Log("Fastest point: " + minPoint + " with " + minTime + ". Slowest point: " + maxPoint + " with " + maxTime);
		IsTesting = false;
		MouseOrbit.Disable = false;
	}

	private void SpawnItem(string prefabName)
	{
	}

	private void OrbitNextItem()
	{
		if (currentItem + 1 < spawnedItems.Count)
		{
			currentItem++;
		}
		else
		{
			currentItem = 0;
		}
		orbit.enabled = true;
	}

	private void OrbitPrevItem()
	{
		if (currentItem - 1 >= 0)
		{
			currentItem--;
		}
		else
		{
			currentItem = spawnedItems.Count - 1;
		}
		orbit.enabled = true;
	}

	private void RemoveAllItems()
	{
		currentItem = -1;
		spawnedItems.Clear();
		GC.Collect();
	}
}
