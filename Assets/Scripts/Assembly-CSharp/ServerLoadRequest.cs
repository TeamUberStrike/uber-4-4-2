using System;
using UberStrike.Realtime.UnitySdk;

public class ServerLoadRequest : ServerRequest
{
	public enum RequestStateType
	{
		None = 0,
		Waiting = 1,
		Running = 2,
		Down = 3
	}

	private Action _callback;

	public RequestStateType RequestState { get; private set; }

	public GameServerView Server { get; private set; }

	private ServerLoadRequest(GameServerView server, Action callback)
		: base(AutoMonoBehaviour<MonoRoutine>.Instance)
	{
		_callback = callback;
		Server = server;
		_client.PeerListener.SubscribeToEvents(OnConnectionEvent);
	}

	private void OnConnectionEvent(PhotonPeerListener.ConnectionEvent ev)
	{
		PhotonPeerListener.ConnectionEventType type = ev.Type;
		if (type == PhotonPeerListener.ConnectionEventType.Disconnected && RequestState == RequestStateType.Waiting)
		{
			RequestState = RequestStateType.Down;
		}
	}

	public static ServerLoadRequest Run(GameServerView server, Action callback)
	{
		ServerLoadRequest serverLoadRequest = new ServerLoadRequest(server, callback);
		serverLoadRequest.RunAgain();
		return serverLoadRequest;
	}

	public void RunAgain()
	{
		if (Execute(Server.ConnectionString, null, 2))
		{
			RequestState = RequestStateType.Waiting;
		}
	}

	protected override void OnRequestCallback(int result, object[] table)
	{
		RequestState = RequestStateType.Down;
		base.OnRequestCallback(result, table);
		if (result == 0)
		{
			if (table.Length > 0 && table[0] is ServerLoadData)
			{
				Server.Data = (ServerLoadData)table[0];
				Server.Data.Latency = _client.Latency;
				Server.Data.State = ServerLoadData.Status.Alive;
				RequestState = RequestStateType.Running;
			}
			else
			{
				Server.Data.State = ServerLoadData.Status.NotReachable;
			}
		}
		else
		{
			Server.Data.State = ServerLoadData.Status.NotReachable;
		}
		_callback();
	}
}
