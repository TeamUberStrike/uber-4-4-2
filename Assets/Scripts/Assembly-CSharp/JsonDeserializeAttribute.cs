using System;

public class JsonDeserializeAttribute : Attribute
{
	public string Name;

	public bool Required = true;

	public JsonDeserializeAttribute(string name)
	{
		Name = name;
	}
}
