using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cmune.DataCenter.Common.Entities;
using UberStrike.WebService.Unity;
using UnityEngine;

public class BundleManager : Singleton<BundleManager>
{
	private BasePopupDialog _appStorePopup;

	private Dictionary<BundleCategoryType, List<BundleUnityView>> _bundlesPerCategory;

	private string _lastErrorTransaction = string.Empty;

	private string _lastErrorBundleId = string.Empty;

	private float dialogTimer;

	public int Count { get; private set; }

	public bool CanMakeMasPayments { get; private set; }

	public bool WaitingForBundles { get; private set; }

	public IEnumerable<BundleUnityView> AllItemBundles
	{
		get
		{
			foreach (KeyValuePair<BundleCategoryType, List<BundleUnityView>> category in _bundlesPerCategory)
			{
				if (category.Key == BundleCategoryType.None)
				{
					continue;
				}
				foreach (BundleUnityView item in category.Value)
				{
					yield return item;
				}
			}
		}
	}

	public IEnumerable<BundleUnityView> AllBundles
	{
		get
		{
			foreach (List<BundleUnityView> bundleUnityViews in _bundlesPerCategory.Values)
			{
				foreach (BundleUnityView item in bundleUnityViews)
				{
					yield return item;
				}
			}
		}
	}

	private BundleManager()
	{
		_bundlesPerCategory = new Dictionary<BundleCategoryType, List<BundleUnityView>>();
		StoreKitManager.productListReceivedEvent += OnStoreKitProductListReceived;
		StoreKitManager.productListRequestFailedEvent += OnStoreKitProductListRequestFailed;
		StoreKitManager.purchaseFailedEvent += OnStoreKitPurchaseFailed;
		StoreKitManager.purchaseCancelledEvent += OnStoreKitPurchaseCancelled;
		StoreKitManager.purchaseSuccessfulEvent += OnStoreKitPurchaseSuccessful;
		CanMakeMasPayments = StoreKitBinding.canMakePayments();
	}

	public List<BundleUnityView> GetCreditBundles()
	{
		List<BundleUnityView> value;
		if (_bundlesPerCategory.TryGetValue(BundleCategoryType.None, out value))
		{
			return value;
		}
		return new List<BundleUnityView>(0);
	}

	public List<BundleUnityView> GetBundlesInCategory(BundleCategoryType category)
	{
		List<BundleUnityView> value;
		if (_bundlesPerCategory.TryGetValue(category, out value))
		{
			return value;
		}
		return new List<BundleUnityView>(0);
	}

	public void Initialize()
	{
		WaitingForBundles = true;
		ShopWebServiceClient.GetBundles(ApplicationDataManager.Channel, delegate(List<BundleView> bundles)
		{
			SetBundles(bundles);
		}, delegate
		{
			Debug.LogError(string.Concat("Error getting ", ApplicationDataManager.Channel, " bundles from the server."));
		});
	}

	private void SetBundles(List<BundleView> bundleViews)
	{
		if (bundleViews != null && bundleViews.Count > 0)
		{
			foreach (BundleView bundleView in bundleViews)
			{
				if ((ApplicationDataManager.Channel != ChannelType.MacAppStore || !string.IsNullOrEmpty(bundleView.MacAppStoreUniqueId)) && (ApplicationDataManager.Channel != ChannelType.IPad || !string.IsNullOrEmpty(bundleView.IosAppStoreUniqueId)) && (ApplicationDataManager.Channel != ChannelType.IPhone || !string.IsNullOrEmpty(bundleView.IosAppStoreUniqueId)) && (ApplicationDataManager.Channel != ChannelType.Android || !string.IsNullOrEmpty(bundleView.AndroidStoreUniqueId)))
				{
					List<BundleUnityView> value;
					if (!_bundlesPerCategory.TryGetValue(bundleView.Category, out value))
					{
						value = new List<BundleUnityView>();
						_bundlesPerCategory[bundleView.Category] = value;
					}
					value.Add(new BundleUnityView(bundleView));
				}
			}
			GetStoreKitProductData();
		}
		else
		{
			Debug.LogError("SetBundles: Bundles received from the server were null or empty!");
		}
	}

	public IEnumerator StartCancelDialogTimer()
	{
		if (dialogTimer < 5f)
		{
			dialogTimer = 5f;
		}
		while (_appStorePopup != null && dialogTimer > 0f)
		{
			dialogTimer -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		if (_appStorePopup != null)
		{
			_appStorePopup.SetAlertType(PopupSystem.AlertType.Cancel);
		}
	}

	public void BuyStoreKitItem(BundleUnityView bundle)
	{
		_appStorePopup = PopupSystem.ShowMessage("In App Purchase", "Opening the Store, please wait...", PopupSystem.AlertType.None) as BasePopupDialog;
		TouchInput.DisableIdleTimer = true;
		MonoRoutine.Start(StartCancelDialogTimer());
		StoreKitBinding.purchaseProduct(bundle.BundleView.IosAppStoreUniqueId, 1);
	}

	public void BuyFacebookBundle(int bundleId)
	{
		_appStorePopup = PopupSystem.ShowMessage("Facebook Purchase", "Opening Facebook Credits, please wait...", PopupSystem.AlertType.None) as BasePopupDialog;
		MonoRoutine.Start(StartCancelDialogTimer());
		AutoMonoBehaviour<FacebookInterface>.Instance.PurchaseBundle(bundleId);
	}

	public void OnFacebookPayment(string status)
	{
		if (_appStorePopup != null)
		{
			PopupSystem.HideMessage(_appStorePopup);
		}
		if (status.Contains("completed"))
		{
			PopupSystem.ShowMessage("Purchase Successful", "Thank you, your purchase was successful.", PopupSystem.AlertType.OK, delegate
			{
				ApplicationDataManager.RefreshWallet();
			});
		}
		else
		{
			PopupSystem.ShowMessage("Purchase Failed", "Sorry, there was a problem processing your payment. Please visit support.uberstrike.com for help.", PopupSystem.AlertType.OK);
		}
	}

	private void AndroidBillingSupportedEvent()
	{
		CanMakeMasPayments = true;
		GetStoreKitProductData();
	}

	private void AndroidBillingNotSupportedEvent(string obj)
	{
		CanMakeMasPayments = false;
		WaitingForBundles = false;
	}

	private void GetStoreKitProductData()
	{
		string[] array = new string[AllBundles.Count()];
		int num = 0;
		foreach (BundleUnityView allBundle in AllBundles)
		{
			array[num] = allBundle.BundleView.IosAppStoreUniqueId;
			num++;
		}
		StoreKitBinding.requestProductData(array);
	}

	private void BuyBundle(BundleUnityView bundle, string transactionIdentifier)
	{
		MonoRoutine.Start(StartBuyBundle(transactionIdentifier, bundle));
	}

	private IEnumerator StartBuyBundle(string transactionIdentifier, BundleUnityView bundle)
	{
		yield return new WaitForSeconds(1f);
		IPopupDialog popupDialog = PopupSystem.ShowMessage("Updating", "Completing your Purchase, please wait...", PopupSystem.AlertType.None);
		BundleUnityView bundle2 = default(BundleUnityView);
		string transactionIdentifier2 = default(string);
		yield return ShopWebServiceClient.BuyBundle(PlayerDataManager.AuthToken, bundle.BundleView.Id, ApplicationDataManager.Channel, transactionIdentifier, delegate(bool success)
		{
			PopupSystem.HideMessage(popupDialog);
			if (success)
			{
				OnBundlePurchased(bundle2);
			}
			else
			{
				PopupSystem.HideMessage(popupDialog);
				PopupSystem.ShowMessage(LocalizedStrings.Error, "There was an error completing your purchase.\nPlease visit support.uberstrike.com");
			}
		}, delegate(Exception exception)
		{
			PopupSystem.HideMessage(popupDialog);
			_lastErrorBundleId = bundle2.BundleView.IosAppStoreUniqueId;
			_lastErrorTransaction = transactionIdentifier2;
			PopupSystem.ShowMessage(LocalizedStrings.Error, "Your payment was processed but our servers had an issue accepting it. Click OK to log the issue with our support system.", PopupSystem.AlertType.OKCancel, ShowSupportPortal);
			Debug.LogError("Error - ShopWebServiceClient.BuyMasBundle(): " + exception.Message);
		});
	}

	private void ShowSupportPortal()
	{
		string url = string.Format("http://support.uberstrike.com/customer/widget/emails/new?t=177975&interaction[name]={0}&email[subject]=Bundle%20Purchase%20failed&email[body]=Attempt%20to%20purchase%20bundle%20failed.%20Channel:{1}%20Identifier:{2}%20Receipt:{3}", WWW.EscapeURL(PlayerDataManager.Name), WWW.EscapeURL(ApplicationDataManager.Channel.ToString()), WWW.EscapeURL(_lastErrorBundleId), WWW.EscapeURL(_lastErrorTransaction));
		ApplicationDataManager.OpenUrl(string.Empty, url);
	}

	private void OnBundlePurchased(BundleUnityView bundle)
	{
		StoreKitBinding.finishPendingTransaction(bundle.BundleView.IosAppStoreUniqueId);
		AutoMonoBehaviour<MATInterface>.Instance.RecordPurchase(PlayerDataManager.CmidSecure, bundle.BundleView.IosAppStoreUniqueId, (double)bundle.BundleView.USDPrice);
		if (bundle.BundleView.Credits > 0 || bundle.BundleView.Points > 0)
		{
			ApplicationDataManager.RefreshWallet();
			return;
		}
		List<IUnityItem> list = new List<IUnityItem>(8);
		for (int i = 0; i < bundle.BundleView.BundleItemViews.Count && i < 8; i++)
		{
			list.Add(Singleton<ItemManager>.Instance.GetItemInShop(bundle.BundleView.BundleItemViews[i].ItemId));
		}
		PopupSystem.ShowItems("Purchase Successful", "New Items have been added to your inventory!", list, ShopArea.Inventory);
		UserWebServiceClient.GetInventory(PlayerDataManager.AuthToken, delegate(List<ItemInventoryView> inventory)
		{
			Singleton<InventoryManager>.Instance.UpdateInventoryItems(inventory);
			foreach (BundleUnityView allBundle in AllBundles)
			{
				allBundle.IsOwned = IsItemPackOwned(allBundle.BundleView.BundleItemViews);
			}
		}, delegate(Exception exception)
		{
			Debug.LogError("Exception getting inventory: " + exception.Message);
		});
	}

	private bool IsItemPackOwned(List<BundleItemView> items)
	{
		if (items.Count > 0)
		{
			foreach (BundleItemView item in items)
			{
				if (!Singleton<InventoryManager>.Instance.Contains(item.ItemId))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public BundleUnityView GetNextItem(BundleUnityView currentItem)
	{
		List<BundleUnityView> list = new List<BundleUnityView>(AllItemBundles);
		if (list.Count > 0)
		{
			int num = list.FindIndex((BundleUnityView i) => i == currentItem);
			if (num < 0)
			{
				return list[UnityEngine.Random.Range(0, list.Count)];
			}
			int index = (num + 1) % list.Count;
			return list[index];
		}
		return currentItem;
	}

	public BundleUnityView GetPreviousItem(BundleUnityView currentItem)
	{
		List<BundleUnityView> list = new List<BundleUnityView>(AllItemBundles);
		if (list.Count > 0)
		{
			int num = list.FindIndex((BundleUnityView i) => i == currentItem);
			if (num < 0)
			{
				return list[UnityEngine.Random.Range(0, list.Count)];
			}
			int index = (num - 1 + list.Count) % list.Count;
			return list[index];
		}
		return currentItem;
	}

	private void OnStoreKitPurchaseFailed(string error)
	{
		TouchInput.DisableIdleTimer = true;
		if (_appStorePopup != null)
		{
			PopupSystem.HideMessage(_appStorePopup);
		}
		PopupSystem.ShowMessage("Purchase Failed", "Sorry, it seems your purchase failed.\n Please visit support.uberstrike.com");
	}

	private void AndroidPurchaseFailedEvent(string obj)
	{
		TouchInput.DisableIdleTimer = true;
		Debug.LogError("Payment failed with error: " + obj);
		if (_appStorePopup != null)
		{
			PopupSystem.HideMessage(_appStorePopup);
		}
		PopupSystem.ShowMessage("Purchase Failed", "Sorry, it seems your purchase failed.\n Please visit support.uberstrike.com");
	}

	private void OnStoreKitPurchaseCancelled(string error)
	{
		TouchInput.DisableIdleTimer = true;
		if (_appStorePopup != null)
		{
			PopupSystem.HideMessage(_appStorePopup);
		}
		PopupSystem.ShowMessage("Purchase Cancelled", "Your purchase was cancelled.");
	}

	private void AndroidPurchaseCancelledEvent(string arg1, string arg2)
	{
		TouchInput.DisableIdleTimer = true;
		if (_appStorePopup != null)
		{
			PopupSystem.HideMessage(_appStorePopup);
		}
		PopupSystem.ShowMessage("Purchase Cancelled", "Your purchase was cancelled.");
	}

	private void OnStoreKitPurchaseSuccessful(StoreKitTransaction transaction)
	{
		TouchInput.DisableIdleTimer = false;
		if (_appStorePopup != null)
		{
			PopupSystem.HideMessage(_appStorePopup);
		}
		string empty = string.Empty;
		empty = transaction.base64EncodedTransactionReceipt;
		Debug.Log(string.Format("OnStoreKitPurchaseSuccessful: ProductIdenitifier={0} Receipt={1} Quantity={2}", transaction.productIdentifier, empty, transaction.quantity));
		BundleUnityView bundleUnityView = AllBundles.FirstOrDefault((BundleUnityView p) => p.BundleView.IosAppStoreUniqueId == transaction.productIdentifier);
		if (bundleUnityView != null)
		{
			BuyBundle(bundleUnityView, empty);
		}
		else
		{
			Debug.LogError("No Bundle found with ProductIdentifier: " + transaction.productIdentifier);
		}
	}

	private void OnStoreKitProductListRequestFailed(string error)
	{
		Debug.LogError("Error Getting Store Kit Product List (" + error + ")");
	}

	private void AndroidQueryInventorySucceededEvent(List<GooglePurchase> purchases, List<GoogleSkuInfo> skuinfo)
	{
		List<BundleUnityView> list = new List<BundleUnityView>();
		BundleUnityView bundle;
		foreach (BundleUnityView allBundle in AllBundles)
		{
			bundle = allBundle;
			Count = 0;
			GoogleSkuInfo googleSkuInfo = skuinfo.Find((GoogleSkuInfo b) => b.productId == bundle.BundleView.AndroidStoreUniqueId);
			if (googleSkuInfo != null)
			{
				bundle.Price = googleSkuInfo.price;
				bundle.IsOwned = IsItemPackOwned(bundle.BundleView.BundleItemViews);
				Count++;
			}
			else
			{
				bundle.Price = string.Empty;
				list.Add(bundle);
			}
		}
		foreach (BundleUnityView item in list)
		{
			foreach (List<BundleUnityView> value in _bundlesPerCategory.Values)
			{
				if (value.Contains(item))
				{
					value.Remove(item);
					break;
				}
			}
		}
		WaitingForBundles = false;
	}

	private void OnStoreKitProductListReceived(List<StoreKitProduct> productList)
	{
		List<BundleUnityView> list = new List<BundleUnityView>();
		BundleUnityView bundle;
		foreach (BundleUnityView allBundle in AllBundles)
		{
			bundle = allBundle;
			Count = 0;
			StoreKitProduct storeKitProduct = null;
			storeKitProduct = productList.Find((StoreKitProduct b) => b.productIdentifier == bundle.BundleView.IosAppStoreUniqueId);
			if (storeKitProduct != null)
			{
				bundle.CurrencySymbol = storeKitProduct.currencySymbol;
				bundle.Price = storeKitProduct.price;
				bundle.IsOwned = IsItemPackOwned(bundle.BundleView.BundleItemViews);
				Count++;
			}
			else
			{
				bundle.Price = string.Empty;
				list.Add(bundle);
			}
		}
		foreach (BundleUnityView item in list)
		{
			foreach (List<BundleUnityView> value in _bundlesPerCategory.Values)
			{
				if (value.Contains(item))
				{
					value.Remove(item);
					break;
				}
			}
		}
		WaitingForBundles = false;
	}
}
