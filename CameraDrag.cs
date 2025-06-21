using UnityEngine;
using Cinemachine;

public class CameraMouseDrag : MonoBehaviour
{
    public CinemachineFreeLook freeLookCamera;
    public Transform birdTransform;
    

    public float xSpeed = 300f;
    public float ySpeed = 2f;
    public float keyboardRotationSpeed = 1f;
    public float recenterSpeed = 2f;

    private float xAxis = 0.5f;
    private float yAxis = 0.5f;
    private Player player;

    void Start()
    {
        player = birdTransform.GetComponent<Player>();
        xAxis = freeLookCamera.m_XAxis.Value;
        yAxis = freeLookCamera.m_YAxis.Value;
    }

    void Update()
    {

        float horizontal = Input.GetAxis("Horizontal");

        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            xAxis += mouseX * xSpeed * Time.deltaTime;
            yAxis -= mouseY * ySpeed * Time.deltaTime;
            yAxis = Mathf.Clamp(yAxis, 0f, 1f); // for Cinemachine FreeLook (0 = top, 1 = bottom)
        }

        // Rotate camera with A/D
        if (!player.isTrapped)
        {
            xAxis += horizontal * keyboardRotationSpeed * Time.deltaTime; // you can adjust this speed
        }

        freeLookCamera.m_XAxis.Value = xAxis;
        freeLookCamera.m_YAxis.Value = yAxis;
    }

}
