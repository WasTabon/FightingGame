using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private PostProcessLayer _layer;

    public void HandleVolume()
    {
        _layer.enabled = !_layer.enabled;
    }
}
