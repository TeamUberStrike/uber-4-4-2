using UnityEngine;

internal class GUIController : MonoBehaviour
{
	[SerializeField]
	private GUIPageBase home;

	[SerializeField]
	private GUIPageBase menu;

	[SerializeField]
	private GUIPageBase social;

	[SerializeField]
	private GameObject xpbar;

	[SerializeField]
	private GameObject screenshotView;

	private void OnEnable()
	{
		GameData.Instance.MainMenu.AddEventAndFire(OnMenuChanged);
	}

	private void OnDisable()
	{
		GameData.Instance.MainMenu.Changed -= OnMenuChanged;
	}

	private void OnMenuChanged(MainMenuState state)
	{
		SetPage(home, state == MainMenuState.Home);
		SetPage(menu, state == MainMenuState.Menu);
		SetPage(social, state == MainMenuState.Menu || state == MainMenuState.Home);
		if (xpbar != null)
		{
			xpbar.SetActive(state != MainMenuState.Logout);
		}
		if (screenshotView != null)
		{
			screenshotView.SetActive(state == MainMenuState.Home);
		}
	}

	private void SetPage(GUIPageBase page, bool enabled)
	{
		if (!(page == null))
		{
			page.gameObject.SetActive(enabled);
			if (enabled)
			{
				page.BringIn();
			}
		}
	}
}
