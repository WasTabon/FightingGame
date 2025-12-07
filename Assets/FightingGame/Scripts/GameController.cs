using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cinemachine;
using TMPro;
using System.Collections;

public class GameController : MonoBehaviour
{
    [SerializeField] private Button findMatchButton;
    [SerializeField] private CarAnimationController carAnimationController;
    [SerializeField] private Transform car;
    [SerializeField] private Transform carPoint1;
    [SerializeField] private Transform carPoint2;
    [SerializeField] private CinemachineVirtualCamera virtualCamera1;
    [SerializeField] private CinemachineVirtualCamera virtualCamera2;
    [SerializeField] private GameObject findMatchPanel;
    [SerializeField] private float carMoveDuration = 2f;
    [SerializeField] private float slideAnimationDuration = 0.5f;
    [SerializeField] private float slideDistance = 1000f;
    [SerializeField] private float searchDuration = 3f;
    [SerializeField] private GameObject[] _canvasObjects;

    private Transform panelChild;
    private TextMeshProUGUI searchText;
    private Vector3 panelChildOriginalPosition;
    private Coroutine dotsAnimationCoroutine;

    void Start()
    {
        if (findMatchButton != null)
        {
            findMatchButton.onClick.AddListener(OnFindMatchClicked);
        }

        if (findMatchPanel != null)
        {
            panelChild = findMatchPanel.transform.GetChild(0);
            if (panelChild != null)
            {
                panelChildOriginalPosition = panelChild.localPosition;
                searchText = panelChild.GetComponentInChildren<TextMeshProUGUI>();
                PreparePanelAnimation();
            }
            findMatchPanel.SetActive(false);
        }
    }

    void PreparePanelAnimation()
    {
        if (panelChild != null)
        {
            panelChild.localPosition = panelChildOriginalPosition + Vector3.right * slideDistance;
        }
    }

    void OnFindMatchClicked()
    {
        foreach (GameObject canvasObject in _canvasObjects)
        {
            canvasObject.SetActive(false);
        }
        
        if (findMatchPanel != null)
        {
            findMatchPanel.SetActive(true);
        }

        PreparePanelAnimation();

        if (panelChild != null)
        {
            panelChild.DOLocalMove(panelChildOriginalPosition, slideAnimationDuration).SetEase(Ease.OutQuad);
        }

        if (searchText != null)
        {
            dotsAnimationCoroutine = StartCoroutine(AnimateDotsCoroutine());
        }

        DOVirtual.DelayedCall(searchDuration, () =>
        {
            if (dotsAnimationCoroutine != null)
            {
                StopCoroutine(dotsAnimationCoroutine);
            }

            HidePanel();
        });
    }

    IEnumerator AnimateDotsCoroutine()
    {
        string[] dotStates = { ".", "..", "...", "" };
        int index = 0;

        while (true)
        {
            if (searchText != null)
            {
                searchText.text = "Searching match" + dotStates[index];
            }

            index = (index + 1) % dotStates.Length;
            yield return new WaitForSeconds(0.5f);
        }
    }

    void HidePanel()
    {
        if (panelChild != null)
        {
            panelChild.DOLocalMove(panelChildOriginalPosition + Vector3.right * slideDistance, slideAnimationDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    if (findMatchPanel != null)
                    {
                        findMatchPanel.SetActive(false);
                    }

                    StartCarMovement();
                });
        }
    }

    void StartCarMovement()
    {
        if (carAnimationController != null)
        {
            carAnimationController.StopIdleAnimation();
        }

        if (virtualCamera1 != null)
        {
            virtualCamera1.gameObject.SetActive(false);
        }

        if (virtualCamera2 != null)
        {
            virtualCamera2.gameObject.SetActive(true);
        }

        if (car != null && carPoint2 != null)
        {
            car.DOMove(carPoint2.position, carMoveDuration).SetEase(Ease.InOutQuad);
        }
    }

    void OnDestroy()
    {
        if (findMatchButton != null)
        {
            findMatchButton.onClick.RemoveListener(OnFindMatchClicked);
        }

        if (dotsAnimationCoroutine != null)
        {
            StopCoroutine(dotsAnimationCoroutine);
        }

        DOTween.Kill(this);
    }
}