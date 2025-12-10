using UnityEngine;
using DG.Tweening;

public class CarAnimationController : MonoBehaviour
{
    [SerializeField] private Transform animatedChild;
    [SerializeField] private float animationDuration = 0.5f;
    
    private Vector3 originalRotation;
    private Vector3 originalScale;
    private Sequence idleSequence;
    private bool isInitialized = false;

    void Start()
    {
        Initialize();
        StartIdleAnimation();
    }

    void Initialize()
    {
        if (isInitialized) return;

        if (animatedChild != null)
        {
            originalRotation = animatedChild.localEulerAngles;
            originalScale = animatedChild.localScale;
            isInitialized = true;
        }
    }

    public void StartIdleAnimation()
    {
        Initialize();

        if (animatedChild == null) return;

        StopIdleAnimation();

        animatedChild.localEulerAngles = originalRotation;
        animatedChild.localScale = originalScale;

        idleSequence = DOTween.Sequence();
        
        idleSequence.Append(animatedChild.DOLocalRotate(new Vector3(originalRotation.x, originalRotation.y, -10), animationDuration));
        idleSequence.Join(animatedChild.DOScale(new Vector3(originalScale.x, originalScale.y * 1.2f, originalScale.z), animationDuration));
        
        idleSequence.Append(animatedChild.DOLocalRotate(originalRotation, animationDuration));
        idleSequence.Join(animatedChild.DOScale(originalScale, animationDuration));
        
        idleSequence.Append(animatedChild.DOLocalRotate(new Vector3(originalRotation.x, originalRotation.y, 10), animationDuration));
        idleSequence.Join(animatedChild.DOScale(new Vector3(originalScale.x, originalScale.y * 1.2f, originalScale.z), animationDuration));
        
        idleSequence.Append(animatedChild.DOLocalRotate(originalRotation, animationDuration));
        idleSequence.Join(animatedChild.DOScale(originalScale, animationDuration));
        
        idleSequence.SetLoops(-1);
    }

    public void StopIdleAnimation()
    {
        if (idleSequence != null)
        {
            idleSequence.Kill();
            idleSequence = null;
        
            if (animatedChild != null)
            {
                animatedChild.localEulerAngles = originalRotation;
                animatedChild.localScale = originalScale;
            }
        }
    }

    void OnEnable()
    {
        if (isInitialized)
        {
            StartIdleAnimation();
        }
    }

    void OnDisable()
    {
        StopIdleAnimation();
    }
    
    void OnDestroy()
    {
        if (idleSequence != null)
        {
            idleSequence.Kill();
        }
    }
}
