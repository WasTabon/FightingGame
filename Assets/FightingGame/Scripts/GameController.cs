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
    [SerializeField] private GameObject _player;
    [SerializeField] private UIController uiController;
    [SerializeField] private FightController fightController;
    [SerializeField] private InitialCameraSwitcher initialCameraSwitcher;

    private Transform panelChild;
    private TextMeshProUGUI searchText;
    private Vector3 panelChildOriginalPosition;
    private Coroutine dotsAnimationCoroutine;
    private Vector3 carOriginalPosition;

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

        if (car != null)
        {
            carOriginalPosition = car.position;
        }

        _player.SetActive(false);
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

        SwitchToCamera(virtualCamera2);

        if (car != null && carPoint2 != null)
        {
            car.DOMove(carPoint2.position, carMoveDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    _player.SetActive(true);
                    
                    if (fightController != null)
                    {
                        fightController.SwitchToFightCamera();
                    }
                    
                    if (uiController != null)
                    {
                        uiController.ShowCardsPanel();
                    }
                    
                    car.DOMove(carPoint1.position, carMoveDuration);
                });
        }
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("[GameController] ReturnToMainMenu called");

        if (fightController != null)
        {
            fightController.HideAllPanels(() =>
            {
                StartReturnSequence();
            });
        }
        else
        {
            StartReturnSequence();
        }
    }

    void StartReturnSequence()
    {
        if (car != null && carPoint2 != null)
        {
            car.DOMove(carPoint2.position, carMoveDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    _player.SetActive(false);

                    if (fightController != null)
                    {
                        fightController.ResetToInitialState();
                    }

                    if (initialCameraSwitcher != null)
                    {
                        initialCameraSwitcher.SwitchToInitialCamera();
                    }
                    else
                    {
                        SwitchToCamera(virtualCamera1);
                    }

                    if (car != null && carPoint1 != null)
                    {
                        car.DOMove(carPoint1.position, carMoveDuration)
                            .SetEase(Ease.InOutQuad)
                            .OnComplete(() =>
                            {
                                OnReturnComplete();
                            });
                    }
                    else
                    {
                        OnReturnComplete();
                    }
                });
        }
        else
        {
            OnReturnComplete();
        }
    }

    void OnReturnComplete()
    {
        Debug.Log("[GameController] OnReturnComplete");

        if (uiController != null)
        {
            uiController.ResetToInitialState();
        }

        foreach (GameObject canvasObject in _canvasObjects)
        {
            if (canvasObject != null)
            {
                canvasObject.SetActive(true);
            }
        }

        if (carAnimationController != null)
        {
            carAnimationController.StartIdleAnimation();
        }
    }

    void SwitchToCamera(CinemachineVirtualCamera targetCamera)
    {
        if (virtualCamera1 != null) virtualCamera1.Priority = 0;
        if (virtualCamera2 != null) virtualCamera2.Priority = 0;
        
        if (targetCamera != null) targetCamera.Priority = 10;
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
