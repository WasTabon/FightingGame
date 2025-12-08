using UnityEngine;

[System.Serializable]
public class RouletteZone
{
    public string zoneName;
    public Color zoneColor;
    [Range(0f, 360f)]
    public float angleSize;
    public float damageMultiplier;
}
