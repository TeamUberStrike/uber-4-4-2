using System;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class TouchConsumableChanger : TouchButton
{
	private bool _touchUsed;

	private Vector2 _touchStartPos;

	public float TimeBeforeTap = 0.2f;

	public float SwipeThreshold = 4f;

	private bool _didStartTap;

	public int ConsumablesHeld { get; private set; }

	public AtlasGUIQuad LeftQuad { get; private set; }

	public AtlasGUIQuad RightQuad { get; private set; }

	public override bool Enabled
	{
		get
		{
			return base.Enabled;
		}
		set
		{
			base.Enabled = value;
			if (value)
			{
				CmuneEventHandler.AddListener<CameraWidthChangeEvent>(OnCameraWidthChange);
			}
			else
			{
				CmuneEventHandler.RemoveListener<CameraWidthChangeEvent>(OnCameraWidthChange);
			}
			if (LeftQuad != null)
			{
				LeftQuad.SetVisible(ConsumablesHeld != 0 && value);
			}
			if (RightQuad != null)
			{
				RightQuad.SetVisible(ConsumablesHeld != 0 && value);
			}
		}
	}

	public override Rect Boundary
	{
		get
		{
			return base.Boundary;
		}
		set
		{
			float num = value.y + value.height / 2f;
			float num2 = (float)MobileIcons.TextureAtlas.mainTexture.width * MobileIcons.TouchArrowLeft.width;
			float num3 = (float)MobileIcons.TextureAtlas.mainTexture.height * MobileIcons.TouchArrowLeft.height;
			LeftQuad.Position = new Vector2(value.x - num2, num - num3 / 2f);
			RightQuad.Position = new Vector2(value.xMax, num - num3 / 2f);
			base.Boundary = value;
		}
	}

	public event Action OnNextConsumable;

	public event Action OnPrevConsumable;

	public event Action OnStartUseConsumable;

	public event Action OnEndUseConsumable;

	public TouchConsumableChanger()
	{
		base.OnTouchBegan += TouchConsumableChanger_OnTouchBegan;
		base.OnTouchMoved += TouchConsumableChanger_OnTouchMoved;
		base.OnTouchEnded += TouchConsumableChanger_OnTouchEnded;
		LeftQuad = new AtlasGUIQuad(MobileIcons.TextureAtlas, MobileIcons.TouchArrowLeft);
		LeftQuad.Hide();
		LeftQuad.Name = "ConsumableL";
		RightQuad = new AtlasGUIQuad(MobileIcons.TextureAtlas, MobileIcons.TouchArrowRight);
		RightQuad.Hide();
		RightQuad.Name = "ConsumableR";
		ConsumablesHeld = 0;
	}

	public void UpdateConsumablesHeld()
	{
		ConsumablesHeld = 0;
		if (Singleton<LoadoutManager>.Instance.HasItemInSlot(LoadoutSlotType.QuickUseItem1))
		{
			ConsumablesHeld++;
		}
		if (Singleton<LoadoutManager>.Instance.HasItemInSlot(LoadoutSlotType.QuickUseItem2))
		{
			ConsumablesHeld++;
		}
		if (Singleton<LoadoutManager>.Instance.HasItemInSlot(LoadoutSlotType.QuickUseItem3))
		{
			ConsumablesHeld++;
		}
		LeftQuad.SetVisible(ConsumablesHeld != 0);
		RightQuad.SetVisible(ConsumablesHeld != 0);
	}

	private void OnCameraWidthChange(CameraWidthChangeEvent ev)
	{
		LeftQuad.Position = LeftQuad.Position;
		RightQuad.Position = RightQuad.Position;
	}

	private void TouchConsumableChanger_OnTouchEnded(Vector2 obj)
	{
		if (ConsumablesHeld == 0)
		{
			return;
		}
		if (!_touchUsed)
		{
			if (this.OnStartUseConsumable != null)
			{
				this.OnStartUseConsumable();
			}
			_touchUsed = true;
			_didStartTap = true;
		}
		if (_didStartTap && this.OnEndUseConsumable != null)
		{
			this.OnEndUseConsumable();
		}
	}

	private void TouchConsumableChanger_OnTouchMoved(Vector2 pos, Vector2 delta)
	{
		if (_touchUsed || ConsumablesHeld == 0)
		{
			return;
		}
		if (finger.StartTouchTime + TimeBeforeTap < Time.time)
		{
			if (this.OnStartUseConsumable != null)
			{
				this.OnStartUseConsumable();
			}
			_touchUsed = true;
			_didStartTap = true;
		}
		else
		{
			if (ConsumablesHeld <= 1)
			{
				return;
			}
			if (_touchStartPos.x - pos.x > SwipeThreshold)
			{
				_touchUsed = true;
				if (this.OnPrevConsumable != null)
				{
					this.OnPrevConsumable();
				}
			}
			else if (_touchStartPos.x - pos.x < 0f - SwipeThreshold)
			{
				_touchUsed = true;
				if (this.OnNextConsumable != null)
				{
					this.OnNextConsumable();
				}
			}
		}
	}

	private void TouchConsumableChanger_OnTouchBegan(Vector2 obj)
	{
		if (ConsumablesHeld != 0)
		{
			_touchStartPos = obj;
			_touchUsed = false;
			_didStartTap = false;
		}
	}
}
