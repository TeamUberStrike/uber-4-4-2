Shader "Cross Platform Shaders/Characters/Hydra" {
Properties {
 _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
 _BumpMap ("Normalmap", 2D) = "bump" {}
 _Caustics ("_Caustics", 2D) = "black" {}
 _Cube ("Reflection Cubemap", CUBE) = "black" {}
 _Color ("Main Color", Color) = (0,0.313726,0.65098,1)
 _WaterColor_Dark ("Dark Water Color", Color) = (1,1,1,1)
 _ReflectColor ("Reflection Color", Color) = (0.72549,0.992157,1,0.501961)
 _Specular ("_Specular", Float) = 2
 _Gloss ("_Gloss", Float) = 1
 _Tiling ("_Tiling", Float) = 1.5
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