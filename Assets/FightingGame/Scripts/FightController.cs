using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cinemachine;
using HighlightPlus;
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
    
    [Header("Health UI")]
    [SerializeField] private Image playerHealthImage;
    [SerializeField] private Image botHealthImage;
    [SerializeField] private float healthAnimationDuration = 0.5f;
    
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
    
    [Header("Hit FX Settings")]
    [SerializeField] private Color hitFXColor = Color.white;
    [SerializeField] private float hitFXDuration = 0.2f;
    [SerializeField] private float hitFXIntensity = 1f;
    
    [Header("Cards Panel Animation")]
    [SerializeField] private float cardsPanelSlideDuration = 0.5f;
    [SerializeField] private float cardsPanelSlideDistance = 1000f;
    
    [Header("Health Panel")]
    [SerializeField] private GameObject healthPanel;
    [SerializeField] private float healthPanelSlideDuration = 0.4f;
    [SerializeField] private float healthPanelSlideDistance = 300f;
    
    private int playerHealth;
    private int botHealth;
    private bool isPlayerTurn;
    private bool isFightActive;
    private float defenseMultiplier = 1f;
    private Vector3 playerOriginalPos;
    private Vector3 botOriginalPos;
    private Vector3 cardsPanelOriginalPos;
    private Vector3 healthPanelOriginalPos;
    
    private HighlightEffect playerHighlightEffect;
    private HighlightEffect botHighlightEffect;
    
    private int pendingDamage;
    private bool pendingDamageToPlayer;
    
    void Start()
    {
        Debug.Log($"[FightController] Start called");
        
        InitializeCards();
        InitializeCardButtons();
        InitializeHighlightEffects();
        
        if (player != null) playerOriginalPos = player.position;
        if (bot != null) botOriginalPos = bot.position;
        if (cardsPanel != null) cardsPanelOriginalPos = cardsPanel.transform.localPosition;
        
        if (healthPanel != null)
        {
            healthPanelOriginalPos = healthPanel.transform.localPosition;
            healthPanel.transform.localPosition = healthPanelOriginalPos + Vector3.up * healthPanelSlideDistance;
        }
    }
    
    void InitializeCards()
    {
        if (cards.Count == 0)
        {
            cards.Add(new CardData("Jab", "Jab 1", "Hit Body New", 0.019f, 15, false));
            cards.Add(new CardData("Uppercut", "Uppercut New", "Head Hit New", 0.014f, 25, false));
            cards.Add(new CardData("Special", "Mma Kick New", "Head Hit New", 0.029f, 35, false));
            cards.Add(new CardData("Defense", "Right Block New", "", 0f, 0, true));
        }
    }
    
    void InitializeCardButtons()
    {
        for (int i = 0; i < cardButtons.Count && i < cards.Count; i++)
        {
            if (cardButtons[i] != null)
            {
                cardButtons[i].Initialize(i, OnCardSelected);
            }
        }
    }
    
    void InitializeHighlightEffects()
    {
        if (player != null)
            playerHighlightEffect = player.GetComponent<HighlightEffect>();
        
        if (bot != null)
            botHighlightEffect = bot.GetComponent<HighlightEffect>();
        
        Debug.Log($"[FightController] playerHighlightEffect null: {playerHighlightEffect == null}");
        Debug.Log($"[FightController] botHighlightEffect null: {botHighlightEffect == null}");
    }
    
    public void StartFight()
    {
        Debug.Log("[FightController] StartFight called");
        ResetFight();
        isFightActive = true;
        isPlayerTurn = true;
        SetCardsInteractable(true);
        ShowHealthPanel();
    }
    
    void ResetFight()
    {
        playerHealth = maxHealth;
        botHealth = maxHealth;
        defenseMultiplier = 1f;
        pendingDamage = 0;
        
        UpdateHealthUI(playerHealthImage, 1f);
        UpdateHealthUI(botHealthImage, 1f);
        
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
        Debug.Log($"[FightController] OnCardSelected: index={cardIndex}");
        
        if (!isFightActive || !isPlayerTurn) return;
        if (cardIndex < 0 || cardIndex >= cards.Count) return;
        
        SetCardsInteractable(false);
        CardData selectedCard = cards[cardIndex];
        
        if (rouletteController == null)
        {
            Debug.LogError("[FightController] rouletteController is NULL!");
            return;
        }
        
        HideCardsPanel();
        HideHealthPanel();
        
        DOVirtual.DelayedCall(cardsPanelSlideDuration, () =>
        {
            int baseDamage = selectedCard.isDefense ? GetAverageAttackDamage() : selectedCard.baseDamage;
            
            rouletteController.Spin(selectedCard.isDefense, baseDamage, (multiplier, finalDamage) =>
            {
                Debug.Log($"[FightController] Spin callback, multiplier: {multiplier}, finalDamage: {finalDamage}");
                if (selectedCard.isDefense)
                {
                    defenseMultiplier = multiplier;
                    DOVirtual.DelayedCall(delayBetweenTurns, () => ExecuteBotTurn());
                }
                else
                {
                    ExecuteAttack(player, bot, playerAnimator, botAnimator, 
                        playerHitPos, playerHitCamera, selectedCard, multiplier, finalDamage, true);
                }
            });
        });
    }
    
    int GetAverageAttackDamage()
    {
        int total = 0;
        int count = 0;
        foreach (var card in cards)
        {
            if (!card.isDefense)
            {
                total += card.baseDamage;
                count++;
            }
        }
        return count > 0 ? total / count : 20;
    }
    
    void ExecuteBotTurn()
    {
        Debug.Log("[FightController] ExecuteBotTurn called");
        isPlayerTurn = false;
        
        List<CardData> attackCards = cards.FindAll(c => !c.isDefense);
        if (attackCards.Count == 0) return;
        
        CardData botCard = attackCards[Random.Range(0, attackCards.Count)];
        Debug.Log($"[FightController] Bot selected: {botCard.cardName}");
        
        rouletteController.Spin(false, botCard.baseDamage, (multiplier, finalDamage) =>
        {
            Debug.Log($"[FightController] Bot spin callback, multiplier: {multiplier}");
            ExecuteAttack(bot, player, botAnimator, playerAnimator,
                botHitPos, botHitCamera, botCard, multiplier, finalDamage, false);
        });
    }
    
    void ExecuteAttack(Transform attacker, Transform defender, 
        Animator attackerAnimator, Animator defenderAnimator,
        Transform hitPos, CinemachineVirtualCamera hitCamera,
        CardData card, float damageMultiplier, int calculatedDamage, bool isPlayerAttacking)
    {
        Debug.Log($"[FightController] ExecuteAttack: {card.cardName}, isPlayerAttacking: {isPlayerAttacking}");
        
        Sequence attackSequence = DOTween.Sequence();

        attackSequence.Append(attacker.DOMove(hitPos.position, moveToHitDuration).SetEase(Ease.OutQuad));

        attackSequence.AppendCallback(() => SwitchCamera(hitCamera));
        attackSequence.AppendInterval(cameraBlendDuration);
        
        attackSequence.AppendCallback(() =>
        {
            attackerAnimator.CrossFade(card.animationStateName, 0.1f);
        });
        
        attackSequence.AppendInterval(card.hitTimingSeconds);
        
        attackSequence.AppendCallback(() =>
        {
            Debug.Log("[FightController] Hit timing reached");
            
            int finalDamage = calculatedDamage;
            HighlightEffect defenderEffect = isPlayerAttacking ? botHighlightEffect : playerHighlightEffect;
            
            if (!isPlayerAttacking && defenseMultiplier < 1f)  // ← только если игрок ВЫБРАЛ защиту и получил бонус
            {
                finalDamage = Mathf.RoundToInt(calculatedDamage * (1f - defenseMultiplier));
                defenderAnimator.CrossFade("Right Block New", 0.05f);
            }
            else if (!string.IsNullOrEmpty(card.victimAnimationStateName))
            {
                defenderAnimator.CrossFade(card.victimAnimationStateName, 0.05f);
            }
            
            if (defenderEffect != null)
            {
                defenderEffect.HitFX(hitFXColor, hitFXDuration, hitFXIntensity);
            }
            
            pendingDamage = finalDamage;
            pendingDamageToPlayer = !isPlayerAttacking;
            
            Debug.Log($"[FightController] Pending damage: {pendingDamage}, toPlayer: {pendingDamageToPlayer}");
        });
        
        float animationLength = GetAnimationLength(attackerAnimator, card.animationStateName);
        attackSequence.AppendInterval(animationLength - card.hitTimingSeconds);
        
        attackSequence.AppendCallback(() =>
        {
            SwitchCamera(cameraPos2);
        });
        attackSequence.AppendInterval(cameraBlendDuration);
        
        attackSequence.AppendCallback(() =>
        {
            OnAttackComplete(isPlayerAttacking);
        });
    }
    
    void ApplyPendingDamage(System.Action onComplete)
    {
        if (pendingDamage <= 0)
        {
            onComplete?.Invoke();
            return;
        }
        
        Debug.Log($"[FightController] Applying pending damage: {pendingDamage}");
        
        Image healthImage;
        int currentHealth;
        int newHealth;
        
        if (pendingDamageToPlayer)
        {
            currentHealth = playerHealth;
            playerHealth = Mathf.Max(0, playerHealth - pendingDamage);
            newHealth = playerHealth;
            healthImage = playerHealthImage;
        }
        else
        {
            currentHealth = botHealth;
            botHealth = Mathf.Max(0, botHealth - pendingDamage);
            newHealth = botHealth;
            healthImage = botHealthImage;
        }
        
        Debug.Log($"[FightController] Health: {currentHealth} -> {newHealth}");
        
        float startFill = (float)currentHealth / maxHealth;
        float endFill = (float)newHealth / maxHealth;
        
        pendingDamage = 0;
        
        AnimateHealthBar(healthImage, startFill, endFill, () =>
        {
            if (newHealth <= 0)
            {
                Animator targetAnimator = pendingDamageToPlayer ? playerAnimator : botAnimator;
                BoxCollider targetCollider = pendingDamageToPlayer ? playerCollider : botCollider;
                OnFighterDeath(targetAnimator, targetCollider);
            }
            onComplete?.Invoke();
        });
    }
    
    void AnimateHealthBar(Image healthImage, float from, float to, System.Action onComplete)
    {
        if (healthImage == null)
        {
            onComplete?.Invoke();
            return;
        }
        
        DOTween.To(() => from, x => healthImage.fillAmount = x, to, healthAnimationDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => onComplete?.Invoke());
    }
    
    void UpdateHealthUI(Image healthImage, float fillAmount)
    {
        if (healthImage != null)
            healthImage.fillAmount = fillAmount;
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
        Debug.Log($"[FightController] OnAttackComplete, wasPlayerAttacking: {wasPlayerAttacking}");
        
        ReturnToPositions(() =>
        {
            if (wasPlayerAttacking)
            {
                ShowHealthPanel(() =>
                {
                    ApplyPendingDamage(() =>
                    {
                        if (!isFightActive) return;
                        
                        defenseMultiplier = 1f;
                        HideHealthPanel();
                        DOVirtual.DelayedCall(healthPanelSlideDuration + delayBetweenTurns, () => ExecuteBotTurn());
                    });
                });
            }
            else
            {
                ShowCardsPanel();
                ShowHealthPanel(() =>
                {
                    ApplyPendingDamage(() =>
                    {
                        if (!isFightActive) return;
                        
                        isPlayerTurn = true;
                        defenseMultiplier = 1f;
                        DOVirtual.DelayedCall(healthAnimationDuration, () =>
                        {
                            SetCardsInteractable(true);
                        });
                    });
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

        cardsPanel.transform.DOLocalMove(cardsPanelOriginalPos, cardsPanelSlideDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => onComplete?.Invoke());
    }
    
    void HideHealthPanel(System.Action onComplete = null)
    {
        if (healthPanel == null)
        {
            onComplete?.Invoke();
            return;
        }

        healthPanel.transform.DOLocalMove(healthPanelOriginalPos + Vector3.up * healthPanelSlideDistance, healthPanelSlideDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => onComplete?.Invoke());
    }

    void ShowHealthPanel(System.Action onComplete = null)
    {
        if (healthPanel == null)
        {
            onComplete?.Invoke();
            return;
        }

        healthPanel.transform.DOLocalMove(healthPanelOriginalPos, healthPanelSlideDuration)
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