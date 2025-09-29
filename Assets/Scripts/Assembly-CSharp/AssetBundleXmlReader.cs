using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class AssetBundleXmlReader
{
	public class Config
	{
		public string Name;

		public string Version;
	}

	public static IEnumerator Read(string url, List<Config> configs)
	{
		WWW www = new WWW(url);
		yield return www;
		if (string.IsNullOrEmpty(www.error))
		{
			using (XmlTextReader reader = new XmlTextReader(new StringReader(www.text)))
			{
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("Prefab"))
					{
						configs.Add(new Config
						{
							Name = reader.GetAttribute("Name"),
							Version = reader.GetAttribute("Version")
						});
					}
				}
				yield break;
			}
		}
		Debug.LogError("Failed to load url: " + url + "\nError: " + www.error);
	}
}
