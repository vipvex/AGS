			
			
			 
			uniform sampler2D _MainTex;        
			uniform float _SaturationRandomize;
			uniform float _BrightnessRandomize;
			uniform float _Cutout;
			
	#ifndef TF_DYNAMIC			
			uniform sampler2D _Bump;
			uniform fixed4 _LightColor0;
			uniform float _LightMult;
	#endif
	#ifdef TF_DISTANCE_CULL
			uniform float _CullingNear;
	#endif
			struct vertexInput  
			{
				float4 center : POSITION;
				float4 corner : TEXCOORD1;
				float3 normal : NORMAL;
				float4 tex : TEXCOORD0;
				float4 color : COLOR;
			};

			struct vertexOutput 
			{
				float4 pos : SV_POSITION;
				float4 tex : TEXCOORD0;
			#ifndef TF_DYNAMIC	
				fixed3 lightDirection : TEXCOORD1;
			#endif
			#ifdef TF_DISTANCE_CULL
				half distanceCull : TEXCOORD2;
			#endif
				fixed3 color : TEXCOORD3;
				
				fixed3 N: TEXCOORD4;
				fixed3 B: TEXCOORD5;
			#ifndef TF_DISABLE_FOG	
				UNITY_FOG_COORDS(6)
			#endif
			};
			
			inline fixed rand(float2 co)
			{
				return frac(sin(dot(co ,half2(12.9898, 78.233))) * 43758.5453);
			}
			
			vertexOutput vert(vertexInput i)  
			{
				vertexOutput o;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,o);
				
#ifndef TF_DYNAMIC			
	
	#ifndef TF_AMBIENT_ONLY
	
		#ifdef TF_MESH
				
			#ifdef TF_BACK_PASS
				o.N = -i.normal;
				o.B = cross(o.N, fixed3(0,1,0));
			#else
				o.N = i.normal;
				o.B = -cross(o.N, fixed3(0,1,0));
			#endif
				
				o.lightDirection = _WorldSpaceLightPos0.xyz;

		#else
			
				o.lightDirection.xyz =  normalize( mul(mul(UNITY_MATRIX_MV, _World2Object), _WorldSpaceLightPos0).xyz );
		
		#endif
	
	#endif
				
#endif

#ifdef TF_DISTANCE_CULL
				o.distanceCull = length( _WorldSpaceCameraPos.xz - mul(_Object2World, i.center).xz );
#endif	
				o.tex = i.tex;

#ifdef TF_MESH
				o.pos = mul(UNITY_MATRIX_MVP, i.center);
#else
	
	#ifdef TF_CYLINDRICAL_MODE
				o.pos = mul(UNITY_MATRIX_MV, float4(i.center.x, i.center.y + i.corner.y, i.center.z, 1));
				o.pos = mul(UNITY_MATRIX_P, o.pos + float4(-i.corner.x, 0, 0, 0) );
	#else
				
				o.pos = mul(UNITY_MATRIX_MV, float4(i.center.x, i.center.y, i.center.z, 1));
				o.pos = mul(UNITY_MATRIX_P, o.pos + float4(-i.corner.x, i.corner.y, 0, 0));
				
		#ifndef TF_DYNAMIC
				fixed3 dir = normalize(_WorldSpaceCameraPos.xyz - mul(_Object2World, i.center).xyz);
				
				fixed d = saturate(dot(dir, fixed3(0, 1, 0)));
				
				if (dot(UNITY_MATRIX_IT_MV[1].xyz, fixed3(0, 1, 0))<0)
					d=.99;
				
				o.tex.xy /= 4.0;
				o.tex.y -= .25;
			
				int row = (int)(d * 4);
				int col = (int)(d * 16);

				o.tex.y -= (1.0 / 4) * row;
				o.tex.x += (1.0 / 4) * (col - row * 4);
		#endif
	
	#endif
	
#endif
				o.tex.z = rand(i.center.xz);
				o.tex.w = rand(i.center.zx);
				
				o.color = i.color.rgb;
				
			#ifndef TF_DISABLE_FOG
				UNITY_TRANSFER_FOG(o,o.pos);
			#endif
			
				return o;
			}
			
			float4 frag(vertexOutput i) : COLOR
			{
				fixed4 c = tex2D(_MainTex, i.tex.xy);
				
#ifdef TF_DISTANCE_CULL
				if (i.distanceCull - rand(i.pos.xy) * 10 < _CullingNear) discard;
#endif				
			#ifndef TF_DISABLE_ALPHATEST
				clip(c.a - _Cutout);
			#endif
			
				c.rgb *= i.color;
				c.rgb = lerp(c.rgb, Luminance(c.rgb), _SaturationRandomize * i.tex.z);
				c.rgb *= 1.0 - _BrightnessRandomize * i.tex.w;

	#ifndef TF_DYNAMIC
			
		#ifndef TF_AMBIENT_ONLY
				fixed3 bump = (tex2D(_Bump, i.tex.xy).xyz - 0.5) * 2;
				
			#ifdef TF_MESH
				bump = bump.x * i.B + fixed3(0,bump.y,0) + bump.z * i.N;
			#endif
			
				c.rgb = c.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb + 
						c.rgb *  ( (dot( bump, i.lightDirection ) + 1.0) * 0.5 )  * 
						_LightColor0.rgb * _LightMult;
						
		#else
				c.rgb = c.rgb * _LightMult;
		#endif
	#endif
	
			#ifndef TF_DISABLE_FOG
				UNITY_APPLY_FOG(i.fogCoord, c);
			#endif
				
				return c;
			}



