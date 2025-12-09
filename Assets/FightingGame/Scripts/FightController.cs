using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;

public class FightController : MonoBehaviour
{
    [Header("Fighters")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform bot;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator botAnimator;
    [SerializeField] private BoxCollider playerCollider;
    [SerializeField] private BoxCollider botCollider;
    
    [Header("Positions")]
    [SerializeField] private Transform playerHitPos;
    [SerializeField] private Transform botHitPos;
    [SerializeField] private Transform playerStartPos;
    [SerializeField] private Transform botStartPos;
    
    [Header("Cameras")]
    [SerializeField] private CinemachineVirtualCamera cameraPos2;
    [SerializeField] private CinemachineVirtualCamera playerHitCamera;
    [SerializeField] private CinemachineVirtualCamera botHitCamera;
    [SerializeField] private float cameraBlendDuration = 2f;
    
    [Header("UI")]
    [SerializeField] private GameObject cardsPanel;
    [SerializeField] private List<CardButton> cardButtons;
    [SerializeField] private RouletteController rouletteController;
    
    [Header("Cards Configuration")]
    [SerializeField] private List<CardData> cards = new List<CardData>();
    
    [Header("Animation Settings")]
    [SerializeField] private float moveToHitDuration = 0.3f;
    [SerializeField] private string idleAnimationState = "Boxing New";
    [SerializeField] private string deathAnimationState = "Death New";
    
    [Header("Combat Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float defaultColliderCenterY = 1f;
    [SerializeField] private float deathColliderCenterY = 2f;
    [SerializeField] private float delayBetweenTurns = 0.5f;
    
    [Header("Cards Panel Animation")]
    [SerializeField] private float cardsPanelSlideDuration = 0.5f;
    [SerializeField] private float cardsPanelSlideDistance = 1000f;
    
    private int playerHealth;
    private int botHealth;
    private bool isPlayerTurn;
    private bool isFightActive;
    private float defenseMultiplier = 1f;
    private Vector3 playerOriginalPos;
    private Vector3 botOriginalPos;
    private Vector3 cardsPanelOriginalPos;
    
    void Start()
    {
        Debug.Log($"[FightController] Start called");
        Debug.Log($"[FightController] cardButtons count: {cardButtons.Count}");
        Debug.Log($"[FightController] cards count: {cards.Count}");
        Debug.Log($"[FightController] rouletteController null: {rouletteController == null}");
        
        InitializeCards();
        InitializeCardButtons();
        
        if (player != null) playerOriginalPos = player.position;
        if (bot != null) botOriginalPos = bot.position;
        if (cardsPanel != null) cardsPanelOriginalPos = cardsPanel.transform.localPosition;
        
        Debug.Log($"[FightController] After init - cards count: {cards.Count}");
    }
    
    void InitializeCards()
    {
        if (cards.Count == 0)
        {
            Debug.Log("[FightController] InitializeCards: Adding default cards");
            cards.Add(new CardData("Jab", "Jab 1", "Hit Body New", 0.019f, 15, false));
            cards.Add(new CardData("Uppercut", "Uppercut New", "Head Hit New", 0.014f, 25, false));
            cards.Add(new CardData("Special", "Mma Kick New", "Head Hit New", 0.029f, 35, false));
            cards.Add(new CardData("Defense", "Right Block New", "", 0f, 0, true));
        }
        else
        {
            Debug.Log($"[FightController] InitializeCards: Cards already exist ({cards.Count})");
        }
    }
    
    void InitializeCardButtons()
    {
        Debug.Log($"[FightController] InitializeCardButtons: {cardButtons.Count} buttons, {cards.Count} cards");
        for (int i = 0; i < cardButtons.Count && i < cards.Count; i++)
        {
            if (cardButtons[i] != null)
            {
                cardButtons[i].Initialize(i, OnCardSelected);
                Debug.Log($"[FightController] Initialized button {i}: {cardButtons[i].gameObject.name}");
            }
            else
            {
                Debug.LogError($"[FightController] cardButtons[{i}] is NULL!");
            }
        }
    }
    
    public void StartFight()
    {
        Debug.Log("[FightController] StartFight called");
        ResetFight();
        isFightActive = true;
        isPlayerTurn = true;
        SetCardsInteractable(true);
        Debug.Log($"[FightController] StartFight complete. isFightActive: {isFightActive}, isPlayerTurn: {isPlayerTurn}");
    }
    
    void ResetFight()
    {
        Debug.Log("[FightController] ResetFight called");
        playerHealth = maxHealth;
        botHealth = maxHealth;
        defenseMultiplier = 1f;
        
        ResetFighter(player, playerAnimator, playerCollider, playerOriginalPos);
        ResetFighter(bot, botAnimator, botCollider, botOriginalPos);
        
        SwitchCamera(cameraPos2);
    }
    
    void ResetFighter(Transform fighter, Animator animator, BoxCollider collider, Vector3 originalPos)
    {
        if (fighter != null)
            fighter.position = originalPos;
        
        if (animator != null)
            animator.CrossFade(idleAnimationState, 0.1f);
        
        if (collider != null)
        {
            Vector3 center = collider.center;
            center.y = defaultColliderCenterY;
            collider.center = center;
        }
    }
    
    void OnCardSelected(int cardIndex)
    {
        Debug.Log($"[FightController] OnCardSelected: index={cardIndex}, isFightActive={isFightActive}, isPlayerTurn={isPlayerTurn}");
        
        if (!isFightActive)
        {
            Debug.LogWarning("[FightController] Fight is not active!");
            return;
        }
        
        if (!isPlayerTurn)
        {
            Debug.LogWarning("[FightController] Not player's turn!");
            return;
        }
        
        if (cardIndex < 0 || cardIndex >= cards.Count)
        {
            Debug.LogError($"[FightController] Invalid card index: {cardIndex}, cards.Count: {cards.Count}");
            return;
        }
        
        SetCardsInteractable(false);
        CardData selectedCard = cards[cardIndex];
        Debug.Log($"[FightController] Selected card: {selectedCard.cardName}, isDefense: {selectedCard.isDefense}");
        
        if (rouletteController == null)
        {
            Debug.LogError("[FightController] rouletteController is NULL!");
            return;
        }
        
        HideCardsPanel(() =>
        {
            Debug.Log("[FightController] Calling rouletteController.Spin()");
            rouletteController.Spin(selectedCard.isDefense, (multiplier) =>
            {
                Debug.Log($"[FightController] Spin callback received, multiplier: {multiplier}");
                if (selectedCard.isDefense)
                {
                    defenseMultiplier = multiplier;
                    DOVirtual.DelayedCall(delayBetweenTurns, () => ExecuteBotTurn());
                }
                else
                {
                    ExecuteAttack(player, bot, playerAnimator, botAnimator, 
                        playerHitPos, playerHitCamera, selectedCard, multiplier, true);
                }
            });
        });
    }
    
    void ExecuteBotTurn()
    {
        Debug.Log("[FightController] ExecuteBotTurn called");
        isPlayerTurn = false;
        
        List<CardData> attackCards = cards.FindAll(c => !c.isDefense);
        if (attackCards.Count == 0)
        {
            Debug.LogError("[FightController] No attack cards found!");
            return;
        }
        
        CardData botCard = attackCards[Random.Range(0, attackCards.Count)];
        Debug.Log($"[FightController] Bot selected: {botCard.cardName}");
        
        rouletteController.Spin(false, (multiplier) =>
        {
            Debug.Log($"[FightController] Bot spin callback, multiplier: {multiplier}");
            ExecuteAttack(bot, player, botAnimator, playerAnimator,
                botHitPos, botHitCamera, botCard, multiplier, false);
        });
    }
    
    void ExecuteAttack(Transform attacker, Transform defender, 
        Animator attackerAnimator, Animator defenderAnimator,
        Transform hitPos, CinemachineVirtualCamera hitCamera,
        CardData card, float damageMultiplier, bool isPlayerAttacking)
    {
        Debug.Log($"[FightController] ExecuteAttack: {card.cardName}, isPlayerAttacking: {isPlayerAttacking}");
        
        Sequence attackSequence = DOTween.Sequence();

        attackSequence.AppendCallback(() => 
        {
            Debug.Log("[FightController] Moving attacker to hit position");
        });
        attackSequence.Append(attacker.DOMove(hitPos.position, moveToHitDuration).SetEase(Ease.OutQuad));

        attackSequence.AppendCallback(() => 
        {
            Debug.Log("[FightController] Switching camera");
            SwitchCamera(hitCamera);
        });
        attackSequence.AppendInterval(cameraBlendDuration);
        
        attackSequence.AppendCallback(() =>
        {
            Debug.Log($"[FightController] Playing attack animation: {card.animationStateName}");
            attackerAnimator.CrossFade(card.animationStateName, 0.1f);
        });
        
        attackSequence.AppendInterval(card.hitTimingSeconds);
        
        attackSequence.AppendCallback(() =>
        {
            Debug.Log("[FightController] Hit timing reached, applying damage");
            float finalMultiplier = damageMultiplier;
            if (!isPlayerAttacking && defenseMultiplier < 1f)
            {
                finalMultiplier *= (1f - defenseMultiplier);
                defenderAnimator.CrossFade("Right Block New", 0.05f);
            }
            else if (!string.IsNullOrEmpty(card.victimAnimationStateName))
            {
                defenderAnimator.CrossFade(card.victimAnimationStateName, 0.05f);
            }
            
            int damage = Mathf.RoundToInt(card.baseDamage * finalMultiplier);
            ApplyDamage(isPlayerAttacking ? false : true, damage, defenderAnimator, 
                isPlayerAttacking ? botCollider : playerCollider);
        });
        
        float animationLength = GetAnimationLength(attackerAnimator, card.animationStateName);
        Debug.Log($"[FightController] Animation length: {animationLength}");
        attackSequence.AppendInterval(animationLength - card.hitTimingSeconds);
        
        attackSequence.AppendCallback(() =>
        {
            Debug.Log("[FightController] Attack animation complete, switching camera back");
            SwitchCamera(cameraPos2);
        });
        attackSequence.AppendInterval(cameraBlendDuration);
        
        attackSequence.AppendCallback(() =>
        {
            Debug.Log("[FightController] Attack complete, returning to positions");
            OnAttackComplete(isPlayerAttacking);
        });
    }
    
    void ApplyDamage(bool toPlayer, int damage, Animator targetAnimator, BoxCollider targetCollider)
    {
        if (toPlayer)
        {
            playerHealth -= damage;
            if (playerHealth <= 0)
            {
                playerHealth = 0;
                OnFighterDeath(targetAnimator, targetCollider);
            }
        }
        else
        {
            botHealth -= damage;
            if (botHealth <= 0)
            {
                botHealth = 0;
                OnFighterDeath(targetAnimator, targetCollider);
            }
        }
        
        Debug.Log($"[FightController] Damage dealt: {damage}. Player HP: {playerHealth}, Bot HP: {botHealth}");
    }
    
    void OnFighterDeath(Animator animator, BoxCollider collider)
    {
        Debug.Log("[FightController] Fighter died!");
        animator.CrossFade(deathAnimationState, 0.1f);
        
        if (collider != null)
        {
            Vector3 center = collider.center;
            center.y = deathColliderCenterY;
            collider.center = center;
        }
        
        isFightActive = false;
    }
    
    void OnAttackComplete(bool wasPlayerAttacking)
    {
        Debug.Log($"[FightController] OnAttackComplete, wasPlayerAttacking: {wasPlayerAttacking}, isFightActive: {isFightActive}");
        
        if (!isFightActive) return;
        
        ReturnToPositions(() =>
        {
            if (wasPlayerAttacking)
            {
                defenseMultiplier = 1f;
                DOVirtual.DelayedCall(delayBetweenTurns, () => ExecuteBotTurn());
            }
            else
            {
                isPlayerTurn = true;
                defenseMultiplier = 1f;
                ShowCardsPanel(() =>
                {
                    SetCardsInteractable(true);
                });
            }
        });
    }
    
    void ReturnToPositions(System.Action onComplete)
    {
        Debug.Log("[FightController] ReturnToPositions");
        Sequence returnSequence = DOTween.Sequence();
        
        returnSequence.Append(player.DOMove(playerOriginalPos, moveToHitDuration).SetEase(Ease.InOutQuad));
        returnSequence.Join(bot.DOMove(botOriginalPos, moveToHitDuration).SetEase(Ease.InOutQuad));
        
        returnSequence.AppendCallback(() =>
        {
            if (playerHealth > 0)
                playerAnimator.CrossFade(idleAnimationState, 0.1f);
            if (botHealth > 0)
                botAnimator.CrossFade(idleAnimationState, 0.1f);
        });
        
        returnSequence.AppendCallback(() => onComplete?.Invoke());
    }
    
    void SwitchCamera(CinemachineVirtualCamera targetCamera)
    {
        if (cameraPos2 != null) cameraPos2.Priority = 0;
        if (playerHitCamera != null) playerHitCamera.Priority = 0;
        if (botHitCamera != null) botHitCamera.Priority = 0;
        
        if (targetCamera != null) targetCamera.Priority = 10;
    }
    
    void SetCardsInteractable(bool interactable)
    {
        Debug.Log($"[FightController] SetCardsInteractable: {interactable}");
        foreach (var cardButton in cardButtons)
        {
            if (cardButton != null)
                cardButton.SetInteractable(interactable);
        }
    }

    void HideCardsPanel(System.Action onComplete = null)
    {
        if (cardsPanel == null)
        {
            onComplete?.Invoke();
            return;
        }

        Debug.Log("[FightController] HideCardsPanel");
        cardsPanel.transform.DOLocalMove(cardsPanelOriginalPos + Vector3.down * cardsPanelSlideDistance, cardsPanelSlideDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => onComplete?.Invoke());
    }

    void ShowCardsPanel(System.Action onComplete = null)
    {
        if (cardsPanel == null)
        {
            onComplete?.Invoke();
            return;
        }

        Debug.Log("[FightController] ShowCardsPanel");
        cardsPanel.transform.DOLocalMove(cardsPanelOriginalPos, cardsPanelSlideDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => onComplete?.Invoke());
    }
    
    float GetAnimationLength(Animator animator, string stateName)
    {
        if (animator == null) return 1f;
        
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        if (ac == null) return 1f;
        
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == stateName || stateName.Contains(clip.name) || clip.name.Contains(stateName))
            {
                return clip.length;
            }
        }
        
        return 1f;
    }
    
    public int GetPlayerHealth() => playerHealth;
    public int GetBotHealth() => botHealth;
    public bool IsFightActive() => isFightActive;

    public void SwitchToFightCamera()
    {
        SwitchCamera(cameraPos2);
    }
    
    void OnDestroy()
    {
        DOTween.Kill(this);
    }
}