using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoRoutine : AutoMonoBehaviour<MonoRoutine>
{
	public class Routine
	{
		public IEnumerator Enu;

		public Coroutine Rot;
	}

	public delegate IEnumerator FunctionFloat(float f);

	public delegate IEnumerator FunctionInt(int i);

	public delegate IEnumerator FunctionVoid();

	public List<Routine> Routines = new List<Routine>();

	private static readonly List<string> _runningRoutines = new List<string>();

	private static bool _isApplicationQuitting = false;

	public event Action OnUpdateEvent;

	private void OnApplicationQuit()
	{
		_isApplicationQuitting = true;
	}

	private void Update()
	{
		if (this.OnUpdateEvent != null)
		{
			this.OnUpdateEvent();
		}
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0f, 0f, 300f, Screen.height));
		foreach (Routine routine in AutoMonoBehaviour<MonoRoutine>.Instance.Routines)
		{
			GUILayout.Label("Routine: " + (routine.Rot != null) + " " + routine.Enu.ToString() + " " + (routine.Enu.Current != null));
		}
		GUILayout.EndArea();
	}

	public static Coroutine Start(IEnumerator routine)
	{
		if (!_isApplicationQuitting)
		{
			return AutoMonoBehaviour<MonoRoutine>.Instance.StartCoroutine(routine);
		}
		return null;
	}

	private static Coroutine Start(IEnumerator routine, string code)
	{
		if (!_isApplicationQuitting)
		{
			return AutoMonoBehaviour<MonoRoutine>.Instance.StartCoroutine(AutoMonoBehaviour<MonoRoutine>.Instance.StartSafeRoutine(routine, code));
		}
		return null;
	}

	private IEnumerator StartSafeRoutine(IEnumerator routine, string code)
	{
		if (!_runningRoutines.Contains(code))
		{
			_runningRoutines.Add(code);
			yield return AutoMonoBehaviour<MonoRoutine>.Instance.StartCoroutine(routine);
			_runningRoutines.Remove(code);
		}
		else
		{
			Debug.LogWarning("Ignored multiple call to routine " + code);
		}
	}

	public static Coroutine Run(FunctionFloat run, float f)
	{
		string code = "Run " + run.Method.GetHashCode() + " " + run.Target.GetHashCode();
		return Start(run(f), code);
	}

	public static Coroutine Run(FunctionInt run, int i)
	{
		string code = "Run " + run.Method.GetHashCode() + " " + run.Target.GetHashCode();
		return Start(run(i), code);
	}

	public static Coroutine Run(FunctionVoid run)
	{
		string code = "Run " + run.Method.GetHashCode() + " " + run.Target.GetHashCode();
		return Start(run(), code);
	}
}
