using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class FreeTicketDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text   statusText;
    [SerializeField] private GameObject availableIcon;

    private Coroutine _countdownCoroutine;

    private void OnEnable()  => Refresh();
    private void OnDisable() => StopCountdown();

    public void Refresh()
    {
        var mgr = UserDataManager.Instance;
        if (mgr == null) return;

        if (availableIcon != null)
            availableIcon.SetActive(mgr.HasFreeCoupon);

        StopCountdown();

        if (statusText == null) return;

        if (mgr.HasFreeCoupon)
            statusText.text = LocalizationManager.IsKorean ? "무료 사용 가능" : "Free";
        else if (mgr.FreeCouponRefreshAt.HasValue)
            _countdownCoroutine = StartCoroutine(CountdownRoutine(mgr.FreeCouponRefreshAt.Value));
        else
            statusText.text = "—";
    }

    private IEnumerator CountdownRoutine(DateTime refreshAt)
    {
        var wait = new WaitForSeconds(1f);
        while (true)
        {
            TimeSpan remaining = refreshAt - DateTime.UtcNow;
            if (remaining.TotalSeconds <= 0)
            {
                statusText.text = LocalizationManager.IsKorean ? "무료 사용 가능" : "Free";
                if (availableIcon != null) availableIcon.SetActive(true);
                yield break;
            }

            statusText.text = remaining.TotalHours >= 1
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
