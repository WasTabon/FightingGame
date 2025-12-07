using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cinemachine;

public class GameController : MonoBehaviour
{
    [SerializeField] private Button findMatchButton;
    [SerializeField] private CarAnimationController carAnimationController;
    [SerializeField] private Transform car;
    [SerializeField] private Transform carPoint1;
    [SerializeField] private Transform carPoint2;
    [SerializeField] private CinemachineVirtualCamera virtualCamera1;
    [SerializeField] private CinemachineVirtualCamera virtualCamera2;
    [SerializeField] private float carMoveDuration = 2f;

    void Start()
    {
        if (findMatchButton != null)
        {
            findMatchButton.onClick.AddListener(OnFindMatchClicked);
        }
    }

    void OnFindMatchClicked()
    {
        findMatchButton.interactable = false;
        
        DOVirtual.DelayedCall(3f, () =>
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
        });
    }

    void OnDestroy()
    {
        if (findMatchButton != null)
        {
            findMatchButton.onClick.RemoveListener(OnFindMatchClicked);
        }
        
        DOTween.Kill(this);
    }
}