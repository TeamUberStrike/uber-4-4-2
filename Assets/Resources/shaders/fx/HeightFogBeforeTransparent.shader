Shader "AngryBots/FX/HeightFogBeforeTransparent" {
Properties {
 _FogColor ("FogColor", Color) = (0,0,0,0)
 _Exponent ("Exponent", Range(0.1,4)) = 0.3
 _Y ("Y", Range(-30,30)) = 0
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