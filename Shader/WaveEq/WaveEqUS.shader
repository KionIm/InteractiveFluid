Shader "Unlit/WaveEqUS"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        Blend One One

        //Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            struct WaveEqData
            {
                float Cm;
                float Cc;
                float Cw;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int NumWidth;
            int NumHeight;
            float timem;

            StructuredBuffer<WaveEqData> _WaveEqR;
            StructuredBuffer<WaveEqData> _WaveEqG;
            StructuredBuffer<WaveEqData> _WaveEqB;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 p = i.uv.xy;
                int ix = int(p.x * NumWidth); 
                int iy = int(p.y * NumHeight); 
                float c1 = _WaveEqR[ix*NumHeight + iy].Cc; 
                float c2 = _WaveEqG[ix*NumHeight + iy].Cc; 
                float c3 = _WaveEqB[ix*NumHeight + iy].Cc; 
                //fixed4 col = float4(0.0,0.0,log(abs(c))/10.0,1.0); 

                float cr = c1*(0.2*sin(timem)+0.2) 
                         + c2*(0.1*sin(timem)+0.3) 
                         + c3*(0.2*sin(timem)+0.2) ; 
                float cg = c1*(0.2*sin(2*timem+0.2)+0.4)
                         + c2*(0.2*sin(2*timem+0.2)+0.3) 
                         + c3*(0.2*sin(2*timem+0.2)+0.4); 
                float cb = c1*(0.2*sin(1.5*timem+0.2)+0.8)
                         + c2*(0.2*sin(1.5*timem+0.2)+0.7)
                         + c3*(0.2*sin(1.5*timem+0.2)+0.8); 

                fixed4 col = float4(cr,cg,cb,1.0); 

                return col; 
            }
            ENDCG 
        }
    }
}
