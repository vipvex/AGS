Shader "TF/ShadowMesh"
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}	
	SubShader
	{
		Tags {"Queue"="Alphatest"  "RenderType"="Alphatest"}
		
		Pass 
	    {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			Fog {Mode Off}
			ZWrite On ZTest LEqual Cull Off
			Offset 1, 1
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			#pragma exclude_renderers xbox360
			
			#define TF_MESH
			
			#include "tfShadow.cginc"

			ENDCG
		}
 
	}
	
	FallBack "Diffuse"
}
