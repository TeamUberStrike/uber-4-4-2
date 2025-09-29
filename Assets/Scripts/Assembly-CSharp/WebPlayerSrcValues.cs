using System;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UnityEngine;

public class WebPlayerSrcValues
{
	public int Cmid { get; private set; }

	public DateTime Expiration { get; private set; }

	public string Content { get; private set; }

	public string Hash { get; private set; }

	public string EsnsId { get; private set; }

	public ChannelType ChannelType { get; private set; }

	public EmbedType EmbedType { get; private set; }

	public string Locale { get; private set; }

	public bool HasValues { get; private set; }

	public bool IsValid
	{
		get
		{
			return Cmid > 0 && !string.IsNullOrEmpty(Hash);
		}
	}

	public WebPlayerSrcValues(string srcValue)
	{
		HasValues = false;
		Expiration = DateTime.MinValue;
		if (!string.IsNullOrEmpty(srcValue))
		{
			Dictionary<string, string> dictionary = ParseQueryString(srcValue);
			if (dictionary.Count > 0)
			{
				HasValues = true;
				ChannelType = ParseKey<ChannelType>(dictionary, "channeltype");
			}
		}
	}

	private static T ParseKey<T>(Dictionary<string, string> dict, string key)
	{
		T result = default(T);
		string value;
		if (dict.TryGetValue(key, out value))
		{
			result = StringUtils.ParseValue<T>(value);
		}
		else
		{
			Debug.LogWarning("ParseKey didn't find value for key '" + key + "'");
		}
		return result;
	}

	public static Dictionary<string, string> ParseQueryString(string queryString)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (string.IsNullOrEmpty(queryString) || !queryString.Contains("=") || queryString.Length < 3)
		{
			Debug.LogWarning("Invalid Querystring: " + queryString);
		}
		else
		{
			string[] array = queryString.Substring(queryString.IndexOf("?") + 1, queryString.Length - queryString.IndexOf("?") - 1).Split('&');
			foreach (string text in array)
			{
				dictionary.Add(text.Substring(0, text.IndexOf("=")).ToLower(), text.Substring(text.IndexOf("=") + 1, text.Length - text.IndexOf("=") - 1));
			}
		}
		return dictionary;
	}
}
