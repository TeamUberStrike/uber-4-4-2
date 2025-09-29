using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class EventStreamHud : Singleton<EventStreamHud>
{
	private Animatable2DGroup _textGroup;

	private Animatable2DGroup _entireGroup;

	private MeshGUIQuad _glowBlur;

	private float _textScale;

	private float _vertGapBetweenText;

	private float _horzGapBetweenText;

	private float _maxDisplayTime;

	private int _maxEventNum;

	private float _nextRemoveEventTime;

	public bool Enabled
	{
		get
		{
			return _entireGroup.IsVisible;
		}
		set
		{
			if (value)
			{
				_entireGroup.Show();
			}
			else
			{
				_entireGroup.Hide();
			}
		}
	}

	private EventStreamHud()
	{
		_maxEventNum = 8;
		_maxDisplayTime = 5f;
		_textScale = 0.2f;
		_vertGapBetweenText = 5f;
		_horzGapBetweenText = 5f;
		_glowBlur = new MeshGUIQuad(HudTextures.WhiteBlur128);
		_glowBlur.Color = HudStyleUtility.GLOW_BLUR_BLUE_COLOR;
		_glowBlur.Name = "EventStreamHudGlow";
		_glowBlur.Depth = 1f;
		_textGroup = new Animatable2DGroup();
		_entireGroup = new Animatable2DGroup();
		_entireGroup.Group.Add(_textGroup);
		_entireGroup.Group.Add(_glowBlur);
		Enabled = false;
		CmuneEventHandler.AddListener<OnSetPlayerTeamEvent>(OnTeamChange);
		CmuneEventHandler.AddListener<ScreenResolutionEvent>(OnScreenResolutionChange);
		CmuneEventHandler.AddListener<OnGlobalUIRibbonChanged>(OnGlobalUIRibbonChange);
	}

	public void Draw()
	{
		_entireGroup.Draw();
	}

	public void Update()
	{
		if (_textGroup.Group.Count > 0 && Time.time > _nextRemoveEventTime)
		{
			DequeueEvent();
			ResetTransform();
		}
	}

	public void AddEventText(string subjective, TeamID subTeamId, string verb, string objective = "", TeamID objTeamId = TeamID.NONE)
	{
		if (_textGroup.Group.Count == 0)
		{
			UpdateNextRemoveEventTime();
		}
		if (ReachMaxEventNum())
		{
			DequeueEvent();
		}
		Animatable2DGroup item = GenerateEventText(subjective, subTeamId, verb, objective, objTeamId);
		_textGroup.Group.Add(item);
		ResetTransform();
	}

	public void ClearAllEvents()
	{
		_textGroup.ClearAndFree();
		ResetTransform();
	}

	private void DequeueEvent()
	{
		_textGroup.RemoveAndFree(0);
		UpdateNextRemoveEventTime();
	}

	private bool ReachMaxEventNum()
	{
		return _textGroup.Group.Count == _maxEventNum && _maxEventNum > 0;
	}

	private void UpdateNextRemoveEventTime()
	{
		_nextRemoveEventTime = Time.time + _maxDisplayTime;
	}

	private Animatable2DGroup GenerateEventText(string subjective, TeamID subTeamId, string verb, string objective = "", TeamID objTeamId = TeamID.NONE)
	{
		MeshGUIText meshGUIText = new MeshGUIText(subjective, HudAssets.Instance.InterparkBitmapFont, TextAnchor.UpperRight);
		meshGUIText.NamePrefix = "EventStreamHud";
		MeshGUIText meshGUIText2 = new MeshGUIText(verb, HudAssets.Instance.InterparkBitmapFont, TextAnchor.UpperRight);
		meshGUIText2.NamePrefix = "EventStreamHud";
		MeshGUIText meshGUIText3 = new MeshGUIText(objective, HudAssets.Instance.InterparkBitmapFont, TextAnchor.UpperRight);
		meshGUIText3.NamePrefix = "EventStreamHud";
		SetTextStyleByTeamId(meshGUIText, subTeamId);
		SetTextStyleByTeamId(meshGUIText2, TeamID.NONE);
		SetTextStyleByTeamId(meshGUIText3, objTeamId);
		float num = 0f;
		meshGUIText3.Position = Vector2.zero;
		meshGUIText3.Scale = Vector2.one * _textScale;
		num -= meshGUIText3.Rect.width + _horzGapBetweenText;
		meshGUIText2.Position = new Vector2(num, 0f);
		meshGUIText2.Scale = Vector2.one * _textScale;
		num -= meshGUIText2.Rect.width + _horzGapBetweenText;
		meshGUIText.Position = new Vector2(num, 0f);
		meshGUIText.Scale = Vector2.one * _textScale;
		Animatable2DGroup animatable2DGroup = new Animatable2DGroup();
		animatable2DGroup.Group.Add(meshGUIText);
		animatable2DGroup.Group.Add(meshGUIText2);
		animatable2DGroup.Group.Add(meshGUIText3);
		return animatable2DGroup;
	}

	private void SetTextStyleByTeamId(MeshGUIText text, TeamID teamId)
	{
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(text);
		switch (teamId)
		{
		case TeamID.RED:
			text.BitmapMeshText.ShadowColor = HudStyleUtility.DEFAULT_RED_COLOR;
			break;
		case TeamID.BLUE:
			text.BitmapMeshText.ShadowColor = HudStyleUtility.DEFAULT_BLUE_COLOR;
			break;
		}
	}

	private void OnTeamChange(OnSetPlayerTeamEvent ev)
	{
		if (ev.TeamId == TeamID.RED)
		{
			_glowBlur.Color = HudStyleUtility.GLOW_BLUR_RED_COLOR;
		}
		else
		{
			_glowBlur.Color = HudStyleUtility.GLOW_BLUR_BLUE_COLOR;
		}
	}

	private void OnScreenResolutionChange(ScreenResolutionEvent ev)
	{
		ResetTransform();
	}

	private void OnGlobalUIRibbonChange(OnGlobalUIRibbonChanged ev)
	{
		ResetTransform();
	}

	private void ResetTransform()
	{
		ResetTextGroupTransform();
		ResetBlurTransform();
		_entireGroup.Position = new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.85f);
	}

	private void ResetTextGroupTransform()
	{
		for (int i = 0; i < _textGroup.Group.Count; i++)
		{
			Animatable2DGroup animatable2DGroup = _textGroup.Group[i] as Animatable2DGroup;
			ResetEventTextTransform(animatable2DGroup);
			animatable2DGroup.Position = new Vector2(0f, (float)i * (animatable2DGroup.Rect.height + _vertGapBetweenText));
			animatable2DGroup.UpdateMeshGUIPosition();
		}
	}

	private void ResetEventTextTransform(Animatable2DGroup eventTextGroup)
	{
		MeshGUIText meshGUIText = eventTextGroup.Group[0] as MeshGUIText;
		MeshGUIText meshGUIText2 = eventTextGroup.Group[1] as MeshGUIText;
		MeshGUIText meshGUIText3 = eventTextGroup.Group[2] as MeshGUIText;
		float num = 0f;
		meshGUIText3.Position = Vector2.zero;
		meshGUIText3.Scale = Vector2.one * _textScale;
		num -= meshGUIText3.Rect.width + _horzGapBetweenText;
		meshGUIText2.Position = new Vector2(num, 0f);
		meshGUIText2.Scale = Vector2.one * _textScale;
		meshGUIText2.BitmapMeshText.ShadowColor = new Color(1f, 1f, 1f, 0f);
		num -= meshGUIText2.Rect.width + _horzGapBetweenText;
		meshGUIText.Position = new Vector2(num, 0f);
		meshGUIText.Scale = Vector2.one * _textScale;
	}

	private void ResetBlurTransform()
	{
		float num = _textGroup.Rect.width * HudStyleUtility.BLUR_WIDTH_SCALE_FACTOR * 0.8f;
		float num2 = _textGroup.Rect.height * HudStyleUtility.BLUR_HEIGHT_SCALE_FACTOR;
		_glowBlur.Scale = new Vector2(num / (float)HudTextures.WhiteBlur128.width, num2 / (float)HudTextures.WhiteBlur128.height);
		_glowBlur.Position = new Vector2((0f - num - _textGroup.Rect.width) / 2f, (_textGroup.Rect.height - num2) / 2f);
	}
}
