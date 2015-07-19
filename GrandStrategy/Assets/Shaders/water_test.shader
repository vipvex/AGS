// Shader created with Shader Forge v1.05 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.05;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,rprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:2,bsrc:0,bdst:0,culm:0,dpts:2,wrdp:False,dith:0,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:6064,x:33344,y:32670,varname:node_6064,prsc:2|emission-1411-OUT;n:type:ShaderForge.SFN_Tex2d,id:5889,x:32622,y:32825,ptovrint:False,ptlb:node_5889,ptin:_node_5889,varname:node_5889,prsc:2,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Color,id:2323,x:32622,y:32662,ptovrint:False,ptlb:node_2323,ptin:_node_2323,varname:node_2323,prsc:2,glob:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:198,x:32858,y:32734,varname:node_198,prsc:2|A-2323-RGB,B-2323-A,C-5889-RGB;n:type:ShaderForge.SFN_DepthBlend,id:287,x:32858,y:32982,varname:node_287,prsc:2|DIST-9316-OUT;n:type:ShaderForge.SFN_Slider,id:9316,x:32515,y:33030,ptovrint:False,ptlb:node_9316,ptin:_node_9316,varname:node_9316,prsc:2,min:-10,cur:0,max:10;n:type:ShaderForge.SFN_Add,id:1411,x:33043,y:32816,varname:node_1411,prsc:2|A-198-OUT,B-287-OUT;proporder:5889-2323-9316;pass:END;sub:END;*/

Shader "Shader Forge/water_test" {
    Properties {
        _node_5889 ("node_5889", 2D) = "white" {}
        _node_2323 ("node_2323", Color) = (0.5,0.5,0.5,1)
        _node_9316 ("node_9316", Range(-10, 10)) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            uniform sampler2D _CameraDepthTexture;
            uniform sampler2D _node_5889; uniform float4 _node_5889_ST;
            uniform float4 _node_2323;
            uniform float _node_9316;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 projPos : TEXCOORD1;
                UNITY_FOG_COORDS(2)
                #ifndef LIGHTMAP_OFF
                    float4 uvLM : TEXCOORD3;
                #else
                    float3 shLight : TEXCOORD3;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                float sceneZ = max(0,LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
                float partZ = max(0,i.projPos.z - _ProjectionParams.g);
/////// Vectors:
////// Lighting:
////// Emissive:
                float4 _node_5889_var = tex2D(_node_5889,TRANSFORM_TEX(i.uv0, _node_5889));
                float3 node_198 = (_node_2323.rgb*_node_2323.a*_node_5889_var.rgb);
                float node_287 = saturate((sceneZ-partZ)/_node_9316);
                float3 emissive = (node_198+node_287);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
