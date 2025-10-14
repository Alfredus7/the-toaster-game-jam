Shader "ToonShader"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor]   _BaseColor("Color", Color) = (1,1,1,1)

        [Header(Detail)][Space(5)]
        [Toggle]_Segmented("Segmented", Float) = 1
        _Steps("Steps", Range(1,25)) = 3
        _Offset("Lit Offset", Range(-1,1.1)) = 0
        
        [Header(Outline)][Space(5)]
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Width", Range(0, 0.1)) = 0.01
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "UniversalMaterialType"="Lit"
            "IgnoreProjector"="True"
        }
        LOD 300

        // Outline Pass
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            Cull Front
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                half4 _OutlineColor;
            CBUFFER_END

            Varyings OutlineVert (Attributes IN)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, o);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS);
                
                // Calculate outline position
                float3 normalWS = normalInput.normalWS;
                float3 positionWS = vertexInput.positionWS;
                
                // Standard outline - simple extrusion based on normals
                float3 outlineOffset = normalWS * _OutlineWidth;
                
                positionWS += outlineOffset;
                o.positionCS = TransformWorldToHClip(positionWS);
                
                return o;
            }

            half4 OutlineFrag (Varyings i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        // Main Forward Lit Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Directivas multi_compile para luces adicionales
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fog
            
            #pragma shader_feature_local _ _SEGMENTED_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

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
                
                // Obtener la luz principal con información de sombras
                Light mainLight = GetMainLight(i.shadowCoord);
                half3 n = normalize(i.normalWS);
                half3 v = normalize(i.viewDirWS);
                half ndl = dot(n, mainLight.direction) + _Offset;

                #ifdef _SEGMENTED_ON
                    half toon = floor(saturate(ndl) * _Steps) / _Steps;
                #else
                    half toon = smoothstep(0,1,saturate(ndl));
                #endif

                half3 ambient = SampleSH(n) * 0.1;
                half3 direct  = mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation * toon;

                // Añadir luces adicionales
                #ifdef _ADDITIONAL_LIGHTS
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < additionalLightsCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, i.positionWS);
                    half addNdl = dot(n, light.direction) + _Offset;
                    
                    #ifdef _SEGMENTED_ON
                        half addToon = floor(saturate(addNdl) * _Steps) / _Steps;
                    #else
                        half addToon = smoothstep(0,1,saturate(addNdl));
                    #endif
                    
                    direct += light.color * light.distanceAttenuation * light.shadowAttenuation * addToon;
                }
                #endif

                // Color base simplificado - sin shadow color
                half3 baseColor = baseTex.rgb * _BaseColor.rgb;
                half3 finalCol = baseColor * (direct + ambient);

                return half4(finalCol, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull Back
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Simple Lit"
}