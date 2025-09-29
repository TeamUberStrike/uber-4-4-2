public class ClearPlayerPrefs : EditorOnlyMono
{
	public bool ClearAll = true;

	private void OnApplicationQuit()
	{
		if (ClearAll)
		{
			CmunePrefs.Reset();
		}
	}
}
