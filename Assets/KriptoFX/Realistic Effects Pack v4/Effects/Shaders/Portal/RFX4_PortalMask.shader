Shader "KriptoFX/RFX4/Portal/PortalMask"
{
	Properties{
		_TurbulenceMask("Turbulence Mask", 2D) = "white" {}
		_NoiseScale("Noize Scale (XYZ) Height (W)", Vector) = (1, 1, 1, 0.2)
	}
		SubShader
		{
			Tags { "RenderType" = "Tranperent" "Queue" = "Geometry-100" "IgnoreProjector" = "True" }
			ColorMask 0
			ZWrite off
			Stencil
			{
				Ref 2
				Comp always
				Pass replace
			}

			Pass
			{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing

				#include "UnityCG.cginc"

				float4 _NoiseScale;
				sampler2D _TurbulenceMask;

				struct appdata
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					UNITY_VERTEX_INPUT_INSTANCE_ID
						UNITY_VERTEX_OUTPUT_STEREO
				};

				v2f vert(appdata v)
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
					return o;
				}

				half4 frag(v2f i) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);
					return 0;
				}
			ENDCG
			}
		}
}
