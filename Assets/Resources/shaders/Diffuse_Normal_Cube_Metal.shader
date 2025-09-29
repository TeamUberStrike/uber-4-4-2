Shader "Cross Platform Shaders/Diffuse Normal Cube Metal" {
Properties {
 _Diffuse ("_Diffuse", 2D) = "gray" {}
 _Normal ("_Normal", 2D) = "bump" {}
 _Cube ("Reflection Cubemap", CUBE) = "black" {}
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