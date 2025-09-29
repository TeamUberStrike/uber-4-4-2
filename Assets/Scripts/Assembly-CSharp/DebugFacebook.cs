using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class DebugFacebook : IDebugPage
{
	private Vector2 scroll;

	private string jsCommand = string.Empty;

	private string jsCommandParam1 = string.Empty;

	private string jsCommandParam2 = string.Empty;

	private string jsCommandLog = "Js Command Log.\n";

	private string button1Param = string.Empty;

	private string button2Param = string.Empty;

	public string Title
	{
		get
		{
			return "Facebook";
		}
	}

	public void Draw()
	{
		scroll = GUILayout.BeginScrollView(scroll);
		GUILayout.BeginHorizontal();
		FBTestPanel();
		JSTestConsole();
		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
	}

	private void FBTestPanel()
	{
		GUILayout.BeginVertical(GUILayout.MinWidth(256f));
		GUILayout.EndVertical();
	}

	private void StartTask(LotteryPopupDialog dialog)
	{
		new LotteryPopupTask(dialog);
		PopupSystem.Show(dialog);
	}

	private void JSTestConsole()
	{
		GUILayout.BeginVertical(GUILayout.MinWidth(256f));
		GUILayout.Label("JavaScript Test Console");
		GUILayout.Space(4f);
		jsCommand = GUILayoutLabelTextField("Function", jsCommand);
		GUILayout.Space(4f);
		jsCommandParam1 = GUILayoutLabelTextField("Param 1", jsCommandParam1);
		GUILayout.Space(4f);
		jsCommandParam2 = GUILayoutLabelTextField("Param 2", jsCommandParam2);
		GUILayout.Space(4f);
		if (GUILayout.Button("Execute", BlueStonez.buttondark_medium, GUILayout.MinHeight(24f)))
		{
			if (string.IsNullOrEmpty(jsCommand))
			{
				jsCommandLog += "Nothing to execute.";
			}
			else if (string.IsNullOrEmpty(jsCommandParam1))
			{
#if UNITY_WEBGL && !UNITY_EDITOR
				// Note: Generic JavaScript calls are not supported in Unity 6 WebGL
				// Application.ExternalCall is obsolete - specific functions need to be defined with [DllImport("__Internal")]
				jsCommandLog = jsCommandLog + jsCommand + " (not executed - obsolete API)\n";
#else
				jsCommandLog = jsCommandLog + jsCommand + " (not executed - WebGL only)\n";
#endif
			}
			else if (string.IsNullOrEmpty(jsCommandParam2) && !string.IsNullOrEmpty(jsCommandParam1))
			{
#if UNITY_WEBGL && !UNITY_EDITOR
				// Note: Generic JavaScript calls are not supported in Unity 6 WebGL
				string text = jsCommandLog;
				jsCommandLog = text + jsCommand + "('" + jsCommandParam1 + "') (not executed - obsolete API)\n";
#else
				string text = jsCommandLog;
				jsCommandLog = text + jsCommand + "('" + jsCommandParam1 + "') (not executed - WebGL only)\n";
#endif
			}
			else if (!string.IsNullOrEmpty(jsCommandParam1) && !string.IsNullOrEmpty(jsCommandParam2))
			{
#if UNITY_WEBGL && !UNITY_EDITOR
				// Note: Generic JavaScript calls are not supported in Unity 6 WebGL
				string text = jsCommandLog;
				jsCommandLog = text + jsCommand + "('" + jsCommandParam1 + "','" + jsCommandParam2 + "') (not executed - obsolete API)\n";
#else
				string text = jsCommandLog;
				jsCommandLog = text + jsCommand + "('" + jsCommandParam1 + "','" + jsCommandParam2 + "') (not executed - WebGL only)\n";
#endif
			}
		}
		GUILayout.Space(4f);
		GUILayout.TextArea(jsCommandLog, GUILayout.MinHeight(64f));
		if (GUILayout.Button("Clear", BlueStonez.buttondark_medium, GUILayout.MinHeight(24f)))
		{
			jsCommandLog = string.Empty;
		}
		GUILayout.EndVertical();
	}

	private string GUILayoutLabelTextField(string label, string text)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, GUILayout.Width(64f));
		string result = GUILayout.TextField(text, GUILayout.MinHeight(24f));
		GUILayout.EndHorizontal();
		return result;
	}

	private string TestFbButton(string label, Action action, string paramOne = null)
	{
		string result = string.Empty;
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(label, BlueStonez.buttondark_medium, GUILayout.MinHeight(24f)) && action != null)
		{
			jsCommandLog = jsCommandLog + "Executed: " + label + "\n";
			action();
		}
		if (paramOne != null)
		{
			result = GUILayout.TextField(paramOne, GUILayout.MinHeight(24f), GUILayout.Width(64f));
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(4f);
		return result;
	}
}
