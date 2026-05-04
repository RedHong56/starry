using UnityEngine;
using TMPro;

/// <summary>
/// Main 씬 상단 HUD: 별가루(코인) 수량과 무료 티켓 여부를 표시.
/// Refresh()를 호출하면 UserDataManager 최신값으로 갱신.
/// </summary>
public class UserStatusHUD : MonoBehaviour
{
    [Header("별가루 (코인)")]
    [SerializeField] private TMP_Text coinText;

    [Header("무료 티켓")]
    [SerializeField] private GameObject freeCouponIcon;   // 보유 시 활성화할 오브젝트 (이미지/텍스트 등)
    [SerializeField] private TMP_Text   freeCouponText;   // "무료 티켓 보유" / "무료 티켓 없음" 표시용 (선택)

    private void OnEnable() => Refresh();

    public void Refresh()
    {
        var mgr = UserDataManager.Instance;
        if (mgr == null) return;

        if (coinText != null)
            coinText.text = $"별가루  {mgr.Coins}";

        if (freeCouponIcon != null)
            freeCouponIcon.SetActive(mgr.HasFreeCoupon);

        if (freeCouponText != null)
            freeCouponText.text = mgr.HasFreeCoupon ? "무료 티켓 보유" : "";
    }
}
