using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

// �p�[�e�B�N���f�[�^�̍\����
public struct ParticleData
{
    public Vector3 Velocity; // ���x
    public Vector3 Position; // �ʒu

}

public class ParticleEmitter : MonoBehaviour
{
    const int NUM_PARTICLES = 32768; // ��������p�[�e�B�N���̐�

    const int NUM_THREAD_X = 8; // �X���b�h�O���[�v��X�����̃X���b�h��
    const int NUM_THREAD_Y = 1; // �X���b�h�O���[�v��Y�����̃X���b�h��
    const int NUM_THREAD_Z = 1; // �X���b�h�O���[�v��Z�����̃X���b�h��

    public ComputeShader SimpleParticleComputeShader; // �p�[�e�B�N���̓������v�Z����R���s���[�g�V�F�[�_
    public Shader SimpleParticleRenderShader;  // �p�[�e�B�N���������_�����O����V�F�[�_

    public Vector3 Gravity = new Vector3(0.0f, -1.0f, 0.0f); // �d��
    public Vector3 AreaSize = Vector3.one * 10.0f;            // �p�[�e�B�N�������݂���G���A�̃T�C�Y

    public Texture2D ParticleTex;          // �p�[�e�B�N���̃e�N�X�`��
    public float ParticleSize = 0.05f; // �p�[�e�B�N���̃T�C�Y

    public Camera RenderCam; // �p�[�e�B�N���������_�����O����J�����i�r���{�[�h�̂��߂̋t�r���[�s��v�Z�Ɏg�p�j

    ComputeBuffer particleBuffer;     // �p�[�e�B�N���̃f�[�^���i�[����R���s���[�g�o�b�t�@ 
    Material particleRenderMat;  // �p�[�e�B�N���������_�����O����}�e���A��
    void Start()
    {
        // �p�[�e�B�N���̃R���s���[�g�o�b�t�@���쐬
        particleBuffer = new ComputeBuffer(NUM_PARTICLES, Marshal.SizeOf(typeof(ParticleData)));
        // �p�[�e�B�N���̏����l��ݒ�
        var pData = new ParticleData[NUM_PARTICLES];
        for (int i = 0; i < pData.Length; i++)
        {
            pData[i].Velocity = Random.insideUnitSphere;
            pData[i].Position = Random.insideUnitSphere;
        }
        // �R���s���[�g�o�b�t�@�ɏ����l�f�[�^���Z�b�g
        particleBuffer.SetData(pData);

        pData = null;

        // �p�[�e�B�N���������_�����O����}�e���A�����쐬
        particleRenderMat = new Material(SimpleParticleRenderShader);
        particleRenderMat.hideFlags = HideFlags.HideAndDontSave;

    }

    void OnRenderObject()
    {
        ComputeShader cs = SimpleParticleComputeShader;
        // �X���b�h�O���[�v�����v�Z
        int numThreadGroup = NUM_PARTICLES / NUM_THREAD_X;
        // �J�[�l��ID���擾
        int kernelId = cs.FindKernel("CSMain");
        // �e�p�����[�^���Z�b�g
        cs.SetFloat("_TimeStep", Time.deltaTime);
        cs.SetVector("_Gravity", Gravity);
        cs.SetFloats("_AreaSize", new float[3] { AreaSize.x, AreaSize.y, AreaSize.z });
        // �R���s���[�g�o�b�t�@���Z�b�g
        cs.SetBuffer(kernelId, "_ParticleBuffer", particleBuffer);
        // �R���s���[�g�V�F�[�_�����s
        cs.Dispatch(kernelId, numThreadGroup, 1, 1);

        var pDataPar = new ParticleData[NUM_PARTICLES];
        particleBuffer.GetData(pDataPar);
        Debug.Log(pDataPar[100].Position);

        // �t�r���[�s����v�Z
        var inverseViewMatrix = RenderCam.worldToCameraMatrix.inverse;

        Material m = particleRenderMat;
        m.SetPass(0); // �����_�����O�̂��߂̃V�F�[�_�p�X���Z�b�g
        // �e�p�����[�^���Z�b�g
        m.SetMatrix("_InvViewMatrix", inverseViewMatrix);
        m.SetTexture("_MainTex", ParticleTex);
        m.SetFloat("_ParticleSize", ParticleSize);
        // �R���s���[�g�o�b�t�@���Z�b�g
        m.SetBuffer("_ParticleBuffer", particleBuffer);
        // �p�[�e�B�N���������_�����O
        Graphics.DrawProceduralNow(MeshTopology.Points, NUM_PARTICLES);
    }

    void OnDestroy()
    {
        if (particleBuffer != null)
        {
            // �o�b�t�@�������[�X�i�Y�ꂸ�ɁI�j
            particleBuffer.Release();
        }

        if (particleRenderMat != null)
        {
            // �����_�����O�̂��߂̃}�e���A�����폜
            DestroyImmediate(particleRenderMat);
        }
    }
}

