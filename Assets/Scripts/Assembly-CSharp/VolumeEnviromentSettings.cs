using UnityEngine;

[RequireComponent(typeof(Collider))]
public class VolumeEnviromentSettings : MonoBehaviour
{
	public EnviromentSettings Settings;

	private void Awake()
	{
		base.GetComponent<Collider>().isTrigger = true;
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (!(collider.tag == "Player"))
		{
			return;
		}
		GameState.LocalPlayer.MoveController.SetEnviroment(Settings, base.GetComponent<Collider>().bounds);
		if (Settings.Type == EnviromentSettings.TYPE.WATER)
		{
			float y = GameState.LocalPlayer.MoveController.Velocity.y;
			if (y < -20f)
			{
				SfxManager.Play3dAudioClip(GameAudio.BigSplash, collider.transform.position);
			}
			else if (y < -10f)
			{
				SfxManager.Play3dAudioClip(GameAudio.MediumSplash, collider.transform.position);
			}
		}
	}

	private void OnTriggerExit(Collider c)
	{
		if (c.tag == "Player")
		{
			GameState.LocalPlayer.MoveController.ResetEnviroment();
		}
	}
}
