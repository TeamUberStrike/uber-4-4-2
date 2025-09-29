using System.Collections.Generic;

internal class HudDrawFlagGroup : Singleton<HudDrawFlagGroup>
{
	private const HudDrawFlags screenshotDrawFlagTuning = HudDrawFlags.None;

	private const HudDrawFlags tabScreenDrawFlagsTuning = HudDrawFlags.Reticle;

	private HudDrawFlags _baseDrawFlag;

	private HashSet<HudDrawFlags> _drawFlagTunings;

	private bool _isScreenShotMode;

	public HudDrawFlags BaseDrawFlag
	{
		get
		{
			return _baseDrawFlag;
		}
		set
		{
			_baseDrawFlag = value;
			UpdateDrawFlags();
		}
	}

	public bool IsScreenshotMode
	{
		get
		{
			return _isScreenShotMode;
		}
		set
		{
			_isScreenShotMode = value;
			if (value)
			{
				_drawFlagTunings.Add(HudDrawFlags.None);
			}
			else
			{
				_drawFlagTunings.Remove(HudDrawFlags.None);
			}
			UpdateDrawFlags();
		}
	}

	public bool TuningTabScreen
	{
		set
		{
			if (value)
			{
				_drawFlagTunings.Add(HudDrawFlags.Reticle);
			}
			else
			{
				_drawFlagTunings.Remove(HudDrawFlags.Reticle);
			}
			UpdateDrawFlags();
		}
	}

	private HudDrawFlagGroup()
	{
		_drawFlagTunings = new HashSet<HudDrawFlags>();
	}

	public void AddFlag(HudDrawFlags drawFlag)
	{
		_drawFlagTunings.Add(drawFlag);
		UpdateDrawFlags();
	}

	public void RemoveFlag(HudDrawFlags drawFlag)
	{
		if (_drawFlagTunings.Contains(drawFlag))
		{
			_drawFlagTunings.Remove(drawFlag);
			UpdateDrawFlags();
		}
	}

	public void ClearFlags()
	{
		_drawFlagTunings.Clear();
		UpdateDrawFlags();
	}

	public HudDrawFlags GetConsolidatedFlag()
	{
		HudDrawFlags hudDrawFlags = (HudDrawFlags)(-1);
		hudDrawFlags &= BaseDrawFlag;
		foreach (HudDrawFlags drawFlagTuning in _drawFlagTunings)
		{
			hudDrawFlags &= drawFlagTuning;
		}
		return hudDrawFlags;
	}

	private void UpdateDrawFlags()
	{
		HudController.Instance.DrawFlags = GetConsolidatedFlag();
	}
}
