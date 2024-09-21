using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraRotate : MonoBehaviour
{
    public Transform camRotatePointObject;
    public Camera mainCam;
    private Vector3 camRotatePoint;
    public float sens = 1;
    public float speed = 1;
    public float smoothness = 1;
    private float inputX;
    float x = 0;

    void getInput() {

        // some smoothing for the orbit
        if (Input.GetKey(KeyCode.Mouse1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            inputX = Mathf.Lerp(inputX, Input.GetAxis("Mouse X"), Time.deltaTime * smoothness);

        }
        else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            inputX = Mathf.Lerp(inputX, 0, Time.deltaTime * smoothness);
        }
    }

    void Start()
    {
        
        camRotatePoint = camRotatePointObject.position;
    }

    // Update is called once per frame
    void Update()
    {
        getInput();
        x  += inputX * Time.deltaTime * speed;

        Quaternion rotation = Quaternion.Euler(20, x + 45, 0);

        // Update the camera's position based on rotation and distance
        Vector3 position = rotation * new Vector3(0.0f, 5f, -10) + camRotatePoint;

        // Apply the new rotation and position to the camera
        transform.rotation = rotation;
        transform.position = position;
    } 
}
