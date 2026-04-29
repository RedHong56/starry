using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// 카드 뒤집기 애니메이션 (Y축 스케일 반전 방식)
public class CardFlipView : MonoBehaviour
{
    [SerializeField] private Image  cardImage;
    [SerializeField] private Sprite backSprite;
    [SerializeField] private float  halfFlipDuration = 0.3f;

    private void Awake()
    {
        if (backSprite != null)
            cardImage.sprite = backSprite;
    }

    public IEnumerator FlipRoutine(Sprite frontSprite)
    {
        bool done = false;

        // 1단계: 절반 축소 (뒷면 → 수직선)
        transform.DOScaleX(0f, halfFlipDuration)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                // 스프라이트 교체
                cardImage.sprite = frontSprite;

                // 2단계: 절반 확장 (수직선 → 앞면)
                transform.DOScaleX(1f, halfFlipDuration)
                    .SetEase(Ease.OutSine)
                    .OnComplete(() => done = true);
            });

        yield return new WaitUntil(() => done);
    }
}
