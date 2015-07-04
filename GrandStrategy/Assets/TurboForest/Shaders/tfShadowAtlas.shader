Shader "TF/ShadowAtlas" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}	
	SubShader
	{
		Tags {"Queue"="Geometry"  "RenderType"="Opaque" "DisableBatching" = "True"}
		
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
			
			#include "tfShadow.cginc"

			ENDCG
		}
 
	}
	
	FallBack "Diffuse"
}
