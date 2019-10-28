Shader "Custom/Terrain" {
    Properties {
        _GrassColour ("Grass Colour", Color) = (0,1,0,1)
        _SnowColour ("Snow Colour", Color) = (1,1,1,1)
        _SnowHeight ("Snow Line", Float) = .5
        _SnowBlend ("Snow Blend", Float) = .5
        _RockColour ("Rock Colour", Color) = (1,1,1,1)
        _GrassSlopeThreshold ("Grass Slope Threshold", Range(0,1)) = .5
        _GrassBlendAmount ("Grass Blend Amount", Range(0,1)) = .5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        struct Input {
            float3 worldPos;
            float3 worldNormal;
        };

        half _MaxHeight;
        half _GrassSlopeThreshold;
        half _GrassBlendAmount;
        fixed4 _GrassColour;
        fixed4 _RockColour;
        fixed4 _SnowColour;
        float _SnowHeight;
        float _SnowBlend;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 detailCol = lerp(_GrassColour, _SnowColour, saturate(IN.worldPos.y - _SnowHeight)/_SnowBlend);
            float slope = 1-IN.worldNormal.y; // slope = 0 when terrain is completely flat
            float grassBlendHeight = _GrassSlopeThreshold * (1-_GrassBlendAmount);
            float grassWeight = 1-saturate((slope-grassBlendHeight)/(_GrassSlopeThreshold-grassBlendHeight));
            o.Albedo = detailCol * grassWeight + _RockColour * (1-grassWeight);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
