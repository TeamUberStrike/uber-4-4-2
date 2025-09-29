Shader "Cross Platform Shaders/EasyWater7_CUBEMAP" {
Properties {
 _Color ("_Color", Color) = (1,1,1,1)
 _Texture1 ("_Texture1", 2D) = "black" {}
 _BumpMap1 ("_BumpMap1", 2D) = "black" {}
 _Texture2 ("_Texture2", 2D) = "black" {}
 _BumpMap2 ("_BumpMap2", 2D) = "black" {}
 _MainTexSpeed ("_MainTexSpeed", Float) = 0
 _Bump1Speed ("_Bump1Speed", Float) = 0
 _Texture2Speed ("_Texture2Speed", Float) = 0
 _Bump2Speed ("_Bump2Speed", Float) = 0
 _DistortionMap ("_DistortionMap", 2D) = "black" {}
 _DistortionSpeed ("_DistortionSpeed", Float) = 0
 _DistortionPower ("_DistortionPower", Float) = 0
 _Specular ("_Specular", Float) = 1
 _Gloss ("_Gloss", Float) = 0.3
 _Opacity ("_Opacity", Float) = 0
 _Reflection ("_Reflection", CUBE) = "black" {}
 _ReflectPower ("_ReflectPower", Float) = 0
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