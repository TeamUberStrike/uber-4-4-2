using System.Collections.Generic;
using Prime31;

public class StoreKitProduct
{
	public string productIdentifier { get; private set; }

	public string title { get; private set; }

	public string description { get; private set; }

	public string price { get; private set; }

	public string currencySymbol { get; private set; }

	public string currencyCode { get; private set; }

	public string formattedPrice { get; private set; }

	public static List<StoreKitProduct> productsFromJson(string json)
	{
		List<StoreKitProduct> list = new List<StoreKitProduct>();
		List<object> list2 = json.listFromJson();
		foreach (Dictionary<string, object> item in list2)
		{
			list.Add(productFromDictionary(item));
		}
		return list;
	}

	public static StoreKitProduct productFromDictionary(Dictionary<string, object> ht)
	{
		StoreKitProduct storeKitProduct = new StoreKitProduct();
		if (ht.ContainsKey("productIdentifier"))
		{
			storeKitProduct.productIdentifier = ht["productIdentifier"].ToString();
		}
		if (ht.ContainsKey("localizedTitle"))
		{
			storeKitProduct.title = ht["localizedTitle"].ToString();
		}
		if (ht.ContainsKey("localizedDescription"))
		{
			storeKitProduct.description = ht["localizedDescription"].ToString();
		}
		if (ht.ContainsKey("price"))
		{
			storeKitProduct.price = ht["price"].ToString();
		}
		if (ht.ContainsKey("currencySymbol"))
		{
			storeKitProduct.currencySymbol = ht["currencySymbol"].ToString();
		}
		if (ht.ContainsKey("currencyCode"))
		{
			storeKitProduct.currencyCode = ht["currencyCode"].ToString();
		}
		if (ht.ContainsKey("formattedPrice"))
		{
			storeKitProduct.formattedPrice = ht["formattedPrice"].ToString();
		}
		return storeKitProduct;
	}

	public override string ToString()
	{
		return string.Format("<StoreKitProduct>\nID: {0}\nTitle: {1}\nDescription: {2}\nPrice: {3}\nCurrency Symbol: {4}\nFormatted Price: {5}\nCurrency Code: {6}", productIdentifier, title, description, price, currencySymbol, formattedPrice, currencyCode);
	}
}
