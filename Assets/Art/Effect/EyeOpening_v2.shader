Shader "Hidden/EyeOpening_v2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OpenAmount ("Open Amount", float) = 0.001
        _Expand ("Expand", float) = 0.0
        _Smoothness ("Smoothness", float) = 0.1
    }
    SubShader
    {
        // 포스트 프로세싱용 설정
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _OpenAmount;
            float _Expand;
            float _Smoothness;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 centeredUV = i.uv - 0.5;
                
                // 0으로 나누기 방지 (매우 중요)
                float safeOpen = max(_OpenAmount, 0.001);
                
                // 타원 거리 계산
                float d = sqrt(centeredUV.x * centeredUV.x + 
                          (centeredUV.y / safeOpen) * (centeredUV.y / safeOpen));

                float radius = 0.5 + _Expand;
                float mask = smoothstep(radius, radius - _Smoothness, d);

                fixed4 col = tex2D(_MainTex, i.uv);
                return col * mask;
            }
            ENDCG
        }
    }
}