using UnityEngine;

namespace Facebook
{
	public class AndroidFacebookLoader : FB.CompiledFacebookLoader
	{
		protected override IFacebook fb
		{
			get
			{
				// Workaround for FBComponentFactory compatibility issue in Unity 6
				GameObject fbObject = GameObject.Find("FB") ?? new GameObject("FB");
				var androidFacebook = fbObject.GetComponent<AndroidFacebook>();
				if (androidFacebook == null)
				{
					androidFacebook = fbObject.AddComponent<AndroidFacebook>();
				}
				return androidFacebook;
			}
		}
	}
}
