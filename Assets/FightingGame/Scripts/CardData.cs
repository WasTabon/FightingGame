using UnityEngine;

[System.Serializable]
public class CardData
{
    public string cardName;
    public string animationStateName;
    public string victimAnimationStateName;
    public float hitTimingSeconds;
    public int baseDamage;
    public bool isDefense;
    
    public CardData(string name, string animState, string victimAnimState, float timing, int damage, bool defense = false)
    {
        cardName = name;
        animationStateName = animState;
        victimAnimationStateName = victimAnimState;
        hitTimingSeconds = timing;
        baseDamage = damage;
        isDefense = defense;
    }
}
