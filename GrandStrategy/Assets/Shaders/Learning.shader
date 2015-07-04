// Shader created with Shader Forge v1.05 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.05;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,rprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:0,bsrc:0,bdst:1,culm:0,dpts:2,wrdp:True,dith:0,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.1280277,fgcg:0.1953466,fgcb:0.2352941,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:5228,x:33469,y:33007,varname:node_5228,prsc:2|diff-4291-OUT;n:type:ShaderForge.SFN_Tex2d,id:6313,x:32706,y:32848,ptovrint:False,ptlb:texture,ptin:_texture,varname:node_6313,prsc:2,tex:56c62eab28431764c8e50392c71fe979,ntxv:2,isnm:False|UVIN-3097-OUT;n:type:ShaderForge.SFN_ChannelBlend,id:4291,x:33003,y:33068,varname:node_4291,prsc:2,chbt:0|M-6313-RGB,R-6587-RGB,G-9991-RGB,B-2222-RGB;n:type:ShaderForge.SFN_Tex2d,id:6587,x:32706,y:33033,varname:node_6587,prsc:2,tex:27c56bd80de6c004bb931e4edca39742,ntxv:0,isnm:False|TEX-7-TEX;n:type:ShaderForge.SFN_Tex2d,id:9991,x:32706,y:33217,ptovrint:False,ptlb:desert,ptin:_desert,varname:node_9991,prsc:2,tex:0863012444c8db1469618774bbb5eb29,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:2222,x:32706,y:33402,ptovrint:False,ptlb:forest,ptin:_forest,varname:node_2222,prsc:2,tex:0118132842878d548b3f704ba8331077,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:62,x:32445,y:33111,varname:node_62,prsc:2|A-2251-UVOUT,B-5177-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:7,x:32415,y:32932,ptovrint:False,ptlb:node_7,ptin:_node_7,varname:node_7,tex:27c56bd80de6c004bb931e4edca39742,ntxv:0,isnm:False;n:type:ShaderForge.SFN_TexCoord,id:2251,x:32229,y:33045,varname:node_2251,prsc:2,uv:0;n:type:ShaderForge.SFN_ValueProperty,id:5177,x:32215,y:33248,ptovrint:False,ptlb:node_5177,ptin:_node_5177,varname:node_5177,prsc:2,glob:False,v1:0;n:type:ShaderForge.SFN_Tex2d,id:2832,x:31870,y:32639,ptovrint:False,ptlb:node_2832,ptin:_node_2832,varname:node_2832,prsc:2,tex:33c14fe30ac09f743971bb51a6184c27,ntxv:2,isnm:False;n:type:ShaderForge.SFN_ComponentMask,id:9155,x:32060,y:32639,varname:node_9155,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-2832-RGB;n:type:ShaderForge.SFN_RemapRange,id:2295,x:32254,y:32639,varname:node_2295,prsc:2,frmn:0,frmx:1,tomn:-1,tomx:1|IN-9155-OUT;n:type:ShaderForge.SFN_Add,id:3097,x:32469,y:32578,varname:node_3097,prsc:2|A-1893-UVOUT,B-2295-OUT;n:type:ShaderForge.SFN_TexCoord,id:1893,x:32254,y:32473,varname:node_1893,prsc:2,uv:0;proporder:6313-9991-2222-7-2832;pass:END;sub:END;*/

Shader "Shader Forge/Learning" {
    Properties {
        _texture ("texture", 2D) = "black" {}
        _desert ("desert", 2D) = "white" {}
        _forest ("forest", 2D) = "white" {}
        _node_7 ("node_7", 2D) = "white" {}
        _node_2832 ("node_2832", 2D) = "black" {}
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
            uniform sampler2D _texture; uniform float4 _texture_ST;
            uniform sampler2D _desert; uniform float4 _desert_ST;
            uniform sampler2D _forest; uniform float4 _forest_ST;
            uniform sampler2D _node_7; uniform float4 _node_7_ST;
            uniform sampler2D _node_2832; uniform float4 _node_2832_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
                UNITY_FOG_COORDS(5)
                #ifndef LIGHTMAP_OFF
                    float4 uvLM : TEXCOORD6;
                #else
                    float3 shLight : TEXCOORD6;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
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
                float4 _node_2832_var = tex2D(_node_2832,TRANSFORM_TEX(i.uv0, _node_2832));
                float2 node_3097 = (i.uv0+(_node_2832_var.rgb.rg*2.0+-1.0));
                float4 _texture_var = tex2D(_texture,TRANSFORM_TEX(node_3097, _texture));
                float4 node_6587 = tex2D(_node_7,TRANSFORM_TEX(i.uv0, _node_7));
                float4 _desert_var = tex2D(_desert,TRANSFORM_TEX(i.uv0, _desert));
                float4 _forest_var = tex2D(_forest,TRANSFORM_TEX(i.uv0, _forest));
                float3 diffuse = (directDiffuse + indirectDiffuse) * (_texture_var.rgb.r*node_6587.rgb + _texture_var.rgb.g*_desert_var.rgb + _texture_var.rgb.b*_forest_var.rgb);
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
            uniform sampler2D _texture; uniform float4 _texture_ST;
            uniform sampler2D _desert; uniform float4 _desert_ST;
            uniform sampler2D _forest; uniform float4 _forest_ST;
            uniform sampler2D _node_7; uniform float4 _node_7_ST;
            uniform sampler2D _node_2832; uniform float4 _node_2832_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
                #ifndef LIGHTMAP_OFF
                    float4 uvLM : TEXCOORD5;
                #else
                    float3 shLight : TEXCOORD5;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
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
                float4 _node_2832_var = tex2D(_node_2832,TRANSFORM_TEX(i.uv0, _node_2832));
                float2 node_3097 = (i.uv0+(_node_2832_var.rgb.rg*2.0+-1.0));
                float4 _texture_var = tex2D(_texture,TRANSFORM_TEX(node_3097, _texture));
                float4 node_6587 = tex2D(_node_7,TRANSFORM_TEX(i.uv0, _node_7));
                float4 _desert_var = tex2D(_desert,TRANSFORM_TEX(i.uv0, _desert));
                float4 _forest_var = tex2D(_forest,TRANSFORM_TEX(i.uv0, _forest));
                float3 diffuse = directDiffuse * (_texture_var.rgb.r*node_6587.rgb + _texture_var.rgb.g*_desert_var.rgb + _texture_var.rgb.b*_forest_var.rgb);
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
