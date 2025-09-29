using System;
using UnityEngine;

public class AutoMonoBehaviour<T> : MonoBehaviour where T : class
{
	public const string GameObjectName = "AutoMonoBehaviours";

	protected static T _instance;

	private static GameObject _parent;

	private static bool _isRunning = true;

	private static bool _isInstantiating;

	private static GameObject Parent
	{
		get
		{
			_parent = GameObject.Find("AutoMonoBehaviours");
			if (_parent == null)
			{
				_parent = new GameObject("AutoMonoBehaviours");
				UnityEngine.Object.DontDestroyOnLoad(_parent);
			}
			return _parent;
		}
	}

	public static T Instance
	{
		get
		{
			if (_instance == null && _isRunning)
			{
				if (_isInstantiating)
				{
					throw new Exception(string.Concat("Recursive calls to Constuctor of AutoMonoBehaviour! Check your ", typeof(T), ":Awake() function for calls to ", typeof(T), ".Instance"));
				}
				_isInstantiating = true;
				_instance = Parent.AddComponent(typeof(T)) as T;
			}
			return _instance;
		}
	}

	private void OnApplicationQuit()
	{
		_isRunning = false;
	}

	protected virtual void Start()
	{
		if (_instance == null)
		{
			throw new Exception("The script " + typeof(T).Name + " is self instantiating and shouldn't be attached manually to a GameObject.");
		}
	}
}
