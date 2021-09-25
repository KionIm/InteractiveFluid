// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/SimpleParticleRender"
{
	CGINCLUDE
#include "UnityCG.cginc"

		// �p�[�e�B�N���f�[�^�̍\����
	struct NSParticleData
	{
		float3 velocity;
		float3 position;
	};
	// VertexShader����GeometryShader�ɓn���f�[�^�̍\����
	struct v2g
	{
		float3 position : TEXCOORD0;
		float4 color    : COLOR;
	};
	// GeometryShader����FragmentShader�ɓn���f�[�^�̍\����
	struct g2f
	{
		float4 position : POSITION;
		float2 texcoord : TEXCOORD0;
		float4 color    : COLOR;
	};

	// �p�[�e�B�N���f�[�^
	StructuredBuffer<NSParticleData> _ParticleBuffer;
	// �p�[�e�B�N���̃e�N�X�`��
	sampler2D _MainTex;
	float4    _MainTex_ST;
	// �p�[�e�B�N���T�C�Y
	float     _ParticleSize;
	// �t�r���[�s��
	float4x4  _InvViewMatrix;

	float time;
	float Depth;

	// Quad�v���[���̍��W
	static const float3 g_positions[4] =
	{
		float3(-1, 1, 0),
		float3(1, 1, 0),
		float3(-1,-1, 0),
		float3(1,-1, 0),
	};
	// Quad�v���[����UV���W
	static const float2 g_texcoords[4] =
	{
		float2(0, 0),
		float2(1, 0),
		float2(0, 1),
		float2(1, 1),
	};

	// --------------------------------------------------------------------
	// Vertex Shader
	// --------------------------------------------------------------------
	v2g vert(uint id : SV_VertexID) // SV_VertexID:���_���Ƃ̎��ʎq
	{
		v2g o = (v2g)0;
		// �p�[�e�B�N���̈ʒu
		o.position = _ParticleBuffer[id].position;
		// �p�[�e�B�N���̑��x��F�ɔ��f
		float3 ColVel = 0.5 + 0.5 * normalize(_ParticleBuffer[id].velocity);
		float3 ColVel2;
		
		ColVel2.x = (0.2*sin(1*time+1) + 0.3)*ColVel.x + (0.2*sin(0.6*time+1) + 0.2-10*Depth)*ColVel.y;
		ColVel2.y = (0.2*sin(0.8*time+2) + 0.7)*ColVel.x + (0.2*sin(0.5*time+2) + 0.8-20*Depth)*ColVel.y;
		ColVel2.z = (0.1*sin(0.6*time) + 0.85)*ColVel.x + (0.1*sin(0.6*time) + 1.2-10*Depth)*ColVel.y;

		o.color = float4(ColVel2, 1.0);
		return o;
	}
	// --------------------------------------------------------------------
// Geometry Shader
// --------------------------------------------------------------------
	[maxvertexcount(4)]
	void geom(point v2g In[1], inout TriangleStream<g2f> SpriteStream)
	{
		g2f o = (g2f)0;
		[unroll]
		for (int i = 0; i < 4; i++)
		{
			float3 position = g_positions[i] * _ParticleSize;
			position = mul(_InvViewMatrix, position) + In[0].position;
			o.position = UnityObjectToClipPos(float4(position, 1.0));

			o.color = In[0].color;
			o.texcoord = g_texcoords[i];
			// ���_�ǉ�
			SpriteStream.Append(o);
		}
		// �X�g���b�v�����
		SpriteStream.RestartStrip();
	}

	// --------------------------------------------------------------------
	// Fragment Shader
	// --------------------------------------------------------------------
	fixed4 frag(g2f i) : SV_Target
	{
		return tex2D(_MainTex, i.texcoord.xy) * i.color;
	}
		ENDCG

		SubShader
	{
		Tags{ "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			LOD 100

			ZWrite Off
			//Blend SrcAlpha OneMinusSrcAlpha
			Blend One One

			Pass
		{
			CGPROGRAM
			#pragma target   5.0
			#pragma vertex   vert
			#pragma geometry geom
			#pragma fragment frag
			ENDCG
		}
	}
}