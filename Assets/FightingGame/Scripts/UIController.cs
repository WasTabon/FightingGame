using UnityEngine;
using DG.Tweening;
using System.Collections;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject cardsPanel;
    [SerializeField] private float slideAnimationDuration = 0.5f;
    [SerializeField] private float slideDistance = 1000f;
    [SerializeField] private float cardShowDelay = 0.2f;
    [SerializeField] private float cardAnimationDuration = 0.3f;
    [SerializeField] private FightController fightController;

    private Vector3 cardsPanelOriginalPosition;
    private Transform[] cardChildren;
    private Coroutine showCardsCoroutine;

    void Start()
    {
        Debug.Log($"[UIController] Start called");
        Debug.Log($"[UIController] fightController null: {fightController == null}");
        
        if (cardsPanel != null)
        {
            cardsPanelOriginalPosition = cardsPanel.transform.localPosition;
            
            int childCount = cardsPanel.transform.childCount;
            cardChildren = new Transform[childCount];
            
            for (int i = 0; i < childCount; i++)
            {
                cardChildren[i] = cardsPanel.transform.GetChild(i);
            }
            
            Debug.Log($"[UIController] Found {childCount} card children");
            PrepareCardsPanel();
            cardsPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[UIController] cardsPanel is NULL!");
        }
    }

    void PrepareCardsPanel()
    {
        if (cardsPanel != null)
        {
            cardsPanel.transform.localPosition = cardsPanelOriginalPosition + Vector3.down * slideDistance;
            
            foreach (Transform child in cardChildren)
            {
                if (child != null)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ShowCardsPanel()
    {
        Debug.Log("[UIController] ShowCardsPanel called");
        
        if (cardsPanel == null)
        {
            Debug.LogError("[UIController] cardsPanel is NULL in ShowCardsPanel!");
            return;
        }

        PrepareCardsPanel();
        cardsPanel.SetActive(true);

        cardsPanel.transform.DOLocalMove(cardsPanelOriginalPosition, slideAnimationDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                Debug.Log("[UIController] Cards panel slide complete, showing cards");
                showCardsCoroutine = StartCoroutine(ShowCardsSequentially());
            });
    }

    IEnumerator ShowCardsSequentially()
    {
        Debug.Log($"[UIController] ShowCardsSequentially, {cardChildren.Length} children");
        
        foreach (Transform child in cardChildren)
        {
            if (child != null)
            {
                child.gameObject.SetActive(true);
                child.localScale = Vector3.zero;
                child.DOScale(Vector3.one, cardAnimationDuration).SetEase(Ease.OutBack);
                Debug.Log($"[UIController] Showed card: {child.name}");
                
                yield return new WaitForSeconds(cardShowDelay);
            }
        }
        
        Debug.Log("[UIController] All cards shown, calling StartFight");
        
        if (fightController != null)
        {
            fightController.StartFight();
        }
        else
        {
            Debug.LogError("[UIController] fightController is NULL! Cannot start fight!");
        }

        showCardsCoroutine = null;
    }

    public void HideCardsPanel()
    {
        if (cardsPanel == null) return;

        if (showCardsCoroutine != null)
        {
            StopCoroutine(showCardsCoroutine);
            showCardsCoroutine = null;
        }

        cardsPanel.transform.DOLocalMove(cardsPanelOriginalPosition + Vector3.down * slideDistance, slideAnimationDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                cardsPanel.SetActive(false);
            });
    }

    public void ResetToInitialState()
    {
        Debug.Log("[UIController] ResetToInitialState called");

        if (showCardsCoroutine != null)
        {
            StopCoroutine(showCardsCoroutine);
            showCardsCoroutine = null;
        }

        DOTween.Kill(cardsPanel?.transform);

        if (cardsPanel != null)
        {
            cardsPanel.transform.localPosition = cardsPanelOriginalPosition + Vector3.down * slideDistance;
            cardsPanel.SetActive(false);

            foreach (Transform child in cardChildren)
            {
                if (child != null)
                {
                    child.gameObject.SetActive(false);
                    child.localScale = Vector3.one;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (showCardsCoroutine != null)
        {
            StopCoroutine(showCardsCoroutine);
        }

        DOTween.Kill(this);
    }
}
