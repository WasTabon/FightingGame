using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using System.Collections.Generic;

public class RouletteController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject roulettePanel;
    [SerializeField] private Image rouletteWheel;
    [SerializeField] private RectTransform arrowTransform;
    [SerializeField] private TextMeshProUGUI resultText;
    
    [Header("Zones Configuration")]
    [SerializeField] private List<RouletteZone> zones = new List<RouletteZone>();
    
    [Header("Animation Settings")]
    [SerializeField] private float spinDuration = 2f;
    [SerializeField] private int minRotations = 3;
    [SerializeField] private int maxRotations = 5;
    [SerializeField] private float resultTextDuration = 1f;
    
    [Header("Result Text Effects")]
    [SerializeField] private string[] criticalTexts = { "CRITICAL!", "DEVASTATING!", "PERFECT!" };
    [SerializeField] private string[] normalTexts = { "HIT!", "SOLID!", "NICE!" };
    [SerializeField] private string[] weakTexts = { "WEAK...", "GLANCING...", "BLOCKED!" };
    [SerializeField] private string[] defenseGoodTexts = { "PERFECT BLOCK!", "IRON DEFENSE!", "IMPENETRABLE!" };
    [SerializeField] private string[] defenseNormalTexts = { "BLOCKED!", "DEFENDED!", "GUARDED!" };
    [SerializeField] private string[] defenseBadTexts = { "PARTIAL BLOCK...", "GRAZED...", "SLIPPED!" };
    
    private float[] zoneStartAngles;
    private Action<float> onSpinComplete;
    
    void Awake()
    {
        Debug.Log($"[RouletteController] Awake called");
        Debug.Log($"[RouletteController] roulettePanel null: {roulettePanel == null}");
        Debug.Log($"[RouletteController] arrowTransform null: {arrowTransform == null}");
        Debug.Log($"[RouletteController] zones count: {zones.Count}");
        
        CalculateZoneAngles();
        if (roulettePanel != null)
            roulettePanel.SetActive(false);
    }
    
    void CalculateZoneAngles()
    {
        if (zones.Count == 0)
        {
            Debug.LogWarning("[RouletteController] No zones configured!");
            return;
        }
        
        zoneStartAngles = new float[zones.Count];
        float currentAngle = 0f;
        
        for (int i = 0; i < zones.Count; i++)
        {
            zoneStartAngles[i] = currentAngle;
            currentAngle += zones[i].angleSize;
            Debug.Log($"[RouletteController] Zone {i}: {zones[i].zoneName}, start: {zoneStartAngles[i]}, size: {zones[i].angleSize}");
        }
    }
    
    public void Spin(bool isDefense, Action<float> callback)
    {
        Debug.Log($"[RouletteController] Spin called, isDefense: {isDefense}, callback null: {callback == null}");
        
        onSpinComplete = callback;
        
        if (roulettePanel == null)
        {
            Debug.LogError("[RouletteController] roulettePanel is NULL!");
            callback?.Invoke(1f);
            return;
        }
        
        roulettePanel.SetActive(true);
        roulettePanel.transform.localScale = Vector3.zero;
        roulettePanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        Debug.Log("[RouletteController] Panel activated and scaling");
        
        if (arrowTransform != null)
            arrowTransform.localRotation = Quaternion.identity;
        
        if (resultText != null)
        {
            resultText.text = "";
            resultText.transform.localScale = Vector3.zero;
        }
        
        float randomAngle = UnityEngine.Random.Range(0f, 360f);
        int rotations = UnityEngine.Random.Range(minRotations, maxRotations + 1);
        float totalRotation = rotations * 360f + randomAngle;
        
        Debug.Log($"[RouletteController] Random angle: {randomAngle}, rotations: {rotations}, total: {totalRotation}");
        
        if (arrowTransform != null)
        {
            Debug.Log("[RouletteController] Starting arrow rotation");
            arrowTransform.DOLocalRotate(new Vector3(0, 0, -totalRotation), spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuart)
                .OnComplete(() => 
                {
                    Debug.Log("[RouletteController] Arrow rotation complete");
                    OnSpinFinished(randomAngle, isDefense);
                });
        }
        else
        {
            Debug.LogError("[RouletteController] arrowTransform is NULL!");
            callback?.Invoke(1f);
        }
    }
    
    void OnSpinFinished(float finalAngle, bool isDefense)
    {
        Debug.Log($"[RouletteController] OnSpinFinished, finalAngle: {finalAngle}");
        
        float multiplier = GetMultiplierForAngle(finalAngle);
        int zoneIndex = GetZoneIndexForAngle(finalAngle);
        
        Debug.Log($"[RouletteController] Multiplier: {multiplier}, zoneIndex: {zoneIndex}");
        
        ShowResultText(zoneIndex, isDefense);
        
        DOVirtual.DelayedCall(resultTextDuration + 0.5f, () =>
        {
            Debug.Log("[RouletteController] Hiding roulette");
            HideRoulette(() => 
            {
                Debug.Log("[RouletteController] Invoking callback with multiplier: " + multiplier);
                onSpinComplete?.Invoke(multiplier);
            });
        });
    }
    
    float GetMultiplierForAngle(float angle)
    {
        angle = angle % 360f;
        float currentAngle = 0f;
        
        for (int i = 0; i < zones.Count; i++)
        {
            if (angle >= currentAngle && angle < currentAngle + zones[i].angleSize)
            {
                return zones[i].damageMultiplier;
            }
            currentAngle += zones[i].angleSize;
        }
        
        Debug.LogWarning($"[RouletteController] Could not find zone for angle {angle}, returning 1");
        return 1f;
    }
    
    int GetZoneIndexForAngle(float angle)
    {
        angle = angle % 360f;
        float currentAngle = 0f;
        
        for (int i = 0; i < zones.Count; i++)
        {
            if (angle >= currentAngle && angle < currentAngle + zones[i].angleSize)
            {
                return i;
            }
            currentAngle += zones[i].angleSize;
        }
        
        return 0;
    }
    
    void ShowResultText(int zoneIndex, bool isDefense)
    {
        if (resultText == null || zones.Count == 0) return;
        
        string[] textArray;
        Color textColor = zones[zoneIndex].zoneColor;
        float multiplier = zones[zoneIndex].damageMultiplier;
        
        if (isDefense)
        {
            if (multiplier >= 1.5f) textArray = defenseGoodTexts;
            else if (multiplier >= 1f) textArray = defenseNormalTexts;
            else textArray = defenseBadTexts;
        }
        else
        {
            if (multiplier >= 1.5f) textArray = criticalTexts;
            else if (multiplier >= 1f) textArray = normalTexts;
            else textArray = weakTexts;
        }
        
        resultText.text = textArray[UnityEngine.Random.Range(0, textArray.Length)];
        resultText.color = textColor;
        
        resultText.transform.localScale = Vector3.zero;
        resultText.transform.DOScale(Vector3.one * 1.5f, 0.3f).SetEase(Ease.OutBack);
        resultText.transform.DOScale(Vector3.one, 0.2f).SetDelay(0.3f);
        
        resultText.transform.DOShakeRotation(0.5f, new Vector3(0, 0, 15), 10, 90, true)
            .SetDelay(0.3f);
    }
    
    void HideRoulette(Action onComplete)
    {
        if (roulettePanel != null)
        {
            roulettePanel.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    roulettePanel.SetActive(false);
                    onComplete?.Invoke();
                });
        }
        else
        {
            onComplete?.Invoke();
        }
    }
    
    public List<RouletteZone> GetZones()
    {
        return zones;
    }
    
    void OnDestroy()
    {
        DOTween.Kill(this);
    }
}