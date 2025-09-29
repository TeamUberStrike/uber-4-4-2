using System.Collections.Generic;
using Prime31;
using UnityEngine;

public class StoreKitGUIManager : MonoBehaviourGUI
{
	private List<StoreKitProduct> _products;

	private void Start()
	{
		StoreKitManager.productListReceivedEvent += delegate(List<StoreKitProduct> allProducts)
		{
			Debug.Log("received total products: " + allProducts.Count);
			_products = allProducts;
		};
	}

	private void OnGUI()
	{
		beginColumn();
		if (GUILayout.Button("Get Can Make Payments"))
		{
			bool flag = StoreKitBinding.canMakePayments();
			Debug.Log("StoreKit canMakePayments: " + flag);
		}
		if (GUILayout.Button("Get Product Data"))
		{
			string[] productIdentifiers = new string[5] { "anotherProduct", "tt", "testProduct", "sevenDays", "oneMonthSubsciber" };
			StoreKitBinding.requestProductData(productIdentifiers);
		}
		if (GUILayout.Button("Restore Completed Transactions"))
		{
			StoreKitBinding.restoreCompletedTransactions();
		}
		endColumn(true);
		if (_products != null && _products.Count > 0 && GUILayout.Button("Purchase Random Product"))
		{
			int index = Random.Range(0, _products.Count);
			StoreKitProduct storeKitProduct = _products[index];
			Debug.Log("preparing to purchase product: " + storeKitProduct.productIdentifier);
			StoreKitBinding.purchaseProduct(storeKitProduct.productIdentifier, 1);
		}
		if (GUILayout.Button("Get Saved Transactions"))
		{
			List<StoreKitTransaction> allSavedTransactions = StoreKitBinding.getAllSavedTransactions();
			Debug.Log("\ntotal transaction received: " + allSavedTransactions.Count);
			foreach (StoreKitTransaction item in allSavedTransactions)
			{
				Debug.Log(item.ToString() + "\n");
			}
		}
		if (GUILayout.Button("Turn Off Auto Confirmation of Transactions"))
		{
			StoreKitManager.autoConfirmTransactions = false;
		}
		if (GUILayout.Button("Display App Store"))
		{
			StoreKitBinding.displayStoreWithProductId("305967442");
		}
		endColumn();
	}
}
