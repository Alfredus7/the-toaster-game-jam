Shader "Digital/GlitchedHoloScreen"
{
    Properties
    {
        _BaseMap("Base Texture", 2D) = "white" {}
        _HoloColor ("Screen Tint", Color) = (0.2, 1, 0.3, 1)
        _ScanSpeed ("Scan Speed", Range(0, 10)) = 2
        _ScanDensity ("Scan Density", Range(10, 1000)) = 300
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.2
        _GlitchSpeed ("Glitch Speed", Range(0, 10)) = 4
        _Alpha ("Alpha", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "GlitchedPass"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float4 _HoloColor;
            float _ScanSpeed;
            float _ScanDensity;
            float _GlowIntensity;
            float _GlitchIntensity;
            float _GlitchSpeed;
            float _Alpha;
            float4 _BaseMap_ST; // para TRANSFORM_TEX

            // Pseudo-aleatorio (estático, determinista)
            float rand(float2 n)
            {
                return frac(sin(dot(n, float2(12.9898,78.233))) * 43758.5453);
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float t = _Time.y * _GlitchSpeed;

                // Escaneo vertical (estética holográfica)
                float scan = sin((i.uv.y + _Time.y * _ScanSpeed) * _ScanDensity) * 0.5 + 0.5;

                // Máscara de glitch por líneas (discreto)
                float lineNoise = rand(float2(floor(i.uv.y * 200.0), floor(t)));
                float glitchLine = step(0.98, lineNoise); // pocas líneas afectadas

                // Desplazamientos UV causados por glitch
                float horizShift = (rand(float2(floor(t * 10.0), i.uv.y * 100.0)) - 0.5) * _GlitchIntensity * glitchLine * 0.1;
                float vertJitter = (rand(i.uv * 100.0 + floor(t))) - 0.5;
                vertJitter *= _GlitchIntensity * 0.02;

                float2 glitchUV = i.uv;
                glitchUV.x += horizShift;
                glitchUV.y += vertJitter;

                // Muestreo seguro de la textura
                float4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, glitchUV);

                // Aplicar color holográfico + escaneo + brillo
                float3 color = tex.rgb * _HoloColor.rgb * (0.5 + scan * 0.5) * _GlowIntensity;

                // alpha principal tomado de la textura y control global
                float alpha = tex.a * _Alpha;

                // Retorno final (clamped)
                return half4(saturate(color), saturate(alpha));
            }

            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
