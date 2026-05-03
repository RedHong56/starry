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
        SoundManager.Instance?.PlayCard(CardSFX.Flip);

        if (cardImage != null)
        {
            cardImage.sprite  = backSprite;
            cardImage.enabled = true;
        }

        bool done = false;

        transform.DOScaleX(0f, halfFlipDuration)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                if (cardImage == null) { done = true; return; }

                // Debug.Log($"[FlipRoutine] 콜백 | frontSprite={(frontSprite != null ? frontSprite.name : "null")}");
                cardImage.sprite = frontSprite;

                transform.DOScaleX(1f, halfFlipDuration)
                    .SetEase(Ease.OutSine)
                    .OnComplete(() => done = true);
            });

        yield return new WaitUntil(() => done);

        // if (cardImage != null)
        //     Debug.Log($"[FlipRoutine] 완료 | sprite={(cardImage.sprite != null ? cardImage.sprite.name : "null")} | enabled={cardImage.enabled} | scale={transform.localScale}");
    }
}
