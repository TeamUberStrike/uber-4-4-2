using Cmune.Core.Models.Views;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class GameServerView
{
	public ServerLoadData Data = ServerLoadData.Empty;

	private ConnectionAddress _address = ConnectionAddress.Empty;

	private PhotonView _view;

	public DynamicTexture Flag { get; set; }

	public static GameServerView Empty
	{
		get
		{
			return new GameServerView();
		}
	}

	public int Id
	{
		get
		{
			return _view.PhotonId;
		}
	}

	public string ConnectionString
	{
		get
		{
			return _address.ConnectionString;
		}
	}

	public float ServerLoad
	{
		get
		{
			return (float)Mathf.Min(Data.PlayersConnected + Data.RoomsCreated, 100) / 100f;
		}
	}

	public int Latency
	{
		get
		{
			return Data.Latency;
		}
	}

	public int MinLatency
	{
		get
		{
			return _view.MinLatency;
		}
	}

	public bool IsValid
	{
		get
		{
			return UsageType != PhotonUsageType.None;
		}
	}

	public PhotonUsageType UsageType
	{
		get
		{
			return _view.UsageType;
		}
	}

	public string Name
	{
		get
		{
			return _view.Name;
		}
	}

	public string Region { get; private set; }

	private GameServerView()
	{
		_view = new PhotonView();
		_address.ConnectionString = "0.0.0.0:0";
		Region = "Default";
		Flag = new DynamicTexture(string.Empty);
	}

	public GameServerView(string address, PhotonUsageType type)
	{
		_address.ConnectionString = address;
		if (_address.IsValid)
		{
			_view = new PhotonView
			{
				Name = "No Name",
				IP = _address.ServerIP,
				Port = int.Parse(_address.ServerPort),
				UsageType = type
			};
		}
		else
		{
			_view = new PhotonView
			{
				Name = "No Name"
			};
		}
		Region = "Default";
		Flag = new DynamicTexture(ApplicationDataManager.BaseImageURL + "Flags/" + Region + ".png", true);
	}

	public GameServerView(PhotonView view)
	{
		_address.ConnectionString = string.Format("{0}:{1}", view.IP, view.Port);
		_view = view;
		int num = _view.Name.IndexOf('[');
		int num2 = _view.Name.IndexOf(']');
		if (num >= 0 && num2 > 1 && num2 > num)
		{
			Region = _view.Name.Substring(num + 1, num2 - num - 1);
		}
		else
		{
			Region = "Default";
		}
		Flag = new DynamicTexture(ApplicationDataManager.BaseImageURL + "Flags/" + Region + ".png", true);
	}

	public override string ToString()
	{
		return string.Format("Address: {0}\nLatency: {1}\nType: {2}\n{3}", _address.ConnectionString, Latency, UsageType, Data.ToString());
	}

	internal bool CheckLatency()
	{
		return MinLatency <= 0 || MinLatency > Latency;
	}
}
