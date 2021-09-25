using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public struct NSEqPaData
{
    public float P;
    public float U;
    public float V;
    public float B;
};

public struct NSEqPPaData
{
    public float P;
};

//-------------------------
public struct NSParticleData
{
    public Vector3 Velocity; // 速度
    public Vector3 Position; // 位置

}
//---------------------------------

public class NSEqPa : MonoBehaviour
{
    const int NUM_THREAD_X = 1000;
    const int NUM_THREAD_Y = 1;
    const int NUM_THREAD_Z = 1;

    public ComputeShader NSEqBCs;
    public ComputeShader NSEqPCs;
    public ComputeShader NSEqPAjCs;
    public ComputeShader NSEqUVCs;
    
    //-----------------------------------------
    public ComputeShader SimpleParticleComputeShader; // パーティクルの動きを計算するコンピュートシェーダ
    public Shader SimpleParticleRenderShader;
    public Texture2D ParticleTex;          // パーティクルのテクスチャ
    public float ParticleSize = 0.05f; // パーティクルのサイズ// パーティクルをレンダリングするシェーダ
    public Texture2D PressAsset;
    //-------------------------------------------

    public OscReceiverImai OscReceiverSc;

    //---------------------------------------

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
    public int TrailLength;
    public int DipictStep;
    public float Aspect;
    public float Depth;
    public float NoiseIntensity;
    public float NoiseAdjust;
    public float speed;
    
    private float IniU;
    public float IniV;
    private int count;
    private int CountDipict;

    ComputeBuffer[] NSEqBuffer;
    ComputeBuffer[] NSEqPBuffer;
    ComputeBuffer[] PressAssetBuffer;

    //-----------------------------------------
    [HideInInspector] public ComputeBuffer[] particleBuffer;     // パーティクルのデータを格納するコンピュートバッファ 
    ComputeBuffer[] ParticleTrailBuffer;
    ComputeBuffer[] ParticleRenderBuffer;
    Material particleRenderMat;
    //------------------------------------------

    // Start is called before the first frame update
    void Awake()
    {
        IniU = 0.0f;//0.05f;
        //IniV = 0.2f;
        count = 0;
        CountDipict = 0;

        Application.targetFrameRate = 30;
        NSEqBuffer = new ComputeBuffer[2];
        NSEqBuffer[0] = new ComputeBuffer(_NumWidth * _NumHeight, Marshal.SizeOf(typeof(NSEqPaData)));
        NSEqBuffer[1] = new ComputeBuffer(_NumWidth * _NumHeight, Marshal.SizeOf(typeof(NSEqPaData)));
        var pData = new NSEqPaData[_NumWidth * _NumHeight];
        for (int i = 0; i < pData.Length; i++)
        {
            int ix = i / _NumHeight;
            int iy = i % _NumHeight;
            pData[i].U = IniU;
            pData[i].V = IniV;
            pData[i].B = 0.0f;
            float Value = -sharpness * (Mathf.Pow((float)(ix - _NumWidth / 2) / (_NumWidth / 2), 2)
                          + Mathf.Pow((float)(iy - _NumHeight / 2) / (_NumHeight / 2), 2));
            pData[i].P = Mathf.Exp(Value);
            pData[i].P = 0.0f;
        }
        NSEqBuffer[0].SetData(pData);
        NSEqBuffer[1].SetData(pData);
        //---------------------------------------------
        NSEqPBuffer = new ComputeBuffer[2];
        NSEqPBuffer[0] = new ComputeBuffer(_NumWidth * _NumHeight, Marshal.SizeOf(typeof(NSEqPPaData)));
        NSEqPBuffer[1] = new ComputeBuffer(_NumWidth * _NumHeight, Marshal.SizeOf(typeof(NSEqPPaData)));
        var pDataP = new NSEqPPaData[_NumWidth * _NumHeight];
        for (int i = 0; i < pData.Length; i++)
        {
            pDataP[i].P = pData[i].P;
        }
        NSEqPBuffer[0].SetData(pDataP);
        NSEqPBuffer[1].SetData(pDataP);
        //-----------------------------------------------
        particleBuffer = new ComputeBuffer[1];
        ParticleTrailBuffer = new ComputeBuffer[1];
        particleBuffer[0] = new ComputeBuffer(IniNumH * IniNumW, 
                                  Marshal.SizeOf(typeof(NSParticleData)));
        ParticleTrailBuffer[0] =
            new ComputeBuffer(TrailLength*IniNumH * IniNumW, Marshal.SizeOf(typeof(NSParticleData)));
        
        ParticleRenderBuffer = new ComputeBuffer[1];
        ParticleRenderBuffer[0] = new ComputeBuffer(TrailLength * IniNumH *IniNumW / DipictStep,
                                  Marshal.SizeOf(typeof(NSParticleData)));
        // パーティクルの初期値を設定
        var pDataPar = new NSParticleData[IniNumH * IniNumW];
        var pDataParTrail = new NSParticleData[TrailLength*IniNumH * IniNumW];
        var pDataParTrailRender = new NSParticleData[TrailLength * IniNumH * IniNumW/DipictStep];
        for (int i = 0; i < pDataPar.Length; i++)
        {
            float ix = i / IniNumH;
            float iy = i % IniNumH;
            pDataPar[i].Velocity.x = IniU;
            pDataPar[i].Velocity.y = IniV;
            pDataPar[i].Velocity.z = 0.0f;
            pDataPar[i].Position.x = (ix / IniNumW - 0.5f)*Aspect;
            pDataPar[i].Position.y = iy / IniNumH - 0.5f;
            pDataPar[i].Position.z = Depth;
            for (int j=0;j < TrailLength;j++)
            {
                pDataParTrail[j * IniNumH * IniNumW + i].Velocity.x = IniU;
                pDataParTrail[j * IniNumH * IniNumW + i].Velocity.y = IniV;
                pDataParTrail[j * IniNumH * IniNumW + i].Velocity.z = 0.0f;
                pDataParTrail[j * IniNumH * IniNumW + i].Position.x = (ix / IniNumW - 0.5f)*Aspect;
                pDataParTrail[j * IniNumH * IniNumW + i].Position.y = iy / IniNumH - 0.5f;
                pDataParTrail[j * IniNumH * IniNumW + i].Position.z = Depth;
            }
            for (int j = 0; j < TrailLength / DipictStep; j++)
            {
                pDataParTrailRender[j * IniNumH * IniNumW + i].Velocity.x = IniU;
                pDataParTrailRender[j * IniNumH * IniNumW + i].Velocity.y = IniV;
                pDataParTrailRender[j * IniNumH * IniNumW + i].Velocity.z = 0.0f;
                pDataParTrailRender[j * IniNumH * IniNumW + i].Position.x = (ix / IniNumW - 0.5f) * Aspect;
                pDataParTrailRender[j * IniNumH * IniNumW + i].Position.y = iy / IniNumH - 0.5f;
                pDataParTrailRender[j * IniNumH * IniNumW + i].Position.z = Depth;
            }
        }
        // コンピュートバッファに初期値データをセット
        particleBuffer[0].SetData(pDataPar);
        ParticleTrailBuffer[0].SetData(pDataParTrail);
        ParticleRenderBuffer[0].SetData(pDataParTrailRender);
        //---------------------------------------------------
        PressAssetBuffer = new ComputeBuffer[1];
        PressAssetBuffer[0] = new ComputeBuffer(_NumWidth * _NumHeight, 
                                  Marshal.SizeOf(typeof(NSEqPPaData)));
        var pDataPAsset = new NSEqPPaData[_NumWidth * _NumHeight];
        for (int i = 0; i < pData.Length; i++)
        {
            int ix = i / _NumHeight;
            int iy = i % _NumHeight;
            pDataPAsset[i].P = PressAsset.GetPixel(ix, iy).grayscale;
        }
        PressAssetBuffer[0].SetData(pDataPAsset);
        //---------------------------------------------------
        
        // パーティクルをレンダリングするマテリアルを作成
        particleRenderMat = new Material(SimpleParticleRenderShader);
        particleRenderMat.hideFlags = HideFlags.HideAndDontSave;
        //-------------------------------------------------
    }

    void OnRenderObject()
    {
        int numThreadGroup = _NumWidth * _NumHeight / NUM_THREAD_X;
        ComputeShader csB = NSEqBCs;
        int kernelIdB = csB.FindKernel("CSMain");
        ComputeShader csP = NSEqPCs;
        int kernelIdP = csP.FindKernel("CSMain");
        ComputeShader csPAj = NSEqPAjCs;
        int kernelIdPA = csPAj.FindKernel("CSMain");
        ComputeShader csUV = NSEqUVCs;
        int kernelIdUV = csUV.FindKernel("CSMain");
        //---------------------------------------------------
        ComputeShader csPa = SimpleParticleComputeShader;
        int kernelIdPa = csPa.FindKernel("CSMain");
        //------------------------------------------------

        Vector3 mousePos = Input.mousePosition;
        Vector2 mousePosuv;
        mousePosuv.x = OscReceiverSc.CurrentHuman.x;
        mousePosuv.y = OscReceiverSc.CurrentHuman.y;

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

        csPAj.SetInt("NumWidth", _NumWidth);
        csPAj.SetInt("NumHeight", _NumHeight);
        csPAj.SetFloat("dx", dx);
        csPAj.SetFloat("dy", dy);
        csPAj.SetFloat("mousePosu", mousePosuv.x);
        csPAj.SetFloat("mousePosv", mousePosuv.y);
        csPAj.SetFloat("Attenuation", Attenuation);
        csPAj.SetFloat("sharpness", sharpness);

        csUV.SetInt("NumWidth", _NumWidth);
        csUV.SetInt("NumHeight", _NumHeight);
        csUV.SetFloat("dx", dx);
        csUV.SetFloat("dy", dy);
        csUV.SetFloat("dt", dt);
        csUV.SetFloat("rho", rho);
        csUV.SetFloat("nu", nu);

        for (int i = 0; i < LPF; i++)
        {

    
            csB.SetBuffer(kernelIdB, "_BufferRead", NSEqBuffer[0]);
            csB.SetBuffer(kernelIdB, "_BufferWrite", NSEqBuffer[1]);
            csB.Dispatch(kernelIdB, numThreadGroup, 1, 1);

            csP.SetBuffer(kernelIdP, "_BufferRead", NSEqBuffer[0]);
            csP.SetBuffer(kernelIdP, "_BufferWrite", NSEqBuffer[1]);

            for (int j = 0; j < PoissoN; j++)
            {
                csP.SetBuffer(kernelIdP, "_BufferPRead", NSEqPBuffer[0]);
                csP.SetBuffer(kernelIdP, "_BufferPWrite", NSEqPBuffer[1]);
                csP.Dispatch(kernelIdP, numThreadGroup, 1, 1);
                (NSEqPBuffer[0], NSEqPBuffer[1]) = (NSEqPBuffer[1], NSEqPBuffer[0]);
            }

            csPAj.SetBuffer(kernelIdPA, "_BufferWrite", NSEqBuffer[1]);
            csPAj.SetBuffer(kernelIdPA, "_BufferPRead", NSEqPBuffer[0]);
            csPAj.SetBuffer(kernelIdPA, "_BufferPAssetRead", PressAssetBuffer[0]);
            csPAj.Dispatch(kernelIdPA, numThreadGroup, 1, 1);

            csUV.SetBuffer(kernelIdUV, "_BufferRead", NSEqBuffer[0]);
            csUV.SetBuffer(kernelIdUV, "_BufferWrite", NSEqBuffer[1]);
            csUV.Dispatch(kernelIdUV, numThreadGroup, 1, 1);

            (NSEqBuffer[0], NSEqBuffer[1]) = (NSEqBuffer[1], NSEqBuffer[0]);

        }

        //-------------------------------//

        // 各パラメータをセット
        csPa.SetFloat("_TimeStep", Time.deltaTime);
        csPa.SetBuffer(kernelIdPa, "_ParticleBuffer", particleBuffer[0]);
        csPa.SetBuffer(kernelIdPa, "_ParticleTrailBuffer", ParticleTrailBuffer[0]);
        csPa.SetBuffer(kernelIdPa, "_ParticleRenderBuffer", ParticleRenderBuffer[0]);
        csPa.SetBuffer(kernelIdPa, "_NSEqBufferO", NSEqBuffer[1]);
        csPa.SetBuffer(kernelIdPa, "_NSEqBufferN", NSEqBuffer[0]);
        csPa.SetInt("count", count);
        csPa.SetInt("CountDipict", CountDipict);
        csPa.SetInt("DipictStep",DipictStep);
        csPa.SetInt("NumP", IniNumH * IniNumW);
        csPa.SetInt("NumWidth", _NumWidth);
        csPa.SetInt("NumHeight", _NumHeight);
        csPa.SetInt("IniNumH", IniNumH);
        csPa.SetInt("TrailLength",TrailLength);
        csPa.SetFloat("Aspect", Aspect);
        csPa.SetFloat("NoiseIntensity", NoiseIntensity);
        csPa.SetFloat("NoiseAdjust",NoiseAdjust);
        csPa.SetFloat("time", Time.time);
        csPa.SetFloat("speed",speed);
        int numThreadGroup2 = IniNumH * IniNumW / 8;
        csPa.Dispatch(kernelIdPa, numThreadGroup2, 1, 1);
        // コンピュートシェーダを実行
        if (CountDipict == DipictStep-1)
        {
            var pDataParTrailRender = new NSParticleData[TrailLength * IniNumH * IniNumW / DipictStep];
            //ParticleRenderBuffer[0].GetData(pDataParTrailRender);
            //Debug.Log("a");
            //Debug.Log(pDataParTrailRender[1].Position.z);
        }

        count += 1;
        count %= TrailLength;

        CountDipict += 1;
        CountDipict %= DipictStep;

        //var inverseViewMatrix = RenderCam.worldToCameraMatrix.inverse;
        //Material mPa = particleRenderMat;
        //mPa.SetPass(0); // レンダリングのためのシェーダパスをセット
        //// 各パラメータをセット
        //mPa.SetMatrix("_InvViewMatrix", inverseViewMatrix);
        //mPa.SetTexture("_MainTex", ParticleTex);
        //mPa.SetFloat("_ParticleSize", ParticleSize);
        //// コンピュートバッファをセット
        //mPa.SetBuffer("_ParticleBuffer", particleBuffer[0]);
        //// パーティクルをレンダリング
        //Graphics.DrawProceduralNow(MeshTopology.Points, IniNumH * IniNumW);

        var inverseViewMatrix = RenderCam.worldToCameraMatrix.inverse;
        Material mPa = particleRenderMat;
        mPa.SetPass(0); // レンダリングのためのシェーダパスをセット
        // 各パラメータをセット
        mPa.SetMatrix("_InvViewMatrix", inverseViewMatrix);
        mPa.SetTexture("_MainTex", ParticleTex);
        mPa.SetFloat("_ParticleSize", ParticleSize);
        // コンピュートバッファをセット
        mPa.SetFloat("time", Time.time);
        mPa.SetFloat("Depth", Depth);
        //mPa.SetBuffer("_ParticleBuffer", ParticleTrailBuffer[0]);
        //Graphics.DrawProceduralNow(MeshTopology.Points, TrailLength * IniNumH * IniNumW);
        mPa.SetBuffer("_ParticleBuffer", ParticleRenderBuffer[0]);
        Graphics.DrawProceduralNow(MeshTopology.Points, TrailLength*IniNumH * IniNumW/DipictStep);

    }
    //void OnDestroy()
    //{
    //    if (particleBuffer != null)
    //    {
    //        // バッファをリリース（忘れずに！）
    //        particleBuffer.Release();
    //    }

    //    if (particleRenderMat != null)
    //    {
    //        // レンダリングのためのマテリアルを削除
    //        DestroyImmediate(particleRenderMat);
    //    }
    //}
}
