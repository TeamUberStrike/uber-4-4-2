using UnityEngine;

namespace Facebook
{
	public class IOSFacebookLoader : FB.CompiledFacebookLoader
	{
		protected override IFacebook fb
		{
			get
			{
				// Workaround for FBComponentFactory compatibility issue in Unity 6
				GameObject fbObject = GameObject.Find("FB") ?? new GameObject("FB");
				var iosFacebook = fbObject.GetComponent<IOSFacebook>();
				if (iosFacebook == null)
				{
					iosFacebook = fbObject.AddComponent<IOSFacebook>();
				}
				return iosFacebook;
			}
		}
	}
}
