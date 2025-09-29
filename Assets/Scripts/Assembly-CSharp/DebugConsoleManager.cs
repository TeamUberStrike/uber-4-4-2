using System;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using SharpBrake;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class DebugConsoleManager : MonoBehaviour
{
	private Vector2 _scrollDebug;

	private List<string> _exceptions = new List<string>(10);

	private static IDebugPage[] _debugPages = new IDebugPage[0];

	private static string[] _debugPageDescriptors = new string[0];

	private static int _currentPageSelectedIdx = 0;

	private static IDebugPage _currentPageSelected;

	[SerializeField]
	private bool _isDebugConsoleEnabled;

	public static DebugConsoleManager Instance { get; private set; }

	public bool IsDebugConsoleEnabled
	{
		get
		{
			return _isDebugConsoleEnabled;
		}
		set
		{
			_isDebugConsoleEnabled = value;
		}
	}

	private void Awake()
	{
		Instance = this;
		if (Application.isEditor)
		{
			UpdatePages(MemberAccessLevel.Admin);
			return;
		}
		CmuneEventHandler.AddListener(delegate(LoginEvent ev)
		{
			UpdatePages(ev.AccessLevel);
		});
	}

	private void Start()
	{
		Application.RegisterLogCallback(OnUnityDebugCallback);
	}

	private void Update()
	{
		if (KeyInput.AltPressed && KeyInput.CtrlPressed && KeyInput.GetKeyDown(KeyCode.D))
		{
			_isDebugConsoleEnabled = !_isDebugConsoleEnabled;
		}
	}

	private void OnGUI()
	{
		if (ApplicationDataManager.BuildType != BuildType.Prod)
		{
			for (int i = 0; i < _exceptions.Count; i++)
			{
				GUI.Label(new Rect(0f, GUITools.ScreenHeight - 40 - i * 25, GUITools.ScreenWidth, 20f), _exceptions[i]);
			}
		}
		if (_isDebugConsoleEnabled && _debugPageDescriptors.Length > 0)
		{
			GUI.skin = BlueStonez.Skin;
			Rect screenRect = new Rect(20f, (float)Screen.height * 0.2f, Screen.width - 40, (float)Screen.height * 0.8f - 20f);
			GUILayout.BeginArea(screenRect, BlueStonez.window);
			GUI.Label(new Rect(0f, 0f, screenRect.width, 23f), "Debug Console", BlueStonez.tab_strip);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Close", BlueStonez.buttondark_small, GUILayout.Height(20f), GUILayout.Width(64f)))
			{
				_isDebugConsoleEnabled = false;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(19f);
			DrawDebugMenuGrid();
			GUILayout.Space(2f);
			DrawDebugPage();
			GUILayout.EndArea();
		}
	}

	private void DrawDebugMenuGrid()
	{
		int num = GUILayout.SelectionGrid(_currentPageSelectedIdx, _debugPageDescriptors, _debugPageDescriptors.Length, BlueStonez.tab_medium);
		if (num != _currentPageSelectedIdx)
		{
			num = (_currentPageSelectedIdx = Mathf.Clamp(num, 0, _debugPages.Length - 1));
			_currentPageSelected = _debugPages[num];
		}
	}

	private void DrawDebugPage()
	{
		_scrollDebug = GUILayout.BeginScrollView(_scrollDebug);
		if (_currentPageSelected != null)
		{
			_currentPageSelected.Draw();
		}
		GUILayout.EndScrollView();
	}

	private void UpdatePages(MemberAccessLevel level)
	{
		if (level >= MemberAccessLevel.QA)
		{
			_debugPages = new IDebugPage[3]
			{
				new DebugLogMessages(),
				new DebugApplication(),
				new DebugGameState()
			};
		}
		else
		{
			_debugPages = new IDebugPage[2]
			{
				new DebugLogMessages(),
				new DebugApplication()
			};
		}
		_debugPageDescriptors = new string[_debugPages.Length];
		for (int i = 0; i < _debugPages.Length; i++)
		{
			_debugPageDescriptors[i] = _debugPages[i].Title;
		}
		_currentPageSelectedIdx = 0;
		_currentPageSelected = _debugPages[0];
	}

	private void OnUnityDebugCallback(string logString, string stackTrace, LogType logType)
	{
		switch (logType)
		{
		case LogType.Log:
			if (ApplicationDataManager.Config.DebugLevel >= DebugLevel.Debug)
			{
				DebugLogMessages.Log(0, logString);
			}
			break;
		case LogType.Warning:
			if (ApplicationDataManager.Config.DebugLevel >= DebugLevel.Warning)
			{
				DebugLogMessages.Log(1, logString);
			}
			break;
		case LogType.Error:
			if (ApplicationDataManager.Config.DebugLevel >= DebugLevel.Error)
			{
				DebugLogMessages.Log(2, logString);
			}
			break;
		case LogType.Assert:
			DebugLogMessages.Log(2, logString);
			break;
		case LogType.Exception:
		{
			if (logString.Contains("Could not resolve host") || logString.Contains("Failed downloading http://"))
			{
				DebugLogMessages.Console.Log(2, logString + "\n Info: It is likely you have lost connection to the internet, or the url is unreachable.");
				break;
			}
			Exception ex = new Exception(logString);
			ex.Data.Add("StackTrace", stackTrace);
			SendExceptionReport(ex);
			if (_exceptions.Count < 10)
			{
				_exceptions.Add(logString + " " + stackTrace);
			}
			break;
		}
		}
	}

	public static void SendExceptionReport(Exception ex, string popupMessage = null)
	{
		Debug.LogError("Exception occured: " + ex);
		SendException(ex);
		if (!string.IsNullOrEmpty(popupMessage))
		{
			PopupSystem.ShowError("Error", popupMessage, PopupSystem.AlertType.OK);
		}
	}

	private static void SendException(Exception ex)
	{
		AirbrakeConfiguration airbrakeConfiguration = new AirbrakeConfiguration();
		airbrakeConfiguration.ApiKey = "fc6c44131405da1e674fa141380a130a";
		airbrakeConfiguration.EnvironmentName = "production";
		airbrakeConfiguration.ServerUri = "https://api.airbrake.io/notifier_api/v2/notices";
		AirbrakeConfiguration configuration = airbrakeConfiguration;
		AirbrakeClient airbrakeClient = new AirbrakeClient(configuration);
		MonoRoutine.Start(airbrakeClient.Send(ex));
	}
}
