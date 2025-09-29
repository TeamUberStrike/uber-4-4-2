using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TutorialShootingTarget : BaseGameProp
{
	[SerializeField]
	private GameObject Body;

	[SerializeField]
	private PlayerDamageEffect _damageEffect;

	private Vector3 _initialPos = new Vector3(0f, -1.72f, 0f);

	private bool _isHit;

	private Dictionary<Rigidbody, Vector3> _bodyPos;

	public Action OnHitCallback;

	public bool IsHit
	{
		get
		{
			return _isHit;
		}
	}

	public bool UseTimer { get; set; }

	public bool IsMoving { get; set; }

	public float MaxTime { get; set; }

	public Vector3 Direction { get; set; }

	public override void ApplyDamage(DamageInfo shot)
	{
		_isHit = true;
		base.gameObject.layer = 1;
		if (OnHitCallback != null)
		{
			OnHitCallback();
		}
		base.ApplyDamage(shot);
		foreach (Rigidbody key in _bodyPos.Keys)
		{
			key.isKinematic = false;
			key.AddExplosionForce(1000f, shot.Hitpoint, 5f);
		}
		if ((bool)_damageEffect)
		{
			PlayerDamageEffect playerDamageEffect = UnityEngine.Object.Instantiate(_damageEffect, shot.Hitpoint, Quaternion.LookRotation(shot.Force)) as PlayerDamageEffect;
			if ((bool)playerDamageEffect)
			{
				playerDamageEffect.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
				playerDamageEffect.Show(shot);
			}
		}
		else
		{
			Debug.LogWarning("No damage effect is attached!");
		}
		SfxManager.Play2dAudioClip(GameAudio.TargetDamage);
	}

	public void Reset()
	{
		_isHit = false;
		Body.transform.localPosition = _initialPos;
		foreach (Rigidbody key in _bodyPos.Keys)
		{
			key.isKinematic = true;
			key.transform.localPosition = _bodyPos[key];
			key.transform.localRotation = Quaternion.identity;
		}
		StartCoroutine(StartPopup());
	}

	private void Awake()
	{
		Body.transform.localPosition = _initialPos;
		base.gameObject.layer = 2;
		_bodyPos = new Dictionary<Rigidbody, Vector3>();
		Transform[] componentsInChildren = Body.GetComponentsInChildren<Transform>(true);
		foreach (Transform transform in componentsInChildren)
		{
			if (!(transform == Body.transform))
			{
				MeshCollider meshCollider = transform.gameObject.AddComponent<MeshCollider>();
				if ((bool)meshCollider)
				{
					meshCollider.convex = true;
				}
				Rigidbody rigidbody = transform.gameObject.AddComponent<Rigidbody>();
				if ((bool)rigidbody)
				{
					rigidbody.isKinematic = true;
					rigidbody.gameObject.layer = 20;
					_bodyPos.Add(rigidbody, rigidbody.transform.localPosition);
				}
			}
		}
	}

	private IEnumerator StartPopup()
	{
		Transform t = Body.transform;
		while (Vector3.Distance(t.localPosition, Vector3.zero) > 0.1f)
		{
			t.localPosition = Vector3.Lerp(t.localPosition, Vector3.zero, Time.deltaTime * 3f);
			yield return new WaitForEndOfFrame();
		}
		base.gameObject.layer = 20;
	}

	private IEnumerator StartSelfDestroy()
	{
		yield return new WaitForSeconds(2f);
		if ((bool)_transform)
		{
			UnityEngine.Object.Destroy(_transform.gameObject);
		}
	}
}
