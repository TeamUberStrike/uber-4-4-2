using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class CharacterHitArea : BaseGameProp
{
	[SerializeField]
	private BodyPart _part;

	public override bool IsLocal
	{
		get
		{
			return Shootable != null && Shootable.IsLocal;
		}
	}

	public IShootable Shootable { get; set; }

	public BodyPart CharacterBodyPart
	{
		get
		{
			return _part;
		}
	}

	public override void ApplyDamage(DamageInfo shot)
	{
		shot.BodyPart = _part;
		if (Shootable != null)
		{
			if (Shootable.IsVulnerable)
			{
				if (_part == BodyPart.Head || _part == BodyPart.Nuts)
				{
					shot.Damage += (short)((float)shot.Damage * shot.CriticalStrikeBonus);
				}
				Shootable.ApplyDamage(shot);
			}
		}
		else
		{
			Debug.LogError("No character set to the body part!");
		}
	}
}
