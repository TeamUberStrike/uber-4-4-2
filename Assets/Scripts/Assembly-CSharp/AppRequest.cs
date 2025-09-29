using System.Collections.Generic;

public class AppRequest
{
	public enum ResultType
	{
		Ok = 0,
		Cancelled = 1,
		Error = 2
	}

	public class ReqResult
	{
		[JsonDeserialize("request")]
		public string ReqId;

		[JsonDeserialize("to")]
		public List<string> To = new List<string>();
	}

	public ResultType Result;

	public bool Ready;

	public ReqResult Response;
}
