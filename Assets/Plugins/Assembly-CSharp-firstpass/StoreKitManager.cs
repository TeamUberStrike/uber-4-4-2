using System;
using System.Collections.Generic;
using Prime31;

public class StoreKitManager : AbstractManager
{
	public static bool autoConfirmTransactions;

	public static event Action<List<StoreKitProduct>> productListReceivedEvent;

	public static event Action<string> productListRequestFailedEvent;

	public static event Action<StoreKitTransaction> productPurchaseAwaitingConfirmationEvent;

	public static event Action<StoreKitTransaction> purchaseSuccessfulEvent;

	public static event Action<string> purchaseFailedEvent;

	public static event Action<string> purchaseCancelledEvent;

	public static event Action<string> restoreTransactionsFailedEvent;

	public static event Action restoreTransactionsFinishedEvent;

	public static event Action<List<StoreKitDownload>> paymentQueueUpdatedDownloadsEvent;

	static StoreKitManager()
	{
		autoConfirmTransactions = true;
		AbstractManager.initialize(typeof(StoreKitManager));
	}

	public void productPurchaseAwaitingConfirmation(string json)
	{
		if (StoreKitManager.productPurchaseAwaitingConfirmationEvent != null)
		{
			StoreKitManager.productPurchaseAwaitingConfirmationEvent(StoreKitTransaction.transactionFromJson(json));
		}
		if (autoConfirmTransactions)
		{
			StoreKitBinding.finishPendingTransactions();
		}
	}

	public void productPurchased(string json)
	{
		if (StoreKitManager.purchaseSuccessfulEvent != null)
		{
			StoreKitManager.purchaseSuccessfulEvent(StoreKitTransaction.transactionFromJson(json));
		}
	}

	public void productPurchaseFailed(string error)
	{
		if (StoreKitManager.purchaseFailedEvent != null)
		{
			StoreKitManager.purchaseFailedEvent(error);
		}
	}

	public void productPurchaseCancelled(string error)
	{
		if (StoreKitManager.purchaseCancelledEvent != null)
		{
			StoreKitManager.purchaseCancelledEvent(error);
		}
	}

	public void productsReceived(string json)
	{
		if (StoreKitManager.productListReceivedEvent != null)
		{
			StoreKitManager.productListReceivedEvent(StoreKitProduct.productsFromJson(json));
		}
	}

	public void productsRequestDidFail(string error)
	{
		if (StoreKitManager.productListRequestFailedEvent != null)
		{
			StoreKitManager.productListRequestFailedEvent(error);
		}
	}

	public void restoreCompletedTransactionsFailed(string error)
	{
		if (StoreKitManager.restoreTransactionsFailedEvent != null)
		{
			StoreKitManager.restoreTransactionsFailedEvent(error);
		}
	}

	public void restoreCompletedTransactionsFinished(string empty)
	{
		if (StoreKitManager.restoreTransactionsFinishedEvent != null)
		{
			StoreKitManager.restoreTransactionsFinishedEvent();
		}
	}

	public void paymentQueueUpdatedDownloads(string json)
	{
		if (StoreKitManager.paymentQueueUpdatedDownloadsEvent != null)
		{
			StoreKitManager.paymentQueueUpdatedDownloadsEvent(StoreKitDownload.downloadsFromJson(json));
		}
	}
}
