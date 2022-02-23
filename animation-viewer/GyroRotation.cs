// This script sets up a gyroscope rotation that starts as soon as a Unity scene is opened.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroRotation : MonoBehaviour
{
    // Declare Variables
    private bool isGyroEnabled;
    private Gyroscope gyro;
    private GameObject cameraContainer;
    private Quaternion rot;

    // Start is called before the first frame update
    void Start()
    {
        cameraContainer = new GameObject("Camera Container");
        cameraContainer.transform.position = transform.position;
        transform.SetParent(cameraContainer.transform);

        isGyroEnabled = EnableGyro();
    }

    // Update is called once per frame
    void Update()
    {
        if (isGyroEnabled)
        {
            transform.localRotation = gyro.attitude * rot;
        }
    }

    // Function Enabling Gyroscope Rotation
    private bool EnableGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
            cameraContainer.transform.rotation = Quaternion.Euler(90f, 90f, 0f);
            rot = new Quaternion(0, 0, 1, 0);

            return true;
        }

        return false;
    }
}
