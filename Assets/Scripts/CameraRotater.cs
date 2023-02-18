using UnityEngine;

public class CameraRotater : MonoBehaviour
{

    public float rotateSpeed = 0;
    Network network;

    private void Awake()
    {
        network = FindObjectOfType<Network>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    //public CinemachineFreeLook playerCam;
    void Update()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + Input.GetAxis("Mouse X") * rotateSpeed, transform.eulerAngles.z);
        network.Look(transform.eulerAngles);
    }
}