using UnityEngine;

namespace Facebook
{
	public class EditorFacebookLoader : FB.CompiledFacebookLoader
	{
		protected override IFacebook fb
		{
			get
			{
				// Workaround for FBComponentFactory compatibility issue in Unity 6
				GameObject fbObject = GameObject.Find("FB") ?? new GameObject("FB");
				var editorFacebook = fbObject.GetComponent<EditorFacebook>();
				if (editorFacebook == null)
				{
					editorFacebook = fbObject.AddComponent<EditorFacebook>();
				}
				return editorFacebook;
			}
		}
	}
}
