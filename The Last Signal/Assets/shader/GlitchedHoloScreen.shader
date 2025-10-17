Shader "UI/RetroGlitchedScreen"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _HoloColor ("Screen Tint", Color) = (0.2, 1, 0.3, 1)
        _ScanSpeed ("Scan Speed", Range(0, 10)) = 2
        _ScanDensity ("Scan Density", Range(10, 1000)) = 300
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2
        _GlitchLevel ("Glitch Level", Range(0, 1)) = 1.0
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
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _HoloColor;
            float _ScanSpeed;
            float _ScanDensity;
            float _GlowIntensity;
            float _GlitchLevel;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            // Función de ruido optimizada - menos operaciones trigonométricas
            float fast_rand(float2 co) {
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                
                // Jitter solo aplicado si hay glitch significativo
                float jitter = (fast_rand(float2(_Time.y, _Time.y)) * 2.0 - 1.0) * 0.02 * _GlitchLevel;
                OUT.worldPosition.xy += jitter * step(0.1, _GlitchLevel); // step es más rápido que if
                
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _HoloColor;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float t = _Time.y * 3.0;
                
                // Pre-calcular intensidades (evita cálculos repetidos)
                float glitchIntensity = _GlitchLevel * 0.1;
                float noiseIntensity = _GlitchLevel * 0.03;
                float blockSize = lerp(0.2, 0.05, _GlitchLevel);
                
                // Calcular todo el ruido de una vez
                float2 blockUV = floor(uv / blockSize) * blockSize;
                float blockNoise = fast_rand(blockUV + t);
                float staticNoise = fast_rand(uv + _Time.xx);
                
                // Aplicar distorsiones sin condicionales usando step y lerp
                float2 glitchUV = uv;
                
                // Distorsión de bloques - reemplaza if statements
                float blockDistortX = step(0.7, blockNoise) * (blockNoise - 0.7) * glitchIntensity * 3.0;
                float blockDistortY = step(0.0, 0.3 - blockNoise) * (0.3 - blockNoise) * glitchIntensity * 2.0;
                glitchUV.x += blockDistortX * step(0.3, _GlitchLevel);
                glitchUV.y += blockDistortY * step(0.3, _GlitchLevel);
                
                // Ruido estático
                glitchUV.xy += (staticNoise - 0.5) * noiseIntensity;
                
                // SOLO UN MUESTREO DE TEXTURA
                half4 finalColor = tex2D(_MainTex, glitchUV);
                finalColor = (finalColor + _TextureSampleAdd) * IN.color;
                
                // Líneas de escaneo optimizadas
                float2 screenUV = IN.vertex.xy / _ScreenParams.xy;
                float scan = frac(screenUV.y * _ScanDensity + _Time.y * _ScanSpeed);
                scan = 1.0 - abs(scan * 2.0 - 1.0);
                scan = scan * scan; // Más rápido que pow(scan, 2.0)
                
                // Aplicar efectos de brillo
                float scanMultiplier = 0.7 + scan * 0.3 + _GlitchLevel * 0.2;
                finalColor.rgb *= scanMultiplier * _GlowIntensity;
                
                // Ruido estático optimizado
                float noiseEffect = step(0.8, staticNoise) * step(0.2, _GlitchLevel);
                finalColor.rgb += staticNoise * 0.1 * _GlitchLevel * noiseEffect;
                
                // Líneas de glitch optimizadas
                float glitchLine = frac(t * 2.0);
                float lineIntensity = 1.0 - smoothstep(0.0, 0.02, abs(uv.y - glitchLine));
                float lineEffect = step(0.1, lineIntensity) * step(0.5, _GlitchLevel);
                float lineNoise = fast_rand(float2(uv.x * 10.0, t));
                finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * (0.5 + lineNoise * 0.5), 0.3 * _GlitchLevel * lineEffect);

                #ifdef UNITY_UI_CLIP_RECT
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(finalColor.a - 0.001);
                #endif

                return finalColor;
            }
            ENDCG
        }
    }
}