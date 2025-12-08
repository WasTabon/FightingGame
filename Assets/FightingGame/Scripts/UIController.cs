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

    private Vector3 cardsPanelOriginalPosition;
    private Transform[] cardChildren;

    void Start()
    {
        if (cardsPanel != null)
        {
            cardsPanelOriginalPosition = cardsPanel.transform.localPosition;
            
            int childCount = cardsPanel.transform.childCount;
            cardChildren = new Transform[childCount];
            
            for (int i = 0; i < childCount; i++)
            {
                cardChildren[i] = cardsPanel.transform.GetChild(i);
            }
            
            PrepareCardsPanel();
            cardsPanel.SetActive(false);
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
        if (cardsPanel == null) return;

        PrepareCardsPanel();
        cardsPanel.SetActive(true);

        cardsPanel.transform.DOLocalMove(cardsPanelOriginalPosition, slideAnimationDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                StartCoroutine(ShowCardsSequentially());
            });
    }

    IEnumerator ShowCardsSequentially()
    {
        foreach (Transform child in cardChildren)
        {
            if (child != null)
            {
                child.gameObject.SetActive(true);
                child.localScale = Vector3.zero;
                child.DOScale(Vector3.one, cardAnimationDuration).SetEase(Ease.OutBack);
                
                yield return new WaitForSeconds(cardShowDelay);
            }
        }
    }

    void OnDestroy()
    {
        DOTween.Kill(this);
    }
}