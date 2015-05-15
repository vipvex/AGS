Shader "Turbo Forest/TurboForest" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Bump ("Bump (RGB)", 2D) = "white" {}
		_SaturationRandomize ("Saturation randomize", Range(0,1)) = 0.5
		_BrightnessRandomize ("Brightness randomize", Range(0,1)) = 0.5
		_YShift ("Y Shift", FLOAT) = 0.0
	}	
	SubShader
	{
		Tags {"Queue"="AlphaTest" "IgnoreProjector"="True"}
		
		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert  
			#pragma fragment frag 
			#pragma exclude_renderers xbox360
		
			uniform sampler2D _MainTex;        
			uniform sampler2D _Bump;
			fixed4 _LightColor0;
			uniform float _SaturationRandomize;
			uniform float _BrightnessRandomize;
			uniform float _YShift;

			struct vertexInput  
			{
				float4 center : POSITION;
				float4 corner : TEXCOORD1;
				float4 tex : TEXCOORD0;
			};

			struct vertexOutput 
			{
				float4 pos : SV_POSITION;
				float4 tex : TEXCOORD0;
				float3 lightDirection : TEXCOORD1;
			};
			
			inline float rand(float2 co)
			{
				return frac(sin(dot(co ,float2(12.9898, 78.233))) * 43758.5453);
			}

			vertexOutput vert(vertexInput input)  
			{
				vertexOutput output;
				
				output.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, float4(input.center.x, input.center.y, input.center.z, 1.0) ) - float4(input.corner.x, -input.corner.y + _YShift, 0.0, 0.0));

				float3 dir=normalize(_WorldSpaceCameraPos.xyz - input.center.xyz);
				float d=saturate(dot(dir, float3(0, 1, 0)));
			
				output.tex = input.tex;
				output.tex.xy /= 4.0;
				output.tex.y -= .25;
			
				int row = (int)(d * 4);
				int col = (int)(d * 16);

				output.tex.y -= (1.0 / 4) * row;
				output.tex.x += (1.0 / 4) * (col - row * 4);
			
				output.tex.z = rand(input.center.xz);
				output.tex.w = rand(input.center.zx);
				
				output.lightDirection.xyz =  normalize( mul(UNITY_MATRIX_MVP,-_WorldSpaceLightPos0).xyz );
				
				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				fixed4 c = tex2D(_MainTex, input.tex.xy);
				if (c.a < .9) discard;
				
				c.rgb = lerp(c.rgb, Luminance(c.rgb), _SaturationRandomize * input.tex.z);
				c.rgb = lerp(c.rgb, fixed3(0,0,0), _BrightnessRandomize * input.tex.w);

				fixed3 bump = tex2D(_Bump, input.tex.xy).xyz;
				bump.xyz = (bump.xyz - 0.5) * 2;
				bump.y =- bump.y;
				
				float att = saturate(dot( bump, input.lightDirection ));
			
				c.rgb = c.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb * 2 + c.rgb * att * _LightColor0.rgb * 2;

				return c;
			}
			ENDCG
		}
	
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
			
			uniform sampler2D _MainTex; 
			uniform float4x4 _LightMatrix0;
			uniform float _YShift;
			
			struct vertexInput  
			{
				float4 center : POSITION;
				float4 corner : TEXCOORD1;
				float4 tex : TEXCOORD0;
			};
			
			struct v2f 
			{
				float4 tex : TEXCOORD1;
				V2F_SHADOW_CASTER;
			};

			v2f vert( vertexInput v )
			{
				v2f o;
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, float4(v.center.x, v.center.y, v.center.z, 1.0) ) - float4(v.corner.x, -v.corner.y + _YShift, 0.0, 0.0));
			    o.pos.z += unity_LightShadowBias.x;
			    float clamped = max(o.pos.z, o.pos.w * UNITY_NEAR_CLIP_VALUE); 
			    o.pos.z = lerp(o.pos.z, clamped, unity_LightShadowBias.y);
				
				float d = saturate(dot(normalize(ObjSpaceLightDir(v.center)), float3(0, 1, 0)));
			
				o.tex = v.tex;
				o.tex.xy /= 4.0;
				o.tex.y -= .25;
			
				int row = (int)(d * 4);
				int col = (int)(d * 16);

				o.tex.y -= (1.0 / 4) * row;
				o.tex.x += (1.0 / 4) * (col - row * 4);

			    return o;
			}

			float4 frag( v2f i ) : COLOR
			{
				if (tex2D(_MainTex, i.tex.xy).a < 0.9) discard;
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
 
	}
	
	FallBack "Diffuse"
}
