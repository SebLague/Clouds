Shader "Unlit/Phase"
{
    Properties
    {
        _HG1 ("g1",Range(-1,1))=0
        _HG2 ("g2",Range(-1,1))=0
        _A ("a",Range(0,1))=0
        _HGtoSchlick ("hgToSchlick",Range(0,1))=0
        _BaseBrightness ("BaseBrightness",Range(0,1))=0
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _HG1;
            float _HG2;
            float _A;
            float _HGtoSchlick;
            float _BaseBrightness;
            
            float hg(float a, float g) {
                float g2 = g*g;
                return (1-g2) / (4*3.1415*pow(1+g2-2*g*cos(a), 1.5));
            }

            //https://cs.dartmouth.edu/~wjarosz/publications/dissertation/chapter4.pdf
            float Schlick(float a, float g)
            {
                float k = 1.55*g-.55*g*g*g;
                return (1.0 - k * k) / (4.0 * 3.1415 * pow(1.0 - k * cos(a), 2.0));
            }

            float remap(float v, float minOld, float maxOld, float minNew, float maxNew) {
                return minNew + (v-minOld) * (maxNew - minNew) / (maxOld-minOld);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = .5 - i.uv;
                float pi2 = 2* 3.1415;
                float a = ((atan2(uv.y, uv.x) + pi2) % pi2);
                // pg 34
                // https://www.slideshare.net/DICEStudio/physically-based-sky-atmosphere-and-cloud-rendering-in-frostbite
                float hgPhase = hg(a, _HG1) * (1-_A) + hg (a,_HG2)*_A;
                float schlickPhase = Schlick(a, _HG1) * (1-_A) + Schlick (a,_HG2)*_A;
                float phase = hgPhase * (1-_HGtoSchlick) + schlickPhase * _HGtoSchlick;
                
                //phase = (_BaseBrightness + phase)/(1+_BaseBrightness);
                //phase = saturate (phase + _BaseBrightness);
                phase = remap(phase,0,1,_BaseBrightness,1);
                //phase = hg(a, _HG1);
                return phase;
                
                // h\left(g,x\right)\ =\frac{\left(1-g\cdot g\right)}{4\cdot\pi\left(1+g\cdot g-2g\cos\left(x\right)\right)^{1.5}}\left\{0<x<\pi\right\}
                // h\left(g_1,x\right)\ \cdot\left(1-a\right)\ +h\left(g_2,x\right)\cdot a
            }
            ENDCG
        }
    }
}
