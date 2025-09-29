using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class DebugRemoteMethodInterface : IDebugPage
{
	private Vector2[] _scrollers = new Vector2[3];

	public string Title
	{
		get
		{
			return "Rmi";
		}
	}

	public void Draw()
	{
		DrawRemoteMethodInterface(GameConnectionManager.Rmi, 0);
	}

	public void DrawRemoteMethodInterface(RemoteMethodInterface rmi, int i)
	{
		GUILayout.BeginHorizontal();
		_scrollers[i * 3] = GUILayout.BeginScrollView(_scrollers[i * 3], GUILayout.Width(200f));
		GUILayout.Label("WAIT REG");
		foreach (RemoteMethodInterface.RegistrationJob registrationJob in rmi.RegistrationJobs)
		{
			GUILayout.Label(string.Format("{0}", registrationJob));
		}
		GUILayout.EndScrollView();
		_scrollers[i * 3 + 1] = GUILayout.BeginScrollView(_scrollers[i * 3 + 1], GUILayout.Width(400f));
		GUILayout.Label("REG CLASS");
		foreach (INetworkClass registeredClass in rmi.RegisteredClasses)
		{
			GUILayout.Label(string.Format("{0}", registeredClass));
		}
		GUILayout.EndScrollView();
		_scrollers[i * 3 + 2] = GUILayout.BeginScrollView(_scrollers[i * 3 + 2], GUILayout.Width(100f));
		GUILayout.Label("NET CLASS");
		foreach (short networkInstantiatedObject in rmi.NetworkInstantiatedObjects)
		{
			GUILayout.Label(string.Format("Nid {0}", networkInstantiatedObject));
		}
		GUILayout.EndScrollView();
		GUILayout.EndHorizontal();
	}
}
