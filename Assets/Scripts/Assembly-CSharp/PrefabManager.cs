using UnityEngine;

public class PrefabManager : MonoBehaviour
{
	[SerializeField]
	private AvatarDecorator _defaultAvatar;

	[SerializeField]
	private AvatarDecoratorConfig _defaultRagdoll;

	[SerializeField]
	private CharacterConfig _remoteCharacter;

	[SerializeField]
	private CharacterConfig _localCharacter;

	[SerializeField]
	private GameObject _lotteryEffect;

	[SerializeField]
	private Animation _lotteryUIAnimation;

	public static PrefabManager Instance { get; private set; }

	public static bool Exists
	{
		get
		{
			return Instance != null;
		}
	}

	public AvatarDecorator DefaultAvatar
	{
		get
		{
			return _defaultAvatar;
		}
	}

	public AvatarDecoratorConfig DefaultRagdoll
	{
		get
		{
			return _defaultRagdoll;
		}
	}

	public CharacterConfig InstantiateLocalCharacter()
	{
		return Object.Instantiate(_localCharacter) as CharacterConfig;
	}

	public CharacterConfig InstantiateRemoteCharacter()
	{
		return Object.Instantiate(_remoteCharacter) as CharacterConfig;
	}

	public void InstantiateLotteryEffect()
	{
		if ((bool)_lotteryEffect)
		{
			Object.Instantiate(_lotteryEffect);
		}
	}

	public Animation GetLotteryUIAnimation()
	{
		Animation result = null;
		if ((bool)_lotteryUIAnimation)
		{
			result = Object.Instantiate(_lotteryUIAnimation) as Animation;
		}
		else
		{
			Debug.LogError("The LotteryUIAnimation is not signed in PrefabManger!");
		}
		return result;
	}

	private void Awake()
	{
		Instance = this;
	}
}
