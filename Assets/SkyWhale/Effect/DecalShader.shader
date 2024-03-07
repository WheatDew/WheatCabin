Shader "Custom/Decal"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)//贴图颜色校正
        _MainTex("Albedo (RGB)", 2D) = "white" {} //贴图的颜色
        _ShadowAmount("ShadowAmount",Range(0,1)) = 0 //阴影的浓度
    }
    SubShader
    {
        Tags{"RenderType" = "Transparent" "Queue" = "Geometry+1" "DisableBatching" = "True" } //queue必须稍晚一些
        Pass
        {
            Tags{"LightMode" = "ForwardBase"}
           // Blend SrcAlpha OneMinusSrcAlpha
            ZWrite off
            ZTest off
            Cull Front //剔除前部，防止摄像机进入cube后，贴花不显示
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
              sampler2D _CameraDepthTexture;  //深度纹理
              fixed _ShadowAmount;//阴影的强度

              //从深度图反推世界坐标  参数屏幕坐标
              float3 DepthToWorldPosition(float4 screenPos)
              {
                  float depth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,screenPos))); 
                  float4 ndcPos = (screenPos / screenPos.w) * 2 - 1;  //要除以一个w是因为从屏幕上获取的坐标（_ProjectionParams）是没有除过w的，需要先除一下来获取NDC
                  float3 clipPos = float3(ndcPos.x, ndcPos.y, 1) * _ProjectionParams.z;
                  float3 viewPos = mul(unity_CameraInvProjection,clipPos.xyzz).xyz * depth;
                  float3 worldPos = mul(UNITY_MATRIX_I_V, float4(viewPos, 1)).xyz;
                  return worldPos;
              }

              v2f vert(appdata_base v)
              {
                  v2f o;
                  o.pos = UnityObjectToClipPos(v.vertex);
                  //屏幕坐标
                  o.screenPos = ComputeScreenPos(o.pos);
                  TRANSFER_SHADOW(o);
                  return o;
              }
              fixed4 frag(v2f i) :SV_Target
              {
                  float3 pos = DepthToWorldPosition(i.screenPos); //世界坐标
                  float3 localPos = mul(unity_WorldToObject, float4(pos,1)).xyz;//局部坐标  注意这里是把别人的世界坐标转为自己的局部坐标
                  clip(0.5 -abs(localPos));//剔除除了底面外的其他面，abs是取绝对值，对一个float3取得值结果在0-1
                  //float a=0.5-abs(localPos);

                  float2 decalUV = localPos.xz + 0.5;
                  fixed4 texColor = tex2D(_MainTex, decalUV);

                  // 透明度测试
                  clip(texColor.a - 0.5); // 例如，当Alpha小于0.5时丢弃片元

                  //计算阴影
                  // UNITY_LIGHT_ATTENUATION(atten, i, pos);
                  // texColor.rgb *= lerp(_ShadowAmount, 1, atten);

                  //return float4(a,a,a,1) ;
                  //return float4(localPos,1) ;
                  return float4(texColor.rgb, 1); // 使用纹理的RGB值，固定Alpha为1
              }

                ENDCG
            }
    }
}

