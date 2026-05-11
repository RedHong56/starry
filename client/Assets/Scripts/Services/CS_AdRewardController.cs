using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdRewardController : MonoBehaviour,
    IUnityAdsInitializationListener,
    IUnityAdsLoadListener,
    IUnityAdsShowListener
{
    public static AdRewardController Instance { get; private set; }

    [SerializeField] private string androidGameId = "6111463";
    [SerializeField] private string iosGameId     = "6111462";
    [SerializeField] private string androidAdUnit = "Rewarded_Android";
    [SerializeField] private string iosAdUnit     = "Rewarded_iOS";
    [SerializeField] private bool   testMode      = false;

    private string _adUnitId;
    private bool   _adLoaded;
    private Action _onSuccess;
    private Action _onFail;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _adUnitId = Application.platform == RuntimePlatform.IPhonePlayer ? iosAdUnit : androidAdUnit;
        string gameId = Application.platform == RuntimePlatform.IPhonePlayer ? iosGameId : androidGameId;
        Advertisement.Initialize(gameId, testMode, this);
    }

    // ── IUnityAdsInitializationListener ──────────────────────────────────────

    public void OnInitializationComplete()
    {
        Advertisement.Load(_adUnitId, this);
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogWarning($"[AdRewardController] Init failed: {error} - {message}");
    }

    // ── IUnityAdsLoadListener ─────────────────────────────────────────────────

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        _adLoaded = true;
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"[AdRewardController] Load failed: {error} - {message}");
        _adLoaded = false;
    }

    // ── 광고 표시 ─────────────────────────────────────────────────────────────

    public void ShowRewardedAd(Action onSuccess, Action onFail)
    {
        if (!_adLoaded)
        {
            Debug.LogWarning("[AdRewardController] 광고가 아직 로드되지 않았습니다.");
            onFail?.Invoke();
            return;
        }

        _onSuccess = onSuccess;
        _onFail    = onFail;
        Advertisement.Show(_adUnitId, this);
    }

    // ── IUnityAdsShowListener ─────────────────────────────────────────────────

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState completionState)
    {
        Debug.Log($"[AdRewardController] ShowComplete: {completionState}");
        _adLoaded = false;
        Advertisement.Load(_adUnitId, this);

        if (completionState == UnityAdsShowCompletionState.COMPLETED)
            StartCoroutine(RewardRoutine(_onSuccess, _onFail));
        else
            _onFail?.Invoke();
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning($"[AdRewardController] Show failed: {error} - {message}");
        _onFail?.Invoke();
    }

    public void OnUnityAdsShowStart(string adUnitId)  => Debug.Log("[AdRewardController] ShowStart");
    public void OnUnityAdsShowClick(string adUnitId)  { }

    // ── 백엔드 보상 처리 ──────────────────────────────────────────────────────

    private IEnumerator RewardRoutine(Action onSuccess, Action onFail)
    {
        yield return UserDataManager.Instance.AdRewardRoutine(success =>
        {
            if (success) onSuccess?.Invoke();
            else         onFail?.Invoke();
        });
    }
}
