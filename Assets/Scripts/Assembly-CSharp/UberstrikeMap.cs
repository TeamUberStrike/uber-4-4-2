using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UnityEngine;

public class UberstrikeMap
{
	public bool IsVisible { get; set; }

	public MapView View { get; private set; }

	public DynamicTexture Icon { get; private set; }

	public bool IsBuiltIn { get; set; }

	public bool IsBluebox
	{
		get
		{
			return View.IsBlueBox;
		}
	}

	public int Id
	{
		get
		{
			return View.MapId;
		}
	}

	public string Name
	{
		get
		{
			return View.DisplayName;
		}
	}

	public string Description
	{
		get
		{
			return View.Description;
		}
	}

	public string SceneName
	{
		get
		{
			return View.SceneName;
		}
	}

	public string AssetbundleName { get; private set; }

	public string MapIconUrl { get; private set; }

	public UberstrikeMap(MapView view)
	{
		View = view;
		IsVisible = true;
		MapIconUrl = ApplicationDataManager.BaseImageURL + "MapIcons/" + View.SceneName + ".jpg";
		Icon = new DynamicTexture(MapIconUrl, true);
		UpdateAssetbundleUrl(string.Empty);
	}

	public bool IsGameModeSupported(GameModeType mode)
	{
		return View.Settings != null && View.Settings.ContainsKey(mode);
	}

	public void SetVersion(string version)
	{
		if (!string.IsNullOrEmpty(version))
		{
			UpdateAssetbundleUrl(version);
		}
		else
		{
			Debug.LogError("Failed to set map version!");
		}
	}

	private void UpdateAssetbundleUrl(string version)
	{
		AssetbundleName = string.Format("{0}-{1}.unity3d", SceneName, version);
	}
}
