// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TriBump"
{
    Properties {
        _Side("Side", 2D) = "white" {}
        _Top("Top", 2D) = "white" {}
        [NoScaleOffset] _Bottom("Bottom", 2D) = "white" {}
        [NoScaleOffset] _BumpMap("NormalMap", 2D) = "bump" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
 
        CGINCLUDE
        #include "HLSLSupport.cginc"
        #include "UnityShaderVariables.cginc"  
        #include "UnityCG.cginc"
   
        sampler2D _Side, _Top, _Bottom, _BumpMap;
        float4 _Side_ST, _Top_ST;
 
        void GetTriplanarTextures(float3 worldPos, float3 worldNormal, out fixed4 albedo, out half3 normal)
        {
                // extract world normal from the unused w component of world to tangent matrix
                float3 projNormal = saturate(pow(worldNormal * 1.4, 4));
 
                // "normalize" projNormal x+y+z to equal 1, ensures even blend
                projNormal /= projNormal.x + projNormal.y + projNormal.z;
   
                // SIDE X
                float xsign = sign(worldNormal.x);
                float2 zy = worldPos.zy * float2(xsign, 1.0) * _Side_ST.xy + _Side_ST.zw;
                fixed4 xAlbedo = tex2D(_Side, zy);
                half3 xNorm = UnpackNormal(tex2D(_BumpMap, zy));
                // xNorm.z *= xsign; // flip normal based on +/- x
   
                // TOP / BOTTOM
                float ysign = sign(worldNormal.y);
                float2 zx = worldPos.zx * _Top_ST.xy + _Top_ST.zw;
                fixed4 yAlbedo = ysign > 0 ? tex2D(_Top, zx) : tex2D(_Bottom, zx);
                half3 yNorm = UnpackNormal(tex2D(_BumpMap, zx));
                // yNorm.z *= ysign;
   
                // SIDE Z
                float zsign = sign(worldNormal.z);
                float2 xy = worldPos.xy * float2(-zsign, 1.0) * _Side_ST.xy + _Side_ST.zw;
                fixed4 zAlbedo = tex2D(_Side, xy);
                half3 zNorm = UnpackNormal(tex2D(_BumpMap, xy));
                // zNorm.z *= zsign; // flip normal based on +/- z
 
                // use normal blending to wrap normal map to surface normal
                xNorm = normalize(half3(xNorm.xy * float2(xsign, 1.0) + worldNormal.zy, worldNormal.x));
                yNorm = normalize(half3(yNorm.xy + worldNormal.zx, worldNormal.y));
                zNorm = normalize(half3(zNorm.xy * float2(-zsign, 1.0) + worldNormal.xy, worldNormal.z));
 
                // reorient normals to their world axis
                xNorm = xNorm.zyx; // hackily swizzle channels to match unity "right"
                yNorm = yNorm.yzx; // hackily swizzle channels to match unity "up"
                zNorm = zNorm.xyz; // no swizzle needed
 
                // blend normals together
                normal = xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
   
                // blend albedos together
                albedo = xAlbedo * projNormal.x + yAlbedo * projNormal.y + zAlbedo * projNormal.z;
        }
        ENDCG
 
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
 
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : NORMAL;
                float3 worldPos : TEXCOORD0;
                SHADOW_COORDS(1)
                UNITY_FOG_COORDS(2)
            };
         
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
         
            half4 frag (v2f i) : SV_Target
            {
                fixed4 albedo;
                half3 normal;
                GetTriplanarTextures(i.worldPos, i.worldNormal, albedo, normal);
             
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                half3 lighting = saturate(dot(normalize(normal), _WorldSpaceLightPos0.xyz)) * _LightColor0.rgb * atten;
                lighting += ShadeSH9(half4(normal,1));
 
                half3 col = albedo.rgb * lighting;
 
                UNITY_APPLY_FOG(i.fogCoord, col);
                return half4(col, 1);
            }
            ENDCG
        }
 
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }
            ZWrite Off Blend One One
 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog
            #pragma multi_compile_fwdadd_fullshadows
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
 
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : NORMAL;
                float3 worldPos : TEXCOORD0;
                SHADOW_COORDS(1)
                UNITY_FOG_COORDS(2)
            };
         
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
         
            half4 frag (v2f i) : SV_Target
            {
                fixed4 albedo;
                half3 normal;
                GetTriplanarTextures(i.worldPos, i.worldNormal, albedo, normal);
             
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
 
            #ifndef USING_DIRECTIONAL_LIGHT
                fixed3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
            #else
                fixed3 lightDir = _WorldSpaceLightPos0.xyz;
            #endif
                half3 lighting = saturate(dot(normalize(normal), lightDir)) * _LightColor0.rgb * atten;
 
                half3 col = albedo.rgb * lighting;
 
                UNITY_APPLY_FOG(i.fogCoord, col);
                return half4(col, 1);
            }
            ENDCG
        }
 
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}
 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
 
            struct v2f {
                V2F_SHADOW_CASTER;
            };
 
            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }
 
            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
}