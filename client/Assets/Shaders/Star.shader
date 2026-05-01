Shader "Starry/Star"
{
    Properties
    {
        _MainTex        ("Star Texture",      2D)           = "white" {}
        _BaseColor      ("Base Color",        Color)        = (0.7, 0.7, 1.0, 1.0)
        _HighlightColor ("Highlight Color",   Color)        = (1.0, 0.9, 0.5, 1.0)
        _TwinkleSpeed   ("Twinkle Speed",     Float)        = 2.0
        _TwinklePhase   ("Twinkle Phase",     Float)        = 0.0
        _TwinkleAmount  ("Twinkle Amount",    Range(0,1))   = 0.5
        _Highlight      ("Highlight Strength",Range(0,1))   = 0.0
    }
    SubShader
    {
        Tags
        {
            "Queue"          = "Transparent"
            "RenderType"     = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        Blend  SrcAlpha One   // Additive — 별빛 느낌
        ZWrite Off
        Cull   Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
                UNITY_DEFINE_INSTANCED_PROP(float4, _HighlightColor)
                UNITY_DEFINE_INSTANCED_PROP(float,  _TwinkleSpeed)
                UNITY_DEFINE_INSTANCED_PROP(float,  _TwinklePhase)
                UNITY_DEFINE_INSTANCED_PROP(float,  _TwinkleAmount)
                UNITY_DEFINE_INSTANCED_PROP(float,  _Highlight)
            UNITY_INSTANCING_BUFFER_END(Props)

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                float highlight = UNITY_ACCESS_INSTANCED_PROP(Props, _Highlight);
                float scale     = lerp(1.0, 1.5, highlight); // 강조 시 1.5배 크기

                // 빌보드: 카메라를 항상 향하도록 월드 공간에서 확장
                float3 worldCenter = TransformObjectToWorld(float3(0, 0, 0));
                float3 camRight    = unity_CameraToWorld._m00_m10_m20;
                float3 camUp       = unity_CameraToWorld._m01_m11_m21;
                float3 worldPos    = worldCenter
                                   + camRight * IN.positionOS.x * scale
                                   + camUp    * IN.positionOS.y * scale;

                OUT.positionCS = TransformWorldToHClip(worldPos);
                OUT.uv         = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                float  speed     = UNITY_ACCESS_INSTANCED_PROP(Props, _TwinkleSpeed);
                float  phase     = UNITY_ACCESS_INSTANCED_PROP(Props, _TwinklePhase);
                float  amount    = UNITY_ACCESS_INSTANCED_PROP(Props, _TwinkleAmount);
                float  highlight = UNITY_ACCESS_INSTANCED_PROP(Props, _Highlight);
                float4 baseCol   = UNITY_ACCESS_INSTANCED_PROP(Props, _BaseColor);
                float4 hlCol     = UNITY_ACCESS_INSTANCED_PROP(Props, _HighlightColor);

                // 별마다 다른 위상으로 각자 따로 반짝임
                float twinkle    = (sin(_Time.y * speed + phase) + 1.0) * 0.5;
                float brightness = lerp(1.0 - amount, 1.0, twinkle);

                // 비강조 별은 30%로 어둡게, 강조 별은 100%
                float dimFactor = lerp(0.3, 1.0, highlight);

                half4 tex    = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float4 col   = lerp(baseCol, hlCol, highlight);

                half4 result = tex * col * brightness * dimFactor;
                result.a     = tex.a * brightness * dimFactor;
                return result;
            }
            ENDHLSL
        }
    }
}
