Shader "Universal Render Pipeline/2D/Seaweed_Lit_V8_Final"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _Speed ("Sway Speed", Range(0, 10)) = 2
        _Amount ("Sway Amount", Range(0, 0.5)) = 0.1
        _Frequency ("Wavy Frequency", Range(0, 10)) = 5
        
        // Các biến ẩn bắt buộc để Sprite Renderer hoạt động đúng
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaScan("AlphaScan", Float) = 0.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha // Trở về Straight Alpha mượt mà

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
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
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
                
                // Uốn lượn: gốc (y thấp) đứng yên, ngọn (y cao) uốn mạnh
                float sway = sin(_Time.y * _Speed + input.positionOS.y * _Frequency) * _Amount * input.uv.y;
                input.positionOS.x += sway;

                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                // Kết hợp màu từ Sprite Renderer và màu Tint
                output.color = input.color * _Color;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                
                // Trong URP 2D Lit, chúng ta chỉ cần trả ra màu gốc tại pass Universal2D.
                // Hệ thống Renderer sẽ tự động nhân với Light Texture ở bước sau.
                return color;
            }
            ENDHLSL
        }
    }
}
