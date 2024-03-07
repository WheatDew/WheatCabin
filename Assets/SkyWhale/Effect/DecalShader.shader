Shader "Custom/Decal"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)//��ͼ��ɫУ��
        _MainTex("Albedo (RGB)", 2D) = "white" {} //��ͼ����ɫ
        _ShadowAmount("ShadowAmount",Range(0,1)) = 0 //��Ӱ��Ũ��
    }
    SubShader
    {
        Tags{"RenderType" = "Transparent" "Queue" = "Geometry+1" "DisableBatching" = "True" } //queue��������һЩ
        Pass
        {
            Tags{"LightMode" = "ForwardBase"}
           // Blend SrcAlpha OneMinusSrcAlpha
            ZWrite off
            ZTest off
            Cull Front //�޳�ǰ������ֹ���������cube����������ʾ
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 screenPos:TEXCOORD0;
                SHADOW_COORDS(1)

              };
              float4 _Color;
              sampler2D _MainTex;
              float4 _MainTex_ST;
              sampler2D _CameraDepthTexture;  //�������
              fixed _ShadowAmount;//��Ӱ��ǿ��

              //�����ͼ������������  ������Ļ����
              float3 DepthToWorldPosition(float4 screenPos)
              {
                  float depth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,screenPos))); 
                  float4 ndcPos = (screenPos / screenPos.w) * 2 - 1;  //Ҫ����һ��w����Ϊ����Ļ�ϻ�ȡ�����꣨_ProjectionParams����û�г���w�ģ���Ҫ�ȳ�һ������ȡNDC
                  float3 clipPos = float3(ndcPos.x, ndcPos.y, 1) * _ProjectionParams.z;
                  float3 viewPos = mul(unity_CameraInvProjection,clipPos.xyzz).xyz * depth;
                  float3 worldPos = mul(UNITY_MATRIX_I_V, float4(viewPos, 1)).xyz;
                  return worldPos;
              }

              v2f vert(appdata_base v)
              {
                  v2f o;
                  o.pos = UnityObjectToClipPos(v.vertex);
                  //��Ļ����
                  o.screenPos = ComputeScreenPos(o.pos);
                  TRANSFER_SHADOW(o);
                  return o;
              }
              fixed4 frag(v2f i) :SV_Target
              {
                  float3 pos = DepthToWorldPosition(i.screenPos); //��������
                  float3 localPos = mul(unity_WorldToObject, float4(pos,1)).xyz;//�ֲ�����  ע�������ǰѱ��˵���������תΪ�Լ��ľֲ�����
                  clip(0.5 -abs(localPos));//�޳����˵�����������棬abs��ȡ����ֵ����һ��float3ȡ��ֵ�����0-1
                  //float a=0.5-abs(localPos);

                  float2 decalUV = localPos.xz + 0.5;
                  fixed4 texColor = tex2D(_MainTex, decalUV);

                  // ͸���Ȳ���
                  clip(texColor.a - 0.5); // ���磬��AlphaС��0.5ʱ����ƬԪ

                  //������Ӱ
                  // UNITY_LIGHT_ATTENUATION(atten, i, pos);
                  // texColor.rgb *= lerp(_ShadowAmount, 1, atten);

                  //return float4(a,a,a,1) ;
                  //return float4(localPos,1) ;
                  return float4(texColor.rgb, 1); // ʹ�������RGBֵ���̶�AlphaΪ1
              }

                ENDCG
            }
    }
}

