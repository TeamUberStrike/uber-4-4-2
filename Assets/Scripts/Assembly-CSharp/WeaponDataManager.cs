using UberStrike.Core.Models.Views;
using UnityEngine;

public static class WeaponDataManager
{
	public static Vector3 ApplyDispersion(Vector3 shootingRay, UberStrikeItemWeaponView view, bool ironSight)
	{
		float num = WeaponConfigurationHelper.GetAccuracySpread(view);
		if (WeaponFeedbackManager.Exists && WeaponFeedbackManager.Instance.IsIronSighted && ironSight)
		{
			num *= 0.5f;
		}
		Vector2 vector = Random.insideUnitCircle * num * 0.5f;
		return Quaternion.AngleAxis(vector.x, GameState.WeaponCameraTransform.right) * Quaternion.AngleAxis(vector.y, GameState.WeaponCameraTransform.up) * shootingRay;
	}
}
