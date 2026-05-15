Shader "Custom/MirrorWorldShader"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _Center ("Center (Screen Space)", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Float) = 0.0
        _WaveWidth ("Wave Width", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            float4 _Center;
            float _Radius;
            float _WaveWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 修正屏幕长宽比，让波纹是个正圆而不是椭圆
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 distVec = i.uv - _Center.xy;
                distVec.x *= aspect;
                float dist = length(distVec);

                // 计算圆环波纹
                float wave = smoothstep(_Radius - _WaveWidth, _Radius, dist) 
                           - smoothstep(_Radius, _Radius + _WaveWidth, dist);

                // 只返回发光的浅蓝色水圈，其余所有地方完全透明 (Alpha = 0)，露出底层游戏画面！
                return fixed4(0.5, 0.8, 1.0, wave * 0.8);
            }
            ENDCG
        }
    }
}