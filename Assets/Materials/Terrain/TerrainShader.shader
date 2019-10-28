// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Standard shader with triplanar mapping
// https://github.com/keijiro/StandardTriplanar
// Grid stuff:
// https://www.gamedev.net/forums/topic/529926-terrain-contour-lines-using-pixel-shader/

Shader "Custom/TerrainShader"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _MainTex("Texture", 2D) = "white" {}

        _Glossiness("Shine", Range(0, 1)) = 0.5
        [Gamma] _Metallic("Metallic", Range(0, 1)) = 0

        _BumpScale("Bump Strength", Float) = 1
        _BumpMap("Bump Map", 2D) = "bump" {}

        _MapScale("Scale", Float) = 1
        
        _GridAlpha("Grid Alpha", Range(0, 1)) = 1
        _GridCol ("Grid Color", Color) = (1,1,1,1)
        _GridStep ("Grid size", Float) = 10
        _GridWidth ("Grid width", Float) = 1

        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM

        #pragma surface surf Standard vertex:vert fullforwardshadows addshadow

        #pragma target 3.0

        half4 _Color;
        sampler2D _MainTex;

        half _Glossiness;
        half _Metallic;

        half _BumpScale;
        sampler2D _BumpMap;

        half _MapScale;

        fixed4 _GridCol;
        float _GridStep;
        float _GridWidth;
        float _GridAlpha;

        struct Input
        {
            float3 worldPosition;
            float3 localNormal;
        };

        void vert(inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            data.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
            data.localNormal = v.normal.xyz;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Blending factor of triplanar mapping
            float3 bf = normalize(abs(IN.localNormal));
            bf /= dot(bf, (float3)1);

            // Triplanar mapping
            float2 tx = IN.worldPosition.yz * _MapScale;
            float2 ty = IN.worldPosition.zx * _MapScale;
            float2 tz = IN.worldPosition.xy * _MapScale;

            // Base color
            half4 cx = tex2D(_MainTex, tx) * bf.x;
            half4 cy = tex2D(_MainTex, ty) * bf.y;
            half4 cz = tex2D(_MainTex, tz) * bf.z;
            half4 color = (cx + cy + cz) * _Color;

            // grid overlay
            float2 pos = IN.worldPosition.xz / _GridStep;
            float2 f  = abs(frac(pos)-.5);
            float2 df = fwidth(pos) * _GridWidth;
            float2 g = smoothstep(-df ,df , f);
            float grid = 1.0 - saturate(g.x * g.y);
            color.rgb = lerp(color.rgb, _GridCol, grid * _GridAlpha);
           

            o.Albedo = color.rgb;

            // Normal map
            half4 nx = tex2D(_BumpMap, tx) * bf.x;
            half4 ny = tex2D(_BumpMap, ty) * bf.y;
            half4 nz = tex2D(_BumpMap, tz) * bf.z;
            o.Normal = UnpackScaleNormal(nx + ny + nz, _BumpScale);

            // Misc parameters
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
