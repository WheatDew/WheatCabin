// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Effects/Water" {
Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
		_RimColor("Rim Color", Color) = (1,1,1,0.5)
        _BumpMap ("Normalmap", 2D) = "bump" {}
		_HeightMap ("_HeightMap (r)", 2D) = "white" {}
		_Height ("_Height", Float) = 0.2
		_OffsetXHeightMap ("_OffsetXHeightMap", Range (0, 1)) = 0
		_OffsetYHeightMap ("_OffsetYHeightMap", Range (0, 1)) = 0
        _FPOW("FPOW Fresnel", Float) = 5.0
        _R0("R0 Fresnel", Float) = 0.05
		_Cutoff ("Emission strength", Range (0, 1)) = 0.5
		_BumpAmt ("Distortion", Float) = 10
}
Category {
	
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
				Blend SrcAlpha OneMinusSrcAlpha
				ZWrite Off
				Offset -1,-1
				Cull Back
				Fog { Mode Off}

	SubShader {
		GrabPass {							
			Name "BASE"
			Tags { "LightMode" = "Always" }
 		}
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma glsl
			
			#include "UnityCG.cginc"

			sampler2D _BumpMap;
			sampler2D _HeightMap;

			float _BumpAmt;
			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;

			float4 _Color;
			float4 _RimColor;
			float4 _ReflectColor;
			float _Shininess;
			float _FPOW;
			float _R0;
			float _Cutoff;
			float _Height;
			float _OffsetXHeightMap;
			float _OffsetYHeightMap;
			
			struct appdata_t {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				half4 vertex : POSITION;
				half2 uv_BumpMap : TEXCOORD1;
				half2 uv_Height : TEXCOORD2;
				half4 proj : TEXCOORD3;
				half3 normalDir : TEXCOORD4;
			    half3 tangentSpaceLightDir : TEXCOORD5;
				fixed4 color : COLOR;
				half3 normal: TEXCOORD6;
				half3 viewDir : TEXCOORD7;
			};
			
			float4 _BumpMap_ST;
			float4 _Height_ST;

			v2f vert (appdata_full v)
			{
				v2f o;
				
				o.uv_BumpMap = TRANSFORM_TEX(v.texcoord, _BumpMap);
				o.uv_Height = TRANSFORM_TEX(v.texcoord, _Height);
				
				float4 oPos = UnityObjectToClipPos(v.vertex);
				
				float4 coord = float4(v.texcoord.xy, 0 ,0);
				coord.x += _OffsetXHeightMap;
				coord.y += _OffsetYHeightMap;
				float4 tex = tex2Dlod (_HeightMap, coord);
				v.vertex.xyz += v.normal * _Height * tex.r;

				o.vertex = UnityObjectToClipPos(v.vertex);
				
				#if UNITY_UV_STARTS_AT_TOP
					float scale = -1.0;
				#else
					float scale = 1.0;
				#endif
				o.proj.xy = (float2(oPos.x, oPos.y*scale) + oPos.w) * 0.5;
				o.proj.zw = oPos.w;
				
#if UNITY_SINGLE_PASS_STEREO
		o.proj.xy = TransformStereoScreenSpaceTex(o.proj.xy, o.proj.w);
#endif
				o.color = v.color;
				o.normal = v.normal;

				o.viewDir  = normalize(ObjSpaceViewDir(v.vertex));
				float3 binormal = cross( v.normal, v.tangent.xyz ) * v.tangent.w;
				float3x3 rotation = float3x3( v.tangent.xyz, binormal, v.normal );
				o.normalDir = normalize(mul(half4(v.normal, 0.0), unity_WorldToObject).xyz);
                o.tangentSpaceLightDir = mul(rotation, normalize(ObjSpaceViewDir(v.vertex)));

				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				fixed3 normal = UnpackNormal(tex2D(_BumpMap, i.uv_BumpMap));
				half rampSample = dot(normal, i.tangentSpaceLightDir);
				
				half fresnel = saturate(1.0 - dot(normal, i.tangentSpaceLightDir));
				fresnel = pow(fresnel, _FPOW);
				fresnel = _R0 + (1.0 - _R0) * fresnel;


				half fresnelRim = saturate(0.7 - dot(i.normal, i.viewDir));
				fresnelRim = pow(fresnelRim, _FPOW);
				fresnelRim = _R0 + (1.0 - _R0) * fresnelRim;

				half2 offset = normal.rg * _BumpAmt * _GrabTexture_TexelSize.xy;
				i.proj.xy = offset * i.proj.z + i.proj.xy;
				half4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.proj));

				fixed3 emission = col.xyz * _Color.xyz + (fresnel *_ReflectColor.xyz) * _Cutoff * col.xyz  + col.xyz * (fresnelRim * _RimColor.xyz)* _Cutoff;
				return fixed4 (emission, _Color.a);
			}
			ENDCG 
		}
	}	
}
}