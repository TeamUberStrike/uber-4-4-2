using UberStrike.WebService.Unity;
using UnityEngine;

public class TestWebserviceFail : MonoBehaviour
{
	private void OnGUI()
	{
		if (GUI.Button(new Rect(Screen.width - 200, Screen.height - 30, 200f, 30f), "Exception: " + ((!Configuration.SimulateWebservicesFail) ? "OFF" : "ON")))
		{
			Configuration.SimulateWebservicesFail = !Configuration.SimulateWebservicesFail;
		}
	}
}
