using UnityEngine;

public class LevelTutorial : MonoBehaviour
{
	[SerializeField]
	private Animation _airlockBrigeAnim;

	[SerializeField]
	private Animation _airlockDoorAnim;

	[SerializeField]
	private DoorBehaviour _armoryDoor;

	[SerializeField]
	private AudioSource _bridgeAudio;

	[SerializeField]
	private AudioSource _backgroundMusic;

	[SerializeField]
	private BitmapFont _font;

	[SerializeField]
	private Transform _npcStartPos;

	[SerializeField]
	private BaseWeaponDecorator _npcWeapon;

	[SerializeField]
	private AudioClip _bigDoorClose;

	[SerializeField]
	private AudioClip _waypoint;

	[SerializeField]
	private AudioClip _bigObjComplete;

	[SerializeField]
	private AudioClip _voiceWelcome;

	[SerializeField]
	private AudioClip _voiceToArmory;

	[SerializeField]
	private AudioClip _voicePickupWeapon;

	[SerializeField]
	private AudioClip _voiceShootingRange;

	[SerializeField]
	private AudioClip _voiceShootMore;

	[SerializeField]
	private AudioClip _voiceArena;

	[SerializeField]
	private AudioClip _voiceTutorialComplete;

	[SerializeField]
	private SplineController _airlockSplineController;

	[SerializeField]
	private TutorialAirlockFrontDoor _airlockFrontDoor;

	[SerializeField]
	private TutorialAirlockDoor _airlockBackDoor;

	[SerializeField]
	private TutorialArmoryEnterTrigger _armoryTrigger;

	[SerializeField]
	private Texture _imgMouse;

	[SerializeField]
	private Texture _imgObjTickBackground;

	[SerializeField]
	private Texture _imgObjTickForeground;

	[SerializeField]
	private Texture[] _imgWasdWalkBlack;

	[SerializeField]
	private Texture[] _imgWasdWalkBlue;

	[SerializeField]
	private TutorialWaypoint _armoryWaypoint;

	[SerializeField]
	private TutorialWaypoint _weaponWaypoint;

	[SerializeField]
	private GameObject _shootingTarget;

	[SerializeField]
	private Transform[] _nearRangeTargetPos;

	[SerializeField]
	private Transform[] _farRangeTargetPos;

	[SerializeField]
	private Transform _armoryCameraPathEnd;

	[SerializeField]
	private Transform _finalPlayerPos;

	[SerializeField]
	private TutorialArmoryPickup _pickupWeapon;

	[SerializeField]
	private TutorialWaypoint _ammoWaypoint;

	public static LevelTutorial Instance { get; private set; }

	public static bool Exists
	{
		get
		{
			return Instance != null;
		}
	}

	public Animation AirlockBridgeAnim
	{
		get
		{
			return _airlockBrigeAnim;
		}
	}

	public Animation AirlockDoorAnim
	{
		get
		{
			return _airlockDoorAnim;
		}
	}

	public DoorBehaviour ArmoryDoor
	{
		get
		{
			return _armoryDoor;
		}
	}

	public AudioSource BridgeAudio
	{
		get
		{
			return _bridgeAudio;
		}
	}

	public AudioSource BackgroundMusic
	{
		get
		{
			return _backgroundMusic;
		}
	}

	public BitmapFont Font
	{
		get
		{
			return _font;
		}
	}

	public Transform NpcStartPos
	{
		get
		{
			return _npcStartPos;
		}
	}

	public int GearHead
	{
		get
		{
			return 1084;
		}
	}

	public int GearGloves
	{
		get
		{
			return 1086;
		}
	}

	public int GearUB
	{
		get
		{
			return 1087;
		}
	}

	public int GearLB
	{
		get
		{
			return 1088;
		}
	}

	public int GearBoots
	{
		get
		{
			return 1089;
		}
	}

	public BaseWeaponDecorator Weapon
	{
		get
		{
			return _npcWeapon;
		}
	}

	public AudioClip BigDoorClose
	{
		get
		{
			return _bigDoorClose;
		}
	}

	public AudioClip WaypointAppear
	{
		get
		{
			return _waypoint;
		}
	}

	public AudioClip BigObjComplete
	{
		get
		{
			return _bigObjComplete;
		}
	}

	public AudioClip VoiceWelcome
	{
		get
		{
			return _voiceWelcome;
		}
	}

	public AudioClip VoiceToArmory
	{
		get
		{
			return _voiceToArmory;
		}
	}

	public AudioClip VoicePickupWeapon
	{
		get
		{
			return _voicePickupWeapon;
		}
	}

	public AudioClip VoiceShootingRange
	{
		get
		{
			return _voiceShootingRange;
		}
	}

	public AudioClip VoiceShootMore
	{
		get
		{
			return _voiceShootMore;
		}
	}

	public AudioClip VoiceArena
	{
		get
		{
			return _voiceArena;
		}
	}

	public AudioClip TutorialComplete
	{
		get
		{
			return _voiceTutorialComplete;
		}
	}

	public SplineController AirlockSplineController
	{
		get
		{
			return _airlockSplineController;
		}
	}

	public TutorialAirlockFrontDoor AirlockFrontDoor
	{
		get
		{
			return _airlockFrontDoor;
		}
	}

	public TutorialAirlockDoor AirlockBackDoor
	{
		get
		{
			return _airlockBackDoor;
		}
	}

	public TutorialArmoryEnterTrigger ArmoryTrigger
	{
		get
		{
			return _armoryTrigger;
		}
	}

	public Texture ImgMouse
	{
		get
		{
			return _imgMouse;
		}
	}

	public Texture ImgObjBk
	{
		get
		{
			return _imgObjTickBackground;
		}
	}

	public Texture ImgObjTick
	{
		get
		{
			return _imgObjTickForeground;
		}
	}

	public Texture[] ImgWasdWalkBlack
	{
		get
		{
			return _imgWasdWalkBlack;
		}
	}

	public Texture[] ImgWasdWalkBlue
	{
		get
		{
			return _imgWasdWalkBlue;
		}
	}

	public TutorialWaypoint ArmoryWaypoint
	{
		get
		{
			return _armoryWaypoint;
		}
	}

	public TutorialWaypoint WeaponWaypoint
	{
		get
		{
			return _weaponWaypoint;
		}
	}

	public GameObject ShootingTargetPrefab
	{
		get
		{
			return _shootingTarget;
		}
	}

	public Transform[] NearRangeTargetPos
	{
		get
		{
			return _nearRangeTargetPos;
		}
	}

	public Transform[] FarRangeTargetPos
	{
		get
		{
			return _farRangeTargetPos;
		}
	}

	public Transform ArmoryCameraPathEnd
	{
		get
		{
			return _armoryCameraPathEnd;
		}
	}

	public Transform FinalPlayerPos
	{
		get
		{
			return _finalPlayerPos;
		}
	}

	public TutorialArmoryPickup PickupWeapon
	{
		get
		{
			return _pickupWeapon;
		}
	}

	public TutorialWaypoint AmmoWaypoint
	{
		get
		{
			return _ammoWaypoint;
		}
	}

	public AvatarDecorator NPC { get; set; }

	public bool IsCinematic { get; set; }

	public bool ShowObjectives { get; set; }

	public bool ShowObjPickupMG { get; set; }

	public bool ShowObjShoot3 { get; set; }

	public bool ShowObjShoot6 { get; set; }

	public bool ShowObjComplete { get; set; }

	public HudDrawFlags HudFlags { get; set; }

	private void Awake()
	{
		Instance = this;
		HudFlags = HudDrawFlags.XpPoints;
	}

	private void Start()
	{
		if (LevelCamera.Instance != null)
		{
			AirlockSplineController.Target = LevelCamera.Instance.gameObject;
			ArmoryTrigger.ArmoryCameraPath.Target = LevelCamera.Instance.gameObject;
		}
		Singleton<GameStateController>.Instance.LoadGameMode(GameMode.Tutorial);
	}
}
