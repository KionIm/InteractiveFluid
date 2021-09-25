using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public struct NSEqLData
{
    public float P;
    public float U;
    public float V;
    public float B;
};

public class FlowLeaf : MonoBehaviour
{
    public NSEqPa NSEqPaSc;
    public GameObject objOrigin;
    public float Scale;

    private int NumW;
    private int NumH;

    ComputeBuffer[] NSEqLBuffer;

    List<GameObject> listObj;
    public int numLeaf;

    // Start is called before the first frame update
    void Start()
    {
        NumW = NSEqPaSc._NumWidth;
        NumH = NSEqPaSc._NumHeight;

        NSEqLBuffer = NSEqPaSc.particleBuffer;

        listObj = new List<GameObject>();
        for (int i = 0; i < numLeaf; i++)
        {
            GameObject obj = Instantiate(objOrigin);

            listObj.Add(obj);
        }

        for (int i = 0; i < numLeaf; i++)
        {
            GameObject obj = listObj[i];
            Material m = obj.GetComponent<MeshRenderer>().material;
            m.SetBuffer("particleBuffer", NSEqLBuffer[0]);
            m.SetFloat("Scale", Scale);

            int myIdx = (int)(Random.Range(0f,1f) * NSEqPaSc.IniNumW * NSEqPaSc.IniNumH);

            m.SetInt("idxParticle", myIdx);
        }

    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 pos = objOrigin.transform.position;
        //Vector3 velocity;

        //if (pos.y > 0.5f)
        //{
        //    velocity = DecideVelocity1(0.02f);
        //}
        //if (pos.y < -0.5f)
        //{
        //    velocity = DecideVelocity1(0.02f);
        //}



    }

    Vector3 DecideVelocity1(float speed)
    {
        Vector3 velocity;
        velocity.x = 0.0f;
        velocity.y = speed;
        velocity.z = 0.0f;
        return velocity;
    }
}
