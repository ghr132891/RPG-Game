Shader "Custom/TimeWorldNoiseShader"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _BaseNoiseIntensity ("Noise Amount", Range(0, 1)) = 0.2
        _ScanlineScale ("Scanline Density", Float) = 600
        
        [Header(Z_Distortion_Settings)]
        _TearFreq ("Z-Shape Freq", Float) = 3.5      // Z 字形的密集程度
        _TearSpeed ("Z-Shape Speed", Float) = 5.0     // 扭曲滚动的速度
        _TearBias ("Distortion Strength", Range(0, 0.1)) = 0.02 // 画面扭曲的幅度
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

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _BaseNoiseIntensity;
            float _ScanlineScale;
            float _TearFreq;
            float _TearSpeed;
            float _TearBias;

            float rand(float2 co) {
                return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
            }

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float time = _Time.y;
                float2 uv = i.uv;

                // 1. 【核心：全屏几何扭曲】
                // 不再使用 isTear 遮罩，而是计算一个全屏通用的 Z 字形偏移
                // 使用三角波（abs/frac）生成锐利的折线轮廓
                float triangleWave = abs(frac(uv.y * _TearFreq + time * _TearSpeed) * 2.0 - 1.0);
                float zSkew = (triangleWave * 2.0 - 1.0) * _TearBias;

                // 2. 应用扭曲到采样坐标
                // 让噪点和扫描线的坐标都跟着这个 Z 字形偏移走
                float2 distortedUV = uv;
                distortedUV.x += zSkew;

                // 3. 生成基于扭曲坐标的噪点
                // 这样噪点本身就会呈现出一种“被折线拉扯过”的质感
                float noise = rand(distortedUV + time) * _BaseNoiseIntensity;
                
                // 4. 生成基于扭曲坐标的扫描线
                // 扫描线现在也会变成折线（Z字形）
                float scanline = sin(distortedUV.y * _ScanlineScale + time * 10.0) * 0.03;

                // 5. 混合并输出
                float finalAlpha = saturate((noise + scanline) * 1.5);
                
                // 稍微深一点的底色，增加压抑感
                return fixed4(0.05, 0.05, 0.05, finalAlpha);
            }
            ENDCG
        }
    }
}