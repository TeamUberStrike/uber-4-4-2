using UberStrike.Realtime.UnitySdk;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TempleTeleporter : SecretDoor
{
	[SerializeField]
	private float _activationTime = 15f;

	[SerializeField]
	private Renderer[] _visuals;

	[SerializeField]
	private Transform _spawnpoint;

	[SerializeField]
	private ParticleSystem _particles;

	private int _doorID;

	private float _timeOut;

	private AudioSource[] _audios;

	public int DoorID
	{
		get
		{
			return _doorID;
		}
	}

	private void Awake()
	{
		_audios = GetComponents<AudioSource>();
		var emission = _particles.emission;
		emission.enabled = false;
		if (_particles.isPlaying)
		{
			_particles.Stop();
		}
		Renderer[] visuals = _visuals;
		foreach (Renderer renderer in visuals)
		{
			renderer.enabled = false;
		}
		_doorID = base.transform.position.GetHashCode();
	}

	private void OnEnable()
	{
		CmuneEventHandler.AddListener<DoorOpenedEvent>(OnDoorOpenedEvent);
	}

	private void OnDisable()
	{
		CmuneEventHandler.RemoveListener<DoorOpenedEvent>(OnDoorOpenedEvent);
	}

	private void Update()
	{
		if (_timeOut < Time.time)
		{
			AudioSource[] audios = _audios;
			foreach (AudioSource audioSource in audios)
			{
				audioSource.Stop();
			}
			var emission = _particles.emission;
			emission.enabled = false;
			if (_particles.isPlaying)
			{
				_particles.Stop();
			}
			Renderer[] visuals = _visuals;
			foreach (Renderer renderer in visuals)
			{
				renderer.enabled = false;
			}
			base.enabled = false;
		}
	}

	private void OnTriggerEnter(Collider c)
	{
		if (c.tag == "Player" && _timeOut > Time.time)
		{
			_timeOut = 0f;
			GameState.LocalPlayer.SpawnPlayerAt(_spawnpoint.position, _spawnpoint.rotation);
		}
	}

	private void OnDoorOpenedEvent(DoorOpenedEvent ev)
	{
		if (DoorID == ev.DoorID)
		{
			OpenDoor();
		}
	}

	public override void Open()
	{
		if (GameState.HasCurrentGame)
		{
			GameState.CurrentGame.OpenDoor(DoorID);
		}
		OpenDoor();
	}

	private void OpenDoor()
	{
		base.enabled = true;
		var emission = _particles.emission;
		emission.enabled = true;
		if (!_particles.isPlaying)
		{
			_particles.Play();
		}
		Renderer[] visuals = _visuals;
		foreach (Renderer renderer in visuals)
		{
			renderer.enabled = true;
		}
		_timeOut = Time.time + _activationTime;
		AudioSource[] audios = _audios;
		foreach (AudioSource audioSource in audios)
		{
			audioSource.Play();
		}
	}
}
