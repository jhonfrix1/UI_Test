Shader "UI/IconWithOutline_Final"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Icon Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Thickness (UV)", Range(0, 0.05)) = 0.01
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.05
    }

    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane" 
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "UIOutlinePass"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineSize;
            float _AlphaThreshold;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float baseAlpha = tex2D(_MainTex, i.uv).a;
                float outlineAlpha = 0;

                for (int x = -1; x <= 1; x++) {
                    for (int y = -1; y <= 1; y++) {
                        float2 offset = float2(x, y) * _OutlineSize;
                        float a = tex2D(_MainTex, i.uv + offset).a;
                        outlineAlpha = max(outlineAlpha, a);
                    }
                }

                // If we're in the outline region only
                if (baseAlpha < _AlphaThreshold && outlineAlpha > _AlphaThreshold) {
                    return fixed4(_OutlineColor.rgb, _OutlineColor.a * outlineAlpha);
                }

                // If we're inside the icon
                fixed4 texCol = tex2D(_MainTex, i.uv);
                return texCol * _Color;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}
