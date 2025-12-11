using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class WalletController : MonoBehaviour
{
    public static WalletController Instance { get; private set; }

    [SerializeField] private GameObject _panelSucces;
    [SerializeField] private GameObject _panelNotMoney;
    
    [Header("Currency")]
    [SerializeField] private int coins;
    [SerializeField] private int gems;
    
    [Header("Progression")]
    [SerializeField] private int rank;
    [SerializeField] private int exp;
    [SerializeField] private int level;
    
    [Header("UI - Main")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private TextMeshProUGUI rankText;
    
    [Header("UI - Battle Pass")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private Image expFillImage;
    
    [Header("Settings")]
    [SerializeField] private int expPerLevel = 300;
    [SerializeField] private int winRankReward = 30;
    [SerializeField] private int winExpReward = 150;
    
    private const string COINS_KEY = "Wallet_Coins";
    private const string GEMS_KEY = "Wallet_Gems";
    private const string RANK_KEY = "Wallet_Rank";
    private const string EXP_KEY = "Wallet_Exp";
    private const string LEVEL_KEY = "Wallet_Level";
    
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnGemsChanged;
    public event Action<int> OnRankChanged;
    public event Action<int, int> OnExpChanged;
    public event Action<int> OnLevelUp;
    
    public int Coins => coins;
    public int Gems => gems;
    public int Rank => rank;
    public int Exp => exp;
    public int Level => level;
    public int ExpPerLevel => expPerLevel;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        LoadData();
    }
    
    void LoadData()
    {
        coins = PlayerPrefs.GetInt(COINS_KEY, 0);
        gems = PlayerPrefs.GetInt(GEMS_KEY, 0);
        rank = PlayerPrefs.GetInt(RANK_KEY, 0);
        exp = PlayerPrefs.GetInt(EXP_KEY, 0);
        level = PlayerPrefs.GetInt(LEVEL_KEY, 1);
        
        Debug.Log($"[WalletController] Loaded: Coins={coins}, Gems={gems}, Rank={rank}, Exp={exp}, Level={level}");
        
        UpdateAllUI();
    }
    
    void UpdateAllUI()
    {
        UpdateCoinsUI();
        UpdateGemsUI();
        UpdateRankUI();
        UpdateExpUI();
    }

    public void BuyHealth()
    {
        if (SpendCoins(300))
        {
            _panelSucces.SetActive(true);
        }
        else
        {
            _panelNotMoney.SetActive(true);
        }
    }
    public void BuyDamage()
    {
        if (SpendGems(50))
        {
            _panelSucces.SetActive(true);
        }
        else
        {
            _panelNotMoney.SetActive(true);
        }
    }
    
    void UpdateCoinsUI()
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();
    }
    
    void UpdateGemsUI()
    {
        if (gemsText != null)
            gemsText.text = gems.ToString();
    }
    
    void UpdateRankUI()
    {
        if (rankText != null)
            rankText.text = rank.ToString();
    }
    
    void UpdateExpUI()
    {
        if (levelText != null)
            levelText.text = level.ToString();
        
        if (expText != null)
            expText.text = $"{exp}/{expPerLevel}";
        
        if (expFillImage != null)
            expFillImage.fillAmount = GetExpProgress();
    }
    
    void SaveData()
    {
        PlayerPrefs.SetInt(COINS_KEY, coins);
        PlayerPrefs.SetInt(GEMS_KEY, gems);
        PlayerPrefs.SetInt(RANK_KEY, rank);
        PlayerPrefs.SetInt(EXP_KEY, exp);
        PlayerPrefs.SetInt(LEVEL_KEY, level);
        PlayerPrefs.Save();
    }
    
    public void AddCoins(int amount)
    {
        if (amount == 0) return;
        
        coins = Mathf.Max(0, coins + amount);
        SaveData();
        UpdateCoinsUI();
        OnCoinsChanged?.Invoke(coins);
        
        Debug.Log($"[WalletController] Coins: {coins} ({(amount >= 0 ? "+" : "")}{amount})");
    }
    
    public void AddGems(int amount)
    {
        if (amount == 0) return;
        
        gems = Mathf.Max(0, gems + amount);
        SaveData();
        UpdateGemsUI();
        OnGemsChanged?.Invoke(gems);
        
        Debug.Log($"[WalletController] Gems: {gems} ({(amount >= 0 ? "+" : "")}{amount})");
    }
    
    public void AddRank(int amount)
    {
        if (amount == 0) return;
        
        rank = Mathf.Max(0, rank + amount);
        SaveData();
        UpdateRankUI();
        OnRankChanged?.Invoke(rank);
        
        Debug.Log($"[WalletController] Rank: {rank} ({(amount >= 0 ? "+" : "")}{amount})");
    }
    
    public void AddExp(int amount)
    {
        if (amount <= 0) return;
        
        exp += amount;
        
        int levelsGained = 0;
        while (exp >= expPerLevel)
        {
            exp -= expPerLevel;
            level++;
            levelsGained++;
            OnLevelUp?.Invoke(level);
            Debug.Log($"[WalletController] Level Up! Now level {level}");
        }
        
        SaveData();
        UpdateExpUI();
        OnExpChanged?.Invoke(exp, expPerLevel);
        
        Debug.Log($"[WalletController] Exp: {exp}/{expPerLevel} (+{amount})");
    }
    
    public void OnPlayerWin()
    {
        Debug.Log("[WalletController] Player won! Giving rewards...");
        AddRank(winRankReward);
        AddExp(winExpReward);
    }
    
    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || coins < amount) return false;
        
        AddCoins(-amount);
        return true;
    }
    
    public bool SpendGems(int amount)
    {
        if (amount <= 0 || gems < amount) return false;
        
        AddGems(-amount);
        return true;
    }
    
    public float GetExpProgress()
    {
        return (float)exp / expPerLevel;
    }
    
    [ContextMenu("Reset All Data")]
    public void ResetAllData()
    {
        coins = 0;
        gems = 0;
        rank = 0;
        exp = 0;
        level = 1;
        SaveData();
        
        OnCoinsChanged?.Invoke(coins);
        OnGemsChanged?.Invoke(gems);
        OnRankChanged?.Invoke(rank);
        OnExpChanged?.Invoke(exp, expPerLevel);
        
        Debug.Log("[WalletController] All data reset!");
    }
    
    [ContextMenu("Add Test Rewards")]
    public void AddTestRewards()
    {
        AddCoins(100);
        AddGems(10);
        OnPlayerWin();
    }
}
