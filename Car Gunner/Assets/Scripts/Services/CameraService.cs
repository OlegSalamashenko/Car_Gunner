using UnityEngine;
using Cinemachine;

public class CameraService : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera introCam;
    [SerializeField] private CinemachineVirtualCamera gameCam;
    [SerializeField] private Transform car;

    public void ResetCamera()
    {
        introCam.Priority = 20;
        gameCam.Priority  = 10;

        introCam.LookAt = car;
        introCam.Follow = null;
    }

    public void LerpToFollow()
    {
        introCam.Priority = 10;
        gameCam.Priority  = 20;

        gameCam.Follow = car;
        gameCam.LookAt = car;
    }
}