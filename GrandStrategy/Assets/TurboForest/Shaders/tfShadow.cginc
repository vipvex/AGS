

			uniform sampler2D TF_SHADOW_TEXTURE; 
			uniform float4x4 _LightMatrix0;
	#ifdef TF_DISTANCE_CULL
			uniform float _CullingNear;
	#endif
	
			struct vertexInput  
			{
				float4 center : POSITION;
				float4 corner : TEXCOORD1;
				float4 tex : TEXCOORD0;
			};
			
			struct v2f 
			{
				float4 tex : TEXCOORD1;
			#ifdef TF_DISTANCE_CULL
				float distanceCull : TEXCOORD2;
			#endif				
				V2F_SHADOW_CASTER;
			};

			v2f vert( vertexInput v )
			{
				v2f o;

				UNITY_INITIALIZE_OUTPUT(v2f, o);

			#ifdef TF_DISTANCE_CULL
				o.distanceCull = length( _WorldSpaceCameraPos.xz - mul(_Object2World, v.center).xz );
			#endif	
				
				o.tex = v.tex;
				
#ifdef TF_MESH
				o.pos = mul(UNITY_MATRIX_MVP, v.center);
				
				o.pos.z += unity_LightShadowBias.x;
			    float clamped = max(o.pos.z, o.pos.w * UNITY_NEAR_CLIP_VALUE); 
			    o.pos.z = lerp(o.pos.z, clamped, unity_LightShadowBias.y);
				
				
				
#else
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, float4(v.center.x, v.center.y, v.center.z, 1.0) ) - float4(v.corner.x, -v.corner.y, 0.0, 0.0));
			    
				o.pos.z += unity_LightShadowBias.x;
			    float clamped = max(o.pos.z, o.pos.w * UNITY_NEAR_CLIP_VALUE); 
			    o.pos.z = lerp(o.pos.z, clamped, unity_LightShadowBias.y);
				
				o.tex = v.tex;
				
			#ifndef TF_DYNAMIC
				float d = saturate(dot(normalize(ObjSpaceLightDir(v.center)), float3(0, -1, 0)));
				
				o.tex.xy /= 4.0;
				o.tex.y -= .25;
			
				int row = (int)(d * 4);
				int col = (int)(d * 16);

				o.tex.y -= (1.0 / 4) * row;
				o.tex.x += (1.0 / 4) * (col - row * 4);
				
				//o.pos.y -= (d - ((float)col) / 16) * .02;
				
			#endif
#endif			
			    return o;
			}

			float4 frag( v2f i ) : COLOR
			{
				#ifdef TF_DISTANCE_CULL
					if (i.distanceCull < _CullingNear) discard;
				#endif
				
				clip(tex2D(TF_SHADOW_TEXTURE, i.tex.xy).a - 0.5);
				
				SHADOW_CASTER_FRAGMENT(i)
			}