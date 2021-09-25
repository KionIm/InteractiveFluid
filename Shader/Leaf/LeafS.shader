Shader "Unlit/LeafS"
{
    Properties
    {
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
            // make fog work
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

            struct NSParticleData
            {
            	float3 velocity; 
            	float3 position; 
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int idxParticle;
            float Scale;


            StructuredBuffer<NSParticleData> particleBuffer;

            v2f vert (appdata v)
            {
                v2f o;
                v.vertex.xyz *= Scale;
                v.vertex.yz = float2(v.vertex.z, -v.vertex.y);
                float angle = idxParticle + _Time.y;//idxParticle;// + _Time.y;
                float ctheta = cos(angle);
                float stheta = sin(angle);
                v.vertex.xy = float2(ctheta * v.vertex.x - stheta * v.vertex.y,
                                     stheta * v.vertex.x + ctheta * v.vertex.y);
                //v.vertex.z += sin(_Time.y*100.0)*0.1;
                v.vertex.xyz += particleBuffer[idxParticle].position.xyz;
                v.vertex.z -= 0.01;
                //v.vertex.x += idxParticle * 0.1;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                //col.xyz = abs(particleBuffer[idxParticle].position.xyz)*10000.0;
                //col.x += float(idxParticle * 0.1);

                return col;
            }
            ENDCG
        }
    }
}
