Shader "Unlit/CurvedUI"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Curve ("Curve Amount", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Curve;

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

                // 기본 정점 처리
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // 여기서 UV 왜곡을 줘서 휘어 보이게 함
                float2 centeredUV = (v.uv - 0.5) * 2; // -1~1 범위
                float curve = _Curve * 0.5;

                // X 방향 곡률 (원통처럼 휨)
                centeredUV.x += (centeredUV.y * centeredUV.y) * curve;

                o.uv = centeredUV * 0.5 + 0.5;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
