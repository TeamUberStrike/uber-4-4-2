Shader "AngryBots/FX/ScreenRefract" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
 _ShimmerDistort ("Distort (in RG channels)", 2D) = "black" {}
 _Distort ("Distort", Range(0,0.25)) = 0.05
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