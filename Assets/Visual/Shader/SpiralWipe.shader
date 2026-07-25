Shader "UI/SpiralWipe"
{
    Properties
    {
        _Progress ("Progress (0-1)", Range(0,1)) = 0
        _ThicknessX ("Thickness X (normalized)", Float) = 0.08
        _ThicknessY ("Thickness Y (normalized)", Float) = 0.08
        _MaxSteps ("Max Steps (set da script)", Float) = 6
        _EdgeSoftness ("Edge Softness", Range(0.0001, 0.05)) = 0.01
        _Color ("Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Progress;
            float _ThicknessX;
            float _ThicknessY;
            float _MaxSteps;
            float _EdgeSoftness;
            fixed4 _Color;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float u = i.uv.x;
                float v = i.uv.y;

                float tx = max(_ThicknessX, 0.0001);
                float ty = max(_ThicknessY, 0.0001);

                // Determina in quale "anello" (k) si trova il pixel
                float layer = min(min(u / tx, (1.0 - u) / tx), min(v / ty, (1.0 - v) / ty));
                float k = floor(layer);

                // Rettangolo di questo specifico anello
                float left   = k * tx;
                float right  = 1.0 - k * tx;
                float top    = 1.0 - k * ty;
                float bottom = k * ty;

                // Confini interni delle 4 bande (assiali, NIENTE diagonali)
                float leftInner   = left + tx;
                float rightInner  = right - tx;
                float bottomInner = bottom + ty;
                float topInner    = top - ty;

                float legIndex;
                float progress;

                if (u < leftInner)
                {
                    // Banda sinistra: percorre dall'alto verso il basso, altezza intera dell'anello
                    legIndex = 0.0;
                    float h = max(top - bottom, 0.0001);
                    progress = (top - v) / h;
                }
                else if (v < bottomInner)
                {
                    // Banda inferiore: da sinistra (dopo la banda sx) verso destra
                    legIndex = 1.0;
                    float w = max(right - leftInner, 0.0001);
                    progress = (u - leftInner) / w;
                }
                else if (u >= rightInner)
                {
                    // Banda destra: dal basso (dopo la banda inferiore) verso l'alto
                    legIndex = 2.0;
                    float h = max(top - bottomInner, 0.0001);
                    progress = (v - bottomInner) / h;
                }
                else
                {
                    // Banda superiore: da destra (dopo la banda destra) verso sinistra
                    legIndex = 3.0;
                    float w = max(rightInner - leftInner, 0.0001);
                    progress = (rightInner - u) / w;
                }

                float localFraction = (legIndex + saturate(progress)) / 4.0;
                float spiralValue = k + localFraction;

                float goal = _Progress * _MaxSteps;

                float covered = smoothstep(goal - _EdgeSoftness, goal + _EdgeSoftness, spiralValue);
                float alpha = 1.0 - covered;

                return fixed4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
}