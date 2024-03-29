// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Effects/Ice/IceVert" {

Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "black" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
		_HeightMap ("_HeightMap (r)", 2D) = "white" {}
		_Height ("_Height", Float) = 0.3
		_OffsetXHeightMap ("_OffsetXHeightMap", Range (0, 1)) = 0
		_OffsetYHeightMap ("_OffsetYHeightMap", Range (0, 1)) = 0
        _FPOW("FPOW Fresnel", Float) = 5.0
        _R0("R0 Fresnel", Float) = 0.05
		_Cutoff ("Emission strength", Range (0, 1)) = 0.5
		_MainTexAlpha ("_MainTexAlpha", range (0, 2)) = 1
		_BumpAmt ("Distortion", Float)= 10
		_RefractiveStrength ("Refractive Strength", Float) = 0
}

Category {
	
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
				Blend SrcAlpha OneMinusSrcAlpha
				ZWrite Off

	SubShader {
		GrabPass {							
			Name "BASE"
			Tags { "LightMode" = "Always" }
 		}
		Pass {
			Name "BASE"
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma glsl
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _HeightMap;

			float _BumpAmt;
			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;

			float4 _Color;
			float4 _ReflectColor;
			float _Shininess;
			float _FPOW;
			float _R0;
			float _Cutoff;
			float _Height;
			float _OffsetXHeightMap;
			float _OffsetYHeightMap;
			float _MainTexAlpha;
			float _RefractiveStrength;
			float4 _LightColor0; 
			
			struct appdata_t {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				half4 vertex : POSITION;
				half2 uv_MainTex: TEXCOORD0;
				half2 uv_BumpMap : TEXCOORD1;
				half2 uv_HeightMap : TEXCOORD2;
				half4 proj : TEXCOORD3;
				half3 viewDir : TEXCOORD4;
				half3 normalDir : TEXCOORD5;
				half3 normal : TEXCOORD6;
			    half3 tangentSpaceLightDir : TEXCOORD7;
				half3 refract : TEXCOORD8;
			};
			
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			float4 _HeightMap_ST;

			v2f vert (appdata_tan v)
			{
				v2f o;
				
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv_BumpMap = TRANSFORM_TEX(v.texcoord, _BumpMap);
				o.uv_HeightMap = TRANSFORM_TEX(v.texcoord, _HeightMap);

				half4 coord = half4(v.texcoord.xy, 0 ,0);
				coord.x += _OffsetXHeightMap;
				coord.y += _OffsetYHeightMap;
				half4 tex = tex2Dlod (_HeightMap, coord);
				v.vertex.xyz += v.normal * _Height * tex.r;
				
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
				o.normal = v.normal;

				float3 binormal = cross( v.normal, v.tangent.xyz ) * v.tangent.w;
				float3x3 rotation = float3x3( v.tangent.xyz, binormal, v.normal );
				o.viewDir  = normalize(ObjSpaceViewDir(v.vertex));
				o.normalDir = normalize(mul(half4(v.normal, 0.0), unity_WorldToObject).xyz);
                o.tangentSpaceLightDir = mul(rotation, normalize(ObjSpaceViewDir(v.vertex)));
				o.refract = refract(normalize(mul (rotation, ObjSpaceViewDir(v.vertex))), fixed3(0,0,0), 1.0/_RefractiveStrength);

				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				fixed4 tex = tex2D(_MainTex, i.uv_MainTex) * _Color;

				fixed3 normal = UnpackNormal(tex2D(_BumpMap, i.uv_BumpMap));
				half rampSample = dot(normal, i.tangentSpaceLightDir);
				
				half fresnel = saturate(1.0 - dot(i.normal, i.viewDir*rampSample));
				fresnel = pow(fresnel, _FPOW);
				fresnel = _R0 + (1.0 - _R0) * fresnel;
				
				half2 offset;
				offset = i.refract.xy  + normal.rg * _BumpAmt * _GrabTexture_TexelSize.xy;
				i.proj.xy = offset * i.proj.z + i.proj.xy;
				
				half4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.proj));
				half4 colTex =  tex2D(_MainTex, i.uv_BumpMap + offset);
				#if UNITY_VERSION >= 500
				fixed4 emission = col * _Color + colTex.r * _MainTexAlpha * _LightColor0/2 + (fresnel *_ReflectColor) * _Cutoff * _LightColor0/2;
				#else 
				fixed4 emission = col * _Color + colTex.r * _MainTexAlpha * _LightColor0 + (fresnel *_ReflectColor) * _Cutoff * _LightColor0;
				#endif
				emission.a = _Color.a;
				return emission ;
			}
			ENDCG 
		}
	}	
}
}