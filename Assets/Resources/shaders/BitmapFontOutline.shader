Shader "Cross Platform Shaders/Outline" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _AlphaMin ("Alpha Min", Range(0,1)) = 0.49
 _AlphaMax ("Alpha Max", Range(0,1)) = 0.54
 _ShadowColor ("Shadow Color", Color) = (0.3,0.3,0.3,1)
 _ShadowAlphaMin ("Shadow Alpha Min", Range(0,1)) = 0.28
 _ShadowAlphaMax ("Shadow Alpha Max", Range(0,1)) = 0.54
 _ShadowOffsetU ("Shadow u-offset", Range(-1,1)) = 0
 _ShadowOffsetV ("Shadow v-offset", Range(-1,1)) = 0
 _MainTex ("Base (RGB)", 2D) = "white" {}
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