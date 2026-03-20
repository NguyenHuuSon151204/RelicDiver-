Shader "Custom/SpriteOutline"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Outline Color", Color) = (1,1,1,1)
        _Thickness ("Outline Thickness", Range(0, 10)) = 1
        [Toggle] _OnlyOutline ("Only Outline", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
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
            float4 _MainTex_TexelSize;
            float4 _Color;
            float _Thickness;
            float _OnlyOutline;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float alpha = mainColor.a;

                float outlineAlpha = 0;
                float2 texelSize = _MainTex_TexelSize.xy * _Thickness;

                // Simple 4-neighborhood check for alpha transition
                outlineAlpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(texelSize.x, 0)).a;
                outlineAlpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-texelSize.x, 0)).a;
                outlineAlpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(0, texelSize.y)).a;
                outlineAlpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(0, -texelSize.y)).a;
                
                // If current pixel's alpha is low and neighbors are high -> outline
                float isOutline = saturate(outlineAlpha) - alpha;
                
                float4 finalColor;
                if (_OnlyOutline > 0.5) {
                    finalColor = _Color;
                    finalColor.a *= saturate(isOutline);
                } else {
                    finalColor = lerp(mainColor, _Color, saturate(isOutline));
                }

                return finalColor * input.color;
            }
            ENDHLSL
        }
    }
}
