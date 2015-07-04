Shader "TF/ShadowAtlasDistanceCull"
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CullingNear ("_CullingNear", FLOAT) = 50.0
	}	
	SubShader
	{
		Tags {"Queue"="Geometry"  "RenderType"="Opaque"}
		
		Pass 
	    {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			Fog {Mode Off}
			ZWrite On ZTest LEqual Cull Back
			Offset 1, 1
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			#pragma exclude_renderers xbox360
			
			#define TF_DISTANCE_CULL
			#include "tfShadow.cginc"

			ENDCG
		}
 
	}
	
	FallBack "Diffuse"
}
