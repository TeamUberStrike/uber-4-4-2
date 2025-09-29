using UnityEngine;

public class SeagullFollowCamera : MonoBehaviour
{
	public Transform Seagull;

	public float _positionDamping;

	public float _rotationDamping;

	private Transform _transformCache;

	private void LateUpdate()
	{
		if ((bool)Seagull)
		{
			if (!_transformCache)
			{
				_transformCache = base.transform;
			}
			_transformCache.position = Vector3.Lerp(_transformCache.position, Seagull.position, Time.deltaTime * _positionDamping);
			_transformCache.rotation = Quaternion.Lerp(_transformCache.rotation, Seagull.rotation, Time.deltaTime * _rotationDamping);
		}
	}
}
