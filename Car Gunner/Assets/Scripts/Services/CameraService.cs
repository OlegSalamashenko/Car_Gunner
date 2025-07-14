using UnityEngine;
using Cinemachine;

public class CameraService : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera vCam;
    [SerializeField] private Transform car;
    [SerializeField] private Transform cameraStartPos;
    [SerializeField] private Transform cameraFollowPos;

    public void ResetCamera()
    {
        vCam.LookAt = car;
        vCam.Follow = cameraStartPos;
    }

    public void LerpToFollow()
    {
        vCam.Follow = cameraFollowPos;
    }
}