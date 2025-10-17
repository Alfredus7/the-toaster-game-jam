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
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.1
        _GlitchSpeed ("Glitch Speed", Range(0, 5)) = 1
        _NoiseIntensity ("Noise Intensity", Range(0, 0.1)) = 0.02
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
            float _GlitchIntensity;
            float _GlitchSpeed;
            float _NoiseIntensity;
            half4 _TextureSampleAdd;

            // Función de ruido rápido y optimizado
            float fast_rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Ruido más suave usando interpolación
            float smooth_noise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                
                float a = fast_rand(i);
                float b = fast_rand(i + float2(1.0, 0.0));
                float c = fast_rand(i + float2(0.0, 1.0));
                float d = fast_rand(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
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
                // Coordenadas de pantalla para efectos constantes
                float2 screenUV = i.pos.xy / _ScreenParams.xy;
                
                // Efecto de escaneo optimizado
                float scanTime = _Time.y * _ScanSpeed;
                float scan = frac(screenUV.y * _ScanDensity + scanTime);
                scan = 1.0 - abs(scan * 2.0 - 1.0);
                scan = pow(scan, 2.0);

                // Parpadeo optimizado
                float flickerTime = _Time.y * _FlickerSpeed;
                float flicker = frac(flickerTime) * 2.0 - 1.0;
                flicker = 1.0 - flicker * flicker;
                flicker = lerp(1.0, flicker, _FlickerIntensity);

                // Efectos glitch más elaborados pero optimizados
                float glitchTime = _Time.y * _GlitchSpeed;
                
                // Glitch horizontal (desplazamiento de líneas)
                float lineGlitch = fast_rand(float2(floor(screenUV.y * 50.0 + glitchTime), glitchTime)) - 0.5;
                float2 glitchedUV = i.uv + float2(lineGlitch * _GlitchIntensity * 0.1, 0);
                
                // Glitch vertical intermitente
                float verticalGlitch = step(0.98, fast_rand(float2(glitchTime, 0)));
                glitchedUV.y += verticalGlitch * (fast_rand(float2(glitchTime, 1.0)) - 0.5) * _GlitchIntensity * 0.05;
                
                // Ruido estático sutil
                float staticNoise = fast_rand(screenUV * 100.0 + glitchTime);
                staticNoise = staticNoise * 2.0 - 1.0;
                
                // Muestreo de textura con efectos glitch aplicados
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, glitchedUV) + _TextureSampleAdd;
                
                // Aplicar ruido estático al color
                texColor.rgb += staticNoise * _NoiseIntensity;

                // Color final con todos los efectos
                half3 glowColor = _HoloColor.rgb * _GlowIntensity;
                half3 finalColor = texColor.rgb * glowColor * scan * flicker;
                
                // Alpha que respeta la textura original
                half alpha = texColor.a * _HoloColor.a * i.color.a;

                return half4(finalColor * i.color.rgb, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/2D/Sprite-Unlit-Default"
}