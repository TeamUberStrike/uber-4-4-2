using Cmune.DataCenter.Common.Entities;

public class ItemAssetBundleLoader
{
	public static string GetSuffixForChannel(ChannelType channel)
	{
		switch (channel)
		{
		case ChannelType.OSXDashboard:
		case ChannelType.WindowsStandalone:
		case ChannelType.MacAppStore:
		case ChannelType.OSXStandalone:
			return "HD";
		case ChannelType.Android:
			return "Android";
		case ChannelType.IPhone:
		case ChannelType.IPad:
			return "iOS";
		default:
			return "SD";
		}
	}
}
