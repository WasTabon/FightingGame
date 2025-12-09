using UnityEngine;
using Cinemachine;

public class InitialCameraSwitcher : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera initialCamera;
    [SerializeField] private CinemachineVirtualCamera[] otherCameras;
    [SerializeField] private bool findAllCamerasAutomatically = true;

    void Start()
    {
        if (findAllCamerasAutomatically)
        {
            var allCameras = FindObjectsOfType<CinemachineVirtualCamera>();
            foreach (var cam in allCameras)
            {
                if (cam != initialCamera)
                    cam.Priority = 0;
            }
        }
        else if (otherCameras != null)
        {
            foreach (var cam in otherCameras)
            {
                if (cam != null)
                    cam.Priority = 0;
            }
        }

        if (initialCamera != null)
            initialCamera.Priority = 10;
    }

    public void SwitchToInitialCamera()
    {
        if (findAllCamerasAutomatically)
        {
            var allCameras = FindObjectsOfType<CinemachineVirtualCamera>();
            foreach (var cam in allCameras)
            {
                if (cam != initialCamera)
                    cam.Priority = 0;
            }
        }
        else if (otherCameras != null)
        {
            foreach (var cam in otherCameras)
            {
                if (cam != null)
                    cam.Priority = 0;
            }
        }

        if (initialCamera != null)
            initialCamera.Priority = 10;
    }
}