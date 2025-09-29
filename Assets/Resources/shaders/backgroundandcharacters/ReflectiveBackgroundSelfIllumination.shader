Shader "Self-Illumin/AngryBots/ReflectiveBackgroundSelfIllumination" {
Properties {
 _MainTex ("Base", 2D) = "white" {}
 _Cube ("Cube", CUBE) = "" {}
 _Normal ("Normal", 2D) = "bump" {}
 _EmissionLM ("Emission (Lightmapper)", Float) = 0
 _OneMinusReflectivity ("OneMinusReflectivity", Range(0,1)) = 0.1
}
	//DummyShaderTextExporter
	
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Lambert
#pragma target 3.0
		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};
		void surf(Input IN, inout SurfaceOutput o)
		{
			float4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
		}
		ENDCG
	}
}