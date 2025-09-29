Shader "CMune/Mobile/Water" {
Properties {
 _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
 _BumpMap ("Normalmap", 2D) = "bump" {}
 _Cube ("Reflection Cubemap", CUBE) = "black" {}
 _Specular ("_Specular", Float) = 2
 _Gloss ("_Gloss", Float) = 1
 _Tiling ("_Tiling", Float) = 1
 _Opacity ("_Opacity", Float) = 0.8
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