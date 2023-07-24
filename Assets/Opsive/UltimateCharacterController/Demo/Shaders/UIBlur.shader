// Adds a blur to the material.
Shader "Ultimate Character Controller/Demo/UIBlur"
{
    Properties
    {
        _Radius("Radius", Range(10, 255)) = 1
        _Step("Step", Range(0.1, 1)) = 0.1
    }

    Category
    {
        Tags{ "Queue" = "Transparent"}

        SubShader
        {
            GrabPass { }

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 texcoord: TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : POSITION;
                    float4 uv : TEXCOORD0;
                };

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = ComputeGrabScreenPos(o.vertex);
                    return o;
                }

                sampler2D _GrabTexture;
                float4 _GrabTexture_TexelSize;
                float _Radius;
                float _Step;

                half4 frag(v2f i) : COLOR
                {
                    #define GRABPIXEL(samplex, sampley) tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uv.x + _GrabTexture_TexelSize.x * samplex, i.uv.y + _GrabTexture_TexelSize.y * sampley, i.uv.z, i.uv.w)))

                    half4 sum = half4(0,0,0,0);
                    int count = 0;

                    for (float range = 0; range < _Radius; range += _Step)
                    {
                        sum += GRABPIXEL(range, range);
                        sum += GRABPIXEL(range, -range);
                        sum += GRABPIXEL(-range, range);
                        sum += GRABPIXEL(-range, -range);
                        count += 4;
                    }

                    return sum / count;
                }
                ENDCG
            }

            GrabPass { }

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 texcoord: TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : POSITION;
                    float4 uv : TEXCOORD0;
                };

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = ComputeGrabScreenPos(o.vertex);
                    return o;
                }

                sampler2D _GrabTexture;
                float4 _GrabTexture_TexelSize;
                float _Radius;
                float _Step;

                half4 frag(v2f i) : COLOR
                {
                    #define GRABPIXEL(samplex, sampley) tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uv.x + _GrabTexture_TexelSize.x * samplex, i.uv.y + _GrabTexture_TexelSize.y * sampley, i.uv.z, i.uv.w)))

                    half4 sum = half4(0,0,0,0);
                    int count = 0;

                    for (float range = 0; range < _Radius; range += _Step)
                    {
                        sum += GRABPIXEL(range, 0);
                        sum += GRABPIXEL(-range, 0);
                        sum += GRABPIXEL(0, range);
                        sum += GRABPIXEL(0, -range);
                        count += 4;
                    }

                    return sum / count;
                }
                ENDCG
            }
        }
    }
}