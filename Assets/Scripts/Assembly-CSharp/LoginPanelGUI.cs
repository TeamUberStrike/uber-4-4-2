using System.Collections;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class LoginPanelGUI : PanelGuiBase
{
	private enum KeyboardFocus
	{
		Email = 0,
		Password = 1
	}

	private Rect _rect;

	private string _emailAddress = string.Empty;

	private string _password = string.Empty;

	private bool _rememberPassword;

	private float _keyboardOffset;

	private float _targetKeyboardOffset;

	private float _errorAlpha;

	private float _panelAlpha;

	private TouchScreenKeyboard _keyboard;

	private KeyboardFocus _keyboardFocus;

	private float dialogTimer;

	public static string ErrorMessage { get; set; }

	public static bool IsBanned { get; set; }

	private void Start()
	{
		_rememberPassword = CmunePrefs.ReadKey<bool>(CmunePrefs.Key.Player_AutoLogin);
		if (_rememberPassword)
		{
			_password = CmunePrefs.ReadKey<string>(CmunePrefs.Key.Player_Password);
			_emailAddress = CmunePrefs.ReadKey<string>(CmunePrefs.Key.Player_Email);
		}
	}

	public override void Hide()
	{
		base.Hide();
		_errorAlpha = 0f;
		ErrorMessage = string.Empty;
	}

	public override void Show()
	{
		base.Show();
		if (IsBanned)
		{
			ErrorMessage = LocalizedStrings.YourAccountHasBeenBanned;
		}
		if (!string.IsNullOrEmpty(ErrorMessage))
		{
			_errorAlpha = 1f;
		}
		_panelAlpha = 0f;
		_keyboardOffset = 0f;
		_targetKeyboardOffset = 0f;
	}

	private void HideKeyboard()
	{
		if (_keyboard != null)
		{
			_keyboard.active = false;
			_keyboard = null;
		}
	}

	private void Update()
	{
		if (_keyboard != null)
		{
			if (_keyboard.done && !_keyboard.wasCanceled)
			{
				_keyboard = null;
				_targetKeyboardOffset = 0f;
			}
			else if (!_keyboard.active)
			{
				_keyboard = null;
				_targetKeyboardOffset = 0f;
			}
			else
			{
				switch (_keyboardFocus)
				{
				case KeyboardFocus.Email:
					_emailAddress = _keyboard.text;
					break;
				case KeyboardFocus.Password:
					_password = _keyboard.text;
					break;
				}
			}
		}
		if (!string.IsNullOrEmpty(_emailAddress))
		{
			_emailAddress = _emailAddress.Replace("\n", string.Empty).Replace("\t", string.Empty);
		}
		if (!string.IsNullOrEmpty(_password))
		{
			_password = _password.Replace("\n", string.Empty).Replace("\t", string.Empty);
		}
		if (_errorAlpha > 0f)
		{
			_errorAlpha -= Time.deltaTime * 0.1f;
		}
	}

	private void OnGUI()
	{
		_panelAlpha = Mathf.Lerp(_panelAlpha, 1f, Time.deltaTime / 2f);
		GUI.color = new Color(1f, 1f, 1f, _panelAlpha);
		if (Mathf.Abs(_keyboardOffset - _targetKeyboardOffset) > 2f)
		{
			_keyboardOffset = Mathf.Lerp(_keyboardOffset, _targetKeyboardOffset, Time.deltaTime * 4f);
		}
		else
		{
			_keyboardOffset = _targetKeyboardOffset;
		}
		_rect = new Rect((Screen.width - 334) / 2, (float)((Screen.height - 200) / 2) - _keyboardOffset, 334f, 200f);
		DrawLoginPanel();
		if (!string.IsNullOrEmpty(GUI.tooltip))
		{
			Matrix4x4 matrix = GUI.matrix;
			GUI.matrix = Matrix4x4.identity;
			Vector2 vector = BlueStonez.tooltip.CalcSize(new GUIContent(GUI.tooltip));
			Rect position = new Rect(Mathf.Clamp(Event.current.mousePosition.x, 14f, (float)Screen.width - (vector.x + 14f)), Event.current.mousePosition.y + 24f, vector.x, vector.y + 16f);
			GUI.Label(position, GUI.tooltip, BlueStonez.tooltip);
			GUI.matrix = matrix;
		}
		GUI.color = Color.white;
	}

	private void DrawLoginPanel()
	{
		GUI.BeginGroup(_rect, GUIContent.none, BlueStonez.window);
		if (!string.IsNullOrEmpty(_emailAddress) && !string.IsNullOrEmpty(_password) && Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return))
		{
			Login(_emailAddress, _password);
			HideKeyboard();
		}
		GUI.depth = 3;
		GUI.Label(new Rect(0f, 0f, _rect.width, 23f), LocalizedStrings.WelcomeToUS, BlueStonez.tab_strip);
		if (!string.IsNullOrEmpty(ErrorMessage))
		{
			GUI.contentColor = ColorScheme.UberStrikeYellow.SetAlpha(_errorAlpha);
			GUI.Label(new Rect(8f, 30f, _rect.width - 16f, 23f), ErrorMessage, BlueStonez.label_interparkmed_11pt);
			GUI.contentColor = Color.white;
		}
		if (GUI.Button(new Rect(8f, 64f, 220f, 24f), new GUIContent(_emailAddress), BlueStonez.textField))
		{
			TouchScreenKeyboard.hideInput = false;
			_keyboard = TouchScreenKeyboard.Open(_emailAddress, TouchScreenKeyboardType.EmailAddress, false, false, false, false);
			_keyboardFocus = KeyboardFocus.Email;
			_targetKeyboardOffset = 200f;
		}
		string text = string.Empty.PadLeft(_password.Length, '*');
		if (GUI.Button(new Rect(8f, 92f, 220f, 24f), new GUIContent(text), BlueStonez.textField))
		{
			TouchScreenKeyboard.hideInput = false;
			_keyboard = TouchScreenKeyboard.Open(string.Empty, TouchScreenKeyboardType.Default, false, false, true, false);
			_keyboardFocus = KeyboardFocus.Password;
			_targetKeyboardOffset = 200f;
		}
		GUI.color = Color.white.SetAlpha(0.7f);
		GUI.Label(new Rect(8f, 120f, 108f, 24f), GUIContent.none, BlueStonez.buttondark_small);
		_rememberPassword = GUI.Toggle(new Rect(12f, 124f, 100f, 24f), _rememberPassword, LocalizedStrings.RememberMe, BlueStonez.toggle);
		if (GUI.Button(new Rect(120f, 120f, 108f, 24f), new GUIContent(LocalizedStrings.ForgotPassword, LocalizedStrings.TooltipForgotPassword), BlueStonez.buttondark_small))
		{
			HideKeyboard();
			ApplicationDataManager.OpenUrl(string.Empty, "http://www.uberstrike.com/#forgot_password");
		}
		GUI.color = Color.white;
		GUI.enabled = !string.IsNullOrEmpty(_emailAddress) && !string.IsNullOrEmpty(_password);
		if (GUITools.Button(new Rect(236f, 64f, 90f, 52f), new GUIContent(LocalizedStrings.Login), BlueStonez.button_green))
		{
			Login(_emailAddress, _password);
			HideKeyboard();
		}
		GUI.enabled = true;
		GUI.Label(new Rect(8f, 150f, _rect.width - 16f, 8f), GUIContent.none, BlueStonez.horizontal_line_grey95);
		if (GUITools.Button(new Rect(8f, 160f, 152f, 30f), new GUIContent(LocalizedStrings.SignUp, LocalizedStrings.CreateNewAccount), BlueStonez.buttondark_medium))
		{
			Hide();
			HideKeyboard();
			PanelManager.Instance.OpenPanel(PanelType.Signup);
		}
		if (GUITools.Button(new Rect(178f, 160f, 152f, 30f), new GUIContent(string.Empty, LocalizedStrings.TooltipFacebookAccount), BlueStonez.button_fbconnect))
		{
			StartCoroutine(LoginCrt());
		}
		GUI.enabled = true;
		GUI.EndGroup();
	}

	private IEnumerator LoginCrt()
	{
		HideKeyboard();
		Hide();
		yield return StartCoroutine(AutoMonoBehaviour<FacebookInterface>.Instance.Init());
		yield return StartCoroutine(AutoMonoBehaviour<FacebookInterface>.Instance.Login());
		if ((bool)AutoMonoBehaviour<FacebookInterface>.Instance.IsLoggedIn)
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				string accessToken = AutoMonoBehaviour<FacebookInterface>.Instance.AccessToken;
				yield return StartCoroutine(Singleton<AuthenticationManager>.Instance.StartLoginMemberFacebook(accessToken));
			}
		}
		else
		{
			ErrorMessage = "Facebook Login attempt failed!";
			Show();
		}
	}

	public IEnumerator StartCancelDialogTimer()
	{
		if (dialogTimer < 5f)
		{
			dialogTimer = 5f;
		}
		yield break;
	}

	private void Login(string emailAddress, string password)
	{
		CmunePrefs.WriteKey(CmunePrefs.Key.Player_AutoLogin, _rememberPassword);
		if (_rememberPassword)
		{
			CmunePrefs.WriteKey(CmunePrefs.Key.Player_Password, password);
			CmunePrefs.WriteKey(CmunePrefs.Key.Player_Email, emailAddress);
		}
		_errorAlpha = 1f;
		if (string.IsNullOrEmpty(emailAddress))
		{
			ApplicationDataManager.EventsSystem.SendLoginInvalid("email-blank");
			ErrorMessage = LocalizedStrings.EnterYourEmailAddress;
		}
		else if (string.IsNullOrEmpty(password))
		{
			ApplicationDataManager.EventsSystem.SendLoginInvalid("password-blank");
			ErrorMessage = LocalizedStrings.EnterYourPassword;
		}
		else if (!ValidationUtilities.IsValidEmailAddress(emailAddress))
		{
			ApplicationDataManager.EventsSystem.SendLoginInvalid("email-invalid");
			ErrorMessage = LocalizedStrings.EmailAddressIsInvalid;
		}
		else if (!ValidationUtilities.IsValidPassword(password))
		{
			ApplicationDataManager.EventsSystem.SendLoginInvalid("password-invalid");
			ErrorMessage = LocalizedStrings.PasswordIsInvalid;
		}
		else
		{
			Hide();
			MonoRoutine.Start(Singleton<AuthenticationManager>.Instance.StartLoginMemberEmail(emailAddress, password));
		}
	}
}
