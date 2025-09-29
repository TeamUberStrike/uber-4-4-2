using System;
using UberStrike.Core.Types;
using UnityEngine;

public class TouchWeaponChanger : TouchButton
{
	private Rect[] weapons;

	private AtlasGUIQuad _quad;

	private AtlasGUIQuad _incomingQuad;

	private AtlasGUIQuad _prevQuad;

	private AtlasGUIQuad _nextQuad;

	private float _startWeaponSwitch;

	private Vector2 _touchStartPos;

	private bool _touchUsed;

	private UberstrikeItemClass _currWeaponClass;

	private bool _moveLeft = true;

	public float WeaponSwitchTime = 0.3f;

	public float SwipeThreshold = 4f;

	private Vector2 _position;

	public Vector2 Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
			Boundary = new Rect(_position.x - _quad.Width / 2f, _position.y - _quad.Height / 2f, _quad.Width, _quad.Height);
			_quad.Position = new Vector2(Boundary.x, Boundary.y);
			_prevQuad.Position = new Vector2(Boundary.x - _prevQuad.Width, Boundary.y + Boundary.height / 2f - _prevQuad.Height / 2f);
			_nextQuad.Position = new Vector2(Boundary.xMax, Boundary.y + Boundary.height / 2f - _nextQuad.Height / 2f);
		}
	}

	public override bool Enabled
	{
		get
		{
			return base.Enabled;
		}
		set
		{
			base.Enabled = value;
			if (_prevQuad != null)
			{
				_prevQuad.SetVisible(value);
			}
			if (_nextQuad != null)
			{
				_nextQuad.SetVisible(value);
			}
			if (_quad != null)
			{
				_quad.SetVisible(value);
			}
			if (!base.Enabled)
			{
				if (_incomingQuad != null)
				{
					_incomingQuad.FreeObject();
					_incomingQuad = null;
				}
			}
			else
			{
				Start();
			}
		}
	}

	public event Action OnNextWeapon;

	public event Action OnPrevWeapon;

	public TouchWeaponChanger(Rect[] weaponRects)
	{
		weapons = weaponRects;
		_position = Vector2.zero;
		_prevQuad = new AtlasGUIQuad(MobileIcons.TextureAtlas, MobileIcons.TouchArrowLeft);
		_prevQuad.Hide();
		_prevQuad.Name = "WeaponChangerL";
		_nextQuad = new AtlasGUIQuad(MobileIcons.TextureAtlas, MobileIcons.TouchArrowRight);
		_nextQuad.Hide();
		_nextQuad.Name = "WeaponChangerR";
		_quad = new AtlasGUIQuad(MobileIcons.TextureAtlas, weaponRects[0]);
		_quad.Hide();
		_quad.Name = "WeaponChanger";
		base.OnTouchBegan += TouchWeaponChanger_OnTouchBegan;
		base.OnTouchMoved += TouchWeaponChanger_OnTouchMoved;
		base.OnTouchEnded += TouchWeaponChanger_OnTouchEnded;
	}

	private void TouchWeaponChanger_OnTouchEnded(Vector2 obj)
	{
		if (!_touchUsed)
		{
			if (this.OnNextWeapon != null)
			{
				this.OnNextWeapon();
			}
			GenerateNewQuad(Singleton<WeaponController>.Instance.GetCurrentWeapon().View.ItemClass, false);
		}
		_touchUsed = true;
	}

	private void TouchWeaponChanger_OnTouchMoved(Vector2 pos, Vector2 delta)
	{
		if (_touchUsed)
		{
			return;
		}
		if (_touchStartPos.x - pos.x > SwipeThreshold)
		{
			_touchUsed = true;
			if (this.OnPrevWeapon != null)
			{
				this.OnPrevWeapon();
			}
			GenerateNewQuad(Singleton<WeaponController>.Instance.GetCurrentWeapon().View.ItemClass);
		}
		else if (_touchStartPos.x - pos.x < 0f - SwipeThreshold)
		{
			_touchUsed = true;
			if (this.OnNextWeapon != null)
			{
				this.OnNextWeapon();
			}
			GenerateNewQuad(Singleton<WeaponController>.Instance.GetCurrentWeapon().View.ItemClass, false);
		}
	}

	private void TouchWeaponChanger_OnTouchBegan(Vector2 obj)
	{
		_touchStartPos = obj;
		_touchUsed = false;
	}

	protected void Start()
	{
		if (_quad != null)
		{
			_quad.FreeObject();
		}
		if (_incomingQuad != null)
		{
			_incomingQuad.FreeObject();
			_incomingQuad = null;
		}
		WeaponSlot currentWeapon = Singleton<WeaponController>.Instance.GetCurrentWeapon();
		if (currentWeapon != null)
		{
			_quad = new AtlasGUIQuad(MobileIcons.TextureAtlas, weapons[(int)currentWeapon.View.ItemClass]);
			_quad.Name = "WeaponChanger";
			_quad.Position = new Vector2(Boundary.x, Boundary.y);
		}
		_startWeaponSwitch = 0f;
	}

	public void CheckWeaponChanged()
	{
		WeaponSlot currentWeapon = Singleton<WeaponController>.Instance.GetCurrentWeapon();
		if (currentWeapon != null)
		{
			UberstrikeItemClass itemClass = Singleton<WeaponController>.Instance.GetCurrentWeapon().View.ItemClass;
			if (itemClass != _currWeaponClass)
			{
				GenerateNewQuad(itemClass);
			}
		}
	}

	private void GenerateNewQuad(UberstrikeItemClass weaponClass, bool moveLeft = true)
	{
		_currWeaponClass = weaponClass;
		if (_incomingQuad != null)
		{
			_quad.FreeObject();
			_quad = _incomingQuad;
		}
		_incomingQuad = new AtlasGUIQuad(MobileIcons.TextureAtlas, weapons[(int)weaponClass]);
		_incomingQuad.Name = "WeaponChanger";
		_incomingQuad.Position = new Vector2(Boundary.x + _quad.Width, Boundary.y);
		_incomingQuad.Alpha = 0f;
		_startWeaponSwitch = Time.time;
		_moveLeft = moveLeft;
	}

	public override void FinalUpdate()
	{
		base.FinalUpdate();
		float num = ((!_moveLeft) ? (0f - _quad.Width) : _quad.Width);
		if (_startWeaponSwitch + WeaponSwitchTime > Time.time)
		{
			float num2 = (Time.time - _startWeaponSwitch) / WeaponSwitchTime;
			_incomingQuad.Position = new Vector2(Boundary.x + num * (1f - num2), Boundary.y);
			_incomingQuad.Alpha = Mathf.Lerp(-1f, 1f, num2);
			_quad.Position = new Vector2(Boundary.x - num * num2, Boundary.y);
			_quad.Alpha = Mathf.Lerp(1f, -1f, num2);
		}
		else if (_incomingQuad != null)
		{
			_incomingQuad.Position = new Vector2(Boundary.x, Boundary.y);
			_incomingQuad.Alpha = 1f;
			if (_quad != null)
			{
				_quad.FreeObject();
			}
			_quad = _incomingQuad;
			_incomingQuad = null;
		}
	}

	public void Reset()
	{
		_currWeaponClass = (UberstrikeItemClass)0;
	}
}
