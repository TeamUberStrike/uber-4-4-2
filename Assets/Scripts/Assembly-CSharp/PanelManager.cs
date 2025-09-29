using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
	private IDictionary<PanelType, IPanelGui> _allPanels;

	private static bool _wasAnyPanelOpen;

	public static PanelManager Instance { get; private set; }

	public static bool Exists
	{
		get
		{
			return Instance != null;
		}
	}

	public LoginPanelGUI LoginPanel
	{
		get
		{
			return _allPanels[PanelType.Login] as LoginPanelGUI;
		}
	}

	public static bool IsAnyPanelOpen { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		_allPanels = new Dictionary<PanelType, IPanelGui>
		{
			{
				PanelType.Login,
				GetComponent<LoginPanelGUI>()
			},
			{
				PanelType.Signup,
				GetComponent<SignupPanelGUI>()
			},
			{
				PanelType.CompleteAccount,
				GetComponent<CompleteAccountPanelGUI>()
			},
			{
				PanelType.Options,
				GetComponent<OptionsPanelGUI>()
			},
			{
				PanelType.Help,
				GetComponent<HelpPanelGUI>()
			},
			{
				PanelType.CreateGame,
				GetComponent<CreateGamePanelGUI>()
			},
			{
				PanelType.ReportPlayer,
				GetComponent<ReportPlayerPanelGUI>()
			},
			{
				PanelType.Moderation,
				GetComponent<ModerationPanelGUI>()
			},
			{
				PanelType.SendMessage,
				GetComponent<SendMessagePanelGUI>()
			},
			{
				PanelType.FriendRequest,
				GetComponent<FriendRequestPanelGUI>()
			},
			{
				PanelType.ClanRequest,
				GetComponent<InviteToClanPanelGUI>()
			},
			{
				PanelType.BuyItem,
				GetComponent<BuyPanelGUI>()
			},
			{
				PanelType.NameChange,
				GetComponent<NameChangePanelGUI>()
			}
		};
		foreach (IPanelGui value in _allPanels.Values)
		{
			MonoBehaviour monoBehaviour = value as MonoBehaviour;
			if ((bool)monoBehaviour)
			{
				monoBehaviour.enabled = false;
			}
		}
	}

	private void OnGUI()
	{
		IsAnyPanelOpen = false;
		foreach (IPanelGui value in _allPanels.Values)
		{
			if (value.IsEnabled)
			{
				IsAnyPanelOpen = true;
				break;
			}
		}
		if (Event.current.type != EventType.Layout)
		{
			return;
		}
		if (IsAnyPanelOpen)
		{
			GuiLockController.EnableLock(GuiDepth.Panel);
		}
		else
		{
			GuiLockController.ReleaseLock(GuiDepth.Panel);
			base.enabled = false;
		}
		if (_wasAnyPanelOpen != IsAnyPanelOpen)
		{
			if (_wasAnyPanelOpen)
			{
				SfxManager.Play2dAudioClip(GameAudio.ClosePanel);
			}
			else
			{
				SfxManager.Play2dAudioClip(GameAudio.OpenPanel);
			}
			_wasAnyPanelOpen = !_wasAnyPanelOpen;
		}
	}

	public bool IsPanelOpen(PanelType panel)
	{
		return _allPanels[panel].IsEnabled;
	}

	public void CloseAllPanels(PanelType except = PanelType.None)
	{
		foreach (IPanelGui value in _allPanels.Values)
		{
			if (value.IsEnabled)
			{
				value.Hide();
			}
		}
	}

	public IPanelGui OpenPanel(PanelType panel)
	{
		foreach (KeyValuePair<PanelType, IPanelGui> allPanel in _allPanels)
		{
			if (panel == allPanel.Key)
			{
				if (!allPanel.Value.IsEnabled)
				{
					allPanel.Value.Show();
				}
			}
			else if (allPanel.Value.IsEnabled)
			{
				allPanel.Value.Hide();
			}
		}
		base.enabled = true;
		return _allPanels[panel];
	}

	public void ClosePanel(PanelType panel)
	{
		if (_allPanels.ContainsKey(panel) && _allPanels[panel].IsEnabled)
		{
			_allPanels[panel].Hide();
		}
	}
}
