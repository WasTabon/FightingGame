using UnityEngine;

public class SkyboxRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1f;
    
    private Material skyboxMaterial;
    private float currentRotation = 0f;

    void Start()
    {
        skyboxMaterial = RenderSettings.skybox;
    }

    void Update()
    {
        currentRotation += rotationSpeed * Time.deltaTime;
        
        if (currentRotation >= 360f)
        {
            currentRotation = 0f;
        }
        
        skyboxMaterial.SetFloat("_Rotation", currentRotation);
    }
}