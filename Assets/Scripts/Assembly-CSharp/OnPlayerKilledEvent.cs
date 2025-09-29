using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;

public class OnPlayerKilledEvent
{
	public CharacterInfo ShooterInfo { get; set; }

	public UberstrikeItemClass WeaponCategory { get; set; }

	public BodyPart BodyHitPart { get; set; }
}
