// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

    Shader "Glass_ReflectiveRefractive" {
        Properties {
            _Color ("Main Color", Color) = (1,1,1,1)
            _Cube ("Reflection Cubemap", Cube) = "" { TexGen CubeReflect }
            _ReflToRefr ("ReflToRefr", Range (0.0, 1.0)) = 0.5
        }
        SubShader {
            Pass {
                Name "BASE"
                Tags { "RenderType"="Opaque"}
                LOD 200
               
                CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma fragmentoption ARB_precision_hint_fastest
                    #include "UnityCG.cginc"
     
                    fixed4 _Color;
                    samplerCUBE _Cube;
                    float _ReflToRefr;
                   
                    struct appdata {
                        float4 vertex : POSITION;
                        float3 normal : NORMAL;
                    };
     
                    struct v2f {
                        float4 Position : POSITION;
                        float3 Reflect: TEXCOORD0;    
                        float3 Refract: TEXCOORD1;
                    };
                   
                    v2f vert(appdata v) {
                        v2f o;
                        o.Position = UnityObjectToClipPos(v.vertex);
                        float3 ViewDirection = -WorldSpaceViewDir(v.vertex);
                        o.Reflect = reflect(ViewDirection, v.normal);   
                        o.Refract = refract(ViewDirection, v.normal, 0.99);
                        return o;
                    }
                   
                    fixed4 frag (v2f i) : COLOR {
                        fixed4 reflColor = texCUBE(_Cube, i.Reflect);
                        fixed4 refrColor = texCUBE(_Cube, i.Refract);
                        half4 c = lerp(refrColor, reflColor, _ReflToRefr) * _Color;
                        return c;
                    }
                ENDCG
            }
        }
        FallBack "Diffuse"
    }