using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;

public class OnPlayerKillEnemyEvent
{
	public CharacterInfo EmemyInfo { get; set; }

	public UberstrikeItemClass WeaponCategory { get; set; }

	public BodyPart BodyHitPart { get; set; }
}
