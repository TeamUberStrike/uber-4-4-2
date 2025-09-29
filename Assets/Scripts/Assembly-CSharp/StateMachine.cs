using System;
using System.Collections.Generic;

public class StateMachine
{
	private Dictionary<int, IState> _statesGroup;

	private Stack<int> _curStates;

	public int CurrentStateId
	{
		get
		{
			return (_curStates.Count <= 0) ? (-1) : _curStates.Peek();
		}
	}

	public bool IsRunning
	{
		get
		{
			return _curStates.Count > 0;
		}
	}

	private IState CurrentState
	{
		get
		{
			return GetState(CurrentStateId);
		}
	}

	public StateMachine()
	{
		_statesGroup = new Dictionary<int, IState>();
		_curStates = new Stack<int>();
	}

	public void SetState(int stateId)
	{
		if (ContainsState(stateId))
		{
			PopAllStates();
			_curStates.Push(stateId);
			GetState(stateId).OnEnter();
			return;
		}
		throw new Exception("ChangeState failed - cannot found state [" + stateId + "] from state machine");
	}

	public void PushState(int stateId)
	{
		if (ContainsState(stateId))
		{
			_curStates.Push(stateId);
			GetState(stateId).OnEnter();
			return;
		}
		throw new Exception("PushState failed - cannot found state[" + stateId + "] from state machine");
	}

	public void PopState()
	{
		if (_curStates.Count != 0)
		{
			CurrentState.OnExit();
			_curStates.Pop();
		}
	}

	public void PopAllStates()
	{
		while (_curStates.Count > 0)
		{
			PopState();
		}
	}

	public void RegisterState(int stateId, IState state)
	{
		if (!_statesGroup.ContainsKey(stateId))
		{
			_statesGroup.Add(stateId, state);
			return;
		}
		throw new Exception("StateMachine::RegisterState - state [" + stateId + "] already exists in the current registry");
	}

	public bool ContainsState(int stateId)
	{
		return _statesGroup.ContainsKey(stateId);
	}

	public void Update()
	{
		if (_curStates.Count > 0)
		{
			CurrentState.OnUpdate();
		}
	}

	public void OnGUI()
	{
		if (_curStates.Count > 0)
		{
			CurrentState.OnGUI();
		}
	}

	public IState GetState(int stateId)
	{
		IState value = null;
		_statesGroup.TryGetValue(stateId, out value);
		return value;
	}
}
