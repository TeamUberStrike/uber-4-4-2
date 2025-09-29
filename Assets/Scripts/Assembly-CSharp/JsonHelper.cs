using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MiniJSON;

public class JsonHelper
{
	public static T Deserialize<T>(string json) where T : new()
	{
		object json2 = Json.Deserialize(json);
		return DeserializeFromJson<T>(json2);
	}

	public static T DeserializeFromJson<T>(object json) where T : new()
	{
		return (T)DeserializeJson(typeof(T), json);
	}

	private static object DeserializeJson(Type wantedType, object jsonObj)
	{
		if (wantedType == typeof(int))
		{
			return (int)(long)jsonObj;
		}
		if (wantedType == typeof(long))
		{
			return (long)jsonObj;
		}
		if (wantedType == typeof(ulong))
		{
			return ulong.Parse(jsonObj as string);
		}
		if (wantedType == typeof(float))
		{
			return (!(jsonObj is double)) ? ((float)(long)jsonObj) : ((float)(double)jsonObj);
		}
		if (wantedType == typeof(double))
		{
			return (!(jsonObj is double)) ? ((double)(long)jsonObj) : ((double)jsonObj);
		}
		if (wantedType == typeof(string))
		{
			return (string)jsonObj;
		}
		if (wantedType == typeof(DateTime))
		{
			return DateTime.Parse((string)jsonObj);
		}
		if (wantedType.FullName.StartsWith("System.Collections.Generic.List`1"))
		{
			ConstructorInfo constructor = wantedType.GetConstructor(new Type[0]);
			IList list = constructor.Invoke(new object[0]) as IList;
			Type wantedType2 = wantedType.GetGenericArguments()[0];
			{
				foreach (object item in jsonObj as List<object>)
				{
					list.Add(DeserializeJson(wantedType2, item));
				}
				return list;
			}
		}
		ConstructorInfo constructor2 = wantedType.GetConstructor(new Type[0]);
		if (constructor2 == null)
		{
			throw new Exception("Json parser does not support " + wantedType);
		}
		object obj = constructor2.Invoke(new object[0]);
		Dictionary<string, object> dictionary = jsonObj as Dictionary<string, object>;
		foreach (KeyValuePair<JsonDeserializeAttribute, FieldInfo> property in GetProperties(wantedType))
		{
			object value;
			if (dictionary.TryGetValue(property.Key.Name, out value))
			{
				property.Value.SetValue(obj, DeserializeJson(property.Value.FieldType, value));
			}
			else if (property.Key.Required)
			{
				throw new Exception("Json doesn't has field " + property.Key.Name + " for required member " + property.Value.Name + ", " + wantedType);
			}
		}
		return obj;
	}

	private static List<KeyValuePair<JsonDeserializeAttribute, FieldInfo>> GetProperties(Type type)
	{
		List<KeyValuePair<JsonDeserializeAttribute, FieldInfo>> list = new List<KeyValuePair<JsonDeserializeAttribute, FieldInfo>>();
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(JsonDeserializeAttribute), true);
			if (customAttributes != null && customAttributes.Length > 0)
			{
				list.Add(new KeyValuePair<JsonDeserializeAttribute, FieldInfo>(customAttributes[0] as JsonDeserializeAttribute, fieldInfo));
			}
		}
		return list;
	}
}
