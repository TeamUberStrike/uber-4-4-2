using UnityEngine;

public class MapConfiguration : MonoBehaviour
{
	[SerializeField]
	private bool _isEnabled = true;

	[SerializeField]
	private int _defaultSpawnPoint;

	[SerializeField]
	private FootStepSoundType _defaultFootStep = FootStepSoundType.Sand;

	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private Transform _defaultViewPoint;

	[SerializeField]
	protected GameObject _staticContentParent;

	[SerializeField]
	private GameObject _spawnPoints;

	[SerializeField]
	private Transform _waterPlane;

	[SerializeField]
	private CombatRangeTier _combatRange;

	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
	}

	public int DefaultSpawnPoint
	{
		get
		{
			return _defaultSpawnPoint;
		}
	}

	public string SceneName { get; private set; }

	public Camera Camera
	{
		get
		{
			return _camera;
		}
	}

	public CombatRangeTier CombatRangeTiers
	{
		get
		{
			return _combatRange;
		}
	}

	public FootStepSoundType DefaultFootStep
	{
		get
		{
			return _defaultFootStep;
		}
	}

	public Transform DefaultViewPoint
	{
		get
		{
			return _defaultViewPoint;
		}
	}

	public GameObject SpawnPoints
	{
		get
		{
			return _spawnPoints;
		}
	}

	public bool HasWaterPlane
	{
		get
		{
			return _waterPlane != null;
		}
	}

	public float WaterPlaneHeight
	{
		get
		{
			return (!_waterPlane) ? float.MinValue : _waterPlane.position.y;
		}
	}

	private void Awake()
	{
		if (_defaultViewPoint == null)
		{
			_defaultViewPoint = base.transform;
		}
		GameState.CurrentSpace = this;
		SceneName = Singleton<SceneLoader>.Instance.CurrentScene;
	}

	private void Start()
	{
		Singleton<SpawnPointManager>.Instance.ConfigureSpawnPoints(SpawnPoints.GetComponentsInChildren<SpawnPoint>(true));
		AudioSource[] componentsInChildren = GetComponentsInChildren<AudioSource>();
		foreach (AudioSource audioSource in componentsInChildren)
		{
			audioSource.enabled = false;
		}
		AudioReverbZone[] componentsInChildren2 = GetComponentsInChildren<AudioReverbZone>();
		foreach (AudioReverbZone audioReverbZone in componentsInChildren2)
		{
			audioReverbZone.enabled = false;
		}
		AudioReverbFilter[] componentsInChildren3 = GetComponentsInChildren<AudioReverbFilter>();
		foreach (AudioReverbFilter audioReverbFilter in componentsInChildren3)
		{
			audioReverbFilter.enabled = false;
		}
		AudioLowPassFilter[] componentsInChildren4 = GetComponentsInChildren<AudioLowPassFilter>();
		foreach (AudioLowPassFilter audioLowPassFilter in componentsInChildren4)
		{
			audioLowPassFilter.enabled = false;
		}
		AudioHighPassFilter[] componentsInChildren5 = GetComponentsInChildren<AudioHighPassFilter>();
		foreach (AudioHighPassFilter audioHighPassFilter in componentsInChildren5)
		{
			audioHighPassFilter.enabled = false;
		}
		AudioEchoFilter[] componentsInChildren6 = GetComponentsInChildren<AudioEchoFilter>();
		foreach (AudioEchoFilter audioEchoFilter in componentsInChildren6)
		{
			audioEchoFilter.enabled = false;
		}
		AudioChorusFilter[] componentsInChildren7 = GetComponentsInChildren<AudioChorusFilter>();
		foreach (AudioChorusFilter audioChorusFilter in componentsInChildren7)
		{
			audioChorusFilter.enabled = false;
		}
	}
}
