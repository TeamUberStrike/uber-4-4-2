using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;

[NetworkClass(3)]
public class ClientLobbyCenter : ClientNetworkClass
{
	private ActorInfo _myInfo;

	public ActorInfo MyInfo
	{
		get
		{
			return _myInfo;
		}
	}

	public ClientLobbyCenter(RemoteMethodInterface rmi)
		: base(rmi)
	{
		_myInfo = new CommActorInfo(string.Empty, 0, ChannelType.WebPortal);
	}

	protected override void OnInitialized()
	{
		base.OnInitialized();
		MyInfo.Cache.Clear();
		MyInfo.ActorId = _rmi.Messenger.PeerListener.ActorIdSecure;
		MyInfo.PlayerName = string.Empty;
		SendMethodToServer(1, MyInfo);
	}
}
