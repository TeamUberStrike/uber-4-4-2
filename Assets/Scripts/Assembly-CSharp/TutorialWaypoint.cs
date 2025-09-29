using System.Collections;
using UnityEngine;

public class TutorialWaypoint : MonoBehaviour
{
	[SerializeField]
	private Texture ImgWaypoint;

	private Transform _transform;

	private Sprite2D _imgWaypoint;

	private MeshGUIText _txtDistance;

	private MeshGUIText _txtTitle;

	private Vector2 _imgPos;

	private Vector2 _txtPos;

	private Vector2 _disPos;

	[SerializeField]
	private string _text = string.Empty;

	private bool _canShow;

	public bool CanShow
	{
		get
		{
			return _canShow;
		}
		set
		{
			_canShow = value;
			if (_canShow)
			{
				_imgWaypoint.Flicker(0.5f);
				_imgWaypoint.Scale = Vector2.one;
				_imgWaypoint.FadeAlphaTo(1f, 0.5f);
				if (_txtDistance != null)
				{
					_txtDistance.Show();
					_txtDistance.Flicker(0.5f);
					_txtDistance.FadeAlphaTo(1f, 0.5f);
				}
				if (_txtTitle != null)
				{
					_txtTitle.Show();
					_txtTitle.Flicker(0.5f);
					_txtTitle.FadeAlphaTo(1f, 0.5f);
				}
			}
			else
			{
				_imgWaypoint.Flicker(0.5f);
				_imgWaypoint.FadeAlphaTo(0f, 0.5f);
				if (_txtTitle != null)
				{
					_txtTitle.Alpha = 0f;
					_txtTitle.Hide();
				}
				if (_txtDistance != null)
				{
					_txtDistance.Alpha = 0f;
					_txtDistance.Hide();
				}
			}
		}
	}

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
		}
	}

	private void Awake()
	{
		_transform = base.transform;
		_imgWaypoint = new Sprite2D(ImgWaypoint);
		_imgWaypoint.Alpha = 0f;
	}

	private void OnGUI()
	{
		if ((CanShow || _imgWaypoint.Alpha > 0.1f) && (bool)LevelCamera.Instance.MainCamera && GameState.HasCurrentPlayer)
		{
			DrawWaypoint2();
		}
	}

	private void DrawWaypoint2()
	{
		Vector3 screenPos = LevelCamera.Instance.MainCamera.WorldToScreenPoint(_transform.position);
		screenPos.y = (float)Screen.height - screenPos.y;
		bool toLeft = true;
		if (screenPos.z > 0f)
		{
			CalcWaypointPos(screenPos, toLeft, out _txtPos, out _imgPos, out _disPos);
		}
		GUI.depth = 100;
		int num = Mathf.RoundToInt(Vector3.Distance(_transform.position, GameState.LocalCharacter.Position));
		_imgWaypoint.Draw(_imgPos.x, _imgPos.y);
		if (_txtDistance == null)
		{
			if (LevelTutorial.Exists)
			{
				_txtDistance = new MeshGUIText(string.Empty, LevelTutorial.Instance.Font);
				_txtDistance.Scale = new Vector2(0.2f, 0.2f);
				_txtDistance.Color = Color.white;
				_txtDistance.Alpha = 0f;
				Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_txtDistance);
				_txtTitle = new MeshGUIText(Text, LevelTutorial.Instance.Font);
				_txtTitle.Scale = new Vector2(0.2f, 0.2f);
				_txtTitle.Color = Color.white;
				_txtTitle.Alpha = 0f;
				Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_txtTitle);
			}
		}
		else
		{
			_txtDistance.Show();
			_txtDistance.Position = _disPos;
			_txtDistance.Text = num + "M";
			_txtTitle.Show();
			_txtTitle.Position = _txtPos;
		}
	}

	private void CalcWaypointPos(Vector3 screenPos, bool toLeft, out Vector2 textPos, out Vector2 imgPos, out Vector2 distancePos)
	{
		float num = screenPos.x - _imgWaypoint.Size.x / 2f;
		float a = screenPos.x + _imgWaypoint.Size.x / 2f;
		float num2 = screenPos.y - _imgWaypoint.Size.y / 2f;
		float num3 = screenPos.y + _imgWaypoint.Size.y / 2f;
		if (_txtTitle != null)
		{
			num = Mathf.Min(num, screenPos.x - _txtTitle.Size.x / 2f);
			a = screenPos.x + Mathf.Max(_txtTitle.Size.x / 2f, (_txtDistance == null) ? (_imgWaypoint.Size.x / 2f) : (_txtDistance.Size.x + 20f));
			num2 -= _txtTitle.Size.y;
		}
		else
		{
			a = Mathf.Max(a, (_txtDistance == null) ? (_imgWaypoint.Size.x / 2f) : (_txtDistance.Size.x + 50f));
		}
		textPos = new Vector2(num, num2);
		imgPos = new Vector2(Mathf.RoundToInt(screenPos.x - _imgWaypoint.Size.x / 2f), Mathf.RoundToInt(screenPos.y - _imgWaypoint.Size.y / 2f));
		distancePos = imgPos + new Vector2(48f, 0f);
		if (screenPos.z > 0f)
		{
			if (num < 0f)
			{
				imgPos.x -= num;
				textPos.x -= num;
				distancePos.x -= num;
			}
			if (a > (float)Screen.width)
			{
				imgPos.x -= a - (float)Screen.width;
				textPos.x -= a - (float)Screen.width;
				distancePos.x -= a - (float)Screen.width;
			}
		}
		if (num2 < 0f)
		{
			imgPos.y -= num2;
			textPos.y -= num2;
			distancePos.y -= num2;
		}
		if (num3 > (float)Screen.height)
		{
			imgPos.y -= num3 - (float)Screen.height;
			textPos.y -= num3 - (float)Screen.height;
			distancePos.y -= num3 - (float)Screen.height;
		}
	}

	private IEnumerator StartHideMe(float sec)
	{
		yield return new WaitForSeconds(sec);
		_canShow = false;
		if (_txtDistance != null)
		{
			_txtDistance.Hide();
		}
		if (_txtTitle != null)
		{
			_txtTitle.Hide();
		}
	}
}
