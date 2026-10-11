Shader "Custom/GlitchFullScreen"
{
    Properties
    {
        _VirtualRes ("Virtual Resolution", Vector) = (320, 180, 0, 0)
        _BandHeightPx ("Band Height (virtual px)", Float) = 6
        _MaxOffsetPx ("Max Offset (virtual px)", Float) = 14
        _SplitPx ("Channel Split (virtual px)", Float) = 2
        _GlitchRate ("Glitch Rate (Hz)", Float) = 12
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off Blend Off

        Pass
        {
            Name "GlitchFullScreen"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _VirtualRes;
                float _BandHeightPx, _MaxOffsetPx, _SplitPx, _GlitchRate;
            CBUFFER_END

            // Driven globally from script (no material editing at runtime)
            float _GlitchIntensity;
            float _GlitchTime;

            float hash21(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;

                // Cheap early-out when nothing is happening
                if (_GlitchIntensity <= 0.001)
                    return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv);

                float2 res = _VirtualRes.xy;
                float2 pixel = floor(uv * res);          // virtual pixel coordinate
                float tick = floor(_GlitchTime * _GlitchRate);

                // --- Horizontal slice displacement ---
                float band = floor(pixel.y / _BandHeightPx);
                float bandActive = step(hash21(float2(band, tick + 7.0)), _GlitchIntensity * 0.6);
                float dir = hash21(float2(band, tick)) * 2.0 - 1.0;
                float offsetPx = round(dir * _MaxOffsetPx * _GlitchIntensity) * bandActive;

                // --- Occasional blocky displacement (both axes) ---
                float2 blockId = floor(pixel / float2(28.0, 14.0));
                float blockActive = step(hash21(blockId + tick * 3.1), _GlitchIntensity * 0.08);
                float2 blockShift = (float2(hash21(blockId + tick), hash21(blockId - tick)) * 2.0 - 1.0)
                                    * 10.0 * blockActive;

                float2 shiftPx = float2(offsetPx, 0) + round(blockShift);
                float active = max(bandActive, blockActive);

                float2 uvShifted = frac(uv + shiftPx / res);   // whole virtual pixels, wraps at edges

                // --- RGB split ---
                float2 split = float2(_SplitPx * active * _GlitchIntensity / res.x, 0);
                half r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uvShifted + split).r;
                half4 g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uvShifted);
                half b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uvShifted - split).b;
                half3 col = half3(r, g.g, b);

                // // --- Static noise on glitched areas + faint scanlines ---
                // col += (hash21(pixel + tick) - 0.5) * 0.25 * active;
                // col *= 1.0 - 0.12 * _GlitchIntensity * step(0.5, frac(pixel.y * 0.5));

                return half4(col, g.a);
            }
            ENDHLSL
        }
    }
}