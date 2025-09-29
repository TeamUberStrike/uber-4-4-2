using Cmune.DataCenter.Common.Entities;

public class LoginEvent
{
	public MemberAccessLevel AccessLevel { get; private set; }

	public LoginEvent(MemberAccessLevel level)
	{
		AccessLevel = level;
	}
}
