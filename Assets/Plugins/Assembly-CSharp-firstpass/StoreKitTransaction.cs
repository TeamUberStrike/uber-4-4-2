using System.Collections.Generic;
using Prime31;

public class StoreKitTransaction
{
	public string productIdentifier;

	public string transactionIdentifier;

	public string base64EncodedTransactionReceipt;

	public int quantity;

	public static List<StoreKitTransaction> transactionsFromJson(string json)
	{
		List<StoreKitTransaction> list = new List<StoreKitTransaction>();
		List<object> list2 = json.listFromJson();
		if (list2 == null)
		{
			return list;
		}
		foreach (Dictionary<string, object> item in list2)
		{
			list.Add(transactionFromDictionary(item));
		}
		return list;
	}

	public static StoreKitTransaction transactionFromJson(string json)
	{
		Dictionary<string, object> dictionary = json.dictionaryFromJson();
		if (dictionary == null)
		{
			return new StoreKitTransaction();
		}
		return transactionFromDictionary(json.dictionaryFromJson());
	}

	public static StoreKitTransaction transactionFromDictionary(Dictionary<string, object> dict)
	{
		StoreKitTransaction storeKitTransaction = new StoreKitTransaction();
		if (dict.ContainsKey("productIdentifier"))
		{
			storeKitTransaction.productIdentifier = dict["productIdentifier"].ToString();
		}
		if (dict.ContainsKey("transactionIdentifier"))
		{
			storeKitTransaction.transactionIdentifier = dict["transactionIdentifier"].ToString();
		}
		if (dict.ContainsKey("base64EncodedReceipt"))
		{
			storeKitTransaction.base64EncodedTransactionReceipt = dict["base64EncodedReceipt"].ToString();
		}
		if (dict.ContainsKey("quantity"))
		{
			storeKitTransaction.quantity = int.Parse(dict["quantity"].ToString());
		}
		return storeKitTransaction;
	}

	public override string ToString()
	{
		return string.Format("<StoreKitTransaction> ID: {0}, quantity: {1}, transactionIdentifier: {2}", productIdentifier, quantity, transactionIdentifier);
	}
}
