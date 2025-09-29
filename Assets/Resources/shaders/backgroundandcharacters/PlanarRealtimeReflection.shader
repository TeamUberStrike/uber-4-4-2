Shader "AngryBots/PlanarRealtimeReflection" {
Properties {
 _MainTex ("Base", 2D) = "white" {}
 _ReflectionTex ("Internal reflection", 2D) = "black" {}
 _CubeReflTex ("Cube", CUBE) = "black" {}
 _Normals ("Normal", 2D) = "bump" {}
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