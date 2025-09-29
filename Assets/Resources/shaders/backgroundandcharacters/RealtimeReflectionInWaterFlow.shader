Shader "AngryBots/RealtimeReflectionInWaterFlow" {
Properties {
 _MainTex ("Base", 2D) = "white" {}
 _Normal ("Normal", 2D) = "bump" {}
 _ReflectionTex ("_ReflectionTex", 2D) = "black" {}
 _FakeReflect ("Fake reflection", 2D) = "black" {}
 _DirectionUv ("Wet scroll direction (2 samples)", Vector) = (1,1,-0.2,-0.2)
 _TexAtlasTiling ("Tex atlas tiling", Vector) = (8,8,4,4)
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