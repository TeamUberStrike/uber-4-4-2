Shader "Self-Illumin/AngryBots/InterlacePatternBlend" {
Properties {
 _MainTex ("Base", 2D) = "white" {}
 _TintColor ("TintColor", Color) = (1,1,1,1)
 _InterlacePattern ("InterlacePattern", 2D) = "white" {}
 _Illum ("_Illum", 2D) = "white" {}
 _EmissionLM ("Emission (Lightmapper)", Float) = 1
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