using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class PlayerStateMsgHud : Singleton<PlayerStateMsgHud>
{
	public delegate void OnButtonClickedDelegate();

	private MeshGUIText _temporaryMsgText;

	private MeshGUIText _permanentMsgText;

	private GUIStyle _buttonGuiStyle;

	public float PermanentMsgHeight
	{
		get
		{
			return (float)Screen.height * 0.03f;
		}
	}

	public float TemporaryMsgHeight
	{
		get
		{
			return (float)Screen.height * 0.08f;
		}
	}

	public Vector2 PermanentMsgPosition
	{
		get
		{
			return new Vector2(Screen.width / 2, (float)Screen.height * 0.48f + (float)Screen.height * 0.58f * (1f - AutoMonoBehaviour<CameraRectController>.Instance.NormalizedWidth));
		}
	}

	public Vector2 TemporaryMsgPosition
	{
		get
		{
			return new Vector2(Screen.width / 2, (float)Screen.height * 0.5f + (float)Screen.height * 0.6f * (1f - AutoMonoBehaviour<CameraRectController>.Instance.NormalizedWidth));
		}
	}

	public bool TemporaryMsgEnabled
	{
		get
		{
			return _temporaryMsgText.IsVisible;
		}
		set
		{
			if (value)
			{
				_temporaryMsgText.Show();
			}
			else
			{
				_temporaryMsgText.Hide();
			}
		}
	}

	public bool PermanentMsgEnabled
	{
		get
		{
			return _permanentMsgText.IsVisible;
		}
		set
		{
			if (value)
			{
				_permanentMsgText.Show();
			}
			else
			{
				_permanentMsgText.Hide();
			}
		}
	}

	public OnButtonClickedDelegate OnButtonClicked { get; set; }

	public bool ButtonEnabled { get; set; }

	public string ButtonCaption { get; set; }

	private PlayerStateMsgHud()
	{
		_temporaryMsgText = new MeshGUIText(string.Empty, HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		_permanentMsgText = new MeshGUIText(string.Empty, HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		ResetHud();
		TemporaryMsgEnabled = true;
		PermanentMsgEnabled = true;
		CmuneEventHandler.AddListener<OnSetPlayerTeamEvent>(OnTeamChange);
		CmuneEventHandler.AddListener<CameraWidthChangeEvent>(OnCameraRectChange);
	}

	public void Draw()
	{
		if (ButtonEnabled && GameState.CurrentSpace != null && GUITools.Button(new Rect((float)Screen.width * AutoMonoBehaviour<CameraRectController>.Instance.NormalizedWidth * 0.5f - 150f, (float)Screen.height * 0.5f + (float)Screen.height * 0.6f * (1f - AutoMonoBehaviour<CameraRectController>.Instance.NormalizedWidth), 300f, 75f), new GUIContent(ButtonCaption), _buttonGuiStyle) && OnButtonClicked != null)
		{
			OnButtonClicked();
		}
	}

	public void DisplayNone()
	{
		_temporaryMsgText.Text = string.Empty;
		_permanentMsgText.Text = string.Empty;
	}

	public void DisplayRespawnTimeMsg(int remainingSeconds)
	{
		_temporaryMsgText.Text = LocalizedStrings.Respawn + ": " + remainingSeconds;
		_temporaryMsgText.Color = Color.white;
		_temporaryMsgText.Position = TemporaryMsgPosition;
		ButtonEnabled = false;
	}

	public void DisplayClickToRespawnMsg()
	{
		if (ApplicationDataManager.IsMobile)
		{
			_temporaryMsgText.Text = LocalizedStrings.TapToRespawn;
		}
		else
		{
			_temporaryMsgText.Text = LocalizedStrings.ClickToRespawn;
		}
		_temporaryMsgText.Color = Color.white;
		_temporaryMsgText.Position = TemporaryMsgPosition;
		ButtonEnabled = false;
	}

	public void DisplayDisconnectionTimeoutMsg(int remainingSeconds)
	{
		_temporaryMsgText.Show();
		_temporaryMsgText.Text = LocalizedStrings.DisconnectionIn + " " + remainingSeconds;
		_temporaryMsgText.Color = Color.red;
		_temporaryMsgText.Position = TemporaryMsgPosition;
	}

	public void DisplayWaitingForOtherPlayerMsg()
	{
		_permanentMsgText.Text = LocalizedStrings.WaitingForOtherPlayers;
		_permanentMsgText.Color = Color.white;
		_permanentMsgText.Position = PermanentMsgPosition;
	}

	public void DisplaySpectatorFollowingMsg(UberStrike.Realtime.UnitySdk.CharacterInfo info)
	{
		string arg = ((info != null) ? info.PlayerName : LocalizedStrings.Nobody);
		_permanentMsgText.Text = string.Format("{0}\n{1}", LocalizedStrings.Following, arg);
		_permanentMsgText.Color = Color.white;
		_permanentMsgText.Position = PermanentMsgPosition;
	}

	public void DisplaySpectatorModeMsg()
	{
		_permanentMsgText.Text = LocalizedStrings.SpectatorMode;
		_permanentMsgText.Color = Color.white;
		_permanentMsgText.Position = PermanentMsgPosition;
	}

	private void ResetHud()
	{
		ResetStyle();
		ResetTransform();
	}

	private void ResetStyle()
	{
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_temporaryMsgText);
		_temporaryMsgText.Color = Color.white;
		_temporaryMsgText.ShadowColorAnim.Alpha = 0f;
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_permanentMsgText);
		_permanentMsgText.Color = Color.white;
		_permanentMsgText.ShadowColorAnim.Alpha = 0f;
		_buttonGuiStyle = StormFront.ButtonBlue;
	}

	private void OnTeamChange(OnSetPlayerTeamEvent ev)
	{
		switch (ev.TeamId)
		{
		case TeamID.NONE:
		case TeamID.BLUE:
			_buttonGuiStyle = StormFront.ButtonBlue;
			break;
		case TeamID.RED:
			_buttonGuiStyle = StormFront.ButtonRed;
			break;
		}
	}

	private void OnCameraRectChange(CameraWidthChangeEvent ev)
	{
		ResetTransform();
	}

	private void ResetTransform()
	{
		float num = TemporaryMsgHeight / _temporaryMsgText.TextBounds.y;
		_temporaryMsgText.Scale = new Vector2(num, num);
		_temporaryMsgText.Position = TemporaryMsgPosition;
		float num2 = PermanentMsgHeight / _temporaryMsgText.TextBounds.y;
		_permanentMsgText.Scale = new Vector2(num2, num2);
		_permanentMsgText.Position = PermanentMsgPosition;
	}
}
