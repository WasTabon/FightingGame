using UnityEngine;
using Cinemachine;

public class InitialCameraSwitcher : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera initialCamera;
    [SerializeField] private CinemachineVirtualCamera[] otherCameras;

    void Start()
    {
        SwitchToInitialCamera();
    }

    public void SwitchToInitialCamera()
    {
        if (otherCameras != null)
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