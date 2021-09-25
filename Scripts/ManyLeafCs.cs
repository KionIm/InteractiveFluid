using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManyLeafCs : MonoBehaviour
{
    public GameObject objOrigin;
    public int numObj;
    public float xmin;
    public float xmax;
    public float ymin;
    public float ymax;
    public float Scale;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numObj; i++)
        {
            GameObject obj = Instantiate(objOrigin);
            obj.transform.position = new Vector3(Random.Range(xmin, xmax), Random.Range(ymin,ymax), 0f);
            obj.transform.localEulerAngles = new Vector3(Random.Range(0, 360), 90, -90);
            obj.transform.localScale = new Vector3(Scale, Scale, Scale) * Random.Range(0.7f,1.3f);
            obj.transform.parent = this.gameObject.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
