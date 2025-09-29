using System.Collections;

public class GameServerController : Singleton<GameServerController>
{
	public GameServerView SelectedServer { get; set; }

	private GameServerController()
	{
	}

	public void JoinFastestServer()
	{
		MonoRoutine.Start(StartJoiningBestGameServer());
	}

	public void CreateOnFastestServer()
	{
		MonoRoutine.Start(StartCreatingOnBestGameServer());
	}

	public void JoinLastGameServer()
	{
		MenuPageManager.Instance.LoadPage(PageType.Play);
		if (PlayPageGUI.Exists)
		{
			PlayPageGUI.Instance.SelectedServerUpdated();
		}
	}

	private IEnumerator StartJoiningBestGameServer()
	{
		MenuPageManager.Instance.LoadPage(PageType.Play);
		if (Singleton<GameServerController>.Instance.SelectedServer == null)
		{
			ProgressPopupDialog _autoJoinPopup = PopupSystem.ShowProgress(LocalizedStrings.LoadingGameList, LocalizedStrings.FindingAServerToJoin);
			yield return MonoRoutine.Start(Singleton<GameServerManager>.Instance.StartUpdatingLatency(delegate(float progress)
			{
				_autoJoinPopup.Progress = progress;
			}));
			GameServerView bestServer = Singleton<GameServerManager>.Instance.GetBestServer();
			if (bestServer == null)
			{
				PopupSystem.HideMessage(_autoJoinPopup);
				PopupSystem.ShowMessage("Could not find server", "No suitable server could be located! Please try again soon.");
				yield break;
			}
			Singleton<GameServerController>.Instance.SelectedServer = bestServer;
			PopupSystem.HideMessage(_autoJoinPopup);
		}
		if (PlayPageGUI.Exists)
		{
			PlayPageGUI.Instance.SelectedServerUpdated();
		}
	}

	private IEnumerator StartCreatingOnBestGameServer()
	{
		if (Singleton<GameServerController>.Instance.SelectedServer == null)
		{
			ProgressPopupDialog _autoJoinPopup = PopupSystem.ShowProgress(LocalizedStrings.LoadingGameList, LocalizedStrings.FindingAServerToJoin);
			yield return MonoRoutine.Start(Singleton<GameServerManager>.Instance.StartUpdatingLatency(delegate(float progress)
			{
				_autoJoinPopup.Progress = progress;
			}));
			GameServerView bestServer = Singleton<GameServerManager>.Instance.GetBestServer();
			if (bestServer == null)
			{
				PopupSystem.HideMessage(_autoJoinPopup);
				PopupSystem.ShowMessage("Could not find server", "No suitable server could be located! Please try again soon.");
				yield break;
			}
			Singleton<GameServerController>.Instance.SelectedServer = bestServer;
			PopupSystem.HideMessage(_autoJoinPopup);
		}
		PanelManager.Instance.OpenPanel(PanelType.CreateGame);
	}
}
