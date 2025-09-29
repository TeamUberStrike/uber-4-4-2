using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SplineInterpolator))]
public class SplineController : MonoBehaviour
{
	public GameObject Target;

	public GameObject SplineRoot;

	public float Duration = 10f;

	public eOrientationMode OrientationMode;

	public eWrapMode WrapMode;

	public bool AutoStart;

	public bool AutoClose;

	public bool HideOnExecute = true;

	public bool LerpInitialPos;

	private SplineInterpolator mSplineInterp;

	private Transform[] mTransforms;

	public bool SplineMovementDone
	{
		get
		{
			if (mSplineInterp != null)
			{
				return mSplineInterp.IsStopped;
			}
			return false;
		}
	}

	private void Awake()
	{
		mSplineInterp = GetComponent<SplineInterpolator>();
		Profiler.enabled = true;
	}

	private IEnumerator Start()
	{
		if (HideOnExecute)
		{
			DisableTransforms();
		}
		while (Target == null)
		{
			yield return new WaitForEndOfFrame();
		}
		if (AutoStart)
		{
			FollowSpline();
		}
	}

	private void SetupSplineInterpolator(SplineInterpolator interp, Transform[] trans)
	{
		interp.Reset();
		float num = ((!AutoClose) ? (Duration / (float)(trans.Length - 1)) : (Duration / (float)trans.Length));
		int i;
		for (i = 0; i < trans.Length; i++)
		{
			if (OrientationMode == eOrientationMode.NODE)
			{
				interp.AddPoint(trans[i].position, trans[i].rotation, num * (float)i, new Vector2(0f, 1f));
			}
			else if (OrientationMode == eOrientationMode.TANGENT)
			{
				interp.AddPoint(quat: (i != trans.Length - 1) ? Quaternion.LookRotation(trans[i + 1].position - trans[i].position, trans[i].up) : ((!AutoClose) ? trans[i].rotation : Quaternion.LookRotation(trans[0].position - trans[i].position, trans[i].up)), pos: trans[i].position, timeInSeconds: num * (float)i, easeInOut: new Vector2(0f, 1f));
			}
		}
		if (AutoClose)
		{
			interp.SetAutoCloseMode(num * (float)i);
		}
	}

	private Transform[] GetTransforms()
	{
		if (SplineRoot != null)
		{
			List<Transform> list = new List<Transform>(SplineRoot.GetComponentsInChildren<Transform>(true));
			list.Remove(SplineRoot.transform);
			list.Sort((Transform a, Transform b) => a.name.CompareTo(b.name));
			return list.ToArray();
		}
		return null;
	}

	private void DisableTransforms()
	{
		if (SplineRoot != null)
		{
			SplineRoot.SetActive(false);
		}
	}

	public void FollowSpline(OnEndCallback callback = null)
	{
		mTransforms = GetTransforms();
		if (mTransforms.Length > 0)
		{
			if (LerpInitialPos)
			{
				mTransforms[0] = Target.transform;
			}
			SetupSplineInterpolator(mSplineInterp, mTransforms);
			mSplineInterp.StartInterpolation(Target, null, true, WrapMode);
		}
	}

	public void Reset()
	{
		if ((bool)Target && mTransforms.Length > 0)
		{
			Target.transform.position = mTransforms[0].position;
			Target.transform.rotation = mTransforms[0].rotation;
		}
	}

	public void Stop()
	{
		if ((bool)mSplineInterp)
		{
			mSplineInterp.Stop();
		}
	}
}
