using System;
using Cmune.Core.Models.Views;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class PhotonServerConfiguration : MonoBehaviour
{
	[Serializable]
	public class LocalRealtimeServer
	{
		public string Ip = string.Empty;

		public int Port;

		public bool IsEnabled;

		public string Address
		{
			get
			{
				return Ip + ":" + Port;
			}
		}
	}

	[SerializeField]
	private LocalRealtimeServer _localGameServer = new LocalRealtimeServer
	{
		Ip = "127.0.0.1",
		Port = 5155
	};

	[SerializeField]
	private LocalRealtimeServer _localCommServer = new LocalRealtimeServer
	{
		Ip = "127.0.0.1",
		Port = 5055
	};

	public LocalRealtimeServer CustomGameServer
	{
		get
		{
			return _localGameServer;
		}
	}

	public LocalRealtimeServer CustomCommServer
	{
		get
		{
			return _localCommServer;
		}
	}

	private void Awake()
	{
		CmuneDebug.AddDebugChannel(new UnityDebug());
		if (CustomGameServer.IsEnabled)
		{
			for (int i = 0; i < 20; i += 5)
			{
				Singleton<GameServerManager>.Instance.AddPhotonGameServer(new PhotonView
				{
					IP = CustomGameServer.Ip,
					Port = CustomGameServer.Port,
					Name = "CUSTOM GAME SERVER",
					PhotonId = UnityEngine.Random.Range(-1, -100),
					Region = RegionType.AsiaPacific,
					UsageType = PhotonUsageType.All,
					MinLatency = i
				});
			}
		}
		if (_localCommServer.IsEnabled)
		{
			CmuneNetworkManager.CurrentCommServer = new GameServerView(_localCommServer.Address, PhotonUsageType.CommServer);
			CmuneNetworkManager.UseLocalCommServer = _localCommServer.IsEnabled;
		}
	}
}
