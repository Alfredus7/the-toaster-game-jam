Shader "UI/Transition"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Progress ("Progress", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "CRTTransition"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ClipRect;
            float _Progress;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;
                o.worldPosition = v.vertex;
                return o;
            }

            // Curvatura CRT
            float2 crtWarp(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                uv *= float2(1.0 + uv.y * uv.y * 0.15, 1.0 + uv.x * uv.x * 0.15);
                return uv * 0.5 + 0.5;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                float2 uv = crtWarp(IN.texcoord);

                // Progreso invertido (0 = encendido, 1 = apagado)
                float collapse = _Progress;

                // Colapso vertical hacia el centro
                float yCenter = 0.5;
                float yDist = abs(uv.y - yCenter);

                // Máscara de colapso (0 en el centro, 1 en los bordes)
                float closeMask = smoothstep(0.0, collapse * 0.4 + 0.01, yDist);

                fixed4 col = tex2D(_MainTex, uv) * IN.color;

                // Aplicar máscara de colapso
                col.rgb *= closeMask;

                // Alpha para permitir transición con transparencia
                col.a *= 1.0 - collapse;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                return col;
            }
            ENDCG
        }
    }
}