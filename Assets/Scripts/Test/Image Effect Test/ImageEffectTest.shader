Shader "Hidden/ImageEffectTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

             // vertex input: position, UV
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
            };
            
            v2f vert (appdata v) {
                v2f output;
                output.pos = UnityObjectToClipPos(v.vertex);
                output.uv = v.uv;
                // Camera space matches OpenGL convention where cam forward is -z. In unity forward is positive z.
                // (https://docs.unity3d.com/ScriptReference/Camera-cameraToWorldMatrix.html)
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
                output.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                return output;
            }


             // Returns (dstToBox, dstInsideBox). If ray misses box, dstInsideBox will be zero)
            float2 rayBoxDst(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 rayDir) {
                // From http://jcgt.org/published/0007/03/04/
                // via https://medium.com/@bromanz/another-view-on-the-classic-ray-aabb-intersection-algorithm-for-bvh-traversal-41125138b525
                float3 t0 = (boundsMin - rayOrigin) / rayDir;
                float3 t1 = (boundsMax - rayOrigin) / rayDir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);
                
                float dstA = max(max(tmin.x, tmin.y), tmin.z);
                float dstB = min(tmax.x, min(tmax.y, tmax.z));

                // CASE 1: ray intersects box from outside (0 <= dstA <= dstB)
                // dstA is dst to nearest intersection, dstB dst to far intersection

                // CASE 2: ray intersects box from inside (dstA < 0 < dstB)
                // dstA is the dst to intersection behind the ray, dstB is dst to forward intersection

                // CASE 3: ray misses box (dstA > dstB)

                float dstToBox = max(0, dstA);
                float dstInsideBox = max(0, dstB - dstToBox);
                return float2(dstToBox, dstInsideBox);
            }

            float4 params;


            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            Texture3D<float4> ShapeNoise;
            Texture3D<float4> DetailNoise;
            SamplerState samplerShapeNoise;
            SamplerState samplerDetailNoise;

            float3 BoundsMin;
            float3 BoundsMax;
            float3 CloudOffset;
            float CloudScale;
            float DensityThreshold;
            float DensityMultiplier;
            int NumSteps;

            float sampleDensity(float3 position) {
                float3 uvw = position * CloudScale * 0.001 + CloudOffset * 0.01;
                float4 shape = ShapeNoise.SampleLevel(samplerShapeNoise, uvw, 0);
                float density = max(0, shape.r - DensityThreshold) * DensityMultiplier;
                return density;
            }
        
            float4 frag (v2f input) : SV_Target
            {
                float4 col = tex2D(_MainTex, input.uv);
                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(input.viewVector);
                
                float nonLinearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, input.uv);
                float depth = LinearEyeDepth(nonLinearDepth) * length(input.viewVector);
                
                return Linear01Depth(nonLinearDepth) * NumSteps;
                /*

                float2 rayBoxInfo = rayBoxDst(BoundsMin, BoundsMax, rayOrigin, rayDir);
                float dstToBox = rayBoxInfo.x;
                float dstInsideBox = rayBoxInfo.y;

                float dstTravelled = 0;
                float stepSize = dstInsideBox / NumSteps;
                float dstLimit = min(depth - dstToBox, dstInsideBox);

                // March through volume:
                float totalDensity = 0;
                while (dstTravelled < dstLimit) {
                    float3 rayPos = rayOrigin + rayDir * (dstToBox + dstTravelled);
                    totalDensity += sampleDensity(rayPos) * stepSize;
                    dstTravelled += stepSize;
                }
                float transmittance = exp(-totalDensity);
                return col * transmittance + (1-transmittance);
                */
            }


            ENDCG
        }
    }
}
