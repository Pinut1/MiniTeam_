Shader "UI/EyeOpening_URP"
{
    Properties
    {
        _OpenAmount ("Open Amount", float) = 0.001
        _Expand ("Expand", float) = 0.0
        _Smoothness ("Smoothness", float) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
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
                float safeOpen = max(_OpenAmount, 0.001);
                
                float d = sqrt(centeredUV.x * centeredUV.x + 
                          (centeredUV.y / safeOpen) * (centeredUV.y / safeOpen));

                float radius = 0.5 + _Expand;
                float mask = smoothstep(radius, radius - _Smoothness, d);

                // 눈을 뜨는 효과: 
                // mask가 1인 곳(눈 안쪽)은 투명하게(알파 0)
                // mask가 0인 곳(눈 바깥쪽)은 까맣게(알파 1)
                return fixed4(0, 0, 0, 1.0 - mask);
            }
            ENDCG
        }
    }
}
