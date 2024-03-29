Shader "KriptoFX/RFX4/DiffuseMobileDemo" {
Properties {
	_Color("Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
_BumpTex("Normal (RGB)", 2D) = "gray" {}
	_MetallicTex("Metallic ", 2D) = "black" {}
	_Metallic("Metallic", Range(0,1)) = 0.0
	_Glossiness("Smoothness", Range(0,1)) = 0.5
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM
#pragma surface surf SimpleSpecular noforwardadd

sampler2D _MainTex;
sampler2D _MetallicTex;
fixed _Color;
fixed _Metallic;
fixed _Glossiness;
sampler2D _BumpTex;

half4 LightingSimpleSpecular(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 h = normalize(lightDir + viewDir);

	half diff = max(0, dot(s.Normal, lightDir));

	float nh = max(0, dot(s.Normal, h));
	float spec = pow(nh, 48.0);

	half4 c;
	c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * atten;
	c.a = s.Alpha;
	return c;
}


half4 LightingSimpleLambert(SurfaceOutput s, half3 lightDir, half atten) {
	half NdotL = dot(s.Normal, lightDir);
	half4 c;
	c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten);


	c.a = s.Alpha;
	return c;
}

struct Input {
	float2 uv_MainTex;
	float4 screenPos;
	float3 viewDir;
	float3 worldRefl; INTERNAL_DATA
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	fixed3 normal = UnpackNormal(tex2D(_BumpTex, IN.uv_MainTex));
	fixed met = tex2D(_MetallicTex, IN.uv_MainTex).a * _Metallic;
	fixed4 val = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, IN.worldRefl);
	fixed fresnel = 1 - dot(normal, IN.viewDir);
	o.Albedo = lerp(c.rgb, val.rgb,  met * fresnel);
	
	o.Normal = normal;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "Mobile/VertexLit"
}
