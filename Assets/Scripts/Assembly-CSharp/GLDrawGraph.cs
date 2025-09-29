using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLDrawGraph : MonoBehaviour
{
	public enum VIEW
	{
		SPLIT = 0,
		ACCUMULATED = 1
	}

	private const int SAMPLE_COUNT = 200;

	private static Material glMaterial;

	private Rect graph1Rect = new Rect(10f, 10f, 200f, 80f);

	private int xOffset;

	private int yOffset;

	private int xLimit;

	private int yLimit;

	private bool doDrag;

	private Dictionary<int, List<float>> _graphArrays;

	private Dictionary<int, float> _graphArraysMax;

	private static Dictionary<int, string> _captions = new Dictionary<int, string>(10);

	private Dictionary<int, float> _lastSamples;

	private float[] _nullNodes;

	private float[] _accumulatedNodes;

	private int _currentSample;

	public VIEW ViewStyle;

	public Color[] Colors;

	public float SampleFrequency = 0.01f;

	public bool DrawGraph = true;

	public static int GraphId;

	public static GLDrawGraph Instance { get; private set; }

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
		_graphArrays = new Dictionary<int, List<float>>(10);
		_graphArraysMax = new Dictionary<int, float>(10);
		_lastSamples = new Dictionary<int, float>(10);
		_accumulatedNodes = new float[200];
		_nullNodes = new float[200];
	}

	private void Start()
	{
		StartCoroutine(startSampleLoop());
		createGLMaterial();
	}

	private void Update()
	{
		if (DrawGraph)
		{
			if (Input.GetMouseButtonDown(0) && graph1Rect.Contains(Input.mousePosition))
			{
				xOffset = (int)(Input.mousePosition.x - graph1Rect.x);
				yOffset = (int)(Input.mousePosition.y - graph1Rect.y);
				doDrag = true;
			}
			if (graph1Rect.Contains(Input.mousePosition))
			{
				graph1Rect.height += (int)(Input.GetAxis("Mouse ScrollWheel") * 40f);
			}
			if (doDrag)
			{
				graph1Rect = new Rect(Input.mousePosition.x - (float)xOffset, Input.mousePosition.y - (float)yOffset, graph1Rect.width, graph1Rect.height);
			}
			if (Input.GetMouseButtonUp(0))
			{
				doDrag = false;
			}
			yLimit = (int)((float)Screen.height - graph1Rect.height);
			xLimit = (int)((float)Screen.width - graph1Rect.width);
			graph1Rect = new Rect(Mathf.Clamp(graph1Rect.x, 2f, xLimit), Mathf.Clamp(graph1Rect.y, 2f, yLimit), graph1Rect.width, Mathf.Clamp(graph1Rect.height, 40f, Screen.height));
		}
	}

	private IEnumerator startSampleLoop()
	{
		while (true)
		{
			if (DrawGraph && SampleFrequency > 0f)
			{
				yield return new WaitForSeconds(SampleFrequency);
				_currentSample++;
				_currentSample %= 200;
				foreach (List<float> nodes in _graphArrays.Values)
				{
					nodes[_currentSample] = 0f;
				}
			}
			else
			{
				yield return new WaitForSeconds(1f);
			}
		}
	}

	public static void AddSampleValue(int graph, float v)
	{
		if (Exists && Instance.DrawGraph)
		{
			if (!Instance._graphArrays.ContainsKey(graph))
			{
				Instance._graphArrays[graph] = new List<float>(Instance._nullNodes);
				Instance._graphArraysMax[graph] = 0f;
			}
			Instance._graphArrays[graph][Instance._currentSample] = v;
			Instance._graphArraysMax[graph] = Mathf.Max(Instance._graphArraysMax[graph], Mathf.Abs(v));
			Instance._lastSamples[graph] = v;
		}
	}

	public static void AddCaption(int graph, string caption)
	{
		_captions[graph] = caption;
	}

	private static void createGLMaterial()
	{
		if (!glMaterial)
		{
			glMaterial = new Material("Shader \"Lines/Colored Blended\" {SubShader { Pass {  Blend SrcAlpha OneMinusSrcAlpha  ZWrite Off Cull Off Fog { Mode Off }  BindChannels { Bind \"vertex\", vertex Bind \"color\", color }} } }");
			glMaterial.hideFlags = HideFlags.HideAndDontSave;
			glMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	private void OnGUI()
	{
		if (!DrawGraph || ViewStyle != VIEW.SPLIT)
		{
			return;
		}
		GUI.color = Color.black;
		float num = graph1Rect.height / (float)_graphArrays.Count;
		int num2 = _graphArrays.Count - 1;
		foreach (KeyValuePair<int, List<float>> graphArray in _graphArrays)
		{
			string value = "<no caption> ";
			if (_captions.TryGetValue(graphArray.Key, out value))
			{
				GUI.Label(new Rect(graph1Rect.x + 5f, (float)Screen.height - graph1Rect.yMax + (float)num2 * num, 200f, 20f), value);
			}
			float value2;
			if (_lastSamples.TryGetValue(graphArray.Key, out value2))
			{
				GUI.Label(new Rect(graph1Rect.xMax - 50f, (float)Screen.height - graph1Rect.yMax + (float)num2 * num, 200f, 20f), value2.ToString("f2"));
			}
			num2--;
		}
		GUI.color = Color.white;
	}

	private void OnPostRender()
	{
		if (!DrawGraph || _graphArrays.Count == 0 || Colors.Length == 0)
		{
			return;
		}
		glMaterial.SetPass(0);
		GL.PushMatrix();
		GL.LoadPixelMatrix();
		float num = graph1Rect.height / (float)_graphArrays.Count;
		switch (ViewStyle)
		{
		case VIEW.SPLIT:
		{
			draw2DRectOutline(new Rect(graph1Rect.x - 1f, graph1Rect.y - 1f, graph1Rect.width + 1f, graph1Rect.height + 1f), new Color(1f, 1f, 1f, 0.5f), new Color(1f, 1f, 1f, 1f), 0);
			int num4 = 0;
			foreach (KeyValuePair<int, List<float>> graphArray in _graphArrays)
			{
				Color color2 = Colors[num4 % Colors.Length];
				for (int k = 0; k < graphArray.Value.Count; k++)
				{
					int index = (k + _currentSample) % 200;
					float left = graph1Rect.x + (float)k;
					float num5 = graphArray.Value[index];
					draw2DLine(new Rect(left, graph1Rect.y + (float)num4 * num, 1f, num5 * num), color2, 0);
				}
				num4++;
			}
			break;
		}
		case VIEW.ACCUMULATED:
		{
			draw2DRectOutline(new Rect(309f, 9f, graph1Rect.width + 1f, graph1Rect.height + 1f), new Color(1f, 1f, 1f, 0.5f), new Color(1f, 1f, 1f, 1f), 0);
			for (int i = 0; i < _accumulatedNodes.Length; i++)
			{
				_accumulatedNodes[i] = 0f;
			}
			int num2 = 0;
			foreach (List<float> value in _graphArrays.Values)
			{
				Color color = Colors[num2 % Colors.Length];
				for (int j = 0; j < value.Count; j++)
				{
					int num3 = (j + _currentSample) % 200;
					float left = 310 + j;
					draw2DLine(new Rect(left, 10f + _accumulatedNodes[num3] * num, 1f, value[num3] * num), color, 0);
					_accumulatedNodes[num3] += value[num3];
				}
				num2++;
			}
			break;
		}
		}
		GL.PopMatrix();
	}

	private void draw2DRect(Rect rect, Color color, int depth)
	{
		GL.Begin(7);
		GL.Color(color);
		GL.Vertex3(rect.x, rect.y + rect.height, depth);
		GL.Vertex3(rect.x + rect.width, rect.y + rect.height, depth);
		GL.Vertex3(rect.x + rect.width, rect.y, depth);
		GL.Vertex3(rect.x, rect.y, depth);
		GL.End();
	}

	private void draw2DRectOutline(Rect rect, Color color, Color outlineColor, int depth)
	{
		draw2DRect(rect, color, depth);
		draw2DOutline(rect, outlineColor, depth);
	}

	private void draw2DOutline(Rect rect, Color color, int depth)
	{
		GL.Begin(1);
		GL.Color(color);
		GL.Vertex3(rect.x, rect.y, depth);
		GL.Vertex3(rect.x + rect.width, rect.y, depth);
		GL.Vertex3(rect.x, rect.y + rect.height, depth);
		GL.Vertex3(rect.x + rect.width, rect.y + rect.height, depth);
		GL.Vertex3(rect.x, rect.y, depth);
		GL.Vertex3(rect.x, rect.y + rect.height, depth);
		GL.Vertex3(rect.x + rect.width, rect.y, depth);
		GL.Vertex3(rect.x + rect.width, rect.y + rect.height, depth);
		GL.End();
	}

	private void draw2DLine(Rect rect, Color color, int depth)
	{
		GL.Begin(1);
		GL.Color(color);
		GL.Vertex3(rect.x, rect.y, depth);
		GL.Vertex3(rect.x, rect.y + rect.height, depth);
		GL.End();
	}

	private void draw2DPoint(Rect rect, Color color, int depth)
	{
		GL.Begin(1);
		GL.Color(color);
		GL.Vertex3(rect.x, rect.y + rect.height, depth);
		GL.Vertex3(rect.x, rect.y + rect.height + 1f, depth);
		GL.End();
	}
}
