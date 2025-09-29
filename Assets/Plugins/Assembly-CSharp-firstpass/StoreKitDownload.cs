using System.Collections.Generic;
using Prime31;

public class StoreKitDownload
{
	public StoreKitDownloadState downloadState;

	public double contentLength;

	public string contentIdentifier;

	public string contentURL;

	public string contentVersion;

	public string error;

	public float progress;

	public double timeRemaining;

	public StoreKitTransaction transaction;

	public static List<StoreKitDownload> downloadsFromJson(string json)
	{
		List<StoreKitDownload> list = new List<StoreKitDownload>();
		List<object> list2 = json.listFromJson();
		if (list2 == null)
		{
			return list;
		}
		foreach (Dictionary<string, object> item in list2)
		{
			list.Add(downloadFromDictionary(item));
		}
		return list;
	}

	public static StoreKitDownload downloadFromDictionary(Dictionary<string, object> dict)
	{
		StoreKitDownload storeKitDownload = new StoreKitDownload();
		if (dict.ContainsKey("downloadState"))
		{
			storeKitDownload.downloadState = (StoreKitDownloadState)int.Parse(dict["downloadState"].ToString());
		}
		if (dict.ContainsKey("contentLength"))
		{
			storeKitDownload.contentLength = double.Parse(dict["contentLength"].ToString());
		}
		if (dict.ContainsKey("contentIdentifier"))
		{
			storeKitDownload.contentIdentifier = dict["contentIdentifier"].ToString();
		}
		if (dict.ContainsKey("contentURL"))
		{
			storeKitDownload.contentURL = dict["contentURL"].ToString();
		}
		if (dict.ContainsKey("contentVersion"))
		{
			storeKitDownload.contentVersion = dict["contentVersion"].ToString();
		}
		if (dict.ContainsKey("error"))
		{
			storeKitDownload.error = dict["error"].ToString();
		}
		if (dict.ContainsKey("progress"))
		{
			storeKitDownload.progress = float.Parse(dict["progress"].ToString());
		}
		if (dict.ContainsKey("timeRemaining"))
		{
			storeKitDownload.timeRemaining = double.Parse(dict["timeRemaining"].ToString());
		}
		if (dict.ContainsKey("transaction"))
		{
			storeKitDownload.transaction = StoreKitTransaction.transactionFromDictionary(dict["transaction"] as Dictionary<string, object>);
		}
		return storeKitDownload;
	}

	public override string ToString()
	{
		return string.Format("<StoreKitDownload> downloadState: {0}\n contentLength: {1}\n contentIdentifier: {2}\n contentURL: {3}\n contentVersion: {4}\n error: {5}\n progress: {6}\n transaction: {7}", downloadState, contentLength, contentIdentifier, contentURL, contentVersion, error, progress, transaction);
	}
}
