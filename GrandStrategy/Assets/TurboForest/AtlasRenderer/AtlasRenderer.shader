Shader "TF/AtlasRenderer"
{
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cutout ("Cutout", Range(0,1)) = 0.5
		_Contrast ("Contrast", Range(0,10)) = 1
		_centerHeight("Center height", Float) = 1
		[MaterialToggle] _normalsMode("Normals Mode", Float) = 0
		[MaterialToggle] _simulateTFLight("Simulate TF light", Float) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Cull Off

		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert  
			#pragma fragment frag 
			#pragma exclude_renderers xbox360
		
			sampler2D _MainTex;
			fixed4 _Color;
			float _normalsMode;
			float _simulateTFLight;
			float _centerHeight;
			float _Cutout;
			float _Contrast;
			
			fixed4 _LightColor0;

			struct vertexInput  
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 uv : TEXCOORD0;
			};

			struct vertexOutput 
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
				float3 worldPos: TEXCOORD1;
				float3 lightDirection : TEXCOORD2;
				float3 normal : TEXCOORD3;
			};

			vertexOutput vert(vertexInput i)  
			{
				vertexOutput o;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,o);

				o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
				o.worldPos = i.vertex;
				o.normal = i.normal;				

				o.uv = i.uv;

				o.lightDirection.xyz =  normalize( mul(mul(UNITY_MATRIX_MV, _World2Object), _WorldSpaceLightPos0).xyz );

				return o;
			}
			
			// this no need to be optimized, not realtime shader
			float4 frag(vertexOutput i) : COLOR
			{

				fixed4 c = tex2D (_MainTex, i.uv);
				
				if(c.a<_Cutout)
					discard;
				
				c.rgb *= _Color.rgb;
				c.rgb = c.rgb - _Contrast * (c.rgb - 1.0) * c.rgb *(c.rgb - 0.5);

				float calcNormals = _normalsMode;
				if(_simulateTFLight)
					calcNormals = 1;

				if(calcNormals>.5)
				{
					// calc normal direction from tree center to it extents
					float3 n = i.worldPos;
					n.y -= _centerHeight; // up center for different center of trees
					if(n.y<0) n.y=0;
					
					n = normalize(n); // world normal ready
					
					n = mul(mul(UNITY_MATRIX_MV, _World2Object),float4(n.x,n.y,n.z,0)).xyz; // convert normal to view space (this necessary for realtime TF shaders)

					n=(n+1.0)*0.5; // pack normal
					
					//n = i.normal.xyz;
					//n = mul(UNITY_MATRIX_MV,float4(n.x,n.y,n.z,0)).xyz;

					if(_simulateTFLight>.5) // just to see how trree will be attenuated
					{
						float att = dot( n, i.lightDirection );
						c.rgb = c.rgb + c.rgb * att;
					}
					else
					{
						c.rgb = n; // tree normals render pass
					}

				}

				return c; // tree diffuse render pass
			}

			ENDCG
		}
	} 
	//FallBack "Diffuse"
}
