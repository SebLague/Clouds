Shader "Custom/Grid"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _GridCol ("Grid Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _GridStep ("Grid size", Float) = 10
        _GridWidth ("Grid width", Float) = 1
        _Test ("Test", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
       
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
 
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
 
        sampler2D _MainTex;
 
        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
        };
 
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _GridCol;
        float _GridStep;
        float _GridWidth;
        float4 _Test;
       
        void surf (Input IN, inout SurfaceOutputStandard o) {
             // Blending factor of triplanar mapping
            float3 bf = normalize(abs(IN.worldNormal));
            bf /= dot(bf, 1);

            // Triplanar mapping
            float2 tx = IN.worldPos.yz * _Test.x;
            float2 ty = IN.worldPos.zx * _Test.x;
            float2 tz = IN.worldPos.xy * _Test.x;
            // Base color
            half4 cx = tex2D(_MainTex, tx) * bf.x;
            half4 cy = tex2D(_MainTex, ty) * bf.y;
            half4 cz = tex2D(_MainTex, tz) * bf.z;
            float4 c = (cx + cy + cz) * _Color;
            

            // Albedo comes from a texture tinted by color
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
           
            // grid overlay
            float2 pos = IN.worldPos.xz / _GridStep;
            float2 f  = abs(frac(pos)-.5);
            float2 df = fwidth(pos) * _GridWidth;
            float2 g = smoothstep(-df ,df , f);
            float grid = 1.0 - saturate(g.x * g.y);
            c.rgb = lerp(c.rgb, _GridCol, grid);
           
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
 