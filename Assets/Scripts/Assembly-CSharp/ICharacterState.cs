using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public interface ICharacterState
{
	UberStrike.Realtime.UnitySdk.CharacterInfo Info { get; }

	Vector3 LastPosition { get; }

	void RecieveDeltaUpdate(SyncObject delta);

	void SubscribeToEvents(CharacterConfig config);

	void UnSubscribeAll();
}
