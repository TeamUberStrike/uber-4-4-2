using UberStrike.Realtime.UnitySdk;

public class PlayerSound
{
	private CharacterInfo _character;

	private CharacterConfig _config;

	public PlayerSound(CharacterInfo character)
	{
		_character = character;
	}

	public void SetCharacter(CharacterConfig config)
	{
		_config = config;
	}

	public void Update()
	{
		if (_config == null || _config.Avatar.Decorator == null)
		{
			return;
		}
		bool flag = (_character.PlayerState & (PlayerStates.GROUNDED | PlayerStates.WADING | PlayerStates.SWIMMING | PlayerStates.DIVING)) != 0;
		bool flag2 = (_character.Is(PlayerStates.DIVING) && _character.Keys != KeyState.Still) || (_character.Keys & KeyState.Walking) != 0;
		if (flag && flag2 && _config.Avatar.Decorator.CanPlayFootSound)
		{
			if (_character.Is(PlayerStates.WADING))
			{
				_config.Avatar.Decorator.PlayFootSound(FootStepSoundType.Water, _config.WalkingSoundSpeed);
			}
			else if (_character.Is(PlayerStates.SWIMMING))
			{
				_config.Avatar.Decorator.PlayFootSound(FootStepSoundType.Swim, _config.WalkingSoundSpeed);
			}
			else if (_character.Is(PlayerStates.DIVING))
			{
				_config.Avatar.Decorator.PlayFootSound(FootStepSoundType.Dive, _config.WalkingSoundSpeed);
			}
			else
			{
				_config.Avatar.Decorator.PlayFootSound(_config.WalkingSoundSpeed);
			}
		}
	}
}
