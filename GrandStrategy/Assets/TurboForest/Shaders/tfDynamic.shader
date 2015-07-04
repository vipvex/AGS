Shader "TF/Dynamic"
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ShadowTex ("Shadow (RGB)", 2D) = "white" {}
		_Cutout ("Cutout", Range(0,1)) = 0.5
		_SaturationRandomize ("Saturation randomize", Range(0,1)) = 0.5
		_BrightnessRandomize ("Brightness randomize", Range(0,1)) = 0.5
	}	
	SubShader
	{
		Tags {
			"Queue"="Transparent-1"
			"DisableBatching" = "True"
			}
		
		Pass
		{
			Tags { "LightMode" = "Always" }
			
			CGPROGRAM

			#include "UnityCG.cginc"
			#pragma vertex vert  
			#pragma fragment frag 
			#pragma multi_compile_fog
			#pragma exclude_renderers xbox360
			
			#define TF_DYNAMIC
			
			#include "tf.cginc"
 
			ENDCG
		} // pass
		
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
			
			#define TF_SHADOW_TEXTURE _ShadowTex
			#define TF_DYNAMIC
			
			#include "tfShadow.cginc"

			ENDCG
		}			
	} // subshader
	
	FallBack "Diffuse"
}
