using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;

public class MultiUIAnimator : MonoBehaviour
{
    public List<UIAnimationStep> animationSteps = new List<UIAnimationStep>();

    void Start()
    {
        foreach (var step in animationSteps)
        {
            step.CacheInitialValues();
            if (step.triggerMode == UIAnimationStep.TriggerMode.OnStart)
            {
                step.Play();
            }
            else if (step.triggerMode == UIAnimationStep.TriggerMode.OnLoop)
            {
                step.Play(loop: true);
            }
        }
    }

    public void PlayStepByIndex(int index)
    {
        if (index >= 0 && index < animationSteps.Count)
        {
            animationSteps[index].Play();
        }
    }

    public void ResetAll()
    {
        foreach (var step in animationSteps)
        {
            step.ResetToOriginal();
        }
    }
}

[System.Serializable]
public class UIAnimationStep
{
    public enum TriggerMode { OnStart, OnLoop, OnCall }

    [Header("Target References")]
    public RectTransform rectTarget;
    public Graphic graphicTarget;
    public CanvasGroup canvasGroupTarget;

    [Header("Trigger")]
    public TriggerMode triggerMode = TriggerMode.OnCall;
    public LoopType loopType = LoopType.Restart;
    public int loopCount = 0;
    public bool useYoyoLoop = false;

    [Header("Position")]
    public bool animatePosition = false;
    public Vector3 targetPosition;

    [Header("Scale")]
    public bool animateScale = false;
    public Vector3 targetScale = Vector3.one;

    [Header("Rotation")]
    public bool animateRotation = false;
    public Vector3 targetRotation;

    [Header("Color")]
    public bool animateColor = false;
    public Color targetColor = Color.white;

    [Header("Alpha")]
    public bool animateAlpha = false;
    [Range(0, 1)] public float targetAlpha = 1f;

    [Header("Timing")]
    public float duration = 1f;
    public float delay = 0f;
    public Ease easing = Ease.OutQuad;

    private Sequence sequence;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Vector3 originalRotation;
    private Color originalColor;
    private float originalAlpha;

    public void CacheInitialValues()
    {
        if (rectTarget != null)
        {
            originalPosition = rectTarget.anchoredPosition;
            originalScale = rectTarget.localScale;
            originalRotation = rectTarget.eulerAngles;
        }
        if (graphicTarget != null)
        {
            originalColor = graphicTarget.color;
        }
        if (canvasGroupTarget != null)
        {
            originalAlpha = canvasGroupTarget.alpha;
        }
    }

    public void Play() => Play(loop: false);

    public void Play(bool loop)
    {
        sequence?.Kill();
        sequence = DOTween.Sequence();

        if (rectTarget == null && (animatePosition || animateScale || animateRotation)) return;

        if (animatePosition)
        {
            sequence.Join(rectTarget.DOAnchorPos(targetPosition, duration).SetEase(easing).SetDelay(delay));
        }

        if (animateScale)
        {
            sequence.Join(rectTarget.DOScale(targetScale, duration).SetEase(easing).SetDelay(delay));
        }

        if (animateRotation)
        {
            sequence.Join(rectTarget.DORotate(targetRotation, duration, RotateMode.FastBeyond360).SetEase(easing).SetDelay(delay));
        }

        if (animateColor && graphicTarget != null)
        {
            sequence.Join(graphicTarget.DOColor(targetColor, duration).SetEase(easing).SetDelay(delay));
        }

        if (animateAlpha && canvasGroupTarget != null)
        {
            sequence.Join(canvasGroupTarget.DOFade(targetAlpha, duration).SetEase(easing).SetDelay(delay));
        }

        if (loop || triggerMode == TriggerMode.OnLoop)
        {
            sequence.SetLoops(loopCount <= 0 ? -1 : loopCount, useYoyoLoop ? LoopType.Yoyo : loopType);
        }

        sequence.Play();
    }

    public void ResetToOriginal()
    {
        if (rectTarget != null)
        {
            rectTarget.anchoredPosition = originalPosition;
            rectTarget.localScale = originalScale;
            rectTarget.eulerAngles = originalRotation;
        }
        if (graphicTarget != null)
        {
            graphicTarget.color = originalColor;
        }
        if (canvasGroupTarget != null)
        {
            canvasGroupTarget.alpha = originalAlpha;
        }
    }
}
