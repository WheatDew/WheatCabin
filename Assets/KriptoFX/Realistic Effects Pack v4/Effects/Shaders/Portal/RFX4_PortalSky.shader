
Shader "KriptoFX/RFX4/Portal/PortalSky" {
	Properties{
	_TintColor("Tint Color", Color) = (0.5,0.5,0.5,1)
	_TurbulenceMask("Turbulence Mask", 2D) = "white" {}
	_Cube("Environment Map", Cube) = "" {}
	_NoiseScale("Noize Scale (XYZ) Height (W)", Vector) = (1, 1, 1, 0.2)
	}
		Category{

			Tags { "Queue" = "Transparent-1" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
						Blend SrcAlpha OneMinusSrcAlpha
						Cull Off
						Lighting Off
						ZWrite Off

			SubShader {

			Stencil {
				Ref 2
				Comp Equal
				Pass Keep
				Fail Keep
			}
				Pass {


					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
		#pragma multi_compile_instancing
					#include "UnityCG.cginc"

					half4 _TintColor;
					samplerCUBE _Cube;
					float4 _NoiseScale;
					sampler2D _TurbulenceMask;

					struct appdata_t {
						float4 vertex : POSITION;
						float3 normal : NORMAL;
						UNITY_VERTEX_INPUT_INSTANCE_ID
					};

					struct v2f {
						float4 vertex : SV_POSITION;
						float3 viewDir : TEXCOORD1;

						UNITY_VERTEX_INPUT_INSTANCE_ID
							UNITY_VERTEX_OUTPUT_STEREO

					};

					float4 _MainTex_ST;

					v2f vert(appdata_t v)
					{
						v2f o;
						UNITY_SETUP_INSTANCE_ID(v);
						UNITY_TRANSFER_INSTANCE_ID(v, o);
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
						float3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
						float4 coordNoise = float4(wpos * _NoiseScale.xyz, 0);
						float4 tex1 = tex2Dlod(_TurbulenceMask, coordNoise + float4(_Time.x * 3, _Time.x * 5, _Time.x * 2.5, 0));
						v.vertex.xyz += v.normal * 0.005 + tex1.rgb * _NoiseScale.w - _NoiseScale.w / 2;


						o.vertex = UnityObjectToClipPos(v.vertex);
	
						o.viewDir = mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos;
						return o;
					}

					half4 frag(v2f i) : SV_Target
					{ UNITY_SETUP_INSTANCE_ID(i);
						float4 cubeTex = texCUBE(_Cube, i.viewDir) * _TintColor;
						return cubeTex;
					}
					ENDCG
				}
			}
	}
}
