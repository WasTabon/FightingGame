using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum RewardType
{
    Coins,
    Gems,
    Skin
}

public class BattlePassReward : MonoBehaviour
{
    [Header("Reward Settings")]
    [SerializeField] private RewardType rewardType;
    [SerializeField] private int amount = 10;
    [SerializeField] private string skinId;
    
    [Header("UI Elements")]
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private GameObject checkMark;
    [SerializeField] private GameObject premiumLockOverlay;
    
    [Header("Visual Feedback")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    
    private int requiredLevel;
    private bool isPremium;
    private bool isClaimed;
    private bool isUnlocked;
    
    public RewardType RewardType => rewardType;
    public int Amount => amount;
    public string SkinId => skinId;
    public int RequiredLevel => requiredLevel;
    public bool IsPremium => isPremium;
    public bool IsClaimed => isClaimed;
    
    void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }
    
    public void Setup(int level, bool premium)
    {
        requiredLevel = level;
        isPremium = premium;
        
        if (BattlePassController.Instance != null)
        {
            isClaimed = BattlePassController.Instance.IsRewardClaimed(GetUniqueId());
        }
        
        UpdateVisuals();
    }
    
    public void RefreshState(int currentLevel, bool canClaimPremium)
    {
        bool levelUnlocked = currentLevel >= requiredLevel;
        bool premiumAccessible = !isPremium || canClaimPremium;
        
        isUnlocked = levelUnlocked && premiumAccessible && !isClaimed;
        
        UpdateVisuals();
    }
    
    void UpdateVisuals()
    {
        if (lockOverlay != null)
        {
            bool showLevelLock = WalletController.Instance != null && 
                                 WalletController.Instance.Level < requiredLevel;
            lockOverlay.SetActive(showLevelLock && !isClaimed);
        }
        
        if (premiumLockOverlay != null)
        {
            bool showPremiumLock = isPremium && 
                                   BattlePassController.Instance != null && 
                                   !BattlePassController.Instance.HasPremium;
            premiumLockOverlay.SetActive(showPremiumLock && !isClaimed);
        }
        
        if (checkMark != null)
        {
            checkMark.SetActive(isClaimed);
        }
        
        if (button != null)
        {
            button.interactable = isUnlocked;
        }
        
        if (amountText != null)
        {
            if (rewardType == RewardType.Skin)
            {
                amountText.text = "";
            }
            else
            {
                amountText.text = amount.ToString();
            }
        }
    }
    
    void OnClick()
    {
        if (!isUnlocked || isClaimed) return;
        
        if (BattlePassController.Instance != null)
        {
            BattlePassController.Instance.ClaimReward(this);
        }
    }
    
    public void SetClaimed(bool claimed)
    {
        isClaimed = claimed;
        isUnlocked = false;
        UpdateVisuals();
    }
    
    public string GetUniqueId()
    {
        return $"L{requiredLevel}_{(isPremium ? "P" : "F")}_{rewardType}";
    }
    
    public void SetRewardType(RewardType type)
    {
        rewardType = type;
    }
    
    public void SetAmount(int value)
    {
        amount = value;
        
        if (amountText != null && rewardType != RewardType.Skin)
        {
            amountText.text = amount.ToString();
        }
    }
    
    public void SetIcon(Sprite sprite)
    {
        if (iconImage != null)
        {
            iconImage.sprite = sprite;
        }
    }
}
