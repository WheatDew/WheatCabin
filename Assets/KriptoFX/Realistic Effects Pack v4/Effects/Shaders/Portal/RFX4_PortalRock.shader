Shader "KriptoFX/RFX4/Portal/Rock" 
{
	Properties
	{
		[HDR]_TintColor("Tint Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		[HDR]_EmissionColor("Tint Color", Color) = (1,1,1,1)
		_EmissionTex("Emission Texture", 2D) = "white" {}

		[HDR]_FresnelColor("Fresnel Color", Color) = (1,1,1,1)
		_FresnelPow("Fresnel Pow", Float) = 5
		_FresnelR0("Fresnel R0", Float) = 0.04
	}

		Category{
		

			SubShader{

			

			Pass{

			CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#pragma multi_compile_fog
#pragma multi_compile_instancing


#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _EmissionTex;
		sampler2D _CameraDepthTexture;

		float4 _MainTex_ST;
		float4 _EmissionTex_ST;

		half4 _TintColor;
		half4 _EmissionColor;
		half4 _FresnelColor;
		half4 _DistortionSpeedScale;
		fixed4 _FogColorMultiplier;

		half _FresnelPow;
		half _FresnelR0;


		struct appdata_t {
			float4 vertex : POSITION;
			float4 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f {
			float4 vertex : SV_POSITION;
			float2 texcoord : TEXCOORD0;
			float2 texcoord2 : TEXCOORD1;
			float fresnel : TEXCOORD2;
			UNITY_FOG_COORDS(4)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
		};


		v2f vert(appdata_t v)
		{
			v2f o;
		
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_TRANSFER_INSTANCE_ID(v, o);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.texcoord2.xy = TRANSFORM_TEX(v.texcoord, _EmissionTex);

			o.fresnel = 1 - dot(normalize(v.normal), normalize(ObjSpaceViewDir(v.vertex)));
			o.fresnel = pow(o.fresnel, _FresnelPow);
			o.fresnel = saturate(_FresnelR0 + (1.0 - _FresnelR0) * o.fresnel);

			UNITY_TRANSFER_FOG(o,o.vertex);
			
			return o;
		}


		half4 frag(v2f i) : SV_Target
		{
			UNITY_SETUP_INSTANCE_ID(i);
			

		half4 tex = tex2D(_MainTex, i.texcoord);
		half4 emission = tex2D(_EmissionTex, i.texcoord2) * _EmissionColor;
		
		half4 res = 2 * tex *  _TintColor;

		res.rgb += i.fresnel * _FresnelColor;
		res.rgb += emission.rgb;
		res.a = saturate(res.a);

		UNITY_APPLY_FOG(i.fogCoord, res);

		return res;
		}
			ENDCG
		}
		}
		}

			Fallback "Diffuse"

}