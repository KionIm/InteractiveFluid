using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;
using PrefsGUI.Example;

public class OscReceiverImai : MonoBehaviour
{
    OSCReceiver receiver;
    public int oscPort;
    public string oscAddress;

    public bool EnableMouse;
    public Vector2 CurrentHuman;
    [HideInInspector] public Vector2 PreviousHuman;
    [HideInInspector] public float CurrentTime;
    [HideInInspector] public float PreviousTime;

    // Start is called before the first frame update
    void Start()
    {
        receiver = gameObject.AddComponent<OSCReceiver>();

        // Set local port.
        receiver.LocalPort = oscPort;

        receiver.Bind(oscAddress, MessageReceived);
        CurrentTime = 0.0f;

    }

    // Update is called once per frame
    void Update()
    {
        EnableMouse = PrefsChild0.enableMouse;
        if (EnableMouse)
        {
            PreviousTime = CurrentTime;
            CurrentTime = Time.time;
            Vector3 mousePos = Input.mousePosition;
            Vector2 mousePosuv;
            mousePosuv.y = mousePos.x / Screen.width;
            mousePosuv.x = 1f - mousePos.y / Screen.height;

            PreviousHuman = CurrentHuman;
            CurrentHuman = mousePosuv;
        }
    }

    protected void MessageReceived(OSCMessage message)
    {
        if (EnableMouse)
        {
            return;
        }
        var listValue = message.Values;

        CurrentHuman = new Vector2(1f - listValue[3].FloatValue, listValue[2].FloatValue);
    }
}
