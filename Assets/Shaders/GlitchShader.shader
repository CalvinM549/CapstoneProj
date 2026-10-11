Shader "Custom/GlitchSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0,1)) = 0
        _BandHeightPx ("Band Height (px)", Float) = 4
        _PPU ("Pixels Per Unit", Float) = 16
        _MaxOffsetPx ("Max Offset (px)", Float) = 6
        _SplitPx ("Channel Split (px)", Float) = 1
        _GlitchRate ("Glitch Rate (Hz)", Float) = 12
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Intensity, _BandHeightPx, _PPU, _MaxOffsetPx, _SplitPx, _GlitchRate;
            CBUFFER_END

            struct Attributes { float4 pos : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Varyings   { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; float3 wpos : TEXCOORD1; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.wpos = TransformObjectToWorld(v.pos.xyz);
                o.pos = TransformWorldToHClip(o.wpos);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Stepped time so the glitch "ticks" instead of flowing
                float tick = floor(_Time.y * _GlitchRate);

                // Band index in world space, snapped to pixel rows
                float band = floor(i.wpos.y * _PPU / _BandHeightPx);

                // Each band has a chance (= Intensity) of glitching this tick
                float active = step(hash(float2(band, tick + 7.0)), _Intensity);
                float dir = hash(float2(band, tick)) * 2.0 - 1.0;

                // Offset in whole texture pixels so it stays crisp
                float offsetPx = round(dir * _MaxOffsetPx) * active;
                float2 px = _MainTex_TexelSize.xy;
                float2 uv = i.uv + float2(offsetPx * px.x, 0);

                float split = _SplitPx * px.x * active;
                half4 r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(split, 0));
                half4 g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                half4 b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - float2(split, 0));

                half4 col = half4(r.r, g.g, b.b, max(r.a, max(g.a, b.a)));
                return col * i.color;
            }
            ENDHLSL
        }
    }
}