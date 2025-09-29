using System;
using System.Text;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class QuickItemBehaviour
{
	private enum States
	{
		CoolingDown = 1,
		Focused = 2
	}

	private class CoolingDownState : IState
	{
		private QuickItemBehaviour behaviour;

		public float TimeOut { get; private set; }

		public CoolingDownState(QuickItemBehaviour behaviour)
		{
			this.behaviour = behaviour;
		}

		public void OnEnter()
		{
			TimeOut = Time.time + (float)behaviour._item.Configuration.CoolDownTime / 1000f;
		}

		public void OnExit()
		{
		}

		public void OnGUI()
		{
		}

		public void OnUpdate()
		{
			if (TimeOut < Time.time)
			{
				behaviour._machine.PopState();
			}
		}
	}

	private class FocusedState : IState
	{
		private QuickItemBehaviour behaviour;

		private float _originalStopSpeed;

		public float TimeOut { get; private set; }

		public FocusedState(QuickItemBehaviour behaviour)
		{
			this.behaviour = behaviour;
		}

		public void OnEnter()
		{
			TimeOut = Time.time + (float)behaviour._item.Configuration.WarmUpTime / 1000f;
			Singleton<WeaponController>.Instance.IsEnabled = false;
			Singleton<QuickItemController>.Instance.IsCharging = true;
			Singleton<WeaponController>.Instance.PutdownCurrentWeapon();
			Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag &= ~HudDrawFlags.Reticle;
			GameState.LocalPlayer.MoveController.IsJumpDisabled = true;
			_originalStopSpeed = LevelEnviroment.Instance.Settings.StopSpeed;
			LevelEnviroment.Instance.Settings.StopSpeed = _originalStopSpeed * behaviour._item.Configuration.SlowdownOnCharge;
		}

		public void OnExit()
		{
			TimeOut = 0f;
			Singleton<WeaponController>.Instance.IsEnabled = true;
			Singleton<QuickItemController>.Instance.IsCharging = false;
			Singleton<WeaponController>.Instance.PickupCurrentWeapon();
			Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag |= HudDrawFlags.Reticle;
			GameState.LocalPlayer.MoveController.IsJumpDisabled = false;
			LevelEnviroment.Instance.Settings.StopSpeed = _originalStopSpeed;
		}

		public void OnUpdate()
		{
			if (TimeOut < Time.time)
			{
				behaviour._machine.PopState();
				behaviour.Activate();
			}
			else if (!AutoMonoBehaviour<InputManager>.Instance.IsDown(behaviour.FocusKey) && !AutoMonoBehaviour<InputManager>.Instance.IsDown(GameInputKey.UseQuickItem) && !Singleton<QuickItemController>.Instance.IsQuickItemMobilePushed)
			{
				behaviour._machine.PopState();
			}
		}

		public void OnGUI()
		{
		}
	}

	private StateMachine _machine;

	private CoolingDownState _coolDownState;

	private FocusedState _focusedState;

	private QuickItem _item;

	private float _chargeTimeOut;

	public Action OnActivated;

	public float CoolDownTimeRemaining
	{
		get
		{
			return Mathf.Max(_coolDownState.TimeOut - Time.time, 0f);
		}
	}

	public float CoolDownTimeTotal
	{
		get
		{
			return (float)_item.Configuration.CoolDownTime / 1000f;
		}
	}

	public float FocusTimeRemaining
	{
		get
		{
			return Mathf.Max(_focusedState.TimeOut - Time.time, 0f);
		}
	}

	public float FocusTimeTotal
	{
		get
		{
			return (float)_item.Configuration.WarmUpTime / 1000f;
		}
	}

	public float ChargingTimeRemaining
	{
		get
		{
			return Mathf.Max(_chargeTimeOut - Time.time, 0f);
		}
	}

	public float ChargingTimeTotal
	{
		get
		{
			return (float)_item.Configuration.RechargeTime / 1000f;
		}
	}

	public int CurrentAmount { get; set; }

	public GameInputKey FocusKey { get; set; }

	public bool IsBusy
	{
		get
		{
			return _machine.IsRunning;
		}
	}

	public QuickItemBehaviour(QuickItem item, Action onActivated)
	{
		_item = item;
		OnActivated = onActivated;
		_machine = new StateMachine();
		_coolDownState = new CoolingDownState(this);
		_focusedState = new FocusedState(this);
		_machine.RegisterState(1, _coolDownState);
		_machine.RegisterState(2, _focusedState);
	}

	private void Activate()
	{
		if (CurrentAmount == _item.Configuration.AmountRemaining)
		{
			_chargeTimeOut = Time.time + (float)_item.Configuration.RechargeTime / 1000f;
		}
		if (_item.Configuration.CoolDownTime > 0)
		{
			_machine.PushState(1);
		}
		CurrentAmount--;
		CmuneEventHandler.Route(new QuickItemAmountChangedEvent());
		if (OnActivated != null)
		{
			OnActivated();
		}
	}

	public bool Run()
	{
		if (CurrentAmount > 0 && !_machine.IsRunning)
		{
			AutoMonoBehaviour<MonoRoutine>.Instance.OnUpdateEvent += Update;
			if (_item.Configuration.WarmUpTime > 0)
			{
				_machine.PushState(2);
			}
			else
			{
				Activate();
			}
			return true;
		}
		return false;
	}

	private void Update()
	{
		_machine.Update();
		if (_item.Configuration.RechargeTime > 0 && _chargeTimeOut < Time.time && CurrentAmount < _item.Configuration.AmountRemaining)
		{
			CurrentAmount = Mathf.Min(CurrentAmount + 1, _item.Configuration.AmountRemaining);
			CmuneEventHandler.Route(new QuickItemAmountChangedEvent());
			if (CurrentAmount < _item.Configuration.AmountRemaining)
			{
				_chargeTimeOut = Time.time + (float)_item.Configuration.RechargeTime / 1000f;
			}
		}
		if (!_machine.IsRunning && CurrentAmount == _item.Configuration.AmountRemaining)
		{
			AutoMonoBehaviour<MonoRoutine>.Instance.OnUpdateEvent -= Update;
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Name: " + _item.Configuration.Name);
		stringBuilder.AppendLine("IsBusy: " + IsBusy);
		stringBuilder.AppendLine("State: " + _machine.CurrentStateId);
		stringBuilder.AppendLine("Amount Current: " + CurrentAmount);
		stringBuilder.AppendLine("Amount Total: " + _item.Configuration.AmountRemaining);
		stringBuilder.AppendLine("Time: " + CoolDownTimeRemaining.ToString("F2") + " || " + ChargingTimeRemaining.ToString("F2"));
		return stringBuilder.ToString();
	}
}
