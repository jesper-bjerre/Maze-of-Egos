Shader "MazeOfEgos/Universal Render Pipeline/CharacterReference"
{
    Properties
    {
        [MainTexture] _BaseMap ("Original reference sheet", 2D) = "white" {}
        [MainColor] _BaseColor ("Tint", Color) = (1,1,1,1)
        _SkinColor ("Temple and neck skin", Color) = (.78,.55,.40,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _SkinColor;
        CBUFFER_END
        ENDHLSL
        Pass
        {
            Name "CharacterForward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fog
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half mask : TEXCOORD3;
                half fog : TEXCOORD4;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.mask = input.color.r;
                output.fog = ComputeFogFactor(output.positionCS.z);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half3 normal = normalize(input.normalWS);
                half3 albedo = lerp(_SkinColor.rgb, SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb, saturate(input.mask)) * _BaseColor.rgb;
                Light sun = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half3 illumination = SampleSH(normal) + sun.color * saturate(dot(normal, sun.direction)) * sun.shadowAttenuation * sun.distanceAttenuation;
                half3 result = albedo * illumination;
                return half4(MixFog(result, input.fog), 1);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ColorMask 0
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            float3 _LightDirection;
            struct ShadowAttributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            float4 ShadowVert(ShadowAttributes input) : SV_POSITION
            {
                float3 world = TransformObjectToWorld(input.positionOS.xyz);
                float3 normal = TransformObjectToWorldNormal(input.normalOS);
                float4 clip = TransformWorldToHClip(ApplyShadowBias(world, normal, _LightDirection));
                return ApplyShadowClamping(clip);
            }
            half4 ShadowFrag() : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
