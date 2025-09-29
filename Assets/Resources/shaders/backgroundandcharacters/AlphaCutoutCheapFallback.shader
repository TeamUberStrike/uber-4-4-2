Shader "AngryBots/AlphaCutoutCheapFallback" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
 _Normal ("Normal", 2D) = "bump" {}
 _Cube ("Cube", CUBE) = "black" {}
 _Color ("Unused main color for depth texture pickup", Color) = (1,1,1,1)
 _Cutoff ("Alpha cutoff", Range(0,1)) = 0.25
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