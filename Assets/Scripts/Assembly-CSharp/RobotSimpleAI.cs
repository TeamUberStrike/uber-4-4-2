using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RobotAnimationController))]
public class RobotSimpleAI : BaseGameProp
{
	private enum RobotStates
	{
		Dance = 0,
		Hide = 1,
		Show = 2,
		DoneShow = 3,
		Explode = 4,
		FadeOutParts = 5,
		Dead = 6
	}

	private AvatarHudInformation _hudInfo;

	private short _health;

	private List<Transform> _children;

	private List<Transform> _parents;

	private List<Vector3> _position;

	private List<Quaternion> _rotation;

	private List<Rigidbody> _rBody;

	private RobotStates _myRobotStates;

	private RobotAnimationController _animationController;

	private float _nextTimeToCheck;

	[SerializeField]
	private float _timeToDance = 2f;

	[SerializeField]
	private float _timeToHide = 3f;

	[SerializeField]
	private float _timeToExplode = 5f;

	[SerializeField]
	private float _timeToReborn = 5f;

	[SerializeField]
	private GameObject _damageFeedback;

	[SerializeField]
	private float _forceFactor = 0.2f;

	[SerializeField]
	private float _transparentTime;

	public override bool CanApplyDamage
	{
		get
		{
			return _health > 0;
		}
	}

	public int Health
	{
		get
		{
			return _health;
		}
	}

	private void Awake()
	{
		_animationController = GetComponent<RobotAnimationController>();
		_children = new List<Transform>();
		_parents = new List<Transform>();
		_position = new List<Vector3>();
		_rotation = new List<Quaternion>();
		_rBody = new List<Rigidbody>();
		GetChildrenData(base.Transform, ref _children, ref _parents, ref _position, ref _rotation, ref _rBody);
		_health = 100;
		if ((bool)base.transform.parent)
		{
			_hudInfo = base.transform.parent.GetComponentInChildren<AvatarHudInformation>();
			if ((bool)_hudInfo)
			{
				_hudInfo.SetHealthBarValue((float)_health / 100f);
			}
		}
		CharacterHitArea[] componentsInChildren = GetComponentsInChildren<CharacterHitArea>(true);
		CharacterHitArea[] array = componentsInChildren;
		foreach (CharacterHitArea characterHitArea in array)
		{
			characterHitArea.Shootable = this;
		}
	}

	private void Start()
	{
		_animationController.PlayAnimationHard("Dance");
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			for (int j = i + 1; j < componentsInChildren.Length; j++)
			{
				Physics.IgnoreCollision(componentsInChildren[i], componentsInChildren[j]);
			}
		}
	}

	private void Update()
	{
		if (_myRobotStates == RobotStates.Show && !_animationController.CheckIfActive("BallToBot"))
		{
			_myRobotStates = RobotStates.DoneShow;
		}
		if (!(Time.time > _nextTimeToCheck))
		{
			return;
		}
		switch (_myRobotStates)
		{
		case RobotStates.Dance:
			_nextTimeToCheck = Time.time + _timeToHide;
			_animationController.PlayAnimationHard("BotToBall");
			_myRobotStates = RobotStates.Hide;
			break;
		case RobotStates.Hide:
			_animationController.PlayAnimationHard("BallToBot");
			_myRobotStates = RobotStates.Show;
			break;
		case RobotStates.DoneShow:
			_nextTimeToCheck = Time.time + _timeToDance;
			_animationController.PlayAnimationHard("Dance");
			_myRobotStates = RobotStates.Dance;
			break;
		case RobotStates.Explode:
			_nextTimeToCheck = Time.time + _transparentTime * Time.deltaTime;
			_myRobotStates = RobotStates.FadeOutParts;
			break;
		case RobotStates.FadeOutParts:
			_nextTimeToCheck = Time.time + _transparentTime * Time.deltaTime;
			if (FadeOutRobot(0.1f))
			{
				_myRobotStates = RobotStates.Dead;
				HideRobot();
				_nextTimeToCheck = Time.time + _timeToReborn;
			}
			break;
		case RobotStates.Dead:
			Reborn();
			break;
		case RobotStates.Show:
			break;
		}
	}

	public void Die(Vector3 force)
	{
		Explode(force);
		_myRobotStates = RobotStates.Explode;
		_nextTimeToCheck = Time.time + _timeToExplode;
	}

	private void Explode(Vector3 force)
	{
		_myRobotStates = RobotStates.Explode;
		_animationController.AnimationStop();
		_animationController.enabled = false;
		base.animation.enabled = false;
		if ((bool)base.collider)
		{
			base.collider.isTrigger = true;
		}
		base.gameObject.layer = 2;
		float magnitude = force.magnitude;
		for (int i = 0; i < _rBody.Count; i++)
		{
			_rBody[i].isKinematic = false;
			_rBody[i].AddForce((Random.onUnitSphere * magnitude + force) * _forceFactor * 0.1f, ForceMode.Impulse);
			_rBody[i].transform.parent = base.Transform;
		}
	}

	private void GetChildrenData(Transform root, ref List<Transform> go, ref List<Transform> parents, ref List<Vector3> pos, ref List<Quaternion> rot, ref List<Rigidbody> rb)
	{
		for (int i = 0; i < root.childCount; i++)
		{
			if (root.GetChild(i).GetComponent<Rigidbody>() != null)
			{
				go.Add(root.GetChild(i));
				pos.Add(root.GetChild(i).localPosition);
				rot.Add(root.GetChild(i).localRotation);
				rb.Add(root.GetChild(i).GetComponent<Rigidbody>());
				parents.Add(root);
			}
			if (root.GetChild(i).childCount > 0)
			{
				GetChildrenData(root.GetChild(i), ref go, ref parents, ref pos, ref rot, ref rb);
			}
		}
	}

	private bool FadeOutRobot(float alpha)
	{
		bool result = false;
		for (int i = 0; i < _rBody.Count; i++)
		{
			Vector4 vector = _children[i].renderer.material.color;
			if (vector.w < alpha)
			{
				vector.w = 0f;
				result = true;
			}
			else
			{
				vector.w -= alpha;
			}
			_children[i].renderer.material.color = vector;
		}
		return result;
	}

	private void Reborn()
	{
		ClearProjectileDecorators();
		for (int i = 0; i < _rBody.Count; i++)
		{
			_rBody[i].isKinematic = true;
			_children[i].localPosition = _position[i];
			_children[i].localRotation = _rotation[i];
			_children[i].GetComponent<MeshRenderer>().enabled = true;
			_children[i].collider.isTrigger = false;
			_children[i].parent = _parents[i];
			Vector4 vector = _children[i].renderer.material.color;
			vector.w = 1f;
			_children[i].renderer.material.color = vector;
		}
		_animationController.enabled = true;
		_myRobotStates = RobotStates.Show;
		base.animation.enabled = true;
		if ((bool)base.collider)
		{
			base.collider.isTrigger = false;
		}
		_animationController.PlayAnimationHard("BallToBot");
		base.gameObject.layer = 21;
		_health = 100;
		if ((bool)_hudInfo)
		{
			_hudInfo.SetHealthBarValue((float)_health / 100f);
		}
	}

	private void HideRobot()
	{
		for (int i = 0; i < _children.Count; i++)
		{
			_children[i].GetComponent<MeshRenderer>().enabled = false;
			_children[i].collider.isTrigger = true;
		}
	}

	public override void ApplyDamage(DamageInfo d)
	{
		if (_health > 0)
		{
			_health -= d.Damage;
			ShowDamageFeedback(d);
			if ((bool)_hudInfo)
			{
				_hudInfo.SetHealthBarValue((float)_health / 100f);
			}
			if (_health <= 0)
			{
				Die(d.Force);
			}
		}
	}

	private void ClearProjectileDecorators()
	{
		ArrowProjectile[] componentsInChildren = GetComponentsInChildren<ArrowProjectile>(true);
		foreach (ArrowProjectile arrowProjectile in componentsInChildren)
		{
			arrowProjectile.Destroy();
		}
	}

	private void ShowDamageFeedback(DamageInfo shot)
	{
		GameObject gameObject = Object.Instantiate(_damageFeedback, shot.Hitpoint, Quaternion.LookRotation(shot.Force)) as GameObject;
		if ((bool)gameObject)
		{
			gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
			PlayerDamageEffect component = gameObject.GetComponent<PlayerDamageEffect>();
			if ((bool)component)
			{
				component.Show(shot);
			}
		}
	}
}
