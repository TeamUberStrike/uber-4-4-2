using System.Runtime.InteropServices;
using UnityEngine;

public class MATInterface : AutoMonoBehaviour<MATInterface>
{
	public struct MATEventItem
	{
		public string item;

		public double unitPrice;

		public int quantity;

		public double revenue;
	}

	[DllImport("__Internal")]
	private static extern void initNativeCode(string advertiserId, string advertiserKey);

	[DllImport("__Internal")]
	private static extern void setAllowDuplicates(bool allowDuplicates);

	[DllImport("__Internal")]
	private static extern void setShouldAutoGenerateMacAddress(bool shouldAutoGenerate);

	[DllImport("__Internal")]
	private static extern void setShouldAutoGenerateODIN1Key(bool shouldAutoGenerate);

	[DllImport("__Internal")]
	private static extern void setShouldAutoGenerateOpenUDIDKey(bool shouldAutoGenerate);

	[DllImport("__Internal")]
	private static extern void setShouldAutoGenerateVendorIdentifier(bool shouldAutoGenerate);

	[DllImport("__Internal")]
	private static extern void setShouldAutoGenerateAdvertiserIdentifier(bool shouldAutoGenerate);

	[DllImport("__Internal")]
	private static extern void setCurrencyCode(string currency_code);

	[DllImport("__Internal")]
	private static extern void setDebugMode(bool enable);

	[DllImport("__Internal")]
	private static extern void setDeviceId(bool enable);

	[DllImport("__Internal")]
	private static extern void setOpenUDID(string open_udid);

	[DllImport("__Internal")]
	private static extern void setPackageName(string package_name);

	[DllImport("__Internal")]
	private static extern void setRedirectUrl(string redirectUrl);

	[DllImport("__Internal")]
	private static extern void setSiteId(string site_id);

	[DllImport("__Internal")]
	private static extern void setTRUSTeId(string truste_tpid);

	[DllImport("__Internal")]
	private static extern void setUserId(string user_id);

	[DllImport("__Internal")]
	private static extern void setUseCookieTracking(bool useCookieTracking);

	[DllImport("__Internal")]
	private static extern void setUseHTTPS(bool useHTTPS);

	[DllImport("__Internal")]
	private static extern void setAdvertiserIdentifier(string advertiserIdentifier);

	[DllImport("__Internal")]
	private static extern void setVendorIdentifier(string vendorIdentifier);

	[DllImport("__Internal")]
	private static extern void setDelegate(bool enable);

	[DllImport("__Internal")]
	private static extern void startAppToAppTracking(string targetAppId, string advertiserId, string offerId, string publisherId, bool shouldRedirect);

	[DllImport("__Internal")]
	private static extern void trackAction(string action, bool isId, double revenue, string currencyCode);

	[DllImport("__Internal")]
	private static extern void trackActionWithEventItem(string action, bool isId, MATEventItem[] items, int eventItemCount, string refId, double revenue, string currency, int transactionState);

	[DllImport("__Internal")]
	private static extern void trackInstall();

	[DllImport("__Internal")]
	private static extern void trackUpdate();

	[DllImport("__Internal")]
	private static extern void trackInstallWithReferenceId(string refId);

	[DllImport("__Internal")]
	private static extern void trackUpdateWithReferenceId(string refId);

	[DllImport("__Internal")]
	private static extern string getSDKDataParameters();

	public void OnStartup()
	{
		initNativeCode("5596", "97092e1117a4e73e67031f4dbb1c8d93");
		if (PlayerPrefs.HasKey("Version"))
		{
			trackUpdate();
		}
		else
		{
			trackInstall();
		}
	}

	public void RecordRegistration(int cmid)
	{
		setUserId(cmid.ToString());
		trackAction("account_creation", false, 0.0, string.Empty);
	}

	public void RecordPurchase(int cmid, string itemId, double usdPrice)
	{
		setUserId(cmid.ToString());
		MATEventItem mATEventItem = new MATEventItem
		{
			item = itemId,
			unitPrice = usdPrice,
			quantity = 1,
			revenue = usdPrice
		};
		MATEventItem[] array = new MATEventItem[1] { mATEventItem };
		trackActionWithEventItem("purchase", false, array, array.Length, null, 0.0, "USD", 1);
	}
}
