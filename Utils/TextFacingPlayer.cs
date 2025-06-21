using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFacingPlayer : MonoBehaviour
{
    Transform mainCamera;
    void Start()
    {
        mainCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        transform.LookAt(mainCamera);
        transform.rotation = Quaternion.LookRotation(mainCamera.forward);
    }
}
