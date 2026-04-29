using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 카드 슬롯 하나의 UI 요소를 묶는 컴포넌트.
/// 각 슬롯 오브젝트(과거/현재/미래)에 부착.
/// </summary>
public class CardSlotView : MonoBehaviour
{
    [SerializeField] private CardFlipView flipView;
    [SerializeField] private TMP_Text     arcanaLabel;    // "Major" 또는 "Minor"
    [SerializeField] private TMP_Text     cardNameLabel;  // "0 The Fool" 또는 "Wand II"
    [SerializeField] private Image        cardImage;      // Resources/cards/{id} 이미지

    public CardFlipView FlipView => flipView;

    public void Clear()
    {
        if (arcanaLabel   != null) arcanaLabel.text   = string.Empty;
        if (cardNameLabel != null) cardNameLabel.text  = string.Empty;
        if (cardImage     != null) cardImage.enabled   = false;
    }

    /// <summary>카드 정보를 표시한다.</summary>
    public void ShowCardInfo(CardInfo info, int cardId)
    {
        if (info == null)
        {
            if (arcanaLabel   != null) arcanaLabel.text  = "Unknown";
            if (cardNameLabel != null) cardNameLabel.text = $"Card {cardId}";
            if (cardImage     != null) cardImage.enabled  = false;
            return;
        }

        // Arcana 표시
        if (arcanaLabel != null)
            arcanaLabel.text = CardInfoDatabase.FormatArcana(info);

        // 카드 이름 표시
        if (cardNameLabel != null)
            cardNameLabel.text = CardInfoDatabase.FormatCardName(info);

        // 카드 이미지 로드: Resources/Cards/{id}_{name_snake_case}
        if (cardImage != null)
        {
            string path = CardInfoDatabase.GetResourcePath(info);
            Sprite sprite = path != null ? Resources.Load<Sprite>(path) : null;
            if (sprite != null)
            {
                cardImage.sprite  = sprite;
                cardImage.enabled = true;
            }
            else
            {
                Debug.LogWarning($"[CardSlotView] Sprite not found: {path}");
                cardImage.enabled = false;
            }
        }
    }
}
