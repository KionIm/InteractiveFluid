using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using PrefsGUI.Example;

public struct WaveEqData
{
    public float Cm;
    public float Cc;
    public float Cw;
};

public class WaveEq : MonoBehaviour
{
    const int NUM_THREAD_X = 1000; // スレッドグループのX成分のスレッド数
    const int NUM_THREAD_Y = 1; // スレッドグループのY成分のスレッド数
    const int NUM_THREAD_Z = 1; // スレッドグループのZ成分のスレッド数

    public ComputeShader WaveEqCs;
    //public PrefsChild0

    public int _NumWidth;
    public int _NumHeight;
    public float sharpness;
    public int LPF;
    public float Attenuation;
    public float coef;
    public float ThresholdSpeed;
    public float ThresholdTimeS;
    public float ThresholdTimeC;
    private float timeexe1; //= 0.0f;
    private float timeexe2;// = ThresholdTimeC/3.0f;
    private float timeexe3;// = 0.4f;

    public OscReceiverImai OscReceiverSc;

    [HideInInspector] public ComputeBuffer[] WaveEqBuffer;

    // Start is called before the first frame update
    void Start()
    {

        timeexe1 = 0.0f;
        timeexe2 = ThresholdTimeC / 3.0f;
        timeexe3 = 2.0f*ThresholdTimeC / 3.0f;
        WaveEqBuffer = new ComputeBuffer[9];
        WaveEqBuffer[0] = new ComputeBuffer(_NumWidth * _NumHeight, Marshal.SizeOf(typeof(WaveEqData)));
        WaveEqBuffer[1] = new ComputeBuffer(_NumWidth * _NumHeight, Marshal.SizeOf(typeof(WaveEqData)));
        WaveEqBuffer[2] = new ComputeBuffer(_NumWidth * _NumHeight, Marshal.SizeOf(typeof(WaveEqData)));
        var pData = new WaveEqData[_NumWidth * _NumHeight];
        for (int i = 0; i < pData.Length; i++)
        {
            int ix = i / _NumHeight;
            int iy = i % _NumHeight;
            float Value = -sharpness * (Mathf.Pow((float)(ix - _NumWidth / 2) / (_NumWidth / 2), 2) + Mathf.Pow((float)(iy - _NumHeight / 2) / (_NumHeight / 2), 2));
            pData[i].Cm = Mathf.Exp(Value);
            pData[i].Cm = 0.0f;// Mathf.Exp(Value);
            pData[i].Cc = 0.0f;
            pData[i].Cw = 0.0f;
        }
        WaveEqBuffer[0].SetData(pData);
        WaveEqBuffer[1].SetData(pData);
        WaveEqBuffer[2].SetData(pData);

    }

    // Update is called once per frame
    void Update()
    {
        ThresholdSpeed = PrefsChild0.thresholdSpeed;

        ComputeShader cs = WaveEqCs;
        int numThreadGroup = _NumWidth * _NumHeight / NUM_THREAD_X;
        int kernelId = cs.FindKernel("CSMain");
        cs.SetFloat("Attenuation", Attenuation);
        cs.SetInt("NumWidth", _NumWidth);
        cs.SetInt("NumHeight", _NumHeight);

        Vector3 mousePos = Input.mousePosition;
        Vector2 mousePosuv;
        mousePosuv.x = mousePos.x / Screen.width;
        mousePosuv.y = mousePos.y / Screen.height;
        mousePosuv.x = OscReceiverSc.CurrentHuman.x;
        mousePosuv.y = OscReceiverSc.CurrentHuman.y;
        float CoefOnClick1 = 0.0f;
        float CoefOnClick2 = 0.0f;
        float CoefOnClick3 = 0.0f;
        float timepass;

        for (int i = 0; i < LPF; i++)
        {
            // コンピュートバッファをセット
            cs.SetBuffer(kernelId, "_BufferR", WaveEqBuffer[0]);
            cs.SetBuffer(kernelId, "_BufferG", WaveEqBuffer[1]);
            cs.SetBuffer(kernelId, "_BufferB", WaveEqBuffer[2]);
            cs.SetFloat("sharpness", sharpness);
            cs.SetFloat("mousePosu", mousePosuv.x);
            cs.SetFloat("mousePosv", mousePosuv.y);
            //float sensitivity = 0.1f; // いわゆるマウス感度
            //float mouse_move_x = Input.GetAxis("Mouse X") * sensitivity;
            //float mouse_move_y = Input.GetAxis("Mouse Y") * sensitivity;

            float current_x = OscReceiverSc.CurrentHuman.x;
            float current_y = OscReceiverSc.CurrentHuman.y;
            float previous_x = OscReceiverSc.PreviousHuman.x;
            float previous_y = OscReceiverSc.PreviousHuman.y;
            float dx = current_x - previous_x;
            float dy = current_y - previous_y;
            cs.SetFloat("mouse_move_x",dx );
            cs.SetFloat("mouse_move_y",dy );
            float speed = dx*dx + dy*dy;
            timepass = Time.time;
            cs.SetFloat("coef", coef);

            float ThresholdTime = 0.1f * Mathf.Cos(Time.time * 2.0f) + ThresholdTimeS;
            

            float SpeedCoef = 0.001f;

            if (speed> ThresholdSpeed * SpeedCoef & (timepass-timeexe1)% ThresholdTimeC> ThresholdTime)
            {       
                CoefOnClick1 = 1.0f;
                timeexe1 = Time.time;
            }
            cs.SetFloat("CoefOnClick1", CoefOnClick1);
            if (speed > ThresholdSpeed * SpeedCoef & (timepass - timeexe2) % ThresholdTimeC > ThresholdTime)
            {
                CoefOnClick2 = 1.0f;
                timeexe2 = Time.time + 0.2f;
            }
            cs.SetFloat("CoefOnClick2", CoefOnClick2);
            if (speed > ThresholdSpeed * SpeedCoef & (timepass - timeexe3) % ThresholdTimeC > ThresholdTime)
            {
                CoefOnClick3 = 1.0f;
                timeexe3 = Time.time + 0.4f;
            }
            cs.SetFloat("CoefOnClick3", CoefOnClick3);
            // コンピュートシェーダを実行
            cs.Dispatch(kernelId, numThreadGroup, 1, 1);
            CoefOnClick1 = 0.0f;
            CoefOnClick2 = 0.0f;
            CoefOnClick3 = 0.0f;

            var pDataR = new WaveEqData[_NumWidth * _NumHeight];
            var pDataG = new WaveEqData[_NumWidth * _NumHeight];
            var pDataB = new WaveEqData[_NumWidth * _NumHeight];
            WaveEqBuffer[0].GetData(pDataR);
            WaveEqBuffer[1].GetData(pDataG);
            WaveEqBuffer[2].GetData(pDataB);
            for (int j = 0; j < pDataR.Length; j++)
            {
                pDataR[j].Cm = pDataR[j].Cc;
                pDataR[j].Cc = pDataR[j].Cw;
                pDataG[j].Cm = pDataG[j].Cc;
                pDataG[j].Cc = pDataG[j].Cw;
                pDataB[j].Cm = pDataB[j].Cc;
                pDataB[j].Cc = pDataB[j].Cw;
            }
            WaveEqBuffer[0].SetData(pDataR);
            WaveEqBuffer[1].SetData(pDataG);
            WaveEqBuffer[2].SetData(pDataB);

        }
        float timem = Time.time;
        Material m = GetComponent<MeshRenderer>().material;
        m.SetBuffer("_WaveEqR", WaveEqBuffer[0]);
        m.SetBuffer("_WaveEqG", WaveEqBuffer[1]);
        m.SetBuffer("_WaveEqB", WaveEqBuffer[2]);
        m.SetInt("NumWidth", _NumWidth);
        m.SetInt("NumHeight", _NumHeight);
        m.SetFloat("timem", timem);
    }
}
