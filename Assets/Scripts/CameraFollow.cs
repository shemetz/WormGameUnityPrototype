using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float movementDamping = 0.4f;
    public float cameraZoomSpeed = 0.04f;
    public Transform target;

    private Vector3 velocity = Vector3.zero;
    private float originalZoom;

    void Start()
    {
        //GetComponent<Camera>().orthographicSize = Screen.height / 2 / 96;
        originalZoom = GetComponent<Camera>().orthographicSize;
        transform.position = new Vector3(target.position.x, target.position.y, -10);
    }

    void FixedUpdate()
    {
        //Put on same plane, so that lerping is good
        transform.position = new Vector3(transform.position.x, transform.position.y, target.position.z);

        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, movementDamping);
        //resture to target.z - 10
        transform.position = new Vector3(transform.position.x, transform.position.y, target.position.z - 10);

        //zoom with Ctrl+mouse scroll
        if (Input.GetButton("Control"))
        {
            float scrollDelta = Input.mouseScrollDelta.y;
            GetComponent<Camera>().orthographicSize *= (1.00f - cameraZoomSpeed * scrollDelta);
            if (Input.GetButtonDown("Hotkey 0")) //middle mouse. TODO make this in UI somehow
                GetComponent<Camera>().orthographicSize = originalZoom;
        }
    }
}
