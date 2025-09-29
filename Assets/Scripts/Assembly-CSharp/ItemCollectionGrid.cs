using System.Collections.Generic;
using UnityEngine;

public class ItemCollectionGrid
{
	private class GridItem
	{
		public GameObject Object { get; set; }

		public Renderer Renderer { get; set; }

		public float YOffset { get; set; }
	}

	private const int GearWidth = 200;

	private string _filter = string.Empty;

	private Vector2 _scroll;

	private ItemCollection _items;

	private Dictionary<string, GridItem> _objects;

	public ItemCollectionGrid(ItemCollection items)
	{
		_items = items;
		_objects = new Dictionary<string, GridItem>(items.GetCount());
		foreach (GearItem gear in items.Gears)
		{
			GridItem gridItem = new GridItem();
			gridItem.Object = Object.Instantiate(gear.gameObject) as GameObject;
			gridItem.Renderer = gridItem.Object.GetComponentInChildren<SkinnedMeshRenderer>();
			float num = 200f * Camera.main.orthographicSize * 2f / (float)Screen.width;
			float num2 = num / Mathf.Max(gridItem.Renderer.bounds.size.x, gridItem.Renderer.bounds.size.y, gridItem.Renderer.bounds.size.z);
			gridItem.Object.transform.localScale = Vector3.one * num2;
			gridItem.YOffset = gridItem.Renderer.bounds.center.y;
			_objects[gear.name] = gridItem;
		}
		foreach (HoloGearItem holo in items.Holos)
		{
			GridItem gridItem2 = new GridItem();
			gridItem2.Object = Object.Instantiate(holo.Configuration.Avatar.gameObject) as GameObject;
			gridItem2.Renderer = gridItem2.Object.GetComponentInChildren<SkinnedMeshRenderer>();
			float num3 = 200f * Camera.main.orthographicSize * 2f / (float)Screen.width;
			float num4 = num3 / Mathf.Max(gridItem2.Renderer.bounds.size.x, gridItem2.Renderer.bounds.size.y, gridItem2.Renderer.bounds.size.z);
			gridItem2.Object.transform.localScale = Vector3.one * num4;
			gridItem2.YOffset = gridItem2.Renderer.bounds.center.y;
			_objects[holo.name] = gridItem2;
		}
		foreach (WeaponItem weapon in items.Weapons)
		{
			GridItem gridItem3 = new GridItem();
			Bounds bounds = default(Bounds);
			gridItem3.Object = Object.Instantiate(weapon.gameObject) as GameObject;
			Renderer[] componentsInChildren = gridItem3.Object.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				bounds.Encapsulate(renderer.bounds);
			}
			float num5 = 200f * Camera.main.orthographicSize * 2f / (float)Screen.width;
			float num6 = num5 / Mathf.Clamp(Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z), 0.1f, 1f);
			gridItem3.Object.transform.localRotation = Quaternion.LookRotation(Vector3.left);
			gridItem3.Object.transform.localScale = Vector3.one * num6;
			_objects[weapon.name] = gridItem3;
		}
	}

	public void SetFilter(string filter)
	{
		if (!filter.Equals(_filter))
		{
			_filter = filter;
		}
	}

	public void Draw(Rect rect)
	{
		_scroll = GUI.BeginScrollView(rect, _scroll, new Rect(0f, 0f, rect.width, 200 * _objects.Count));
		int num = 0;
		foreach (GearItem gear in _items.Gears)
		{
			GridItem value2;
			if (string.IsNullOrEmpty(_filter) || gear.name.ToLower().Contains(_filter.ToLower()))
			{
				GUI.Label(new Rect(48f, num * 200, 152f, 20f), gear.name);
				Vector2 vector = GUIUtility.GUIToScreenPoint(new Vector3(0f, num * 200) + Vector3.one * 200f * 0.5f);
				Vector3 vector2 = Camera.main.ScreenToWorldPoint(new Vector3(vector.x, (float)Screen.height - vector.y, 0f));
				GridItem value;
				if (_objects.TryGetValue(gear.name, out value))
				{
					value.Object.SetActive(true);
					value.Object.transform.position = vector2 + new Vector3(0f, 0f - value.YOffset, -1f);
				}
				num++;
			}
			else if (_objects.TryGetValue(gear.name, out value2))
			{
				value2.Object.SetActive(false);
			}
		}
		foreach (HoloGearItem holo in _items.Holos)
		{
			GridItem value4;
			if (string.IsNullOrEmpty(_filter) || holo.name.ToLower().Contains(_filter.ToLower()))
			{
				GUI.Label(new Rect(48f, num * 200, 152f, 20f), holo.name);
				Vector2 vector3 = GUIUtility.GUIToScreenPoint(new Vector3(0f, num * 200) + Vector3.one * 200f * 0.5f);
				Vector3 vector4 = Camera.main.ScreenToWorldPoint(new Vector3(vector3.x, (float)Screen.height - vector3.y, 0f));
				GridItem value3;
				if (_objects.TryGetValue(holo.name, out value3))
				{
					value3.Object.SetActive(true);
					value3.Object.transform.position = vector4 + new Vector3(0f, 0f - value3.YOffset, -1f);
				}
				num++;
			}
			else if (_objects.TryGetValue(holo.name, out value4))
			{
				value4.Object.SetActive(false);
			}
		}
		foreach (WeaponItem weapon in _items.Weapons)
		{
			GridItem value6;
			if (string.IsNullOrEmpty(_filter) || weapon.name.ToLower().Contains(_filter.ToLower()))
			{
				GUI.Label(new Rect(48f, num * 200, 152f, 20f), weapon.name);
				Vector2 vector5 = GUIUtility.GUIToScreenPoint(new Vector3(0f, num * 200) + Vector3.one * 200f * 0.5f);
				Vector3 vector6 = Camera.main.ScreenToWorldPoint(new Vector3(vector5.x, (float)Screen.height - vector5.y, 0f));
				GridItem value5;
				if (_objects.TryGetValue(weapon.name, out value5))
				{
					value5.Object.SetActive(true);
					value5.Object.transform.position = vector6 + new Vector3(0f, 0f - value5.YOffset, -1f);
				}
				num++;
			}
			else if (_objects.TryGetValue(weapon.name, out value6))
			{
				value6.Object.SetActive(false);
			}
		}
		GUI.EndGroup();
	}

	public void Dispose()
	{
		List<GridItem> list = new List<GridItem>(_objects.Values);
		_objects.Clear();
		foreach (GridItem item in list)
		{
			Object.Destroy(item.Object);
		}
	}
}
