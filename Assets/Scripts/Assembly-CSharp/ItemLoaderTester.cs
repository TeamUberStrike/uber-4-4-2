using UnityEngine;

public class ItemLoaderTester : MonoBehaviour
{
	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(20f, 300f, 100f, 30f), (!Singleton<ItemLoader>.Instance.Paused) ? "Pause" : "Resume") || Input.GetKeyDown(KeyCode.R))
		{
			Input.ResetInputAxes();
			Singleton<ItemLoader>.Instance.Paused = !Singleton<ItemLoader>.Instance.Paused;
		}
		if (Input.GetKeyDown(KeyCode.T))
		{
			AvatarGearParts avatarGear = Singleton<LoadoutManager>.Instance.GearLoadout.GetAvatarGear();
			Singleton<AvatarBuilder>.Instance.CreateRemoteAvatar(avatarGear, Color.black);
		}
	}
}
