Shader "Turbo Forest/Diffuse_VertexColor"
{
	Properties 
	{
		_Color ("Main Color", COLOR) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		float4 _Color;

		struct Input 
		{
			float2 uv_MainTex;
			float4 color: Color; // Vertex color
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb * _Color.rgb * IN.color.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
