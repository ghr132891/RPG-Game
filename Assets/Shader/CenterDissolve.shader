Shader "UI/CenterDissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1.2)) = 0
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.3
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off Lighting Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float _DissolveAmount;
            float _NoiseStrength;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 获取原图颜色
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                // 1. 计算当前像素到中心点 (0.5, 0.5) 的距离
                float dist = distance(i.uv, float2(0.5, 0.5));

                // 2. 采样噪点图，制造破碎感边缘
                float noise = tex2D(_NoiseTex, i.uv).r;

                // 3. 混合距离和噪点
                float dissolveValue = dist + (noise * _NoiseStrength);

                // 4. 核心逻辑：如果混合值小于消散进度，就把透明度设为 0（变透明）
                if (dissolveValue < _DissolveAmount)
                {
                    col.a = 0;
                }

                return col;
            }
            ENDCG
        }
    }
}