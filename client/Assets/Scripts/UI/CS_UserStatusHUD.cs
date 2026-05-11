using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class UserStatusHUD : MonoBehaviour
{
    [Header("별가루 (코인)")]
    [SerializeField] private TMP_Text coinText;

    [Header("무료 티켓")]
    [SerializeField] private GameObject freeCouponIcon;
    [SerializeField] private TMP_Text   freeCouponText;

    private Coroutine _countdownCoroutine;

    private void OnEnable()  => Refresh();
    private void OnDisable() => StopCountdown();

    public void Refresh()
    {
        var mgr = UserDataManager.Instance;
        if (mgr == null) return;

        if (coinText != null)
            coinText.text = $"{mgr.Coins}";

        if (freeCouponIcon != null)
            freeCouponIcon.SetActive(mgr.HasFreeCoupon);

        StopCountdown();

        if (freeCouponText == null) return;

        if (mgr.HasFreeCoupon)
        {
            freeCouponText.text = "사용 가능";
        }
        else if (mgr.FreeCouponRefreshAt.HasValue)
        {
            _countdownCoroutine = StartCoroutine(CountdownRoutine(mgr.FreeCouponRefreshAt.Value));
        }
        else
        {
            freeCouponText.text = "—";
        }
    }

    private IEnumerator CountdownRoutine(DateTime refreshAt)
    {
        var wait = new WaitForSeconds(1f);
        while (true)
        {
            TimeSpan remaining = refreshAt - DateTime.UtcNow;
            if (remaining.TotalSeconds <= 0)
            {
                freeCouponText.text = "사용 가능";
                yield break;
            }

            freeCouponText.text = remaining.TotalHours >= 1
                ? $"{(int)remaining.TotalHours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}"
                : $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";

            yield return wait;
        }
    }

    private void StopCountdown()
    {
        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
        }
    }
}
