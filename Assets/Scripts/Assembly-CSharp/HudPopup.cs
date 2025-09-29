using System.Collections.Generic;
using UnityEngine;

public class HudPopup : Singleton<HudPopup>
{
	private class HudMessage
	{
		public IUnityItem Item;

		public string Text;

		public float Time;

		public float Alpha
		{
			get
			{
				return Mathf.Lerp(1f, 0f, UnityEngine.Time.time - Time + 1f);
			}
		}

		public Color Color
		{
			get
			{
				return new Color(1f, 1f, 1f, Alpha);
			}
		}

		public float IconWidth(float height)
		{
			if (Item != null)
			{
				float num = 1f;
				return num * height;
			}
			return 0f;
		}
	}

	private const int ShowTime = 3;

	private const int FadeTime = 1;

	private Queue<HudMessage> queue = new Queue<HudMessage>();

	private HudPopup()
	{
	}

	public void Show(string text, IUnityItem item)
	{
		AutoMonoBehaviour<UnityRuntime>.Instance.OnGui += OnGUI;
		queue.Enqueue(new HudMessage
		{
			Text = text,
			Item = item,
			Time = Time.time + 3f
		});
	}

	private void OnGUI()
	{
		if (queue.Count > 0)
		{
			if (queue.Peek().Time > Time.time)
			{
				HudMessage hudMessage = queue.Peek();
				Vector2 vector = new Vector2(260f, 50f);
				GUI.color = hudMessage.Color;
				GUI.BeginGroup(new Rect(0f, (float)Screen.height * 0.5f, vector.x, vector.y), BlueStonez.window);
				GUI.Label(new Rect(10f, 5f, vector.x - hudMessage.IconWidth(vector.y) - 20f, vector.y - 10f), hudMessage.Text, BlueStonez.label_interparkbold_13pt_left);
				hudMessage.Item.DrawIcon(new Rect(vector.x - hudMessage.IconWidth(vector.y), 0f, hudMessage.IconWidth(vector.y), vector.y));
				GUI.EndGroup();
				GUI.color = Color.white;
			}
			else
			{
				queue.Dequeue();
				if (queue.Count > 0)
				{
					queue.Peek().Time = Time.time + 3f;
				}
			}
		}
		else
		{
			AutoMonoBehaviour<UnityRuntime>.Instance.OnGui -= OnGUI;
		}
	}
}
