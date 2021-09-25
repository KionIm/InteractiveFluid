using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public struct DiffEqData
{
    public float Cr;
    public float Cw;
};

public class DiffEq : MonoBehaviour
{

    const int NUM_THREAD_X = 1000; // �X���b�h�O���[�v��X�����̃X���b�h��
    const int NUM_THREAD_Y = 1; // �X���b�h�O���[�v��Y�����̃X���b�h��
    const int NUM_THREAD_Z = 1; // �X���b�h�O���[�v��Z�����̃X���b�h��

    public WaveEq waveScript;
    public ComputeShader ComShader; // �p�[�e�B�N���̓������v�Z����R���s���[�g�V�F�[�_

    //public Shader TestQuad;  // �p�[�e�B�N���������_�����O����V�F�[�_

    public float _exkappa;
    public int _NumWidth;
    public int _NumHeight;
    public float sharpness;
    public float sharpnessT;
    public int LPF;
    public float Attenuation;
    private float time;
    public float timeP;
    private float timeexe;

    public OscReceiverImai OscReceiverSc;

    ComputeBuffer[] DiffEqBuffer;
    ComputeBuffer[] DiffEqTBuffer;
    // Start is called before the first frame update
    void Start()
    {
        
        Application.targetFrameRate = 30;

        timeexe = timeP;

        DiffEqBuffer = new ComputeBuffer[3];
        DiffEqBuffer[0] = new ComputeBuffer(_NumWidth*_NumHeight, Marshal.SizeOf(typeof(DiffEqData)));
        DiffEqBuffer[1] = new ComputeBuffer(_NumWidth * _NumHeight, Marshal.SizeOf(typeof(DiffEqData)));
        DiffEqBuffer[2] = new ComputeBuffer(_NumWidth * _NumHeight, Marshal.SizeOf(typeof(DiffEqData)));
        var pData = new DiffEqData[_NumWidth*_NumHeight];
        for (int i = 0; i < pData.Length; i++)
        {
            int ix = i / _NumHeight;
            int iy = i % _NumHeight;
            float Value = -sharpness*(Mathf.Pow((float)(ix - _NumWidth / 2) / (_NumWidth / 2), 2) + Mathf.Pow((float)(iy - _NumHeight / 2) / (_NumHeight/2), 2));
            //pData[i].C = Mathf.Exp(Value);
            pData[i].Cr = 0.0f;// Mathf.Exp(Value);
            pData[i].Cw = 0.0f;
        }
        // �R���s���[�g�o�b�t�@�ɏ����l�f�[�^���Z�b�g
        DiffEqBuffer[0].SetData(pData);
        DiffEqBuffer[1].SetData(pData);
        DiffEqBuffer[2].SetData(pData);
    }

    // Update is called once per frame
    void Update()
    {
        ComputeShader cs = ComShader;
        // �X���b�h�O���[�v�����v�Z
        int numThreadGroup = _NumWidth * _NumHeight / NUM_THREAD_X;
        // �J�[�l��ID���擾
        int kernelId = cs.FindKernel("CSMain");
        // �e�p�����[�^���Z�b�g
        cs.SetFloat("exkappa", _exkappa);
        cs.SetFloat("Attenuation", Attenuation);
        cs.SetInt("NumWidth", _NumWidth);
        cs.SetInt("NumHeight", _NumHeight);

        Vector3 mousePos = Input.mousePosition;
        Vector2 mousePosuv;
        mousePosuv.x = OscReceiverSc.CurrentHuman.x;
        mousePosuv.y = OscReceiverSc.CurrentHuman.y;

        float CoefTime = 0.0f;


        //if (Time.frameCount % 30 == 0) Debug.Log(mousePosuv.x + "," + mousePosuv.y);

        for (int i = 0; i < LPF; i++)
        {
            time = Time.time;
            // �R���s���[�g�o�b�t�@���Z�b�g
            cs.SetBuffer(kernelId, "_BufferR", DiffEqBuffer[0]);
            cs.SetBuffer(kernelId, "_BufferG", DiffEqBuffer[1]);
            cs.SetBuffer(kernelId, "_BufferB", DiffEqBuffer[2]);

            cs.SetFloat("sharpness", sharpness);
            cs.SetFloat("sharpnessT", sharpnessT);
            cs.SetFloat("mousePosu", mousePosuv.x);
            cs.SetFloat("mousePosv", mousePosuv.y);
            cs.SetFloat("time", time);

            if (Time.time - timeexe> timeP)
            {
                CoefTime = 1.0f;
                timeexe = Time.time;
            }
            cs.SetFloat("CoefTime", CoefTime);

            // �R���s���[�g�V�F�[�_�����s
            cs.Dispatch(kernelId, numThreadGroup, 1, 1);

            CoefTime = 0.0f;

            var pDataR = new DiffEqData[_NumWidth * _NumHeight];
            var pDataG = new DiffEqData[_NumWidth * _NumHeight];
            var pDataB = new DiffEqData[_NumWidth * _NumHeight];
            DiffEqBuffer[0].GetData(pDataR);
            DiffEqBuffer[1].GetData(pDataG);
            DiffEqBuffer[2].GetData(pDataB);
            for (int j = 0; j < pDataR.Length; j++)
            {
                pDataR[j].Cr = pDataR[j].Cw;
                pDataG[j].Cr = pDataG[j].Cw;
                pDataB[j].Cr = pDataB[j].Cw;
            }
            DiffEqBuffer[0].SetData(pDataR);
            DiffEqBuffer[1].SetData(pDataG);
            DiffEqBuffer[2].SetData(pDataB);

        }

        Material m = GetComponent<MeshRenderer>().material;
        m.SetBuffer("_DiffEqBufferR", DiffEqBuffer[0]);
        m.SetBuffer("_DiffEqBufferG", DiffEqBuffer[1]);
        m.SetBuffer("_DiffEqBufferB", DiffEqBuffer[2]);

        m.SetInt("NumWidth", _NumWidth);
        m.SetInt("NumHeight", _NumHeight);
        m.SetFloat("time", time);
    }
}
