using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class MeshGUIManager : MonoBehaviour
{
	[SerializeField]
	private GameObject _meshTextObject;

	[SerializeField]
	private GameObject _quadMeshObject;

	[SerializeField]
	private GameObject _circleMeshObject;

	[SerializeField]
	private GameObject _atlasMeshObject;

	[SerializeField]
	private Camera _guiCamera;

	private ObjectRecycler _meshTextRecycler;

	private ObjectRecycler _quadMeshRecycler;

	private ObjectRecycler _circleMeshRecycler;

	private ObjectRecycler _atlasMeshRecycler;

	private GameObject _meshTextContainer;

	private GameObject _quadMeshContainer;

	private GameObject _circleMeshContainer;

	private GameObject _atlasMeshContainer;

	public static MeshGUIManager Instance { get; private set; }

	public static bool Exists
	{
		get
		{
			return Instance != null;
		}
	}

	private void Awake()
	{
		Instance = this;
		CreateMeshGUIContainers();
		CreateMeshGUIRecyclers();
		CmuneEventHandler.AddListener<CameraWidthChangeEvent>(OnCameraRectChange);
	}

	private void Start()
	{
		_guiCamera.gameObject.SetActive(true);
	}

	private void OnCameraRectChange(CameraWidthChangeEvent ev)
	{
		if (_guiCamera != null)
		{
			_guiCamera.rect = new Rect(0f, 0f, AutoMonoBehaviour<CameraRectController>.Instance.NormalizedWidth, 1f);
		}
	}

	private void CreateMeshGUIContainers()
	{
		_meshTextContainer = new GameObject("MeshTextContainer");
		_meshTextContainer.transform.parent = base.gameObject.transform;
		_quadMeshContainer = new GameObject("QuadMeshContainer");
		_quadMeshContainer.transform.parent = base.gameObject.transform;
		_circleMeshContainer = new GameObject("CircleContainer");
		_circleMeshContainer.transform.parent = base.gameObject.transform;
		_atlasMeshContainer = new GameObject("AtlasContainer");
		_atlasMeshContainer.transform.parent = base.gameObject.transform;
	}

	private void CreateMeshGUIRecyclers()
	{
		_meshTextRecycler = new ObjectRecycler(_meshTextObject, 5, _meshTextContainer);
		_quadMeshRecycler = new ObjectRecycler(_quadMeshObject, 5, _quadMeshContainer);
		_circleMeshRecycler = new ObjectRecycler(_circleMeshObject, 5, _circleMeshContainer);
		_atlasMeshRecycler = new ObjectRecycler(_atlasMeshObject, 5, _atlasMeshContainer);
	}

	public GameObject CreateMeshText(GameObject parentObject = null)
	{
		GameObject nextFree = _meshTextRecycler.GetNextFree();
		if (parentObject != null)
		{
			nextFree.transform.parent = parentObject.transform;
		}
		return nextFree;
	}

	public void FreeMeshText(GameObject meshTextObject)
	{
		meshTextObject.transform.parent = _meshTextContainer.transform;
		_meshTextRecycler.FreeObject(meshTextObject);
	}

	public GameObject CreateQuadMesh(GameObject parentObject = null)
	{
		GameObject nextFree = _quadMeshRecycler.GetNextFree();
		if (parentObject != null)
		{
			nextFree.transform.parent = parentObject.transform;
		}
		return nextFree;
	}

	public void FreeQuadMesh(GameObject quadMeshObject)
	{
		quadMeshObject.GetComponent<Renderer>().material.mainTextureOffset = Vector2.zero;
		quadMeshObject.transform.parent = _quadMeshContainer.transform;
		_quadMeshRecycler.FreeObject(quadMeshObject);
	}

	public GameObject CreateAtlasMesh(GameObject parentObject = null)
	{
		GameObject nextFree = _atlasMeshRecycler.GetNextFree();
		if (parentObject != null)
		{
			nextFree.transform.parent = parentObject.transform;
		}
		return nextFree;
	}

	public void FreeAtlasMesh(GameObject atlasMeshObject)
	{
		atlasMeshObject.transform.parent = _atlasMeshContainer.transform;
		_atlasMeshRecycler.FreeObject(atlasMeshObject);
	}

	public GameObject CreateCircleMesh(GameObject parentObject = null)
	{
		GameObject nextFree = _circleMeshRecycler.GetNextFree();
		if (parentObject != null)
		{
			nextFree.transform.parent = parentObject.transform;
		}
		return nextFree;
	}

	public void FreeCircleMesh(GameObject circleMeshObject)
	{
		circleMeshObject.transform.parent = _circleMeshContainer.transform;
		_circleMeshRecycler.FreeObject(circleMeshObject);
	}

	public Vector3 TransformPosFromScreenToWorld(Vector2 screenPos)
	{
		float orthographicSize = _guiCamera.orthographicSize;
		float num = orthographicSize * _guiCamera.aspect;
		Vector3 zero = Vector3.zero;
		zero.x = screenPos.x / (float)Screen.width * num * 2f - num;
		zero.y = screenPos.y / (float)Screen.height * orthographicSize * 2f - orthographicSize;
		zero.y = 0f - zero.y;
		return zero;
	}

	public Vector2 TransformPosFromWorldToScreen(Vector3 worldPos)
	{
		float orthographicSize = _guiCamera.orthographicSize;
		float num = orthographicSize / (float)Screen.height * (float)Screen.width;
		return new Vector2((worldPos.x + num) * (float)Screen.width / num / 2f, (0f - worldPos.y + orthographicSize) * (float)Screen.height / orthographicSize / 2f);
	}

	public Vector3 TransformSizeFromScreenToWorld(Vector2 screenSize)
	{
		float orthographicSize = _guiCamera.orthographicSize;
		float num = orthographicSize / (float)Screen.height * (float)Screen.width;
		return new Vector2(screenSize.x / (float)Screen.width * num * 2f, screenSize.y / (float)Screen.height * orthographicSize * 2f);
	}

	public Vector2 TransformSizeFromWorldToScreen(Vector3 worldSize)
	{
		float orthographicSize = _guiCamera.orthographicSize;
		float num = orthographicSize / (float)Screen.height * (float)Screen.width;
		return new Vector2(worldSize.x / num / 2f * (float)Screen.width, worldSize.y / orthographicSize / 2f * (float)Screen.height);
	}
}
