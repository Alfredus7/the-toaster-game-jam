Shader "PipBoyScreen"
{
    Properties
    {
        _HoloColor ("Screen Color", Color) = (0.2, 1, 0.3, 1)
        _ScanSpeed ("Scan Speed", Range(0, 10)) = 2
        _ScanDensity ("Scan Density", Range(10, 1000)) = 300
        _FlickerSpeed ("Flicker Speed", Range(0, 10)) = 3
        _FlickerIntensity ("Flicker Intensity", Range(0, 1)) = 0.3
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2
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
            Name "PipBoyPass"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _HoloColor;
            float _ScanSpeed;
            float _ScanDensity;
            float _FlickerSpeed;
            float _FlickerIntensity;
            float _GlowIntensity;
            float _Alpha;

            // Ruido mínimo (para parpadeo leve)
            float rand(float2 n)
            {
                return frac(sin(dot(n, float2(12.9898, 78.233))) * 43758.5453);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Líneas de escaneo verticales (bajando)
                float scan = sin((i.uv.y + _Time.y * _ScanSpeed) * _ScanDensity) * 0.5 + 0.5;

                // Parpadeo global
                float flicker = sin(_Time.y * _FlickerSpeed) * 0.5 + 0.5;
                flicker = lerp(1.0, flicker, _FlickerIntensity);

                // Color holográfico
                float3 color = _HoloColor.rgb * scan * flicker * _GlowIntensity;

                // Alpha con leve modulación por flicker
                float alpha = _Alpha * (0.8 + flicker * 0.2);

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
