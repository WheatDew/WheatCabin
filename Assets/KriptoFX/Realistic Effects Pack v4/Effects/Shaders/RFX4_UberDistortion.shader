
Shader "KriptoFX/RFX4/Distortion"
{
	Properties
	{
		[Header(Main Settings)]
	[Toggle(_EMISSION)] _UseMainTex("Use Main Texture", Int) = 0
		[PerRendererData][HDR]_TintColor("Tint Color", Color) = (1,1,1,1)
		_TintDistortion("Tint Distortion", Float) = 1
		_MainTex("Main Texture", 2D) = "black" {}
			[Header(Main Settings)]
		[Normal]_NormalTex("Normal(RG) Alpha(A)", 2D) = "bump" {}
		[PerRendererData][HDR]_MainColor("Main Color", Color) = (1,1,1,1)
			[PerRendererData]	_Distortion("Distortion", Float) = 100
	[Toggle(USE_REFRACTIVE)] _UseRefractive("Use Refractive Distort", Int) = 0
		_RefractiveStrength("Refractive Strength", Range(-1, 1)) = 0

	[Toggle(_FADING_ON)] _UseSoft("Use Soft Particles", Int) = 0
		_InvFade("Soft Particles Factor", Float) = 3
		[Space]
		[Header(Height Settings)]
	[Toggle(USE_HEIGHT)] _UseHeight("Use Height Map", Int) = 0
		_HeightTex("Height Tex", 2D) = "white" {}
		_Height("_Height", Float) = 0.1
		_HeightUVScrollDistort("Height UV Scroll(XY)", Vector) = (8, 12, 0, 0)

		[Space]
		[Header(Fresnel)]
	[Toggle(USE_FRESNEL)] _UseFresnel("Use Fresnel", Int) = 0
		[HDR]_FresnelColor("Fresnel Color", Color) = (0.5,0.5,0.5,1)
		_FresnelInvert("Fresnel Invert", Range(0, 1)) = 1
		_FresnelPow("Fresnel Pow", Float) = 5
		_FresnelR0("Fresnel R0", Float) = 0.04
		_FresnelDistort("Fresnel Distort", Float) = 1500

		[Space]
		[Header(Cutout)]
	[Toggle(USE_CUTOUT)] _UseCutout("Use Cutout", Int) = 0
		_CutoutTex("Cutout Tex", 2D) = "white" {}
	[PerRendererData]	_Cutout("Cutout", Range(0, 1.2)) = 1
		[HDR]_CutoutColor("Cutout Color", Color) = (1,1,1,1)
		_CutoutThreshold("Cutout Threshold", Range(0, 1)) = 0.015

		[Space]
		[Header(Rendering)]
		[Toggle] _ZWriteMode("ZWrite On?", Int) = 0
		[Enum(Off,0,Front,1,Back,2)] _CullMode("Culling", Float) = 0 //0 = off, 2=back
		[Toggle(USE_ALPHA_CLIPING)] _UseAlphaCliping("Use Alpha Cliping", Int) = 0
		_AlphaClip("Alpha Clip Threshold", Float) = 100
		[Toggle(_FLIPBOOK_BLENDING)] _UseBlending("Use Blending", Int) = 0


	}
		SubShader
		{
			GrabPass {
			"_GrabTexture"
			}

			Tags { "Queue" = "Transparent+1" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
			ZWrite[_ZWriteMode]
			Cull[_CullMode]

			Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#pragma multi_compile_particles
				#pragma multi_compile_instancing

				#pragma shader_feature USE_REFRACTIVE
				#pragma shader_feature _FADING_ON
				#pragma shader_feature USE_FRESNEL
				#pragma shader_feature USE_CUTOUT
				#pragma shader_feature USE_HEIGHT
				#pragma shader_feature USE_ALPHA_CLIPING
				#pragma shader_feature _FLIPBOOK_BLENDING
				#pragma shader_feature _EMISSION
				#pragma multi_compile _ DISTORT_OFF

				#include "UnityCG.cginc"


				UNITY_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture);
				sampler2D _MainTex;
				sampler2D _NormalTex;
				float4 _NormalTex_ST;
				float4 _MainTex_ST;
				half _RefractiveStrength;
				half _InvFade;

				sampler2D _HeightTex;
				float4 _HeightTex_ST;
				half4 _HeightUVScrollDistort;
				half _Height;

				float4 _GrabTexture_TexelSize;
				half4 _FresnelColor;
				half _FresnelInvert;
				half _FresnelPow;
				half _FresnelR0;
				half _FresnelDistort;

				sampler2D _CutoutTex;
				float4 _CutoutTex_ST;

				half4 _CutoutColor;
				half _CutoutThreshold;
				half _AlphaClip;
				half _TintDistortion;

				UNITY_DECLARE_DEPTH_TEXTURE (_CameraDepthTexture);
				float4x4 _InverseTransformMatrix;

				UNITY_INSTANCING_BUFFER_START(MyProperties)
					UNITY_DEFINE_INSTANCED_PROP(float4, _MainColor)
#define _MainColor_arr MyProperties
					UNITY_DEFINE_INSTANCED_PROP(float4, _TintColor)
#define _TintColor_arr MyProperties
					UNITY_DEFINE_INSTANCED_PROP(half, _Distortion)
#define _Distortion_arr MyProperties
					UNITY_DEFINE_INSTANCED_PROP(half, _Cutout)
#define _Cutout_arr MyProperties
				UNITY_INSTANCING_BUFFER_END(MyProperties)



				struct appdata
				{
					float4 vertex : POSITION;
	#if defined (USE_HEIGHT) || defined (USE_REFRACTIVE) || defined (USE_FRESNEL)
					half3 normal : NORMAL;
	#endif
	#ifdef USE_REFRACTIVE
					half4 tangent : TANGENT;
	#endif
					half4 color : COLOR;
	#ifdef _FLIPBOOK_BLENDING
	#if UNITY_VERSION == 600
					float4 uv : TEXCOORD0;
					float texcoordBlend : TEXCOORD1;
	#else
					float2 uv : TEXCOORD0;
					float4 texcoordBlendFrame : TEXCOORD1;
	#endif
	#else
					float2 uv : TEXCOORD0;
	#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					half4 color : COLOR;
					half4 uvgrab : TEXCOORD0;
					UNITY_FOG_COORDS(1)
	#ifdef _FLIPBOOK_BLENDING
						float4 uv : TEXCOORD2;
					fixed blend : TEXCOORD3;
	#else
						float2 uv : TEXCOORD2;
	#endif

	#ifdef USE_CUTOUT
					float2 uvCutout : TEXCOORD4;
	#endif
	#ifdef _FADING_ON
		#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD5;
		#endif
	#endif

	#ifdef _EMISSION
					float2 mainUV : TEXCOORD6;
	#endif
	#ifdef USE_FRESNEL
	#if  defined (USE_HEIGHT)
					half4 localPos : TEXCOORD7;
					half3 viewDir : TEXCOORD8;
	#else
					half fresnel : TEXCOORD7;
	#endif
	#endif
	#ifdef USE_REFRACTIVE
					half2 refract : TEXCOORD9;
	#endif

					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				v2f vert(appdata v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					float2 offset = 0;
	#ifdef USE_HEIGHT
					offset = _Time.xx * _HeightUVScrollDistort.xy;
	#endif

	#ifdef _EMISSION
					o.mainUV = TRANSFORM_TEX(v.uv.xy, _MainTex) + offset;
	#endif

	#ifdef _FLIPBOOK_BLENDING
	#if UNITY_VERSION == 600
					o.uv.xy = TRANSFORM_TEX(v.uv.xy, _NormalTex) + offset;
					o.uv.zw = TRANSFORM_TEX(v.uv.zw, _NormalTex) + offset;
					o.blend = v.texcoordBlend;
	#else
					o.uv.xy = TRANSFORM_TEX(v.uv, _NormalTex) + offset;
					o.uv.zw = TRANSFORM_TEX(v.texcoordBlendFrame.xy, _NormalTex) + offset;
					o.blend = v.texcoordBlendFrame.z;
	#endif
	#else
					o.uv.xy = TRANSFORM_TEX(v.uv, _NormalTex) + offset;
	#endif

	#ifdef USE_HEIGHT
					float4 uv2 = float4(TRANSFORM_TEX(v.uv, _HeightTex) + offset, 0, 0);
					float4 tex = tex2Dlod(_HeightTex, uv2);
					half3 norm = normalize(v.normal);
					v.vertex.xyz += norm * _Height * tex - norm * _Height / 2;
	#endif

	#ifdef USE_CUTOUT
					o.uvCutout = TRANSFORM_TEX(v.uv, _CutoutTex);
	#endif
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;

	#ifdef USE_REFRACTIVE
					float3 binormal = cross(v.normal, v.tangent.xyz) * v.tangent.w;
					float3x3 rotation = float3x3(v.tangent.xyz, binormal, v.normal);
					o.refract = refract(normalize(mul(rotation, ObjSpaceViewDir(v.vertex))), 0, _RefractiveStrength) * v.color.a * v.color.a;
	#endif

					o.uvgrab = ComputeGrabScreenPos(o.vertex);

	#ifdef _FADING_ON
		#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos(o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
		#endif
	#endif

	#ifdef USE_FRESNEL
	#if  defined (USE_HEIGHT)
					o.localPos = v.vertex;
					o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
	#else
					o.fresnel = (_FresnelInvert - abs(dot(normalize(v.normal), normalize(ObjSpaceViewDir(v.vertex)))));
					o.fresnel = pow(o.fresnel, _FresnelPow);
					o.fresnel = saturate(_FresnelR0 + (1.0 - _FresnelR0) * o.fresnel);
	#endif
	#endif

					UNITY_TRANSFER_FOG(o, o.vertex);
					return o;
				}

				half4 frag(v2f i) : SV_Target
				{
#ifdef DISTORT_OFF
					return 0;
#endif

				UNITY_SETUP_INSTANCE_ID(i);


	#ifdef _FLIPBOOK_BLENDING
				half4 dist1 = tex2D(_NormalTex, i.uv.xy);
				half4 dist2 = tex2D(_NormalTex, i.uv.zw);
				half3 dist = UnpackNormal(lerp(dist1, dist2, i.blend));
	#else
				half3 dist = UnpackNormal(tex2D(_NormalTex, i.uv));
	#endif


	#ifdef USE_ALPHA_CLIPING
				//half alphaBump = saturate((dot(abs(dist.rg), 0.5) - 0.00392) * _AlphaClip);
				half alphaBump = saturate((0.94 - pow(dist.z, 127)) * _AlphaClip * 0.1);

	#endif
				half fade = 1;
#ifdef _FADING_ON
	#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,  i.projPos.xy / i.projPos.w));
				float partZ = i.projPos.z;
				fade = saturate(_InvFade * (sceneZ - partZ));
				i.color.a *= fade;
	#endif
#endif

				half2 offset = dist.rg * UNITY_ACCESS_INSTANCED_PROP(_Distortion_arr, _Distortion) * _GrabTexture_TexelSize.xy * i.color.a * fade;

				half3 fresnelCol = 0;
	#ifdef USE_FRESNEL

	#if  defined (USE_HEIGHT)
	#ifdef UNITY_UV_STARTS_AT_TOP
				half3 n = normalize(cross(ddx(i.localPos.xyz), ddy(i.localPos.xyz) * _ProjectionParams.x));
	#else
				half3 n = normalize(cross(ddx(i.localPos.xyz), -ddy(i.localPos.xyz) * _ProjectionParams.x));
	#endif
				half fresnel = (_FresnelInvert - dot(n, i.viewDir));
				fresnel = pow(fresnel, _FresnelPow);
				fresnel = saturate(_FresnelR0 + (1.0 - _FresnelR0) * fresnel);
				offset += fresnel * _GrabTexture_TexelSize.xy * _FresnelDistort * dist.rg;
				fresnelCol = _FresnelColor * fresnel * abs(dist.r + dist.g) * 2 * i.color.rgb * i.color.a;
	#else
				offset += i.fresnel * _GrabTexture_TexelSize.xy * _FresnelDistort * dist.rg;
				fresnelCol = _FresnelColor * i.fresnel * abs(dist.r + dist.g) * 2 * i.color.rgb * i.color.a;
	#endif

	#endif

				half4 cutoutCol = 0;
				cutoutCol.a = 1;
	#ifdef USE_CUTOUT
				half cutout = UNITY_ACCESS_INSTANCED_PROP(_Cutout_arr, _Cutout);
				cutout = i.color.a - cutout;
				half mask = tex2D(_CutoutTex, i.uvCutout).r - (dist.r + dist.g) / 10;
				fixed diffMask = cutout - mask;
				fixed alphaMask = saturate(diffMask * 10000);
				fixed alphaMaskThreshold = saturate((diffMask - _CutoutThreshold) * 10000);
				cutoutCol.rgb = lerp(cutoutCol.rgb, _CutoutColor, saturate((1 - alphaMaskThreshold) * alphaMask));
				cutoutCol.a = alphaMask;
	#endif

	#ifdef USE_ALPHA_CLIPING
				offset *= alphaBump;
	#endif

	#ifdef USE_REFRACTIVE
				i.uvgrab.xy += i.refract * i.color.a;
	#endif

				i.uvgrab.xy = offset * i.color.a + i.uvgrab.xy;
				//half4 grabColor = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));;
				half4 grabColor = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture, float4(i.uvgrab.xy / i.uvgrab.w, 0, 0));


				half4 result;
				half4 mainCol = UNITY_ACCESS_INSTANCED_PROP(_MainColor_arr, _MainColor);
				result.rgb = grabColor * lerp(float3(1, 1, 1), mainCol.rgb,  i.color.a) + fresnelCol * grabColor + cutoutCol.rgb;

	#ifdef _EMISSION
				half4 tintCol = UNITY_ACCESS_INSTANCED_PROP(_TintColor_arr, _TintColor);
				half4 emissionCol = tex2D(_MainTex, i.mainUV + offset * _TintDistortion);
				emissionCol.rgb *= emissionCol.a * tintCol.rgb * i.color.a * tintCol.a;
				UNITY_APPLY_FOG_COLOR(i.fogCoord, emissionCol, half4(0,0,0,0));
				result.rgb += emissionCol.rgb;
	#endif
				result.a = lerp(saturate(dot(fresnelCol, 0.33) * 10) * _FresnelColor.a, mainCol.a , mainCol.a) * cutoutCol.a;
				//#ifdef DISTORT_ON
				//			result.a *= i.color.a;
				//#endif
				#ifdef USE_ALPHA_CLIPING
							result.a *= alphaBump;
				#endif

							result.a = saturate(result.a);
							return result;
							}

							ENDCG
					}

		}
			CustomEditor "RFX4_UberDistortionGUI"
}
