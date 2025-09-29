using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;

public class GameDataRestrictionComparer : IComparer<GameMetaData>
{
	private int _playerLevel;

	private IComparer<GameMetaData> _baseComparer;

	public GameDataRestrictionComparer(int playerLevel, IComparer<GameMetaData> baseComparer)
	{
		_playerLevel = playerLevel;
		_baseComparer = baseComparer;
	}

	public int Compare(GameMetaData x, GameMetaData y)
	{
		if (x.HasLevelRestriction || y.HasLevelRestriction)
		{
			return (_playerLevel >= 5) ? VeteranLevelsUp(x, y) : NoobLevelsUp(x, y);
		}
		return _baseComparer.Compare(x, y);
	}

	private int NoobLevelsUp(GameMetaData x, GameMetaData y)
	{
		return ((x.LevelMin >= 5 || x.LevelMin == 0) ? x.LevelMin : (x.LevelMin - 100)) - ((y.LevelMin >= 5 || y.LevelMin == 0) ? y.LevelMin : (y.LevelMin - 100));
	}

	private int VeteranLevelsUp(GameMetaData x, GameMetaData y)
	{
		return ((x.LevelMin >= 5) ? x.LevelMin : (x.LevelMin + 100)) - ((y.LevelMin >= 5) ? y.LevelMin : (y.LevelMin + 100));
	}
}
