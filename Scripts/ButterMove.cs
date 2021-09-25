using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterMove : MonoBehaviour
{
    public float AccelM;
    public float Speed;
    public float TopSpeed;
    public float EscapePointDistance;
    private float Aspect;// = 0.68645;
    public float TarPoCoef1;
    public float TarPoCoef2;

    private Vector3 TargetPoint;
    private Vector3 PrePos;
    private Vector3 ButterVelocity;

    public OscReceiverImai OscReceiverSc;

    // Start is called before the first frame update
    void Start()
    {
        Aspect = 0.68645f;
        Transform myTransform = this.transform;
        Vector3 pos = myTransform.position;
        TargetPoint.x = 0.3f;
        TargetPoint.y = 0.4f;
        TargetPoint.z = pos.z;
        PrePos = pos;
        //float deltaTime = 1.0f/30.0f;
        ButterVelocity.x = 0.0f;// (pos - PrePos) / deltaTime;
        ButterVelocity.y = 0.0f;
        ButterVelocity.z = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        Transform myTransform = this.transform;
        Vector3 pos = myTransform.position;
        float SpeedUpdate = Speed;
        Vector3 mousePos = Input.mousePosition;
        Vector3 mousePosuv;
        mousePosuv.x = (OscReceiverSc.CurrentHuman.x-0.5f)*Aspect;// (mousePos.x / Screen.width -0.5f)*Aspect;
        mousePosuv.y = OscReceiverSc.CurrentHuman.y - 0.5f;// mousePos.y / Screen.height - 0.5f;
        mousePosuv.z = 0.0f;

        Vector3 Dirmt = mousePosuv - pos;
        Dirmt.z = 0.0f;
        float DirmtM = Dirmt.magnitude;   

        float distance = (TargetPoint - pos).magnitude;

        if (distance < 0.1)
        {
            TargetPoint.x = 0.35f * Mathf.Sin(2*TarPoCoef1 * Time.time);
            TargetPoint.y = -0.45f * Mathf.Cos(TarPoCoef1 * Time.time + TarPoCoef2);
        }

        if (DirmtM < 0.1)
        {
            TargetPoint = pos - EscapePointDistance * Dirmt / DirmtM;
            TargetPoint.z = pos.z;
            SpeedUpdate = TopSpeed;
        }

        Vector3 Accel = new Vector3(TargetPoint.x - pos.x, 
                                    TargetPoint.y - pos.y,0.0f);

        float deltaTime = Time.deltaTime;
        ButterVelocity += AccelM * Accel * deltaTime / Accel.magnitude;
        if (ButterVelocity.magnitude>Speed)
        {
            ButterVelocity = SpeedUpdate* ButterVelocity/ButterVelocity.magnitude;
        }
        PrePos = pos;
        pos += ButterVelocity * deltaTime;

        if (pos.x > 0.4)
        {
            ButterVelocity.x *= -1.0f;
        }
        if (pos.x < -0.4)
        {
            ButterVelocity.x *= -1.0f;
        }
        if (pos.y > 0.6)
        {
            ButterVelocity.y *= -1.0f;
        }
        if (pos.y < -0.6)
        {
            ButterVelocity.y *= -1.0f;
        }

        float dir = 180f/Mathf.PI*Mathf.Atan2(ButterVelocity.y, ButterVelocity.x)-90;

        Vector3 worldAngle = myTransform.eulerAngles;
        worldAngle.x = -90.0f +dir; // ワールド座標を基準に、x軸を軸にした回転を10度に変更
        worldAngle.y = -90.0f; // ワールド座標を基準に、y軸を軸にした回転を10度に変更
        worldAngle.z = 90.0f; // ワールド座標を基準に、z軸を軸にした回転を10度に変更
        myTransform.eulerAngles = worldAngle;

        myTransform.position = pos;
    }
}
