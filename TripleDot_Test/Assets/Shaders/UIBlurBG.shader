Shader "UI/BlurSimple"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 10)) = 1
        _Darkness ("Darkness", Range(0, 1)) = 0.6
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurSize;
            float _Darkness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                fixed4 col = tex2D(_MainTex, uv) * 0.36;
                col += tex2D(_MainTex, uv + float2(_MainTex_TexelSize.x * _BlurSize, 0)) * 0.16;
                col += tex2D(_MainTex, uv - float2(_MainTex_TexelSize.x * _BlurSize, 0)) * 0.16;
                col += tex2D(_MainTex, uv + float2(0, _MainTex_TexelSize.y * _BlurSize)) * 0.16;
                col += tex2D(_MainTex, uv - float2(0, _MainTex_TexelSize.y * _BlurSize)) * 0.16;

                col.rgb *= _Darkness;

                return col;
            }
            ENDCG
        }
    }
}
