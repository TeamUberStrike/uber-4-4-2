using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class ReticleHud : Singleton<ReticleHud>
{
	private enum ReticleState
	{
		None = 0,
		Enemy = 1,
		Friend = 2
	}

	private class ReticleConfiguration
	{
		public Reticle Primary;

		public Reticle Secondary;
	}

	private bool _enabled;

	private MeshGUIQuad ReticleQuad;

	private MeshGUIQuad ReticleQuadBlock1;

	private MeshGUIQuad ReticleQuadBlock2;

	private ReticleConfiguration _reticle;

	private ReticleState _curState;

	private bool _isDisplayingEnemyReticle;

	private float _enemyReticleHideTime;

	private float _lastResolutionX;

	private float _lastResolutionY;

	public bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			_enabled = value;
			if (!_enabled && ReticleQuad != null)
			{
				ReticleQuad.IsEnabled = false;
				ReticleQuadBlock1.IsEnabled = false;
				ReticleQuadBlock2.IsEnabled = false;
			}
		}
	}

	private ReticleHud()
	{
		Enabled = false;
		ReticleQuad = new MeshGUIQuad(HudTextures.ReticleSRZoom);
		ReticleQuad.IsEnabled = false;
		ReticleQuad.Depth = 1f;
		Texture2D texture2D = new Texture2D(1, 1);
		texture2D.SetPixel(0, 0, Color.black);
		texture2D.Apply();
		ReticleQuadBlock1 = new MeshGUIQuad(texture2D);
		ReticleQuadBlock1.IsEnabled = false;
		ReticleQuadBlock1.Depth = 1f;
		ReticleQuadBlock2 = new MeshGUIQuad(texture2D);
		ReticleQuadBlock2.IsEnabled = false;
		ReticleQuadBlock2.Depth = 1f;
	}

	public void ConfigureReticle(UberStrikeItemWeaponView weapon)
	{
		Reticle reticle = Singleton<ReticleRepository>.Instance.GetReticle(weapon.ItemClass);
		_reticle = new ReticleConfiguration
		{
			Primary = ((weapon.ItemClass == UberstrikeItemClass.WeaponSniperRifle && !weapon.CustomProperties.ContainsKey("ShowReticleForSniper")) ? null : reticle),
			Secondary = ((weapon.SecondaryActionReticle != 1) ? null : reticle)
		};
	}

	public void Draw()
	{
		if (_reticle == null || !Singleton<WeaponController>.Instance.HasAnyWeapon)
		{
			return;
		}
		if (_curState == ReticleState.Friend)
		{
			GUI.color = Color.green;
		}
		else if (_isDisplayingEnemyReticle)
		{
			GUI.color = Color.red;
		}
		else
		{
			GUI.color = Color.white;
		}
		if (Singleton<WeaponController>.Instance.IsSecondaryAction)
		{
			if (_reticle.Secondary != null)
			{
				_reticle.Secondary.Draw(new Rect((float)(Screen.width - 64) * 0.5f, (float)(Screen.height - 64) * 0.5f, 64f, 64f));
			}
			else if (!WeaponFeedbackManager.Instance.IsIronSighted)
			{
				ReticleQuad.IsEnabled = true;
				ReticleQuadBlock1.IsEnabled = true;
				ReticleQuadBlock2.IsEnabled = true;
				if (_lastResolutionX != (float)Screen.width || _lastResolutionY != (float)Screen.height)
				{
					_lastResolutionX = Screen.width;
					_lastResolutionY = Screen.height;
					float num = Mathf.Min(Screen.width, Screen.height);
					float num2 = ((float)Screen.width - num) * 0.5f;
					float num3 = ((float)Screen.height - num) * 0.5f;
					ReticleQuad.Position = new Vector2(num2, num3);
					float num4 = num / (float)ReticleQuad.Texture.width;
					ReticleQuad.Scale = new Vector2(num4, num4);
					if (Screen.width > Screen.height)
					{
						ReticleQuadBlock1.Position = new Vector2(0f, 0f);
						ReticleQuadBlock1.Scale = new Vector2(num2, Screen.height);
						ReticleQuadBlock2.Position = new Vector2(num + num2, 0f);
						ReticleQuadBlock2.Scale = new Vector2(num2, Screen.height);
					}
					else
					{
						ReticleQuadBlock1.Position = new Vector2(0f, 0f);
						ReticleQuadBlock1.Scale = new Vector2(Screen.width, num3);
						ReticleQuadBlock2.Position = new Vector2(0f, num3 + num);
						ReticleQuadBlock2.Scale = new Vector2(Screen.width, num3);
					}
				}
			}
		}
		else
		{
			ReticleQuad.IsEnabled = false;
			ReticleQuadBlock1.IsEnabled = false;
			ReticleQuadBlock2.IsEnabled = false;
			if (_reticle.Primary != null)
			{
				_reticle.Primary.Draw(new Rect((float)(Screen.width - 64) * 0.5f, (float)(Screen.height - 64) * 0.5f, 64f, 64f));
			}
		}
		GUI.color = Color.white;
	}

	public void Update()
	{
		if (_isDisplayingEnemyReticle && Time.time > _enemyReticleHideTime)
		{
			_isDisplayingEnemyReticle = false;
		}
		Singleton<ReticleRepository>.Instance.UpdateAllReticles();
	}

	public void TriggerReticle(UberstrikeItemClass type)
	{
		Reticle reticle = Singleton<ReticleRepository>.Instance.GetReticle(type);
		if (reticle != null)
		{
			reticle.Trigger();
		}
		else
		{
			Debug.LogError("The weapon class: " + type.ToString() + " is not configured!");
		}
	}

	public void FocusCharacter(TeamID teamId)
	{
		if (GameState.CurrentGameMode == GameMode.TeamDeathMatch)
		{
			if (GameState.HasCurrentPlayer && teamId == GameState.LocalCharacter.TeamID)
			{
				_curState = ReticleState.Friend;
			}
			else if (_isDisplayingEnemyReticle)
			{
				_curState = ReticleState.Enemy;
			}
			else
			{
				_curState = ReticleState.None;
			}
		}
		else if (_isDisplayingEnemyReticle)
		{
			_curState = ReticleState.Enemy;
		}
		else
		{
			_curState = ReticleState.None;
		}
	}

	public void UnFocusCharacter()
	{
		_curState = ReticleState.None;
	}

	public void EnableEnemyReticle()
	{
		_isDisplayingEnemyReticle = true;
		_enemyReticleHideTime = Time.time + 1f;
	}
}
