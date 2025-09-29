using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class StoreKitBinding
{
	[DllImport("__Internal")]
	private static extern bool _storeKitCanMakePayments();

	public static bool canMakePayments()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _storeKitCanMakePayments();
		}
		return false;
	}

	[DllImport("__Internal")]
	private static extern string _storeKitGetAppStoreReceiptUrl();

	public static string getAppStoreReceiptLocation()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _storeKitGetAppStoreReceiptUrl();
		}
		return null;
	}

	[DllImport("__Internal")]
	private static extern void _storeKitRequestProductData(string productIdentifier);

	public static void requestProductData(string[] productIdentifiers)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_storeKitRequestProductData(string.Join(",", productIdentifiers));
		}
	}

	[DllImport("__Internal")]
	private static extern void _storeKitPurchaseProduct(string productIdentifier, int quantity);

	public static void purchaseProduct(string productIdentifier, int quantity)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_storeKitPurchaseProduct(productIdentifier, quantity);
		}
	}

	[DllImport("__Internal")]
	private static extern void _storeKitFinishPendingTransactions();

	public static void finishPendingTransactions()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_storeKitFinishPendingTransactions();
		}
	}

	[DllImport("__Internal")]
	private static extern void _storeKitFinishPendingTransaction(string transactionIdentifier);

	public static void finishPendingTransaction(string transactionIdentifier)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_storeKitFinishPendingTransaction(transactionIdentifier);
		}
	}

	[DllImport("__Internal")]
	private static extern void _storeKitRestoreCompletedTransactions();

	public static void restoreCompletedTransactions()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_storeKitRestoreCompletedTransactions();
		}
	}

	[DllImport("__Internal")]
	private static extern string _storeKitGetAllSavedTransactions();

	public static List<StoreKitTransaction> getAllSavedTransactions()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			string json = _storeKitGetAllSavedTransactions();
			return StoreKitTransaction.transactionsFromJson(json);
		}
		return new List<StoreKitTransaction>();
	}

	[DllImport("__Internal")]
	private static extern void _storeKitDisplayStoreWithProductId(string productId);

	public static void displayStoreWithProductId(string productId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_storeKitDisplayStoreWithProductId(productId);
		}
	}
}
