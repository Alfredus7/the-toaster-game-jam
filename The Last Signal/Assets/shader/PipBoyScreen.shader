Shader "UI/RetroScreen"
{
    Properties
    {
        _HoloColor ("Screen Color", Color) = (0.2, 1, 0.3, 1)
        _ScanSpeed ("Scan Speed", Range(0, 10)) = 2
        _ScanDensity ("Scan Density", Range(10, 1000)) = 300
        _FlickerSpeed ("Flicker Speed", Range(0, 10)) = 3
        _FlickerIntensity ("Flicker Intensity", Range(0, 1)) = 0.3
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "PipBoyUI"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _HoloColor;
            float _ScanSpeed;
            float _ScanDensity;
            float _FlickerSpeed;
            float _FlickerIntensity;
            float _GlowIntensity;
            half4 _TextureSampleAdd;

            // Función de ruido optimizada
            float simple_noise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Muestreo de textura base (para compatibilidad con UI Image)
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) + _TextureSampleAdd;
                
                // Escaneo con tamaño constante en pantalla
float2 screenUV = i.pos.xy / _ScreenParams.xy; // Coordenadas normalizadas de pantalla
float scanTime = _Time.y * _ScanSpeed;
float scan = frac(screenUV.y * _ScanDensity + scanTime);
scan = 1.0 - abs(scan * 2.0 - 1.0); // Onda triangular
scan = pow(scan, 2.0); // Suavizado del brillo de línea


                // Parpadeo optimizado (sin funciones trigonométricas costosas)
                float flickerTime = _Time.y * _FlickerSpeed;
                float flicker = frac(flickerTime) * 2.0 - 1.0;
                flicker = 1.0 - flicker * flicker; // Parábola en lugar de seno
                flicker = lerp(1.0, flicker, _FlickerIntensity);

                // Color final combinado con la textura original
                half3 glowColor = _HoloColor.rgb * _GlowIntensity;
                half3 finalColor = texColor.rgb * glowColor * scan * flicker;
                
                // Alpha que respeta la textura original y el color del vértice
                half alpha = texColor.a * _HoloColor.a * i.color.a;

                return half4(finalColor * i.color.rgb, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/2D/Sprite-Unlit-Default"
}