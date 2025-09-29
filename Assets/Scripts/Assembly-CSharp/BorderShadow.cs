using Cmune.DataCenter.Common.Entities;
using UnityEngine;

public class BorderShadow : MonoBehaviour
{
	[SerializeField]
	private Texture2D _pageShadowLeft;

	[SerializeField]
	private Texture2D _pageShadowRight;

	private void OnGUI()
	{
		if (!Screen.fullScreen && (ApplicationDataManager.Channel == ChannelType.WebPortal || ApplicationDataManager.Channel == ChannelType.WebFacebook || ApplicationDataManager.Channel == ChannelType.Kongregate))
		{
			GUI.DrawTexture(new Rect(0f, 0f, 4f, Screen.height), _pageShadowLeft);
			GUI.DrawTexture(new Rect(Screen.width - 4, 0f, 4f, Screen.height), _pageShadowRight);
		}
	}
}
