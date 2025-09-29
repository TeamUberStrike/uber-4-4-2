using System;

public class UnityRuntime : AutoMonoBehaviour<UnityRuntime>
{
	public event Action OnGui;

	public event Action OnUpdate;

	public event Action OnFixedUpdate;

	public event Action<bool> OnAppFocus;

	private void FixedUpdate()
	{
		if (this.OnFixedUpdate != null)
		{
			this.OnFixedUpdate();
		}
	}

	private void Update()
	{
		if (this.OnUpdate != null)
		{
			this.OnUpdate();
		}
	}

	private void OnGUI()
	{
		if (this.OnGui != null)
		{
			this.OnGui();
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (this.OnAppFocus != null)
		{
			this.OnAppFocus(focus);
		}
	}
}
