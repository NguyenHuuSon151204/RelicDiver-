Shader "Universal Render Pipeline/2D/SeaweedWavyLit_v3"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _Speed ("Sway Speed", Range(0, 10)) = 2
        _Amount ("Sway Amount", Range(0, 0.5)) = 0.1
        _Frequency ("Wavy Frequency", Range(0, 10)) = 5
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaScan("AlphaScan", Float) = 0.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "SpriteLit"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _Speed;
                float _Amount;
                float _Frequency;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Uốn lượn mượt mà
                float wavy = sin(_Time.y * _Speed + input.positionOS.y * _Frequency) * _Amount * input.uv.y;
                input.positionOS.x += wavy;

                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                // Với Universal2D, Fragment shader trả về màu gốc, 
                // hệ thống Light 2D của URP sẽ tự động nhân với màu ánh sáng tại vị trí đó.
                return tex * input.color;
            }
            ENDHLSL
        }
    }
}
