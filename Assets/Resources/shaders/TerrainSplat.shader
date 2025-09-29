Shader "Cross Platform Shaders/TerrainSplat" {
Properties {
 _Control ("Control (RGBA)", 2D) = "red" {}
 _Splat0 ("Layer 0 (R)", 2D) = "white" {}
 _Splat1 ("Layer 1 (G)", 2D) = "white" {}
 _Splat2 ("Layer 2 (B)", 2D) = "white" {}
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