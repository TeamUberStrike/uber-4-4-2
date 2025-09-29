using UnityEngine;

public class BaseGameProp : MonoBehaviour, IShootable
{
	protected Transform _transform;

	private Rigidbody _rigidbody;

	private bool _isMoved;

	private bool _isSleeping;

	private bool _isFreezed;

	private bool _isPassive;

	private float _originalMass = 1f;

	private Component[] _rbs;

	private float[] _oriMass;

	private float[] _adrag;

	private float[] _drag;

	[SerializeField]
	protected bool _recieveProjectileDamage = true;

	public virtual bool IsVulnerable
	{
		get
		{
			return true;
		}
	}

	public virtual bool IsLocal
	{
		get
		{
			return false;
		}
	}

	public virtual bool CanApplyDamage
	{
		get
		{
			return true;
		}
	}

	public Vector3 Scale
	{
		get
		{
			return Transform.localScale;
		}
	}

	public Vector3 Position
	{
		get
		{
			return Transform.position;
		}
	}

	public Quaternion Rotation
	{
		get
		{
			return Transform.rotation;
		}
	}

	public Vector3 Velocity
	{
		get
		{
			if (HasRigidbody)
			{
				return _rigidbody.velocity;
			}
			return Vector3.zero;
		}
	}

	public Vector3 AngularVelocity
	{
		get
		{
			if (HasRigidbody)
			{
				return _rigidbody.angularVelocity;
			}
			return Vector3.zero;
		}
	}

	public Transform Transform
	{
		get
		{
			if (_transform == null)
			{
				_transform = base.transform;
			}
			return _transform;
		}
	}

	public bool HasRigidbody
	{
		get
		{
			return Rigidbody != null;
		}
	}

	public Rigidbody Rigidbody
	{
		get
		{
			if (_rigidbody == null)
			{
				_rigidbody = base.rigidbody;
			}
			return _rigidbody;
		}
	}

	public bool IsMoved
	{
		get
		{
			return _isMoved;
		}
		set
		{
			_isMoved = value;
		}
	}

	public bool IsSleeping
	{
		get
		{
			return _isSleeping;
		}
		set
		{
			_isSleeping = value;
			if (HasRigidbody)
			{
				if (_isSleeping)
				{
					Rigidbody.Sleep();
				}
				else
				{
					Rigidbody.WakeUp();
				}
			}
		}
	}

	public float Mass
	{
		get
		{
			return _originalMass;
		}
		set
		{
			_originalMass = value;
			if (HasRigidbody)
			{
				Rigidbody.mass = _originalMass;
			}
		}
	}

	public virtual bool IsFreezed
	{
		get
		{
			return _isFreezed;
		}
		set
		{
			FreezeObject(value);
		}
	}

	public virtual bool IsPassiv
	{
		get
		{
			return _isPassive;
		}
		set
		{
			_isPassive = value;
		}
	}

	public bool RecieveProjectileDamage
	{
		get
		{
			return _recieveProjectileDamage;
		}
	}

	private void FreezeObject(bool b)
	{
		if (b == _isFreezed)
		{
			return;
		}
		if (b)
		{
			_rbs = GetComponentsInChildren(typeof(Rigidbody));
			_oriMass = new float[_rbs.Length];
			_adrag = new float[_rbs.Length];
			_drag = new float[_rbs.Length];
		}
		for (int i = 0; i < _rbs.Length; i++)
		{
			Rigidbody rigidbody = (Rigidbody)_rbs[i];
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
			rigidbody.useGravity = !b;
			rigidbody.freezeRotation = b;
			if (b)
			{
				_oriMass[i] = rigidbody.mass;
				_adrag[i] = rigidbody.angularDrag;
				_drag[i] = rigidbody.drag;
				rigidbody.angularDrag = 10000f;
				rigidbody.drag = 10000f;
				rigidbody.mass = 0.1f;
			}
			else
			{
				rigidbody.mass = _oriMass[i];
				rigidbody.angularDrag = _adrag[i];
				rigidbody.drag = _drag[i];
			}
		}
		_isFreezed = b;
	}

	public virtual void ApplyDamage(DamageInfo shot)
	{
		ApplyForce(shot.Hitpoint, shot.Force * 5f);
	}

	public virtual void ApplyForce(Vector3 position, Vector3 direction)
	{
		if (HasRigidbody)
		{
			Rigidbody.AddForceAtPosition(direction, position);
		}
	}
}
