Shader "Toon/ToonBase"
{
    Properties
    {
        [MainTexture]_BaseMap("Texture", 2D) = "white" {}
        [MainColor]_BaseColor("Color", Color) = (1,1,1,1)
        _Steps("Steps", Range(1,25)) = 3
        _Offset("Lit Offset", Range(-1,1.1)) = 0
    }

    SubShader
    {
        Tags { 
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalPipeline"
            "UniversalMaterialType" = "Unlit"
        }
        LOD 300

        // Main forward lit pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float3 viewDirWS  : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                half   _Steps;
                half   _Offset;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, o);
                
                VertexPositionInputs vpos = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs vnor  = GetVertexNormalInputs(IN.normalOS);
                
                o.positionCS = vpos.positionCS;
                o.positionWS = vpos.positionWS;
                o.normalWS   = vnor.normalWS;
                o.viewDirWS  = GetWorldSpaceNormalizeViewDir(vpos.positionWS);
                o.uv         = TRANSFORM_TEX(IN.uv, _BaseMap);
                o.shadowCoord = TransformWorldToShadowCoord(o.positionWS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                Light mainLight = GetMainLight(i.shadowCoord);
                
                half3 n = normalize(i.normalWS);
                half ndl = dot(n, mainLight.direction) + _Offset;

                // Siempre usar el efecto segmentado (toon) ya que se eliminó el toggle
                half toon = floor(saturate(ndl) * _Steps) / _Steps;

                half3 ambient = SampleSH(n) * 0.1;
                half3 direct  = mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation * toon;

                #ifdef _ADDITIONAL_LIGHTS
                uint count = GetAdditionalLightsCount();
                for (uint iL = 0u; iL < count; ++iL)
                {
                    Light light = GetAdditionalLight(iL, i.positionWS);
                    half addNdl = dot(n, light.direction) + _Offset;
                    half addToon = floor(saturate(addNdl) * _Steps) / _Steps;
                    direct += light.color * light.distanceAttenuation * light.shadowAttenuation * addToon;
                }
                #endif

                half3 color = baseTex.rgb * _BaseColor.rgb * (direct + ambient);
                return half4(color, 1);
            }
            ENDHLSL
        }

        // ShadowCaster pass
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Simple Lit"
}