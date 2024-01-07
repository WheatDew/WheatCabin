Shader "KriptoFX/RFX1/Chains" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		[HDR]_TintColor("Tint Color", Color) = (0,0,0,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		
		_BumpMap("Normal (RG)", 2D) = "bump" {}
		_ReflTex("Cubemap", CUBE) = "" {}

	}




		SubShader{

		Cull Back
		ZWrite On
		Pass{

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
		#pragma multi_compile_instancing

#pragma multi_compile_fog

#include "UnityCG.cginc"


		sampler2D RFX1_PointLightAttenuation;
	half4 RFX1_AmbientColor;
	float4 RFX1_LightPositions[40];
	float4 RFX1_LightColors[40];
	int RFX1_LightCount;


	sampler2D _MainTex;
	float4 _MainTex_ST;
	half4 _Color;
	half4 _TintColor;
	samplerCUBE  RFX1_Reflection;
	float _Cutoff;
	float _Scale;
	sampler2D _BumpMap;
	samplerCUBE _ReflTex;

	struct appdata_t {
		float4 vertex : POSITION;
		fixed4 color : COLOR0;
		half3 normal : NORMAL;
		float2 texcoord : TEXCOORD0;

		UNITY_VERTEX_INPUT_INSTANCE_ID

	};

	struct v2f {
		float4 vertex : SV_POSITION;
		half3 color : COLOR0;
		float2 texcoord : TEXCOORD0;
		float3 viewDir : TEXCOORD1;
		half3 normal : NORMAL;
		half3 lightColor : TEXCOORD2;
		UNITY_FOG_COORDS(3)
			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
	};




	half3 ShadeCustomLights(float4 vertex, half3 normal, int lightCount)
	{
		float3 worldPos = mul(unity_ObjectToWorld, vertex);
		float3 worldNormal = UnityObjectToWorldNormal(normal);

		float3 lightColor = 0;
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

	v2f vert(appdata_t v)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		o.vertex = UnityObjectToClipPos(v.vertex);

		o.color = v.color;
		o.lightColor = ShadeCustomLights(v.vertex, v.normal, RFX1_LightCount) * 2;
		//o.lightColor = lerp(dot(o.lightColor, 0.33), o.lightColor, 0.25);

		o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
		o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
		o.normal = normalize(UnityObjectToWorldNormal(v.normal));


		UNITY_TRANSFER_FOG(o,o.vertex);
		return o;
	}


	half4 frag(v2f i) : SV_Target
	{
		UNITY_SETUP_INSTANCE_ID(i);
		half4 tex = tex2D(_MainTex, i.texcoord);
		if (tex.a < 0.2) discard;

		half4 finalCol = 1;
		
		half3 normal = UnpackNormal(tex2D(_BumpMap, i.texcoord));
		
		finalCol.rgb = saturate(tex * dot(texCUBE(_ReflTex, normal.xyy).rgb, 0.33) * _Color.rgb * 1.5 * i.lightColor);
		finalCol.rgb = finalCol.rgb * 0.75 + finalCol.rgb * finalCol.rgb;
		finalCol.rgb += _TintColor.rgb * i.color.rgb *  tex.rgb;
		
		//finalCol.rgb = saturate(envSample.rgb * _MainColor.rgb * 1.5 + fresnel * i.lightColor *  _MainColor.rgb * 0.25);
		//finalCol.rgb = envSample.rgb * _MainColor.rgb
		//o.Emission = _TintColor * c * IN.color;
		

		UNITY_APPLY_FOG(i.fogCoord, finalCol);

		return finalCol;
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

		struct appdata
	{
		float2 texcoord : TEXCOORD0;
		float4 vertex : POSITION;
		half3 normal : NORMAL;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};


	sampler2D _MainTex;
	float4 _MainTex_ST;
	half _Cutoff;
	fixed4 _MainColor;
	half4 _SpeedDistort;

	struct v2f
	{
		float2 texcoord : TEXCOORD3;
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
		o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
		TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
			return o;
	}

	float4 frag(v2f i) : COLOR
	{
		UNITY_SETUP_INSTANCE_ID(i);
		half4 tex = tex2D(_MainTex, i.texcoord);
		if (tex.a < 0.2) discard;
	SHADOW_CASTER_FRAGMENT(i)
	}

		ENDCG
	}


	}

}
