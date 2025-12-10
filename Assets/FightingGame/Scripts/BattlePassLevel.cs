using UnityEngine;
using TMPro;

public class BattlePassLevel : MonoBehaviour
{
    [Header("Level Info")]
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private TextMeshProUGUI levelText;
    
    [Header("Rewards")]
    [SerializeField] private BattlePassReward freeReward;
    [SerializeField] private BattlePassReward premiumReward;
    
    public int LevelNumber => levelNumber;
    
    void Start()
    {
        if (levelText != null)
        {
            levelText.text = levelNumber.ToString();
        }
        
        if (freeReward != null)
        {
            freeReward.Setup(levelNumber, false);
        }
        
        if (premiumReward != null)
        {
            premiumReward.Setup(levelNumber, true);
        }
    }
    
    public void RefreshState(int currentLevel, bool hasPremium)
    {
        if (freeReward != null)
        {
            freeReward.RefreshState(currentLevel, true);
        }
        
        if (premiumReward != null)
        {
            premiumReward.RefreshState(currentLevel, hasPremium);
        }
    }
    
    public void ResetClaimed()
    {
        if (freeReward != null)
        {
            freeReward.SetClaimed(false);
        }
        
        if (premiumReward != null)
        {
            premiumReward.SetClaimed(false);
        }
    }
    
    public void SetLevelNumber(int level)
    {
        levelNumber = level;
        
        if (levelText != null)
        {
            levelText.text = levelNumber.ToString();
        }
        
        if (freeReward != null)
        {
            freeReward.Setup(levelNumber, false);
        }
        
        if (premiumReward != null)
        {
            premiumReward.Setup(levelNumber, true);
        }
    }
}
