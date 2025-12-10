using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class BattlePassController : MonoBehaviour
{
    public static BattlePassController Instance { get; private set; }
    
    [Header("Premium Status")]
    [SerializeField] private bool hasPremium;
    
    [Header("Levels Container")]
    [SerializeField] private Transform levelsContainer;
    
    [Header("Reward Claimed Panel")]
    [SerializeField] private GameObject rewardClaimedPanel;
    [SerializeField] private TextMeshProUGUI rewardClaimedText;
    [SerializeField] private Image rewardClaimedIcon;
    [SerializeField] private float panelShowDuration = 2f;
    
    [Header("Reward Icons")]
    [SerializeField] private Sprite coinsIcon;
    [SerializeField] private Sprite gemsIcon;
    [SerializeField] private Sprite skinsIcon;
    
    [Header("Scroll Settings")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private bool autoScrollToCurrentLevel = true;
    
    private List<BattlePassLevel> levels = new List<BattlePassLevel>();
    private const string CLAIMED_KEY_PREFIX = "BP_Claimed_";
    
    public bool HasPremium => hasPremium;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Start()
    {
        CollectLevels();
        RefreshAllLevels();
        
        if (WalletController.Instance != null)
        {
            WalletController.Instance.OnLevelUp += OnLevelUp;
        }
        
        if (rewardClaimedPanel != null)
        {
            rewardClaimedPanel.SetActive(false);
        }
        
        if (autoScrollToCurrentLevel)
        {
            ScrollToCurrentLevel();
        }
    }
    
    void CollectLevels()
    {
        levels.Clear();
        
        if (levelsContainer != null)
        {
            foreach (Transform child in levelsContainer)
            {
                BattlePassLevel level = child.GetComponent<BattlePassLevel>();
                if (level != null)
                {
                    levels.Add(level);
                }
            }
        }
        
        Debug.Log($"[BattlePassController] Collected {levels.Count} levels");
    }
    
    void OnLevelUp(int newLevel)
    {
        RefreshAllLevels();
    }
    
    public void RefreshAllLevels()
    {
        int currentLevel = WalletController.Instance != null ? WalletController.Instance.Level : 1;
        
        foreach (var level in levels)
        {
            level.RefreshState(currentLevel, hasPremium);
        }
    }
    
    public void ClaimReward(BattlePassReward reward)
    {
        if (reward == null || reward.IsClaimed) return;
        
        int currentLevel = WalletController.Instance != null ? WalletController.Instance.Level : 1;
        
        if (reward.RequiredLevel > currentLevel)
        {
            Debug.Log("[BattlePassController] Level too low to claim reward");
            return;
        }
        
        if (reward.IsPremium && !hasPremium)
        {
            Debug.Log("[BattlePassController] Premium required to claim this reward");
            return;
        }
        
        switch (reward.RewardType)
        {
            case RewardType.Coins:
                WalletController.Instance?.AddCoins(reward.Amount);
                break;
            case RewardType.Gems:
                WalletController.Instance?.AddGems(reward.Amount);
                break;
            case RewardType.Skin:
                Debug.Log($"[BattlePassController] Skin claimed: {reward.SkinId}");
                break;
        }
        
        reward.SetClaimed(true);
        SaveClaimedState(reward);
        
        ShowRewardClaimedPanel(reward);
        
        Debug.Log($"[BattlePassController] Claimed {reward.RewardType} x{reward.Amount}");
    }
    
    void ShowRewardClaimedPanel(BattlePassReward reward)
    {
        if (rewardClaimedPanel == null) return;
        
        if (rewardClaimedText != null)
        {
            switch (reward.RewardType)
            {
                case RewardType.Coins:
                    rewardClaimedText.text = $"+{reward.Amount} Coins";
                    break;
                case RewardType.Gems:
                    rewardClaimedText.text = $"+{reward.Amount} Gems";
                    break;
                case RewardType.Skin:
                    rewardClaimedText.text = "New Skin!";
                    break;
            }
        }
        
        if (rewardClaimedIcon != null)
        {
            switch (reward.RewardType)
            {
                case RewardType.Coins:
                    rewardClaimedIcon.sprite = coinsIcon;
                    break;
                case RewardType.Gems:
                    rewardClaimedIcon.sprite = gemsIcon;
                    break;
                case RewardType.Skin:
                    rewardClaimedIcon.sprite = skinsIcon;
                    break;
            }
        }
        
        rewardClaimedPanel.transform.localScale = Vector3.zero;
        rewardClaimedPanel.SetActive(true);
        
        rewardClaimedPanel.transform.DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(panelShowDuration, () =>
                {
                    rewardClaimedPanel.transform.DOScale(Vector3.zero, 0.2f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => rewardClaimedPanel.SetActive(false));
                });
            });
    }
    
    void ScrollToCurrentLevel()
    {
        if (scrollRect == null || levels.Count == 0) return;
        
        int currentLevel = WalletController.Instance != null ? WalletController.Instance.Level : 1;
        int targetIndex = Mathf.Clamp(currentLevel - 1, 0, levels.Count - 1);
        
        float normalizedPos = (float)targetIndex / (levels.Count - 1);
        
        DOVirtual.DelayedCall(0.1f, () =>
        {
            scrollRect.horizontalNormalizedPosition = normalizedPos;
        });
    }
    
    public void SetPremium(bool premium)
    {
        hasPremium = premium;
        PlayerPrefs.SetInt("BP_HasPremium", premium ? 1 : 0);
        PlayerPrefs.Save();
        RefreshAllLevels();
    }
    
    void SaveClaimedState(BattlePassReward reward)
    {
        string key = CLAIMED_KEY_PREFIX + reward.GetUniqueId();
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }
    
    public bool IsRewardClaimed(string uniqueId)
    {
        string key = CLAIMED_KEY_PREFIX + uniqueId;
        return PlayerPrefs.GetInt(key, 0) == 1;
    }
    
    [ContextMenu("Reset All Claimed Rewards")]
    public void ResetAllClaimedRewards()
    {
        foreach (var level in levels)
        {
            level.ResetClaimed();
        }
        
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        RefreshAllLevels();
        
        Debug.Log("[BattlePassController] All claimed rewards reset");
    }
    
    [ContextMenu("Toggle Premium")]
    public void TogglePremium()
    {
        SetPremium(!hasPremium);
    }
    
    void OnDestroy()
    {
        if (WalletController.Instance != null)
        {
            WalletController.Instance.OnLevelUp -= OnLevelUp;
        }
        
        DOTween.Kill(rewardClaimedPanel?.transform);
    }
}
