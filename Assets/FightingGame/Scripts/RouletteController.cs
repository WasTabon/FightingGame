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
    [SerializeField] private TextMeshProUGUI damageText;
    
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
    
    [Header("Audio")]
    [SerializeField] private AudioClip spinSound;
    [SerializeField] private AudioClip criticalResultSound;
    [SerializeField] private AudioClip normalResultSound;
    [SerializeField] private AudioClip weakResultSound;
    
    private float[] zoneStartAngles;
    private Action<float, int> onSpinComplete;
    private bool isInitialized = false;
    private int currentBaseDamage;
    private bool currentIsDefense;
    
    void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        if (isInitialized) return;
        
        Debug.Log($"[RouletteController] Initialize called");
        
        CalculateZoneAngles();
        
        if (roulettePanel != null)
        {
            roulettePanel.transform.localScale = Vector3.one;
            roulettePanel.SetActive(false);
        }
        
        isInitialized = true;
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
        }
    }
    
    public void Spin(bool isDefense, int baseDamage, Action<float, int> callback)
    {
        Initialize();
        
        Debug.Log($"[RouletteController] Spin called, isDefense: {isDefense}, baseDamage: {baseDamage}");
        
        onSpinComplete = callback;
        currentBaseDamage = baseDamage;
        currentIsDefense = isDefense;
        
        if (roulettePanel == null)
        {
            Debug.LogError("[RouletteController] roulettePanel is NULL!");
            callback?.Invoke(1f, baseDamage);
            return;
        }

        DOTween.Kill(roulettePanel.transform);
        if (arrowTransform != null)
            DOTween.Kill(arrowTransform);
        
        roulettePanel.transform.localScale = Vector3.zero;
        roulettePanel.SetActive(true);
        
        if (arrowTransform != null)
            arrowTransform.localRotation = Quaternion.identity;
        
        if (resultText != null)
        {
            resultText.text = "";
            resultText.transform.localScale = Vector3.zero;
        }
        
        if (damageText != null)
        {
            damageText.text = "";
            damageText.transform.localScale = Vector3.zero;
        }
        
        roulettePanel.transform.DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                StartSpinAnimation(isDefense);
            });
    }

    void StartSpinAnimation(bool isDefense)
    {
        float randomAngle = UnityEngine.Random.Range(0f, 360f);
        int rotations = UnityEngine.Random.Range(minRotations, maxRotations + 1);
        float totalRotation = rotations * 360f + randomAngle;
        
        if (spinSound != null && MusicController.Instance != null)
        {
            MusicController.Instance.PlaySpecificSound(spinSound);
        }
        
        if (arrowTransform != null)
        {
            arrowTransform.DOLocalRotate(new Vector3(0, 0, -totalRotation), spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuart)
                .OnComplete(() => 
                {
                    OnSpinFinished(randomAngle, isDefense);
                });
        }
        else
        {
            Debug.LogError("[RouletteController] arrowTransform is NULL!");
            onSpinComplete?.Invoke(1f, currentBaseDamage);
        }
    }
    
    void OnSpinFinished(float finalAngle, bool isDefense)
    {
        float multiplier = GetMultiplierForAngle(finalAngle);
        int zoneIndex = GetZoneIndexForAngle(finalAngle);
        
        int finalDamage = Mathf.RoundToInt(currentBaseDamage * multiplier);
        
        Debug.Log($"[RouletteController] Multiplier: {multiplier}, finalDamage: {finalDamage}");
        
        ShowResultText(zoneIndex, isDefense, multiplier, finalDamage);
        
        DOVirtual.DelayedCall(resultTextDuration + 0.5f, () =>
        {
            HideRoulette(() => 
            {
                onSpinComplete?.Invoke(multiplier, finalDamage);
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
    
    void ShowResultText(int zoneIndex, bool isDefense, float multiplier, int finalDamage)
    {
        if (resultText == null || zones.Count == 0) return;
        
        string[] textArray;
        Color textColor = zones[zoneIndex].zoneColor;
        AudioClip resultSound = null;
        
        if (isDefense)
        {
            if (multiplier >= 1.5f)
            {
                textArray = defenseGoodTexts;
                resultSound = criticalResultSound;
            }
            else if (multiplier >= 1f)
            {
                textArray = defenseNormalTexts;
                resultSound = normalResultSound;
            }
            else
            {
                textArray = defenseBadTexts;
                resultSound = weakResultSound;
            }
        }
        else
        {
            if (multiplier >= 1.5f)
            {
                textArray = criticalTexts;
                resultSound = criticalResultSound;
            }
            else if (multiplier >= 1f)
            {
                textArray = normalTexts;
                resultSound = normalResultSound;
            }
            else
            {
                textArray = weakTexts;
                resultSound = weakResultSound;
            }
        }
        
        if (resultSound != null && MusicController.Instance != null)
        {
            MusicController.Instance.PlaySpecificSound(resultSound);
        }
        
        resultText.text = textArray[UnityEngine.Random.Range(0, textArray.Length)];
        resultText.color = textColor;
        
        resultText.transform.localScale = Vector3.zero;
        resultText.transform.DOScale(Vector3.one * 1.5f, 0.3f).SetEase(Ease.OutBack);
        resultText.transform.DOScale(Vector3.one, 0.2f).SetDelay(0.3f);
        
        resultText.transform.DOShakeRotation(0.5f, new Vector3(0, 0, 15), 10, 90, true)
            .SetDelay(0.3f);
        
        if (damageText != null)
        {
            if (isDefense)
            {
                int blockPercent = Mathf.RoundToInt(multiplier * 100f);
                damageText.text = $"-{blockPercent}% DMG";
            }
            else
            {
                damageText.text = $"{finalDamage} DMG";
            }
            
            damageText.color = textColor;
            damageText.transform.localScale = Vector3.zero;
            damageText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetDelay(0.2f);
        }
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
        DOTween.Kill(roulettePanel?.transform);
        DOTween.Kill(arrowTransform);
        DOTween.Kill(resultText?.transform);
        DOTween.Kill(damageText?.transform);
    }
}