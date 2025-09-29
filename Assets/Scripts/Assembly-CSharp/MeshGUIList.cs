using System;
using System.Collections;
using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class MeshGUIList
{
	private float _sizeAttenuationFactor;

	private float _alphaAttenuationFactor;

	private float _gapBetweenItems = 1f;

	private float _animTime = 0.2f;

	private int _destIndex;

	private bool _isCircular;

	private int _maxUnilateralSlotCount;

	private int _currentDisplayIndex;

	private Animatable2DGroup _listItemsGroup;

	private MeshGUIQuad _glowBlur;

	private Animatable2DGroup _entireGroup;

	private Animatable2DGroup _currentVisibleGroup;

	private Action _onUpdate;

	private float _scaleFactor;

	private float _itemHeight;

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

	public MeshGUIList(Action onDrawMeshGUIList = null)
	{
		_maxUnilateralSlotCount = 1;
		_onUpdate = onDrawMeshGUIList;
		_listItemsGroup = new Animatable2DGroup();
		_glowBlur = new MeshGUIQuad(HudTextures.WhiteBlur128);
		_glowBlur.Name = "MeshGUIList Glow";
		_glowBlur.Depth = 2f;
		_glowBlur.Color = HudStyleUtility.GLOW_BLUR_BLUE_COLOR;
		_entireGroup = new Animatable2DGroup();
		_entireGroup.Group.Add(_listItemsGroup);
		_entireGroup.Group.Add(_glowBlur);
		_currentVisibleGroup = new Animatable2DGroup();
		_currentDisplayIndex = 0;
		_sizeAttenuationFactor = 0.7f;
		_alphaAttenuationFactor = 0.5f;
		_isCircular = true;
		ResetHud();
		Enabled = false;
		CmuneEventHandler.AddListener<OnSetPlayerTeamEvent>(OnTeamChange);
		CmuneEventHandler.AddListener<ScreenResolutionEvent>(OnScreenResolutionChange);
	}

	public void Draw()
	{
		_entireGroup.Draw();
	}

	public void Update()
	{
		_entireGroup.Draw();
		if (_onUpdate != null)
		{
			_onUpdate();
		}
	}

	public bool HasItem(int index)
	{
		return index >= 0 && index < _listItemsGroup.Group.Count;
	}

	public void SetItemText(int index, string text)
	{
		if (HasItem(index))
		{
			MeshGUIText meshGUIText = _listItemsGroup.Group[index] as MeshGUIText;
			meshGUIText.Text = text;
		}
		StopAnimToIndexCoroutine();
	}

	public void InsertItem(int index, string text)
	{
		MeshGUIText item = CreateListItem(text);
		_listItemsGroup.Group.Insert(index, item);
		StopAnimToIndexCoroutine();
	}

	public void AddItem(string text)
	{
		MeshGUIText item = CreateListItem(text);
		_listItemsGroup.Group.Add(item);
		StopAnimToIndexCoroutine();
	}

	public void RemoveItem(int index)
	{
		_listItemsGroup.RemoveAndFree(index);
		StopAnimToIndexCoroutine();
	}

	public void ClearAllItems()
	{
		_listItemsGroup.ClearAndFree();
		StopAnimToIndexCoroutine();
	}

	public void FadeOut(float time, EaseType easeType)
	{
		_entireGroup.FadeAlphaTo(0f, time, easeType);
	}

	public void AnimUpward()
	{
		StopAnimToIndexCoroutine();
		if (_currentDisplayIndex > 0)
		{
			_currentDisplayIndex--;
			UpdateGroupDisplay(_currentDisplayIndex, _animTime);
		}
		else if (_isCircular)
		{
			_currentDisplayIndex = _listItemsGroup.Group.Count - 1;
			UpdateGroupDisplay(_currentDisplayIndex, _animTime);
		}
	}

	public void AnimDownward()
	{
		StopAnimToIndexCoroutine();
		if (_currentDisplayIndex < _listItemsGroup.Group.Count - 1)
		{
			_currentDisplayIndex++;
			UpdateGroupDisplay(_currentDisplayIndex, _animTime);
		}
		else if (_isCircular)
		{
			_currentDisplayIndex = 0;
			UpdateGroupDisplay(_currentDisplayIndex, _animTime);
		}
	}

	public void AnimToIndex(int destIndex, float time)
	{
		_destIndex = destIndex;
		_animTime = time;
		UpdateGroupDisplay(_currentDisplayIndex);
		MonoRoutine.Start(AnimToIndexCoroutine());
	}

	private void StopAnimToIndexCoroutine()
	{
		Singleton<PreemptiveCoroutineManager>.Instance.IncrementId(AnimToIndexCoroutine);
		UpdateGroupDisplay(_currentDisplayIndex);
	}

	private IEnumerator AnimToIndexCoroutine()
	{
		if (_destIndex == _currentDisplayIndex)
		{
			yield break;
		}
		int coroutineId = Singleton<PreemptiveCoroutineManager>.Instance.IncrementId(AnimToIndexCoroutine);
		int indexDelta = _destIndex - _currentDisplayIndex;
		int absIndexDelta = ((indexDelta <= 0) ? (-indexDelta) : indexDelta);
		int step = ((indexDelta > 0) ? 1 : (-1));
		if (_isCircular && absIndexDelta > _listItemsGroup.Group.Count / 2)
		{
			step = -step;
			absIndexDelta = _listItemsGroup.Group.Count - absIndexDelta;
		}
		float animTime = _animTime / (float)absIndexDelta;
		while (_currentDisplayIndex != _destIndex)
		{
			_currentDisplayIndex += step;
			UpdateGroupDisplay(_currentDisplayIndex, animTime);
			yield return new WaitForSeconds(animTime);
			if (!Singleton<PreemptiveCoroutineManager>.Instance.IsCurrent(AnimToIndexCoroutine, coroutineId))
			{
				break;
			}
			if (_isCircular)
			{
				_currentDisplayIndex = CirculateSpriteIndex(_currentDisplayIndex);
			}
		}
	}

	private void ResetHud()
	{
		ResetStyle();
		ResetTransform();
	}

	private void ResetStyle()
	{
		foreach (IAnimatable2D item in _listItemsGroup.Group)
		{
			MeshGUIText teamStyle = item as MeshGUIText;
			Singleton<HudStyleUtility>.Instance.SetTeamStyle(teamStyle);
		}
	}

	private void OnTeamChange(OnSetPlayerTeamEvent ev)
	{
		ResetStyle();
		if (ev.TeamId == TeamID.RED)
		{
			_glowBlur.Color = HudStyleUtility.GLOW_BLUR_RED_COLOR;
		}
		else
		{
			_glowBlur.Color = HudStyleUtility.GLOW_BLUR_BLUE_COLOR;
		}
		UpdateGroupDisplay(_currentDisplayIndex);
	}

	private void OnScreenResolutionChange(ScreenResolutionEvent ev)
	{
		ResetTransform();
	}

	private void ResetTransform()
	{
		_scaleFactor = 0.45f;
		_itemHeight = (float)Screen.height * 0.055f;
		UpdateGroupDisplay(_currentDisplayIndex);
		_entireGroup.Position = new Vector2(Screen.width / 2, (float)Screen.height * 0.74f);
	}

	private MeshGUIText CreateListItem(string text)
	{
		MeshGUIText meshGUIText = new MeshGUIText(text, HudAssets.Instance.InterparkBitmapFont);
		Singleton<HudStyleUtility>.Instance.SetTeamStyle(meshGUIText);
		meshGUIText.Alpha = 0f;
		if (!Enabled)
		{
			meshGUIText.Hide();
		}
		return meshGUIText;
	}

	private float GetItemYOffset(float itemHeight, int slotIndex)
	{
		float num = 0f;
		if (slotIndex == 0)
		{
			num = (0f - itemHeight) / 2f;
		}
		else if (slotIndex > 0)
		{
			num = itemHeight / 2f + _gapBetweenItems;
			for (int i = 2; i < slotIndex + 1; i++)
			{
				float num2 = itemHeight * Mathf.Pow(_sizeAttenuationFactor, i - 1);
				num += num2 + _gapBetweenItems - (float)i * 2f * _scaleFactor;
			}
		}
		else if (slotIndex < 0)
		{
			num = (0f - itemHeight) / 2f;
			for (int j = 1; j < -slotIndex + 1; j++)
			{
				float num3 = itemHeight * Mathf.Pow(_sizeAttenuationFactor, j);
				num -= num3 + _gapBetweenItems - (float)j * 2f * _scaleFactor;
			}
		}
		return num;
	}

	private void UpdateGroupDisplay(int currentDisplayIndex, float time = 0f)
	{
		ResetListGroupDisplay(currentDisplayIndex, time);
		ResetBlurDisplay();
	}

	private void ResetListGroupDisplay(int currentDisplayIndex, float time = 0f)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < _listItemsGroup.Group.Count; i++)
		{
			int num = i - currentDisplayIndex;
			if (_isCircular)
			{
				if (num < -_maxUnilateralSlotCount)
				{
					num += _listItemsGroup.Group.Count;
				}
				else if (num > _maxUnilateralSlotCount)
				{
					num -= _listItemsGroup.Group.Count;
				}
			}
			list.Add(num);
		}
		for (int j = 0; j < _listItemsGroup.Group.Count; j++)
		{
			MeshGUIText meshGUIText = (MeshGUIText)_listItemsGroup.Group[j];
			int num2 = list[j];
			int num3 = ((num2 <= 0) ? (-num2) : num2);
			float destAlpha;
			if (num2 <= _maxUnilateralSlotCount && num2 >= -_maxUnilateralSlotCount)
			{
				float num4 = Mathf.Pow(_alphaAttenuationFactor, num3);
				destAlpha = 1f * num4;
			}
			else
			{
				destAlpha = 0f;
			}
			float num5 = Mathf.Pow(_sizeAttenuationFactor, num3);
			float num6 = 1f * num5 * _scaleFactor;
			Vector2 destScale = new Vector2(num6, num6);
			Vector2 destPosition = new Vector2((0f - meshGUIText.TextBounds.x) * num6 / 2f, GetItemYOffset(_itemHeight, num2));
			meshGUIText.StopFading();
			meshGUIText.StopMoving();
			meshGUIText.StopScaling();
			meshGUIText.FadeAlphaTo(destAlpha, time, EaseType.Out);
			meshGUIText.MoveTo(destPosition, time, EaseType.Out, 0f);
			meshGUIText.ScaleTo(destScale, time, EaseType.Out);
		}
	}

	private void ResetBlurDisplay()
	{
		_currentVisibleGroup.Group.Clear();
		foreach (IAnimatable2D item in _listItemsGroup.Group)
		{
			MeshGUIText meshGUIText = item as MeshGUIText;
			if (meshGUIText.Alpha > 0f)
			{
				_currentVisibleGroup.Group.Add(meshGUIText);
			}
		}
		float num = _currentVisibleGroup.Rect.width * HudStyleUtility.BLUR_WIDTH_SCALE_FACTOR;
		float num2 = _currentVisibleGroup.Rect.height * HudStyleUtility.BLUR_HEIGHT_SCALE_FACTOR;
		_glowBlur.Scale = new Vector2(num / (float)HudTextures.WhiteBlur128.width, num2 / (float)HudTextures.WhiteBlur128.height);
		_glowBlur.Position = new Vector2((0f - num) / 2f, (0f - num2) / 2f);
		_glowBlur.StopFading();
		_glowBlur.Alpha = 1f;
	}

	private int CirculateSpriteIndex(int index)
	{
		if (index < 0)
		{
			index += _listItemsGroup.Group.Count;
		}
		else if (index >= _listItemsGroup.Group.Count)
		{
			index -= _listItemsGroup.Group.Count;
		}
		return index;
	}
}
