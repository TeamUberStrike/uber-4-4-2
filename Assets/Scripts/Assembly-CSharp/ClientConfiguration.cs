using System;
using Cmune.DataCenter.Common.Entities;
using UnityEngine;

public class ClientConfiguration
{
	public string FacebookAppId = string.Empty;

	public BuildType BuildType { get; set; }

	public DebugLevel DebugLevel { get; set; }

	public string WebServiceBaseUrl { get; set; }

	public string ContentBaseUrl { get; set; }

	public string ContentRouterBaseUrl { get; set; }

	public ChannelType ChannelType { get; set; }

	public ClientConfiguration()
	{
		BuildType = BuildType.Dev;
		DebugLevel = DebugLevel.Debug;
		WebServiceBaseUrl = string.Empty;
		ContentBaseUrl = string.Empty;
		ContentRouterBaseUrl = string.Empty;
		ChannelType = ChannelType.WebPortal;
	}

	public void SetBuildType(string value)
	{
		try
		{
			BuildType = (BuildType)(int)Enum.Parse(typeof(BuildType), value);
		}
		catch
		{
			Debug.LogError("Unsupported BuildType!");
		}
	}

	public void SetChannelType(string value)
	{
		try
		{
			ChannelType = (ChannelType)(int)Enum.Parse(typeof(ChannelType), value);
		}
		catch
		{
			Debug.LogError("Unsupported ChannelType!");
		}
	}

	public void SetDebugLevel(string value)
	{
		try
		{
			DebugLevel = (DebugLevel)(int)Enum.Parse(typeof(DebugLevel), value);
		}
		catch
		{
			Debug.LogError("Unsupported DebugLevel!");
		}
	}

	public bool IsValid()
	{
		return !string.IsNullOrEmpty(WebServiceBaseUrl) && !string.IsNullOrEmpty(ContentBaseUrl) && !string.IsNullOrEmpty(ContentRouterBaseUrl);
	}
}
