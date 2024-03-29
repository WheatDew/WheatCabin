Shader "KriptoFX/RFX4/Decal/ParallaxDecal" {
    Properties
    { [HDR] _Emission("Emission Color",  Color) = (1, 1, 1, 1)
         _EmissionTex("_Emission Map", 2D) = "white" {}
        _NoiseTex("Noise", 2D) = "white" {}
        _NoiseSpeedScale("Noise Speed Scale", Vector) = (1, 1, 1, 1)
        _EmissionDepth("Emission Depth", Float) = 10
        _NoiseDepth("Noise Depth", Float) = 10

        _Height("Height", Range(0.001,1)) = 0.25
        _HeightTex("Height Map", 2D) = "white" {}

        _MainTex("Main Texture", 2D) = "white" {}
        _NormalTex("Normal Map", 2D) = "white" {}

        _Cutout("Cutout", Range(0,1)) = 1
        _Steps("Steps Linear", int) = 10
        _StepsBin("Steps Binary", int) = 10

         [Toggle(USE_LIGHT)] _UseLight("Use Light", Float) = 0
    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "DisableBatching" = "True"}
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                // make fog work
                #pragma multi_compile_fog
                #pragma shader_feature USE_LIGHT

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normal : NORMAL;
                    float4 tangent : TANGENT;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    UNITY_FOG_COORDS(2)
                    float4 vertex : SV_POSITION;
                    float3 viewDir : TEXCOORD3;
                    float3 worldPos : TEXCOORD6;
                    float3 worldNormal  : TEXCOORD7;
                    float3 worldTangent  : TEXCOORD8;
                    float3 worldBitangent : TEXCOORD9;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                float4 _Emission;
                sampler2D _NoiseTex;
                float4 _NoiseTex_ST;
                float4 _NoiseSpeedScale;
                float _EmissionDepth;
                float _NoiseDepth;

                sampler2D _MainTex;
                float4 _MainTex_ST;

                sampler2D _HeightTex;
                float4 _HeightTex_ST;
                float _Offset;
                float _Height;
                int _Steps;
                int _StepsBin;

                sampler2D _EmissionTex;
                float4 _EmissionTex_ST;
                float _Cutout;

                sampler2D _NormalTex;
                float4 RFX4_LightPositions[8];
                float4 RFX4_LightColors[8];
                int RFX4_LightCount;
                sampler2D RFX4_PointLightAttenuation;

                v2f vert(appdata v)
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v); //Insert
                    UNITY_INITIALIZE_OUTPUT(v2f, o); //Insert
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;

                    o.worldNormal = UnityObjectToWorldNormal(v.normal);
                    o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                    o.worldBitangent = cross(o.worldNormal, o.worldTangent) * v.tangent.w * unity_WorldTransformParams.w;

                    float3 binormal = cross(normalize(v.normal), normalize(v.tangent.xyz)) * v.tangent.w;
                    float3x3 rotation = float3x3(v.tangent.xyz, binormal, v.normal);
                    o.viewDir = mul(rotation, ObjSpaceViewDir(v.vertex));

                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    //o.worldPos = v.vertex;

                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }


                half4 frag(v2f i) : SV_Target
                {
                    //return float4(frac(i.worldPos.xyz*10), 1);

                    float3 p = float3(i.uv, 0);
                    float3 v = normalize(i.viewDir * -1);
                    v.z = abs(v.z);
                    float depthBias = 1.0 - v.z;
                    depthBias *= depthBias;
                    depthBias *= depthBias;
                    depthBias = 1.0 - depthBias * depthBias;

                    v.xy *= depthBias;
                    v.xy *= _Height;
                    const int linearSearchSteps = _Steps;
                    const int binarySearchSteps = _StepsBin;
                    v /= v.z * linearSearchSteps;

                    int idx;
                    for (idx = 0; idx < linearSearchSteps; idx++)
                    {
                        float tex = tex2D(_HeightTex, p.xy).r;
#if UNITY_COLORSPACE_GAMMA
                        tex *= tex;
#endif
                        if (p.z < tex)		p += v;
                    }

                    for (idx = 0; idx < binarySearchSteps; idx++)
                    {
                        v *= 0.5;
                        float tex = tex2D(_HeightTex, p.xy).r;
#if UNITY_COLORSPACE_GAMMA
                        tex *= tex;
#endif
                        if (p.z < tex)		p += v;	else	p -= v;
                    }


                    half3 normal = UnpackNormal(tex2D(_NormalTex,p.xy));
                    float3x3 local2WorldTranspose = float3x3(i.worldTangent, i.worldBitangent, i.worldNormal);
                    normal = -normalize(mul(normal, local2WorldTranspose));

                    float4 col;
                    col.a = 1;
                    col.rgb = tex2D(_MainTex, p.xy * _MainTex_ST.xy + _MainTex_ST.zw);
    #if USE_LIGHT

                    half3 light = 0;
                    float3 lightDir;
                    half attenuation = 1;
                    [loop]
                    for (idx = 0; idx < RFX4_LightCount; idx++)
                    {
                        if (RFX4_LightColors[idx].w > 0.5) {
                            lightDir = RFX4_LightPositions[idx].xyz - i.worldPos;
                            float lightDist = length(lightDir);
                            if (lightDist > RFX4_LightPositions[idx].w) continue;
                            half normalizedDist = lightDist / RFX4_LightPositions[idx].w;
                            attenuation = saturate(1.0 / (1.0 + 25.0 * normalizedDist * normalizedDist) * saturate((1 - normalizedDist) * 5.0));
                        }
                        else {
                            lightDir = RFX4_LightPositions[idx].xyz;
                        }
                        light += abs(dot(normal, normalize(lightDir))) * RFX4_LightColors[idx].rgb * attenuation;

                    }
                    col.rgb *= light;
    #endif
                    col.a = tex2D(_HeightTex, p.xy).a > 0.5 ? 1 : 0;
                    col.a *= saturate((tex2D(_HeightTex, i.uv).r - 1 + _Cutout * 2)* 100);

                    float2 noiseOffset = tex2D(_NoiseTex, p.xy * _NoiseTex_ST.xy + _NoiseTex_ST.zw + _Time.x * _NoiseSpeedScale.xy);
                    half3 emission = tex2D(_EmissionTex, p.xy * _EmissionTex_ST.xy + _EmissionTex_ST.zw + noiseOffset * pow(p.z, _EmissionDepth)).rgb;
#if UNITY_COLORSPACE_GAMMA
                    emission *= emission;
#endif
                    emission *= pow(p.z, _NoiseDepth) * _Emission;

                    col.rgb += emission;

                    return max(0.00001, col);
                }
                ENDCG
            }
        }
}
