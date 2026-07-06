Shader "Unlit/Recover"
{
    Properties
    {
        _MainTex ("MaskRT", 2D) = "black" {}
        _Strength ("Strength", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Strength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed r = tex2D(_MainTex, i.uv).r;

                // ”’(1) -> •(0) ‚Öˆê’è—Ê‚¸‚Â–ß‚·
                r -= _Strength;
                r = max(r, 0);

                return fixed4(r, r, r, 1);
            }
            ENDHLSL
        }
    }
}