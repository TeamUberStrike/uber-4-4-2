using UberStrike.Realtime.UnitySdk;

public interface IInputChannel : IByteArray
{
	InputChannelType ChannelType { get; }

	string Name { get; }

	bool IsChanged { get; }

	float Value { get; }

	float RawValue();

	void Listen();

	void Reset();
}
