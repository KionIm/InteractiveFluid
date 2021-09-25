using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public struct NSEqData
{
    public float P;
    public float U;
    public float V;
    public float B;
};

public struct NSEqPData
{
    public float P;
};

//-------------------------
//public struct ParticleData
//{
//    public Vector3 Velocity; // 速度
//    public Vector3 Position; // 位置

//}
//---------------------------------

public class NSEq : MonoBehaviour
{
    const int NUM_THREAD_X = 1000; 
    const int NUM_THREAD_Y = 1;
    const int NUM_THREAD_Z = 1;

    public ComputeShader NSEqBCs;
    public ComputeShader NSEqPCs;
    public ComputeShader NSEqUVCs;

    //-----------------------------------------
    //public ComputeShader SimpleParticleComputeShader; // パーティクルの動きを計算するコンピュートシェーダ
    //public Shader SimpleParticleRenderShader;
    //public Texture2D ParticleTex;          // パーティクルのテクスチャ
    //public float ParticleSize = 0.05f; // パーティクルのサイズ// パーティクルをレンダリングするシェーダ
    //-------------------------------------------

    public int _NumWidth;
    public int _NumHeight;
    public int IniNumW;
    public int IniNumH;
    public float dx;
    public float dy;
    public float dt;
    public int LPF;
    public float sharpness;
    public float rho;
    public float nu;
    public int PoissoN;
    public Camera RenderCam;
    public float Attenuation;

    private float IniU;

    ComputeBuffer[] NSEqBuffer;
    ComputeBuffer[] NSEqPBuffer;

    //-----------------------------------------
    //ComputeBuffer particleBuffer;     // パーティクルのデータを格納するコンピュートバッファ 
    //Material particleRenderMat;
    //------------------------------------------
    // Start is called before the first frame update
    void Start()
    {
        IniU = 0.5f;
        
        Application.targetFrameRate = 30;
        NSEqBuffer = new ComputeBuffer[2];
        NSEqBuffer[0] = new ComputeBuffer(_NumWidth * _NumHeight, Marshal.SizeOf(typeof(NSEqData)));
        NSEqBuffer[1] = new ComputeBuffer(_NumWidth * _NumHeight, Marshal.SizeOf(typeof(NSEqData)));
        var pData = new NSEqData[_NumWidth * _NumHeight];
        for (int i = 0; i < pData.Length; i++)
        {
            int ix = i / _NumHeight;
            int iy = i % _NumHeight;
            pData[i].U = IniU;
            pData[i].V = 0.0f;
            pData[i].B = 0.0f;
            float Value = - sharpness * (Mathf.Pow((float)(ix - _NumWidth / 2) / (_NumWidth / 2), 2)
                          + Mathf.Pow((float)(iy - _NumHeight / 2) / (_NumHeight / 2), 2));
            pData[i].P = Mathf.Exp(Value);
            pData[i].P = 0.0f;
        }
        NSEqBuffer[0].SetData(pData);
        NSEqBuffer[1].SetData(pData);

        NSEqPBuffer = new ComputeBuffer[2];
        NSEqPBuffer[0] = new ComputeBuffer(_NumWidth * _NumHeight, Marshal.SizeOf(typeof(NSEqPData)));
        NSEqPBuffer[1] = new ComputeBuffer(_NumWidth * _NumHeight, Marshal.SizeOf(typeof(NSEqPData)));
        var pDataP = new NSEqPData[_NumWidth * _NumHeight];
        for (int i = 0; i < pData.Length; i++)
        {
            pDataP[i].P = pData[i].P;
        }
        NSEqPBuffer[0].SetData(pDataP);
        NSEqPBuffer[1].SetData(pDataP);

        //-----------------------------------------------

        //particleBuffer = new ComputeBuffer(IniNumH * IniNumW, Marshal.SizeOf(typeof(ParticleData)));
        //// パーティクルの初期値を設定
        //var pDataPar = new ParticleData[IniNumH * IniNumW];
        //for (int i = 0; i < pData.Length; i++)
        //{
        //    int ix = i / IniNumH;
        //    int iy = i % IniNumH;
        //    pDataPar[i].Velocity.x = IniU;
        //    pDataPar[i].Velocity.y = 0.0f;
        //    pDataPar[i].Position.x = ix;
        //    pDataPar[i].Position.y = iy;
        //}
        //// コンピュートバッファに初期値データをセット
        //particleBuffer.SetData(pData);

        //// パーティクルをレンダリングするマテリアルを作成
        //particleRenderMat = new Material(SimpleParticleRenderShader);
        //particleRenderMat.hideFlags = HideFlags.HideAndDontSave;
        //-------------------------------------------------
    }

    // Update is called once per frame
    void Update()
    {
        int numThreadGroup = _NumWidth * _NumHeight / NUM_THREAD_X;
        ComputeShader csB = NSEqBCs;
        int kernelIdB = csB.FindKernel("CSMain");
        ComputeShader csP = NSEqPCs;
        int kernelIdP = csP.FindKernel("CSMain");
        ComputeShader csUV = NSEqUVCs;
        int kernelIdUV = csUV.FindKernel("CSMain");
        //---------------------------------------------------
        //ComputeShader csPa = SimpleParticleComputeShader;
        //int kernelIdPa = csPa.FindKernel("CSMain1");
        //------------------------------------------------

        csB.SetInt("NumWidth", _NumWidth);
        csB.SetInt("NumHeight", _NumHeight);
        csB.SetFloat("dx", dx);
        csB.SetFloat("dy", dy);
        csB.SetFloat("dt", dt);
        csB.SetFloat("rho", rho);

        csP.SetInt("NumWidth", _NumWidth);
        csP.SetInt("NumHeight", _NumHeight);
        csP.SetFloat("dx", dx);
        csP.SetFloat("dy", dy);

        csUV.SetInt("NumWidth", _NumWidth);
        csUV.SetInt("NumHeight", _NumHeight);
        csUV.SetFloat("dx", dx);
        csUV.SetFloat("dy", dy);
        csUV.SetFloat("dt", dt);
        csUV.SetFloat("rho", rho);
        csUV.SetFloat("nu", nu);

        Vector3 mousePos = Input.mousePosition;
        Vector2 mousePosuv;
        mousePosuv.x = mousePos.x / Screen.width;
        mousePosuv.y = mousePos.y / Screen.height;

        for (int i = 0; i < LPF; i++)
        {
            csB.SetBuffer(kernelIdB, "_BufferRead", NSEqBuffer[0]);
            csB.SetBuffer(kernelIdB, "_BufferWrite", NSEqBuffer[1]);
            csB.Dispatch(kernelIdB, numThreadGroup, 1, 1);

            csP.SetBuffer(kernelIdP, "_BufferRead", NSEqBuffer[0]);
            csP.SetBuffer(kernelIdP, "_BufferWrite", NSEqBuffer[1]);

            for (int j=0; j < PoissoN; j++)
            {
                csP.SetBuffer(kernelIdP, "_BufferPRead", NSEqPBuffer[0]);
                csP.SetBuffer(kernelIdP, "_BufferPWrite", NSEqPBuffer[1]);
                csP.Dispatch(kernelIdP, numThreadGroup, 1, 1);
                (NSEqPBuffer[0], NSEqPBuffer[1]) = (NSEqPBuffer[1], NSEqPBuffer[0]);
            }
            var pData = new NSEqData[_NumWidth * _NumHeight];
            var pDataP = new NSEqPData[_NumWidth * _NumHeight];
            NSEqBuffer[1].GetData(pData);
            NSEqPBuffer[0].GetData(pDataP);
            for (int j = 0; j < pDataP.Length; j++)
            {
                int ix = j / _NumHeight;
                int iy = j % _NumHeight;
                float Value = -sharpness * (Mathf.Pow((float)(ix - mousePosuv.x* _NumWidth) / (_NumWidth / 2), 2)
                          + Mathf.Pow((float)(iy - mousePosuv.y* _NumHeight) / (_NumHeight / 2), 2));
                pData[j].P = Mathf.Max(pDataP[j].P,Mathf.Exp(Value));

                if (pData[j].P > 1.0f)
                {
                    pData[j].P = 1.0f;
                }
                if (pData[j].P < 0.0f)
                {
                    pData[j].P = 0.0f;
                }

                float AttenuationP = -Attenuation * Mathf.Cos(0.5f*3.1415f * pData[j].P) + Attenuation + 1.0f;
                pData[j].P /= AttenuationP;
            }
            NSEqBuffer[1].SetData(pData);

            csUV.SetBuffer(kernelIdUV, "_BufferRead", NSEqBuffer[0]);
            csUV.SetBuffer(kernelIdUV, "_BufferWrite", NSEqBuffer[1]);
            csUV.Dispatch(kernelIdUV, numThreadGroup, 1, 1);

            (NSEqBuffer[0], NSEqBuffer[1]) = (NSEqBuffer[1], NSEqBuffer[0]);

        }

        //-------------------------------

        // 各パラメータをセット
        //csPa.SetFloat("_TimeStep", Time.deltaTime);

        //// コンピュートバッファをセット
        //csPa.SetBuffer(kernelIdPa, "_ParticleBuffer", particleBuffer);
        //csPa.SetBuffer(kernelIdPa, "_NSEqBuffer", NSEqBuffer[0]);
        //// コンピュートシェーダを実行
        //csPa.Dispatch(kernelIdPa, numThreadGroup, 1, 1);

        ////--------------------------------------
        //// 逆ビュー行列を計算
        //var inverseViewMatrix = RenderCam.worldToCameraMatrix.inverse;

        //Material mPa = particleRenderMat;
        //mPa.SetPass(0); // レンダリングのためのシェーダパスをセット
        //// 各パラメータをセット
        //mPa.SetMatrix("_InvViewMatrix", inverseViewMatrix);
        //mPa.SetTexture("_MainTex", ParticleTex);
        //mPa.SetFloat("_ParticleSize", ParticleSize);
        //// コンピュートバッファをセット
        //mPa.SetBuffer("_ParticleBuffer", particleBuffer);
        //// パーティクルをレンダリング
        //Graphics.DrawProceduralNow(MeshTopology.Points, IniNumH * IniNumW);

        //-------------------------------------------
        Material m = GetComponent<MeshRenderer>().material;
        m.SetBuffer("_NSEqBuffer", NSEqBuffer[1]);
        m.SetInt("NumWidth", _NumWidth);
        m.SetInt("NumHeight", _NumHeight);

    }
}
