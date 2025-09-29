using System.Collections.Generic;
using UberStrike.Core.Types;

internal class ReticleRepository : Singleton<ReticleRepository>
{
	private Dictionary<UberstrikeItemClass, Reticle> _reticles;

	private ReticleRepository()
	{
		InitReticles();
	}

	public Reticle GetReticle(UberstrikeItemClass type)
	{
		if (_reticles.ContainsKey(type))
		{
			return _reticles[type];
		}
		return null;
	}

	public void UpdateAllReticles()
	{
		foreach (Reticle value in _reticles.Values)
		{
			value.Update();
		}
	}

	private void InitReticles()
	{
		_reticles = new Dictionary<UberstrikeItemClass, Reticle>(8);
		Reticle reticle = new Reticle();
		reticle.SetInnerScale(HudTextures.MGScale, 1.2f);
		reticle.SetTranslate(HudTextures.MGTranslate, 5f);
		Reticle reticle2 = new Reticle();
		reticle2.SetInnerScale(HudTextures.SRScale, 1.2f);
		reticle2.SetTranslate(HudTextures.SRTranslate, 5f);
		Reticle reticle3 = new Reticle();
		reticle3.SetInnerScale(HudTextures.SGScaleInside, 1.2f);
		reticle3.SetOutterScale(HudTextures.SGScaleOutside, 2f);
		Reticle reticle4 = new Reticle();
		reticle4.SetRotate(HudTextures.CNRotate, 60f);
		reticle4.SetInnerScale(HudTextures.CNScale, 1.5f);
		Reticle reticle5 = new Reticle();
		reticle5.SetTranslate(HudTextures.HGTraslate, 5f);
		Reticle reticle6 = new Reticle();
		reticle6.SetInnerScale(HudTextures.SPScale, 1.2f);
		reticle6.SetTranslate(HudTextures.SPTranslate, 5f);
		Reticle reticle7 = new Reticle();
		reticle7.SetInnerScale(HudTextures.LRScale, 1.2f);
		reticle7.SetTranslate(HudTextures.LRTranslate, 5f);
		Reticle reticle8 = new Reticle();
		reticle8.SetTranslate(HudTextures.MWTranslate, 5f);
		_reticles.Add(UberstrikeItemClass.WeaponMachinegun, reticle);
		_reticles.Add(UberstrikeItemClass.WeaponSniperRifle, reticle2);
		_reticles.Add(UberstrikeItemClass.WeaponShotgun, reticle3);
		_reticles.Add(UberstrikeItemClass.WeaponCannon, reticle4);
		_reticles.Add(UberstrikeItemClass.WeaponSplattergun, reticle6);
		_reticles.Add(UberstrikeItemClass.WeaponLauncher, reticle7);
		_reticles.Add(UberstrikeItemClass.WeaponMelee, reticle8);
	}
}
