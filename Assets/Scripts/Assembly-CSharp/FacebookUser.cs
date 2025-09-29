public class FacebookUser
{
	[JsonDeserialize("id")]
	public ulong Id;

	[JsonDeserialize("name")]
	public string Name = string.Empty;

	public string FirstName
	{
		get
		{
			return Name.Split(' ')[0];
		}
	}

	public string Avatar
	{
		get
		{
			return "http://graph.facebook.com/" + Id + "/picture?width=128&height=128";
		}
	}
}
