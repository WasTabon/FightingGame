using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class FightResultController : MonoBehaviour
{
    public static FightResultController Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI rewardsText;
    [SerializeField] private GameObject rewardsContainer;
    [SerializeField] private Image iconImage;
    
    [Header("Button")]
    [SerializeField] private Button continueButton;
    
    [Header("Icons")]
    [SerializeField] private Sprite victoryIcon;
    [SerializeField] private Sprite defeatIcon;
    
    [Header("Colors")]
    [SerializeField] private Color victoryColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color defeatColor = new Color(0.8f, 0.2f, 0.2f);
    
    [Header("Animation Settings")]
    [SerializeField] private float panelScaleDuration = 0.4f;
    [SerializeField] private float contentFadeDuration = 0.3f;
    [SerializeField] private float contentDelay = 0.2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip defeatSound;
    
    [Header("References")]
    [SerializeField] private FightController fightController;
    [SerializeField] private GameController gameController;
    [SerializeField] private UIController uiController;

    public event Action OnContinueClicked;

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
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }

        if (fightController != null)
        {
            fightController.OnFightEnded += OnFightEnded;
        }
    }

    void OnFightEnded(bool playerWon)
    {
        ShowResult(playerWon);
    }

    public void ShowResult(bool playerWon)
    {
        if (resultPanel == null) return;

        if (playerWon && victorySound != null && MusicController.Instance != null)
        {
            MusicController.Instance.PlaySpecificSound(victorySound);
        }
        else if (!playerWon && defeatSound != null && MusicController.Instance != null)
        {
            MusicController.Instance.PlaySpecificSound(defeatSound);
        }

        if (titleText != null)
        {
            titleText.text = playerWon ? "VICTORY!" : "DEFEAT";
            titleText.color = playerWon ? victoryColor : defeatColor;
        }

        if (rewardsContainer != null)
        {
            rewardsContainer.SetActive(playerWon);
        }

        if (rewardsText != null && playerWon)
        {
            int rankReward = WalletController.Instance != null ? 30 : 0;
            int expReward = WalletController.Instance != null ? 150 : 0;
            rewardsText.text = $"+{rankReward} Rank\n+{expReward} EXP";
        }

        if (iconImage != null)
        {
            iconImage.sprite = playerWon ? victoryIcon : defeatIcon;
            iconImage.color = playerWon ? victoryColor : defeatColor;
        }

        resultPanel.transform.localScale = Vector3.zero;
        
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
        }

        resultPanel.SetActive(true);

        Sequence showSequence = DOTween.Sequence();

        showSequence.Append(resultPanel.transform.DOScale(Vector3.one, panelScaleDuration).SetEase(Ease.OutBack));

        if (panelCanvasGroup != null)
        {
            showSequence.Join(panelCanvasGroup.DOFade(1f, panelScaleDuration * 0.5f));
        }

        if (titleText != null)
        {
            titleText.transform.localScale = Vector3.zero;
            showSequence.Append(titleText.transform.DOScale(Vector3.one, contentFadeDuration).SetEase(Ease.OutBack));
            showSequence.AppendCallback(() =>
            {
                titleText.transform.DOShakeRotation(0.5f, new Vector3(0, 0, 10), 10, 90, true);
            });
        }

        if (iconImage != null)
        {
            iconImage.transform.localScale = Vector3.zero;
            showSequence.Append(iconImage.transform.DOScale(Vector3.one, contentFadeDuration).SetEase(Ease.OutBack));
        }

        if (playerWon && rewardsText != null)
        {
            rewardsText.transform.localScale = Vector3.zero;
            showSequence.Append(rewardsText.transform.DOScale(Vector3.one, contentFadeDuration).SetEase(Ease.OutBack));
        }

        if (continueButton != null)
        {
            continueButton.transform.localScale = Vector3.zero;
            showSequence.Append(continueButton.transform.DOScale(Vector3.one, contentFadeDuration).SetEase(Ease.OutBack));
        }
    }

    void OnContinueButtonClicked()
    {
        HideResult(() =>
        {
            OnContinueClicked?.Invoke();
            
            if (gameController != null)
            {
                gameController.ReturnToMainMenu();
            }
        });
    }

    void HideResult(Action onComplete)
    {
        if (resultPanel == null)
        {
            onComplete?.Invoke();
            return;
        }

        Sequence hideSequence = DOTween.Sequence();

        hideSequence.Append(resultPanel.transform.DOScale(Vector3.zero, panelScaleDuration * 0.7f).SetEase(Ease.InBack));

        if (panelCanvasGroup != null)
        {
            hideSequence.Join(panelCanvasGroup.DOFade(0f, panelScaleDuration * 0.5f));
        }

        hideSequence.OnComplete(() =>
        {
            resultPanel.SetActive(false);
            onComplete?.Invoke();
        });
    }

    void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonClicked);
        }

        if (fightController != null)
        {
            fightController.OnFightEnded -= OnFightEnded;
        }

        DOTween.Kill(resultPanel?.transform);
        DOTween.Kill(titleText?.transform);
        DOTween.Kill(iconImage?.transform);
        DOTween.Kill(rewardsText?.transform);
        DOTween.Kill(continueButton?.transform);
    }
}
