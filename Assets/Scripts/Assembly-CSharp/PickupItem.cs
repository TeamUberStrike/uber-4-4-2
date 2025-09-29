using System.Collections;
using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PickupItem : MonoBehaviour
{
	[SerializeField]
	protected int _respawnTime = 20;

	[SerializeField]
	private ParticleSystem _emitter;

	[SerializeField]
	protected Transform _pickupItem;

	protected MeshRenderer[] _renderers;

	private bool _isAvailable;

	private int _pickupID;

	private Collider _collider;

	private static int _instanceCounter = 0;

	private static Dictionary<int, PickupItem> _instances = new Dictionary<int, PickupItem>();

	private static List<byte> _pickupRespawnDurations = new List<byte>();

	public bool IsAvailable
	{
		get
		{
			return _isAvailable;
		}
		protected set
		{
			_isAvailable = value;
		}
	}

	protected virtual bool CanPlayerPickup
	{
		get
		{
			return true;
		}
	}

	public int PickupID
	{
		get
		{
			return _pickupID;
		}
		set
		{
			_pickupID = value;
		}
	}

	public int RespawnTime
	{
		get
		{
			return _respawnTime;
		}
	}

	protected virtual void Awake()
	{
		_collider = GetComponent<Collider>();
		if ((bool)_pickupItem)
		{
			_renderers = _pickupItem.GetComponentsInChildren<MeshRenderer>(true);
		}
		else
		{
			_renderers = new MeshRenderer[0];
		}
		_collider.isTrigger = true;
		if (_emitter != null)
		{
			var emission = _emitter.emission;
			emission.enabled = false;
			if (_emitter.isPlaying)
			{
				_emitter.Stop();
			}
		}
		base.gameObject.layer = 2;
	}

	private void OnEnable()
	{
		IsAvailable = true;
		_pickupID = AddInstance(this);
		MeshRenderer[] renderers = _renderers;
		foreach (Renderer renderer in renderers)
		{
			renderer.enabled = true;
		}
		CmuneEventHandler.AddListener<PickupItemEvent>(OnRemotePickupEvent);
	}

	private void OnDisable()
	{
		CmuneEventHandler.RemoveListener<PickupItemEvent>(OnRemotePickupEvent);
	}

	private void OnRemotePickupEvent(PickupItemEvent ev)
	{
		if (PickupID == ev.PickupID)
		{
			SetItemAvailable(ev.ShowItem);
			if (!ev.ShowItem && IsAvailable)
			{
				OnRemotePickup();
			}
		}
	}

	protected virtual void OnRemotePickup()
	{
	}

	private void OnTriggerEnter(Collider c)
	{
		if (IsAvailable && c.tag == "Player" && GameState.HasCurrentPlayer && GameState.LocalCharacter.IsAlive && OnPlayerPickup())
		{
			SetItemAvailable(false);
		}
	}

	protected void PlayLocalPickupSound(AudioClip AudioClip)
	{
		SfxManager.Play2dAudioClip(AudioClip);
	}

	protected void PlayRemotePickupSound(AudioClip AudioClip, Vector3 position)
	{
		SfxManager.Play3dAudioClip(AudioClip, position);
	}

	protected IEnumerator StartHidingPickupForSeconds(int seconds)
	{
		IsAvailable = false;
		ParticleEffectController.ShowPickUpEffect(_pickupItem.position, 100);
		MeshRenderer[] renderers = _renderers;
		foreach (Renderer r in renderers)
		{
			if (r != null)
			{
				r.enabled = false;
			}
		}
		if (seconds > 0)
		{
			yield return new WaitForSeconds(seconds);
			ParticleEffectController.ShowPickUpEffect(_pickupItem.position, 5);
			yield return new WaitForSeconds(1f);
			MeshRenderer[] renderers2 = _renderers;
			foreach (Renderer r2 in renderers2)
			{
				r2.enabled = true;
			}
			IsAvailable = true;
		}
		else
		{
			base.enabled = false;
			yield return new WaitForSeconds(2f);
			Object.Destroy(base.gameObject);
		}
	}

	public void SetItemAvailable(bool isVisible)
	{
		if (isVisible)
		{
			ParticleEffectController.ShowPickUpEffect(_pickupItem.position, 5);
		}
		else if (IsAvailable)
		{
			ParticleEffectController.ShowPickUpEffect(_pickupItem.position, 100);
		}
		MeshRenderer[] renderers = _renderers;
		foreach (Renderer renderer in renderers)
		{
			if ((bool)renderer)
			{
				renderer.enabled = isVisible;
			}
		}
		IsAvailable = isVisible;
	}

	protected virtual bool OnPlayerPickup()
	{
		return true;
	}

	public static void ResetInstanceCounter()
	{
		_instanceCounter = 0;
		_instances.Clear();
		_pickupRespawnDurations.Clear();
	}

	public static int GetInstanceCounter()
	{
		return _instanceCounter;
	}

	public static List<byte> GetRespawnDurations()
	{
		return _pickupRespawnDurations;
	}

	private static int AddInstance(PickupItem i)
	{
		int num = _instanceCounter++;
		_instances[num] = i;
		_pickupRespawnDurations.Add((byte)i.RespawnTime);
		return num;
	}

	public static PickupItem GetInstance(int id)
	{
		PickupItem value = null;
		_instances.TryGetValue(id, out value);
		return value;
	}
}
