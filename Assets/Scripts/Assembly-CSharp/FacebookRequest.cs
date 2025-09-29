using System;

public class FacebookRequest
{
	[JsonDeserialize("id")]
	public string Id = string.Empty;

	[JsonDeserialize("from")]
	public FacebookUser User = new FacebookUser();

	[JsonDeserialize("message")]
	public string Message = string.Empty;

	[JsonDeserialize("created_time")]
	public DateTime Date;

	[JsonDeserialize("data", Required = false)]
	public string Data = string.Empty;
}
