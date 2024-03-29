// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Effects/Mobile/SlimeCutOut" {
Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
        _MainTex ("Base (RGB) Emission Tex (A)", 2D) = "white" {}
		_CutOut ("CutOut (A)", 2D) = "white" {}
        _Cube ("Reflection Cubemap", Cube) = "" { TexGen CubeReflect }
        _BumpMap ("Normalmap", 2D) = "bump" {}
		_BumpAmt ("Distortion", Float) = 10
}
Category {
	
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
				Blend SrcAlpha OneMinusSrcAlpha
				ZWrite Off
				Offset -1,-1
				Cull Off
				Fog { Mode Off}

	SubShader {
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _CutOut;
			samplerCUBE _Cube;

			float _BumpAmt;
			sampler2D _GrabTextureMobile;
			float4 _GrabTextureMobile_TexelSize;

			float4 _Color;
			float4 _ReflectColor;
			float _FPOW;
			float _R0;
			
			struct appdata_t {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				half4 vertex : POSITION;
				half2 uv_MainTex: TEXCOORD0;
				half2 uv_BumpMap : TEXCOORD1;
				half2 uv_CutOut : TEXCOORD2;
				half4 proj : TEXCOORD3;
				half3 normalDir : TEXCOORD4;
			    half3 tangentSpaceLightDir : TEXCOORD5;
				fixed4 color : COLOR;
			};
			
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			float4 _CutOut_ST;

			v2f vert (appdata_full v)
			{
				v2f o;
				
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv_BumpMap = TRANSFORM_TEX(v.texcoord, _BumpMap);
				o.uv_CutOut = TRANSFORM_TEX(v.texcoord, _CutOut);

				o.vertex = UnityObjectToClipPos(v.vertex);
				#if UNITY_UV_STARTS_AT_TOP
				half scale = -1.0;
				#else
				half scale = 1.0;
				#endif
				o.proj.xy = (half2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
				o.proj.zw = o.vertex.w;
#if UNITY_SINGLE_PASS_STEREO
				o.proj.xy = TransformStereoScreenSpaceTex(o.proj.xy, o.proj.w);
#endif
				o.color = v.color;

				float3 binormal = cross( v.normal, v.tangent.xyz ) * v.tangent.w;
				float3x3 rotation = float3x3( v.tangent.xyz, binormal, v.normal );
				o.normalDir = normalize(mul(half4(v.normal, 0.0), unity_WorldToObject).xyz);
                o.tangentSpaceLightDir = mul(rotation, normalize(ObjSpaceViewDir(v.vertex)));

				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				half4 tex = tex2D(_MainTex, i.uv_MainTex);
				half4 c = tex * _Color;
				half4 cut = tex2D(_CutOut, i.uv_CutOut);
		
				half3 normal = UnpackNormal(tex2D(_BumpMap, i.uv_BumpMap));
				
				float3 reflectedDir = reflect(i.tangentSpaceLightDir, i.normalDir)*normal;
				half4 reflcol = texCUBE (_Cube, reflectedDir);
				reflcol *= tex.a;     

				half2 offset = normal.rg * _BumpAmt * _GrabTextureMobile_TexelSize.xy * i.color.a;
				i.proj.xy = offset * i.proj.z + i.proj.xy;
				half4 col = tex2Dproj(_GrabTextureMobile, UNITY_PROJ_COORD(i.proj));

				fixed gray = col.r * 0.3 + col.g * 0.59 + col.b * 0.11;
				half3 emission = col.rgb*_Color.rgb + reflcol.rgb * _ReflectColor.rgb * _ReflectColor.a * gray * i.color.a;
				
				return fixed4(emission, cut.a * _Color.a * i.color.r);
			}
			ENDCG 
		}
	}	
}
}