using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CarAnimationController : MonoBehaviour
{
    [SerializeField] private Transform animatedChild;
    [SerializeField] private float animationDuration = 0.5f;
    
    private Vector3 originalRotation;
    private Vector3 originalScale;
    private Sequence idleSequence;

    void Start()
    {
        if (animatedChild != null)
        {
            originalRotation = animatedChild.localEulerAngles;
            originalScale = animatedChild.localScale;
            StartIdleAnimation();
        }
    }

    void StartIdleAnimation()
    {
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

    void OnDestroy()
    {
        if (idleSequence != null)
        {
            idleSequence.Kill();
        }
    }
}