Shader "KriptoFX/RFX1/CutoutBorder" {
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	[HDR]_EmissionColor("Emission Color", Color) = (1,1,1,1)
		_EmissionTex("Emission (A)", 2D) = "black" {}
	//_BumpTex("Normal (RGB)", 2D) = "gray" {}
	_Cutoff("_Cutoff", Range(0,1)) = 0
		//_Cutout2 ("Cutout2", Range(0,1)) = 0
		[HDR]_BorderColor("Border Color", Color) = (1,1,1,1)
		_CutoutThickness("Cutout Thickness", Range(0,1)) = 0.03
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "AlphaTest-1" }
		LOD 100
		//Cull Off
		ZWrite On

		Pass
	{
		CGPROGRAM

#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog
		#pragma multi_compile_instancing

#include "UnityCG.cginc"


		sampler2D _MainTex;
	sampler2D _EmissionTex;
	sampler2D _BumpTex;

	half _Glossiness;
	half _Metallic;
	half4 _Color;
	half4 _BorderColor;
	half4 _EmissionColor;
	half4 _MainTex_ST;
	half4 _BumpTex_ST;

	half4 _EmissionTex_ST;
	half _CutoutThickness;
	half _Cutoff;

	sampler2D RFX1_PointLightAttenuation;
	half4 RFX1_AmbientColor;
	float4 RFX1_LightPositions[4];
	float4 RFX1_LightColors[4];
	int RFX1_LightCount;

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		half4 color : COLOR;
		half3 normal : NORMAL;

		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float2 uv2 : TEXCOORD1;
		UNITY_FOG_COORDS(2)
			float4 vertex : SV_POSITION;
		half4 color : TEXCOORD3;
		half3 light : TEXCOORD4;
		float2 uv3 : TEXCOORD5;
		UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
	};

	half3 ShadeCustomLights(float4 vertex, half3 normal, int lightCount)
	{
		float3 worldPos = mul(unity_ObjectToWorld, vertex);
		float3 worldNormal = UnityObjectToWorldNormal(normal);

		float3 lightColor = RFX1_AmbientColor.xyz;
		for (int i = 0; i < lightCount; i++) {
			float3 lightDir = RFX1_LightPositions[i].xyz - worldPos.xyz * RFX1_LightColors[i].w;
			half normalizedDist = length(lightDir) / RFX1_LightPositions[i].w;
			fixed attenuation = tex2Dlod(RFX1_PointLightAttenuation, half4(normalizedDist.xx, 0, 0));
			attenuation = lerp(1, attenuation, RFX1_LightColors[i].w);
			float diff = max(0, dot(normalize(worldNormal), normalize(lightDir)));
			lightColor += RFX1_LightColors[i].rgb * (diff * attenuation);
		}
		return (lightColor);
	}

	v2f vert(appdata v)
	{
		v2f o;

		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		o.uv2 = TRANSFORM_TEX(v.uv, _EmissionTex);
		o.uv3 = TRANSFORM_TEX(v.uv, _BumpTex);
		o.color = v.color;

		o.light = ShadeCustomLights(v.vertex, v.normal, RFX1_LightCount);


		UNITY_TRANSFER_FOG(o,o.vertex);

		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		UNITY_SETUP_INSTANCE_ID(i);
		half4 c = tex2D(_MainTex, i.uv) * _Color;
		c.rgb = saturate(c.rgb * i.color.rgb * (i.light, 0.33) * 10) + i.light * 0.15;
		half cutoff = _Cutoff + (1 - i.color.a);
		clip(c.a - cutoff);
		if (c.a < cutoff + _CutoutThickness) c.rgb += _BorderColor;
		else c.rgb += tex2D(_EmissionTex, i.uv2).rgb * _EmissionColor.rgb * i.color.rgb;
		return c;
	}
		ENDCG
	}


		Pass
	{
		Tags{ "Queue" = "Transparent" "LightMode" = "ShadowCaster" }

		CGPROGRAM

#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_shadowcaster
#pragma multi_compile_instancing


#include "UnityCG.cginc"

		sampler2D _MainTex;
	half4 _MainTex_ST;
	half _Cutoff;
	half4 _Color;

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		float4 color : COLOR0;
		half3 normal : NORMAL;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};


	struct v2f
	{
		float2 uv : TEXCOORD1;
		float4 color : COLOR0;
		V2F_SHADOW_CASTER;
		UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
	};

	v2f vert(appdata v)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		o.color.a = v.color.a;
		TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
			return o;
	}

	float4 frag(v2f i) : COLOR
	{
		UNITY_SETUP_INSTANCE_ID(i);
		half cutoff = _Cutoff + (1 - i.color.a);
	clip(tex2D(_MainTex, i.uv).a - cutoff);
	SHADOW_CASTER_FRAGMENT(i)
	}

		ENDCG
	}
	}
		//Fallback "Transparent/Cutout/Diffuse"
}
