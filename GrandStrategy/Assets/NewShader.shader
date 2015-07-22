// Shader created with Shader Forge v1.05 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.05;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,rprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:0,bsrc:0,bdst:1,culm:0,dpts:2,wrdp:True,dith:0,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:9873,x:33409,y:32666,varname:node_9873,prsc:2|diff-2953-OUT;n:type:ShaderForge.SFN_NormalVector,id:7867,x:32452,y:32552,prsc:2,pt:True;n:type:ShaderForge.SFN_Abs,id:4498,x:32668,y:32552,varname:node_4498,prsc:2|IN-7867-OUT;n:type:ShaderForge.SFN_ChannelBlend,id:2953,x:33170,y:32673,varname:node_2953,prsc:2,chbt:0|M-9085-OUT,R-9983-RGB,G-122-RGB,B-5287-RGB;n:type:ShaderForge.SFN_Tex2d,id:9983,x:32889,y:32721,ptovrint:False,ptlb:node_9983,ptin:_node_9983,varname:node_9983,prsc:2,tex:0863012444c8db1469618774bbb5eb29,ntxv:0,isnm:False|UVIN-7414-OUT;n:type:ShaderForge.SFN_Tex2d,id:122,x:32889,y:32915,ptovrint:False,ptlb:node_122,ptin:_node_122,varname:node_122,prsc:2,tex:a8fbd4fedd6a2b842995940c6d09ef44,ntxv:0,isnm:False|UVIN-7243-OUT;n:type:ShaderForge.SFN_FragmentPosition,id:4612,x:32452,y:32755,varname:node_4612,prsc:2;n:type:ShaderForge.SFN_Multiply,id:9085,x:32889,y:32552,varname:node_9085,prsc:2|A-4498-OUT,B-4498-OUT;n:type:ShaderForge.SFN_Append,id:7414,x:32686,y:32721,varname:node_7414,prsc:2|A-4612-Y,B-4612-Z;n:type:ShaderForge.SFN_Append,id:7243,x:32686,y:32886,varname:node_7243,prsc:2|A-4612-Z,B-4612-X;n:type:ShaderForge.SFN_Append,id:8186,x:32670,y:33052,varname:node_8186,prsc:2|A-4612-X,B-4612-Y;n:type:ShaderForge.SFN_Tex2d,id:5287,x:32876,y:33093,ptovrint:False,ptlb:node_122_copy,ptin:_node_122_copy,varname:_node_122_copy,prsc:2,tex:1d37abd1854078b4391984637d53f67b,ntxv:0,isnm:False|UVIN-8186-OUT;proporder:9983-122-5287;pass:END;sub:END;*/

Shader "Shader Forge/NewShader" {
    Properties {
        _node_9983 ("node_9983", 2D) = "white" {}
        _node_122 ("node_122", 2D) = "white" {}
        _node_122_copy ("node_122_copy", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _node_9983; uniform float4 _node_9983_ST;
            uniform sampler2D _node_122; uniform float4 _node_122_ST;
            uniform sampler2D _node_122_copy; uniform float4 _node_122_copy_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                LIGHTING_COORDS(2,3)
                UNITY_FOG_COORDS(4)
                #ifndef LIGHTMAP_OFF
                    float4 uvLM : TEXCOORD5;
                #else
                    float3 shLight : TEXCOORD5;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.normalDir = mul(_Object2World, float4(v.normal,0)).xyz;
                o.posWorld = mul(_Object2World, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
/////// Vectors:
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 indirectDiffuse = float3(0,0,0);
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float3 node_4498 = abs(normalDirection);
                float3 node_9085 = (node_4498*node_4498);
                float2 node_7414 = float2(i.posWorld.g,i.posWorld.b);
                float4 _node_9983_var = tex2D(_node_9983,TRANSFORM_TEX(node_7414, _node_9983));
                float2 node_7243 = float2(i.posWorld.b,i.posWorld.r);
                float4 _node_122_var = tex2D(_node_122,TRANSFORM_TEX(node_7243, _node_122));
                float2 node_8186 = float2(i.posWorld.r,i.posWorld.g);
                float4 _node_122_copy_var = tex2D(_node_122_copy,TRANSFORM_TEX(node_8186, _node_122_copy));
                float3 diffuse = (directDiffuse + indirectDiffuse) * (node_9085.r*_node_9983_var.rgb + node_9085.g*_node_122_var.rgb + node_9085.b*_node_122_copy_var.rgb);
/// Final Color:
                float3 finalColor = diffuse;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "ForwardAdd"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _node_9983; uniform float4 _node_9983_ST;
            uniform sampler2D _node_122; uniform float4 _node_122_ST;
            uniform sampler2D _node_122_copy; uniform float4 _node_122_copy_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                LIGHTING_COORDS(2,3)
                #ifndef LIGHTMAP_OFF
                    float4 uvLM : TEXCOORD4;
                #else
                    float3 shLight : TEXCOORD4;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.normalDir = mul(_Object2World, float4(v.normal,0)).xyz;
                o.posWorld = mul(_Object2World, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
/////// Vectors:
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 node_4498 = abs(normalDirection);
                float3 node_9085 = (node_4498*node_4498);
                float2 node_7414 = float2(i.posWorld.g,i.posWorld.b);
                float4 _node_9983_var = tex2D(_node_9983,TRANSFORM_TEX(node_7414, _node_9983));
                float2 node_7243 = float2(i.posWorld.b,i.posWorld.r);
                float4 _node_122_var = tex2D(_node_122,TRANSFORM_TEX(node_7243, _node_122));
                float2 node_8186 = float2(i.posWorld.r,i.posWorld.g);
                float4 _node_122_copy_var = tex2D(_node_122_copy,TRANSFORM_TEX(node_8186, _node_122_copy));
                float3 diffuse = directDiffuse * (node_9085.r*_node_9983_var.rgb + node_9085.g*_node_122_var.rgb + node_9085.b*_node_122_copy_var.rgb);
/// Final Color:
                float3 finalColor = diffuse;
                return fixed4(finalColor * 1,0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
