using System.Collections;
using System.Collections.Generic;

public static class CoroutineManager
{
	public delegate IEnumerator CoroutineFunction();

	private static int _routineId = 1;

	public static Dictionary<string, int> coroutineFuncIds = new Dictionary<string, int>();

	public static void StartCoroutine(CoroutineFunction func, bool unique = true)
	{
		if (!unique || !IsRunning(func))
		{
			MonoRoutine.Start(func());
		}
	}

	public static int Begin(CoroutineFunction func)
	{
		coroutineFuncIds[GetMethodId(func)] = ++_routineId;
		return _routineId;
	}

	public static void End(CoroutineFunction func, int id)
	{
		string methodId = GetMethodId(func);
		if (coroutineFuncIds.ContainsKey(methodId) && coroutineFuncIds[methodId] == id)
		{
			coroutineFuncIds.Remove(methodId);
		}
	}

	public static bool IsRunning(CoroutineFunction func)
	{
		return coroutineFuncIds.ContainsKey(GetMethodId(func));
	}

	public static bool IsCurrent(CoroutineFunction func, int coroutineId)
	{
		int value = 0;
		coroutineFuncIds.TryGetValue(GetMethodId(func), out value);
		return value == coroutineId;
	}

	public static void StopCoroutine(CoroutineFunction func)
	{
		coroutineFuncIds.Remove(GetMethodId(func));
	}

	private static string GetMethodId(CoroutineFunction callback)
	{
		return string.Format("{0}{1}", callback.Method.DeclaringType.FullName, callback.Method.Name);
	}
}
