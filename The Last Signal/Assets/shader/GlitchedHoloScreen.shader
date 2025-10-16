Shader "UI/RetroGlitchedScreen"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _HoloColor ("Screen Tint", Color) = (0.2, 1, 0.3, 1)
        _ScanSpeed ("Scan Speed", Range(0, 10)) = 2
        _ScanDensity ("Scan Density", Range(10, 1000)) = 300
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2
        _GlitchSpeed ("Glitch Speed", Range(0, 10)) = 3
        _GlitchThickness ("Glitch Thickness", Range(0, 0.2)) = 0.02
        _GlitchIntensity ("Glitch Offset", Range(0, 0.1)) = 0.03

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
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _HoloColor;
            float _ScanSpeed;
            float _ScanDensity;
            float _GlowIntensity;
            float _GlitchSpeed;
            float _GlitchThickness;
            float _GlitchIntensity;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _HoloColor;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
{
    // --- Tiempo y glitch ---
    float t = frac(_Time.y * _GlitchSpeed);

    // --- Coordenadas de pantalla normalizadas ---
    float2 screenUV = IN.vertex.xy / _ScreenParams.xy;

    // --- Escaneo con tamaño constante ---
    float scanTime = _Time.y * _ScanSpeed;
    float scan = frac(screenUV.y * _ScanDensity + scanTime);
    scan = 1.0 - abs(scan * 2.0 - 1.0);   // Onda triangular
    scan = pow(scan, 2.0);                // Suavizado del brillo

    // --- Línea de glitch ---
    float linePos = t;
    float dist = abs(screenUV.y - linePos);
    float glitchLine = smoothstep(_GlitchThickness, 0.0, dist);

    // --- Desplazamiento UV para glitch ---
    float2 glitchUV = IN.texcoord;
    glitchUV.x += glitchLine * _GlitchIntensity * sin(_Time.y * 30.0);

    // --- Textura base y color holográfico ---
    half4 color = (tex2D(_MainTex, glitchUV) + _TextureSampleAdd) * IN.color;
    color.rgb *= (0.6 + scan * 0.4) * _GlowIntensity;

    #ifdef UNITY_UI_CLIP_RECT
    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
    #endif

    #ifdef UNITY_UI_ALPHACLIP
    clip(color.a - 0.001);
    #endif

    return color;
}

            ENDCG
        }
    }
}