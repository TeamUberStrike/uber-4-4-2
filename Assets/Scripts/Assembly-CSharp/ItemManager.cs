using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UberStrike.WebService.Unity;
using UnityEngine;

public class ItemManager : Singleton<ItemManager>
{
	private Dictionary<UberstrikeItemClass, string> _defaultGearPrefabNames;

	private Dictionary<UberstrikeItemClass, string> _defaultWeaponPrefabNames;

	private Dictionary<int, IUnityItem> _shopItemsById;

	public IEnumerable<IUnityItem> ShopItems
	{
		get
		{
			return _shopItemsById.Values;
		}
	}

	public int ShopItemCount
	{
		get
		{
			return _shopItemsById.Count;
		}
	}

	private ItemManager()
	{
		_shopItemsById = new Dictionary<int, IUnityItem>();
		_defaultGearPrefabNames = new Dictionary<UberstrikeItemClass, string>
		{
			{
				UberstrikeItemClass.GearHead,
				"LutzDefaultGearHead"
			},
			{
				UberstrikeItemClass.GearGloves,
				"LutzDefaultGearGloves"
			},
			{
				UberstrikeItemClass.GearUpperBody,
				"LutzDefaultGearUpperBody"
			},
			{
				UberstrikeItemClass.GearLowerBody,
				"LutzDefaultGearLowerBody"
			},
			{
				UberstrikeItemClass.GearBoots,
				"LutzDefaultGearBoots"
			}
		};
		_defaultWeaponPrefabNames = new Dictionary<UberstrikeItemClass, string>
		{
			{
				UberstrikeItemClass.WeaponMelee,
				"TheSplatbat"
			},
			{
				UberstrikeItemClass.WeaponMachinegun,
				"MachineGun"
			},
			{
				UberstrikeItemClass.WeaponSplattergun,
				"SplatterGun"
			},
			{
				UberstrikeItemClass.WeaponCannon,
				"Cannon"
			},
			{
				UberstrikeItemClass.WeaponSniperRifle,
				"SniperRifle"
			},
			{
				UberstrikeItemClass.WeaponLauncher,
				"Launcher"
			},
			{
				UberstrikeItemClass.WeaponShotgun,
				"ShotGun"
			}
		};
	}

	private void UpdateShopItems(UberStrikeItemShopClientView shopView, List<AssetBundleXmlReader.Config> configs)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (AssetBundleXmlReader.Config config in configs)
		{
			dictionary[config.Name] = config.Version;
		}
		List<BaseUberStrikeItemView> list = new List<BaseUberStrikeItemView>();
		list.AddRange(shopView.GearItems.ToArray());
		list.AddRange(shopView.WeaponItems.ToArray());
		list.AddRange(shopView.QuickItems.ToArray());
		try
		{
			foreach (BaseUberStrikeItemView item in list)
			{
				if (item != null && !string.IsNullOrEmpty(item.PrefabName))
				{
					if (dictionary.ContainsKey(item.PrefabName))
					{
						if (!_shopItemsById.ContainsKey(item.ID))
						{
							_shopItemsById.Add(item.ID, new ProxyItem(item, dictionary[item.PrefabName]));
							continue;
						}
						ProxyItem proxyItem = (ProxyItem)_shopItemsById[item.ID];
						if (proxyItem != null)
						{
							proxyItem.UpdateProxyItem(item, dictionary[item.PrefabName]);
						}
					}
					else if (UnityItemConfiguration.Exists && UnityItemConfiguration.Instance.Contains(item.PrefabName))
					{
						_shopItemsById[item.ID] = new DefaultItem(item);
					}
					else
					{
						Debug.LogWarning("Failed to add shop view: " + item.PrefabName);
					}
				}
				else if (item == null)
				{
					Debug.LogWarning("Shop view is null!");
				}
				else if (string.IsNullOrEmpty(item.PrefabName))
				{
					Debug.LogWarning("PrefabName is empty: " + item.Name + " " + item.ID + " " + item.Description);
				}
			}
			foreach (UberStrikeItemFunctionalView functionalItem in shopView.FunctionalItems)
			{
				_shopItemsById[functionalItem.ID] = new FunctionalItem(functionalItem);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public bool AddDefaultItem(BaseUberStrikeItemView itemView)
	{
		if (itemView != null)
		{
			if (itemView.ItemClass == UberstrikeItemClass.FunctionalGeneral)
			{
				IUnityItem value;
				if (_shopItemsById.TryGetValue(itemView.ID, out value))
				{
					ItemConfigurationUtil.CopyProperties(value.View, itemView);
				}
			}
			else if (string.IsNullOrEmpty(itemView.PrefabName))
			{
				Debug.LogWarning("Missing PrefabName for item: " + itemView.Name);
			}
			else
			{
				Debug.LogError("Missing UnityItem for: '" + itemView.Name + "' with PrefabName: '" + itemView.PrefabName + "'");
			}
		}
		return false;
	}

	public bool TryGetDefaultItem(UberstrikeItemClass itemClass, out IUnityItem item)
	{
		string prefabName;
		if (_defaultGearPrefabNames.TryGetValue(itemClass, out prefabName) || _defaultWeaponPrefabNames.TryGetValue(itemClass, out prefabName))
		{
			item = _shopItemsById.Values.FirstOrDefault((IUnityItem i) => i.View.PrefabName == prefabName);
			return item != null;
		}
		item = null;
		return false;
	}

	public bool IsDefaultGearItem(string prefabName)
	{
		return _defaultGearPrefabNames.ContainsValue(prefabName);
	}

	public GameObject GetDefaultGearItem(UberstrikeItemClass itemClass)
	{
		string defaultGearPrefabName = string.Empty;
		switch (itemClass)
		{
		case UberstrikeItemClass.GearHead:
			defaultGearPrefabName = "LutzDefaultGearHead";
			break;
		case UberstrikeItemClass.GearGloves:
			defaultGearPrefabName = "LutzDefaultGearGloves";
			break;
		case UberstrikeItemClass.GearUpperBody:
			defaultGearPrefabName = "LutzDefaultGearUpperBody";
			break;
		case UberstrikeItemClass.GearLowerBody:
			defaultGearPrefabName = "LutzDefaultGearLowerBody";
			break;
		case UberstrikeItemClass.GearBoots:
			defaultGearPrefabName = "LutzDefaultGearBoots";
			break;
		case UberstrikeItemClass.GearFace:
			defaultGearPrefabName = "LutzDefaultGearFace";
			break;
		}
		GearItem gearItem = UnityItemConfiguration.Instance.UnityItemsDefaultGears.Find((GearItem item) => item.name.Equals(defaultGearPrefabName));
		return (!(gearItem != null)) ? null : gearItem.gameObject;
	}

	public GameObject GetDefaultWeaponItem(UberstrikeItemClass itemClass)
	{
		string defaultWeaponPrefabName = string.Empty;
		switch (itemClass)
		{
		case UberstrikeItemClass.WeaponMelee:
			defaultWeaponPrefabName = "TheSplatbat";
			break;
		case UberstrikeItemClass.WeaponMachinegun:
			defaultWeaponPrefabName = "MachineGun";
			break;
		case UberstrikeItemClass.WeaponSplattergun:
			defaultWeaponPrefabName = "SplatterGun";
			break;
		case UberstrikeItemClass.WeaponCannon:
			defaultWeaponPrefabName = "Cannon";
			break;
		case UberstrikeItemClass.WeaponSniperRifle:
			defaultWeaponPrefabName = "SniperRifle";
			break;
		case UberstrikeItemClass.WeaponLauncher:
			defaultWeaponPrefabName = "Launcher";
			break;
		case UberstrikeItemClass.WeaponShotgun:
			defaultWeaponPrefabName = "ShotGun";
			break;
		}
		WeaponItem weaponItem = UnityItemConfiguration.Instance.UnityItemsDefaultWeapons.Find((WeaponItem item) => item.name.Equals(defaultWeaponPrefabName));
		return (!(weaponItem != null)) ? null : weaponItem.gameObject;
	}

	public List<IUnityItem> GetShopItems(UberstrikeItemType itemType, BuyingMarketType marketType)
	{
		List<IUnityItem> allShopItems = GetAllShopItems();
		allShopItems.RemoveAll((IUnityItem item) => item.View.ItemType != itemType);
		return allShopItems;
	}

	public List<IUnityItem> GetAllShopItems()
	{
		List<IUnityItem> list = new List<IUnityItem>(_shopItemsById.Values);
		list.RemoveAll((IUnityItem item) => !item.View.IsForSale);
		return list;
	}

	public IUnityItem GetItemInShop(int itemId)
	{
		if (_shopItemsById.ContainsKey(itemId))
		{
			return _shopItemsById[itemId];
		}
		return null;
	}

	public bool ValidateItemMall()
	{
		return _shopItemsById.Count > 0;
	}

	public IEnumerator StartGetShop()
	{
		string url = ApplicationDataManager.BaseItemsURL + "ItemAssetBundle.xml?arg=" + DateTime.Now.Ticks;
		List<AssetBundleXmlReader.Config> configs = new List<AssetBundleXmlReader.Config>();
		yield return MonoRoutine.Start(AssetBundleXmlReader.Read(url, configs));
		yield return ShopWebServiceClient.GetShop(delegate(UberStrikeItemShopClientView shop)
		{
			if (shop != null)
			{
				UpdateShopItems(shop, configs);
				WeaponConfigurationHelper.UpdateWeaponStatistics(shop);
			}
			else
			{
				Debug.LogError("ShopWebServiceClient.GetShop returned with NULL");
			}
		}, delegate(Exception ex)
		{
			DebugConsoleManager.SendExceptionReport(ex);
		});
	}

	public IEnumerator StartGetInventory(bool showProgress)
	{
		if (_shopItemsById.Count == 0)
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			PopupSystem.ShowMessage("Error Getting Shop Data", "The shop is empty, perhaps there\nwas an error getting the Shop data?", PopupSystem.AlertType.OK, null);
			yield break;
		}
		List<ItemInventoryView> inventoryView = new List<ItemInventoryView>();
		if (showProgress)
		{
			IPopupDialog popupDialog = PopupSystem.ShowMessage(LocalizedStrings.UpdatingInventory, LocalizedStrings.WereUpdatingYourInventoryPleaseWait, PopupSystem.AlertType.None);
			yield return UserWebServiceClient.GetInventory(PlayerDataManager.AuthToken, delegate(List<ItemInventoryView> view)
			{
				inventoryView = view;
			}, delegate(Exception ex)
			{
				ApplicationDataManager.EventsSystem.SendLoadingError();
				DebugConsoleManager.SendExceptionReport(ex);
			});
			PopupSystem.HideMessage(popupDialog);
		}
		else
		{
			yield return UserWebServiceClient.GetInventory(PlayerDataManager.AuthToken, delegate(List<ItemInventoryView> view)
			{
				inventoryView = view;
			}, delegate(Exception ex)
			{
				ApplicationDataManager.EventsSystem.SendLoadingError();
				DebugConsoleManager.SendExceptionReport(ex);
			});
		}
		List<string> prefabs = new List<string>();
		inventoryView.ForEach(delegate(ItemInventoryView view)
		{
			IUnityItem value;
			if (_shopItemsById.TryGetValue(view.ItemId, out value) && value.View.ItemType != UberstrikeItemType.Functional)
			{
				prefabs.Add(value.View.PrefabName);
			}
			prefabs.Reverse();
		});
		Singleton<ItemLoader>.Instance.SetFirstToLoadItems(prefabs);
		AutoMonoBehaviour<TextureLoader>.Instance.SetFirstToLoadImages(prefabs);
		Singleton<InventoryManager>.Instance.UpdateInventoryItems(inventoryView);
	}
}
