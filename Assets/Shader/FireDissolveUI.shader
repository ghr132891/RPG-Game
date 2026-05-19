Shader "Custom/UI/FireDissolveFromCenter"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Dissolve Settings)]
        _Progress ("Transition Progress", Range(0, 1)) = 0.0
        _NoiseTex ("Noise Texture (Perlin/Organic)", 2D) = "white" {}
        _NoiseStrength ("Noise Distortion", Range(0, 2)) = 0.8 // 调高可让火焰形状更撕裂
        
        [Header(Fire Edge)]
        [HDR] _EdgeColor1 ("Hot Core Color (HDR)", Color) = (4, 3, 1, 1)    // 核心高温区：亮黄色/白黄色
        [HDR] _EdgeColor2 ("Outer Flame Color (HDR)", Color) = (3, 0.5, 0, 1) // 外围低温区：深橙色/暗红色
        _EdgeWidth ("Flame Edge Width", Range(0, 0.5)) = 0.2 // 火焰渐变带的宽度
    }

    SubShader
    {
        Tags
        {
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            sampler2D _NoiseTex;
            
            float _Progress;
            float _NoiseStrength;
            
            float4 _EdgeColor1;
            float4 _EdgeColor2;
            float _EdgeWidth;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 1. 采样原图
                half4 col = tex2D(_MainTex, i.texcoord) * i.color;

                // 2. 计算距中心点的距离（中心是0，四个角落最远约是 1.414）
                float dist = distance(i.texcoord, float2(0.5, 0.5)) * 2.0;

                // 3. 采样噪点（范围0~1，减去0.5让它在 -0.5~0.5 波动，不改变整体扩展趋势）
                float noise = tex2D(_NoiseTex, i.texcoord).r - 0.5;

                // 4. 溶解权重 = 距离 + 噪点干扰
                float dissolveValue = dist + noise * _NoiseStrength;

                // 5. 【核心修复】：动态计算真实的起始点和终点，绝不产生初见缺口！
                // 最小可能值是中心点(0)减去最大噪声影响
                float minVal = -0.5 * _NoiseStrength;
                // 最大可能值是角落(1.414)加上最大噪声影响
                float maxVal = 1.414 + 0.5 * _NoiseStrength; 
                
                // 根据进度，从 绝对安全值 平滑过渡到 绝对烧尽值
                float currentThreshold = lerp(minVal - _EdgeWidth - 0.05, maxVal + 0.05, _Progress);

                // 6. 剔除烧尽的像素
                if (dissolveValue < currentThreshold)
                {
                    discard;
                }

                // 7. 【核心修复】：计算平滑的高级火焰渐变
                // edgeDistance 越靠近 0，说明离被烧掉越近（火最旺）
                float edgeDistance = dissolveValue - currentThreshold;
                
                // 将距离转换为 0 到 1 的强度 (1 = 最靠近燃烧线，0 = 安全的原图)
                float fireIntensity = 1.0 - saturate(edgeDistance / _EdgeWidth);
                
                if (fireIntensity > 0)
                {
                    // 让核心高温区更加集中（使用指数曲线让亮黄色收束在最边缘）
                    float fireCore = pow(fireIntensity, 3.0);
                    
                    // 根据强度，在 深红(外焰) 和 亮黄(内焰) 之间平滑过渡
                    half4 fireColor = lerp(_EdgeColor2, _EdgeColor1, fireCore);
                    
                    // 将火焰颜色自然地覆盖/混合到原图上
                    col.rgb = lerp(col.rgb, fireColor.rgb, fireIntensity);
                }

                return col;
            }
            ENDCG
        }
    }
}