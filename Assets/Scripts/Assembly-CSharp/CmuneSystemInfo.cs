using System.Text;
using UnityEngine;

public class CmuneSystemInfo
{
	public string OperatingSystem;

	public string ProcessorType;

	public string ProcessorCount;

	public string SystemMemorySize;

	public string GraphicsDeviceName;

	public string GraphicsDeviceVendor;

	public string GraphicsDeviceVersion;

	public string GraphicsMemorySize;

	public string GraphicsShaderLevel;

	public string GraphicsPixelFillRate;

	public string SupportsImageEffects;

	public string SupportsRenderTextures;

	public string SupportsShadows;

	public string SupportsVertexPrograms;

	public string Platform;

	public string RunInBackground;

	public string AbsoluteURL;

	public string DataPath;

	public string BackgroundLoadingPriority;

	public string SrcValue;

	public string SystemLanguage;

	public string TargetFrameRate;

	public string UnityVersion;

	public string Gravity;

	public string BounceThreshold;

	public string MaxAngularVelocity;

	public string MinPenetrationForPenalty;

	public string PenetrationPenaltyForce;

	public string SleepAngularVelocity;

	public string SleepVelocity;

	public string SolverIterationCount;

	public string CurrentResolution;

	public string AmbientLight;

	public string FlareStrength;

	public string FogEnabled;

	public string FogColor;

	public string FogDensity;

	public string HaloStrength;

	public string CurrentQualityLevel;

	public string AnisotropicFiltering;

	public string MasterTextureLimit;

	public string MaxQueuedFrames;

	public string PixelLightCount;

	public string ShadowCascades;

	public string ShadowDistance;

	public string SoftVegetationEnabled;

	public string BrowserIdentifier;

	public string BrowserVersion;

	public string BrowserMajorVersion;

	public string BrowserMinorVersion;

	public string BrowserEngine;

	public string BrowserEngineVersion;

	public string BrowserUserAgent;

	public CmuneSystemInfo()
	{
		OperatingSystem = SystemInfo.operatingSystem;
		ProcessorType = SystemInfo.processorType;
		ProcessorCount = SystemInfo.processorCount.ToString("N0");
		SystemMemorySize = SystemInfo.systemMemorySize.ToString("N0") + "Mb";
		GraphicsDeviceName = SystemInfo.graphicsDeviceName;
		GraphicsDeviceVendor = SystemInfo.graphicsDeviceVendor;
		GraphicsDeviceVersion = SystemInfo.graphicsDeviceVersion;
		GraphicsMemorySize = SystemInfo.graphicsMemorySize.ToString("N0") + "Mb";
		GraphicsShaderLevel = GetShaderLevelName(SystemInfo.graphicsShaderLevel);
		GraphicsPixelFillRate = SystemInfo.graphicsPixelFillrate.ToString();
		SupportsImageEffects = SystemInfo.supportsImageEffects.ToString();
		SupportsRenderTextures = SystemInfo.supportsRenderTextures.ToString();
		SupportsShadows = SystemInfo.supportsShadows.ToString();
		SupportsVertexPrograms = SystemInfo.supportsVertexPrograms.ToString();
		Platform = Application.platform.ToString();
		RunInBackground = Application.runInBackground.ToString();
		AbsoluteURL = Application.absoluteURL;
		DataPath = Application.dataPath;
		BackgroundLoadingPriority = Application.backgroundLoadingPriority.ToString();
		SrcValue = Application.srcValue;
		SystemLanguage = Application.systemLanguage.ToString();
		TargetFrameRate = Application.targetFrameRate.ToString("N0");
		UnityVersion = Application.unityVersion;
		Gravity = Physics.gravity.ToString();
		BounceThreshold = Physics.bounceThreshold.ToString("N2");
		MaxAngularVelocity = Physics.maxAngularVelocity.ToString("N2");
		MinPenetrationForPenalty = Physics.minPenetrationForPenalty.ToString("N2");
		SleepAngularVelocity = Physics.sleepAngularVelocity.ToString("N2");
		SleepVelocity = Physics.sleepVelocity.ToString("N2");
		SolverIterationCount = Physics.solverIterationCount.ToString("N2");
		CurrentResolution = "X " + Screen.width + ", Y " + Screen.height + ", Refresh " + Screen.currentResolution.refreshRate.ToString("N0") + "Hz";
		AmbientLight = RenderSettings.ambientLight.ToString();
		FlareStrength = RenderSettings.flareStrength.ToString("N2");
		FogEnabled = RenderSettings.fog.ToString();
		FogColor = RenderSettings.fogColor.ToString();
		FogDensity = RenderSettings.fogDensity.ToString("N2");
		HaloStrength = RenderSettings.haloStrength.ToString("N2");
		CurrentQualityLevel = QualitySettings.GetQualityLevel().ToString();
		AnisotropicFiltering = QualitySettings.anisotropicFiltering.ToString();
		MasterTextureLimit = QualitySettings.masterTextureLimit.ToString();
		MaxQueuedFrames = QualitySettings.maxQueuedFrames.ToString();
		PixelLightCount = QualitySettings.pixelLightCount.ToString();
		ShadowCascades = QualitySettings.shadowCascades.ToString();
		ShadowDistance = QualitySettings.shadowDistance.ToString("N2");
		SoftVegetationEnabled = QualitySettings.softVegetation.ToString();
		BrowserIdentifier = (BrowserVersion = (BrowserMajorVersion = (BrowserMinorVersion = (BrowserEngine = (BrowserEngineVersion = (BrowserUserAgent = "No information."))))));
	}

	private string GetShaderLevelName(int shaderLevel)
	{
		switch (shaderLevel)
		{
		case 30:
			return "Shader Model 3.0 - We like!";
		case 20:
			return "Shader Model 2.x";
		case 10:
			return "Shader Model 1.x";
		case 7:
			return "Fixed function, DirectX 7.";
		case 6:
			return "Fixed function, DirectX 6.";
		case 5:
			return "Fixed function, DirectX 5.";
		default:
			return "Unknown";
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("///SYSTEM INFO REPORT///");
		stringBuilder.AppendLine(string.Empty);
		stringBuilder.AppendLine("UNITY SYSTEM INFO");
		stringBuilder.AppendLine("   Operating System: " + OperatingSystem);
		stringBuilder.AppendLine("   ProcessorType: " + ProcessorType);
		stringBuilder.AppendLine("   ProcessorCount: " + ProcessorCount);
		stringBuilder.AppendLine("   SystemMemorySize: " + SystemMemorySize);
		stringBuilder.AppendLine("   GraphicsDeviceName: " + GraphicsDeviceName);
		stringBuilder.AppendLine("   GraphicsDeviceVendor: " + GraphicsDeviceVendor);
		stringBuilder.AppendLine("   GraphicsDeviceVersion: " + GraphicsDeviceVersion);
		stringBuilder.AppendLine("   GraphicsMemorySize: " + GraphicsMemorySize);
		stringBuilder.AppendLine("   GraphicsShaderLevel: " + GraphicsShaderLevel);
		stringBuilder.AppendLine("   GraphicsPixelFillRate: " + GraphicsPixelFillRate);
		stringBuilder.AppendLine("   SupportsImageEffects: " + SupportsImageEffects);
		stringBuilder.AppendLine("   SupportsRenderTextures: " + SupportsRenderTextures);
		stringBuilder.AppendLine("   SupportsShadows: " + SupportsShadows);
		stringBuilder.AppendLine("   SupportsVertexPrograms: " + SupportsVertexPrograms);
		stringBuilder.AppendLine(string.Empty);
		stringBuilder.AppendLine("UNITY APPLICATION INFO");
		stringBuilder.AppendLine("   Platform: " + Platform);
		stringBuilder.AppendLine("   Run In Background: " + RunInBackground);
		stringBuilder.AppendLine("   Absolute URL: " + AbsoluteURL);
		stringBuilder.AppendLine("   Data Path: " + DataPath);
		stringBuilder.AppendLine("   Background Loading Priority: " + BackgroundLoadingPriority);
		stringBuilder.AppendLine("   Src Value: " + SrcValue);
		stringBuilder.AppendLine("   System Language: " + SystemLanguage);
		stringBuilder.AppendLine("   Target Frame Rate: " + TargetFrameRate);
		stringBuilder.AppendLine("   Unity Version: " + UnityVersion);
		stringBuilder.AppendLine(string.Empty);
		stringBuilder.AppendLine("UNITY PHYSICS INFO");
		stringBuilder.AppendLine("   Gravity: " + Gravity);
		stringBuilder.AppendLine("   Bounce Threshold: " + BounceThreshold);
		stringBuilder.AppendLine("   Max Angular Velocity: " + MaxAngularVelocity);
		stringBuilder.AppendLine("   Min Penetration For Penalty: " + MinPenetrationForPenalty);
		stringBuilder.AppendLine("   Penetration Penalty Force: " + PenetrationPenaltyForce);
		stringBuilder.AppendLine("   Sleep Angular Velocity: " + SleepAngularVelocity);
		stringBuilder.AppendLine("   Sleep Velocity: " + SleepVelocity);
		stringBuilder.AppendLine("   Solver Iteration Count: " + SolverIterationCount);
		stringBuilder.AppendLine(string.Empty);
		stringBuilder.AppendLine("UNITY RENDERING INFO");
		stringBuilder.AppendLine("   Current Resolution: " + CurrentResolution);
		stringBuilder.AppendLine("   Ambient Light: " + AmbientLight);
		stringBuilder.AppendLine("   Flare Strength: " + FlareStrength);
		stringBuilder.AppendLine("   Fog Enabled: " + FogEnabled);
		stringBuilder.AppendLine("   Fog Color: " + FogColor);
		stringBuilder.AppendLine("   Fog Density: " + FogDensity);
		stringBuilder.AppendLine("   Halo Strength: " + HaloStrength);
		stringBuilder.AppendLine(string.Empty);
		stringBuilder.AppendLine("UNITY QUALITY SETTINGS INFO");
		stringBuilder.AppendLine("   Current Quality Level: " + CurrentQualityLevel);
		stringBuilder.AppendLine("   Anisotropic Filtering: " + AnisotropicFiltering);
		stringBuilder.AppendLine("   Master Texture Limit: " + MasterTextureLimit);
		stringBuilder.AppendLine("   Max Queued Frames: " + MaxQueuedFrames);
		stringBuilder.AppendLine("   Pixel Light Count: " + PixelLightCount);
		stringBuilder.AppendLine("   Shadow Cascades: " + ShadowCascades);
		stringBuilder.AppendLine("   Shadow Distance: " + ShadowDistance);
		stringBuilder.AppendLine("   Soft Vegetation Enabled: " + SoftVegetationEnabled);
		stringBuilder.AppendLine(string.Empty);
		stringBuilder.AppendLine("BROWSER INFO");
		stringBuilder.AppendLine("   Browser Identifier: " + BrowserIdentifier);
		stringBuilder.AppendLine("   Browser Version: " + BrowserVersion);
		stringBuilder.AppendLine("   Browser Major Version: " + BrowserMajorVersion);
		stringBuilder.AppendLine("   Browser Minor Version: " + BrowserMinorVersion);
		stringBuilder.AppendLine("   Browser Engine: " + BrowserEngine);
		stringBuilder.AppendLine("   Browser Engine Version: " + BrowserEngineVersion);
		stringBuilder.AppendLine("   Browser User Agent: " + BrowserUserAgent);
		stringBuilder.AppendLine("GAME SERVER INFO");
		if (Singleton<GameServerManager>.Instance.PhotonServerCount > 0)
		{
			foreach (GameServerView photonServer in Singleton<GameServerManager>.Instance.PhotonServerList)
			{
				stringBuilder.AppendLine(string.Format("   Server:{0} Ping:{1}", photonServer.ConnectionString, photonServer.Latency));
			}
		}
		else
		{
			stringBuilder.AppendLine("   No Game Server Information available.");
		}
		stringBuilder.AppendLine("END OF REPORT");
		return stringBuilder.ToString();
	}

	public string ToHTML()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("<h2>SYSTEM INFO REPORT</h2>");
		stringBuilder.AppendLine("<h3>UNITY SYSTEM INFO</h3>");
		stringBuilder.AppendLine("        Operating System: " + OperatingSystem);
		stringBuilder.AppendLine("<br/>   ProcessorType: " + ProcessorType);
		stringBuilder.AppendLine("<br/>   ProcessorCount: " + ProcessorCount);
		stringBuilder.AppendLine("<br/>   SystemMemorySize: " + SystemMemorySize);
		stringBuilder.AppendLine("<br/>   GraphicsDeviceName: " + GraphicsDeviceName);
		stringBuilder.AppendLine("<br/>   GraphicsDeviceVendor: " + GraphicsDeviceVendor);
		stringBuilder.AppendLine("<br/>   GraphicsDeviceVersion: " + GraphicsDeviceVersion);
		stringBuilder.AppendLine("<br/>   GraphicsMemorySize: " + GraphicsMemorySize);
		stringBuilder.AppendLine("<br/>   GraphicsShaderLevel: " + GraphicsShaderLevel);
		stringBuilder.AppendLine("<br/>   GraphicsPixelFillRate: " + GraphicsPixelFillRate);
		stringBuilder.AppendLine("<br/>   SupportsImageEffects: " + SupportsImageEffects);
		stringBuilder.AppendLine("<br/>   SupportsRenderTextures: " + SupportsRenderTextures);
		stringBuilder.AppendLine("<br/>   SupportsShadows: " + SupportsShadows);
		stringBuilder.AppendLine("<br/>   SupportsVertexPrograms: " + SupportsVertexPrograms);
		stringBuilder.AppendLine("<br/><h3>UNITY APPLICATION INFO</h3>");
		stringBuilder.AppendLine("        Platform: " + Platform);
		stringBuilder.AppendLine("<br/>   Run In Background: " + RunInBackground);
		stringBuilder.AppendLine("<br/>   Absolute URL: " + AbsoluteURL);
		stringBuilder.AppendLine("<br/>   Data Path: " + DataPath);
		stringBuilder.AppendLine("<br/>   Background Loading Priority: " + BackgroundLoadingPriority);
		stringBuilder.AppendLine("<br/>   Src Value: " + SrcValue);
		stringBuilder.AppendLine("<br/>   System Language: " + SystemLanguage);
		stringBuilder.AppendLine("<br/>   Target Frame Rate: " + TargetFrameRate);
		stringBuilder.AppendLine("<br/>   Unity Version: " + UnityVersion);
		stringBuilder.AppendLine("<br/><h3>UNITY PHYSICS INFO</h3>");
		stringBuilder.AppendLine("        Gravity: " + Gravity);
		stringBuilder.AppendLine("<br/>   Bounce Threshold: " + BounceThreshold);
		stringBuilder.AppendLine("<br/>   Max Angular Velocity: " + MaxAngularVelocity);
		stringBuilder.AppendLine("<br/>   Min Penetration For Penalty: " + MinPenetrationForPenalty);
		stringBuilder.AppendLine("<br/>   Penetration Penalty Force: " + PenetrationPenaltyForce);
		stringBuilder.AppendLine("<br/>   Sleep Angular Velocity: " + SleepAngularVelocity);
		stringBuilder.AppendLine("<br/>   Sleep Velocity: " + SleepVelocity);
		stringBuilder.AppendLine("<br/>   Solver Iteration Count: " + SolverIterationCount);
		stringBuilder.AppendLine("<br/><h3>UNITY RENDERING INFO</h3>");
		stringBuilder.AppendLine("        Current Resolution: " + CurrentResolution);
		stringBuilder.AppendLine("<br/>   Ambient Light: " + AmbientLight);
		stringBuilder.AppendLine("<br/>   Flare Strength: " + FlareStrength);
		stringBuilder.AppendLine("<br/>   Fog Enabled: " + FogEnabled);
		stringBuilder.AppendLine("<br/>   Fog Color: " + FogColor);
		stringBuilder.AppendLine("<br/>   Fog Density: " + FogDensity);
		stringBuilder.AppendLine("<br/>   Halo Strength: " + HaloStrength);
		stringBuilder.AppendLine("<br/><h3>UNITY QUALITY SETTINGS INFO</h3>");
		stringBuilder.AppendLine("        Current Quality Level: " + CurrentQualityLevel);
		stringBuilder.AppendLine("<br/>   Anisotropic Filtering: " + AnisotropicFiltering);
		stringBuilder.AppendLine("<br/>   Master Texture Limit: " + MasterTextureLimit);
		stringBuilder.AppendLine("<br/>   Max Queued Frames: " + MaxQueuedFrames);
		stringBuilder.AppendLine("<br/>   Pixel Light Count: " + PixelLightCount);
		stringBuilder.AppendLine("<br/>   Shadow Cascades: " + ShadowCascades);
		stringBuilder.AppendLine("<br/>   Shadow Distance: " + ShadowDistance);
		stringBuilder.AppendLine("<br/>   Soft Vegetation Enabled: " + SoftVegetationEnabled);
		stringBuilder.AppendLine("<br/><h3>BROWSER INFO</h3>");
		stringBuilder.AppendLine("        Browser Identifier: " + BrowserIdentifier);
		stringBuilder.AppendLine("<br/>   Browser Version: " + BrowserVersion);
		stringBuilder.AppendLine("<br/>   Browser Major Version: " + BrowserMajorVersion);
		stringBuilder.AppendLine("<br/>   Browser Minor Version: " + BrowserMinorVersion);
		stringBuilder.AppendLine("<br/>   Browser Engine: " + BrowserEngine);
		stringBuilder.AppendLine("<br/>   Browser Engine Version: " + BrowserEngineVersion);
		stringBuilder.AppendLine("<br/>   Browser User Agent: " + BrowserUserAgent);
		stringBuilder.AppendLine("<br/><h3>GAME SERVER INFO</h3>");
		if (Singleton<GameServerManager>.Instance.PhotonServerCount > 0)
		{
			foreach (GameServerView photonServer in Singleton<GameServerManager>.Instance.PhotonServerList)
			{
				stringBuilder.AppendLine(string.Format(" Server:{0} Ping:{1}<br/>", photonServer.ConnectionString, photonServer.Latency));
			}
		}
		else
		{
			stringBuilder.AppendLine("No Game Server Information available.<br/>");
		}
		stringBuilder.AppendLine("<h3>END OF REPORT</h3>");
		return stringBuilder.ToString();
	}
}
