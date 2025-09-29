using System;
using UnityEngine;

[Serializable]
public class AnimationInfo
{
	public AnimationState State;

	public string Name;

	public AnimationIndex Index;

	public float EndTime;

	public float CurrentTimePlayed;

	public float Speed = 1f;

	public AnimationInfo(AnimationIndex idx, AnimationState state)
	{
		State = state;
		Name = Enum.GetName(typeof(AnimationIndex), idx);
		Index = idx;
	}
}
