public class EventFunction
{
	private EventDelegate _eventDelegate;

	private object[] _args;

	public EventFunction(EventDelegate eventDelegate, params object[] args)
	{
		_eventDelegate = eventDelegate;
		_args = args;
	}

	public void Execute()
	{
		_eventDelegate(_args);
	}

	private void DefaultEventDelegate(params object[] args)
	{
	}
}
