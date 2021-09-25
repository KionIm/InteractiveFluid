using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMovement : MonoBehaviour
{
    public float movementScale;

    void Start()
    {
        
    }


    void Update()
    {
        transform.position += Vector3.one * movementScale;
    }
}
