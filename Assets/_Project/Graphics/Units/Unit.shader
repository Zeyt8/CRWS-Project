Shader "Uint"
{
    Properties
    {
        [NoScaleOffset]_BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Smoothness ("Smoothness", Float) = 0.7
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }

        HLSLINCLUDE
        #pragma target 4.5
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BaseColor;
            float _Smoothness;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.worldPos = TransformObjectToWorld(v.vertex);
                o.normalWS = TransformObjectToWorldNormal(v.normal);
                o.viewDirWS = normalize(_WorldSpaceCameraPos - o.worldPos);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 baseTexture = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                float3 baseColor = baseTexture.rgb * _BaseColor.rgb;
                
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 lightColor = mainLight.color;
                
                float3 normal = normalize(i.normalWS);
                float NdotL = max(dot(normal, lightDir), 0.0);
                float3 diffuse = baseColor * lightColor * NdotL;
                
                float3 viewDir = normalize(i.viewDirWS);
                float3 floatDir = normalize(lightDir + viewDir);
                float specular = 1.5 * pow(max(dot(normal, floatDir), 0.0), 300);
                float3 specularColor = specular * lightColor * _Smoothness;
                
                float fresnel = 0.3 * pow(1.0 - saturate(dot(normal, viewDir)), 5);
                float3 fresnelColor = fresnel * float3(1,1,1);
                
                float3 finalColor = diffuse + specularColor + fresnelColor;
                return float4(finalColor, baseTexture.a);
            }
            ENDHLSL
        }
    }
}