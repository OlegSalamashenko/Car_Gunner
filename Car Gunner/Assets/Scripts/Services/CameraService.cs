using UnityEngine;

public class CameraService : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform cameraStartPos;
    [SerializeField] private Transform cameraFollowPos;

    public void ResetCamera()
    {
        mainCamera.transform.position = cameraStartPos.position;
        mainCamera.transform.rotation = cameraStartPos.rotation;
    }

    public void FollowCar()
    {
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, cameraFollowPos.position, Time.deltaTime * 2f);
        mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, cameraFollowPos.rotation, Time.deltaTime * 2f);
    }
}