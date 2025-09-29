using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class TryItemGUI : MonoBehaviour
{
	private LoadoutArea _currentLoadoutArea;

	private void OnGUI()
	{
		if (!PopupSystem.IsAnyPopupOpen && !PanelManager.IsAnyPanelOpen)
		{
			LoadoutArea currentLoadoutArea = _currentLoadoutArea;
			if (currentLoadoutArea == LoadoutArea.Gear)
			{
				DrawResetGear();
			}
		}
	}

	private void OnEnable()
	{
		CmuneEventHandler.AddListener<LoadoutAreaChangedEvent>(OnLoadoutAreaChanged);
	}

	private void OnDisable()
	{
		CmuneEventHandler.RemoveListener<LoadoutAreaChangedEvent>(OnLoadoutAreaChanged);
	}

	private void DrawResetGear()
	{
		float num = Mathf.Max((float)(Screen.width - 584) * 0.5f, 170f);
		float num2 = ((float)(Screen.width - 584) - num) * 0.5f;
		if (Singleton<TemporaryLoadoutManager>.Instance.IsGearLoadoutModified && GUITools.Button(new Rect(2f + num2, Screen.height - 60, num, 32f), new GUIContent("Reset Avatar"), BlueStonez.button_white))
		{
			Singleton<TemporaryLoadoutManager>.Instance.ResetGearLoadout();
		}
	}

	public void OnLoadoutAreaChanged(LoadoutAreaChangedEvent ev)
	{
		_currentLoadoutArea = ev.Area;
	}
}
